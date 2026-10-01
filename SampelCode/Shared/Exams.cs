using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovinApp.Shared
{
    public class Exams
    {
        [Key]
        public int Id { get; set; }

        //کد دانش آموز
        public int IdUser { get; set; } = 0;
        //سوال
        public int Number { get; set; } = 0;
        public int IdQuestion { get; set; } = 0;

        // گزینه‌
        public string? selectedOption { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Status { get; set; } = string.Empty;

        public virtual User? User { get; set; }
        public virtual Question? Question { get; set; }
    }
}
