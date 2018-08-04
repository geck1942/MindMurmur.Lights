using EasyNetQ;
using MindMurmur.Domain.Messages;
using System.Drawing;
using System.Reactive.Concurrency;
using System;
using System.Reactive.Subjects;
using MindMurmur.Lights.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace MindMurmur.Lights
{
    public class LightManager
    {
        #region Private Properties

        private IScheduler scheduler = Scheduler.Default;
        private IObservable<int> HeartRate;
        private Subject<int> bpmSubject = new Subject<int>();
        private Subject<Color> colorSubject = new Subject<Color>();
        HeartRateMonitor bpmMonitor;
        ColorMonitor colorMonitor;
        private Timer heartRateBlinkTimer;

        #endregion

        #region Public Properties

        private int CurrentHeartRate { get; set; }
        public Color CurrentColor { get; set; }
        public DMX LightDMX { get; set; }

        #endregion

        #region ctor

        public LightManager(DMX dmx) {
            CurrentColor = Color.Aqua;
            CurrentHeartRate = 50;
            LightDMX = dmx;
            bpmMonitor = new HeartRateMonitor(dmx);
            colorMonitor = new ColorMonitor(dmx);
            heartRateBlinkTimer = new Timer(HeartRateBlinkTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }
        #endregion

        /// <summary>
        /// timer event that initiates the lights blinking at the same rate of the heartbeat
        /// </summary>
        /// <param name="state"></param>
        private void HeartRateBlinkTimer_Tick(object state)
        {
            Debug.WriteLine("Tick");
            if (this.LightDMX.IsConnected)
                LightDMX.HeartRateBlink(CurrentColor);
        }

        //public void StartTimerInOneMinute()
        //{
        //    Observable
        //        .Timer(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(5))
        //        .Subscribe(
        //            x =>
        //            {
        //                // Do Stuff Here
        //                Console.WriteLine(x);
        //                // Console WriteLine Prints
        //                // ...
        //            });
        //}

        /// <summary>
        /// Starts the primary light processes
        /// </summary>
        /// <returns></returns>
        async public Task Start()
        {
            var bus = RabbitHutch.CreateBus("host=localhost");
            // Start DMX
            if (LightDMX != null)
                await LightDMX.Connect();

            if (CurrentHeartRate < 20) CurrentHeartRate = 20;

            //bind it to the console
            bpmSubject.SubscribeConsole();
            colorSubject.SubscribeConsole();

            bpmSubject.Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Subscribe(bpmMonitor);

            colorSubject.Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Subscribe(colorMonitor);

            heartRateBlinkTimer.Change(5, GetHeartRateTimer(CurrentHeartRate));
            bus.Subscribe<HeartRateCommand>("heartRateCommand", (cmd) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Heart.Rx {cmd.CommandId}");
                Console.ForegroundColor = ConsoleColor.White;
                bpmSubject.OnNext(cmd.HeartRate);
                CurrentHeartRate = cmd.HeartRate;
                //change the heart beat timer
                heartRateBlinkTimer.Change(5, GetHeartRateTimer(CurrentHeartRate));
            });
            
            bus.Subscribe<ColorControlCommand>("colorCommand", (cmd) => {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Color.Rx {cmd.CommandId}");
                Console.ForegroundColor = ConsoleColor.White;
                var thisColor = Color.FromArgb(cmd.ColorRed, cmd.ColorGreen, cmd.ColorBlue);

                //if we have a new color, then we want to send this directly to the lights
                if (this.LightDMX.IsConnected && thisColor!=CurrentColor)
                {
                    byte[] dmx = this.LightDMX.GetDMXFromColors(new List<Color> { thisColor });
                    LightDMX.SendDMXFrames(dmx);
                }
                CurrentColor = thisColor;
                colorSubject.OnNext(thisColor);
            });
        }

        static int GetHeartRateTimer(int bpm)
        {
            return Convert.ToInt32((Convert.ToDouble(bpm) / 60.0))*1000;
        }

        public async Task RunTestsTask()
        {
            if (LightDMX != null)
                await LightDMX.Connect();

            await LightDMX.Test().ConfigureAwait(true);
        }

        //async private void MainTimer_Tick(object State)
            //{
            //    if (CurrentHeartRate < 20) CurrentHeartRate = 20;

            //    //mainTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //    /// LIGHTS
            //    /// Run the visualization
            //    /// And send the colors to DMX. (unless stopped)
            //    try
            //    {
            //        if (this.LightDMX.IsConnected)
            //        {
            //            byte[] dmx = this.LightDMX.GetDMXFromColors(new List<Color> { CurrentColor });
            //            LightDMX.SendDMXFrames(dmx);
            //        }
            //    }
            //    catch (Exception TimerException)
            //    {
            //        Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[!] Error during MainTimer");
            //        Console.ForegroundColor = ConsoleColor.White; Console.WriteLine(TimerException.ToString());
            //    }
            //   // mainTimer.Change(40, 40);
            //}

            //private void HandleColorControlCommand(ColorControlCommand cmd)
            //{
            //    CurrentColor = Color.FromArgb(cmd.ColorRed, cmd.ColorGreen, cmd.ColorBlue);
            //   // ColorQueue.Enqueue(cmd);
            //}
            //private void HandleHeartRateCommand(HeartRateCommand cmd)
            //{
            //    CurrentHeartRate = cmd.HeartRate;
            //    //HeartRateQueue.Enqueue(cmd);
            //}
        }
}
