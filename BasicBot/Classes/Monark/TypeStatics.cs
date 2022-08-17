using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Webhook;
using System.Net;
using BasicBot.Handler;

namespace BasicBot.MonarkTypes
{
    public static class TypeStatics
    {

        public static ApplicationCommandOptionChoiceProperties CreateChoiceProperties(string name) => CreateChoiceProperties(name, name);
        
        public static ApplicationCommandOptionChoiceProperties CreateChoiceProperties(string name, string value)
        {
            var prop = new ApplicationCommandOptionChoiceProperties();
            prop.Name = name;
            prop.Value = value;

            return prop;
        }
    }
}
