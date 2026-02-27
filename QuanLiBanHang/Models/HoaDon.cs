using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Models
{
    public class HoaDon
    {
        public string MaHoaDon { get; set; }

        public string KhachHang { get; set; }

        public DateTime Ngay { get; set; }

        public ObservableCollection<ChiTietHoaDon> DanhSachSP
            = new ObservableCollection<ChiTietHoaDon>();

        public double TongTien
        {
            get
            {
                return DanhSachSP.Sum(x => x.ThanhTien);
            }
        }
    }
} 