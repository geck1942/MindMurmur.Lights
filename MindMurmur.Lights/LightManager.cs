using EasyNetQ;
using MindMurmur.Domain.Messages;
using System.Drawing;
using System.Reactive.Concurrency;
using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reactive.Subjects;
using MindMurmur.Lights.Utils;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using MindMurmur.Domain;
using MindMurmur.Lights.Control;

namespace MindMurmur.Lights
{
    public class LightManager
    {
        #region Private Properties
        static object lockObj = new object();
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
        private DateTime lastStateChange = DateTime.UtcNow;
        Bitmap fromTransitionBitmap = new Bitmap(51, 2);
        Bitmap toTransitionBitmap = new Bitmap(51, 2);
        private int currentHeartRate = 20;

        private double secondsFromLastTransition
        {
            get { return (DateTime.UtcNow - lastStateChange).TotalSeconds; }
        }

        #endregion

        #region Public Properties

        public MeditationState CurrentMediationState { get; set; }

        private int CurrentHeartRate
        {
            get { return (secondsFromLastTransition > 15.0) ? currentHeartRate / 2 : currentHeartRate; }
            set { currentHeartRate = value; }
        }
        public Color PreviousColor { get; set; }
        public Color CurrentColor { get; set; }
        public Color CurrentSecondaryColor { get; set; }
        public Color CurrentTertiaryColor { get; set; }
        public DMX LightDMX { get; set; }

        #endregion

        #region ctor

        public LightManager(DMX dmx)
        {
            PreviousColor = Color.Blue;
            CurrentColor = Color.Aqua;
            CurrentSecondaryColor = Color.LightSlateGray;
            CurrentTertiaryColor = Color.HotPink;
            CurrentHeartRate = 50;
            LightDMX = dmx;
            bpmMonitor = new HeartRateMonitor(dmx);
            colorMonitor = new ColorMonitor(dmx);
            meditationMonitor = new MeditationMonitor(dmx);
            BuildGradientTransition();
            // 25 FPS timer to update LEDs 
            heartRatePulseTimer = new Timer(PulseTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }
        #endregion
        
        #region Timers

        /// <summary>
        /// Builds two gradient graphic objects that are use to get a pixel color for transitioning between one color to another
        /// </summary>
        private void BuildGradientTransition()
        {
            lock (lockObj)
            {
                using (var graphics = Graphics.FromImage(fromTransitionBitmap))
                {
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(fromTransitionBitmap.Width, fromTransitionBitmap.Height),
                        PreviousColor,
                        Color.White))
                    {
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                        fromTransitionBitmap.Save(Directory.GetCurrentDirectory() + "fromTransitionBitmap.bmp");
                    }
                }
                using (var graphics = Graphics.FromImage(toTransitionBitmap))
                {
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(toTransitionBitmap.Width, toTransitionBitmap.Height),
                        Color.White, CurrentColor))
                    {
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                        toTransitionBitmap.Save(Directory.GetCurrentDirectory() + "toTransitionBitmap.bmp");
                    }
                }
            }
        }

        /// <summary>
        /// Returns the current color from the ascending or descending transition color scheme
        /// </summary>
        /// <returns></returns>
        private Color GetCurrentColor()
        {
            var seconds = secondsFromLastTransition;
            Color rtn = CurrentColor;
            if (seconds > 5 && seconds <= 10) rtn = Color.White;
            if (seconds <= 5)
            {
                lock (lockObj)
                {
                    rtn = fromTransitionBitmap.GetPixel(Convert.ToInt32(seconds) * 10, 1);
                }
            }
            if (!(seconds > 10) || !(seconds < 15)) return rtn;
            lock (lockObj)
            {
                rtn = toTransitionBitmap.GetPixel(Convert.ToInt32(seconds - 10.0) * 10, 1);
            }
            return rtn;
        }

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
            var color = GetCurrentColor();

            // 0 = top | 0.5 = low | 1 = top
            int alpha = (int)(255 * (1 - Math.Sin(pulsetime * Math.PI)));
            Color fadedColor = Color.FromArgb(alpha,
                color.R,
                color.G,
                color.B);

            //byte[] dmx = this.LightDMX.GetDMXFromColors(new List<Color> { fadedColor });
            //LightDMX.SendDMXFrames(dmx);
            LightDMX.SetEdgeLightStrips(fadedColor);
            
            Console.WriteLine($"LED COLOR: {fadedColor.ToString()}");
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
                    Console.WriteLine($"Heart.Rx {cmd.CommandId} [{cmd.HeartRate}]");
                    CurrentHeartRate = cmd.HeartRate/2;//cutting the heart rate in half
                    bpmSubject.OnNext(cmd.HeartRate);
                }
            });

            bus.Subscribe<ColorControlCommand>("colorCommand", (cmd) => {
                Color cmdColor = Color.FromArgb(cmd.ColorRed, cmd.ColorGreen, cmd.ColorBlue);
                if (CurrentColor != cmdColor)
                {
                    Console.WriteLine($"Color.Rx {cmd.CommandId} [{cmd.ColorRed},{cmd.ColorGreen},{cmd.ColorBlue}]");
                    CurrentColor = cmdColor;
                    colorSubject.OnNext(cmdColor);
                }
            });

            bus.Subscribe<MeditationStateCommand>("meditationStateCommand", (cmd) => {
                Console.WriteLine($"MeditationState.Rx {cmd.CommandId} [{cmd.State}]");
                var thisState = (MeditationState)cmd.State;
                lastStateChange = DateTime.UtcNow;
                PreviousColor = CurrentColor;

                CurrentColor = Config.MeditationColors[thisState].Item1; //sets the primary color from the meditation state
                CurrentSecondaryColor = Config.MeditationColors[thisState].Item2; //sets the secondary color from the meditation state
                CurrentTertiaryColor = Config.MeditationColors[thisState].Item3; //sets the tertiary color from the meditation state
                CurrentMediationState = thisState;

                meditationSubject.OnNext(thisState);

                BuildGradientTransition();//build the bitmap so we can get the color for transitioning
            });
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
