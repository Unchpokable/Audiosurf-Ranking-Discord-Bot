using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRankingBot.Services.Internal;
using EventScheduler;
using EventScheduler.Events;
using EventScheduler.Service;

namespace DiscordRankingBot.Services
{
    public class Scheduler
    {
        private IEventScheduler _scheduler;
        private List<IScheduledEvent> _events;

        public Scheduler()
        {
            _scheduler = new EventSchedulerService();
            _events = new List<IScheduledEvent>();
        }

        public IScheduledEvent NewTask(Action action, TimeSpan timespan, int repeats)
        {
            var scheduledEvent = new CountedEvent(timespan, action, repeats);
            _scheduler.Schedule(scheduledEvent);
            _events.Add(scheduledEvent);
            return scheduledEvent;
        }

        public bool CancelTask(IScheduledEvent task)
        {
            return _scheduler.CancelEvent(task);
        }
    }
}
