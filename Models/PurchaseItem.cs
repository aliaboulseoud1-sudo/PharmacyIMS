namespace PharmacyIMS.Models
{
    public class PurchaseItem
    {
        [Key]
        public int PurchaseItemID { get; set; }

        [Required]
        public int PurchaseID { get; set; }

        [ForeignKey(nameof(PurchaseID))]
        public Purchase? Purchase { get; set; }

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
        [Display(Name = "سعر الوحدة")]
        public decimal UnitCost { get; set; }

        [NotMapped]
        public decimal LineTotal => Quantity * UnitCost;
    }
}
