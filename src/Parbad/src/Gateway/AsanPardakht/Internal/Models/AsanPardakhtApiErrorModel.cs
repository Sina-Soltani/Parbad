using System;
using System.Collections.Generic;
using System.Text;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    internal class AsanPardakhtApiErrorModel
    {
        public string Type { get; set; }

        public string Title { get; set; }

        public int Status { get; set; }

        public string Detail { get; set; }

        public string Instance { get; set; }
    }
}
