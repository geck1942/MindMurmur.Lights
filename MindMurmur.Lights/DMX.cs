using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MindMurmur.Lights
{
    public class DMX : IDisposable
    {
        private DMXController DMXController;
        private short maxChannel = 64;
  
        public bool IsConnected = false;

        public DMX(short channelCount)
        {
        }

        async public Task Connect()
        {
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[ ] Connecting to DMX Device...");
            ushort vendorId = 0x10CF;
            ushort productId = 0x8062;
            ushort usagePage = 0x00FF;
            ushort usageId = 0x0001;

            try
            {
                //// K8062
                IsConnected = false;
                if (DMXController != null) DMXController.Dispose();
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[ ] Loading DMX Controller...");
                DMXController = new DMXController();
                DMXController.StartDevice();
                DMXController.SetChannelCount(512);
                await Task.Delay(3000);
                DMXController.SetChannelCount(512);
                await Task.Delay(3000);
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[<] DMX: SetChannelCount: " + maxChannel);
                IsConnected = true;
                Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("[-] Connected");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[!] " + ex.Message);
                IsConnected = false;
                throw ex;
            }
        }

        public byte[] GetDMXFromColors(IEnumerable<Color> LEDColors)
        {
            if (LEDColors == null || !LEDColors.Any()) return null;
            if (LEDColors.Count() > maxChannel / 3)
                LEDColors = LEDColors.Take(maxChannel / 3);

            byte[] channels = LEDColors.SelectMany((color) =>
            {
                var A = color.A / 255M;
                var RGB = new ColorMine.ColorSpaces.Rgb() { R = (int)(color.R * A), G = (int)(color.G * A), B = (int)(color.B * A) };
                var CMY = RGB.To<ColorMine.ColorSpaces.Cmy>();
                // LAB
                return new byte[] {
                    (byte)(15*CMY.C),
                    (byte)(15*CMY.M),
                    (byte)(15*CMY.Y)
                };
            }).ToArray();

            return channels;
        }

        public void SendDMXFrames(byte[] channelsdata)
        {
            //// V3 k8062
            for (int i = 0; i < 2; i++)
                for (int channel = 0; channel < channelsdata.Length; channel++)
                    DMXController.SetChannel((short)(channel + 10 + (channelsdata.Length * i)), channelsdata[channel]);
        }

        public async Task TestSequence()
        {
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[ ] Testing lights...");
            foreach (var color in new Color[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Orange, Color.Cyan, Color.Pink, Color.Purple, Color.White, Color.LightGray, Color.Gray, Color.DarkGray, Color.Black })
            {
                SetColor(color);
                Debug.WriteLine(color.ToString());
                await Task.Delay(300);
            }
            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("[-] Done testing");
        }

        public void SetColor(Color color)
        {
            byte[] dmxdata = GetDMXFromColors(new Color[] { color, color, color, color, color, color });
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[ ] " + color.ToString() + ": (" + dmxdata[0] + "," + dmxdata[1] + "," + dmxdata[2] + ") ");
            SendDMXFrames(dmxdata);
        }

        public void HeartRateBlink(Color currentColor)
        {
            byte[] dmxdata = GetDMXFromColors(new Color[] { Color.Black, Color.Black, Color.Black, currentColor });
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[ BLINK ] " + currentColor.ToString() + ": (" + dmxdata[0] + "," + dmxdata[1] + "," + dmxdata[2] + ") ");
            SendDMXFrames(dmxdata);
        }

        public void Dispose()
        {
            if (this.DMXController != null && IsConnected)
                this.DMXController.Dispose();
            VellemanDMX.StopDevice();
        }

        public void SetMaxChannel(short channelCount)
        {
            maxChannel = channelCount;
        }
    }
}
