using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Dtos
{
    public class CommentorDto
    {
        public string Keyword { get; set; } = string.Empty;

        public int MaxResults { get; set; } = 100;

        public bool HttpMode { get; set; }
    }
}
