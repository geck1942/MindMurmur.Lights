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

        public LightStrip(int fixtureIndex, int numberOfLioghtSegments, int lightOffset=0)
        {
            Index = (short)fixtureIndex;
            Offset = (short)(fixtureIndex + 3 + lightOffset);
            LEDList = new List<LED>();
            var x = fixtureIndex;
            var maxIndex = fixtureIndex + (numberOfLioghtSegments * (3 + lightOffset));
            while (x<maxIndex)
            {
                LEDList.Add(new LED((short)x)); //add LED to the list
                x += 3+lightOffset;
            }//end while
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
