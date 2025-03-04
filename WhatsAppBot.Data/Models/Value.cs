using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppBot.Data.Models
{
    public class Value
    {
        public int ad_id { get; set; }
        public long form_id { get; set; }

        public long leadgen_id { get; set; }

        public int created_time { get; set; }

        public long page_id { get; set; }

        public int adgroup_id { get; set; }

        public Messages[] messages { get; set; }
    }
}
