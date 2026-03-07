using System;
using System.Linq;
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
                using (var db = DbHelper.GetContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Username == Username);

                    if (user == null)
                    {
                        MessageBox.Show("Tài khoản không tồn tại!",
                                        "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!user.IsActive)
                    {
                        MessageBox.Show("Tài khoản đã bị vô hiệu hóa!",
                                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (DbHelper.VerifyPassword(Password, user.PasswordHash, user.Salt))
                    {
                        string fullName = user.FullName ?? Username;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message,
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
