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
        private Timer heartRatePulseTimer;
        private float pulsetime;
        private DateTime lasttick;

        #endregion

        #region Public Properties

        private int CurrentHeartRate { get; set; }
        public Color CurrentColor { get; set; }
        public DMX LightDMX { get; set; }

        #endregion

        #region ctor

        public LightManager(DMX dmx)
        {
            CurrentColor = Color.Aqua;
            CurrentHeartRate = 50;
            LightDMX = dmx;
            bpmMonitor = new HeartRateMonitor(dmx);
            colorMonitor = new ColorMonitor(dmx);
            // 25 FPS timer to update LEDs 
            heartRatePulseTimer = new Timer(PulseTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }
        #endregion


        /// <summary>
        /// timer event that initiates the lights according to the current color and pulse
        /// </summary>
        /// <param name="state"></param>
        private void PulseTimer_Tick(object state)
        {

            var deltatime = DateTime.UtcNow - lasttick;
            lasttick = DateTime.UtcNow;
            //move pulse timer from heartbeat rate
            pulsetime += (float)deltatime.TotalSeconds * (CurrentHeartRate / 60f);
            // restrict value between 0 and 1
            pulsetime -= (float)Math.Floor(pulsetime);

            // 0 = top | 0.5 = low | 1 = top
            int alpha = (int)(255 * (1 - Math.Sin(pulsetime * Math.PI)));
            Color fadedColor = Color.FromArgb(alpha,
                                              CurrentColor.R,
                                              CurrentColor.G,
                                              CurrentColor.B);

            byte[] dmx = this.LightDMX.GetDMXFromColors(new List<Color> { fadedColor });
            LightDMX.SendDMXFrames(dmx);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"LED COLOR: {fadedColor.ToString()}");
            Console.ForegroundColor = ConsoleColor.White;

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
            try
            {
                if (LightDMX != null)
                    await LightDMX.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: DMX controller not found. continuing...");
            }


            if (CurrentHeartRate < 20) CurrentHeartRate = 20;

            //bind it to the console
            //bpmSubject.SubscribeConsole();
            //colorSubject.SubscribeConsole();

            //bpmSubject.Throttle(TimeSpan.FromMilliseconds(300))
            //    .DistinctUntilChanged()
            //    .Subscribe(bpmMonitor);

            //colorSubject.Throttle(TimeSpan.FromMilliseconds(300))
            //    .DistinctUntilChanged()
            //    .Subscribe(colorMonitor);

            bus.Subscribe<HeartRateCommand>("heartRateCommand", (cmd) =>
            {
                if (CurrentHeartRate != cmd.HeartRate)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Heart.Rx {cmd.CommandId}");
                    Console.ForegroundColor = ConsoleColor.White;
                    CurrentHeartRate = cmd.HeartRate;
                    bpmSubject.OnNext(cmd.HeartRate);
                }
            });

            bus.Subscribe<ColorControlCommand>("colorCommand", (cmd) => {
                Color cmdColor = Color.FromArgb(cmd.ColorRed, cmd.ColorGreen, cmd.ColorBlue);
                if (CurrentColor != cmdColor)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"Color.Rx {cmd.CommandId}");
                    Console.ForegroundColor = ConsoleColor.White;
                    CurrentColor = cmdColor;
                    colorSubject.OnNext(cmdColor);
                }
            });

            // set pulse timer @ 25 FPS to update LEDs
            heartRatePulseTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(40));

        }

        static int GetHeartRateTimer(int bpm)
        {
            return Convert.ToInt32((Convert.ToDouble(bpm) / 60.0)) * 1000;
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
