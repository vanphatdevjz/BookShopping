using System.ComponentModel.DataAnnotations;

namespace BookShoppingCartMvcUI.Models.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Tên Người Dùng")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Số Điện Thoại")]
        public string PhoneNumber { get; set; }
    }
}
