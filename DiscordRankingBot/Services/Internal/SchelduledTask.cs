using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordRankingBot.Services.Internal
{
    class SchelduledTask
    {
        public Action Task
        {
            get => _task;
            set
            {
                if (IsActive)
                    throw new Exception("Tried to enscheldule other job when current job is active");
            }
        }
        public DateTime Created { get; private set; }
        public DateTime LastAwake { get; private set; }

        /// <summary>
        /// Interval in millisecond, specifying time interval that SchelduleDescriptor should check time trigger to run
        /// </summary>
        public TimeSpan Interval { get; set; }
        public bool IsActive { get; private set; }

        private System.Timers.Timer _timer;
        private Action _task;
        private Func<DateTime, bool> _trigger;

        public SchelduledTask(TimeSpan interval, Action task, Func<DateTime, bool> timeTrigger, bool awakeNow = true)
        {
            Interval = interval;
            Task = task;
            Created = DateTime.Now;

            _timer = new System.Timers.Timer();
            _timer.Interval = interval.TotalMilliseconds;
            _timer.Elapsed += OnElapsed;
        }

        public void Awake()
        {
            _timer.Start();
            LastAwake = DateTime.Now;
            IsActive = true;
        }

        public void Asleep()
        {
            _timer.Stop();
            IsActive = false;
        }

        private async void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_trigger(DateTime.Now))
            {
                LastAwake = DateTime.Now;
                await System.Threading.Tasks.Task.Run(() =>
                {
                    Task.Invoke();
                });
            }
        }
    }
}
