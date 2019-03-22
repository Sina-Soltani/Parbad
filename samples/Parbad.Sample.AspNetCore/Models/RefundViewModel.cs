using System.ComponentModel.DataAnnotations;

namespace Parbad.Sample.AspNetCore.Models
{
    public class RefundViewModel
    {
        [Display(Name = "Tracking number")]
        public long TrackingNumber { get; set; }
    }
}
