using System.ComponentModel.DataAnnotations;

namespace Parbad.Sample.Mvc.Models
{
    public class RequestViewModel
    {
        [Display(Name = "Tracking number")]
        public long TrackingNumber { get; set; }

        [Display(Name = "Generate the Tracking number automatically?")]
        public bool GenerateTrackingNumberAutomatically { get; set; } = true;

        public long Amount { get; set; }

        public Gateways Gateway { get; set; }
    }
}
