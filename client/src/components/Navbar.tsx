import React, { useState, useEffect } from 'react';
import { Home, Phone, User, LogOut, FileText, Menu, X } from 'lucide-react';
import type { UserSummary } from '../types';

interface NavbarProps {
  onOpenLogin: () => void;
  onOpenRegister: () => void;
  onOpenInvoices: () => void;
  currentUser: UserSummary | null;
  onLogout: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({
  onOpenLogin,
  onOpenRegister,
  onOpenInvoices,
  currentUser,
  onLogout,
}) => {
  const [isScrolled, setIsScrolled] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  return (
    <header
      className={`fixed top-0 left-0 right-0 z-40 transition-all duration-300 ${
        isScrolled
          ? 'bg-white/95 backdrop-blur-md shadow-md border-b border-slate-200/80 py-3'
          : 'bg-white/80 backdrop-blur-sm border-b border-slate-200/50 py-4'
      }`}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex items-center justify-between">
        
        {/* Brand Logo */}
        <a href="#" className="flex items-center gap-3 group">
          <div className="w-11 h-11 rounded-2xl bg-gradient-to-tr from-emerald-600 to-teal-500 flex items-center justify-center text-white shadow-md shadow-emerald-500/20 group-hover:scale-105 transition-transform">
            <Home className="w-5 h-5" />
          </div>
          <div>
            <span className="text-xl font-extrabold tracking-tight bg-gradient-to-r from-slate-900 via-emerald-900 to-emerald-700 bg-clip-text text-transparent">
              GREEN HOUSE
            </span>
            <span className="block text-[11px] font-bold text-slate-500 uppercase tracking-wider">
              Nhà Trọ Xanh Cao Cấp
            </span>
          </div>
        </a>

        {/* Desktop Navigation Links */}
        <nav className="hidden md:flex items-center gap-7 text-sm font-semibold text-slate-600">
          <a href="#gioi-thieu" className="hover:text-emerald-600 transition-colors">
            Giới Thiệu
          </a>
          <a href="#khong-gian-phong" className="hover:text-emerald-600 transition-colors">
            Không Gian Phòng
          </a>
          <a href="#tien-ich" className="hover:text-emerald-600 transition-colors">
            Tiện Ích & An Ninh
          </a>
          <a href="#vi-tri-ket-noi" className="hover:text-emerald-600 transition-colors flex items-center gap-1.5 text-emerald-700 font-bold">
            <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
            Vị Trí & Ngoại Khu
          </a>
          <a href="#dat-lich" className="hover:text-emerald-600 transition-colors">
            Hẹn Xem Trọ
          </a>
        </nav>

        {/* Right Actions: Hotline & Auth State */}
        <div className="hidden sm:flex items-center gap-3">
          {/* Hotline */}
          <a
            href="tel:0988776655"
            className="flex items-center gap-2 px-3.5 py-2 rounded-full bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-bold border border-slate-200 transition-all"
          >
            <span className="w-2 h-2 rounded-full bg-emerald-500 animate-ping"></span>
            <Phone className="w-3.5 h-3.5 text-emerald-600" />
            <span>0988.776.655</span>
          </a>

          {currentUser ? (
            // Logged-in State
            <div className="flex items-center gap-2">
              <button
                onClick={onOpenInvoices}
                className="flex items-center gap-1.5 px-3 py-2 rounded-xl bg-emerald-50 hover:bg-emerald-100 text-emerald-800 text-xs font-bold border border-emerald-200 transition-all"
                title="Tra cứu hóa đơn & VietQR"
              >
                <FileText className="w-3.5 h-3.5 text-emerald-600" />
                <span>Hóa Đơn & QR</span>
              </button>

              <div className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-slate-100 border border-slate-200">
                <div className="w-6 h-6 rounded-full bg-emerald-600 text-white flex items-center justify-center text-xs font-bold">
                  {currentUser.fullName.charAt(0)}
                </div>
                <div className="text-left text-xs">
                  <span className="font-bold text-slate-800 block leading-tight">{currentUser.fullName}</span>
                  <span className="text-[10px] text-slate-500 font-medium">Khách thuê trọ</span>
                </div>
                <button
                  onClick={onLogout}
                  title="Đăng xuất"
                  className="p-1 hover:text-red-500 text-slate-400 transition-colors ml-1"
                >
                  <LogOut className="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          ) : (
            // Guest State (No login required to view)
            <div className="flex items-center gap-2">
              <button
                onClick={onOpenLogin}
                className="px-4 py-2 text-xs font-bold text-slate-700 hover:text-emerald-600 rounded-xl hover:bg-slate-100 border border-transparent hover:border-slate-200 transition-all flex items-center gap-1.5"
              >
                <User className="w-3.5 h-3.5" />
                Đăng Nhập
              </button>
              <button
                onClick={onOpenRegister}
                className="px-4 py-2 text-xs font-bold text-white bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 rounded-xl shadow-xs transition-all"
              >
                Đăng Ký
              </button>
            </div>
          )}
        </div>

        {/* Mobile Hamburger Button */}
        <button
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          className="md:hidden p-2 text-slate-600 hover:text-slate-900"
          aria-label="Toggle menu"
        >
          {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
        </button>

      </div>

      {/* Mobile Menu Dropdown */}
      {mobileMenuOpen && (
        <div className="md:hidden bg-white border-b border-slate-200 px-4 pt-3 pb-6 space-y-3 shadow-lg">
          <a
            href="#gioi-thieu"
            onClick={() => setMobileMenuOpen(false)}
            className="block py-2 text-sm font-semibold text-slate-700"
          >
            Giới Thiệu
          </a>
          <a
            href="#khong-gian-phong"
            onClick={() => setMobileMenuOpen(false)}
            className="block py-2 text-sm font-semibold text-slate-700"
          >
            Không Gian Phòng
          </a>
          <a
            href="#tien-ich"
            onClick={() => setMobileMenuOpen(false)}
            className="block py-2 text-sm font-semibold text-slate-700"
          >
            Tiện Ích & An Ninh
          </a>
          <a
            href="#vi-tri-ket-noi"
            onClick={() => setMobileMenuOpen(false)}
            className="block py-2 text-sm font-semibold text-emerald-700 font-bold"
          >
            Vị Trí & Ngoại Khu
          </a>
          <a
            href="#dat-lich"
            onClick={() => setMobileMenuOpen(false)}
            className="block py-2 text-sm font-semibold text-slate-700"
          >
            Hẹn Xem Trọ
          </a>

          <div className="pt-3 border-t border-slate-200 flex flex-col gap-2">
            {currentUser ? (
              <div className="flex items-center justify-between">
                <span className="text-xs font-bold text-slate-800">{currentUser.fullName}</span>
                <button onClick={onLogout} className="text-xs font-bold text-red-600">Đăng xuất</button>
              </div>
            ) : (
              <div className="grid grid-cols-2 gap-2">
                <button
                  onClick={() => { setMobileMenuOpen(false); onOpenLogin(); }}
                  className="py-2.5 text-center text-xs font-bold text-slate-700 bg-slate-100 rounded-xl"
                >
                  Đăng Nhập
                </button>
                <button
                  onClick={() => { setMobileMenuOpen(false); onOpenRegister(); }}
                  className="py-2.5 text-center text-xs font-bold text-white bg-emerald-600 rounded-xl"
                >
                  Đăng Ký
                </button>
              </div>
            )}
          </div>
        </div>
      )}
    </header>
  );
};
