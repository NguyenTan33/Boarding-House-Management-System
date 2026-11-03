import React, { useState, useEffect } from 'react';
import { Navbar } from './components/Navbar';
import { HeroSection } from './components/HeroSection';
import { RoomShowcase } from './components/RoomShowcase';
import { AmenitiesSection } from './components/AmenitiesSection';
import { LocationSection } from './components/LocationSection';
import { BookingSection } from './components/BookingSection';
import { Footer } from './components/Footer';
import { FloatingActions } from './components/FloatingActions';
import { AuthModals } from './components/AuthModals';
import { Toast, type ToastData } from './components/Toast';
import { roomService, SHOWCASE_ROOMS } from './services/roomService';
import { tokenStorage } from './services/apiClient';
import { authService } from './services/authService';
import type { BookingAppointment, Room, UserSummary } from './types';

export const App: React.FC = () => {
  const [currentUser, setCurrentUser] = useState<UserSummary | null>(() => tokenStorage.getUser());
  const [rooms, setRooms] = useState<Room[]>(SHOWCASE_ROOMS);
  const [selectedRoom, setSelectedRoom] = useState<string>('Tất cả các phòng còn trống');

  // Modals
  const [loginOpen, setLoginOpen] = useState(false);
  const [registerOpen, setRegisterOpen] = useState(false);
  const [invoicesOpen, setInvoicesOpen] = useState(false);

  // Toast
  const [toast, setToast] = useState<ToastData | null>(null);

  const showToast = (title: string, message: string, isSuccess = true) => {
    setToast({ title, message, isSuccess });
    setTimeout(() => {
      setToast((curr) => (curr?.title === title ? null : curr));
    }, 4500);
  };

  // Fetch rooms on mount
  useEffect(() => {
    roomService.getRooms().then((res) => {
      if (res.items && res.items.length > 0) {
        setRooms(res.items);
      }
    });
  }, []);

  // API rate limit and unauthorized listeners
  useEffect(() => {
    const handleRateLimit = (e: Event) => {
      const customEvent = e as CustomEvent<{ retryAfterSeconds: number }>;
      showToast(
        'Thao Tác Quá Nhanh',
        `Bạn đã gửi yêu cầu quá thường xuyên. Vui lòng chờ ${customEvent.detail.retryAfterSeconds} giây.`,
        false
      );
    };

    const handleUnauthorized = () => {
      setCurrentUser(null);
      showToast('Phiên Đăng Nhập Hết Hạn', 'Vui lòng đăng nhập lại để tiếp tục.', false);
    };

    window.addEventListener('api:rate-limited', handleRateLimit);
    window.addEventListener('api:unauthorized', handleUnauthorized);

    return () => {
      window.removeEventListener('api:rate-limited', handleRateLimit);
      window.removeEventListener('api:unauthorized', handleUnauthorized);
    };
  }, []);

  const handleLogout = async () => {
    await authService.logout();
    setCurrentUser(null);
    showToast('Đã Đăng Xuất', 'Hẹn gặp lại bạn lần sau!');
  };

  const handleSelectRoomForBooking = (roomName: string) => {
    setSelectedRoom(roomName);
    document.getElementById('dat-lich')?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleBookingSuccess = (booking: BookingAppointment) => {
    showToast(
      'Đặt Lịch Hẹn Thành Công!',
      `Cảm ơn ${booking.fullName} (${booking.phoneNumber}). Quản lý trọ sẽ gọi xác nhận khung giờ ${booking.preferredTime}!`
    );
  };

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col selection:bg-emerald-500 selection:text-white">
      {/* Toast Notification */}
      <Toast toast={toast} onClose={() => setToast(null)} />

      {/* Top Navbar */}
      <Navbar
        currentUser={currentUser}
        onOpenLogin={() => setLoginOpen(true)}
        onOpenRegister={() => setRegisterOpen(true)}
        onOpenInvoices={() => setInvoicesOpen(true)}
        onLogout={handleLogout}
      />

      <main className="flex-1">
        {/* Hero Section */}
        <HeroSection
          onExploreRooms={() => {
            document.getElementById('khong-gian-phong')?.scrollIntoView({ behavior: 'smooth' });
          }}
          onBookAppointment={() => {
            document.getElementById('dat-lich')?.scrollIntoView({ behavior: 'smooth' });
          }}
        />

        {/* Room Showcase (NO PRICE) */}
        <RoomShowcase
          rooms={rooms}
          onSelectRoomForBooking={handleSelectRoomForBooking}
        />

        {/* 6 Pillars Amenities & Security */}
        <AmenitiesSection />

        {/* Location & Neighborhood Hub (Interactive Map & Landmarks) */}
        <LocationSection />

        {/* Fast Booking Form */}
        <BookingSection
          selectedRoom={selectedRoom}
          onSuccess={handleBookingSuccess}
        />
      </main>

      {/* Footer */}
      <Footer onOpenLogin={() => setLoginOpen(true)} />

      {/* Floating Action Buttons: Zalo, Phone, Calendar */}
      <FloatingActions
        onQuickBook={() => {
          document.getElementById('dat-lich')?.scrollIntoView({ behavior: 'smooth' });
        }}
      />

      {/* Login & Register & Invoice Modals */}
      <AuthModals
        loginOpen={loginOpen}
        registerOpen={registerOpen}
        invoicesOpen={invoicesOpen}
        onCloseLogin={() => setLoginOpen(false)}
        onCloseRegister={() => setRegisterOpen(false)}
        onCloseInvoices={() => setInvoicesOpen(false)}
        onLoginSuccess={(user) => {
          setCurrentUser(user);
        }}
        onShowToast={showToast}
      />
    </div>
  );
};

export default App;
