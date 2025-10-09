using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Dtos
{
    public class MessengerDto
    {
        [Key]
        public int Id { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string EmailFrom { get; set; } = string.Empty;

        public string EmailTo { get; set; } = string.Empty;

        public bool Html { get; set; }

        public bool TestMode { get; set; }
    }
}
