import React from 'react';
import { Home, MapPin, Phone, Mail, QrCode } from 'lucide-react';

interface FooterProps {
  onOpenLogin: () => void;
}

export const Footer: React.FC<FooterProps> = ({ onOpenLogin }) => {
  return (
    <footer className="bg-slate-900 text-slate-400 py-12 border-t border-slate-800">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
          
          {/* Brand Col */}
          <div className="space-y-3">
            <div className="flex items-center gap-2.5">
              <div className="w-10 h-10 rounded-xl bg-emerald-600 text-white flex items-center justify-center">
                <Home className="w-5 h-5" />
              </div>
              <span className="text-lg font-extrabold text-white">GREEN HOUSE</span>
            </div>
            <p className="text-xs text-slate-400 leading-relaxed">
              Hệ thống nhà trọ & căn hộ dịch vụ cao cấp chuẩn xanh. Mang đến không gian sống an toàn, văn minh và tiện nghi vượt trội.
            </p>
          </div>

          {/* Contact Col */}
          <div>
            <h4 className="text-sm font-bold text-white mb-3 uppercase tracking-wider">Thông Tin Liên Hệ</h4>
            <ul className="space-y-2 text-xs">
              <li className="flex items-start gap-2">
                <MapPin className="w-4 h-4 text-emerald-500 shrink-0 mt-0.5" />
                <span>Số 123 Đường Số 8, Phường Linh Trung, TP. Thủ Đức, TP.HCM</span>
              </li>
              <li className="flex items-center gap-2">
                <Phone className="w-4 h-4 text-emerald-500 shrink-0" />
                <span>Hotline: 0988.776.655 (Zalo 24/7)</span>
              </li>
              <li className="flex items-center gap-2">
                <Mail className="w-4 h-4 text-emerald-500 shrink-0" />
                <span>support@greenhouse.vn</span>
              </li>
            </ul>
          </div>

          {/* Operating Hours */}
          <div>
            <h4 className="text-sm font-bold text-white mb-3 uppercase tracking-wider">Thời Gian Làm Việc</h4>
            <ul className="space-y-2 text-xs">
              <li>• Tiếp khách xem phòng: 07:30 - 21:00 hàng ngày</li>
              <li>• Hỗ trợ kỹ thuật: 24/7 qua Hotline / Zalo</li>
              <li>• Thứ 7, Chủ Nhật và ngày lễ đón tiếp bình thường</li>
            </ul>
          </div>

          {/* Tenant Portal */}
          <div>
            <h4 className="text-sm font-bold text-white mb-3 uppercase tracking-wider">Cổng Dành Cho Khách Trọ</h4>
            <p className="text-xs text-slate-400 mb-3">
              Cư dân đang ở có thể đăng nhập để tra cứu số điện nước và quét mã VietQR tự động thanh toán.
            </p>
            <button
              onClick={onOpenLogin}
              className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-white text-xs font-bold rounded-xl border border-slate-700 transition-colors flex items-center gap-2 cursor-pointer"
            >
              <QrCode className="w-3.5 h-3.5 text-emerald-400" />
              <span>Cổng Tra Cứu Hóa Đơn</span>
            </button>
          </div>

        </div>

        <div className="pt-8 border-t border-slate-800 text-center text-xs text-slate-500 flex flex-col sm:flex-row items-center justify-between gap-4">
          <p>© 2026 GreenHouse Management System. Bản quyền thuộc về Hệ thống Nhà Trọ Xanh.</p>
          <p className="flex items-center gap-3">
            <span className="hover:text-slate-400 cursor-pointer">Chính sách bảo mật</span>
            <span>•</span>
            <span className="hover:text-slate-400 cursor-pointer">Quy chế phòng trọ</span>
          </p>
        </div>
      </div>
    </footer>
  );
};
