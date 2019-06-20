using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL
{
    public class Randomizer
    {
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        public static int Next(int max)
        {
            //lock (syncLock)
            { // synchronize
                return random.Next(max);
            }
        }

        public static double NextDouble()
        {
            //lock (syncLock)
            { // synchronize
                return random.NextDouble();
            }
        }
    }
}
