using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Entities
{
    public class Credientals : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }   
        public string ImapHost { get; set; } = string.Empty;
        public int ImapPost { get; set; }
        public bool StmpSsl { get; set; }
        public bool ImapSsl { get; set; }
    }
}
