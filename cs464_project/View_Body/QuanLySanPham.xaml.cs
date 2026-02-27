using System.Windows.Controls;

namespace cs464_project.View_Body
{
    public partial class QuanLySanPham : UserControl
    {
        public QuanLySanPham()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModel.QuanLySanPhamViewModel vm)
            {
                vm.LoadData();
            }
        }
    }
}
