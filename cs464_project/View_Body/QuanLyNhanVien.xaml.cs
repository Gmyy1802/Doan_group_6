using System.Windows.Controls;

namespace cs464_project.View_Body
{
    public partial class QuanLyNhanVien : UserControl
    {
        public QuanLyNhanVien()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModel.QuanLyNhanVienViewModel vm)
            {
                vm.LoadData();
            }
        }
    }
}
