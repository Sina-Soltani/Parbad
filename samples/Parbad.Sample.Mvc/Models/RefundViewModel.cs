using System.ComponentModel.DataAnnotations;

namespace Parbad.Sample.Mvc.Models
{
    public class RefundViewModel
    {
        [Display(Name = "Order number")]
        public long OrderNumber { get; set; }

        public long Amount { get; set; }
    }
}