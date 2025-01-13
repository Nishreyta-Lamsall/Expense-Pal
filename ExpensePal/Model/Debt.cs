using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensePal.Model
{
    public class Debt
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Due Date is required")]
        public DateTime DueDate { get; set; } = DateTime.MinValue;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string Notes { get; set; }

        public string Source { get; set; }

        public string Tags { get; set; }
    }
}
