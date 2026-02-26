using cs464_project.View_Body;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace cs464_project.View
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Window
    {
        public HomePage()
        {
            InitializeComponent();
            if (txtHomNay != null)
                txtHomNay.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtDoanhThuHomNay.Text = "0 đ";
            txtSoHoaDonHomNay.Text = "0";
            txtTongSanPham.Text = "0";
            txtTongNhanVien.Text = "0";
        }

        private void Menu_TongQuat_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bạn đang ở trang Tổng Quát.");
        }

        private void Menu_BanHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chưa có trang Bán Hàng.");
        }

        private void Menu_SanPham_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chưa có trang Sản Phẩm.");
        }

        private void Menu_KhachHang_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chưa có trang Khách Hàng.");
        }

        private void Menu_NhanVien_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chưa có trang Nhân Viên.");
        }

        private void Menu_ThongKe_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chưa có trang Thống Kê.");
        }

        private void Menu_DangXuat_Click(object sender, RoutedEventArgs e)
        {
           

            MessageBox.Show("Đăng xuất (demo).");
        
    }
    }
}
