using System;
using System.ComponentModel;
using System.Timers;

namespace Henooh.Framework.Utility
{
    /// <summary>
    /// HenoohTimer provides the Timer component with Countdown component that shows TimeSpan of countdown.
    /// </summary>
    /// <remarks>
    /// Henooh Timer is a timer based on interval basis.
    /// </remarks>
    /// <revisionhistory>
    /// YYYY-MM-DD  AS#####  v#.##.##.###  Change Description
    /// ==========  =======  ============  ============================================================================
    /// 2019-12-11  AS01259  v1.00.00.035  Initial Version
    /// 2019-12-12  AS01260  v1.00.00.036  Implement the Tick of the Timer
    /// 2020-02-27  AS01287  v1.02.00.005  Port HenoohTimer from HenoohUtility to Henooh.Framework.Utility
    /// 2020-03-13  AS01293  v1.02.00.007  Add XML comments in summary and remarks
    /// 2020-03-15  AS01294  v1.02.01.000  Remove unused using statements
    /// </revisionhistory>
    public class HenoohTimer : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HenoohTimer"/> class.
        /// </summary>
        public HenoohTimer()
        {
            Timer = new Timer(20);
            Timer.Elapsed += UpdateHenoohTimer;
        }

        /// <summary>
        /// Occurs when the timer interval has elasped.
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        /// Provides INotifyPropertyChanged implementation according to Bindings.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the period of time between timer ticks.
        /// </summary>
        public TimeSpan Interval { get; set; }
        /// <summary>
        /// Gets or sets a value that indicates whether the timer is running.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Internal timer that updates remaining time until the next trigger.
        /// </summary>
        Timer Timer { get; set; }
        /// <summary>
        /// Internal timer that keeps track of when the last trigger was set.
        /// </summary>
        DateTime LastTriggeredTime { get; set; }

        private TimeSpan countdownUntilNextInterval;
        /// <summary>
        /// Displays the Remaining time until the next interval.
        /// </summary>
        public TimeSpan CountdownUntilNextInterval
        {
            get
            {
                return countdownUntilNextInterval;
            }

            private set
            {
                countdownUntilNextInterval = value;
                NotifyPropertyChanged("CountdownUntilNextInterval");
            }
        }

        /// <summary>
        /// Notifies that a property has changed.
        /// </summary>
        /// <param name="aInfo"></param>
        private void NotifyPropertyChanged(string aInfo)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aInfo));
        }

        /// <summary>
        /// Starts the <see cref="HenoohTimer"/>.
        /// </summary>
        public void Start()
        {
            Timer.Start();
            IsEnabled = true;
            LastTriggeredTime = DateTime.Now;
        }

        /// <summary>
        /// Stops the <see cref="HenoohTimer"/>.
        /// </summary>
        public void Stop()
        {
            Timer.Stop();
            IsEnabled = false;
        }

        /// <summary>
        /// Updates the property values for the Timer class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateHenoohTimer(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            CountdownUntilNextInterval = LastTriggeredTime + Interval - currentTime;

            if (currentTime > LastTriggeredTime + Interval)
            {
                LastTriggeredTime += Interval;
                Tick?.Invoke(this, e);
            }
        }
    }
}
