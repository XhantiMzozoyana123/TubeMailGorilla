using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Entities
{
    public class Templates : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool Html { get; set; }
    }
}
