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
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private bool _isPasswordVisible = false;
        private bool _syncingText = false; 

        public Login()
        {
            InitializeComponent();
        }

        
        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                txtPasswordVisible.Text = pw_mk.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                pw_mk.Visibility = Visibility.Collapsed;
                txtEyeIcon.Text = "👁"; 
                txtPasswordVisible.Focus();
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
            }
            else
            {
                pw_mk.Password = txtPasswordVisible.Text;
                pw_mk.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtEyeIcon.Text = "👁";
                pw_mk.Focus();
            }
        }

        private void pw_mk_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_syncingText) return;

            if (_isPasswordVisible)
            {
                _syncingText = true;
                txtPasswordVisible.Text = pw_mk.Password;
                _syncingText = false;
            }
        }

        private void txtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_syncingText) return;

            // Khi đang ở chế độ hiện mật khẩu -> đồng bộ sang PasswordBox
            if (_isPasswordVisible)
            {
                _syncingText = true;
                pw_mk.Password = txtPasswordVisible.Text;
                _syncingText = false;
            }
        }

        // ====== Đăng nhập / Thoát ======
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txt_DN.Text.Trim();
            string password = pw_mk.Password; // luôn lấy từ PasswordBox (đã đồng bộ)

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Demo tạm: admin / 123
            // Sau này bạn nối DB thì đổi phần này thành query ADO.NET
            if (username == "admin" && password == "123")
            {
                HomePage home = new HomePage();
                home.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!",
                                "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
