using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_MEP_TEP.Communicate.MagStim
{
    public class RapidStatus
    {
        public int IsEnhanced;
        public int IsTrain;
        public int IsWait;
        public int IsSinglePulseMode;
        public int IsPSUConnected;
        public int IsCoilReady;
        public int ThetaDetected;
        public int ModifiedCoil;

        public RapidStatus(byte b)
        {
            IsEnhanced = b & 0x1;
            IsTrain = b >> 1 & 0x1;
            IsWait = b >> 2 & 0x1;
            IsSinglePulseMode = b >> 3 & 0x1;
            IsPSUConnected = b >> 4 & 0x1;
            IsCoilReady = b >> 5 & 0x1;
            ThetaDetected = b >> 6 & 0x1;
            ModifiedCoil = b >> 7 & 0x1;
        }

    }
}
