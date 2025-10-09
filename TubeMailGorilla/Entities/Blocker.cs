using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Entities
{
    public class Blocker : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
    }
}
