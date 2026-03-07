using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using cs464_project.DataAccess;

namespace cs464_project.ViewModel
{
    public class ThongKeDoanhThuViewModel : BaseViewModel
    {
        private DateTime? _tuNgay;
        private DateTime? _denNgay;
        private string _tongDoanhThu;
        private string _soHoaDon;
        private string _trungBinh;

        public DateTime? TuNgay
        {
            get => _tuNgay;
            set => SetProperty(ref _tuNgay, value);
        }

        public DateTime? DenNgay
        {
            get => _denNgay;
            set => SetProperty(ref _denNgay, value);
        }

        public string TongDoanhThu
        {
            get => _tongDoanhThu;
            set => SetProperty(ref _tongDoanhThu, value);
        }

        public string SoHoaDon
        {
            get => _soHoaDon;
            set => SetProperty(ref _soHoaDon, value);
        }

        public string TrungBinh
        {
            get => _trungBinh;
            set => SetProperty(ref _trungBinh, value);
        }

        public ObservableCollection<DoanhThuNgayItem> DoanhThuTheoNgay { get; }
        public ObservableCollection<TopSanPhamItem> TopSanPham { get; }

        public ICommand ThongKeCommand { get; }

        public ThongKeDoanhThuViewModel()
        {
            DoanhThuTheoNgay = new ObservableCollection<DoanhThuNgayItem>();
            TopSanPham = new ObservableCollection<TopSanPhamItem>();

            ThongKeCommand = new RelayCommand(ExecuteThongKe);

            TuNgay = DateTime.Now.AddMonths(-1);
            DenNgay = DateTime.Now;
            TongDoanhThu = "0 ₫";
            SoHoaDon = "0";
            TrungBinh = "0 ₫";
        }

        private void ExecuteThongKe(object parameter)
        {
            if (TuNgay == null || DenNgay == null)
            {
                MessageBox.Show("Vui lòng chọn khoảng thời gian!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime tuNgay = TuNgay.Value.Date;
            DateTime denNgay = DenNgay.Value.Date.AddDays(1);

            if (tuNgay > denNgay)
            {
                MessageBox.Show("Ngày bắt đầu phải nhỏ hơn ngày kết thúc!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = DbHelper.GetContext())
                {
                    var orders = db.Orders
                        .Where(o => o.OrderDate >= tuNgay && o.OrderDate < denNgay)
                        .ToList();

                    // Tổng doanh thu
                    decimal tongDT = orders.Sum(o => o.TotalAmount);
                    TongDoanhThu = tongDT.ToString("N0") + " ₫";

                    // Số hóa đơn
                    int soHD = orders.Count;
                    SoHoaDon = soHD.ToString();

                    // Trung bình/đơn
                    decimal tb = soHD > 0 ? tongDT / soHD : 0;
                    TrungBinh = tb.ToString("N0") + " ₫";

                    // Doanh thu theo ngày
                    DoanhThuTheoNgay.Clear();
                    var theoNgay = orders
                        .GroupBy(o => o.OrderDate.Date)
                        .Select(g => new DoanhThuNgayItem
                        {
                            Ngay = g.Key,
                            SoHD = g.Count(),
                            DoanhThu = g.Sum(o => o.TotalAmount)
                        })
                        .OrderBy(x => x.Ngay)
                        .ToList();
                    foreach (var item in theoNgay)
                    {
                        DoanhThuTheoNgay.Add(item);
                    }

                    // Top sản phẩm bán chạy
                    TopSanPham.Clear();
                    var orderIds = orders.Select(o => o.OrderId).ToList();
                    var topSP = db.OrderItems
                        .Include(oi => oi.Product)
                        .Where(oi => orderIds.Contains(oi.OrderId))
                        .GroupBy(oi => new { oi.ProductId, oi.Product.ProductName })
                        .Select(g => new
                        {
                            TenSP = g.Key.ProductName,
                            SoLuong = g.Sum(x => x.Quantity),
                            DoanhThu = g.Sum(x => x.LineTotal ?? 0)
                        })
                        .OrderByDescending(x => x.SoLuong)
                        .Take(10)
                        .ToList();
                    foreach (var sp in topSP)
                    {
                        TopSanPham.Add(new TopSanPhamItem
                        {
                            TenSP = sp.TenSP,
                            SoLuong = sp.SoLuong,
                            DoanhThu = sp.DoanhThu
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thống kê: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class DoanhThuNgayItem
    {
        public DateTime Ngay { get; set; }
        public int SoHD { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class TopSanPhamItem
    {
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DoanhThu { get; set; }
    }
}
