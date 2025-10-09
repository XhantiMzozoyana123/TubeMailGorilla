using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Entities
{
    public class Captions : BaseEntity
    {
        public int EmailerId { get; set; }

        public string Text { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
