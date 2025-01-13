using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensePal.Model
{
    public class Transaction
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; }

        public string Tags { get; set; }
    }
}
