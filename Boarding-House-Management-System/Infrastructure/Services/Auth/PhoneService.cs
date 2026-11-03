using System.Text.RegularExpressions;
using Application.Interfaces.Auth;

namespace Infrastructure.Services.Auth
{
    public class PhoneService : IPhoneService
    {
        public bool IsValid(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            var normalized = Normalize(phoneNumber);
            // Chuẩn định dạng số điện thoại Việt Nam (+84xxxxxxxxx)
            return Regex.IsMatch(normalized, @"^\+84(3|5|7|8|9)\d{8}$");
        }

        public string Normalize(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var phone = phoneNumber.Trim().Replace(" ", "").Replace(".", "").Replace("-", "");

            if (phone.StartsWith("0"))
            {
                // Chuyển 09xxxxxxxx thành +849xxxxxxxx
                phone = "+84" + phone.Substring(1);
            }
            else if (phone.StartsWith("84") && !phone.StartsWith("+84"))
            {
                phone = "+" + phone;
            }

            return phone;
        }
    }
}
