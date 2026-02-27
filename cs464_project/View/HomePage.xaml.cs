using cs464_project.View_Body;
using System.Windows;

namespace cs464_project.View
{
    public partial class HomePage : Window
    {
        public HomePage(string fullName = "Admin")
        {
            InitializeComponent();
            txtSidebarDate.Text = System.DateTime.Now.ToString("dd/MM/yyyy");
            txtSidebarUser.Text = fullName;
            mainFrame.Navigate(new DashboardUC());
        }

        private void Menu_TongQuat_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new DashboardUC());
        }

        private void Menu_BanHang_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new QuanLyHoaDon());
        }

        private void Menu_SanPham_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new QuanLySanPham());
        }

        private void Menu_KhachHang_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new QuanLyKhachHang());
        }

        private void Menu_NhanVien_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new QuanLyNhanVien());
        }

        private void Menu_ThongKe_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ThongKeDoanhThu());
        }

        private void Menu_DangXuat_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                                         System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Login login = new Login();
                login.Show();
                this.Close();
            }
        }
    }
}
