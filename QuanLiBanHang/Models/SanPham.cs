namespace Models
{
    public class SanPham
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public double DonGia { get; set; }

        public override string ToString()
        {
            return TenSP;
        }
    }
}