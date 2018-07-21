using System;
using System.Drawing;

namespace MindMurmur.Domain.EEG
{
    public sealed class LED
    {
        public System.Drawing.Point Position { get; set; }
        public int index { get; set; }
        public Color Color { get; set; }
        public System.Drawing.Point RelativePosition { get; internal set; }
        public System.Drawing.Point Offset { get; set; }

        public System.Drawing.Point GetWorldPosition()
        {
            return new System.Drawing.Point(this.Position.X + this.Offset.X, this.Position.Y + this.Offset.Y);
        }
    }
}
