using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using cs464_project.DataAccess;

namespace cs464_project.ViewModel
{
    public class QuanLySanPhamViewModel : BaseViewModel
    {
        private string _maSP;
        private string _tenSP;
        private string _donVi;
        private string _giaBan;
        private string _giaNhap;
        private string _soLuong;
        private string _moTa;
        private int? _categoryId;
        private string _timKiem;
        private ProductItem _selectedProduct;
        private CategoryItem _selectedCategory;

        public string MaSP
        {
            get => _maSP;
            set => SetProperty(ref _maSP, value);
        }

        public string TenSP
        {
            get => _tenSP;
            set => SetProperty(ref _tenSP, value);
        }

        public string DonVi
        {
            get => _donVi;
            set => SetProperty(ref _donVi, value);
        }

        public string GiaBan
        {
            get => _giaBan;
            set => SetProperty(ref _giaBan, value);
        }

        public string GiaNhap
        {
            get => _giaNhap;
            set => SetProperty(ref _giaNhap, value);
        }

        public string SoLuong
        {
            get => _soLuong;
            set => SetProperty(ref _soLuong, value);
        }

        public string MoTa
        {
            get => _moTa;
            set => SetProperty(ref _moTa, value);
        }

        public int? CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        public string TimKiem
        {
            get => _timKiem;
            set => SetProperty(ref _timKiem, value);
        }

