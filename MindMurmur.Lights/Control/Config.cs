using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MindMurmur.Domain;
using MindMurmur.Domain.Light;

namespace MindMurmur.Lights.Control
{
    public static class Config
    {
        /// <summary>
        /// Channels currently configured for LED lights
        /// </summary>
        public static List<LightStrip> LightStripList = new List<LightStrip>();
        public static List<LightStrip> ChandelierLightStripList = new List<LightStrip>();

        /// <summary>
        /// Frequency for updates
        /// </summary>
        public static int TimerMs = 500;
        public static Int16 DimmerValue = -1; //TODO: make this a config value

        static Config() {

        }

        public static void Init() {
            LightStripList.Add(new LightStrip(1,1, DimmerValue, false)); //this is the cheap light that already is connected

            //LightStripList.Add(new LightStrip(12, 1));
            //LightStripList.Add(new LightStrip(16, 1));
            //LightStripList.Add(new LightStrip(20, 1));

            ////Chandelier
            //ChandelierLightStripList.Add(new LightStrip(50, 1));
            //ChandelierLightStripList.Add(new LightStrip(54, 1));
            //ChandelierLightStripList.Add(new LightStrip(58, 1));
            //ChandelierLightStripList.Add(new LightStrip(62, 72)); //this is the advanced light strip with 72 independent addressable lights
        }

        public static short MaxChannels
        {
            get
            {
                short maxChannel = 0;
                foreach (LightStrip strip in LightStripList)
                {
                    if (strip.MaxChannel > maxChannel)
                        maxChannel = strip.MaxChannel;
                }
                foreach (LightStrip strip in ChandelierLightStripList)
                {
                    if (strip.MaxChannel > maxChannel)
                        maxChannel = strip.MaxChannel;
                }
                return maxChannel;
            }
        }

        public static Dictionary<MeditationState, Tuple<Color, Color, Color>> MeditationColors =
            new Dictionary<MeditationState, Tuple<Color, Color, Color>>()
            {
                {
                    MeditationState.OFF,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 0, 0, 0),
                        Color.FromArgb(255, 0, 0, 0),
                        Color.FromArgb(255, 0, 0, 0))
                },
                {
                    MeditationState.IDLE,
                        new Tuple<Color, Color, Color>(
                            Color.FromArgb(255, 204, 0, 255), 
                            Color.FromArgb(255, 0, 255, 24),
                            Color.FromArgb(255, 0, 24, 255))
                },
                {
                    MeditationState.LEVEL_4,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 0, 192, 255),
                        Color.FromArgb(255, 255, 188, 0),
                        Color.FromArgb(255, 0, 255, 150))
                },
                {
                    MeditationState.LEVEL_3,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 0, 255, 0),
                        Color.FromArgb(255, 181, 0, 255),
                        Color.FromArgb(255,  255, 220, 0))
                },
                {
                    MeditationState.LEVEL_2,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 255, 210, 0),
                        Color.FromArgb(255, 0, 135, 255),
                        Color.FromArgb(255, 220, 0, 255))
                },
                {
                    MeditationState.LEVEL_1,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 255, 0, 0),
                        Color.FromArgb(255, 150, 0, 255),
                        Color.FromArgb(255,  0, 192, 255))
                },
                {
                    MeditationState.LEVEL_5,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 204, 0, 255),
                        Color.FromArgb(255, 0, 255, 24),
                        Color.FromArgb(255, 0, 24, 255))
                }
            };


    }
}
