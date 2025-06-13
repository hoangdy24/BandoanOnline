using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace QlW_BandoanOnline.Models
{
    public class ChiTietDonHang
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int MaDonHang { get; set; }
        public int? MaSanPham { get; set; }

        // Thông tin sản phẩm
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public string GhiChu { get; set; }

        // Tùy chọn thêm (lưu dạng JSON)
        public string TuyChonThem { get; set; }

        // Navigation properties
        [ForeignKey("MaSanPham")] // Rõ ràng chỉ định khóa ngoại
        public virtual SanPham SanPham { get; set; }

        [ForeignKey("MaDonHang")]
        public virtual DonHang DonHang { get; set; }

        // Tính toán thành tiền
        [NotMapped]
        public decimal ThanhTien
        {
            get
            {
                var thanhTien = DonGia * SoLuong;
                
                if (!string.IsNullOrEmpty(TuyChonThem))
                {
                    try
                    {
                        var tuyChonList = JsonConvert.DeserializeObject<List<TuyChonThemModel>>(TuyChonThem);
                        thanhTien += tuyChonList?.Sum(t => t.GiaThem * SoLuong) ?? 0;
                    }
                    catch
                    {
                        // Xử lý lỗi nếu cần
                    }
                }

                return thanhTien;
            }
        }

        [NotMapped]
        public List<TuyChonThemModel> DanhSachTuyChon 
        {
            get
            {
                if (string.IsNullOrEmpty(TuyChonThem))
                    return new List<TuyChonThemModel>();

                try
                {
                    return JsonConvert.DeserializeObject<List<TuyChonThemModel>>(TuyChonThem);
                }
                catch
                {
                    return new List<TuyChonThemModel>();
                }
            }
        }
    }
}