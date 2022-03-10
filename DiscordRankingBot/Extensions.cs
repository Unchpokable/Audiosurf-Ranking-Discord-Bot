using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRankingBot
{
    public static class Extensions
    {
        public static bool Contains<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var item in source)
                if (predicate(item)) return true;
            return false;
        }
    }
}
