using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using cs464_project.DataAccess;
using cs464_project.Model;

namespace cs464_project.ViewModel
{
    public class QuanLyKhachHangViewModel : BaseViewModel
    {
        private string _maKH;
        private string _hoTen;
        private string _sdt;
        private string _email;
        private string _diaChi;
        private string _timKiem;
        private CustomerItem _selectedCustomer;

        public string MaKH
        {
            get => _maKH;
            set => SetProperty(ref _maKH, value);
        }

        public string HoTen
        {
            get => _hoTen;
            set => SetProperty(ref _hoTen, value);
        }

        public string SDT
        {
            get => _sdt;
            set => SetProperty(ref _sdt, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string DiaChi
        {
            get => _diaChi;
            set => SetProperty(ref _diaChi, value);
        }

        public string TimKiem
        {
            get => _timKiem;
            set => SetProperty(ref _timKiem, value);
        }

        public CustomerItem SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && value != null)
                {
                    MaKH = value.CustomerCode;
                    HoTen = value.FullName;
                    SDT = value.Phone;
                    Email = value.Email;
                    DiaChi = value.Address;
                }
            }
        }

        public ObservableCollection<CustomerItem> Customers { get; }

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public QuanLyKhachHangViewModel()
        {
            Customers = new ObservableCollection<CustomerItem>();
            SearchCommand = new RelayCommand(ExecuteSearch);
            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, CanEditOrDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanEditOrDelete);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            LoadData();
        }

        private bool CanEditOrDelete(object parameter) => SelectedCustomer != null;

        public void LoadData()
        {
            try
            {
                Customers.Clear();
                using (var db = DbHelper.GetContext())
                {
                    var customers = db.Customers.ToList();
                    foreach (var cust in customers)
                    {
                        Customers.Add(new CustomerItem
                        {
                            CustomerId = cust.CustomerId,
                            CustomerCode = cust.CustomerCode,
                            FullName = cust.FullName,
                            Phone = cust.Phone,
                            Email = cust.Email,
                            Address = cust.Address,
                            CreatedAt = cust.CreatedAt
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSearch(object parameter)
        {
            try
            {
                Customers.Clear();
                using (var db = DbHelper.GetContext())
                {
                    string keyword = (TimKiem ?? "").Trim().ToLower();
                    var query = db.Customers.AsQueryable();

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        query = query.Where(c =>
                            c.FullName.ToLower().Contains(keyword) ||
                            c.CustomerCode.ToLower().Contains(keyword) ||
                            c.Phone.Contains(keyword) ||
                            (c.Email != null && c.Email.ToLower().Contains(keyword)));
                    }

                    foreach (var cust in query.ToList())
                    {
                        Customers.Add(new CustomerItem
                        {
                            CustomerId = cust.CustomerId,
                            CustomerCode = cust.CustomerCode,
                            FullName = cust.FullName,
                            Phone = cust.Phone,
                            Email = cust.Email,
                            Address = cust.Address,
                            CreatedAt = cust.CreatedAt
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAdd(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(HoTen))
                {
                    MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(SDT))
                {
                    MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = DbHelper.GetContext())
                {
                    string phone = SDT.Trim();
                    if (db.Customers.Any(c => c.Phone == phone))
                    {
                        MessageBox.Show("Số điện thoại đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string customerCode = GenerateCustomerCode(db);

                    var customer = new Customer
                    {
                        CustomerCode = customerCode,
                        FullName = HoTen.Trim(),
                        Phone = SDT.Trim(),
                        Email = Email?.Trim() ?? "",
                        Address = DiaChi?.Trim() ?? "",
                        CreatedAt = DateTime.Now
                    };

                    db.Customers.Add(customer);
                    db.SaveChanges();

                    MessageBox.Show("Thêm khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                    ExecuteRefresh(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm khách hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEdit(object parameter)
        {
            try
            {
                if (SelectedCustomer == null)
                {
                    MessageBox.Show("Vui lòng chọn khách hàng cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(HoTen))
                {
                    MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(SDT))
                {
                    MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = DbHelper.GetContext())
                {
                    string phone = SDT.Trim();
                    if (db.Customers.Any(c => c.Phone == phone && c.CustomerId != SelectedCustomer.CustomerId))
                    {
                        MessageBox.Show("Số điện thoại đã được sử dụng bởi khách hàng khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var customer = db.Customers.Find(SelectedCustomer.CustomerId);
                    if (customer != null)
                    {
                        customer.FullName = HoTen.Trim();
                        customer.Phone = SDT.Trim();
                        customer.Email = Email?.Trim() ?? "";
                        customer.Address = DiaChi?.Trim() ?? "";

                        db.SaveChanges();
                        MessageBox.Show("Cập nhật khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        ExecuteRefresh(null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa khách hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDelete(object parameter)
        {
            try
            {
                if (SelectedCustomer == null)
                {
                    MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này?\n\nLưu ý: Không thể xóa khách hàng đã có hóa đơn.",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var db = DbHelper.GetContext())
                    {
                        var customer = db.Customers.Find(SelectedCustomer.CustomerId);
                        if (customer != null)
                        {
                            if (customer.Orders.Any())
                            {
                                MessageBox.Show("Không thể xóa khách hàng đã có hóa đơn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            db.Customers.Remove(customer);
                            db.SaveChanges();
                            MessageBox.Show("Xóa khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadData();
                            ExecuteRefresh(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa khách hàng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRefresh(object parameter)
        {
            MaKH = "";
            HoTen = "";
            SDT = "";
            Email = "";
            DiaChi = "";
            SelectedCustomer = null;
            TimKiem = "";
            LoadData();
        }

        private string GenerateCustomerCode(QLCH1Entities db)
        {
            var lastCustomer = db.Customers
                .OrderByDescending(c => c.CustomerId)
                .FirstOrDefault();

            if (lastCustomer == null)
                return "KH001";

            int lastNumber = 0;
            if (lastCustomer.CustomerCode.StartsWith("KH"))
            {
                int.TryParse(lastCustomer.CustomerCode.Substring(2), out lastNumber);
            }

            return $"KH{(lastNumber + 1):D3}";
        }
    }

    public class CustomerItem
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
