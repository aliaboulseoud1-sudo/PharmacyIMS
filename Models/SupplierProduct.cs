using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyIMS.Models
{
    public class SupplierProduct
    {
        [Key]
        public int SupplierProductID { get; set; }

        [Required]
        [Display(Name = "المورد")]
        public int SupplierID { get; set; }

        [ForeignKey(nameof(SupplierID))]
        public Supplier? Supplier { get; set; }

        [Required]
        [Display(Name = "المنتج")]
        public int ProductID { get; set; }

        [ForeignKey(nameof(ProductID))]
        public Product? Product { get; set; }

        [StringLength(50)]
        [Display(Name = "رمز المنتج لدى المورد")]
        public string? SupplierSKU { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "سعر التعاقد")]
        public decimal ContractPrice { get; set; }

        [Range(0, 365)]
        [Display(Name = "مدة التوريد (أيام)")]
        public int LeadTimeDays { get; set; }
    }
}
