using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TMS_MEP_TEP.Communicate
{
    public class CommandInfo
    {
        public int cmdId;
        public byte param1;
        public byte param2;
        public int cmdLen;
        public byte[] retData;
        public Semaphore semaphore;

        public CommandInfo()
        {
            semaphore = new Semaphore(0, 1);
        }

    }
}
