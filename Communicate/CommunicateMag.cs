using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMS_MEP_TEP.Communicate.MagStim;

namespace TMS_MEP_TEP.Communicate
{
    public class CommunicateMag : Communicate
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public CommunicateMag()
        {
            uart = new UartMag();
            protocol = new ProtocolMag();
        }

        private Action<InstrumentStatus> On_Device_Status_change;
        public void Add_On_Device_Status_change_Listener(Action<InstrumentStatus> listener)
        {
            On_Device_Status_change += listener;
        }

        private int bytesToRead = 0;
        protected override void ParseUartData()
        {
            byte[] buff;
            int readLen;

            while (uart.Isconnect())
            {
                if (bytesToRead <= 0)
                {
                    Thread.Sleep(20);
                    continue;
                }
                if (bytesToRead > uart.BytesToRead())
                {
                    Thread.Sleep(10);
                    continue;
                }

                int dataLen = bytesToRead;
                bytesToRead = 0;

                buff = new byte[dataLen];
                readLen = uart.ReadData(buff, 0, dataLen);

                // 接收完一条协议数据，开始处理
                // log.Debug("recv cmd : " + BitConverter.ToString(buff, 0, dataLen));
                ParseOneFrame(buff, dataLen);
            }
        }

        protected override int ParseOneFrame(byte[] data, int cmdLen)
        {
            // 处理协议数据
            CommandInfo cmd = protocol.GeneratCommand(data, cmdLen);
            if (cmd == null)
            {
                return -1;
            }

            // 寻找等待回复的命令列表
            bool paired = false;
            foreach (CommandInfo cmdInfo in CmdList)
            {
                if (cmdInfo.cmdId == cmd.cmdId)
                {
                    paired = true;
                    cmdInfo.retData = data;
                    cmdInfo.semaphore.Release();

                    CmdList.Remove(cmdInfo);
                    break;
                }
            }

            // 未匹配现有命令，说明是下位机主动上报
            if (!paired)
            {
                log.Error("CommunicateMag  未匹配到命令.");
            }

            return 0;
        }

        private int SendCmd(byte[] cmd, byte[] retData, int retLen, int timeout)
        {
            CommandInfo cmdInfo = new CommandInfo();
            cmdInfo.cmdId = cmd[0];

            uart.ClearReadBuffer();
            bytesToRead = retLen;

            int ret = uart.SendData(cmd);
            if (ret < 0)
            {
                return ret;
            }

            // 等待数据返回
            CmdList.Add(cmdInfo);
            bool hasResponse = cmdInfo.semaphore.WaitOne(timeout);
            if (!hasResponse)
            {
                CmdList.Remove(cmdInfo);
                return -1;
            }

            cmdInfo.retData.CopyTo(retData, 0);

            return retData.Length;
        }


        public override int SetPower(int strength)
        {
            int retDataLen = 3;
            byte[] retData = new byte[retDataLen];
            byte[] cmd;
            int retVal = 0;

            lastPokeTime = DateTime.Now;
            // 生成待发送的设置强度命令
            cmd = ProtocolMag.GenerateSetStimStrength(strength);
            // 发送数据并获取结果
            retVal = SendCmd(cmd, retData, retDataLen, 1000);
            if (retVal < 0)
            {
                return retVal;
            }

            // 解析数据
            InstrumentStatus status = new InstrumentStatus(retData[1]);
            On_Device_Status_change?.Invoke(status);

            return 0;
        }

        public override int Arm()
        {
            int retDataLen = 3;
            byte[] retData = new byte[retDataLen];
            byte[] cmd;
            int retVal = 0;

            lastPokeTime = DateTime.Now;
            // 生成待发送的 Arm 命令
            cmd = ProtocolMag.GenerateCmdArm();
            // 发送数据并获取结果
            retVal = SendCmd(cmd, retData, retDataLen, 1000);
            if (retVal < 0)
            {
                return retVal;
            }

            // 解析数据
            InstrumentStatus status = new InstrumentStatus(retData[1]);
            On_Device_Status_change?.Invoke(status);
            pokeLatency = 500;

            return 0;
        }

