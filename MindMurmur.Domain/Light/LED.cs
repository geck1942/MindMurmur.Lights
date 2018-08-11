using System;
using System.Collections.Generic;
using System.Drawing;

namespace MindMurmur.Domain.Light
{
    public class LED
    {
        public short Index { get; set; }
        public short AlphaChannel { get; set; }
        public short RedChannel { get; set; }
        public short GreenChannel { get; set; }
        public short BlueChannel { get; set; }

        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public LED(short index)
        {
            Index = index;
            AlphaChannel = (short)(index + 0);
            RedChannel = (short)(index + 1);
            GreenChannel = (short)(index + 2);
            BlueChannel = (short)(index + 3);
        }

        public Dictionary<short, byte> GetChannelColors()
        {
            var rtn = new Dictionary<short, byte>();
            rtn.Add(AlphaChannel, Alpha);
            rtn.Add(RedChannel, Red);
            rtn.Add(GreenChannel, Green);
            rtn.Add(BlueChannel, Blue);
            return rtn;
        }

        public void SetColor(Color color, Int16 dimmer)
        {
            Alpha = Convert.ToByte((color.A + dimmer) > 0 && (color.A + dimmer) <= 256 ? color.A + dimmer : color.A); //applies dimmer to alpha
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

    }
}
