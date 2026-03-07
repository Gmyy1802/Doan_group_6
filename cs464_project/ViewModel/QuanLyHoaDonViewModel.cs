using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using cs464_project.DataAccess;
using cs464_project.Model;

namespace cs464_project.ViewModel
{
    public class QuanLyHoaDonViewModel : BaseViewModel
    {
        private string _timKiem;
        private DateTime? _tuNgay;
        private DateTime? _denNgay;
        private Customer _selectedCustomer;
        private Employee _selectedEmployee;
        private Product _selectedProduct;
        private int _soLuong;
        private string _ghiChu;
        private OrderDisplayItem _selectedOrder;
        private decimal _tongCong;

        public string TimKiem
        {
            get => _timKiem;
            set => SetProperty(ref _timKiem, value);
        }

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

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set => SetProperty(ref _selectedEmployee, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public int SoLuong
        {
            get => _soLuong;
            set => SetProperty(ref _soLuong, value);
        }

        public string GhiChu
        {
            get => _ghiChu;
            set => SetProperty(ref _ghiChu, value);
        }

        public OrderDisplayItem SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value) && value != null)
                {
                    LoadOrderDetails(value.OrderId);
                }
            }
        }

        public decimal TongCong
        {
            get => _tongCong;
            set => SetProperty(ref _tongCong, value);
        }

        public ObservableCollection<OrderDisplayItem> Orders { get; }
        public ObservableCollection<Customer> Customers { get; }
        public ObservableCollection<Employee> Employees { get; }
        public ObservableCollection<Product> Products { get; }
        public ObservableCollection<OrderItemTemp> OrderItems { get; }

        public ICommand SearchCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }

        public QuanLyHoaDonViewModel()
        {
            Orders = new ObservableCollection<OrderDisplayItem>();
            Customers = new ObservableCollection<Customer>();
            Employees = new ObservableCollection<Employee>();
            Products = new ObservableCollection<Product>();
            OrderItems = new ObservableCollection<OrderItemTemp>();

            SearchCommand = new RelayCommand(ExecuteSearch);
            AddProductCommand = new RelayCommand(ExecuteAddProduct);
            CreateOrderCommand = new RelayCommand(ExecuteCreateOrder);
            CancelOrderCommand = new RelayCommand(ExecuteCancel);

            SoLuong = 1;
            TuNgay = DateTime.Now.AddMonths(-1);
            DenNgay = DateTime.Now;
        }

        public void LoadData()
        {
            LoadOrders();
            LoadCustomers();
            LoadEmployees();
            LoadProducts();
        }

        private void LoadOrders()
        {
            try
            {
                Orders.Clear();
                using (var db = DbHelper.GetContext())
                {
                    var query = db.Orders.Include(o => o.Customer).AsQueryable();

                    if (TuNgay.HasValue)
                        query = query.Where(o => o.OrderDate >= TuNgay.Value);
                    if (DenNgay.HasValue)
                    {
                        var endDate = DenNgay.Value.Date.AddDays(1);
                        query = query.Where(o => o.OrderDate < endDate);
                    }

                    var orders = query.OrderByDescending(o => o.OrderDate).ToList();
                    foreach (var order in orders)
                    {
                        Orders.Add(new OrderDisplayItem
                        {
                            OrderId = order.OrderId,
                            OrderCode = order.OrderCode,
                            OrderDate = order.OrderDate,
                            CustomerName = order.Customer?.FullName ?? "",
                            TotalAmount = order.TotalAmount,
                            Status = order.Status
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCustomers()
        {
            try
            {
                Customers.Clear();
                using (var db = DbHelper.GetContext())
                {
                    var customers = db.Customers.ToList();
                    foreach (var customer in customers)
                    {
                        Customers.Add(customer);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải khách hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEmployees()
        {
            try
            {
                Employees.Clear();
                using (var db = DbHelper.GetContext())
                {
                    var employees = db.Employees.Where(e => e.IsWorking).ToList();
                    foreach (var employee in employees)
                    {
                        Employees.Add(employee);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                Products.Clear();
                using (var db = DbHelper.GetContext())
                {
                    var products = db.Products.Where(p => p.IsActive && p.Quantity > 0).ToList();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải sản phẩm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrderDetails(int orderId)
        {
            try
            {
                using (var db = DbHelper.GetContext())
                {
                    var order = db.Orders
                        .Include(o => o.OrderItems.Select(oi => oi.Product))
                        .FirstOrDefault(o => o.OrderId == orderId);

                    if (order != null)
                    {
                        GhiChu = order.Note;
                        OrderItems.Clear();
                        
                        foreach (var item in order.OrderItems)
                        {
                            OrderItems.Add(new OrderItemTemp
                            {
                                ProductId = item.ProductId,
                                ProductName = item.Product.ProductName,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                LineTotal = item.LineTotal ?? 0
                            });
                        }
                        UpdateTotal();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSearch(object parameter)
        {
            try
            {
                Orders.Clear();
                using (var db = DbHelper.GetContext())
                {
                    string keyword = (TimKiem ?? "").Trim().ToLower();
                    var query = db.Orders.Include(o => o.Customer).AsQueryable();

                    if (TuNgay.HasValue)
                        query = query.Where(o => o.OrderDate >= TuNgay.Value);
                    if (DenNgay.HasValue)
                    {
                        var endDate = DenNgay.Value.Date.AddDays(1);
                        query = query.Where(o => o.OrderDate < endDate);
                    }

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        query = query.Where(o =>
                            o.OrderCode.ToLower().Contains(keyword) ||
                            o.Customer.FullName.ToLower().Contains(keyword));
                    }

                    var orders = query.OrderByDescending(o => o.OrderDate).ToList();
                    foreach (var order in orders)
                    {
                        Orders.Add(new OrderDisplayItem
                        {
                            OrderId = order.OrderId,
                            OrderCode = order.OrderCode,
                            OrderDate = order.OrderDate,
                            CustomerName = order.Customer?.FullName ?? "",
                            TotalAmount = order.TotalAmount,
                            Status = order.Status
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddProduct(object parameter)
        {
            try
            {
                if (SelectedProduct == null)
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SoLuong <= 0)
                {
                    MessageBox.Show("Số lượng không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SoLuong > SelectedProduct.Quantity)
                {
                    MessageBox.Show($"Số lượng tồn kho chỉ còn {SelectedProduct.Quantity}!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existingItem = OrderItems.FirstOrDefault(x => x.ProductId == SelectedProduct.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += SoLuong;
                    existingItem.LineTotal = existingItem.Quantity * existingItem.UnitPrice;
                }
                else
                {
                    OrderItems.Add(new OrderItemTemp
                    {
                        ProductId = SelectedProduct.ProductId,
                        ProductName = SelectedProduct.ProductName,
                        Quantity = SoLuong,
                        UnitPrice = SelectedProduct.Price,
                        LineTotal = SoLuong * SelectedProduct.Price
                    });
                }

                UpdateTotal();
                SoLuong = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm sản phẩm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCreateOrder(object parameter)
        {
            try
            {
                if (SelectedCustomer == null)
                {
                    MessageBox.Show("Vui lòng chọn khách hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OrderItems.Count == 0)
                {
                    MessageBox.Show("Vui lòng thêm ít nhất một sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = DbHelper.GetContext())
                {
                    string orderCode = GenerateOrderCode(db);

                    var order = new Order
                    {
                        OrderCode = orderCode,
                        CustomerId = SelectedCustomer.CustomerId,
                        EmployeeId = SelectedEmployee.EmployeeId,
                        OrderDate = DateTime.Now,
                        Status = "Đã thanh toán",
                        Note = GhiChu ?? "",
                        TotalAmount = TongCong
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    foreach (var item in OrderItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            LineTotal = item.LineTotal
                        };
                        db.OrderItems.Add(orderItem);

                        var product = db.Products.Find(item.ProductId);
                        if (product != null)
                        {
                            product.Quantity -= item.Quantity;
                        }
                    }

                    db.SaveChanges();

                    MessageBox.Show($"Tạo hóa đơn {orderCode} thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    LoadOrders();
                    LoadProducts();
                    ExecuteCancel(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel(object parameter)
        {
            SelectedCustomer = null;
            SelectedEmployee = null;
            SelectedProduct = null;
            SoLuong = 1;
            GhiChu = "";
            OrderItems.Clear();
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            TongCong = OrderItems.Sum(x => x.LineTotal);
        }

        private string GenerateOrderCode(QLCH1Entities db)
        {
            var lastOrder = db.Orders
                .OrderByDescending(o => o.OrderId)
                .FirstOrDefault();

            if (lastOrder == null)
                return "HD001";

            int lastNumber = 0;
            if (lastOrder.OrderCode.StartsWith("HD"))
            {
                int.TryParse(lastOrder.OrderCode.Substring(2), out lastNumber);
            }

            return $"HD{(lastNumber + 1):D3}";
        }
    }

    public class OrderDisplayItem
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class OrderItemTemp
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
