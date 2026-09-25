using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmacyIMS.ViewModels
{
    public class SupplierProductViewModel
    {
        public int SupplierProductID { get; set; }

        [Required(ErrorMessage = "المورد مطلوب")]
        [Display(Name = "المورد")]
        public int SupplierID { get; set; }

        public string? SupplierName { get; set; }

        [Required(ErrorMessage = "المنتج مطلوب")]
        [Display(Name = "المنتج")]
        public int ProductID { get; set; }

        public string? ProductName { get; set; }

        [StringLength(50, ErrorMessage = "رمز المنتج لا يمكن أن يزيد عن 50 حرف")]
        [Display(Name = "رمز المنتج لدى المورد")]
        public string? SupplierSKU { get; set; }

        [Required(ErrorMessage = "سعر التعاقد مطلوب")]
        [Range(typeof(decimal), "0.01", "9999999999999999",
            ErrorMessage = "سعر التعاقد يجب أن يكون أكبر من صفر")]
        [Display(Name = "سعر التعاقد")]
        public decimal ContractPrice { get; set; }

        [Range(0, 365, ErrorMessage = "مدة التوريد يجب أن تكون بين 0 و365 يوم")]
        [Display(Name = "مدة التوريد (أيام)")]
        public int LeadTimeDays { get; set; }

        public List<SelectListItem> AvailableProducts { get; set; } = new();
    }
}