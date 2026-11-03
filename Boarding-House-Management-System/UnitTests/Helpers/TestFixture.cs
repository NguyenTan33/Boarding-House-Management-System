using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace UnitTests.Helpers
{
    public static class TestFixture
    {
        public static (AppDbContext context, UserManager<Account> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration) CreateIdentityDependencies(string? dbName = null)
        {
            var db = dbName ?? Guid.NewGuid().ToString();
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(db));

            services.AddIdentity<Account, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            var configValues = new Dictionary<string, string?>
            {
                { "Jwt:Key", "BoardingHouseSecretKeyForJwtAuthenticationSuperSecure2026!" },
                { "Jwt:Issuer", "BoardingHouseAPI" },
                { "Jwt:Audience", "BoardingHouseClient" },
                { "Jwt:ExpireMinutes", "60" },
                { "RefreshToken:Secret", "BoardingHouseRefreshTokenSecretKeyForHashing2026Secure!" },
                { "RefreshToken:ExpireDays", "7" }
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
            services.AddSingleton<IConfiguration>(configuration);

            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<AppDbContext>();
            var userManager = provider.GetRequiredService<UserManager<Account>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            return (context, userManager, roleManager, configuration);
        }

        public static IConfiguration CreateConfiguration(Dictionary<string, string?>? customValues = null)
        {
            var values = new Dictionary<string, string?>
            {
                { "Jwt:Key", "BoardingHouseSecretKeyForJwtAuthenticationSuperSecure2026!" },
                { "Jwt:Issuer", "BoardingHouseAPI" },
                { "Jwt:Audience", "BoardingHouseClient" },
                { "Jwt:ExpireMinutes", "60" },
                { "RefreshToken:Secret", "BoardingHouseRefreshTokenSecretKeyForHashing2026Secure!" },
                { "RefreshToken:ExpireDays", "7" }
            };

            if (customValues != null)
            {
                foreach (var kv in customValues)
                {
                    values[kv.Key] = kv.Value;
                }
            }

            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        public static Mock<UserManager<Account>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<Account>>();
            return new Mock<UserManager<Account>>(
                store.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<Account>>(),
                Array.Empty<IUserValidator<Account>>(),
                Array.Empty<IPasswordValidator<Account>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<Account>>>());
        }

        public static Mock<RoleManager<IdentityRole>> CreateMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object,
                Array.Empty<IRoleValidator<IdentityRole>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<ILogger<RoleManager<IdentityRole>>>());
        }
    }
}
