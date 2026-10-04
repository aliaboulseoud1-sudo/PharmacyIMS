namespace PharmacyIMS.Models
{
    public class SaleItem
    {
        [Key]
        public int SaleItemID { get; set; }

        [Required]
        public int SaleID { get; set; }

        [ForeignKey(nameof(SaleID))]
        public Sale? Sale { get; set; }

        [Required]
        [Display(Name = "المنتج")]
        public int ProductID { get; set; }

        [ForeignKey(nameof(ProductID))]
        public Product? Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        [Display(Name = "الكمية")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "سعر البيع للوحدة")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
