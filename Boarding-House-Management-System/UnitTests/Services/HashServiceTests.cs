using Infrastructure.Services.Auth;

namespace UnitTests.Services
{
    public class HashServiceTests
    {
        private readonly HashService _hashService;
        private const string Secret = "MyVerySecretKeyForUnitTesting2026!";

        public HashServiceTests()
        {
            _hashService = new HashService();
        }

        [Fact]
        public void Hash_WithSameInputAndSecret_ShouldProduceIdenticalHash()
        {
            // Arrange
            var input = "my_refresh_token_string_123456";

            // Act
            var hash1 = _hashService.Hash(input, Secret);
            var hash2 = _hashService.Hash(input, Secret);

            // Assert
            Assert.NotNull(hash1);
            Assert.NotEmpty(hash1);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Hash_WithDifferentInputs_ShouldProduceDifferentHashes()
        {
            // Arrange
            var input1 = "token_one";
            var input2 = "token_two";

            // Act
            var hash1 = _hashService.Hash(input1, Secret);
            var hash2 = _hashService.Hash(input2, Secret);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Hash_WithDifferentSecrets_ShouldProduceDifferentHashes()
        {
            // Arrange
            var input = "token_value";
            var secret1 = "secret_one_123";
            var secret2 = "secret_two_456";

            // Act
            var hash1 = _hashService.Hash(input, secret1);
            var hash2 = _hashService.Hash(input, secret2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Verify_WhenHashMatches_ShouldReturnTrue()
        {
            // Arrange
            var input = "token_to_verify";
            var hash = _hashService.Hash(input, Secret);

            // Act
            var isValid = _hashService.Verify(input, hash, Secret);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void Verify_WhenInputOrSecretDiffers_ShouldReturnFalse()
        {
            // Arrange
            var input = "original_token";
            var hash = _hashService.Hash(input, Secret);

            // Act & Assert
            Assert.False(_hashService.Verify("different_token", hash, Secret));
            Assert.False(_hashService.Verify(input, hash, "wrong_secret"));
        }
    }
}
