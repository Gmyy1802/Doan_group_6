using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using cs464_project.DataAccess;

namespace cs464_project.ViewModel
{
    public class DashboardViewModel : BaseViewModel
    {
        private string _doanhThuHomNay;
        private string _soHoaDonHomNay;
        private string _tongSanPham;
        private string _tongNhanVien;
        private string _homNay;

        public string DoanhThuHomNay
        {
            get => _doanhThuHomNay;
            set => SetProperty(ref _doanhThuHomNay, value);
        }

        public string SoHoaDonHomNay
        {
            get => _soHoaDonHomNay;
            set => SetProperty(ref _soHoaDonHomNay, value);
        }

        public string TongSanPham
        {
            get => _tongSanPham;
            set => SetProperty(ref _tongSanPham, value);
        }

        public string TongNhanVien
        {
            get => _tongNhanVien;
            set => SetProperty(ref _tongNhanVien, value);
        }

        public string HomNay
        {
            get => _homNay;
            set => SetProperty(ref _homNay, value);
        }

        public ObservableCollection<RecentOrderItem> RecentOrders { get; }
        public ObservableCollection<TopProductItem> TopProducts { get; }
        public ObservableCollection<LowStockItem> LowStockProducts { get; }

        public DashboardViewModel()
        {
            RecentOrders = new ObservableCollection<RecentOrderItem>();
            TopProducts = new ObservableCollection<TopProductItem>();
            LowStockProducts = new ObservableCollection<LowStockItem>();

            DoanhThuHomNay = "0 đ";
            SoHoaDonHomNay = "0";
            TongSanPham = "0";
            TongNhanVien = "0";
            HomNay = DateTime.Now.ToString("dd/MM/yyyy");
        }

        public void LoadData()
        {
            try
            {
                using (var db = DbHelper.GetContext())
                {
                    DateTime today = DateTime.Today;
                    DateTime tomorrow = today.AddDays(1);

                    // Doanh thu hôm nay
                    var doanhThu = db.Orders
                        .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                        .Select(o => o.TotalAmount)
                        .DefaultIfEmpty(0)
                        .Sum();
                    DoanhThuHomNay = doanhThu.ToString("N0") + " đ";

                    // Số hóa đơn hôm nay
                    var soHD = db.Orders
                        .Count(o => o.OrderDate >= today && o.OrderDate < tomorrow);
                    SoHoaDonHomNay = soHD.ToString();

                    // Tổng sản phẩm
                    TongSanPham = db.Products.Count(p => p.IsActive).ToString();

                    // Tổng nhân viên
                    TongNhanVien = db.Employees.Count(e => e.IsWorking).ToString();

                    // Hóa đơn gần đây
                    RecentOrders.Clear();
                    var orders = db.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.Employee)
                        .OrderByDescending(o => o.OrderDate)
                        .Take(10)
                        .ToList();
                    foreach (var o in orders)
                    {
                        RecentOrders.Add(new RecentOrderItem
                        {
                            InvoiceCode = o.OrderCode,
                            InvoiceDate = o.OrderDate.ToString("dd/MM/yyyy"),
                            CustomerName = o.Customer?.FullName ?? "",
                            TotalAmount = o.TotalAmount.ToString("N0"),
                            EmployeeName = o.Employee?.FullName ?? "",
                            Status = o.Status
                        });
                    }

                    // Top sản phẩm bán chạy
                    TopProducts.Clear();
                    var topProducts = db.OrderItems
                        .Include(oi => oi.Product)
                        .GroupBy(oi => new { oi.ProductId, oi.Product.ProductName })
                        .Select(g => new
                        {
                            g.Key.ProductName,
                            Quantity = g.Sum(x => x.Quantity)
                        })
                        .OrderByDescending(x => x.Quantity)
                        .Take(5)
                        .ToList();
                    foreach (var p in topProducts)
                    {
                        TopProducts.Add(new TopProductItem
                        {
                            ProductName = p.ProductName,
                            Quantity = p.Quantity
                        });
                    }

                    // Sản phẩm sắp hết hàng
                    LowStockProducts.Clear();
                    var lowStock = db.Products
                        .Where(p => p.IsActive && p.Quantity <= 5)
                        .OrderBy(p => p.Quantity)
                        .Take(5)
                        .ToList();
                    foreach (var p in lowStock)
                    {
                        LowStockProducts.Add(new LowStockItem
                        {
                            ProductName = p.ProductName,
                            Quantity = p.Quantity
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu Dashboard: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class RecentOrderItem
    {
        public string InvoiceCode { get; set; }
        public string InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string TotalAmount { get; set; }
        public string EmployeeName { get; set; }
        public string Status { get; set; }
    }

    public class TopProductItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class LowStockItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
