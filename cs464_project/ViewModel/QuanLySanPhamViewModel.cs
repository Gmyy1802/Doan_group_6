using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
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
                using (var db = DbHelper.GetContext())
                {
                    var products = db.Products.Include(p => p.Category).ToList();
                    foreach (var p in products)
                    {
                        Products.Add(new ProductItem
                        {
                            ProductId = p.ProductId,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            CategoryName = p.Category?.CategoryName,
                            Unit = p.Unit,
                            Price = p.Price,
                            Cost = p.Cost ?? 0,
                            Quantity = p.Quantity,
                            Description = p.Description,
                            CategoryId = p.CategoryId,
                            IsActive = p.IsActive,
                            CreatedAt = p.CreatedAt
                        });
                    }

                    var categories = db.Categories.ToList();
                    foreach (var c in categories)
                    {
                        Categories.Add(new CategoryItem
                        {
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName
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
                using (var db = DbHelper.GetContext())
                {
                    string kw = (TimKiem ?? "").Trim().ToLower();
                    var query = db.Products.Include(p => p.Category).AsQueryable();

                    if (!string.IsNullOrEmpty(kw))
                    {
                        query = query.Where(p => p.ProductName.ToLower().Contains(kw)
                                              || p.ProductCode.ToLower().Contains(kw));
                    }

                    foreach (var p in query.ToList())
                    {
                        Products.Add(new ProductItem
                        {
                            ProductId = p.ProductId,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            CategoryName = p.Category?.CategoryName,
                            Unit = p.Unit,
                            Price = p.Price,
                            Cost = p.Cost ?? 0,
                            Quantity = p.Quantity,
                            Description = p.Description,
                            CategoryId = p.CategoryId,
                            IsActive = p.IsActive,
                            CreatedAt = p.CreatedAt
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
                using (var db = DbHelper.GetContext())
                {
                    string code = MaSP.Trim();
                    if (db.Products.Any(p => p.ProductCode == code))
                    {
                        MessageBox.Show("Mã sản phẩm đã tồn tại! Vui lòng nhập mã khác.");
                        return;
                    }

                    var product = new Model.Product
                    {
                        ProductCode = code,
                        ProductName = TenSP?.Trim() ?? "",
                        CategoryId = CategoryId ?? 0,
                        Unit = DonVi?.Trim() ?? "",
                        Price = decimal.Parse(GiaBan ?? "0"),
                        Cost = decimal.Parse(GiaNhap ?? "0"),
                        Quantity = int.Parse(SoLuong ?? "0"),
                        Description = MoTa?.Trim() ?? "",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    db.Products.Add(product);
                    db.SaveChanges();
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
                using (var db = DbHelper.GetContext())
                {
                    string code = MaSP.Trim();
                    int id = SelectedProduct.ProductId;

                    if (db.Products.Any(p => p.ProductCode == code && p.ProductId != id))
                    {
                        MessageBox.Show("Mã sản phẩm đã tồn tại! Vui lòng nhập mã khác.");
                        return;
                    }

                    var product = db.Products.Find(id);
                    if (product != null)
                    {
                        product.ProductCode = code;
                        product.ProductName = TenSP?.Trim() ?? "";
                        product.CategoryId = CategoryId ?? 0;
                        product.Unit = DonVi?.Trim() ?? "";
                        product.Price = decimal.Parse(GiaBan ?? "0");
                        product.Cost = decimal.Parse(GiaNhap ?? "0");
                        product.Quantity = int.Parse(SoLuong ?? "0");
                        product.Description = MoTa?.Trim() ?? "";
                        db.SaveChanges();
                    }
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
                using (var db = DbHelper.GetContext())
                {
                    var product = db.Products.Find(SelectedProduct.ProductId);
                    if (product != null)
                    {
                        db.Products.Remove(product);
                        db.SaveChanges();
                    }
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
