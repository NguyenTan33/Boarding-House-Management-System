using Application.Helpers;

namespace UnitTests.Helpers
{
    public class VietQrHelperTests
    {
        [Fact]
        public void GenerateQrUrl_ShouldFormatValidNapasUrlAndDeepLink()
        {
            // Arrange
            var bankCode = "MB";
            var accountNo = "0987654321";
            var accountName = "NGUYEN VAN CHU TRO";
            var amount = 3500000m;
            var description = "THANHTOAN PHONG 101";

            // Act
            var (qrImageUrl, qrQuickLink) = VietQrHelper.GenerateQrUrl(bankCode, accountNo, accountName, amount, description);

            // Assert
            Assert.Contains("img.vietqr.io/image/MB-0987654321-compact2.png", qrImageUrl);
            Assert.Contains("amount=3500000", qrImageUrl);
            Assert.Contains("addInfo=THANHTOAN+PHONG+101", qrImageUrl);
            Assert.Contains("api.vietqr.io/MB/0987654321/3500000", qrQuickLink);
        }
    }
}
