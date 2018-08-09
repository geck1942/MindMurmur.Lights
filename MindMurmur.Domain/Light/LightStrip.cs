using System;
using System.Collections.Generic;
using System.Drawing;

namespace MindMurmur.Domain.Light
{
    public class LightStrip
    {
        public short Index { get; set; }
        public short Offset { get; set; }

        public List<LED> LEDList { get; set; }

        public LightStrip()
        {
            Index = 1;
            Offset = 3;//for the three colors
            LEDList = new List<LED>(){new LED(1)};
        }

        public LightStrip(int fixtureIndex, int numberOfLightSegments, int lightOffset=0)
        {
            fixtureIndex = Convert.ToInt16(fixtureIndex + 7);//TODO: figure out why the strip is indexed for red at 8 instead of 1
            Index = (short)fixtureIndex;
            Offset = (short)(fixtureIndex + lightOffset);
            LEDList = new List<LED>();
            var x = fixtureIndex;
            var maxIndex = fixtureIndex + (numberOfLightSegments * (3 + lightOffset));
            while (x<maxIndex)
            {
                LEDList.Add(new LED((short)x)); //add LED to the list
                x += 3+lightOffset;
            } //end while
        }

        public void SetColor(Color color)
        {
            foreach (LED led in LEDList)
                led.SetColor(color);
        }
        
        public Dictionary<short,byte> ChannelColors()
        {
            var rtn = new Dictionary<short, byte>();
            foreach (LED led in LEDList)
            {
                rtn.Add(led.AlphaChannel,led.Alpha);
                rtn.Add(led.RedChannel, led.Red);
                rtn.Add(led.GreenChannel, led.Green);
                rtn.Add(led.BlueChannel, led.Blue);
            }
            return rtn;
        }

        public Dictionary<short, byte> ChannelColors(Color color)
        {
            SetColor(color);
            return ChannelColors();
        }

        public short MaxChannel {
            get
            {
                short maxChannel = 0;
                foreach (LED led in LEDList)
                {
                    if ((led.Index+Offset)>maxChannel)
                        maxChannel = (short)(led.Index + Offset);
                }
                return maxChannel;
            }
        }

    }
}
