import React, { useState, useEffect } from 'react';
import { ArrowUp, CalendarDays, Phone } from 'lucide-react';

interface FloatingActionsProps {
  onQuickBook: () => void;
}

export const FloatingActions: React.FC<FloatingActionsProps> = ({ onQuickBook }) => {
  const [showScrollTop, setShowScrollTop] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setShowScrollTop(window.scrollY > 350);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <aside aria-label="Liên hệ nhanh" className="fixed bottom-6 right-6 z-50 flex flex-col items-end gap-3.5 pointer-events-auto">
      
      {/* Scroll to Top */}
      <button
        onClick={scrollToTop}
        className={`w-11 h-11 rounded-full bg-white/95 text-slate-700 shadow-md border border-slate-200 flex items-center justify-center hover:bg-slate-100 hover:scale-110 transition-all duration-300 ${
          showScrollTop ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4 pointer-events-none'
        }`}
        title="Cuộn lên đầu trang"
      >
        <ArrowUp className="w-4 h-4" />
      </button>

      {/* Button 1: Quick Appointment Booking */}
      <button
        onClick={onQuickBook}
        title="Đặt lịch hẹn xem trọ trực tiếp"
        className="group relative w-14 h-14 rounded-full bg-gradient-to-tr from-amber-500 to-orange-500 text-white shadow-xl shadow-orange-500/30 flex items-center justify-center hover:scale-110 transition-transform duration-200 cursor-pointer"
      >
        <CalendarDays className="w-6 h-6" />
        <span className="absolute right-16 px-3 py-1.5 rounded-xl bg-slate-900 text-white text-xs font-bold whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none shadow-md">
          Hẹn Xem Trọ Ngay
        </span>
      </button>

      {/* Button 2: Zalo Chat Direct */}
      <a
        href="https://zalo.me/0988776655"
        target="_blank"
        rel="noopener noreferrer"
        title="Chat Zalo nhận tư vấn ngay"
        className="group relative w-14 h-14 rounded-full bg-gradient-to-tr from-blue-600 to-cyan-500 text-white shadow-xl shadow-blue-500/30 flex items-center justify-center hover:scale-110 transition-transform duration-200"
      >
        <span className="text-xs font-black tracking-tighter">Zalo</span>
        <span className="absolute right-16 px-3 py-1.5 rounded-xl bg-slate-900 text-white text-xs font-bold whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none shadow-md">
          Nhắn Zalo Chủ Trọ
        </span>
      </a>

      {/* Button 3: Hotline Call with Pulse Wave & Shake */}
      <a
        href="tel:0988776655"
        title="Gọi Hotline tư vấn 24/7"
        className="group relative w-14 h-14 rounded-full bg-emerald-500 text-white shadow-xl shadow-emerald-500/40 flex items-center justify-center hover:scale-110 transition-transform duration-200 animate-pulse-ring"
      >
        <Phone className="w-6 h-6 animate-phone-shake" />
        <span className="absolute right-16 px-3 py-1.5 rounded-xl bg-slate-900 text-white text-xs font-bold whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none shadow-md">
          Gọi Ngay: 0988.776.655
        </span>
      </a>

    </aside>
  );
};
