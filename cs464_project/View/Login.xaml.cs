using cs464_project.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace cs464_project.View
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void pw_mk_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = pw_mk.Password;
            }
        }

        private void txtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = txtPasswordVisible.Text;
            }
        }
    }
}
