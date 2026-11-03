import React from 'react';
import { Fingerprint, Clock, Zap, ShieldCheck, CalendarCheck, Eye, Sparkles } from 'lucide-react';

interface HeroSectionProps {
  onExploreRooms: () => void;
  onBookAppointment: () => void;
}

export const HeroSection: React.FC<HeroSectionProps> = ({ onExploreRooms, onBookAppointment }) => {
  return (
    <section id="gioi-thieu" className="pt-28 pb-16 lg:pt-36 lg:pb-24 overflow-hidden relative">
      {/* Decorative background glows */}
      <div className="absolute top-1/4 -left-20 w-96 h-96 bg-emerald-200/40 rounded-full blur-3xl pointer-events-none -z-10"></div>
      <div className="absolute bottom-10 right-0 w-80 h-80 bg-teal-200/30 rounded-full blur-3xl pointer-events-none -z-10"></div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 items-center">
          
          {/* Left Text & Value Proposition */}
          <div className="lg:col-span-7 space-y-6 text-center lg:text-left">
            
            {/* Trust Pill */}
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-emerald-50 border border-emerald-200/80 text-emerald-800 text-xs sm:text-sm font-bold shadow-xs">
              <span className="flex h-2 w-2 rounded-full bg-emerald-500 animate-pulse"></span>
              Hệ Thống Nhà Trọ Chuẩn Xanh • Giờ Giấc Tự Do 24/7
            </div>

            {/* Headline */}
            <h1 className="text-3xl sm:text-4xl lg:text-5xl font-extrabold tracking-tight text-slate-900 leading-[1.2]">
              Không Gian Sống Tiện Nghi, <br className="hidden sm:inline" />
              <span className="bg-gradient-to-r from-emerald-600 via-teal-600 to-cyan-600 bg-clip-text text-transparent">
                An Ninh Chuẩn Mực & Vị Trí Vàng
              </span>
            </h1>

            {/* Subtitle */}
            <p className="text-base sm:text-lg text-slate-600 max-w-2xl mx-auto lg:mx-0 leading-relaxed">
              Phòng mới 100%, ban công đón nắng gió tự nhiên, khóa vân tay thẻ từ hiện đại. Tọa lạc ngay trục giao thông huyết mạch, liền kề các trường đại học, bệnh viện đa khoa và khu chợ dân sinh sầm uất.
            </p>

            {/* 4 Feature Badges */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 pt-2">
              <div className="p-3.5 rounded-2xl bg-white border border-slate-200/80 shadow-xs flex flex-col items-center sm:items-start">
                <Fingerprint className="w-5 h-5 text-emerald-600 mb-1.5" />
                <span className="text-xs font-bold text-slate-800">Khóa Vân Tay</span>
                <span className="text-[11px] text-slate-500">Camera AI 24/7</span>
              </div>
              <div className="p-3.5 rounded-2xl bg-white border border-slate-200/80 shadow-xs flex flex-col items-center sm:items-start">
                <Clock className="w-5 h-5 text-amber-500 mb-1.5" />
                <span className="text-xs font-bold text-slate-800">Giờ Giấc Tự Do</span>
                <span className="text-[11px] text-slate-500">Không chung chủ</span>
              </div>
              <div className="p-3.5 rounded-2xl bg-white border border-slate-200/80 shadow-xs flex flex-col items-center sm:items-start">
                <Zap className="w-5 h-5 text-teal-600 mb-1.5" />
                <span className="text-xs font-bold text-slate-800">Sạc Xe Điện</span>
                <span className="text-[11px] text-slate-500">Hầm xe rộng rãi</span>
              </div>
              <div className="p-3.5 rounded-2xl bg-white border border-slate-200/80 shadow-xs flex flex-col items-center sm:items-start">
                <ShieldCheck className="w-5 h-5 text-blue-600 mb-1.5" />
                <span className="text-xs font-bold text-slate-800">PCCC Đạt Chuẩn</span>
                <span className="text-[11px] text-slate-500">Kiểm định 2026</span>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="pt-4 flex flex-col sm:flex-row items-center justify-center lg:justify-start gap-4">
              <button
                onClick={onBookAppointment}
                className="w-full sm:w-auto px-7 py-3.5 rounded-2xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-sm shadow-lg shadow-emerald-600/30 hover:-translate-y-0.5 transition-all flex items-center justify-center gap-2"
              >
                <CalendarCheck className="w-4 h-4" />
                Đặt Lịch Hẹn Xem Trọ Thực Tế
              </button>
              <button
                onClick={onExploreRooms}
                className="w-full sm:w-auto px-7 py-3.5 rounded-2xl bg-white hover:bg-slate-100 text-slate-700 font-bold text-sm border border-slate-300 shadow-xs transition-all flex items-center justify-center gap-2"
              >
                <Eye className="w-4 h-4 text-emerald-600" />
                Khám Phá Phòng Còn Trống
              </button>
            </div>

            {/* Live Room Status Alert (Strictly NO Price) */}
            <div className="flex items-center justify-center lg:justify-start gap-3 text-xs text-slate-500 font-medium pt-2">
              <span className="inline-flex items-center gap-1.5 text-emerald-800 font-bold bg-emerald-100/70 px-3 py-1 rounded-full border border-emerald-200">
                <span className="w-2 h-2 rounded-full bg-emerald-600"></span>
                Hiện đang còn 3 phòng trống sẵn sàng
              </span>
              <span>•</span>
              <span className="flex items-center gap-1 text-slate-600">
                <Sparkles className="w-3.5 h-3.5 text-amber-500" /> Hỗ trợ xem phòng miễn phí 24/7
              </span>
            </div>

          </div>

          {/* Right Visual Image */}
          <div className="lg:col-span-5 relative">
            <div className="relative mx-auto max-w-md lg:max-w-none">
              <div className="relative rounded-3xl overflow-hidden shadow-2xl border-4 border-white bg-slate-100 aspect-4/3 sm:aspect-16/11">
                <img
                  src="https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1000&q=80"
                  alt="Không gian phòng trọ thực tế"
                  className="w-full h-full object-cover hover:scale-105 transition-transform duration-700"
                />
                
                {/* Visual Image Overlay */}
                <div className="absolute bottom-4 left-4 right-4 p-3.5 rounded-2xl bg-slate-900/70 backdrop-blur-md text-white flex items-center justify-between">
                  <div>
                    <p className="text-xs font-semibold text-emerald-400">Hình ảnh phòng mẫu thực tế</p>
                    <p className="text-sm font-bold">Studio Duplex - Ban công đón gió</p>
                  </div>
                  <span className="px-2.5 py-1 bg-white/20 rounded-lg text-xs font-medium">Full Tiện Nghi</span>
                </div>
              </div>

              {/* Floating Badge 1 */}
              <div className="absolute -top-5 -left-5 bg-white p-3 rounded-2xl shadow-xl border border-slate-100 hidden sm:flex items-center gap-3">
                <div className="w-9 h-9 rounded-xl bg-emerald-100 text-emerald-600 flex items-center justify-center">
                  <Fingerprint className="w-5 h-5" />
                </div>
                <div>
                  <p className="text-xs font-bold text-slate-900">An Ninh 3 Lớp</p>
                  <p className="text-[11px] text-slate-500">Khóa cửa từ & FaceID</p>
                </div>
              </div>

              {/* Floating Badge 2 */}
              <div className="absolute -bottom-5 -right-5 bg-white p-3 rounded-2xl shadow-xl border border-slate-100 hidden sm:flex items-center gap-3">
                <div className="w-9 h-9 rounded-xl bg-teal-100 text-teal-600 flex items-center justify-center">
                  <ShieldCheck className="w-5 h-5" />
                </div>
                <div>
                  <p className="text-xs font-bold text-slate-900">Dân Trí Cao</p>
                  <p className="text-[11px] text-slate-500">100% Sinh viên & NVVP</p>
                </div>
              </div>

            </div>
          </div>

        </div>
      </div>
    </section>
  );
};
