﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppBot.Data.Models.Config.Models;

namespace WhatsAppBot.Service.Services.WhatsApp
{
    public interface ITextMessageBuilder
    {
        object BuildPayload(string message);
    }

    public class TextMessageBuilder : ITextMessageBuilder
    {
        private readonly WhatsAppConfig _config;

        public TextMessageBuilder(WhatsAppConfig config)
        {
            _config = config;
        }

        public object BuildPayload(string message)
        {
            return new
            {
                messaging_product = "whatsapp",
                to = _config.PhoneNumber,
                type = "text",
                text = new { body = message }
            };
        }
    }
}
