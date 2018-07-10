using System.ComponentModel.DataAnnotations;
using Parbad.Providers;

namespace Parbad.Sample.Mvc.Models
{
    public class RequestViewModel
    {
        [Display(Name = "Order number")]
        public long OrderNumber { get; set; }

        public long Amount { get; set; }

        public Gateway Gateway { get; set; }
    }
}