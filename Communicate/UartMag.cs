using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_MEP_TEP.Communicate
{
    public class UartMag : Uart
    {
        protected override SerialPort PaiseSerialPort(string name)
        {
            return new SerialPort()
            {
                PortName = name,
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                WriteTimeout = 1000,
            };
        }

    }
}
