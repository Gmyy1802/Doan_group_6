using Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace BanHangApp
{
    public partial class MainWindow : Window
    {
        ObservableCollection<SanPham> dsSanPham =
            new ObservableCollection<SanPham>();

        ObservableCollection<ChiTietHoaDon> dsChiTiet =
            new ObservableCollection<ChiTietHoaDon>();

        ObservableCollection<HoaDon> dsHoaDon =
            new ObservableCollection<HoaDon>();

        public MainWindow()
        {
            InitializeComponent();

            dpNgay.SelectedDate = DateTime.Now;

            dsSanPham.Add(new SanPham
            {
                MaSP = "SP01",
                TenSP = "Laptop",
                DonGia = 15000
            });

            dsSanPham.Add(new SanPham
            {
                MaSP = "SP02",
                TenSP = "Chuột",
                DonGia = 200
            });

            cboSanPham.ItemsSource = dsSanPham;
            dataGrid.ItemsSource = dsChiTiet;
        }

        private void BtnThemSP_Click(object sender, RoutedEventArgs e)
        {
            SanPham sp = (SanPham)cboSanPham.SelectedItem;

            int sl = int.Parse(txtSoLuong.Text);

            dsChiTiet.Add(new ChiTietHoaDon
            {
                SanPham = sp,
                SoLuong = sl
            });

            TinhTongTien();
        }

        void TinhTongTien()
        {
            double tong = 0;

            foreach (var item in dsChiTiet)
                tong += item.ThanhTien;

            txtTongTien.Text = tong.ToString();
        }

        private void BtnLuuHoaDon_Click(object sender, RoutedEventArgs e)
        {
            HoaDon hd = new HoaDon();

            hd.MaHoaDon = txtMaHD.Text;
            hd.KhachHang = txtKhachHang.Text;
            hd.Ngay = dpNgay.SelectedDate.Value;

            foreach (var item in dsChiTiet)
                hd.DanhSachSP.Add(item);

            dsHoaDon.Add(hd);

            MessageBox.Show("Lưu hóa đơn thành công!");

            dsChiTiet.Clear();

            txtTongTien.Text = "0";
        }
    }
}