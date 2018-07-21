using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Domain.EEG
{
    public class Scene
    {
        public List<Pole> Poles { get; set; }

        private Dictionary<string, object> _visualizationParams;
        internal string CurrentVisualization { get; private set; }

        public void SetVisualization(string newVisualization)
        {
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[ ] Starting Visualization " + newVisualization);

            this.CurrentVisualization = newVisualization;
            _visualizationParams.Clear();
            switch (CurrentVisualization)
            {
                case "ritual_b":
                    _visualizationParams.Add("nextpulse", DateTime.MinValue);
                    _visualizationParams.Add("startpulse", DateTime.MinValue);
                    break;
                case "ritual_c":
                    _visualizationParams.Add("startfade", DateTime.MinValue);
                    _visualizationParams.Add("endfade", DateTime.MinValue);
                    _visualizationParams.Add("startcolor", Color.Black);
                    break;
                case "welcome":
                    _visualizationParams.Add("startfade", DateTime.MinValue);
                    _visualizationParams.Add("endfade", DateTime.MinValue);
                    break;
                case "stop":
                    _visualizationParams.Add("stopped", false);
                    break;
                case "sleep":
                    _visualizationParams.Add("last_min23_index", -1);
                    _visualizationParams.Add("stopped", false);
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("[-] Success");
        }

        public Scene()
        {
            Poles = new List<Pole>();
            _visualizationParams = new Dictionary<string, object>();
        }

        async public Task LoadConfig()
        {
            LoadPoles();
            //await LoadMusic();
        }

        private void LoadPoles()
        {
            Poles.Clear();
            Poles.Add(new Pole()
            {
                Name = "A",
                Depth = 152.4d, // 5'
                Height = 396.24d, // 13'
                HorizontalPosition = -195.072d, // -6.4'
                Radius = 38.1d // 15"
            });
            Poles.Add(new Pole()
            {
                Name = "B",
                Depth = 0, // 0'
                Height = 403.86d, // 13.25'
                HorizontalPosition = -76.2d, // -2.5'
                Radius = 38.1d // 15"
            });
            Poles.Add(new Pole()
            {
                Name = "C",
                Depth = 91.44d, // 3'
                Height = 402.336d, // 13.2'
                HorizontalPosition = -30.48d, // -1'
                Radius = 38.1d // 15"
            });
            Poles.Add(new Pole()
            {
                Name = "D",
                Depth = 60.96, // 2'
                Height = 335.28d, // 11'
                HorizontalPosition = 36.576d, // 1.2'
                Radius = 38.1d // 15"
            });
            Poles.Add(new Pole()
            {
                Name = "E",
                Depth = 121.92d, // 4'
                Height = 283.464d, // 9.3'
                HorizontalPosition = 121.92d, // 4'
                Radius = 38.1d // 15"
            });
            Poles.Add(new Pole()
            {
                Name = "F",
                Depth = 30.48d, // 1'
                Height = 274.32d, // 9'
                HorizontalPosition = 152.4d, // 5'
                Radius = 38.1d // 15"
            });
            Poles.Add(new Pole()
            {
                Name = "G",
                Depth = 182.88d, // 6'
                Height = 384.048, // 12.6'
                HorizontalPosition = 201.168d, // 6.6'
                Radius = 38.1d // 15"
            });
            foreach (var pole in Poles)
            {
                pole.SetLEDS(5.08d);
            }

            var allLEDs = Poles.SelectMany(pole => pole.LEDS);

            //calculate the bounds of the screen made out of all the lights.
            var left = allLEDs.Min(led => led.GetWorldPosition().X);
            var right = allLEDs.Max(led => led.GetWorldPosition().X);
            var top = allLEDs.Max(led => led.GetWorldPosition().Y);
            var bottom = allLEDs.Min(led => led.GetWorldPosition().Y);
            PolesBounds = Rectangle.FromLTRB(left, top, right, bottom);
        }

        public void UserTalking(string Source, object eqValues)
        {
            if (CurrentVisualization == "sleep")
            {
                SetVisualization("welcome");
            }

        }
        public void UserSleep(string Source, object eqValues)
        {
            if (CurrentVisualization == "equalizer")
            {
                SetVisualization("sleep");
            }
        }

        async internal Task SetLightsFromVisualization()
        {
            long timestamp = (long)DateTime.Now.TimeOfDay.TotalMilliseconds;
            var alllights = Poles.SelectMany(p => p.LEDS);
            if (CurrentVisualization == "stop")
            {
                #region STOP
                var stopped = (bool)this._visualizationParams["stopped"];
                if (!stopped)
                {
                    // set all leds to black and do nothing else again.
                    foreach (var light in alllights)
                        light.Color = Color.FromArgb(255, 0, 0, 0);
                    _visualizationParams["stopped"] = true;
                }
                #endregion
            }
            if (CurrentVisualization == "ritual_a")
            {
                #region RITUAL A (very slow pulse to fast)
                float timestamptonextevent = (24 * 3600 * 1000) - (long)DateTime.Now.TimeOfDay.TotalMilliseconds; // milliseconds

                float maxpulseperiod = 16000;
                float minpulseperiod = 160;

                float currentperiod = Utils.Formulas.Clamp(timestamptonextevent, 0, 20 * 60 * 1000, minpulseperiod, maxpulseperiod, false);
                // ritual frequency:
                float frequency = 1 / currentperiod;

                // period is a sine curve, from 0 to loopduration.  y = [0-1] is light intensity
                double intensity = (Math.Sin((double)(timestamptonextevent * 2 * (float)Math.PI * frequency))) / 2d + 0.5f;
                var red = Color.FromArgb(255, (byte)(255 * intensity), 0, 0);
                foreach (var light in alllights)
                    light.Color = red;

                Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine("    Color: " + red);
                #endregion
            }
            else if (CurrentVisualization == "test")
            {
                #region TEST
                int step = 0;
                Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Pink };
                foreach (var pole in Poles)
                {
                    foreach (var light in pole.LEDS)
                    {
                        light.Color = colors[step];
                    }
                    step++;
                }
                #endregion
            }
            else if (CurrentVisualization == "sleep")
            {
                int last_min23_index = (int)this._visualizationParams["last_min23_index"];
                // 5 minutes every 23 minutes;
                long min23_duration = (long)(7 * 60 * 1000);
                int min23_index = (int)(timestamp / min23_duration);
                decimal min23_pct = (timestamp % min23_duration) / (decimal)min23_duration;
                bool active = min23_pct < (5 / 7M);

                if (!active)
                {
                    var stopped = (bool)this._visualizationParams["stopped"];
                    if (!stopped)
                    {
                        // set all leds to black and do nothing else again.
                        foreach (var light in alllights)
                            light.Color = Color.FromArgb((int)(255 * 0.70f), 255, 0, 0);
                        _visualizationParams["stopped"] = true;
                    }
                    return;
                }
                string[] SLEEP_VIZ = new string[] { "classicrainbow","count", "rainbow" };
                int viz_index = min23_index % SLEEP_VIZ.Length;
                string viz_name = SLEEP_VIZ[viz_index];
                _visualizationParams["stopped"] = false;
                switch (viz_name)
                {
                    case "rainbow":
                        #region rainbow
                        decimal rainbowloopduration2 = 9810; // timestamp in milliseconds
                        decimal rainbowloopnow2 = timestamp % rainbowloopduration2;
                        decimal rainbowlooppct2 = rainbowloopnow2 / rainbowloopduration2;
                        decimal step2 = 0;
                        foreach (var pole in Poles)
                        {
                            long lightrainbowangle2 = (long)((((step2 / 7M) + (decimal)rainbowlooppct2) ) * 360) % 360;
                            var rainbowcolor2 = HSLColor.ColorFromAhsb(255, lightrainbowangle2, 1, 0.5f);
                            foreach (var light in pole.LEDS)
                            {
                                //double lightXrelativePosition = (light.GetWorldPosition().X - PolesBounds.Left) / PolesBounds.Width;
                                light.Color = rainbowcolor2;
                            }
                            step2++;
                        }
                        #endregion
                        break;
                    case "count":
                        #region count
                        Color[] digits = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Magenta };
                        long seconds_past_18 = (long)(DateTime.Now - TimeSpan.FromHours(18)).TimeOfDay.TotalMilliseconds / 52;
                        for (int i = 0; i < Poles.Count; i++)
                        {
                            long digit7 = ((int)(seconds_past_18/ Math.Pow(7, i)) % 7);
                            var pole = Poles[Poles.Count - 1 - i];
                            //long pow = (long)Math.Pow(2, i);
                            //if ((pow & seconds_past_18) > 0)
                            //{
                                foreach (var light in pole.LEDS)
                                {
                                    light.Color = digits[digit7];
                                }
                            //}
                        }
                        #endregion
                        break;
                    case "equalizer":
                        #region EQUALIZER
                        var values = Equalizers.First(eq => eq.Label == "Aux").GetVisualValues().ToArray();
                        var @outlevel = values.Average() * 20; // Equalizers.First(eq => eq.Label == "Out").GetVisualValues().Average();
                        float maxpoint = values.Max();
                        var eqcolor = Color.Black;
                        if (@outlevel >= (1 / 5f))
                        {
                            int highestindex = 0;
                            for (var i = 0; i < values.Length; i++)
                            {
                                if (values[i] == maxpoint)
                                    break;
                                highestindex++;
                            }
                            // from 0 to 6
                            float pitch = highestindex / (float)(values.Length - 1);
                            int red = Math.Max(0, Math.Min((int)(pitch * 255), 255));
                            int gre = 255 - red;// Math.Max(0, Math.Min((int)((result / 7) * 255), 255));
                                                // from 4200 (high sound) to 8000 ( low low sound);
                            int alpha = (int)Math.Max(0, Math.Min(outlevel * 255, 255));// Math.Max(0, Math.Min((int)((values.Sum() - 8400) / (16000 - 8400)), 255));

                            eqcolor = Color.FromArgb(alpha, red, gre, 0);
                        }
                        else
                            eqcolor = Color.Black;
                        for (int i = 0; i < Poles.Count; i++)
                        {
                            var bandpole = Poles.ElementAt(i);
                            foreach (var light in bandpole.LEDS)
                            {
                                light.Color = eqcolor;
                            }
                        }
                        #endregion
                        break;
                }


            }
            else if (CurrentVisualization == "random")
            {
                #region RANDOM
                var ran = new Random();
                foreach (var light in alllights)
                {
                    light.Color = Color.FromArgb(255, (byte)ran.Next(0, 256), (byte)ran.Next(0, 256), (byte)ran.Next(0, 256));
                }
                #endregion
            }
            else if (CurrentVisualization == "welcome")
            {
                #region WELCOME
                DateTime endfade = (DateTime)this._visualizationParams["endfade"];
                DateTime startfade = (DateTime)this._visualizationParams["startfade"];
                var green = Color.FromArgb(255, 0, 255, 0);

                if (endfade == DateTime.MinValue)
                {
                    startfade = DateTime.Now;
                    endfade = DateTime.Now + TimeSpan.FromMilliseconds(1000);
                }
                else if (DateTime.Now < endfade)
                {
                    float fadepct = (float)(DateTime.Now - startfade).TotalMilliseconds / 1000f;
                    var actual = Color.FromArgb(255, 0, (byte)(255 * fadepct), 0);
                    foreach (var light in alllights)
                        light.Color = actual;
                }
                else
                {
                    SetVisualization("equalizer");
                }
                Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine("    Color: " + green);
                this._visualizationParams["endfade"] = endfade;
                this._visualizationParams["startfade"] = startfade;
                #endregion
            }
        }


    }


}
