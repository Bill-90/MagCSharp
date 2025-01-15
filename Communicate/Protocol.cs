using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_MEP_TEP.Communicate
{
    public abstract class Protocol
    {

        public abstract CommandInfo GeneratCommand(byte[] data, int length);

    }
}
