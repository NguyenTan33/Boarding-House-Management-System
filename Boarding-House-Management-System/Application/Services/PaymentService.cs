using System.Text.RegularExpressions;
using Application.DTOs.Payments;
using Application.Exceptions;
using Application.Helpers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enum;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IInvoiceRepository invoiceRepository,
            IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _configuration = configuration;
        }

        public async Task<VietQrResponse> GenerateVietQrAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetInvoiceDetailsAsync(invoiceId);
            if (invoice == null)
                throw new NotFoundException($"Không tìm thấy hóa đơn với ID {invoiceId}.");

            if (invoice.RemainingAmount <= 0)
                throw new BadRequestException("Hóa đơn này đã được thanh toán đầy đủ.");

            // Lấy thông tin tài khoản ngân hàng từ cấu hình appsettings.json hoặc mặc định
            string bankCode = _configuration["VietQR:BankCode"] ?? "MB";
            string accountNo = _configuration["VietQR:AccountNo"] ?? "0987654321";
            string accountName = _configuration["VietQR:AccountName"] ?? "CHU NHA TRO";
            string bankName = _configuration["VietQR:BankName"] ?? "MBBank";

            string description = $"PHONG{invoice.Room?.RoomNumber ?? "0"} T{invoice.Month} {invoice.InvoiceCode}";

            var (qrImageUrl, quickLink) = VietQrHelper.GenerateQrUrl(
                bankCode: bankCode,
                accountNo: accountNo,
                accountName: accountName,
                amount: invoice.RemainingAmount,
                description: description
            );

            return new VietQrResponse
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode,
                Amount = invoice.RemainingAmount,
                BankCode = bankCode,
                BankName = bankName,
                BankAccountNo = accountNo,
                AccountName = accountName,
                Description = description,
                QrImageUrl = qrImageUrl,
                QrQuickLink = quickLink
            };
        }

        public async Task<PaymentResponse> ConfirmCashPaymentAsync(CashPaymentRequest request, string? confirmedByAccountId = null)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
                throw new NotFoundException($"Không tìm thấy hóa đơn với ID {request.InvoiceId}.");

            if (request.AmountPaid <= 0)
                throw new BadRequestException("Số tiền thanh toán phải lớn hơn 0.");

            if (request.AmountPaid > invoice.RemainingAmount)
                throw new BadRequestException($"Số tiền thanh toán ({request.AmountPaid:N0} đ) vượt quá số nợ còn lại ({invoice.RemainingAmount:N0} đ).");

            // 1. Ghi nhận thanh toán vào Hóa đơn
            invoice.RecordPayment(request.AmountPaid, "Tiền mặt", request.Note);

            // 2. Tạo bản ghi giao dịch Payment
            string paymentCode = $"PAY-CASH-{DateTime.UtcNow:yyyyMMddHHmmss}-{invoice.InvoiceId}";
            var payment = new Payment(
                paymentCode: paymentCode,
                accountId: invoice.AccountId,
                amount: request.AmountPaid,
                method: PaymentMethod.Cash,
                type: PaymentType.Invoice,
                invoiceId: invoice.InvoiceId,
                contractId: invoice.ContractId,
                note: request.Note
            );

            payment.MarkAsSuccess(
                transactionId: null,
                confirmedByAccountId: confirmedByAccountId,
                note: "Chủ trọ xác nhận thu tiền mặt trực tiếp"
            );

            await _paymentRepository.AddAsync(payment);
            await _invoiceRepository.UpdateAsync(invoice);

            return new PaymentResponse
            {
                PaymentId = payment.PaymentId,
                PaymentCode = payment.PaymentCode,
                InvoiceId = invoice.InvoiceId,
                Amount = payment.Amount,
                Method = payment.Method,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt,
                RemainingInvoiceAmount = invoice.RemainingAmount,
                Note = payment.Note
            };
        }

        public async Task<SepayWebhookResultResponse> ProcessSepayWebhookAsync(SepayWebhookRequest request)
        {
            // 1. Kiểm tra tính Idempotency (Tránh xử lý 2 lần cùng 1 giao dịch từ ngân hàng)
            if (!string.IsNullOrWhiteSpace(request.ReferenceCode))
            {
                var existingPayment = await _paymentRepository.GetPaymentByTransactionIdAsync(request.ReferenceCode);
                if (existingPayment != null)
                {
                    return new SepayWebhookResultResponse
                    {
                        Success = true,
                        Message = "Giao dịch đã được ghi nhận trước đó (Idempotent bypass).",
                        InvoiceId = existingPayment.InvoiceId
                    };
                }
            }

            // 2. Tìm mã hóa đơn trong nội dung chuyển khoản (Content)
            // Ví dụ nội dung: "PHONG101 T11 HDT-202611-P101" hoặc "HDT-202611-P101"
            string content = request.Content ?? request.Description ?? string.Empty;
            var match = Regex.Match(content, @"(HDT-\d{6}-P[A-Za-z0-9]+)", RegexOptions.IgnoreCase);

            Invoice? invoice = null;
            if (match.Success)
            {
                string matchedCode = match.Groups[1].Value.Trim();
                invoice = await _invoiceRepository.GetInvoiceByCodeAsync(matchedCode);
            }

            if (invoice == null)
            {
                return new SepayWebhookResultResponse
                {
                    Success = false,
                    Message = $"Không tìm thấy hóa đơn khớp với nội dung chuyển khoản: '{content}'."
                };
            }

            // 3. Khớp số tiền và tự động gạch nợ
            decimal amountToPay = Math.Min(request.TransferAmount, invoice.RemainingAmount);
            if (amountToPay > 0)
            {
                invoice.RecordPayment(amountToPay, "VietQR", $"SePay Webhook Ref: {request.ReferenceCode}");
            }

            // 4. Tạo Payment lưu lịch sử
            string paymentCode = $"PAY-QR-{DateTime.UtcNow:yyyyMMddHHmmss}-{invoice.InvoiceId}";
            var payment = new Payment(
                paymentCode: paymentCode,
                accountId: invoice.AccountId,
                amount: request.TransferAmount,
                method: PaymentMethod.BankTransfer,
                type: PaymentType.Invoice,
                invoiceId: invoice.InvoiceId,
                contractId: invoice.ContractId,
                note: $"SePay Gateway: {request.Gateway}. Nội dung: {request.Content}"
            );

            payment.MarkAsSuccess(
                transactionId: request.ReferenceCode ?? request.Id.ToString(),
                confirmedByAccountId: null,
                note: "Xác nhận tự động qua Webhook SePay"
            );

            await _paymentRepository.AddAsync(payment);
            await _invoiceRepository.UpdateAsync(invoice);

            return new SepayWebhookResultResponse
            {
                Success = true,
                Message = "Tự động gạch nợ hóa đơn thành công.",
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode
            };
        }
    }
}
