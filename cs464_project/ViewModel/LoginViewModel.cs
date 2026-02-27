using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using cs464_project.DataAccess;

namespace cs464_project.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private bool _isPasswordVisible;
        private bool _syncingText;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordCommand { get; }
        public ICommand ExitCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            TogglePasswordCommand = new RelayCommand(ExecuteTogglePassword);
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
        }

        private void ExecuteTogglePassword(object parameter)
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void ExecuteLogin(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
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
                        cmd.Parameters.AddWithValue("@username", Username);
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

                                if (DbHelper.VerifyPassword(Password, storedHash, salt))
                                {
                                    string fullName = reader["FullName"]?.ToString() ?? Username;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        var home = new View.HomePage(fullName);
                                        home.Show();
                                        foreach (Window window in Application.Current.Windows)
                                        {
                                            if (window is View.Login)
                                            {
                                                window.Close();
                                                break;
                                            }
                                        }
                                    });
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
    }
}
