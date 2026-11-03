import React, { useState, useEffect, useRef } from 'react';
import { ShoppingBag, Hospital, GraduationCap, Navigation, MapPin, Sparkles } from 'lucide-react';
import L from 'leaflet';

interface Landmark {
  name: string;
  dist: string;
  time: string;
  tag: string;
  desc: string;
  coords: [number, number];
}

const neighborhoodData: Record<string, Landmark[]> = {
  markets: [
    {
      name: 'Chợ Dân Sinh Linh Trung',
      dist: '150 mét',
      time: '2 phút đi bộ',
      tag: 'Rau củ tươi • Đồ ăn sáng phong phú',
      desc: 'Họp chợ từ 5h sáng đến 19h tối, đồ tươi sống giá bình dân dành cho sinh viên và cư dân.',
      coords: [10.8710, 106.7795],
    },
    {
      name: 'Siêu Thị WinMart+ / Bách Hóa Xanh',
      dist: '300 mét',
      time: '3 phút đi bộ',
      tag: 'Mở cửa 06:00 - 22:00',
      desc: 'Đầy đủ thực phẩm sạch đóng gói, đồ gia dụng, thanh toán ví điện tử nhanh chóng.',
      coords: [10.8685, 106.7770],
    },
    {
      name: 'TTTM Vincom Plaza & CGV Cinema',
      dist: '1.2 km',
      time: '4 phút xe máy',
      tag: 'Rạp chiếu phim CGV • Mua sắm',
      desc: 'Khu phức hợp ẩm thực, thời trang và rạp chiếu phim giải trí cuối tuần.',
      coords: [10.8520, 106.7710],
    },
  ],
  hospitals: [
    {
      name: 'Bệnh Viện Đa Khoa Khu Vực',
      dist: '850 mét',
      time: '3 phút xe máy',
      tag: 'Cấp cứu 24/24 • BHYT',
      desc: 'Bệnh viện hạng 1 đầy đủ các chuyên khoa, trang thiết bị hiện đại, an tâm sức khỏe.',
      coords: [10.8655, 106.7745],
    },
    {
      name: 'Trạm Y Tế & Nhà Thuốc Long Châu 24/7',
      dist: '120 mét',
      time: '1 phút đi bộ',
      tag: 'Nhà thuốc ngay đầu ngõ',
      desc: 'Tiện lợi mua thuốc cảm cúm, sơ cứu y tế thông thường mọi khung giờ trong ngày.',
      coords: [10.8702, 106.7788],
    },
  ],
  universities: [
    {
      name: 'Trường ĐH Bách Khoa / SP Kỹ Thuật',
      dist: '1.3 km',
      time: '5 phút xe máy',
      tag: 'Tuyến đường thẳng không kẹt xe',
      desc: 'Rất thuận tiện cho các bạn sinh viên và giảng viên đi học, nghiên cứu.',
      coords: [10.8780, 106.7820],
    },
    {
      name: 'Trường ĐH Kinh Tế - Luật / ĐHQG TP.HCM',
      dist: '2.1 km',
      time: '7 phút xe máy',
      tag: 'Xe buýt tuyến 08, 10, 19 đón tận nơi',
      desc: 'Kết nối nhanh chóng bằng tuyến xe buýt sinh viên ngay đầu ngõ.',
      coords: [10.8840, 106.7800],
    },
  ],
  transports: [
    {
      name: 'Mặt Tiền Đường Huyết Mạch (Đường 30m)',
      dist: '50 mét',
      time: '1 phút đi bộ',
      tag: 'Hẻm bê tông 6m ô tô quay đầu',
      desc: 'Nhà trọ nằm cách mặt tiền đường lớn chỉ 50m, vừa yên tĩnh vừa thuận tiện bắt xe.',
      coords: [10.8700, 106.7782],
    },
    {
      name: 'Trạm Xe Buýt & Tuyến Metro Số 1',
      dist: '250 mét',
      time: '3 phút đi bộ',
      tag: 'Trạm đón xe buýt 06, 53, 56',
      desc: 'Dễ dàng di chuyển vào trung tâm Quận 1 hoặc Khu Công Nghệ Cao.',
      coords: [10.8690, 106.7805],
    },
  ],
};

