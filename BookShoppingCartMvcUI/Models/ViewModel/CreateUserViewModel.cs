using System.ComponentModel.DataAnnotations;

namespace BookShoppingCartMvcUI.Models.ViewModels
{
    public class CreateUserViewModel
    {
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

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật Khẩu")]
        public string Password { get; set; }
    }
}
