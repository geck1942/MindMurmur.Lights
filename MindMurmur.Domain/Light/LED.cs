using System.Drawing;

namespace MindMurmur.Domain.Light
{
    public class LED
    {
        public short Index { get; set; }
        public short RedChannel { get; set; }
        public short GreenChannel { get; set; }
        public short BlueChannel { get; set; }
        
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public LED(short index)
        {
            Index = index;
            RedChannel = (short)(index + 1);
            GreenChannel = (short)(index + 2);
            BlueChannel = (short)(index + 3);
        }

        public void SetColor(Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

    }
}
