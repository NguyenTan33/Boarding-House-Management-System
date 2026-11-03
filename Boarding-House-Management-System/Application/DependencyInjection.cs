using Application.Interfaces.Services;
using Application.Services;
using Application.Strategies.Billing;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 1. Đăng ký các Strategy tính toán tiền phòng & hóa đơn
            services.AddScoped<IBillingCalculationStrategy, StandardBillingCalculationStrategy>();
            services.AddScoped<IBillingCalculationStrategy, TieredBillingCalculationStrategy>();
            services.AddScoped<IBillingStrategyFactory, BillingStrategyFactory>();

            // 2. Đăng ký các Business Services chính
            services.AddScoped<IHouseService, HouseService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IMeterReadingService, MeterReadingService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
