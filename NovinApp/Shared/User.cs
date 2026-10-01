using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovinApp.Shared
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]
        [MaxLength(100, ErrorMessage = "تعداد کارکتر ورودی زیاد می باشد")]
        public string fname { get; set; }

        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]
        [MaxLength(20)]
        public string code_meli { get; set; }

        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]
        [MaxLength(50, ErrorMessage = "تعداد کارکتر ورودی زیاد می باشد")]
        public string gender { get; set; }

        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]
        [MaxLength(50, ErrorMessage = "تعداد کارکتر ورودی زیاد می باشد")]
        public string mobile { get; set; }
        public string? pic { get; set; }

        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]
        [MaxLength(100, ErrorMessage = "تعداد کارکتر ورودی زیاد می باشد")]
        public string pass { get; set; }
        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]
        [MaxLength(100, ErrorMessage = "تعداد کارکتر ورودی زیاد می باشد")]
        public string Rool { get; set; }
        public int? Id_Moshaver { get; set; }
        [MaxLength(100, ErrorMessage = "تعداد کارکتر ورودی زیاد می باشد")]
        public string? Moshaver { get; set; }
        public int? Id_School { get; set; }
        public bool active { get; set; } = true;
        [Timestamp]
        public byte[]? rowId { get; set; }
    }
}
