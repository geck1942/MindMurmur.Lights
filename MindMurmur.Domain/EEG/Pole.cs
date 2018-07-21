using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Domain.EEG
{
    public class Pole
    {
        public double HorizontalPosition { get; set; }
        public double Depth { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }

        public string Name { get; set; }
        public List<LED> LEDS { get; set; }

        public int SetLEDS(double LEDlength)
        {
            double HoopPerimeter = Math.PI * 2d * this.Radius;
            int totalLEDS = (int)Math.Ceiling(HoopPerimeter);
            this.LEDS = new List<LED>();
            for (int i = 0; i < totalLEDS; i++)
            {
                double pct = i / (double)totalLEDS;
                double x_pos = Math.Sin(2 * Math.PI * pct) ;
                double y_pos = -Math.Cos(2 * Math.PI * pct) ;
                this.LEDS.Add(new LED()
                {
                    index = i,
                    Position = new Point((int)(x_pos * this.Radius), (int)(y_pos * this.Radius)),
                    RelativePosition = new Point((int)x_pos, (int)(y_pos / 2 + 0.5d)),
                    Color = Color.Black,
                    Offset = new Point((int)HorizontalPosition, (int)Height)
                });
            }
            return LEDS.Count();
        }

    }
}
