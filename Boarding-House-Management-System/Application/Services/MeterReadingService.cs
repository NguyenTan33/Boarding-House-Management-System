using Application.Algorithms;
using Application.DTOs.MeterReadings;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IContractRepository _contractRepository;

        public MeterReadingService(
            IMeterReadingRepository meterReadingRepository,
            IRoomRepository roomRepository,
            IContractRepository contractRepository)
        {
            _meterReadingRepository = meterReadingRepository;
            _roomRepository = roomRepository;
            _contractRepository = contractRepository;
        }

        public async Task<List<MeterReadingResponse>> GetReadingsByHouseAndMonthAsync(int houseId, int month, int year)
        {
            var readings = await _meterReadingRepository.GetReadingsByHouseAndMonthAsync(houseId, month, year);

            return readings.Select(m => new MeterReadingResponse
            {
                MeterReadingId = m.MeterReadingId,
                RoomId = m.RoomId,
                RoomNumber = m.Room?.RoomNumber ?? string.Empty,
                Month = m.Month,
                Year = m.Year,
                ReadingDate = m.ReadingDate,
                OldElectricityIndex = m.OldElectricityIndex,
                NewElectricityIndex = m.NewElectricityIndex,
                ElectricityUsage = m.ElectricityUsage,
                OldWaterIndex = m.OldWaterIndex,
                NewWaterIndex = m.NewWaterIndex,
                WaterUsage = m.WaterUsage,
                IsBilled = m.IsBilled,
                Note = m.Note
            }).ToList();
        }

        public async Task<MeterReadingResponse> RecordMeterReadingAsync(
            RecordMeterReadingRequest request, 
            string? recordedByAccountId = null)
        {
            // 1. Kiểm tra phòng có tồn tại không
            var room = await _roomRepository.GetByIdAsync(request.RoomId);
            if (room == null || !room.IsActive)
                throw new NotFoundException($"Không tìm thấy phòng trọ với ID {request.RoomId}.");

            // 2. Kiểm tra tính hợp lệ cơ bản
            if (request.NewElectricity < request.OldElectricity)
                throw new BadRequestException($"Chỉ số điện mới ({request.NewElectricity}) không được nhỏ hơn số cũ ({request.OldElectricity}).");

            if (request.NewWater < request.OldWater)
                throw new BadRequestException($"Chỉ số nước mới ({request.NewWater}) không được nhỏ hơn số cũ ({request.OldWater}).");

            // 3. Kiểm tra xem tháng này phòng đã chốt số chưa
            var existing = await _meterReadingRepository.GetReadingByRoomAndMonthAsync(request.RoomId, request.Month, request.Year);
            if (existing != null)
                throw new ConflictException($"Phòng '{room.RoomNumber}' đã được ghi chỉ số điện nước cho tháng {request.Month}/{request.Year}.");

            // 4. Lấy hợp đồng đang thuê hiện tại (nếu có)
            var activeContract = await _contractRepository.GetActiveContractByRoomIdAsync(request.RoomId);

            // 5. Kiểm tra cảnh báo bất thường so với tháng trước (Anomaly Detection Algorithm)
            var previous = await _meterReadingRepository.GetPreviousReadingAsync(request.RoomId, request.Month, request.Year);
            decimal currentElectricityUsage = request.NewElectricity - request.OldElectricity;
            decimal currentWaterUsage = request.NewWater - request.OldWater;

            var anomalyResult = MeterReadingAnomalyDetector.Detect(
                currentElectricityUsage,
                previous?.ElectricityUsage,
                currentWaterUsage,
                previous?.WaterUsage
            );

            // 6. Tạo thực thể MeterReading
            var meterReading = new MeterReading(
                roomId: request.RoomId,
                month: request.Month,
                year: request.Year,
                readingDate: DateTime.UtcNow,
                oldElectricityIndex: request.OldElectricity,
                newElectricityIndex: request.NewElectricity,
                oldWaterIndex: request.OldWater,
                newWaterIndex: request.NewWater,
                contractId: activeContract?.ContractId,
                recordedByAccountId: recordedByAccountId,
                note: request.Note
            );

            await _meterReadingRepository.AddAsync(meterReading);

            return new MeterReadingResponse
            {
                MeterReadingId = meterReading.MeterReadingId,
                RoomId = meterReading.RoomId,
                RoomNumber = room.RoomNumber,
                Month = meterReading.Month,
                Year = meterReading.Year,
                ReadingDate = meterReading.ReadingDate,
                OldElectricityIndex = meterReading.OldElectricityIndex,
                NewElectricityIndex = meterReading.NewElectricityIndex,
                ElectricityUsage = meterReading.ElectricityUsage,
                OldWaterIndex = meterReading.OldWaterIndex,
                NewWaterIndex = meterReading.NewWaterIndex,
                WaterUsage = meterReading.WaterUsage,
                IsBilled = meterReading.IsBilled,
                IsAnomaly = anomalyResult.IsAnomaly,
                AnomalyWarning = anomalyResult.WarningMessage,
                Note = meterReading.Note
            };
        }

        public async Task<BulkRecordResponse> BulkRecordMeterReadingsAsync(
            BulkMeterReadingRequest request, 
            string? recordedByAccountId = null)
        {
            var response = new BulkRecordResponse();
            var seenRoomIds = new HashSet<int>();

            foreach (var item in request.Readings)
            {
                // Kiểm tra trùng lặp phòng trong cùng mảng dữ liệu gửi lên (O(1) HashSet check)
                if (!seenRoomIds.Add(item.RoomId))
                {
                    response.FailureCount++;
                    response.Errors.Add($"Phòng ID {item.RoomId} bị lặp lại trong danh sách gửi lên.");
                    continue;
                }

                try
                {
                    var singleRequest = new RecordMeterReadingRequest
                    {
                        RoomId = item.RoomId,
                        Month = request.Month,
                        Year = request.Year,
                        OldElectricity = item.OldElectricity,
                        NewElectricity = item.NewElectricity,
                        OldWater = item.OldWater,
                        NewWater = item.NewWater,
                        Note = item.Note
                    };

                    var result = await RecordMeterReadingAsync(singleRequest, recordedByAccountId);
                    response.SuccessCount++;
                    response.RecordedReadings.Add(result);
                }
                catch (Exception ex)
                {
                    response.FailureCount++;
                    response.Errors.Add($"Phòng ID {item.RoomId}: {ex.Message}");
                }
            }

            return response;
        }

        public async Task<PreviousReadingResponse?> GetPreviousReadingAsync(int roomId, int month, int year)
        {
            var previous = await _meterReadingRepository.GetPreviousReadingAsync(roomId, month, year);
            if (previous == null)
            {
                // Nếu chưa có kỳ trước đó, thử lấy chỉ số ban đầu từ hợp đồng đang thuê
                var contract = await _contractRepository.GetActiveContractByRoomIdAsync(roomId);
                if (contract != null)
                {
                    return new PreviousReadingResponse
                    {
                        RoomId = roomId,
                        PreviousMonth = contract.StartDate.Month,
                        PreviousYear = contract.StartDate.Year,
                        LastElectricityIndex = contract.InitialElectricityIndex,
                        LastWaterIndex = contract.InitialWaterIndex
                    };
                }
                return null;
            }

            return new PreviousReadingResponse
            {
                RoomId = roomId,
                PreviousMonth = previous.Month,
                PreviousYear = previous.Year,
                LastElectricityIndex = previous.NewElectricityIndex,
                LastWaterIndex = previous.NewWaterIndex
            };
        }
    }
}
