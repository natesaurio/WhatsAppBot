using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppBot.Data.Models.Prompts
{
    public class Prompt
    {
        public string Message { get; set; }
        public PromptRules Rules { get; set; }
    }
}
