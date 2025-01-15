using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_MEP_TEP.Communicate.MagStim
{
    public class InstrumentStatus
    {
        public int IsStandby;
        public int IsArmed;
        public int IsReady;
        public int IsCoilPresent;
        public int IsReplaceCoil;
        public int IsErrorPresent;
        public int ErrorType;
        public int IsRemoteControl;

        public InstrumentStatus(byte b)
        {
            IsStandby = b & 0x1;
            IsArmed = b >> 1 & 0x1;
            IsReady = b >> 2 & 0x1;
            IsCoilPresent = b >> 3 & 0x1;
            IsReplaceCoil = b >> 4 & 0x1;
            IsErrorPresent = b >> 5 & 0x1;
            ErrorType = b >> 6 & 0x1;
            IsRemoteControl = b >> 7 & 0x1;
        }

    }
}
