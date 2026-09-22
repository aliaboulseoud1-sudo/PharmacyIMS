using System.ComponentModel.DataAnnotations;

namespace PharmacyIMS.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "اسم المورد مطلوب")]
        [StringLength(150)]
        [Display(Name = "اسم المورد / الشركة")]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "اسم مسؤول التواصل")]
        public string? ContactName { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Display(Name = "رقم الهاتف")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        [StringLength(250)]
        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
    }
}
