using Application.DTOs.Invoices;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Strategies.Billing;
using Domain.Entities;
using Domain.Enum;

namespace Application.Services
{
    public class BillingService : IBillingService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IHouseRepository _houseRepository;
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly IBillingStrategyFactory _strategyFactory;

        public BillingService(
            IInvoiceRepository invoiceRepository,
            IRoomRepository roomRepository,
            IHouseRepository houseRepository,
            IMeterReadingRepository meterReadingRepository,
            IBillingStrategyFactory strategyFactory)
        {
            _invoiceRepository = invoiceRepository;
            _roomRepository = roomRepository;
            _houseRepository = houseRepository;
            _meterReadingRepository = meterReadingRepository;
            _strategyFactory = strategyFactory;
        }

        public async Task<GenerateInvoicesResultResponse> GenerateMonthlyInvoicesAsync(GenerateMonthlyInvoicesRequest request)
        {
            var house = await _houseRepository.GetByIdAsync(request.HouseId);
            if (house == null || !house.IsActive)
                throw new NotFoundException($"Không tìm thấy dãy trọ với ID {request.HouseId}.");

            // 1. Áp dụng Strategy Pattern để tính toán chi phí (Standard hoặc Tiered theo cấu hình)
            var strategy = _strategyFactory.GetStrategy(request.StrategyType);

            // 2. Lấy danh sách các phòng ĐANG CÓ NGƯỜI Ở nhưng CHƯA CÓ HÓA ĐƠN tháng này
            var pendingRooms = await _invoiceRepository.GetRoomsPendingInvoiceAsync(request.HouseId, request.Month, request.Year);

            var response = new GenerateInvoicesResultResponse
            {
                HouseId = request.HouseId,
                Month = request.Month,
                Year = request.Year
            };

            var newInvoices = new List<Invoice>();

            foreach (var room in pendingRooms)
            {
                // Lấy hợp đồng đang hoạt động của phòng
                var activeContract = await _roomRepository.GetRoomWithActiveContractAsync(room.RoomId);
                var contract = activeContract?.Contracts.FirstOrDefault(c => c.Status == ContractStatus.Active);
                if (contract == null)
                {
                    response.SkippedRoomsInfo.Add($"Phòng {room.RoomNumber}: Bỏ qua vì không tìm thấy hợp đồng đang hiệu lực.");
                    continue;
                }

                // Lấy chỉ số điện nước tháng này của phòng (nếu đã chốt số)
                var meterReading = room.MeterReadings.FirstOrDefault(m => m.Month == request.Month && m.Year == request.Year);
                
                decimal electricityUsage = meterReading?.ElectricityUsage ?? 0;
                decimal waterUsage = meterReading?.WaterUsage ?? 0;

                decimal electricityUnitPrice = contract.ElectricityUnitPrice > 0 
                    ? contract.ElectricityUnitPrice 
                    : house.ElectricityUnitPrice;

                decimal waterUnitPrice = contract.WaterUnitPrice > 0 
                    ? contract.WaterUnitPrice 
                    : house.WaterUnitPrice;

                // Tính toán tiền điện, nước thông qua Strategy
                decimal electricityCost = strategy.CalculateElectricityCost(electricityUsage, electricityUnitPrice);
                decimal waterCost = strategy.CalculateWaterCost(waterUsage, waterUnitPrice);

                string invoiceCode = $"HDT-{request.Year}{request.Month:D2}-P{room.RoomNumber}";

                var invoice = new Invoice(
                    invoiceCode: invoiceCode,
                    contractId: contract.ContractId,
                    roomId: room.RoomId,
                    accountId: contract.AccountId,
                    month: request.Month,
                    year: request.Year,
                    issueDate: DateTime.UtcNow,
                    dueDate: request.DueDate,
                    roomPrice: contract.RentalPrice,
                    electricityUsage: electricityUsage,
                    electricityUnitPrice: electricityUnitPrice,
                    waterUsage: waterUsage,
                    waterUnitPrice: waterUnitPrice,
                    internetCost: contract.InternetUnitPrice,
                    cleaningCost: contract.CleaningUnitPrice,
                    numberOfVehicles: contract.NumberOfVehicles,
                    parkingUnitPrice: contract.ParkingUnitPrice,
                    meterReadingId: meterReading?.MeterReadingId,
                    note: $"Hóa đơn tự động tháng {request.Month}/{request.Year} ({strategy.StrategyName})"
                );

                newInvoices.Add(invoice);

                // Đánh dấu chỉ số điện nước đã được xuất hóa đơn (tránh sửa số)
                if (meterReading != null)
                {
                    meterReading.MarkAsBilled();
                    await _meterReadingRepository.UpdateAsync(meterReading);
                }
            }

            foreach (var invoice in newInvoices)
            {
                await _invoiceRepository.AddAsync(invoice);
                response.TotalInvoicesCreated++;
                response.TotalAmountBilled += invoice.TotalAmount;
            }

            return response;
        }

