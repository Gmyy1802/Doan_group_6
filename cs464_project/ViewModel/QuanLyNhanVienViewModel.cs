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
    public class QuanLyNhanVienViewModel : BaseViewModel
    {
        private string _maNV;
        private string _hoTen;
        private string _gioiTinh;
        private DateTime? _ngaySinh;
        private string _diaChi;
        private DateTime? _ngayVao;
        private string _luongCoBan;
        private string _timKiem;
        private EmployeeItem _selectedEmployee;

        public string MaNV
        {
            get => _maNV;
            set => SetProperty(ref _maNV, value);
        }

        public string HoTen
        {
            get => _hoTen;
            set => SetProperty(ref _hoTen, value);
        }

        public string GioiTinh
        {
            get => _gioiTinh;
            set => SetProperty(ref _gioiTinh, value);
        }

        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set => SetProperty(ref _ngaySinh, value);
        }

        public string DiaChi
        {
            get => _diaChi;
            set => SetProperty(ref _diaChi, value);
        }

        public DateTime? NgayVao
        {
            get => _ngayVao;
            set => SetProperty(ref _ngayVao, value);
        }

        public string LuongCoBan
        {
            get => _luongCoBan;
            set => SetProperty(ref _luongCoBan, value);
        }

        public string TimKiem
        {
            get => _timKiem;
            set => SetProperty(ref _timKiem, value);
        }

        public EmployeeItem SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetProperty(ref _selectedEmployee, value) && value != null)
                {
                    MaNV = value.EmployeeCode;
                    HoTen = value.FullName;
                    GioiTinh = value.Gender;
                    NgaySinh = value.BirthDate;
                    DiaChi = value.Address;
                    NgayVao = value.HireDate;
                    LuongCoBan = value.BaseSalary.ToString();
                }
            }
        }

        public ObservableCollection<EmployeeItem> Employees { get; }

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public QuanLyNhanVienViewModel()
        {
            Employees = new ObservableCollection<EmployeeItem>();
            SearchCommand = new RelayCommand(ExecuteSearch);
            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, CanEditOrDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanEditOrDelete);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            LoadData();
        }

        private bool CanEditOrDelete(object parameter) => SelectedEmployee != null;

        public void LoadData()
        {
            try
            {
                Employees.Clear();
                using (var db = DbHelper.GetContext())
                {
                    var employees = db.Employees.ToList();
                    foreach (var emp in employees)
                    {
                        Employees.Add(new EmployeeItem
                        {
                            EmployeeId = emp.EmployeeId,
                            EmployeeCode = emp.EmployeeCode,
                            FullName = emp.FullName,
                            Gender = emp.Gender,
                            BirthDate = emp.BirthDate,
                            Address = emp.Address,
                            HireDate = emp.HireDate,
                            BaseSalary = emp.BaseSalary,
                            IsWorking = emp.IsWorking,
                            StatusText = emp.IsWorking ? "Đang làm" : "Nghỉ việc"
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
                Employees.Clear();
                using (var db = DbHelper.GetContext())
                {
                    string keyword = (TimKiem ?? "").Trim().ToLower();
                    var query = db.Employees.AsQueryable();

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        query = query.Where(emp =>
                            emp.FullName.ToLower().Contains(keyword) ||
                            emp.EmployeeCode.ToLower().Contains(keyword) ||
                            emp.Address.ToLower().Contains(keyword));
                    }

                    foreach (var emp in query.ToList())
                    {
                        Employees.Add(new EmployeeItem
                        {
                            EmployeeId = emp.EmployeeId,
                            EmployeeCode = emp.EmployeeCode,
                            FullName = emp.FullName,
                            Gender = emp.Gender,
                            BirthDate = emp.BirthDate,
                            Address = emp.Address,
                            HireDate = emp.HireDate,
                            BaseSalary = emp.BaseSalary,
                            IsWorking = emp.IsWorking,
                            StatusText = emp.IsWorking ? "Đang làm" : "Nghỉ việc"
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

                using (var db = DbHelper.GetContext())
                {
                    string employeeCode = GenerateEmployeeCode(db);

                    var employee = new Employee
                    {
                        EmployeeCode = employeeCode,
                        FullName = HoTen.Trim(),
                        Gender = GioiTinh ?? "Nam",
                        BirthDate = NgaySinh,
                        Address = DiaChi?.Trim() ?? "",
                        HireDate = NgayVao ?? DateTime.Now,
                        BaseSalary = decimal.TryParse(LuongCoBan, out decimal salary) ? salary : 0,
                        IsWorking = true
                    };

                    db.Employees.Add(employee);
                    db.SaveChanges();

                    MessageBox.Show("Thêm nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                    ExecuteRefresh(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEdit(object parameter)
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(HoTen))
                {
                    MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = DbHelper.GetContext())
                {
                    var employee = db.Employees.Find(SelectedEmployee.EmployeeId);
                    if (employee != null)
                    {
                        employee.FullName = HoTen.Trim();
                        employee.Gender = GioiTinh ?? "Nam";
                        employee.BirthDate = NgaySinh;
                        employee.Address = DiaChi?.Trim() ?? "";
                        employee.HireDate = NgayVao ?? DateTime.Now;
                        employee.BaseSalary = decimal.TryParse(LuongCoBan, out decimal salary) ? salary : 0;

                        db.SaveChanges();
                        MessageBox.Show("Cập nhật nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        ExecuteRefresh(null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDelete(object parameter)
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa nhân viên này?", "Xác nhận", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var db = DbHelper.GetContext())
                    {
                        var employee = db.Employees.Find(SelectedEmployee.EmployeeId);
                        if (employee != null)
                        {
                            // Đánh dấu nghỉ việc thay vì xóa
                            employee.IsWorking = false;
                            db.SaveChanges();
                            MessageBox.Show("Đã chuyển nhân viên sang trạng thái nghỉ việc!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadData();
                            ExecuteRefresh(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRefresh(object parameter)
        {
            MaNV = "";
            HoTen = "";
            GioiTinh = "Nam";
            NgaySinh = null;
            DiaChi = "";
            NgayVao = DateTime.Now;
            LuongCoBan = "";
            SelectedEmployee = null;
            TimKiem = "";
            LoadData();
        }

        private string GenerateEmployeeCode(QLCH1Entities db)
        {
            var lastEmployee = db.Employees
                .OrderByDescending(emp => emp.EmployeeId)
                .FirstOrDefault();

            if (lastEmployee == null)
                return "NV001";

            int lastNumber = 0;
            if (lastEmployee.EmployeeCode.StartsWith("NV"))
            {
                int.TryParse(lastEmployee.EmployeeCode.Substring(2), out lastNumber);
            }

            return $"NV{(lastNumber + 1):D3}";
        }
    }

    public class EmployeeItem
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Address { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal BaseSalary { get; set; }
        public bool IsWorking { get; set; }
        public string StatusText { get; set; }
    }
}
