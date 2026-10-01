using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovinApp.Shared.Login
{
    public class UserInfo
    {
        [Required(ErrorMessage = "نام کاربری الزامی می باشد")]
        public string Username { get; set; }

        [Required(ErrorMessage = "کلمه عبور الزامی می باشد")]
        public string Password { get; set; }

        //[Required(ErrorMessage = "کلمه عبور الزامی می باشد")]
        //public string Modle { get; set; }
    }
}