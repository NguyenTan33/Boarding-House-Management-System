using System.Web;

namespace Application.Helpers
{
    public static class VietQrHelper
    {
        public static (string QrImageUrl, string QrQuickLink) GenerateQrUrl(
            string bankCode,
            string accountNo,
            string accountName,
            decimal amount,
            string description)
        {
            var encodedDesc = HttpUtility.UrlEncode(description);
            var encodedName = HttpUtility.UrlEncode(accountName);
            var cleanAmount = Math.Round(amount, 0);

            // Cấu trúc URL ảnh QR theo chuẩn VietQR NAPAS
            string qrImageUrl = $"https://img.vietqr.io/image/{bankCode}-{accountNo}-compact2.png?amount={cleanAmount}&addInfo={encodedDesc}&accountName={encodedName}";
            
            // Cấu trúc deep link mở ứng dụng ngân hàng
            string quickLink = $"https://api.vietqr.io/{bankCode}/{accountNo}/{cleanAmount}/{encodedDesc}/compact2.jpg?accountName={encodedName}";

            return (qrImageUrl, quickLink);
        }
    }
}
