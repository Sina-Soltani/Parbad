using System.ComponentModel.DataAnnotations;

namespace Parbad.Sample.AspNetCore.Models
{
    public class PayViewModel
    {
        public long Amount { get; set; }

        [Display(Name = "Gateway")]
        public Gateway SelectedGateway { get; set; }
    }
}
