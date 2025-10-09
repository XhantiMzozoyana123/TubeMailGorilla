using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Entities
{
    public class Icebreaker : BaseEntity
    {
        public int EmailerId { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
