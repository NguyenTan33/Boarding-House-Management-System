import React, { useState, useEffect } from 'react';
import { X, Key, UserPlus, FileText, QrCode } from 'lucide-react';
import { authService } from '../services/authService';
import { invoiceService } from '../services/invoiceService';
import type { Invoice, UserSummary, VietQRResponse } from '../types';

interface AuthModalsProps {
  loginOpen: boolean;
  registerOpen: boolean;
  invoicesOpen: boolean;
  onCloseLogin: () => void;
  onCloseRegister: () => void;
  onCloseInvoices: () => void;
  onLoginSuccess: (user: UserSummary) => void;
  onShowToast: (title: string, message: string, isSuccess?: boolean) => void;
}

export const AuthModals: React.FC<AuthModalsProps> = ({
  loginOpen,
  registerOpen,
  invoicesOpen,
  onCloseLogin,
  onCloseRegister,
  onCloseInvoices,
  onLoginSuccess,
  onShowToast,
}) => {
  // Login State
  const [loginRole, setLoginRole] = useState<'Tenant' | 'Landlord'>('Tenant');
  const [loginUsername, setLoginUsername] = useState('0988776655');
  const [loginPassword, setLoginPassword] = useState('Password123@');
  const [loginLoading, setLoginLoading] = useState(false);

  // Register State
  const [regFullName, setRegFullName] = useState('');
  const [regPhone, setRegPhone] = useState('');
  const [regPassword, setRegPassword] = useState('');
  const [regRole, setRegRole] = useState<'Tenant' | 'Landlord'>('Tenant');
  const [regLoading, setRegLoading] = useState(false);

  // Invoices & VietQR State
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);
  const [vietQrData, setVietQrData] = useState<VietQRResponse | null>(null);
  const [loadingInvoice, setLoadingInvoice] = useState(false);

  // Fetch invoices when Invoices modal opens
  useEffect(() => {
    if (invoicesOpen) {
      setLoadingInvoice(true);
      invoiceService
        .getInvoices()
        .then(async (res) => {
          if (res.length > 0) {
            setSelectedInvoice(res[0]);
            const qr = await invoiceService.getVietQR(res[0].invoiceId);
            setVietQrData(qr);
          }
        })
        .finally(() => setLoadingInvoice(false));
    }
  }, [invoicesOpen]);

  // Handle Login Submit
  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoginLoading(true);
    try {
      const res = await authService.login({
        username: loginUsername,
        password: loginPassword,
      });
      onLoginSuccess(res.user);
      onShowToast('Đăng Nhập Thành Công!', `Chào mừng ${res.user.fullName} quay trở lại hệ thống!`);
      onCloseLogin();
    } catch (err: unknown) {
      const errMsg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Sai số điện thoại hoặc mật khẩu!';
      onShowToast('Đăng Nhập Thất Bại', errMsg, false);
    } finally {
      setLoginLoading(false);
    }
  };

  // Handle Register Submit
  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setRegLoading(true);
    try {
      const res = await authService.register({
        username: regPhone,
        phoneNumber: regPhone,
        password: regPassword,
        fullName: regFullName,
        role: regRole,
      });
      onLoginSuccess(res.user);
      onShowToast('Đăng Ký Thành Công!', `Tài khoản ${res.user.fullName} đã được tạo thành công.`);
      onCloseRegister();
    } catch (err: unknown) {
      const errMsg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Số điện thoại hoặc CCCD đã được đăng ký!';
      onShowToast('Đăng Ký Thất Bại', errMsg, false);
    } finally {
      setRegLoading(false);
    }
  };

  return (
    <>
      {/* ========================================================================= */}
      {/* 1. LOGIN MODAL                                                            */}
      {/* ========================================================================= */}
      {loginOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-xs">
          <div className="relative w-full max-w-md bg-white rounded-3xl shadow-2xl border border-slate-100 overflow-hidden animate-in fade-in zoom-in-95 duration-200">
            <button
              onClick={onCloseLogin}
              className="absolute top-4 right-4 w-9 h-9 rounded-full bg-slate-100 hover:bg-slate-200 text-slate-500 flex items-center justify-center transition-colors cursor-pointer"
            >
              <X className="w-4 h-4" />
            </button>

            <div className="p-8">
              <div className="text-center mb-6">
                <div className="w-12 h-12 rounded-2xl bg-emerald-100 text-emerald-600 flex items-center justify-center mx-auto mb-3">
                  <Key className="w-6 h-6" />
                </div>
                <h3 className="text-xl font-extrabold text-slate-900">Đăng Nhập Hệ Thống</h3>
                <p className="text-xs text-slate-500 mt-1">Dành cho cư dân đang thuê hoặc ban quản lý trọ</p>
              </div>

              {/* Role Toggle Tabs */}
              <div className="grid grid-cols-2 gap-2 bg-slate-100 p-1 rounded-xl mb-5 text-xs font-bold">
                <button
                  type="button"
                  onClick={() => setLoginRole('Tenant')}
                  className={`py-2 rounded-lg transition-all cursor-pointer ${
                    loginRole === 'Tenant' ? 'bg-white text-slate-800 shadow-xs' : 'text-slate-500 hover:text-slate-800'
                  }`}
                >
                  Khách Thuê Trọ
                </button>
                <button
                  type="button"
                  onClick={() => setLoginRole('Landlord')}
                  className={`py-2 rounded-lg transition-all cursor-pointer ${
                    loginRole === 'Landlord' ? 'bg-white text-slate-800 shadow-xs' : 'text-slate-500 hover:text-slate-800'
                  }`}
                >
                  Chủ Trọ / Quản Lý
                </button>
              </div>

              <form onSubmit={handleLogin} className="space-y-4">
                <div className="space-y-1">
                  <label className="text-xs font-bold text-slate-700">Số điện thoại hoặc Tài khoản</label>
                  <input
                    type="text"
                    required
                    value={loginUsername}
                    onChange={(e) => setLoginUsername(e.target.value)}
                    placeholder="0988xxxxxx"
                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white"
                  />
                </div>

                <div className="space-y-1">
                  <div className="flex justify-between items-center">
                    <label className="text-xs font-bold text-slate-700">Mật khẩu</label>
                    <a href="#" className="text-[11px] font-semibold text-emerald-600 hover:underline">
                      Quên mật khẩu?
                    </a>
                  </div>
                  <input
                    type="password"
                    required
                    value={loginPassword}
                    onChange={(e) => setLoginPassword(e.target.value)}
                    placeholder="••••••••"
                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white"
                  />
                </div>

                <button
                  type="submit"
                  disabled={loginLoading}
                  className="w-full py-3 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-sm rounded-xl shadow-md transition-all cursor-pointer disabled:opacity-50"
                >
                  {loginLoading ? 'Đang xác thực...' : 'Đăng Nhập Ngay'}
                </button>
              </form>

              <div className="mt-5 text-center text-xs text-slate-500">
                Chưa có tài khoản?{' '}
                <button
                  type="button"
                  onClick={() => {
                    onCloseLogin();
                    onCloseRegister();
                  }}
                  className="font-bold text-emerald-600 hover:underline cursor-pointer"
                >
                  Đăng ký ngay
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* ========================================================================= */}
      {/* 2. REGISTER MODAL                                                         */}
      {/* ========================================================================= */}
      {registerOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-xs">
          <div className="relative w-full max-w-md bg-white rounded-3xl shadow-2xl border border-slate-100 overflow-hidden animate-in fade-in zoom-in-95 duration-200">
            <button
              onClick={onCloseRegister}
              className="absolute top-4 right-4 w-9 h-9 rounded-full bg-slate-100 hover:bg-slate-200 text-slate-500 flex items-center justify-center transition-colors cursor-pointer"
            >
              <X className="w-4 h-4" />
            </button>

            <div className="p-8">
              <div className="text-center mb-6">
                <div className="w-12 h-12 rounded-2xl bg-teal-100 text-teal-600 flex items-center justify-center mx-auto mb-3">
                  <UserPlus className="w-6 h-6" />
                </div>
                <h3 className="text-xl font-extrabold text-slate-900">Tạo Tài Khoản Mới</h3>
                <p className="text-xs text-slate-500 mt-1">Đăng ký hồ sơ để lưu thông tin hợp đồng & phòng trọ</p>
              </div>

              <form onSubmit={handleRegister} className="space-y-3.5">
                <div className="space-y-1">
                  <label className="text-xs font-bold text-slate-700">Họ và tên</label>
                  <input
                    type="text"
                    required
                    value={regFullName}
                    onChange={(e) => setRegFullName(e.target.value)}
                    placeholder="Nguyễn Văn A"
                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white"
                  />
                </div>

                <div className="space-y-1">
                  <label className="text-xs font-bold text-slate-700">Số điện thoại</label>
                  <input
                    type="tel"
                    required
                    pattern="[0-9]{10}"
                    value={regPhone}
                    onChange={(e) => setRegPhone(e.target.value)}
                    placeholder="09xxxxxxxx"
                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white"
                  />
                </div>

                <div className="space-y-1">
                  <label className="text-xs font-bold text-slate-700">Mật khẩu</label>
                  <input
                    type="password"
                    required
                    minLength={6}
                    value={regPassword}
                    onChange={(e) => setRegPassword(e.target.value)}
                    placeholder="Tối thiểu 6 ký tự"
                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white"
                  />
                </div>

                <div className="space-y-1">
                  <label className="text-xs font-bold text-slate-700">Vai trò</label>
                  <select
                    value={regRole}
                    onChange={(e) => setRegRole(e.target.value as 'Tenant' | 'Landlord')}
                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-hidden focus:ring-2 focus:ring-emerald-500 focus:bg-white"
                  >
                    <option value="Tenant">Khách thuê trọ</option>
                    <option value="Landlord">Chủ trọ / Quản lý</option>
                  </select>
                </div>

                <button
                  type="submit"
                  disabled={regLoading}
                  className="w-full py-3 bg-teal-600 hover:bg-teal-700 text-white font-bold text-sm rounded-xl shadow-md transition-all mt-2 cursor-pointer disabled:opacity-50"
                >
                  {regLoading ? 'Đang tạo hồ sơ...' : 'Đăng Ký Tài Khoản'}
                </button>
              </form>

              <div className="mt-4 text-center text-xs text-slate-500">
                Đã có tài khoản?{' '}
                <button
                  type="button"
                  onClick={() => {
                    onCloseRegister();
                  }}
                  className="font-bold text-emerald-600 hover:underline cursor-pointer"
                >
                  Đăng nhập
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* ========================================================================= */}
      {/* 3. TENANT INVOICES & DYNAMIC VIETQR MODAL                                  */}
      {/* ========================================================================= */}
      {invoicesOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-xs">
          <div className="relative w-full max-w-2xl bg-white rounded-3xl shadow-2xl border border-slate-100 overflow-hidden max-h-[90vh] flex flex-col">
            
            {/* Header */}
            <div className="px-6 py-4 border-b border-slate-200 flex items-center justify-between bg-slate-50">
              <div className="flex items-center gap-2.5">
                <div className="w-8 h-8 rounded-xl bg-emerald-100 text-emerald-700 flex items-center justify-center">
                  <FileText className="w-4 h-4" />
                </div>
                <div>
                  <h3 className="text-base font-extrabold text-slate-900">Hóa Đơn Tiền Nhà & VietQR</h3>
                  <p className="text-xs text-slate-500">Tự động gạch nợ sau khi quét mã ngân hàng</p>
                </div>
              </div>
              <button
                onClick={onCloseInvoices}
                className="w-8 h-8 rounded-full bg-slate-200 hover:bg-slate-300 text-slate-600 flex items-center justify-center transition-colors cursor-pointer"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Content Body */}
            <div className="p-6 overflow-y-auto space-y-6">
              {loadingInvoice ? (
                <div className="text-center py-12 text-slate-500 text-sm">Đang tải hóa đơn tháng...</div>
              ) : selectedInvoice ? (
                <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
                  
                  {/* Left: Bill Breakdown */}
                  <div className="md:col-span-7 space-y-4">
                    <div className="p-4 rounded-2xl bg-slate-50 border border-slate-200 space-y-2 text-xs">
                      <div className="flex justify-between">
                        <span className="text-slate-500">Mã hóa đơn:</span>
                        <span className="font-bold text-slate-900">{selectedInvoice.invoiceCode}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-slate-500">Phòng:</span>
                        <span className="font-bold text-slate-900">{selectedInvoice.roomNumber} ({selectedInvoice.houseName})</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-slate-500">Khách thuê:</span>
                        <span className="font-bold text-slate-900">{selectedInvoice.tenantName}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-slate-500">Kỳ thanh toán:</span>
                        <span className="font-bold text-emerald-700">Tháng {selectedInvoice.month}/{selectedInvoice.year}</span>
                      </div>
                    </div>

                    {/* Breakdown table */}
                    <div className="space-y-2 text-xs">
                      <div className="flex justify-between py-1 border-b border-slate-100">
                        <span className="text-slate-600">Tiền phòng</span>
                        <span className="font-semibold">{selectedInvoice.roomPrice.toLocaleString()} đ</span>
                      </div>
                      <div className="flex justify-between py-1 border-b border-slate-100">
                        <span className="text-slate-600">Tiền điện ({selectedInvoice.electricityUsage} kWh x {selectedInvoice.electricityUnitPrice.toLocaleString()}đ)</span>
                        <span className="font-semibold">{selectedInvoice.electricityCost.toLocaleString()} đ</span>
                      </div>
                      <div className="flex justify-between py-1 border-b border-slate-100">
                        <span className="text-slate-600">Tiền nước ({selectedInvoice.waterUsage} m³ x {selectedInvoice.waterUnitPrice.toLocaleString()}đ)</span>
                        <span className="font-semibold">{selectedInvoice.waterCost.toLocaleString()} đ</span>
                      </div>
                      <div className="flex justify-between py-1 border-b border-slate-100">
                        <span className="text-slate-600">Wifi + Dịch vụ vệ sinh + Gửi xe</span>
                        <span className="font-semibold">{(selectedInvoice.internetCost + selectedInvoice.cleaningCost + selectedInvoice.parkingCost).toLocaleString()} đ</span>
                      </div>
                      <div className="flex justify-between py-2 border-t-2 border-slate-200 text-sm font-extrabold text-slate-900">
                        <span>Tổng tiền thanh toán:</span>
                        <span className="text-emerald-600">{selectedInvoice.totalAmount.toLocaleString()} đ</span>
                      </div>
                    </div>
                  </div>

                  {/* Right: VietQR Dynamic Code */}
                  <div className="md:col-span-5 flex flex-col items-center justify-center text-center p-4 rounded-2xl bg-emerald-50 border border-emerald-200">
                    <div className="flex items-center gap-1.5 text-xs font-bold text-emerald-800 mb-2">
                      <QrCode className="w-4 h-4" /> Quét mã VietQR chuyển khoản
                    </div>

                    {vietQrData?.qrImageUrl ? (
                      <div className="bg-white p-2 rounded-xl shadow-md border border-emerald-200 mb-2">
                        <img
                          src={vietQrData.qrImageUrl}
                          alt="VietQR Chuyển khoản"
                          className="w-44 h-44 object-contain rounded-lg"
                        />
                      </div>
                    ) : (
                      <div className="w-44 h-44 bg-slate-200 rounded-xl flex items-center justify-center text-xs text-slate-500 mb-2">
                        Đang tạo mã QR...
                      </div>
                    )}

                    <p className="text-[11px] text-slate-600">
                      Ngân hàng: <strong>{vietQrData?.bankName || 'MBBank'}</strong>
                    </p>
                    <p className="text-[11px] text-slate-600">
                      STK: <strong>{vietQrData?.bankAccountNo || '0988776655'}</strong>
                    </p>
                    <p className="text-[10px] text-emerald-700 mt-1">
                      Nội dung: <strong>{vietQrData?.description || 'PHONG204 T11'}</strong>
                    </p>
                  </div>

                </div>
              ) : (
                <div className="text-center py-10 text-slate-500 text-xs">
                  Hiện bạn không có hóa đơn nào chưa thanh toán.
                </div>
              )}
            </div>

          </div>
        </div>
      )}
    </>
  );
};
