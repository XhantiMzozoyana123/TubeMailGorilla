using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Entities
{
    public class Commentor : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string ChanneUrl { get; set; } = string.Empty;
        public string OriginVideoUrl { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
    }
}
