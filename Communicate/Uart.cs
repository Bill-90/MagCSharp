using log4net;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_MEP_TEP.Communicate
{
    public abstract class Uart
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected SerialPort serialPort;
        protected bool isConnect = false;

        public virtual int ConnectUart(string name)
        {
            serialPort = PaiseSerialPort(name);
            if (serialPort.IsOpen)
            {
                return -1;
            }

            serialPort.Open();
            serialPort.DiscardInBuffer();

            isConnect = true;

            return 0;
        }

        protected abstract SerialPort PaiseSerialPort(string name);

        public int DisConnectUart()
        {
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                serialPort.Dispose();
                isConnect = false;
            }
            return 0;
        }

        public int AddDataReceiveHandler(SerialDataReceivedEventHandler serialDataReceivedEventHandler)
        {
            if (serialPort == null)
            {
                return -1;
            }

            serialPort.DataReceived += serialDataReceivedEventHandler;
            return 0;
        }

        public bool Isconnect()
        {
            return isConnect;
        }

        public int SendData(byte[] data)
        {
            if (!Isconnect())
            {
                return -1;
            }

            try
            {
                lock (serialPort)
                {
                    serialPort.Write(data, 0, data.Length);
                    log.Debug("send data >>>>>>>>>>>>>>> " + BitConverter.ToString(data));
                }
            }
            catch (Exception)
            {
                return -2;
            }

            return 0;
        }

        public int ReadData(byte[] buff, int offset, int count)
        {
            int readLen = 0;

            if (!Isconnect())
            {
                return -1;
            }

            try
            {
                readLen = serialPort.Read(buff, offset, count);
            }
            catch (Exception)
            {
                return -2;
            }
            if (readLen > 0)
            {
                log.Debug("read data <<<<< " + BitConverter.ToString(buff, offset, readLen));
            }

            return readLen;
        }

        public int BytesToRead()
        {
            if (!Isconnect())
            {
                return -1;
            }

            int bytesToRead = -1;
            try
            {
                bytesToRead = serialPort.BytesToRead;
            }
            catch (InvalidOperationException)
            {
            }
            return bytesToRead;
        }

        public void ClearReadBuffer()
        {
            if (serialPort != null)
            {
                try
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.DiscardInBuffer();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public int RtsEnable()
        {
            if (!Isconnect())
            {
                return -1;
            }

            try
            {
                lock (serialPort)
                {
                    serialPort.RtsEnable = true;
                    log.Debug("RTS __/▔\\__");
                    serialPort.RtsEnable = false;
                }
            }
            catch (Exception)
            {
                return -2;
            }

            return 0;
        }

    }
}
