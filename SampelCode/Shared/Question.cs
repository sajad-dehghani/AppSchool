using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovinApp.Shared
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]
        public int NumberOfOptions { get; set; } = 1;
        [MaxLength(100, ErrorMessage = "تعداد کارکتر ورودی زیاد می باشد")]
        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]

        public string? Groups { get; set; } = string.Empty;

        [Required(ErrorMessage = "تکمیل این مورد ضروری می باشد")]
        public string QuestionText { get; set; } = null!;

        [MaxLength(50, ErrorMessage = "تعداد کارکتر ورودی زیاد می باشد")]
        public string? Gender_specific { get; set; } ="هر دو گروه";
        public string? Tags { get; set; } = string.Empty;

        public string? Memo { get; set; }

        // گزینه‌ها به صورت فیلدهای مجزا
        public string? Option1Text { get; set; } = string.Empty;
        public decimal Option1Score { get; set; }  = decimal.Zero;  
        public bool? Option1IsCorrect { get; set; }=false;

        public string? Option2Text { get; set; } = string.Empty;
        public decimal Option2Score { get; set; } = decimal.Zero;
        public bool? Option2IsCorrect { get; set; } = false;

        public string? Option3Text { get; set; } = string.Empty;
        public decimal Option3Score { get; set; } = decimal.Zero;
        public bool? Option3IsCorrect { get; set; } = false;

        public string?   Option4Text { get; set; } = string.Empty;
        public decimal Option4Score { get; set; } = decimal.Zero;
        public bool? Option4IsCorrect { get; set; } = false;

        public string? Option5Text { get; set; } = string.Empty;
        public decimal Option5Score { get; set; } = decimal.Zero;
        public bool? Option5IsCorrect { get; set; } = false;

        public string? Option6Text { get; set; } = string.Empty;
        public decimal Option6Score { get; set; } = decimal.Zero;
        public bool? Option6IsCorrect { get; set; } = false;

    }

}
