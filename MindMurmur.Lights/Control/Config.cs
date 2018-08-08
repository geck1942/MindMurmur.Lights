using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MindMurmur.Domain.Light;

namespace MindMurmur.Lights.Control
{
    public static class Config
    {
        /// <summary>
        /// Channels currently configured for LED lights
        /// </summary>
        public static List<LightStrip> LightStripList = new List<LightStrip>();

        /// <summary>
        /// Frequency for updates
        /// </summary>
        public static int TimerMs = 500;

        static Config() {

        }

        public static void Init() {
            LightStripList.Add(new LightStrip(1,1)); //this is the cheap light that already is connected

            LightStripList.Add(new LightStrip(12, 1));
            LightStripList.Add(new LightStrip(16, 1));
            LightStripList.Add(new LightStrip(20, 1));

            //Chandelier
            LightStripList.Add(new LightStrip(50, 1));
            LightStripList.Add(new LightStrip(54, 1));
            LightStripList.Add(new LightStrip(58, 1));
            LightStripList.Add(new LightStrip(62, 72)); //this is the cheap light that already is connected
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
                return maxChannel;
            }
        }
    }
}
