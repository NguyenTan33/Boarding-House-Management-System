import React, { useState, useEffect } from 'react';
import { Send, User, Phone, DoorOpen, Clock } from 'lucide-react';
import type { BookingAppointment } from '../types';

interface BookingSectionProps {
  selectedRoom: string;
  onSuccess: (booking: BookingAppointment) => void;
}

export const BookingSection: React.FC<BookingSectionProps> = ({ selectedRoom, onSuccess }) => {
  const [fullName, setFullName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [roomPreference, setRoomPreference] = useState('Tất cả các phòng còn trống');
  const [preferredTime, setPreferredTime] = useState('Hôm nay (Buổi chiều: 14h - 17h)');
  const [note, setNote] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (selectedRoom) {
      setRoomPreference(selectedRoom);
    }
  }, [selectedRoom]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    setTimeout(() => {
      onSuccess({
        fullName,
        phoneNumber,
        roomPreference,
        preferredTime,
        note,
      });
      setIsSubmitting(false);
      setFullName('');
      setPhoneNumber('');
      setNote('');
    }, 600);
  };

  return (
    <section id="dat-lich" className="py-20 bg-slate-50 relative">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        
        <div className="bg-white rounded-3xl p-8 sm:p-12 border border-slate-200 shadow-xl relative overflow-hidden">
          {/* Subtle Accent Glow */}
          <div className="absolute -top-12 -right-12 w-48 h-48 bg-emerald-100 rounded-full blur-2xl pointer-events-none"></div>

          <div className="text-center max-w-xl mx-auto space-y-2 mb-8">
            <span className="text-xs font-bold text-emerald-700 uppercase tracking-widest bg-emerald-50 px-3 py-1 rounded-full border border-emerald-200">
              Xem Phòng Thực Tế Miễn Phí
            </span>
            <h2 className="text-2xl sm:text-3xl font-extrabold text-slate-900">
              Đặt Lịch Hẹn Xem Phòng Trực Tiếp
            </h2>
            <p className="text-xs sm:text-sm text-slate-600">
              Vui lòng để lại thông tin, quản lý nhà trọ sẽ liên hệ xác nhận và chuẩn bị phòng đón tiếp bạn chu đáo nhất!
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              
              {/* Full Name */}
              <div className="space-y-1.5">
                <label className="text-xs font-bold text-slate-700">Họ và tên của bạn <span className="text-red-500">*</span></label>
                <div className="relative">
                  <span className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                    <User className="w-4 h-4" />
                  </span>
                  <input
                    type="text"
                    required
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    placeholder="Nguyễn Văn A"
                    className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-all"
                  />
                </div>
              </div>

              {/* Phone */}
              <div className="space-y-1.5">
                <label className="text-xs font-bold text-slate-700">Số điện thoại liên hệ <span className="text-red-500">*</span></label>
                <div className="relative">
                  <span className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                    <Phone className="w-4 h-4" />
                  </span>
                  <input
                    type="tel"
                    required
                    pattern="[0-9]{10}"
                    value={phoneNumber}
                    onChange={(e) => setPhoneNumber(e.target.value)}
                    placeholder="0912345678"
                    className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-all"
                  />
                </div>
              </div>

              {/* Room Preference */}
              <div className="space-y-1.5">
                <label className="text-xs font-bold text-slate-700">Loại phòng bạn muốn xem</label>
                <div className="relative">
                  <span className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                    <DoorOpen className="w-4 h-4" />
                  </span>
                  <select
                    value={roomPreference}
                    onChange={(e) => setRoomPreference(e.target.value)}
                    className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-all appearance-none"
                  >
                    <option value="Tất cả các phòng còn trống">Tất cả các phòng còn trống</option>
                    <option value="P.302 (28m² - Tầng 3 - Studio ban công)">Studio View Thoáng Cửa Sổ Lớn (28m²)</option>
                    <option value="P.204 (34m² - Tầng 2 - Duplex gác lửng)">Duplex Gác Lửng Cao Cấp (34m²)</option>
                    <option value="P.401 (38m² - Tầng 4 - Căn 1PN)">Căn Hộ Mini 1PN Riêng Biệt (38m²)</option>
                    <option value="P.102 (24m² - Tầng 1 - Trệt tiện nghi)">Phòng Trệt Tiện Nghi (24m²)</option>
                  </select>
                </div>
              </div>

              {/* Time Slot */}
              <div className="space-y-1.5">
                <label className="text-xs font-bold text-slate-700">Khung giờ hẹn thuận tiện</label>
                <div className="relative">
                  <span className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                    <Clock className="w-4 h-4" />
                  </span>
                  <select
                    value={preferredTime}
                    onChange={(e) => setPreferredTime(e.target.value)}
                    className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-all appearance-none"
                  >
                    <option value="Hôm nay (Buổi chiều: 14h - 17h)">Hôm nay (Buổi chiều: 14h - 17h)</option>
                    <option value="Hôm nay (Buổi tối: 18h - 20h)">Hôm nay (Buổi tối: 18h - 20h)</option>
                    <option value="Ngày mai (Buổi sáng: 8h - 11h)">Ngày mai (Buổi sáng: 8h - 11h)</option>
                    <option value="Ngày mai (Buổi chiều: 14h - 17h)">Ngày mai (Buổi chiều: 14h - 17h)</option>
                    <option value="Cuối tuần này (Thứ 7 / CN)">Cuối tuần này (Thứ 7 / CN)</option>
                  </select>
                </div>
              </div>

            </div>

            {/* Note */}
            <div className="space-y-1.5">
              <label className="text-xs font-bold text-slate-700">Ghi chú thêm</label>
              <div className="relative">
                <textarea
                  rows={2}
                  value={note}
                  onChange={(e) => setNote(e.target.value)}
                  placeholder="Ví dụ: Mình dự kiến chuyển vào đầu tháng sau, muốn xem phòng có ban công..."
                  className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-all"
                />
              </div>
            </div>

            {/* Submit */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-4 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white font-extrabold text-sm sm:text-base shadow-lg shadow-emerald-600/30 hover:shadow-xl transition-all flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50"
            >
              <Send className="w-4 h-4" />
              {isSubmitting ? 'Đang gửi thông tin...' : 'Xác Nhận Đặt Lịch Xem Phòng'}
            </button>
          </form>

        </div>

      </div>
    </section>
  );
};
