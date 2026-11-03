using Infrastructure.Services.Auth;

namespace UnitTests.Services
{
    public class PhoneServiceTests
    {
        private readonly PhoneService _phoneService;

        public PhoneServiceTests()
        {
            _phoneService = new PhoneService();
        }

        [Theory]
        [InlineData("0912345678", "+84912345678")]
        [InlineData("0381234567", "+84381234567")]
        [InlineData("84912345678", "+84912345678")]
        [InlineData("+84912345678", "+84912345678")]
        [InlineData(" 0912 345 678 ", "+84912345678")]
        [InlineData("0912.345.678", "+84912345678")]
        [InlineData("0912-345-678", "+84912345678")]
        public void Normalize_ValidPhoneFormats_ShouldReturnStandardE164Format(string input, string expected)
        {
            // Act
            var result = _phoneService.Normalize(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Normalize_NullOrWhitespace_ShouldReturnEmptyString(string? input)
        {
            // Act
            var result = _phoneService.Normalize(input!);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData("0912345678")] // Viettel/Mobi/Vina 09x
        [InlineData("0862345678")] // 08x
        [InlineData("0792345678")] // 07x
        [InlineData("0582345678")] // 05x
        [InlineData("0392345678")] // 03x
        [InlineData("+84912345678")]
        [InlineData("84912345678")]
        [InlineData("0912 345 678")]
        [InlineData("0912.345.678")]
        public void IsValid_ValidVietnamesePhoneNumbers_ShouldReturnTrue(string phoneNumber)
        {
            // Act
            var result = _phoneService.IsValid(phoneNumber);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("091234567")]      // Quá ngắn (9 số)
        [InlineData("091234567890")]    // Quá dài (11 số)
        [InlineData("0243456789")]      // Đầu số cố định 02x
        [InlineData("0412345678")]      // Đầu số không tồn tại 04x
        [InlineData("0612345678")]      // Đầu số 06x
        [InlineData("abcdefghij")]      // Chữ
        [InlineData("091234567a")]      // Chứa ký tự
        public void IsValid_InvalidPhoneNumbers_ShouldReturnFalse(string? phoneNumber)
        {
            // Act
            var result = _phoneService.IsValid(phoneNumber!);

            // Assert
            Assert.False(result);
        }
    }
}