export const LocationSection: React.FC = () => {
  const [activeCategory, setActiveCategory] = useState<'markets' | 'hospitals' | 'universities' | 'transports'>('markets');
  const mapContainerRef = useRef<HTMLDivElement>(null);
  const mapInstanceRef = useRef<L.Map | null>(null);

  // Boarding house coordinates
  const houseCoords: [number, number] = [10.8698, 106.7785];

  useEffect(() => {
    if (!mapContainerRef.current) return;
    if (mapInstanceRef.current) return; // Map already initialized

    const map = L.map(mapContainerRef.current, {
      center: houseCoords,
      zoom: 15,
      scrollWheelZoom: false,
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
    }).addTo(map);

    // House custom pin
    const houseIcon = L.divIcon({
      className: 'custom-house-pin',
      html: `
        <div style="background-color: #059669; width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; border: 3px solid white; box-shadow: 0 4px 12px rgba(0,0,0,0.3); font-size: 18px;">
          🏠
        </div>
      `,
      iconSize: [40, 40],
      iconAnchor: [20, 20],
    });

    L.marker(houseCoords, { icon: houseIcon })
      .addTo(map)
      .bindPopup(
        `<div style="font-family: sans-serif; font-size: 12px; padding: 4px;">
          <strong style="color: #059669; font-size: 13px;">GREEN HOUSE LIVING</strong><br/>
          <span>Nhà trọ cao cấp chuẩn xanh</span><br/>
          <small style="color: #64748b;">Hẻm 6m ô tô quay đầu</small>
        </div>`
      )
      .openPopup();

    // Nearby pins
    Object.values(neighborhoodData).flat().forEach((place) => {
      L.marker(place.coords)
        .addTo(map)
        .bindPopup(`<b>${place.name}</b><br/><span style="color:#059669">${place.dist} (${place.time})</span>`);
    });

    mapInstanceRef.current = map;

    return () => {
      map.remove();
      mapInstanceRef.current = null;
    };
  }, []);

  const items = neighborhoodData[activeCategory] || [];

  return (
    <section id="vi-tri-ket-noi" className="py-20 bg-slate-900 text-white relative overflow-hidden">
      {/* Background Grid Pattern */}
      <div className="absolute inset-0 opacity-10 bg-[radial-gradient(#10b981_1px,transparent_1px)] [background-size:16px_16px]"></div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
        
        {/* Section Header */}
        <div className="text-center max-w-3xl mx-auto space-y-3 mb-14">
          <span className="text-xs font-bold text-emerald-400 uppercase tracking-widest bg-emerald-950/80 px-3.5 py-1.5 rounded-full border border-emerald-800">
            Bản Đồ Kết Nối & Tiện Ích Ngoại Khu
          </span>
          <h2 className="text-3xl sm:text-4xl font-extrabold text-white">
            Vị Trí Vàng Trung Tâm – Vạn Tiện Ích Xung Quanh
          </h2>
          <p className="text-slate-300 text-sm sm:text-base">
            Tọa lạc tại khu vực dân trí cao, hẻm xe hơi thông thoáng, di chuyển nhanh chóng đến các bệnh viện tuyến đầu, trường đại học lớn và chợ truyền thống.
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
          
          {/* Left: Category Tabs & Nearby Hubs */}
          <div className="lg:col-span-5 space-y-5">
            
            {/* Category Selector Tabs */}
            <div className="grid grid-cols-2 gap-2 bg-slate-800/80 p-1.5 rounded-2xl border border-slate-700">
              <button
                onClick={() => setActiveCategory('markets')}
                className={`py-2.5 px-3 rounded-xl text-xs font-bold flex items-center justify-center gap-1.5 transition-all ${
                  activeCategory === 'markets'
                    ? 'bg-emerald-600 text-white shadow-sm'
                    : 'text-slate-300 hover:text-white hover:bg-slate-700'
                }`}
              >
                <ShoppingBag className="w-3.5 h-3.5 text-emerald-300" />
                Chợ & Siêu Thị
              </button>
              <button
                onClick={() => setActiveCategory('hospitals')}
                className={`py-2.5 px-3 rounded-xl text-xs font-bold flex items-center justify-center gap-1.5 transition-all ${
                  activeCategory === 'hospitals'
                    ? 'bg-emerald-600 text-white shadow-sm'
                    : 'text-slate-300 hover:text-white hover:bg-slate-700'
                }`}
              >
                <Hospital className="w-3.5 h-3.5 text-red-400" />
                Bệnh Viện & Y Tế
              </button>
              <button
                onClick={() => setActiveCategory('universities')}
                className={`py-2.5 px-3 rounded-xl text-xs font-bold flex items-center justify-center gap-1.5 transition-all ${
                  activeCategory === 'universities'
                    ? 'bg-emerald-600 text-white shadow-sm'
                    : 'text-slate-300 hover:text-white hover:bg-slate-700'
                }`}
              >
                <GraduationCap className="w-3.5 h-3.5 text-amber-400" />
                Trường Đại Học
              </button>
              <button
                onClick={() => setActiveCategory('transports')}
                className={`py-2.5 px-3 rounded-xl text-xs font-bold flex items-center justify-center gap-1.5 transition-all ${
                  activeCategory === 'transports'
                    ? 'bg-emerald-600 text-white shadow-sm'
                    : 'text-slate-300 hover:text-white hover:bg-slate-700'
                }`}
              >
                <Navigation className="w-3.5 h-3.5 text-blue-400" />
                Tuyến Đường Chính
              </button>
            </div>

            {/* List of Landmarks */}
            <div className="space-y-3">
              {items.map((item, idx) => (
                <div
                  key={idx}
                  className="p-4 rounded-2xl bg-slate-800/90 border border-slate-700/80 hover:border-emerald-500 transition-all"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h4 className="text-sm font-bold text-white">{item.name}</h4>
                      <p className="text-xs text-slate-400 mt-1">{item.desc}</p>
                    </div>
                    <div className="text-right shrink-0">
                      <span className="inline-block px-2.5 py-1 rounded-lg bg-emerald-500/20 text-emerald-400 text-xs font-extrabold border border-emerald-500/30">
                        {item.dist}
                      </span>
                      <span className="block text-[11px] text-slate-400 mt-0.5">{item.time}</span>
                    </div>
                  </div>
                  <div className="mt-2.5 pt-2 border-t border-slate-700/60 flex items-center gap-1.5 text-[11px] text-amber-400 font-semibold">
                    <Sparkles className="w-3 h-3 text-amber-400" /> {item.tag}
                  </div>
                </div>
              ))}
            </div>

            {/* Road Environment Highlight */}
            <div className="p-4 rounded-2xl bg-slate-800/60 border border-slate-700/80 text-xs text-slate-300 flex items-center gap-3">
              <div className="w-9 h-9 rounded-xl bg-emerald-500/20 text-emerald-400 flex items-center justify-center shrink-0">
                <MapPin className="w-4 h-4" />
              </div>
              <span>
                <strong>Giao thông thuận tiện:</strong> Hẻm bê tông 6m ô tô quay đầu thoải mái, cao ráo, hoàn toàn <strong>không ngập nước</strong> vào mùa mưa.
              </span>
            </div>

          </div>

          {/* Right: Leaflet Interactive Map */}
          <div className="lg:col-span-7">
            <div className="relative bg-slate-800 rounded-3xl p-3 border border-slate-700 shadow-2xl">
              <div className="flex items-center justify-between px-3 py-2 text-xs text-slate-400 border-b border-slate-700/60 mb-2">
                <div className="flex items-center gap-2">
                  <span className="w-2.5 h-2.5 rounded-full bg-emerald-500"></span>
                  <span className="text-slate-200 font-bold">Vị Trí Thực Tế Trên Bản Đồ</span>
                </div>
                <span className="text-[11px] text-slate-400">Thu phóng & kéo để xem các tiện ích</span>
              </div>

              {/* Map Container */}
              <div ref={mapContainerRef} className="w-full h-[430px] rounded-2xl z-0" />

              {/* Floating Legend */}
              <div className="absolute bottom-6 left-6 right-6 sm:right-auto bg-slate-900/90 backdrop-blur-md px-4 py-2.5 rounded-xl border border-slate-700 text-xs text-slate-200 flex flex-wrap items-center gap-4 z-10 shadow-lg">
                <div className="flex items-center gap-1.5 font-bold text-emerald-400">
                  <span className="w-3 h-3 rounded-full bg-emerald-500 inline-block border-2 border-white"></span>
                  Nhà Trọ GreenHouse
                </div>
                <div className="flex items-center gap-1.5 text-slate-300">
                  <span className="w-2.5 h-2.5 rounded-full bg-blue-500 inline-block"></span>
                  Tiện Ích Ngoại Khu
                </div>
              </div>

            </div>
          </div>

        </div>

      </div>
    </section>
  );
};
