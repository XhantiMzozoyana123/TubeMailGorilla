using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Dtos
{
    public class EmailerDto
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Keyword { get; set; } = string.Empty;
        public string Captions { get; set; } = string.Empty;
        public string FirstLine { get; set; } = string.Empty;
        public bool Checked { get; set; }
    }
}
