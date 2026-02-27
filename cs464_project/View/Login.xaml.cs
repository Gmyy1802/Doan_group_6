using cs464_project.DataAccess;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace cs464_project.View
{
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

        private void txtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_syncingText) return;
            if (_isPasswordVisible)
            {
                _syncingText = true;
                pw_mk.Password = txtPasswordVisible.Text;
                _syncingText = false;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txt_DN.Text.Trim();
            string password = pw_mk.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT UserId, PasswordHash, Salt, FullName, RoleId, IsActive FROM Users WHERE Username = @username";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
                                if (!isActive)
                                {
                                    MessageBox.Show("Tài khoản đã bị vô hiệu hóa!",
                                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }

                                byte[] storedHash = (byte[])reader["PasswordHash"];
                                byte[] salt = (byte[])reader["Salt"];

                                if (DbHelper.VerifyPassword(password, storedHash, salt))
                                {
                                    string fullName = reader["FullName"]?.ToString() ?? username;
                                    HomePage home = new HomePage(fullName);
                                    home.Show();
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Sai mật khẩu!",
                                                    "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Tài khoản không tồn tại!",
                                                "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message,
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
