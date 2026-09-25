using System.ComponentModel.DataAnnotations;

namespace PharmacyIMS.ViewModels
{
    public class SupplierViewModel
    {
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "اسم المورد مطلوب")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "اسم المورد يجب أن يكون بين 2 و150 حرف")]
        [Display(Name = "اسم المورد / الشركة")]
        public string SupplierName { get; set; } = string.Empty;


        [Required(ErrorMessage = "اسم مسؤول التواصل مطلوب")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "اسم مسؤول التواصل يجب أن يكون بين 2 و100 حرف")]
        [Display(Name = "اسم مسؤول التواصل")]
        public string ContactName { get; set; } = string.Empty;


        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(
            @"^(010|011|012|015)[0-9]{8}$",
            ErrorMessage = "رقم الهاتف يجب أن يكون رقمًا مصريًا صحيحًا مكونًا من 11 رقمًا")]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;


        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "العنوان مطلوب")]
        [StringLength(250, MinimumLength = 5,
            ErrorMessage = "العنوان يجب أن يكون بين 5 و250 حرف")]
        [Display(Name = "العنوان")]
        public string Address { get; set; } = string.Empty;
    }
}