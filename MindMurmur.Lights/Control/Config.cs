using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Lights.Control
{
    public static class Config
    {
        /// <summary>
        /// Channels currently configured for LED lights
        /// </summary>
        public static List<short> LightStripList = new List<short>();

        /// <summary>
        /// Frequency for updates
        /// </summary>
        public static int TimerMs = 500;

        static Config() {

        }

        public static void Init() {
            LightStripList.Add(1);
            //LightStripList.Add(6);
            //LightStripList.Add(11);
            //LightStripList.Add(16);
            //LightStripList.Add(21);
        }

        public static short MaxChannels
        {
            get { return Convert.ToInt16(LightStripList.Max() + 4); }
        }

    }
}