        public async Task<List<InvoiceResponse>> GetInvoicesAsync(InvoiceFilterRequest filter)
        {
            var invoices = await _invoiceRepository.GetAsync(i =>
                (!filter.HouseId.HasValue || (i.Room != null && i.Room.HouseId == filter.HouseId.Value)) &&
                (!filter.Month.HasValue || i.Month == filter.Month.Value) &&
                (!filter.Year.HasValue || i.Year == filter.Year.Value) &&
                (!filter.Status.HasValue || i.Status == filter.Status.Value) &&
                (!filter.RoomId.HasValue || i.RoomId == filter.RoomId.Value) &&
                (string.IsNullOrEmpty(filter.TenantId) || i.AccountId == filter.TenantId)
            );

            return invoices
                .OrderByDescending(i => i.IssueDate)
                .Select(i => new InvoiceResponse
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceCode = i.InvoiceCode,
                    RoomId = i.RoomId,
                    RoomNumber = i.Room?.RoomNumber ?? string.Empty,
                    TenantName = i.Account?.FullName ?? string.Empty,
                    Month = i.Month,
                    Year = i.Year,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    RoomPrice = i.RoomPrice,
                    ElectricityCost = i.ElectricityCost,
                    WaterCost = i.WaterCost,
                    ServiceCost = i.InternetCost + i.CleaningCost + i.ParkingCost + i.OtherFees,
                    TotalAmount = i.TotalAmount,
                    PaidAmount = i.PaidAmount,
                    RemainingAmount = i.RemainingAmount,
                    Status = i.Status
                })
                .ToList();
        }

        public async Task<InvoiceDetailResponse> GetInvoiceByIdAsync(int id)
        {
            var invoice = await _invoiceRepository.GetInvoiceDetailsAsync(id);
            if (invoice == null)
                throw new NotFoundException($"Không tìm thấy hóa đơn với ID {id}.");

            return new InvoiceDetailResponse
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode,
                ContractId = invoice.ContractId,
                ContractNumber = invoice.Contract?.ContractNumber ?? string.Empty,
                RoomId = invoice.RoomId,
                RoomNumber = invoice.Room?.RoomNumber ?? string.Empty,
                HouseName = invoice.Room?.House?.HouseName ?? string.Empty,
                HouseAddress = invoice.Room?.House?.Address ?? string.Empty,
                TenantName = invoice.Account?.FullName ?? string.Empty,
                Month = invoice.Month,
                Year = invoice.Year,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                RoomPrice = invoice.RoomPrice,
                ElectricityUsage = invoice.ElectricityUsage,
                ElectricityUnitPrice = invoice.ElectricityUnitPrice,
                ElectricityCost = invoice.ElectricityCost,
                WaterUsage = invoice.WaterUsage,
                WaterUnitPrice = invoice.WaterUnitPrice,
                WaterCost = invoice.WaterCost,
                InternetCost = invoice.InternetCost,
                CleaningCost = invoice.CleaningCost,
                ParkingCost = invoice.ParkingCost,
                OtherFees = invoice.OtherFees,
                OtherFeesDescription = invoice.OtherFeesDescription,
                DiscountAmount = invoice.DiscountAmount,
                DiscountReason = invoice.DiscountReason,
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount,
                Status = invoice.Status,
                Payments = invoice.Payments.Select(p => new PaymentHistoryDto
                {
                    PaymentId = p.PaymentId,
                    PaymentCode = p.PaymentCode,
                    Amount = p.Amount,
                    Method = p.Method.ToString(),
                    PaidAt = p.PaidAt,
                    Note = p.Note
                }).ToList()
            };
        }

        public async Task CancelInvoiceAsync(int id, string? reason = null)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null)
                throw new NotFoundException($"Không tìm thấy hóa đơn với ID {id}.");

            if (invoice.PaidAmount > 0)
                throw new BadRequestException("Hóa đơn đã có khoản thanh toán, không thể hủy. Cần hoàn tiền hoặc đối soát lại trước.");

            // Hủy hóa đơn
            invoice.Cancel(reason ?? "Hủy hóa đơn để chỉnh sửa hoặc tính lại số liệu");

            // Mở khóa cho chỉ số điện nước nếu có
            if (invoice.MeterReadingId.HasValue)
            {
                var meterReading = await _meterReadingRepository.GetByIdAsync(invoice.MeterReadingId.Value);
                if (meterReading != null)
                {
                    meterReading.UnmarkAsBilled();
                    await _meterReadingRepository.UpdateAsync(meterReading);
                }
            }

            await _invoiceRepository.UpdateAsync(invoice);
        }
    }
}
