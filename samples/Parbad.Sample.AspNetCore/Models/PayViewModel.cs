using System.ComponentModel.DataAnnotations;

namespace Parbad.Sample.AspNetCore.Models
{
    public class PayViewModel
    {
        [Display(Name = "Tracking number")]
        public long TrackingNumber { get; set; }

        [Display(Name = "Generate the Tracking number automatically?")]
        public bool GenerateTrackingNumberAutomatically { get; set; } = true;

        public long Amount { get; set; }

        [Display(Name = "Gateway")]
        public Gateways SelectedGateway { get; set; }
    }
}