        public override int DisArm()
        {
            int retDataLen = 3;
            byte[] retData = new byte[retDataLen];
            byte[] cmd;
            int retVal = 0;

            lastPokeTime = DateTime.Now;
            // 生成待发送的 DisArm 命令
            cmd = ProtocolMag.GenerateCmdDisArm();
            // 发送数据并获取结果
            retVal = SendCmd(cmd, retData, retDataLen, 1000);
            if (retVal < 0)
            {
                return retVal;
            }

            // 解析数据
            InstrumentStatus status = new InstrumentStatus(retData[1]);
            On_Device_Status_change?.Invoke(status);
            pokeLatency = 5000;

            return 0;
        }

        public override int Fire(int strength)
        {
            int retDataLen = 3;
            byte[] retData = new byte[retDataLen];
            byte[] cmd;
            int retVal = 0;

            lastPokeTime = DateTime.Now;
            // 生成待发送的 Fire 命令
            cmd = ProtocolMag.GenerateCmdFire();
            // 发送数据并获取结果
            retVal = SendCmd(cmd, retData, retDataLen, 1000);
            if (retVal < 0)
            {
                return retVal;
            }

            // 解析数据
            InstrumentStatus status = new InstrumentStatus(retData[1]);
            On_Device_Status_change?.Invoke(status);

            return 0;
        }

        public int QuickFire()
        {
            int retVal = 0;

            retVal = uart.RtsEnable();

            return retVal;
        }

        private int pokeLatency = 5000;
        private DateTime lastPokeTime = DateTime.Now;


        protected override void StartDeamonThread()
        {
            // 开启 robot 线程保持连接
            Thread robotThread = new Thread(new ThreadStart(DeamonRobot));
            robotThread.IsBackground = true;
            robotThread.Start();
        }

        private void DeamonRobot()
        {
            Thread.Sleep(100);
            EnableRemoteControl();

            Thread.Sleep(100);
            GetParameter();

            while (uart.Isconnect())
            {
                TimeSpan span = DateTime.Now - lastPokeTime;
                if (span.TotalMilliseconds < pokeLatency)
                {
                    Thread.Sleep(200);
                    continue;
                }

                EnableRemoteControl();
                Thread.Sleep(500);
            }
        }

        public int EnableRemoteControl()
        {
            int retDataLen = 3;
            byte[] retData = new byte[retDataLen];
            byte[] cmd;
            int retVal = 0;

            lastPokeTime = DateTime.Now;
            // 生成待发送的 Enable Remote Control 命令
            cmd = ProtocolMag.GenerateEnableRemoteControl();
            // 发送数据并获取结果
            retVal = SendCmd(cmd, retData, retDataLen, 1000);
            if (retVal < 0)
            {
                return retVal;
            }

            // 解析数据
            InstrumentStatus Istatus = new InstrumentStatus(retData[1]);
            On_Device_Status_change?.Invoke(Istatus);

            return 0;
        }

        private int GetParameter()
        {
            int retDataLen = 24;
            byte[] retData = new byte[retDataLen];
            byte[] cmd;
            int retVal = 0;

            lastPokeTime = DateTime.Now;
            // 生成待发送的获取参数命令
            cmd = ProtocolMag.GenerateGetParameter();
            // 发送数据并获取结果
            retVal = SendCmd(cmd, retData, retDataLen, 1000);
            if (retVal < 0)
            {
                return retVal;
            }

            // 解析数据
            InstrumentStatus Istatus = new InstrumentStatus(retData[1]);
            On_Device_Status_change?.Invoke(Istatus);
            RapidStatus Rstatus = new RapidStatus(retData[2]);
            int power = (retData[3] - 0x30) * 100 + (retData[4] - 0x30) * 10 + (retData[5] - 0x30);
            On_TMS_Strength_change?.Invoke(power);

            return 0;
        }

        public int IgnoreCoilInterlock()
        {
            int retDataLen = 3;
            byte[] retData = new byte[retDataLen];
            byte[] cmd;
            int retVal = 0;

            // 生成待发送的忽略线圈内部开关命令
            cmd = ProtocolMag.GenerateIgnoreCoilInterlock();
            // 发送数据并获取结果
            retVal = SendCmd(cmd, retData, retDataLen, 1000);
            if (retVal < 0)
            {
                return retVal;
            }

            // 解析数据
            InstrumentStatus Istatus = new InstrumentStatus(retData[1]);
            On_Device_Status_change?.Invoke(Istatus);

            return 0;
        }

    }
}
