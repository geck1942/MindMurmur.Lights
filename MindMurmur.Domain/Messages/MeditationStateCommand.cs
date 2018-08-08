using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Domain.Messages
{
    public class MeditationStateCommand
    {
        public string CommandId { get; set; }
        public Int32 State { get; set; }
    }

    public enum MeditationState
    {
        OFF = -1,
        IDLE = 0,
        LEVEL_1 = 1,
        LEVEL_2 = 2,
        LEVEL_3 = 3,
        LEVEL_4 = 4,
        LEVEL_5 = 5
    }
}
