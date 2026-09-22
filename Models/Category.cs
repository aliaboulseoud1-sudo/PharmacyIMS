using System.ComponentModel.DataAnnotations;

namespace PharmacyIMS.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "اسم الفئة مطلوب")]
        [StringLength(100)]
        [Display(Name = "اسم الفئة")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
