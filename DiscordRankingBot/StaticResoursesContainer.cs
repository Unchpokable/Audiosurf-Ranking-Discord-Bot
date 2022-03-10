using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventScheduler.Events;
using System.Data.SQLite;

namespace DiscordRankingBot
{
    class StaticResoursesContainer
    {
        public static Dictionary<string, IScheduledEvent> ActiveExams { get; set; }

        static StaticResoursesContainer()
        {
            ActiveExams = new Dictionary<string, IScheduledEvent>();
        }
    }
}
