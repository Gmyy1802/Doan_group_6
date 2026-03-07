using System.Windows.Controls;

namespace cs464_project.View_Body
{
    public partial class QuanLyHoaDon : UserControl
    {
        public QuanLyHoaDon()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModel.QuanLyHoaDonViewModel vm)
            {
                vm.LoadData();
            }
        }
    }
}
