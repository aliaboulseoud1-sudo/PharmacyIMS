using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyIMS.Models
{
    public class Sale
    {
        [Key]
        public int SaleID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ البيع")]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "إجمالي المبلغ")]
        public decimal TotalAmount { get; set; }

        [StringLength(150)]
        [Display(Name = "بيانات العميل")]
        public string? CustomerInfo { get; set; }

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
