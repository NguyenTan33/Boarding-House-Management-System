using Domain.Entities;
using Domain.Exceptions;

namespace UnitTests.Domain
{
    public class AccountEntityTests
    {
        [Fact]
        public void Constructor_WithValidArguments_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var userName = "0912345678";
            var email = "tenant@test.com";
            var fullName = "Nguyen Van A";
            var citizenId = "123456789012";
            var phoneNumber = "0912345678";
            var dob = new DateTime(2000, 1, 1);
            var address = "123 Le Loi, TP.HCM";

            // Act
            var account = new Account(userName, email, fullName, citizenId, phoneNumber, dob, address);

            // Assert
            Assert.Equal(userName, account.UserName);
            Assert.Equal(email, account.Email);
            Assert.Equal(fullName, account.FullName);
            Assert.Equal(citizenId, account.CitizenId);
            Assert.Equal(phoneNumber, account.PhoneNumber);
            Assert.Equal(dob, account.DateOfBirth);
            Assert.Equal(address, account.Address);
            Assert.True(account.IsActive);
            Assert.True(account.CreatedAt <= DateTime.UtcNow);
            Assert.Null(account.UpdatedAt);
            Assert.NotNull(account.Contracts);
            Assert.NotNull(account.Invoices);
            Assert.NotNull(account.Payments);
            Assert.NotNull(account.RefreshTokens);
            Assert.NotNull(account.Houses);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithEmptyFullName_ShouldThrowValidationException(string? invalidFullName)
        {
            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() =>
                new Account("user1", "email@test.com", invalidFullName!, "123456789012"));

            Assert.Equal("Họ và tên không được để trống.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithEmptyCitizenId_ShouldThrowValidationException(string? invalidCitizenId)
        {
            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() =>
                new Account("user1", "email@test.com", "Nguyen Van A", invalidCitizenId!));

            Assert.Equal("Số CCCD/CMND không được để trống.", ex.Message);
        }

        [Fact]
        public void UpdateProfile_WithValidData_ShouldUpdatePropertiesAndSetUpdatedAt()
        {
            // Arrange
            var account = new Account("0912345678", "a@test.com", "Old Name", "123456789");
            var newFullName = "New Name";
            var newCitizenId = "987654321";
            var newPhone = "0987654321";
            var newDob = new DateTime(1995, 5, 20);
            var newAddress = "456 Tran Phu";
            var newGender = "Nam";
            var newAvatar = "https://image.com/avatar.png";

            // Act
            account.UpdateProfile(newFullName, newCitizenId, newPhone, newDob, newAddress, newGender, newAvatar);

            // Assert
            Assert.Equal(newFullName, account.FullName);
            Assert.Equal(newCitizenId, account.CitizenId);
            Assert.Equal(newPhone, account.PhoneNumber);
            Assert.Equal(newDob, account.DateOfBirth);
            Assert.Equal(newAddress, account.Address);
            Assert.Equal(newGender, account.Gender);
            Assert.Equal(newAvatar, account.AvatarUrl);
            Assert.NotNull(account.UpdatedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateProfile_WithEmptyFullName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var account = new Account("0912345678", "a@test.com", "Nguyen A", "123456789");

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() =>
                account.UpdateProfile(invalidName, "123456789", "0912345678", null, null));

            Assert.Equal("Họ và tên không được để trống.", ex.Message);
        }

        [Fact]
        public void ActivateAndDeactivate_ShouldUpdateIsActiveAndSetUpdatedAt()
        {
            // Arrange
            var account = new Account("0912345678", "a@test.com", "Nguyen A", "123456789");
            Assert.True(account.IsActive);

            // Act - Deactivate
            account.Deactivate();

            // Assert
            Assert.False(account.IsActive);
            Assert.NotNull(account.UpdatedAt);

            // Act - Activate
            account.Activate();

            // Assert
            Assert.True(account.IsActive);
        }

        [Fact]
        public void RefreshToken_Defaults_ShouldBeProperlyInitialized()
        {
            // Arrange & Act
            var token = new RefreshToken
            {
                AccountId = "acc-123",
                TokenHash = "hash123",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            // Assert
            Assert.Equal("acc-123", token.AccountId);
            Assert.Equal("hash123", token.TokenHash);
            Assert.False(token.IsRevoked);
            Assert.Null(token.RevokedAt);
            Assert.True(token.CreatedAt <= DateTime.UtcNow);
        }
    }
}
