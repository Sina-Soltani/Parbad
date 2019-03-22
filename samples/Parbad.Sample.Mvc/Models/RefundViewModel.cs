using System.ComponentModel.DataAnnotations;

namespace Parbad.Sample.Mvc.Models
{
    public class RefundViewModel
    {
        [Display(Name = "Tracking number")]
        public long TrackingNumber { get; set; }
    }
}
