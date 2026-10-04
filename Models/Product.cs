namespace PharmacyIMS.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "رمز الصنف (SKU) مطلوب")]
        [StringLength(50)]
        [Display(Name = "رمز الصنف (SKU)")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [StringLength(150)]
        [Display(Name = "اسم الدواء / المنتج")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "الشركة المصنعة")]
        public string? Manufacturer { get; set; }

        [StringLength(50)]
        [Display(Name = "الشكل الصيدلاني")]
        public string? DosageForm { get; set; }

        [Required(ErrorMessage = "الفئة مطلوبة")]
        [Display(Name = "الفئة")]
        public int CategoryID { get; set; }

        [ForeignKey(nameof(CategoryID))]
        public Category? Category { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون قيمة موجبة")]
        [Display(Name = "سعر البيع")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون قيمة موجبة")]
        [Display(Name = "الكمية المتوفرة بالمخزون")]
        public int StockQuantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "حد إعادة الطلب (تنبيه النقص)")]
        public int LowStockThreshold { get; set; } = 10;

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ انتهاء الصلاحية")]
        public DateTime? ExpiryDate { get; set; }

        [NotMapped]
        [Display(Name = "الحالة")]
        public bool IsLowStock => StockQuantity <= LowStockThreshold;

        public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
