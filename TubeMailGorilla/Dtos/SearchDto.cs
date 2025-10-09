using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeMailGorilla.Dtos
{
    public class SearchDto
    {
        public string Keywords { get; set; } = string.Empty;

        public int PageViewLimit { get; set; }

        public int NumberofChannelsVideos { get; set; }

        public int NumberofViewCount { get; set; }

        public int NumberofLikeCount { get; set; }

        public bool AddressRecipientByEmailUserName { get; set; }

        public bool ReturnGmailAccountOnly { get; set; }

        public bool ReturnValidatedEmails { get; set; }

        public bool AccelerateMode { get; set; }

        public bool HttpMode { get; set; }

        public bool TestMode { get; set; }
    }
}
