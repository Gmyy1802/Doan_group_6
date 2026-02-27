namespace Models
{
    public class ChiTietHoaDon
    {
        public SanPham SanPham { get; set; }

        public int SoLuong { get; set; }

        public double ThanhTien
        {
            get
            {
                return SoLuong * SanPham.DonGia;
            }
        }

        public string TenSP
        {
            get { return SanPham.TenSP; }
        }

        public double DonGia
        {
            get { return SanPham.DonGia; }
        }
    }
}