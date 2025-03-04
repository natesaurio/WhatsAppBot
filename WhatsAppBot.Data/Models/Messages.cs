using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppBot.Data.Models
{
    public class Messages
    {
        public string id { get; set; }

        public string from { get; set; }

        public Text text { get; set; }
    }

}
