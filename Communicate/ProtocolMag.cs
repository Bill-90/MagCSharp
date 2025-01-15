using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_MEP_TEP.Communicate
{
    public class ProtocolMag : Protocol
    {
        // 协议号
        public const byte PROT_ENABLE_REMOTE = 0x51; // enable remote control  使能远程控制
        public const byte PROT_DISABLE_REMOTE = 0x52; // disable remote control  禁用远程控制

        public const byte PROT_SET_POWER = 0x40; // set power A 设置强度
        public const byte PROT_SET_FREQUENCY = 0x42; // set frequency 设置频率
        public const byte PROT_SET_TOTAL_PULSES = 0x44; // set number of pulses 设置总个数
        public const byte PROT_SET_DURATION = 0x5B; // set duration  设置刺激时间
        public const byte PROT_SET_BASE_MODE = 0x45; // set base mode( stopped / armed )  设置模式

        public const byte PROT_ENABLE_ENHANCED_POWER = 0x5E; // enable enhanced rapid power setting mode  使能增强模式
        public const byte PROT_DISABLE_ENHANCED_POWER = 0x5F; // disable enhanced rapid power setting mode  禁用增强模式

        public const byte PROT_OBTAIN_PARAMETER = 0x5C; // get current rapid parameter settings  获取当前设备状态及参数
        public const byte PROT_OBTAIN_STSTEM_STATUS = 0x78; // get system status  获取当前设备状态
        public const byte PROT_OBTAIN_ERROR_CODE = 0x49; // get current error code  获取当前错误码

        public const byte PROT_IGNORE_COIL_INTERLOCK = 0x62; // base ignore coil interlock switch 忽略线圈内部开关
        public const byte PROT_SET_CHARGE_DELAY = 0x6E; // set charge delay  设置充电延迟时间
        public const byte PROT_GET_CHARGE_DELAY = 0x6F; // get charge delay  获取充电延迟时间

        public const byte PROT_OBTAIN_WW_VER = 0x4E; // get stored device software version numbers  获取软件版本号




        // 生成使能远程控制指令 '7cef-88f213ff-08'
        public static byte[] GenerateEnableRemoteControl()
        {
            int cmdLen = 18;
            byte[] data = new byte[cmdLen];
            data[0] = PROT_ENABLE_REMOTE;
            data[1] = 0x37;
            data[2] = 0x63;
            data[3] = 0x65;
            data[4] = 0x66;
            data[5] = 0x2D;
            data[6] = 0x38;
            data[7] = 0x38;
            data[8] = 0x66;
            data[9] = 0x32;
            data[10] = 0x31;
            data[11] = 0x33;
            data[12] = 0x66;
            data[13] = 0x66;
            data[14] = 0x2D;
            data[15] = 0x30;
            data[16] = 0x38;
            data[17] = CalculateCRC(data, cmdLen);

            return data;
        }

        // 生成设置刺激强度命令数据
        // strength: 0-100
        public static byte[] GenerateSetStimStrength(int strength)
        {
            int cmdLen = 5;
            byte[] data = new byte[cmdLen];
            data[0] = PROT_SET_POWER;
            data[1] = (byte)(0x30 + strength / 100);
            data[2] = (byte)(0x30 + strength / 10 % 10);
            data[3] = (byte)(0x30 + strength % 10);
            data[4] = CalculateCRC(data, cmdLen);

            return data;
        }

        // 生成设置忽略线圈内部开关
        public static byte[] GenerateIgnoreCoilInterlock()
        {
            int cmdLen = 3;
            byte[] data = new byte[cmdLen];
            data[0] = PROT_IGNORE_COIL_INTERLOCK;
            data[1] = 0x40;
            data[2] = CalculateCRC(data, cmdLen);

            return data;
        }

        // 生成获取当前设备状态及参数
        public static byte[] GenerateGetParameter()
        {
            int cmdLen = 3;
            byte[] data = new byte[cmdLen];
            data[0] = PROT_OBTAIN_PARAMETER;
            data[1] = 0x40;
            data[2] = CalculateCRC(data, cmdLen);

            return data;
        }

        // 生成 Arm 指令
        public static byte[] GenerateCmdArm()
        {
            int cmdLen = 3;
            byte[] data = new byte[cmdLen];
            data[0] = PROT_SET_BASE_MODE;
            data[1] = 0x42;
            data[2] = CalculateCRC(data, cmdLen);

            return data;
        }

        // 生成 DisArm 指令
        public static byte[] GenerateCmdDisArm()
        {
            int cmdLen = 3;
            byte[] data = new byte[cmdLen];
            data[0] = PROT_SET_BASE_MODE;
            data[1] = 0x41;
            data[2] = CalculateCRC(data, cmdLen);

            return data;
        }

        // 生成 Fire 指令
        public static byte[] GenerateCmdFire()
        {
            int cmdLen = 3;
            byte[] data = new byte[cmdLen];
            data[0] = PROT_SET_BASE_MODE;
            data[1] = 0x48;
            data[2] = CalculateCRC(data, cmdLen);

            return data;
        }


        private static byte CalculateCRC(byte[] data, int cmdLen)
        {
            byte crc = 0;
            for (int i = 0; i < cmdLen - 1; i++)
            {
                crc += data[i];
            }

            return (byte)((~crc) & 0xFF);
        }

        private bool CheckCRC(byte[] data, int cmdLen)
        {
            byte crc = CalculateCRC(data, cmdLen);
            return crc == data[cmdLen - 1];
        }

        public override CommandInfo GeneratCommand(byte[] data, int cmdLen)
        {
            CommandInfo cmd = new CommandInfo();
            cmd.cmdId = data[0];

            if (!CheckCRC(data, cmdLen))
            {
                return null;
            }

            return cmd;
        }
        
        
    }
}
