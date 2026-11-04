using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Models
{
    public class Layers
    {
        public decimal Id { get; set; }
        public decimal? LayerId { get; set; }
        public decimal? UserId { get; set; }

        public decimal? Priority { get; set; }
        public string? Layer { get; set; }
        public bool? IsOpen { get; set; }

        public decimal? MinZoom { get; set; }
        public decimal? MaxZoom { get; set; }

        public decimal? GroupId { get; set; }

        public string? Group { get; set; }

        public string? Tag { get; set; }
    }
}
