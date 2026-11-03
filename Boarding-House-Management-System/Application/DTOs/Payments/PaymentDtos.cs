using Domain.Enum;

namespace Application.DTOs.Payments
{
    public class VietQrResponse
    {
        public int InvoiceId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BankAccountNo { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string QrImageUrl { get; set; } = string.Empty;
        public string QrQuickLink { get; set; } = string.Empty;
    }

    public class CashPaymentRequest
    {
        public int InvoiceId { get; set; }
        public decimal AmountPaid { get; set; }
        public string? Note { get; set; }
    }

    public class PaymentResponse
    {
        public int PaymentId { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public int? InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string MethodName => Method.ToString();
        public PaymentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal RemainingInvoiceAmount { get; set; }
        public string? Note { get; set; }
    }

    public class SepayWebhookRequest
    {
        public int Id { get; set; }
        public string? Gateway { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public string? SubAccount { get; set; }
        public decimal TransferAmount { get; set; }
        public string? TransferType { get; set; }
        public string? Content { get; set; }
        public string? ReferenceCode { get; set; }
        public string? Description { get; set; }
    }

    public class SepayWebhookResultResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? InvoiceId { get; set; }
        public string? InvoiceCode { get; set; }
    }
}
