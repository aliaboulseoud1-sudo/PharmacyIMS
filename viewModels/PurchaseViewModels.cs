namespace PharmacyIMS.ViewModels
{
    public class CreatePurchaseViewModel
    {
        [Required(ErrorMessage = "يجب اختيار المورد")]
        [Display(Name = "المورد")]
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "تاريخ الشراء مطلوب")]
        [Display(Name = "تاريخ الشراء")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Display(Name = "أصناف طلب الشراء")]
        public List<PurchaseItemInputModel> Items { get; set; } = new();
    }

    public class PurchaseItemInputModel
    {
        [Required(ErrorMessage = "يجب اختيار المنتج")]
        [Display(Name = "المنتج / الدواء")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        [Display(Name = "الكمية")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون قيمة موجبة")]
        [Display(Name = "سعر الوحدة")]
        public decimal UnitCost { get; set; }
    }

    // Note: ProductPickerItem is already defined in viewModels/SaleViewModels.cs
    // (same PharmacyIMS.ViewModels namespace) and is reused here for the product dropdown.

    public class SupplierPickerItem
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
    }
}
