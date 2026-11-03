import React from 'react';
import { Fingerprint, ShieldAlert, Clock, Zap, Wifi, Wrench } from 'lucide-react';

export const AmenitiesSection: React.FC = () => {
  const pillars = [
    {
      icon: <Fingerprint className="w-6 h-6 text-emerald-600" />,
      bg: 'bg-emerald-100',
      title: 'An Ninh Vân Tay & Camera AI',
      desc: 'Cửa ra vào nhận diện vân tay/thẻ từ chống sao chép. Camera độ nét cao 24/7 giám sát khu vực nhà xe, hành lang và cổng ra vào.',
    },
    {
      icon: <ShieldAlert className="w-6 h-6 text-red-600" />,
      bg: 'bg-red-100',
      title: 'PCCC Đạt Chuẩn Nghiệm Thu',
      desc: 'Đầy đủ chuông báo khói tự động từng phòng, bình chữa cháy, vòi nước cứu hỏa và thang thoát hiểm ngoài trời đạt kiểm định an toàn PCCC.',
    },
    {
      icon: <Clock className="w-6 h-6 text-amber-600" />,
      bg: 'bg-amber-100',
      title: 'Giờ Giấc Tự Do 100%',
      desc: 'Không chung chủ, tự do đi lại làm việc ca đêm hay về khuya. Mỗi cư dân sở hữu mã mở cửa riêng biệt, lịch sự và văn minh.',
    },
    {
      icon: <Zap className="w-6 h-6 text-teal-600" />,
      bg: 'bg-teal-100',
      title: 'Hầm Xe & Trạm Sạc Xe Điện',
      desc: 'Khu để xe máy rộng rãi có phân ô rõ ràng, trang bị ổ cắm sạc xe điện an toàn có cảm biến tự ngắt khi đầy bình phòng chống chập cháy.',
    },
    {
      icon: <Wifi className="w-6 h-6 text-blue-600" />,
      bg: 'bg-blue-100',
      title: 'Wifi 6 Tốc Độ Cao Riêng Biệt',
      desc: 'Mỗi tầng trang bị Access Point công nghiệp chuẩn Wifi 6, đường truyền cáp quang băng thông cao đáp ứng mượt mà học tập và làm việc online.',
    },
    {
      icon: <Wrench className="w-6 h-6 text-purple-600" />,
      bg: 'bg-purple-100',
      title: 'Kỹ Thuật Hỗ Trợ Nhanh 24/7',
      desc: 'Ban quản lý và thợ kỹ thuật túc trực. Mọi sự cố bóng đèn, vòi nước, máy lạnh được tiếp nhận qua hệ thống và xử lý dứt điểm trong ngày.',
    },
  ];

  return (
    <section id="tien-ich" className="py-20 bg-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        
        {/* Section Header */}
        <div className="text-center max-w-3xl mx-auto space-y-3 mb-16">
          <span className="text-xs font-bold text-emerald-700 uppercase tracking-widest bg-emerald-50 px-3.5 py-1.5 rounded-full border border-emerald-200">
            Chất Lượng Sống Chuẩn Mực
          </span>
          <h2 className="text-3xl font-extrabold text-slate-900">
            6 Trụ Cột An Ninh & Tiện Ích Nội Khu
          </h2>
          <p className="text-slate-600 text-sm sm:text-base">
            Môi trường sống văn minh, yên tĩnh được quản lý chuyên nghiệp nhằm đảm bảo sự thoải mái và an tâm tuyệt đối cho bạn.
          </p>
        </div>

        {/* Pillars Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {pillars.map((item, idx) => (
            <div
              key={idx}
              className="p-7 rounded-3xl bg-slate-50 border border-slate-200/80 hover:bg-white hover:shadow-xl hover:border-emerald-200 transition-all duration-300 group"
            >
              <div className={`w-14 h-14 rounded-2xl ${item.bg} flex items-center justify-center mb-5 group-hover:scale-110 transition-transform`}>
                {item.icon}
              </div>
              <h3 className="text-lg font-bold text-slate-900 mb-2 group-hover:text-emerald-700 transition-colors">
                {item.title}
              </h3>
              <p className="text-slate-600 text-xs sm:text-sm leading-relaxed">
                {item.desc}
              </p>
            </div>
          ))}
        </div>

      </div>
    </section>
  );
};
