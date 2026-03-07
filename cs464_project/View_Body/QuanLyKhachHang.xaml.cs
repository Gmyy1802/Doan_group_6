using System.Windows.Controls;

namespace cs464_project.View_Body
{
    public partial class QuanLyKhachHang : UserControl
    {
        public QuanLyKhachHang()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModel.QuanLyKhachHangViewModel vm)
            {
                vm.LoadData();
            }
        }
    }
}
