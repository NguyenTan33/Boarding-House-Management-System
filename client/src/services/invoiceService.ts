import { apiClient } from './apiClient';
import type { Invoice, VietQRResponse } from '../types';

export const invoiceService = {
  // Lấy danh sách hóa đơn của khách thuê
  async getInvoices(params?: { month?: number; year?: number; status?: string }): Promise<Invoice[]> {
    try {
      const response = await apiClient.get<Invoice[]>('/invoices', { params });
      return response.data;
    } catch {
      // Mock data for demo portal
      return [
        {
          invoiceId: 18,
          invoiceCode: 'HDT-202611-P204',
          contractNumber: 'HD-20260101-P204-5678',
          roomNumber: '204',
          houseName: 'GreenHouse Linh Trung',
          houseAddress: '123 Đường Số 8, P. Linh Trung, TP. Thủ Đức',
          tenantName: 'Nguyễn Minh Tân',
          month: 11,
          year: 2026,
          issueDate: '2026-11-01T00:00:00Z',
          dueDate: '2026-11-05T00:00:00Z',
          roomPrice: 4200000,
          electricityUsage: 140,
          electricityUnitPrice: 3500,
          electricityCost: 490000,
          waterUsage: 7,
          waterUnitPrice: 20000,
          waterCost: 140000,
          internetCost: 80000,
          cleaningCost: 30000,
          parkingCost: 120000,
          totalAmount: 5060000,
          paidAmount: 0,
          remainingAmount: 5060000,
          status: 0,
          statusName: 'Unpaid',
        },
      ];
    }
  },

  // Lấy mã VietQR động để chuyển khoản tự động
  async getVietQR(invoiceId: number): Promise<VietQRResponse> {
    try {
      const response = await apiClient.get<VietQRResponse>(`/payments/${invoiceId}/vietqr`);
      return response.data;
    } catch {
      // Fallback VietQR template
      return {
        invoiceId,
        invoiceCode: 'HDT-202611-P204',
        amount: 5060000,
        bankAccountNo: '0988776655',
        bankCode: 'MB',
        bankName: 'Ngân hàng Quân Đội (MBBank)',
        accountName: 'CHU NHA TRO GREENHOUSE',
        description: `PHONG204 T11 HDT-202611-P204`,
        qrImageUrl: `https://img.vietqr.io/image/MB-0988776655-compact2.png?amount=5060000&addInfo=PHONG204+T11&accountName=CHU+NHA+TRO`,
        qrQuickLink: 'https://vietqr.io',
      };
    }
  },
};
