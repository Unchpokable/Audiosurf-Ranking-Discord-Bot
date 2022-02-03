using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventScheduler;
using EventScheduler.Events;
using EventScheduler.Service;

namespace DiscordRankingBot.Services.Internal
{
    class CountedEvent : ScheduledEventBase
    {
        public override DateTime ScheduledTime { get ; protected set ; }

        private int _repeats;
        private TimeSpan _period;

        public CountedEvent(TimeSpan period, Action action, int repeats)
        {
            if (repeats <= 0) throw new Exception("Repeats count can not be a negative number or zero");

            _repeats = repeats;
            _action = action;
            _period = period;
            ScheduledTime = DateTime.Now + period;
        }

        public override void Trigger(IEventScheduler scheduler)
        {
            base.Trigger(scheduler);
            --_repeats;
            ScheduledTime = DateTime.Now + _period;

            if (_repeats > 0)
                scheduler.Schedule(this);
        }
    }
}
