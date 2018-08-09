using EasyNetQ;
using MindMurmur.Domain.Messages;
using System.Drawing;
using System.Reactive.Concurrency;
using System;
using System.Reactive.Subjects;
using MindMurmur.Lights.Utils;
using System.Collections.Generic;
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
        private Subject<MeditationState> meditationSubject = new Subject<MeditationState>();
        HeartRateMonitor bpmMonitor;
        MeditationMonitor meditationMonitor;
        ColorMonitor colorMonitor;
        private Timer heartRatePulseTimer;
        private float pulsetime;
        private DateTime lasttick;

        #endregion

        #region Public Properties

        public MeditationState CurrentMediationState { get; set; }
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
            meditationMonitor = new MeditationMonitor(dmx);
            // 25 FPS timer to update LEDs 
            //heartRatePulseTimer = new Timer(PulseTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }
        #endregion
        
        #region Timers
        
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
        #endregion 

        /// <summary>
        /// Starts the primary light processes
        /// </summary>
        /// <returns></returns>
        async public Task Start()
        {
            // Start DMX
            try
            {
                if (LightDMX != null)
                    await LightDMX.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: DMX controller not found. continuing...\r\n" + ex);
            }

            if (CurrentHeartRate < 20) CurrentHeartRate = 20;

            //bind it to the console
            meditationSubject.SubscribeConsole();
            bpmSubject.SubscribeConsole();
            colorSubject.SubscribeConsole();

            bpmSubject.Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Subscribe(bpmMonitor);

            colorSubject.Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Subscribe(colorMonitor);

            meditationSubject.Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Subscribe(meditationMonitor);

            //sets up bus connection and subscribes
            HitchToTheBus();

            // set pulse timer @ 25 FPS to update LEDs
            heartRatePulseTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(40));
        }

        /// <summary>
        /// Establishes all bus subscriptions to RabbitMQ
        /// </summary>
        public void HitchToTheBus()
        {
            var bus = RabbitHutch.CreateBus("host=localhost");

            // heartRateBlinkTimer.Change(5, GetHeartRateTimer(CurrentHeartRate));
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

            bus.Subscribe<MeditationStateCommand>("meditationStateCommand", (cmd) => {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"MeditationState.Rx {cmd.CommandId}");
                Console.ForegroundColor = ConsoleColor.White;
                var thisState = (MeditationState)cmd.State;

                CurrentMediationState = thisState;
                meditationSubject.OnNext(thisState);
            });
        }

        static int GetHeartRateTimer(int bpm)
        {
            return Convert.ToInt32((Convert.ToDouble(bpm) / 60.0)) * 1000;
        }

        public async Task RunTestsTask()
        {
            if (LightDMX != null)
                await LightDMX.Connect();

            //await LightDMX.TestSequence().ConfigureAwait(true);
            await LightDMX.Test().ConfigureAwait(true);
        }
    }
}
