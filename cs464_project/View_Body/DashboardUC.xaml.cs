using System.Windows;
using System.Windows.Controls;

namespace cs464_project.View_Body
{
    public partial class DashboardUC : UserControl
    {
        public DashboardUC()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel.DashboardViewModel vm)
            {
                vm.LoadData();
            }
        }
    }
}