        public ProductItem SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value) && value != null)
                {
                    MaSP = value.ProductCode;
                    TenSP = value.ProductName;
                    DonVi = value.Unit;
                    GiaBan = value.Price.ToString();
                    GiaNhap = value.Cost?.ToString() ?? "0";
                    SoLuong = value.Quantity.ToString();
                    MoTa = value.Description;
                }
            }
        }

        public CategoryItem SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    CategoryId = value?.CategoryId;
                }
            }
        }

        public ObservableCollection<ProductItem> Products { get; }
        public ObservableCollection<CategoryItem> Categories { get; }

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public QuanLySanPhamViewModel()
        {
            Products = new ObservableCollection<ProductItem>();
            Categories = new ObservableCollection<CategoryItem>();
            SearchCommand = new RelayCommand(ExecuteSearch);
            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, CanEditOrDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanEditOrDelete);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            LoadData();
        }

        private bool CanEditOrDelete(object parameter) => SelectedProduct != null;

        public void LoadData()
        {
            try
            {
                Products.Clear();
                Categories.Clear();
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        @"select p.ProductId, p.ProductCode, p.ProductName, c.CategoryName, 
                          p.Unit, p.Price, p.Cost, p.Quantity, p.Description, p.CategoryId, p.IsActive, p.CreatedAt
                          from Products p join Categories c ON p.CategoryId = c.CategoryId", conn);
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    foreach (DataRow row in dt.Rows)
                    {
                        Products.Add(new ProductItem
                        {
                            ProductId = Convert.ToInt32(row["ProductId"]),
                            ProductCode = row["ProductCode"]?.ToString(),
                            ProductName = row["ProductName"]?.ToString(),
                            CategoryName = row["CategoryName"]?.ToString(),
                            Unit = row["Unit"]?.ToString(),
                            Price = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0,
                            Cost = row["Cost"] != DBNull.Value ? Convert.ToDecimal(row["Cost"]) : 0,
                            Quantity = row["Quantity"] != DBNull.Value ? Convert.ToInt32(row["Quantity"]) : 0,
                            Description = row["Description"]?.ToString(),
                            CategoryId = row["CategoryId"] != DBNull.Value ? Convert.ToInt32(row["CategoryId"]) : (int?)null,
                            IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                            CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.Now
                        });
                    }

                    var cmdCat = new SqlCommand("select * from Categories", conn);
                    var dtCat = new DataTable();
                    dtCat.Load(cmdCat.ExecuteReader());
                    foreach (DataRow row in dtCat.Rows)
                    {
                        Categories.Add(new CategoryItem
                        {
                            CategoryId = Convert.ToInt32(row["CategoryId"]),
                            CategoryName = row["CategoryName"]?.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void ExecuteSearch(object parameter)
        {
            try
            {
                Products.Clear();
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        @"select p.ProductId, p.ProductCode, p.ProductName, c.CategoryName, 
                          p.Unit, p.Price, p.Cost, p.Quantity, p.Description, p.CategoryId, p.IsActive, p.CreatedAt
                          FROM Products p join Categories c ON p.CategoryId = c.CategoryId
                          where p.ProductName LIKE @kw OR p.ProductCode LIKE @kw", conn);
                    cmd.Parameters.AddWithValue("@kw", "%" + (TimKiem ?? "") + "%");
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    foreach (DataRow row in dt.Rows)
                    {
                        Products.Add(new ProductItem
                        {
                            ProductId = Convert.ToInt32(row["ProductId"]),
                            ProductCode = row["ProductCode"]?.ToString(),
                            ProductName = row["ProductName"]?.ToString(),
                            CategoryName = row["CategoryName"]?.ToString(),
                            Unit = row["Unit"]?.ToString(),
                            Price = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0,
                            Cost = row["Cost"] != DBNull.Value ? Convert.ToDecimal(row["Cost"]) : 0,
                            Quantity = row["Quantity"] != DBNull.Value ? Convert.ToInt32(row["Quantity"]) : 0,
                            Description = row["Description"]?.ToString(),
                            CategoryId = row["CategoryId"] != DBNull.Value ? Convert.ToInt32(row["CategoryId"]) : (int?)null,
                            IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                            CreatedAt = row["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(row["CreatedAt"]) : DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
            }
        }

        private void ExecuteAdd(object parameter)
        {
            if (string.IsNullOrWhiteSpace(MaSP))
            {
                MessageBox.Show("Vui lòng nhập mã sản phẩm!");
                return;
            }
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Products WHERE ProductCode = @code", conn);
                    checkCmd.Parameters.AddWithValue("@code", MaSP?.Trim() ?? "");
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Mã sản phẩm đã tồn tại! Vui lòng nhập mã khác.");
                        return;
                    }
                    var cmd = new SqlCommand(
                        @"INSERT INTO Products (ProductCode, ProductName, CategoryId, Unit, Price, Cost, Quantity, Description, IsActive, CreatedAt)
                          VALUES (@code, @name, @cat, @unit, @price, @cost, @qty, @desc, 1, GETDATE())", conn);
                    cmd.Parameters.AddWithValue("@code", MaSP?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@name", TenSP?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@cat", CategoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@unit", DonVi?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@price", decimal.Parse(GiaBan ?? "0"));
                    cmd.Parameters.AddWithValue("@cost", decimal.Parse(GiaNhap ?? "0"));
                    cmd.Parameters.AddWithValue("@qty", int.Parse(SoLuong ?? "0"));
                    cmd.Parameters.AddWithValue("@desc", MoTa?.Trim() ?? "");
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Thêm sản phẩm thành công!");
                LoadData();
                ExecuteRefresh(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm: " + ex.Message);
            }
        }

        private void ExecuteEdit(object parameter)
        {
            if (SelectedProduct == null) { MessageBox.Show("Vui lòng chọn sản phẩm cần sửa."); return; }
            if (string.IsNullOrWhiteSpace(MaSP))
            {
                MessageBox.Show("Vui lòng nhập mã sản phẩm!");
                return;
            }
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Products WHERE ProductCode = @code AND ProductId != @id", conn);
                    checkCmd.Parameters.AddWithValue("@code", MaSP?.Trim() ?? "");
                    checkCmd.Parameters.AddWithValue("@id", SelectedProduct.ProductId);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Mã sản phẩm đã tồn tại! Vui lòng nhập mã khác.");
                        return;
                    }
                    var cmd = new SqlCommand(
                        @"UPDATE Products SET ProductCode=@code, ProductName=@name, CategoryId=@cat, Unit=@unit, 
                          Price=@price, Cost=@cost, Quantity=@qty, Description=@desc
                          WHERE ProductId=@id", conn);
                    cmd.Parameters.AddWithValue("@id", SelectedProduct.ProductId);
                    cmd.Parameters.AddWithValue("@code", MaSP?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@name", TenSP?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@cat", CategoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@unit", DonVi?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@price", decimal.Parse(GiaBan ?? "0"));
                    cmd.Parameters.AddWithValue("@cost", decimal.Parse(GiaNhap ?? "0"));
                    cmd.Parameters.AddWithValue("@qty", int.Parse(SoLuong ?? "0"));
                    cmd.Parameters.AddWithValue("@desc", MoTa?.Trim() ?? "");
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Cập nhật thành công!");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa: " + ex.Message);
            }
        }

        private void ExecuteDelete(object parameter)
        {
            if (SelectedProduct == null) { MessageBox.Show("Vui lòng chọn sản phẩm cần xóa."); return; }
            if (MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand("DELETE FROM Products WHERE ProductId=@id", conn);
                    cmd.Parameters.AddWithValue("@id", SelectedProduct.ProductId);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Xóa thành công!");
                LoadData();
                ExecuteRefresh(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message);
            }
        }

        private void ExecuteRefresh(object parameter)
        {
            MaSP = "";
            TenSP = "";
            DonVi = "";
            GiaBan = "";
            GiaNhap = "";
            SoLuong = "";
            MoTa = "";
            SelectedCategory = null;
            SelectedProduct = null;
            TimKiem = "";
            LoadData();
        }
    }

    public class ProductItem
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal? Cost { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
