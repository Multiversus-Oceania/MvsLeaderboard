using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rnd = System.Random;

namespace BasicBot.Handler
{
    public static class Random
    {
        private static Rnd rnd = new Rnd();

        public static bool RandomBool()
        {
            var rand = rnd.Next(0, 2);
            var _bool = false;

            if (rand == 1)
            {
                _bool = true;
            }

            return _bool;
        }

        public static ulong RandomUser(List<ulong> users)
        {
            return users[RandomInt(0,users.Count)];
        }

        public static int RandomInt(int min, int max)//might seem stupid to have it here, but i have a static random varaible here which will make it more random
        {
            return rnd.Next(min,max);
        }
    }
}
