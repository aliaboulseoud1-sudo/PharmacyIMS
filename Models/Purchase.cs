using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyIMS.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseID { get; set; }

        [Required]
        [Display(Name = "المورد")]
        public int SupplierID { get; set; }

        [ForeignKey(nameof(SupplierID))]
        public Supplier? Supplier { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الشراء")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "إجمالي المبلغ")]
        public decimal TotalAmount { get; set; }

        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
