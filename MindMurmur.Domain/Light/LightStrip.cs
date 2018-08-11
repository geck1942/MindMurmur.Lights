using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace MindMurmur.Domain.Light
{
    public class LightStrip
    {
        public short Index { get; set; }
        public short Offset { get; set; }
        public short Dimmer { get; set; }
        /// <summary>
        /// Controls whether the lightstrip honors alpha or not.
        /// If it does not, the alpha will be used to reduce the RGB values
        /// by 255 - Current Alpha on Color.A 
        /// </summary>
        public bool HonorAlpha { get; set; }

        public List<LED> LEDList { get; set; }
        

        public LightStrip()
        {
            Index = 1;
            Offset = 3;//for the three colors
            LEDList = new List<LED>(){new LED(1)};
        }

        public LightStrip(int fixtureIndex, int numberOfLightSegments, short dimmer, bool honorAlpha, int lightOffset=0)
        {
            HonorAlpha = honorAlpha;
            Dimmer = dimmer;
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
                led.SetColor(color, Dimmer);
        }
        
        public Dictionary<short,byte> ChannelColors()
        {
            var rtn = new Dictionary<short, byte>();
            foreach (LED led in LEDList)
            {
                rtn.Add(led.AlphaChannel, led.Alpha);
                var red = HonorAlpha ? led.Red : led.Red.ApplyDimmer(led.Alpha, Dimmer);
                var green = HonorAlpha ? led.Green : led.Green.ApplyDimmer(led.Alpha, Dimmer);
                var blue = HonorAlpha ? led.Blue : led.Blue.ApplyDimmer(led.Alpha, Dimmer);
                Debug.WriteLine(string.Format("ChannelColors() [{0}, {1}, {2}]", red, green, blue));
                rtn.Add(led.RedChannel, red);
                rtn.Add(led.GreenChannel,green);
                rtn.Add(led.BlueChannel,blue );
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
