using Models;
using System.Windows;

namespace HienThiSanPham
{
    public partial class ChiTietSanPhamWindow : Window
    {
        public ChiTietSanPhamWindow(SanPham sp)
        {
            InitializeComponent();

            txtMa.Text = sp.MaSP;
            txtTen.Text = sp.TenSP;
            txtGia.Text = sp.DonGia.ToString();
        }
    }
}