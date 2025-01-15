using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TMS_MEP_TEP.Communicate
{
    public abstract class Communicate
    {
        
        protected ArrayList CmdList = new ArrayList();

        protected Uart uart;
        protected Protocol protocol;

        protected Action<int> On_TMS_Strength_change;
        protected Action<int> On_TMS_Stim_count_change;

        public void Add_On_TMS_Strength_change_Listener(Action<int> listener)
        {
            On_TMS_Strength_change += listener;
        }

        public void Add_On_TMS_Stim_count_change_Listener(Action<int> listener)
        {
            On_TMS_Stim_count_change += listener;
        }


        public int Init(string name)
        {
            int ret = uart.ConnectUart(name);
            if (ret < 0)
            {
                return ret;
            }

            // 注册数据读取函数，目前不用这种方法
            // int val = Uart.Instance.AddDataReceiveHandler(SerialPort_DataReceived);

            // 开启线程读取串口数据
            Thread thread = new Thread(new ThreadStart(ParseUartData));
            thread.IsBackground = true;
            thread.Start();

            // 开启守护线程
            StartDeamonThread();

            return 0;
        }

        public int DeInit()
        {
            uart.DisConnectUart();
            return 0;
        }

        public bool IsConnected()
        {
            return uart.Isconnect();
        }

        public abstract int SetPower(int strength);

        public abstract int Arm();

        public abstract int DisArm();

        public abstract int Fire(int strength);


        public void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
        }

        protected abstract void StartDeamonThread();

        protected abstract void ParseUartData();

        protected abstract int ParseOneFrame(byte[] data, int cmdLen);

    }
}
