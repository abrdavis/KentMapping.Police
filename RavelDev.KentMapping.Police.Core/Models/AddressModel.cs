using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavelDev.KentMapping.Police.Core.Models
{
    public class AddressModel
    {
        public int AddressId { get; set; }
        public string StreetAddress { get; set; }
        public Point Coordinate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
