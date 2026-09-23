using System.ComponentModel.DataAnnotations;

namespace PharmacyIMS.ViewModels
{
    public class CreateSaleViewModel
    {
        [StringLength(150)]
        [Display(Name = "بيانات العميل (الاسم أو رقم الهاتف)")]
        public string? CustomerInfo { get; set; }

        [Required(ErrorMessage = "تاريخ البيع مطلوب")]
        [Display(Name = "تاريخ ووقت البيع")]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Display(Name = "أصناف الفاتورة")]
        public List<SaleItemInputModel> Items { get; set; } = new();
    }

    public class SaleItemInputModel
    {
        [Required(ErrorMessage = "يجب اختيار المنتج")]
        [Display(Name = "المنتج / الدواء")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        [Display(Name = "الكمية")]
        public int Quantity { get; set; }
    }

    public class ProductPickerItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
    }
}
