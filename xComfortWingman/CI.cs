using HidSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using xComfortWingman.Protocol;

//Used when creating allDeviceTypes
using static xComfortWingman.Protocol.PT_RX.MGW_RX_MSG_TYPE;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_DATA_TYPE;

//Used for all sorts of logging
using static xComfortWingman.MyLogger;
using System.IO;
using System.Threading.Tasks;

namespace xComfortWingman
{
    public class CI
    {
        static System.IO.Ports.SerialPort com;
        static HidDevice myDevice;
        static HidStream myHidStream;
        private static bool readyToTransmit;
        private static List<byte> receivedData = new List<byte>();
        //static readonly DeviceTypeList dtl = new DeviceTypeList();
        private static bool acceptingData = false;

        public static List<Datapoint> datapoints=new List<Datapoint>();
        //public static List<DeviceType> devicetypes = dtl.ListDeviceTypes();

        public static byte sequenceCounter = 0x00;
        public static byte[][] messageHistory = new byte[15][];
        public static readonly List<DeviceType> devicetypes = new List<DeviceType>
            {
            //                 ID   Name                        ShortName       Number            Channels                    Modes                        MessageTypes
                new DeviceType(1, "push-button single",             "PB",       (1),    new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(2, "push-button dual",               "PB",       (2),    new int[] { 0, 1 },         new int[] { 0 },            new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(3, "push-button quad",               "PB",       (3),    new int[] { 0, 1, 2, 3 },   new int[] { 0 },            new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(4, "Room Controller ( /w Switch)",   "RC",       (5),    new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_TOO_COLD, MGW_RMT_TOO_WARM }, new byte[] { MGW_RDT_RC_DATA }, "NoComment"),
                new DeviceType(5, "Room Controller ( /w Switch)",   "RC",       (5),    new int[] { 0 },            new int[] { 1 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_RC_DATA }, "NoComment"),
                new DeviceType(6, "Switching Actuator",             "SA",       (16),   new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = OFF / ON]"),
                new DeviceType(7, "Dimming Actuator",               "DA",       (17),   new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = 0..100]"),
                new DeviceType(8, "Jalousie Actuator",              "JA",       (18),   new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = STOP / OPEN / CLOSE]"),
                new DeviceType(9, "Binary Input, 230V",             "BI 230",   (19),   new int[] { 0 },            new int[] { 0, 2 },         new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(10, "Binary Input, 230V",            "BI 230",   (19),   new int[] { 0 },            new int[] { 1, 3 },         new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(11, "Binary Input, 230V",            "BI 230",   (19),   new int[] { 1 },            new int[] { 0, 3 },         new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(12, "Binary Input, 230V",            "BI 230",   (19),   new int[] { 1 },            new int[] { 1, 2 },         new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(13, "Binary Input, 230V",            "BI 230",   (19),   new int[] { 0, 1 },         new int[] { 0, 1, 2, 3 },   new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = OFF / ON]"),
                new DeviceType(14, "Binary Input, Battery",         "BI Batt",  (20),   new int[] { 0 },            new int[] { 0, 2 },         new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(15, "Binary Input, Battery",         "BI Batt",  (20),   new int[] { 0 },            new int[] { 1, 3 },         new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(16, "Binary Input, Battery",         "BI Batt",  (20),   new int[] { 1 },            new int[] { 0, 3 },         new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(17, "Binary Input, Battery",         "BI Batt",  (20),   new int[] { 1 },            new int[] { 1, 2 },         new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(18, "Remote Control 12 Channel (old design)", "Rt 12 old", (21), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new int[] { 0 }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(19, "Home-Manager",                  "HM",       (22),   new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 }, new int[] { 0 }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(20, "Temperature Input",             "TI",       (23),   new int[] { 0, 1 },         new int[] { 0 },            new byte[] { MGW_RMT_TOO_COLD, MGW_RMT_TOO_WARM }, new byte[] { MGW_RDT_INT16_1POINT }, "NoComment"),
                new DeviceType(21, "Temperature Input",             "TI",       (23),   new int[] { 0, 1 },         new int[] { 1 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_INT16_1POINT }, "NoComment"),
                new DeviceType(22, "Analog Input",                  "AI",       (24),   new int[] { 0, 1 },         new int[] { 0 },            new byte[] { MGW_RMT_ON, MGW_RMT_OFF }, new byte[] { MGW_RDT_FLOAT }, "NoComment"),
                new DeviceType(23, "Analog Input",                  "AI",       (24),   new int[] { 0 },            new int[] { 1 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_FLOAT }, "NoComment"),
                new DeviceType(24, "Analog Input",                  "AI",       (24),   new int[] { 0 },            new int[] { 2 },            new byte[] { MGW_RMT_FORCED }, new byte[] { MGW_RDT_PERCENT }, "NoComment"),
                new DeviceType(25, "Analog Input",                  "AI",       (24),   new int[] { 0, 1 },         new int[] { 0 },            new byte[] { MGW_RMT_TOO_COLD, MGW_RMT_TOO_WARM }, new byte[] { MGW_RDT_INT16_1POINT }, "NoComment"),
                new DeviceType(26, "Analog Input",                  "AI",       (24),   new int[] { 0, 1 },         new int[] { 1 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_INT16_1POINT }, "NoComment"),
                new DeviceType(27, "Analog Actuator",               "AA",       (25),   new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = 0..100]"),
                new DeviceType(28, "Room-Manager",                  "RM",       (26),   new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148 }, new int[] { 0 }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(29, "Jalousie Actuator with Security", "JA S",   (27),   new int[] { 0 },            new int[] { 0, 1 },         new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = STOP / OPEN / CLOSE]"),
                new DeviceType(30, "Communication Interface",       "CI",       (28),   new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 }, new int[] { 0 }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(31, "Motion Detector",               "MD",       (29),   new int[] { 0, 1 },         new int[] { 0 },            new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(32, "Remote Control 2 Channel small", "Rt 2",    (48),   new int[] { 0, 1 },         new int[] { 0 },            new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(33, "Remote Control 12 Channel",     "Rt 12",    (49),   new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new int[] { 0 }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(34, "Remote Control 12 Channel w/ display", "Rt 12 d", (50), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new int[] { 0 }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(35, "Room Controller w/ Switch/Humidity", "RC s/h", (51), new int[] { 0 },           new int[] { 0 },            new byte[] { MGW_RMT_TOO_COLD, MGW_RMT_TOO_WARM }, new byte[] { MGW_RDT_RC_DATA }, "NoComment"),
                new DeviceType(36, "Room Controller w/ Switch/Humidity", "RC s/h", (51), new int[] { 0 },           new int[] { 1 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_RC_DATA }, "NoComment"),
                new DeviceType(37, "Room Controller w/ Switch/Humidity", "RC s/h", (51), new int[] { 1 },           new int[] { 0 },            new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_FLOAT }, "NoComment"),
                new DeviceType(38, "Room Controller w/ Switch/Humidity", "RC s/h", (51), new int[] { 1 },           new int[] { 1 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_FLOAT }, "NoComment"),
                new DeviceType(39, "Router (no communication possible, just ignore it)", "Router", (52), new int[] { 0 }, new int[] { 0 }, new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(40, "Impulse Input",                 "ImpI",     (53),   new int[] { 0, 1 },         new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32 }, "[totalnumberofImpulses]"),
                new DeviceType(41, "EMS",                           "EMS",      (54),   new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32_3POINT }, "[Energy, MGW_RMT_kWh]"),
                new DeviceType(42, "EMS",                           "EMS",      (54),   new int[] { 1 },            new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16_1POINT }, "[Power, MGW_RMT_W]"),
                new DeviceType(43, "EMS",                           "EMS",      (54),   new int[] { 2 },            new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16_3POINT }, "[Current, MGW_RMT_A]"),
                new DeviceType(44, "EMS",                           "EMS",      (54),   new int[] { 3 },            new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16_2POINT }, "[Voltage, MGW_RMT_V]"),
                new DeviceType(45, "E-Raditor Actuator",            "RadAct",   (55),   new int[] { 0, 1, 2 },      new int[] { 0 },            new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(46, "Remote Control Alarm Pushbutton", "Rt 1",   (56),   new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(47, "BOSCOS (Bed/Chair Occupancy Sensor)", "BOSCOS", (57), new int[] { 0, 1 },       new int[] { 0 },            new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(48, "MEP",                           "MEP",      (62),   new int[] { 0, 1, 2, 3 },   new int[] { 0 },            new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(49, "MEP",                           "MEP",      (62),   new int[] { 10 },           new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT8 }, "[currenttariffinuse]"),
                new DeviceType(50, "MEP",                           "MEP",      (62),   new int[] { 11 },           new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32_3POINT }, "[totalEnergy, MGW_RMT_Wh]"),
                new DeviceType(51, "MEP",                           "MEP",      (62),   new int[] { 13 },           new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16 }, "[totalPower, MGW_RMT_W]"),
                new DeviceType(52, "MEP",                           "MEP",      (62),   new int[] { 15, 16, 17, 18, 19, 20, 21 }, new int[] { 0 }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32_3POINT }, "[Energy, MGW_RMT_Wh]"),
                new DeviceType(53, "MEP",                           "MEP",      (62),   new int[] { 35, 36, 37, 38, 39, 40, 41, 42, 43, 44 }, new int[] { 0 }, new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(54, "HRV",                           "HRV",      (65),   new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_HRV_OUT }, "NoComment"),
                new DeviceType(55, "Rosetta Sensor",                "Ros Sens", (68),   new int[] { 0 },            new int[] { 0, 2 },         new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_ROSETTA }, "NoComment"),
                new DeviceType(56, "Rosetta Sensor",                "Ros Sens", (68),   new int[] { 0 },            new int[] { 1, 3 },         new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_ROSETTA }, "NoComment"),
                new DeviceType(57, "Rosetta Sensor",                "Ros Sens", (68),   new int[] { 1 },            new int[] { 0, 3 },         new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(58, "Rosetta Sensor",                "Ros Sens", (68),   new int[] { 1 },            new int[] { 1, 2 },         new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(59, "Rosetta Router",                "Ros Rout", (69),   new int[] { 0 },            new int[] { 0 },            new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(60, "Multi Channel Heating Actuator", "MCHA",    (71),   new int[] { 0, 1 },         new int[] { 0 },            new byte[] { MGW_RMT_ON, MGW_RMT_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(61, "Multi Channel Heating Actuator", "MCHA",    (71),   new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, new int[] { 0 }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(62, "Multi Channel Heating Actuator", "MCHA",    (71),   new int[] { 14, 15 },       new int[] { 0 }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(63, "Communication Interface USB", "CI Stick",   (72),   new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 }, new int[] { 0 }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(64, "Switching Actuator New Generation", "SA-NG",(74),   new int[] { 0 },            new int[] { 0 },            new byte[] { MGW_RMT_STATUS }, new byte[] { }, "[INFO_SHORT = OFF(0x02) / ON(0x03)]"),
                new DeviceType(65, "Switching Actuator New Generation", "SA-NG",(74),   new int[] { 1 },            new int[] { 0 },            new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(66, "Switching Actuator New Generation", "SA-NG",(74),   new int[] { 1 },            new int[] { 1 },            new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(67, "Switching Actuator New Generation", "SA-NG",(74),   new int[] { 2 },            new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16_1POINT }, "[Power, MGW_RMT_W]"),
                new DeviceType(68, "Switching Actuator New Generation", "SA-NG",(74),   new int[] { 3 },            new int[] { 0 },            new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32_3POINT }, "[Energy, MGW_RMT_kWh]"),
                new DeviceType(69, "Switching Actuator New Generation", "SA-NG",(74),   new int[] { 4 },            new int[] { 0 },            new byte[] { MGW_RMT_ON, MGW_RMT_OFF }, new byte[] { MGW_RDT_NO_DATA }, "[LoadError]"),
                new DeviceType(70, "Router New Generation",         "RA-NG",    (75),   new int[] { 1, 2, 3, 4, 5 }, new int[] { 0 },           new byte[] { MGW_RMT_ON, MGW_RMT_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment")
            };

        #region "SETUP and BOOT stuff"

        public static async Task ConnectToCI()
        {
            if (Program.Settings.CONNECTION_MODE == CI_CONNECTION_MODE.USB_MODE)
            {
                await ConnecAsHID(); //Connecting to CI as USB HID
                if (myDevice == null)
                {
                    DoLog("FAILED", 3, true, 12);
                    Program.BootWithoutError = false;
                    return;
                }
            }
            else
            {
                ConnectAsRS232(); //Connecting to CI via RS232
            }
        }

        private static async Task ConnecAsHID()
        {
            //Console.Write("Connecting to CI...");
            Stopwatch stopwatch = new Stopwatch();
            DoLog("Connecting to CI device...", false);
            stopwatch.Start();
            var list = DeviceList.Local;
            //list.Changed += (sender, e) => DoLog("Device list changed."); //We don't need to implement support for hotswap right now... 
            var allHidList = list.GetHidDevices().ToArray();
            foreach (HidDevice dev in allHidList)
            {
                if (dev.VendorID == 0x188A && dev.ProductID == 0x1101)
                {
                    //We have found the CI!
                    myDevice = dev;
                    break;
                }
            }
            if (myDevice != null)
            {
                var reportDescriptor = myDevice.GetReportDescriptor();

                if (myDevice.TryOpen(out myHidStream))//HidStream hidStream))
                {
                    DoLog("OK", 3, false, 10);
                    DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                    DoLog("Listening for xComfort messages...");
                    myHidStream.ReadTimeout = Timeout.Infinite;

                    using (myHidStream)
                    {
                        var inputReportBuffer = new byte[myDevice.GetMaxInputReportLength()];

                        // -------------------- RAW -------------------------
                        IAsyncResult ar = null;
                        readyToTransmit = true;
                        int startTime = Environment.TickCount;
                        while (true)
                        {
                            if (ar == null)
                            {
                                ar = myHidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);
                            }

                            if (ar != null)
                            {
                                if (ar.IsCompleted)
                                {
                                    int byteCount = myHidStream.EndRead(ar);
                                    ar = null;

                                    if (byteCount > 0)
                                    {
                                        //string hexOfBytes = string.Join(" ", inputReportBuffer.Take(byteCount).Select(b => b.ToString("X2")));
                                        //DoLog("  {0}", hexOfBytes);
                                        //PrintByte(inputReportBuffer, "Received data from CI");
                                        await IncommingData(inputReportBuffer);
                                    }
                                }
                                else
                                {
                                    ar.AsyncWaitHandle.WaitOne(500);
                                }
                            }
                            uint elapsedTime = (uint)(Environment.TickCount - startTime);
                            //if (elapsedTime >= 20000) { break; } // Stay open for 20 seconds.
                        }
                        // --------------------------------------------------
                    }
                }
                else
                {
                    DoLog("FAIL", 3, true, 14);
                    DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                    //return false;
                }
            }
            //return false;
        }

        #region "RS232"
        private static void ConnectAsRS232()
        {
            try
            {
                com = new System.IO.Ports.SerialPort(Program.Settings.RS232_PORT, Program.Settings.RS232_BAUD)
                {
                    StopBits = System.IO.Ports.StopBits.One,
                    Parity = System.IO.Ports.Parity.None
                };

                com.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(DataReceivedHandler);

                com.Open();
                DoLog($"{com.PortName} is open: " + com.IsOpen);

                //{ 0x5A, 0x06, 0xB1, 0x02, 0x0A, 0x01, 0x70, 0xA5 }; // Turns on DP #2

                byte[] myCommand = { 0x5A, 0x04, 0xB2, 0x1B, 0x00, 0xA5 }; // Requests the software versions of the interface 
                PrintByte(myCommand, "Requesting software version");
                com.Write(myCommand, 0, 6);
            } catch (Exception exception)
            {
                Program.BootWithoutError = false;
                MyLogger.LogException(exception);
            }
        }

        private static void DataReceivedHandler(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //DoLog("Receiving data:");
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;

            string myData = sp.ReadExisting();
            int cmdLength = 0;
            foreach (byte b in myData)
            {
                if (b == Program.Settings.RS232_STARTBYTE)
                {
                    acceptingData = true;
                    continue;
                }
                if (acceptingData)
                {
                    receivedData.Add(b);
                    //Now we need to know the value of the second byte.
                    if (cmdLength == 0 && receivedData.Count > 1) { cmdLength = receivedData[0]; }
                    if (cmdLength > 0 && receivedData.Count == (cmdLength - 0))
                    {
                        //We are done!
                        acceptingData = false;
                        byte[] dataAsBytes = receivedData.ToArray();
                        dataAsBytes = RemoveRS232Bytes(dataAsBytes);
                        PrintByte(dataAsBytes, "Serial data");
                        IncommingData(dataAsBytes).Wait();
                        receivedData.Clear();
                    }
                    //Console.Write(Convert.ToString(b, 16).PadLeft(2, '0') + " ");
                }
            }
        }

        #endregion

        #region "Datapoints stuff"
        public static bool ImportDatapointsFromFile(String filePath)
        {
            try
            { 
                //Boilerplate - Read a datapoint file exported from Eatons own software
                /* Snippet from an actual file:
                 *  DP  DP+channel      Serial  Typ Ch  Mod Cls N/A
                    13	DblBathroomF0 	4925325 16	0	0	0	#000#000#000#000#0#000#000#005#000#	
                    14	DimToiletDown 	5057045	17	0	0	0	#000#000#000#000#0#000#000#006#000#	
                    15	DimBathroomDwn 	5027425	17	0	0	0	#000#000#000#000#0#000#000#006#000#	
                    16	DimInnerHallN 	3288803	17	0	0	0	#000#000#000#000#0#000#000#006#000#	
                    17	DimInnerHallS 	3812402	17	0	0	0	#000#000#000#000#0#000#000#006#000#	
                   Delimiter is tab
                   The 
                 */
                if (!File.Exists(filePath))
                {
                    DoLog("Datapoint file not found!");
                    return false;
                }
                string aline;
                FileStream fileStream = new FileStream(filePath, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while ((aline = reader.ReadLine()) != null)
                    {
                        string[] line = aline.Split("\t");
                        datapoints.Add(new Datapoint(Convert.ToInt32(line[0]), line[1], Convert.ToInt32(line[2]), Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToInt32(line[6]), line[7]));
                        //DoLog("Added datapoint #" + line[0] + " named " + line[1]);
                    }
                    DoLog("There are now " + datapoints.Count + " datapoints registered in the system!");
                }
                fileStream.Close();
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
        }

        public static bool ImportDatapointsOneByOne(String dataPointLine)
        {
            try
            {          
                //Allows us to add a single datapoint through some other method than reading the file from disk.
                string[] line = dataPointLine.Split("\t");
                datapoints.Add(new Datapoint(Convert.ToInt32(line[0]), line[1], Convert.ToInt32(line[2]), Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToInt32(line[6]), line[7]));
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
        }

        public static String GetDatapointFile()
        {
            try
            {
                if (!File.Exists(Program.Settings.DATAPOINTS_FILENAME))
                {
                    DoLog("Datapoint file not found!");
                    return "File not found!";
                }
                string everything = "Empty file!";
                FileStream fileStream = new FileStream(Program.Settings.DATAPOINTS_FILENAME, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    everything= reader.ReadToEnd();
                }
                fileStream.Close();
                return everything;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return exception.Message;
            }
        }

        public static bool SetDatapointFile(String contents)
        {
            try
            {
                FileStream fileStream = new FileStream(Program.Settings.DATAPOINTS_FILENAME, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(contents);
                }
                fileStream.Close();
                return true;
            } catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
        }
        #endregion

#endregion

        private static async Task SendThenBlockTransmit(byte[] dataToSend)
        {
            readyToTransmit = false;    // Stop any other thread from sending right now

            // Prepare the packet by adding extra bytes if needed.
            if (Program.Settings.CONNECTION_MODE == CI_CONNECTION_MODE.RS232_MODE) { dataToSend = AddRS232Bytes(dataToSend); }
            if (dataToSend[0] != 0x00 && dataToSend[0] != Program.Settings.RS232_STARTBYTE) { dataToSend = AddZeroAsFirstByte(dataToSend); }

            Array.Resize(ref dataToSend, myDevice.GetMaxOutputReportLength()); //If we don't fill the buffer, it will repeat the data instead of using 0x00. That causes undersired behavior...

            DateTime start = DateTime.Now;

            switch (Program.Settings.CONNECTION_MODE)
            {
                case CI_CONNECTION_MODE.RS232_MODE:
                    {
                        com.Write(dataToSend,0,dataToSend.Length);
                        break;
                    }
                case CI_CONNECTION_MODE.USB_MODE:
                    {
                        await myHidStream.WriteAsync(dataToSend, 0, dataToSend.Length, CancellationToken.None);
                        break;
                    }
            }           

            // Crude timeout check works here, because we don't need any strict timing.
            bool preReleased = false;
            while (DateTime.Now.Subtract(start).TotalMilliseconds < Program.Settings.RMF_TIMEOUT)
            {
                if (readyToTransmit) {
                    preReleased = true; // No need to wait for timeout!
                    break;
                } 
            }
            if (!preReleased)
            {
                DoLog("Transmit blockage timed out!");
                readyToTransmit = true; // Unlock due to timeout.
            }
        }

        private static async Task IncommingData(byte[] dataFromCI) //We've got data from the CI
        {
            if (dataFromCI[0] == 0) { dataFromCI = RemoveFirstByte(dataFromCI); } // CKOZ-00/14 triggers this, but CKOZ-00/03 doesn't...
            Console.WriteLine();
            PrintByte(dataFromCI, "Incomming data");
            /*
            Example of an acknowledgement message (OK_MRF):
                    Start   Len     Type    St-Type     Status  Seq+Pri     Ack                         Stop
            RS232:  5A      08      C3      1C          04      70          10          00      00      A5
            USB:            08      C3      1C          04      70          10          00      00
                            8 Byte  Status  OK          MRF     7+Std       ACK_DIRECT  NA      NA
            
            Example of an actuator response/status message:
                            Len     Type    DP      Msg T.  Data T.     Info Sh.    Data1   Data2   Data3   Data4   RSSI    Battery
            USB:            0C      C1      02      70      00          01          00      00      00      00      40      10
                            12 Byte Rx      Dp 2    Status  No Data     On                                          Signal  Mains pwr
            */

            if (Program.Settings.RAW_ENABLED)
            {
                DoLog("Sending RAW data via MQTT...", 2);
                await MQTT.SendMQTTMessageAsync("BachelorPad/xComfort/RAW", FormatByteForPrint(dataFromCI, true));
                //DoLog("OK", 1, true, 10);
            }

            byte MGW_TYPE = dataFromCI[1];
            switch (MGW_TYPE)
            {
                case Protocol.MGW_TYPE.MGW_PT_RX: // Incomming transmission from some device
                    {
                        DoLog("This was a RX packet", 1);
                        //                          Length          Type          Datapoint       Msg type      Data type      Info short               {   Data 0          Data 1          Data 2          Data 3   }      RSSI            Battery
                        HandleRX(new PT_RX.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], dataFromCI[4], dataFromCI[5], new byte[4] { dataFromCI[9], dataFromCI[8], dataFromCI[7], dataFromCI[6] }, dataFromCI[10], dataFromCI[11], dataFromCI[12]), true);
                        break;
                    }
                case Protocol.MGW_TYPE.MGW_PT_TX: // This is strictly speaking a packet type that we are sending, never receiving...
                    {
                        DoLog("If you're seeing this, it means that outbound data has ended up as inbound. This is not really possible!", 5);
                        break;
                    }
                case Protocol.MGW_TYPE.MGW_PT_CONFIG: // Configuration info
                    {
                        DoLog("This was a config packet", 1);
                        DoLog("Config data!");
                        break;
                    }
                case Protocol.MGW_TYPE.MGW_PT_STATUS: // Incomming status. Generated by the interface device, not arrived (directly) by radio transmissions.
                    {
                        DoLog("This was a status packet", 1);
                        //                                Length         Type           StatusType     Status         StatusData {   Data 0          Data 1          Data 2          Data 3   }
                        HandleStatus(new PT_STATUS.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], new byte[4] { dataFromCI[4], dataFromCI[5], dataFromCI[6], dataFromCI[7] }));
                        break;
                    }
                default:
                    {
                        DoLog("Unexpected type: " + Convert.ToString(MGW_TYPE, 16).ToUpper().PadLeft(2, '0'), 4);
                        break;
                    }
            }
        }

        public static async Task SendData(int DP, double dataDouble)
        {
            if (!readyToTransmit)
            {
                // We're not ready, let's wait...
                DoLog("We're not ready to transmit yet...", 2);
                DateTime start = DateTime.Now;
                while (!readyToTransmit)
                {
                    if (DateTime.Now.Subtract(start).TotalSeconds > 10)
                    {
                        // This should never actually happen, as there is another timeout function.
                        // But we'll include it anyway, as it could prevent a total hang if the first function fails.
                        DoLog($"Command to DP #{DP} timed out!", 4);
                        readyToTransmit = true;
                        return;
                    }
                    Thread.Sleep(200);
                }
            };

            Datapoint myDP = datapoints.Find(x => x.DP == DP);
            DeviceType myDT = devicetypes.Find(x => x.ID == myDP.Type);

            byte[] myCommand = new byte[myDevice.GetMaxOutputReportLength()];  //.ConnectedDeviceDefinition.WriteBufferSize.Value]; 
            myCommand[0] = 0x00; // This one is not interpreted as data, it is ignored.
            myCommand[1] = 0x09; // This is the length of the packet. It can be dynamic, but it's also safe to use a fixed value of 0x09 and pad with 0x00.
            myCommand[2] = 0xB1; // This indicates that we want to control a datapoint.
            myCommand[3] = Convert.ToByte(DP); // The datapoint to control.
            myCommand[4] = 0x00; // What kind of "event" we want the datapoint to perform, such as set mode/state/level
            myCommand[5] = 0x00; // Event data, such as "ON"/"OFF"/"42%"
            myCommand[6] = 0x00; // Sometimes event data requires more than a single byte.
            myCommand[7] = 0x00; // ---- || -----
            myCommand[8] = 0x00; // ---- || -----
            myCommand[9] = 0x00; // Sequence number + priority (If used) 0x00 is a safe value in any case.

            DoLog($"Setting DP #{ DP } ({ myDP.Name }) to {dataDouble}.");

            switch (myDP.Type)
            {
                case 16: // Switching Actuator
                    {
                        myCommand[4] = PT_TX.MGW_TX_EVENT.MGW_TE_SWITCH;
                        if (dataDouble > 0) { myCommand[5] = PT_TX.MGW_TX_EVENT_DATA.MGW_TED_ON; } //No need for else, because it's already 0x00.
                        break;
                    }
                case 17: // Dimming Actuator
                    {
                        myCommand[4] = PT_TX.MGW_TX_EVENT.MGW_TE_DIM;
                        myCommand[5] = PT_TX.MGW_TX_EVENT_DATA.MGW_TED_PERCENT;
                        myCommand[6] = Convert.ToByte(dataDouble);
                        break;
                    }
                case 18: // Jalousie Actuator
                    {

                        break;
                    }
            }

            //Update the sequence counter and history
            int shiftedCounter = sequenceCounter << 4; // This bit shift places the value in the upper nibble, allowing the lower nibble to be used as priority
            myCommand[9] = Convert.ToByte(shiftedCounter);
            messageHistory[sequenceCounter] = myCommand;
            sequenceCounter++;
            if (sequenceCounter > 15) { sequenceCounter = 0; } // Reset to 0 in order to keep the size to 4 bits.

            //Send the data
            Console.WriteLine();
            PrintByte(myCommand, "Outgoing data");
            await SendThenBlockTransmit(myCommand);
        }

        public static async Task SendData(byte[] RAWdata)
        {
            await SendThenBlockTransmit(RAWdata);   
        }

        private static void HandleStatus(Protocol.PT_STATUS.Packet statusPacket) // Handling packets containing status info
        {
            //Example of the acnknowledge message (OK_MRF)
            //Length  Type  StatusType  Status  StatusData  Ack  N/A  N/A
            //08      C3    1C          04      70          10   00   00
            bool denyReady = false;
            switch (statusPacket.MGW_ST_TYPE)
            {
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_CONNEX:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x01: //MGW_CM_AUTO(default)
                                {
                                    DoLog($"Interface connection mode: AUTO (default)");
                                    break;
                                }
                            case 0x02: //MGW_CM_USB
                                {
                                    DoLog($"Interface connection mode: USB");
                                    break;
                                }
                            case 0x03: //MGW_CM_RS232
                                {
                                    DoLog($"Interface connection mode: RS232");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown connex status: {statusPacket.MGW_ST_STATUS}", 5);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_ERROR:
                    {

                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //MGW_STS_GENERAL (DATA: specific code)      General Error-Msg
                                {
                                    switch (statusPacket.MGW_ST_DATA[0])
                                    {
                                        case 0xA9:
                                            {
                                                DoLog("General error!\nAssuming invalid datapoint for this interface. (Consider using MRF to update associations)", 4);
                                                break;
                                            }
                                        default:
                                            {
                                                DoLog($"Error! General error, data: {Convert.ToString(statusPacket.MGW_ST_DATA[0], 16).ToUpper().PadLeft(2, '0')}, {Convert.ToString(statusPacket.MGW_ST_DATA[1], 16).ToUpper().PadLeft(2, '0')}", 4);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case 0x01://MGW_STS_UNKNOWN (DATA: specific code)       Msg Unknown
                                {
                                    DoLog($"Error! Unknown error, data: {Convert.ToString(statusPacket.MGW_ST_DATA[0], 16).ToUpper().PadLeft(2, '0')}, {Convert.ToString(statusPacket.MGW_ST_DATA[1], 16).ToUpper().PadLeft(2, '0')}", 4);
                                    break;
                                }
                            case 0x02://MGW_STS_DP_OOR                              Datapoint out of range
                                {
                                    DoLog("Error! Datapoint out of range!", 4);
                                    break;
                                }
                            case 0x03://MGW_STS_BUSY_MRF                            RF Busy (Tx Msg lost)
                                {
                                    DoLog("Error! RF busy, TX message lost", 4);
                                    break;
                                }
                            case 0x04://MGW_STS_BUSY_MRF_RX                         RF Busy (Rx in progress)
                                {
                                    DoLog("Error! RF busy, RX in progress...", 4);
                                    break;
                                }
                            case 0x05://MGW_STS_TX_MSG_LOST                         Tx-Msg lost, repeat it (buffer full)
                                {
                                    //DoLog("Error! TX mesage lost, buffer full!");
                                    Console.Write("WARNING! TX message was lost!", 4);
                                    //readyToTransmit = false;
                                    denyReady = true;
                                    byte maskSequence = 0x0F;      // 00001111
                                    byte seq = statusPacket.MGW_ST_DATA[1];
                                    seq &= maskSequence;
                                    DoLog($" Re-sending message #{seq}...");
                                    //myDevice.WriteAsync(messageHistory[seq]);
                                    break;
                                }
                            case 0x06: //MGW_STS_NO_ACK                             RF ≥90: Timeout, no ACK received!
                                {
                                    DoLog("Timeout, no ACK reveived!", 4);
                                    break;
                                }
                            default:   //                                           Completely undocumented!
                                {
                                    DoLog("Undocumented error!", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_LED:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x01: //LED standard mode (default)
                                {
                                    DoLog($"LED is in standard mode.");
                                    break;
                                }
                            case 0x02: //switch green LED to "reverse" fct
                                {
                                    DoLog($"LED is in reversed mode.");
                                    break;
                                }
                            case 0x03: //switch LEDs completely off
                                {
                                    DoLog($"LED is turned off.");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown LED status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_LED_DIM:
                    {
                        DoLog($"LED brightness: {statusPacket.MGW_ST_STATUS}%");
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_OK: //       The most common status type
                    {
                        readyToTransmit = true; //No matter which of the OK-statuses we get, we know it's allright to transmit a new packet
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x04: // MGW_STS_OK_MRF
                                {
                                    switch (statusPacket.MGW_ST_DATA[1])
                                    {
                                        case 0x00:  //MGW_STD_OKMRF_NOINFO          RF Rel. 60
                                            {
                                                DoLog("MRF OK!");
                                                break;
                                            }
                                        case 0x10://MGW_STD_OKMRF_ACK_DIRECT        RF ≥90: ACK from controlled device
                                            {
                                                DoLog("MRF OK! (Direct)");
                                                //broadcastAck()
                                                break;
                                            }

                                        case 0x20://MGW_STD_OKMRF_ACK_ROUTED        RF ≥90: ACK from routing device
                                            {
                                                DoLog("MRF OK! (Routed)");
                                                break;
                                            }

                                        case 0x30://MGW_STD_OKMRF_ACK
                                            {
                                                DoLog("MRF OK! (ACK)");
                                                break;
                                            }
                                        case 0x40://MGW_STD_OKMRF_ACK_BM            RF ≥91: ACK, device in learnmode
                                            {
                                                DoLog("MRF OK! (Device in learn mode)");
                                                break;
                                            }

                                        case 0x50://MGW_STD_OKMRF_DPREMOVED         RF ≥90: Basic Mode: DP removed
                                            {
                                                DoLog("MRF OK! (Basic, DP removed)");
                                                break;
                                            }
                                    }

                                    break;
                                }
                            case 0x05: // MGW_STS_OK_CONFIG
                                {
                                    DoLog("Config OK!");
                                    break;
                                }
                            case 0xCE: // MGW_STS_OK_BTF
                                {
                                    DoLog("BackToFactory OK!");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown status data for MGW_STT_OK: {statusPacket.MGW_ST_DATA[0]}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RELEASE:
                    {
                        DoLog($"RF-version: {statusPacket.MGW_ST_DATA[0]}.{statusPacket.MGW_ST_DATA[1]}, Firmware: {statusPacket.MGW_ST_DATA[2]}.{statusPacket.MGW_ST_DATA[3]}");
                        readyToTransmit = true;
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RS232_BAUD:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x01: //MGW_CM_BD1200
                                {
                                    DoLog($"Interface baudrate: 1200");
                                    break;
                                }
                            case 0x02: //MGW_CM_BD2400
                                {
                                    DoLog($"Interface baudrate: 2400");
                                    break;
                                }
                            case 0x03: //MGW_CM_BD4800
                                {
                                    DoLog($"Interface baudrate: 4800");
                                    break;
                                }
                            case 0x04: //MGW_CM_BD9600
                                {
                                    DoLog($"Interface baudrate: 9600");
                                    break;
                                }
                            case 0x05: //MGW_CM_BD14400
                                {
                                    DoLog($"Interface baudrate: 14400");
                                    break;
                                }
                            case 0x06: //MGW_CM_BD19200
                                {
                                    DoLog($"Interface baudrate: 19200");
                                    break;
                                }
                            case 0x07: //MGW_CM_BD38400(actually 37.500 Bit / s))
                                {
                                    DoLog($"Interface baudrate: 37500");
                                    break;
                                }
                            case 0x08: //MGW_CM_BD57600(default)
                                {
                                    DoLog($"Interface baudrate: 57600 (Default)");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown baudrate: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RS232_CRC:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //Not set
                                {
                                    DoLog($"CRC not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    DoLog($"CRC in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    DoLog($"Unknown CRC status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RS232_FLOW:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //Not set
                                {
                                    DoLog($"Flow control not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    DoLog($"Flow control in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    DoLog($"Unknown flow control status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SEND_CLASS:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //Not set
                                {
                                    DoLog($"Tg-class not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    DoLog($"Tg-class in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    DoLog($"Unknown Tg-class status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SEND_OK_MRF:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //Not set
                                {
                                    DoLog($"OK_MRF not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    DoLog($"OK_MRF in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    DoLog($"Unknown OK_MRF status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SEND_RFSEQNO:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: // Not set
                                {
                                    DoLog($"Send RF sequence number not set!");
                                    break;
                                }
                            case 0x01: // Set
                                {
                                    DoLog($"Send RF sequence number set!");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown RF sequence number status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SERIAL:
                    {
                        Console.Write($"Serial: { BitConverter.ToInt32(statusPacket.MGW_ST_DATA, 0)}");
                        Array.Reverse(statusPacket.MGW_ST_DATA);
                        DoLog($" or { BitConverter.ToInt32(statusPacket.MGW_ST_DATA, 0)} ?");

                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_TIMEACCOUNT:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00://MGW_STS_DATA        DATA contains timeaccount in %
                                {
                                    DoLog($"Timeaccount: {statusPacket.MGW_ST_DATA[0]}%");
                                    break;
                                }
                            case 0x01://MGW_STS_IS_0        no more Tx-msg possible
                                {
                                    DoLog($"No more transmissions possible!");
                                    break;
                                }
                            case 0x02://MGW_STS_LESS_10     timeaccount fell under 10%
                                {
                                    DoLog($"Timeaccount: <10% and sinking.");
                                    break;
                                }
                            case 0x03://MGW_STS_MORE_15     timeaccount climbed above 15%
                                {
                                    DoLog($"Timeaccount: >15% and rising.");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown Timeaccount status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_COUNTER_RX:
                    {
                        DoLog($"RX counter: {statusPacket.MGW_ST_STATUS}");
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_COUNTER_TX:
                    {
                        DoLog($"TX counter: {statusPacket.MGW_ST_STATUS}");
                        break;
                    }
                default:
                    {
                        DoLog($"Unknown status type: {statusPacket.MGW_ST_TYPE }", 4);
                        break;
                    }
            }
            if (!denyReady) { readyToTransmit = true; } // If not, any status would stop the entire program...
        }
        
        private static void HandleRX(Protocol.PT_RX.Packet rxPacket, bool assignPacket) // Handling packets containing info about other devices
        {
            try
            {
                // What sort of device are we getting data from?
                Datapoint datapoint = datapoints.Find(x => x.DP == rxPacket.MGW_RX_DATAPOINT);
                if (datapoint == null)
                {
                    DoLog("Datapoint " + rxPacket.MGW_RX_DATAPOINT + " was not found!", 4);
                    //Unfortunately, there is no way to create a new datapoint from the rxPacket.
                    //All that can be done is to alert the user and move on...
                    MQTT.SendMQTTMessageAsync("BachelorPad/xComfort/info",$"CI received data from an unknown datapoint: {rxPacket.MGW_RX_DATAPOINT}. Is it time to update the datapoints.txt file perhaps?").Wait();
                    return;
                }
                DeviceType devicetype = devicetypes.Find(x => x.Number == datapoint.Type);

                double[] doubleArrayData = new double[2];
                double doubleData = 0;
                //string stringData = "";


                if (assignPacket)
                {
                    DoLog("Updating datapoint...", 0);
                    datapoint.LatestDataValues = rxPacket;
                    datapoint.LastUpdate = DateTime.Now;
                }

                // And what does the data mean?
                // To be certain that we know what the data means, we might need to know several things.
                //      For room controllers, we need to know what mode it's in.
                //      For dimmers, we only need the percentage from Info Short.

                DoLog("DataType=" + Protocol.PT_RX.MGW_RX_DATA_TYPE.GetNameFromByte(devicetype.DataTypes[0]),2);


                if (devicetype.DataTypes[0] == (MGW_RDT_NO_DATA))
                {
                    //We know that we can get all the information we need from the message type.
                    switch (rxPacket.MGW_RX_MSG_TYPE)
                    {
                        case MGW_RMT_ON:
                            {
                                //The device has been turned on!
                                BroadcastChange(datapoint.DP, "ON");
                                break;
                            }
                        case MGW_RMT_OFF:
                            {
                                //The device has been turned off!
                                BroadcastChange(datapoint.DP, "OFF");
                                break;
                            }
                        case MGW_RMT_SWITCH_ON:
                            {
                                //The device has been turned on!
                                BroadcastChange(datapoint.DP, "ON");
                                break;
                            }
                        case MGW_RMT_SWITCH_OFF:
                            {
                                //The device has been turned off!
                                BroadcastChange(datapoint.DP, "OFF");
                                break;
                            }
                        case MGW_RMT_UP_PRESSED:
                            {
                                //"Up" is pressed (and held)!
                                break;
                            }
                        case MGW_RMT_UP_RELEASED:
                            {
                                //"Up" is released!
                                break;
                            }
                        case MGW_RMT_DOWN_PRESSED:
                            {
                                //"Down" is pressed (and held)!
                                break;
                            }
                        case MGW_RMT_DOWN_RELEASED:
                            {
                                //"Down" is released!
                                break;
                            }
                        case MGW_RMT_FORCED:
                            {
                                //Fixed value
                                BroadcastChange(datapoint.DP, "Fixed value");
                                break;
                            }
                        case MGW_RMT_SINGLE_ON:
                            {
                                //Single contact
                                break;
                            }
                        case MGW_RMT_VALUE:
                            {
                                //Analogue value
                                BroadcastChange(datapoint.DP, GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData).ToString());
                                break;
                            }
                        case MGW_RMT_TOO_COLD:
                            {
                                //"Cold" - This means that the temperature is below the set threshold value
                                // Depending on the xComfort installation, this might have triggered a second device, like a valve actuator or such.
                                DoLog("Too cold!");
                                break;
                            }
                        case MGW_RMT_TOO_WARM:
                            {
                                //"Warm" - This means that the temperature is above the set threshold value
                                // Depending on the xComfort installation, this might have triggered a second device, like a valve actuator or such.
                                DoLog("Too hot!");
                                break;
                            }
                        case MGW_RMT_STATUS:
                            {
                                //Data about the current status
                                BroadcastAck(rxPacket.MGW_RX_DATAPOINT, rxPacket.MGW_RX_INFO_SHORT.ToString());
                                break;
                            }
                        case MGW_RMT_BASIC_MODE:
                            {
                                //Confirmation: Assigned or Removed RF-Device
                                break;
                            }
                        default:
                            {
                                //If any unexpected values should appear, they'll be handled here.
                                DoLog($"Unexpected value: {rxPacket.MGW_RX_MSG_TYPE.ToString()}", 4);
                                break;
                            }
                    }
                }
                else
                {
                    //We need to "go deeper" to get the information we need.

                    //Since there is a different data type, we need to know more.
                    //These types have other data types than NO_DATA:
                    //5 22 23 24 26 28 51 52 53 54 55 62 65 68 69 71 72 74
                    DoLog($"Datapoint type: {datapoint.Type}", 0);
                    switch (datapoint.Type)
                    {
                        case 5:     // Room controller
                        case 51:    // Room Controller w/ Switch/Humidity CRCA-00/05
                            {
                                switch (datapoint.Channel)
                                {
                                    case 0: //  Channel 0 is temperature. The same on both device models.
                                        {
                                            switch (datapoint.Mode)
                                            {
                                                case 0:
                                                    {
                                                        //Mode 0 (Send switching commands): MGW_RDT_RC_DATA(temperature and wheel; MGW_RX_MSG_TYPE = MGW_RMT_TOO_COLD / MGW_RMT_TOO_WARM)
                                                        double[] data = new double[2];
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleArrayData);
                                                        BroadcastChange(datapoint.DP, ("Temperature: " + data[1] + ", Wheel position: " + data[0]));
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        //Mode 1 (Send temperature value):  MGW_RDT_RC_DATA(temperature and wheel; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                                        double[] data = new double[2];
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleArrayData);
                                                        BroadcastChange(datapoint.DP, ("Temperature: " + data[1] + ", Wheel position: " + data[0]));
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case 1: //  Channel 1 is humidity. Only available on the CRCA-00/05
                                        {
                                            switch (datapoint.Mode)
                                            {
                                                case 0:
                                                    {
                                                        //Mode 0 (Send switching commands): MGW_RDT_FLOAT(humidity value in percent; MGW_RX_MSG_TYPE = MGW_RMT_SWITCH_ON / MGW_RMT_SWITCH_OFF)
                                                        double data = new double();
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                        BroadcastChange(datapoint.DP, data.ToString());
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        //Mode 1 (Send humidity value):     MGW_RDT_FLOAT(humidity value in percent; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                                        double data = new double();
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                        BroadcastChange(datapoint.DP, data.ToString());
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        case 22:    // Home manager
                            {
                                // This one has 99 channels, and it's impossible to act without knowing what device is associated with each channel (which represents datapoints, actually)
                                BroadcastChange(datapoint.DP, $"Datapoint: {datapoint.Channel}, Data: {rxPacket.MGW_RX_DATA[0]} {rxPacket.MGW_RX_DATA[1]} {rxPacket.MGW_RX_DATA[2]} {rxPacket.MGW_RX_DATA[3]}");
                                break;
                            }
                        case 23:    // Temperature Input
                            {
                                switch (datapoint.Mode)
                                {
                                    case 0:
                                        {
                                            //Mode 0 (Send switching commands): MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_TOO_COLD / MGW_RMT_TOO_WARM)
                                            double data = new double();
                                            data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                            BroadcastChange(datapoint.DP, data.ToString());
                                            break;
                                        }
                                    default:
                                        {
                                            //Mode 1 (Send temperature value):  MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                            double data = new double();
                                            data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                            BroadcastChange(datapoint.DP, data.ToString());
                                            break;
                                        }
                                }
                                break;
                            }
                        case 24:    // Analog Input OR PT-1000 temperature reading
                            {
                                if (rxPacket.MGW_RX_DATA_TYPE == MGW_RDT_INT16_1POINT)
                                {
                                    //This is a temperature reading
                                    switch (datapoint.Mode)
                                    {
                                        case 0:
                                            {
                                                // This is a Too hot/Too cold value
                                                double data = new double();
                                                data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                BroadcastChange(datapoint.DP, data.ToString());
                                                break;
                                            }
                                        case 1:
                                            {
                                                // This is the temperature measurement value
                                                double data = new double();
                                                data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                BroadcastChange(datapoint.DP, data.ToString());
                                                break;
                                            }
                                    }
                                    
                                }
                                else
                                {
                                    //This is an analogue reading (0 - 10V or 0/4 - 20mA)
                                    switch (datapoint.Mode)
                                    {
                                        case 0:
                                            {
                                                // This is an ON/OFF value
                                                double data = new double();
                                                data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                BroadcastChange(datapoint.DP, data.ToString());
                                                break;
                                            }
                                        case 1:
                                            {
                                                // This is the analogue voltage/current value
                                                double data = new double();
                                                data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                BroadcastChange(datapoint.DP, data.ToString());
                                                break;
                                            }
                                        case 2: // "FORCED"
                                            {
                                                // This is a percentage value
                                                double data = new double();
                                                data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                BroadcastChange(datapoint.DP, data.ToString());
                                                break;
                                            }
                                    }
                                }
                                break;
                            }
                        //case 26:    // Room-manager
                        //    {
                        //        break;
                        //    }
                        //case 28:    // Communication Interface 
                        //    {
                        //        break;
                        //    }
                        //case 72:    // Communication Interface USB
                        //    {
                        //        break;
                        //    }
                        //case 53:    // Impulse input
                        //    {
                        //        break;
                        //    }
                        //case 54:    // EMS
                        //    {
                        //        break;
                        //    }
                        //case 55:    // E-Raditor Actuator
                        //    {
                        //        break;
                        //    }
                        //case 62:    // MEP
                        //    {
                        //        break;
                        //    }
                        //case 65:    // HRV
                        //    {
                        //        break;
                        //    }
                        //case 68:    // Rosetta Sensor
                        //    {
                        //        break;
                        //    }
                        //case 69:    // Rosetta Router
                        //    {
                        //        break;
                        //    }
                        //case 71:    // Multi Channel Heating Actuator
                        //    {
                        //        break;
                        //    }
                        //case 74:    // Switching Actuator New Generation / w Binary input / w EMS
                        //    {
                        //        break;
                        //    }
                        //case 52:    // Router(no communication possible, just ignore it)
                        //    {
                        //        break;
                        //    }
                        default:    // Other stuff
                            {
                                DoLog("Unhandled datapoint: " + datapoint.ToString(),4);
                                BroadcastChange(0, "Unhandled datapoint: " + datapoint.ToString()); //Datapoint 0 is not a valid datapoint, but the MQTT doesn't care, so it's a nice channel to monitor.
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
        }
        private static void HandleRX(Protocol.PT_RX.Packet rxPacket)
        {
            // Default for handling a packet is to assign it to the datapoint as well.
            HandleRX(rxPacket, true);
        }

        private static async void BroadcastChange(int dataPointID, string dataValue)
        {
            //This is where we tell BachelorPad about the change that has been made.
            //(Could also consider making this compatible with OpenHAB2 and other such systems, so that more could benefit from it)
            DoLog("Datapoint " + dataPointID + " (" + datapoints.Find(x => x.DP == dataPointID).Name + ") just reported value " + dataValue);
            Homie.ArrayElement arrayElement = Homie.GetArrayElement(dataPointID);
            Homie.UpdateArrayElement(arrayElement, dataValue);
            await MQTT.SendMQTTMessageAsync(arrayElement.PublishPath, dataValue);
            //await MQTT.SendMQTTMessageAsync("BachelorPad/xComfort/" + dataPointID + "/set/", dataValue);
        }

        private static async void BroadcastAck(int dataPointID, string dataValue)
        {
            //This is where we tell BachelorPad about the change that has been made.
            //(Could also consider making this compatible with OpenHAB2 and other such systems, so that more could benefit from it)
            DoLog("Datapoint " + dataPointID + " (" + datapoints.Find(x => x.DP == dataPointID).Name + ") just confirmed value " + dataValue);
            Homie.ArrayElement arrayElement = Homie.GetArrayElement(dataPointID);
            Homie.UpdateArrayElement(arrayElement, dataValue);
            await MQTT.SendMQTTMessageAsync(arrayElement.PublishPath, dataValue);
            //await MQTT.SendMQTTMessageAsync("BachelorPad/xComfort/" + dataPointID + "/ack/", dataValue);
        }

        #region "Helpers"


        //These GetDataFromPacket methods are to avoid casting the type afterwards.
        //Now, the method returns the needed data type based on the calling signature.
        //We can retireve data as Double, an array of Double, and string

        static double[] GetDataFromPacket(byte[] mgw_rx_data, byte mgw_rx_data_type, double[] data)
        {
            switch (mgw_rx_data_type)
            {
                case MGW_RDT_RC_DATA: // 4 bytes(only with room controller) : two values, first temperature, then adjustment wheel
                    {
                        double[] values = new double[2];
                        values[0] = BitConverter.ToInt16(mgw_rx_data, 0);
                        values[0] = values[0] / 10;

                        values[1] = BitConverter.ToInt16(mgw_rx_data, 2);
                        values[1] = values[1] / 10;

                        return values;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        static double GetDataFromPacket(byte[] mgw_rx_data, byte mgw_rx_data_type, double data)
        {
            switch (mgw_rx_data_type)
            {
                case MGW_RDT_NO_DATA: // No data
                    {
                        return 0;
                    }
                case MGW_RDT_PERCENT: // 1 byte: 0 = 0% ; 255 = 100%
                    {
                        int percentage = mgw_rx_data[0] / 255;
                        double ret = percentage;
                        return ret;
                    }
                case MGW_RDT_UINT8: // 1 byte, integer number unsigned
                    {
                        int value = mgw_rx_data[0];
                        double ret = value;
                        return ret;
                    }
                case MGW_RDT_INT16_1POINT: // 2 bytes, signed with one decimal (0x00FF => 25.5; 0xFFFF => -0.1)
                    {
                        double value = (BitConverter.ToInt16(mgw_rx_data, 2));

                        value = value / 10;
                        return value;
                    }
                case MGW_RDT_FLOAT: // 4 bytes, 32-bit floating-point number(IEEE 754)
                    {
                        float value = BitConverter.ToSingle(mgw_rx_data, 0);
                        double ret = value;
                        return ret;
                    }
                case MGW_RDT_UINT16: // 2 bytes, integer number unsigned
                    {
                        UInt16 value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        double ret = value;
                        return ret;
                    }
                case MGW_RDT_UINT16_1POINT: // 2 bytes, integer unsigned, value x10   (1 digit after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 10;
                        return value;
                    }
                case MGW_RDT_UINT16_2POINT: // 2 bytes, integer unsigned, value x100   (2 digits after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 100;
                        return value;
                    }
                case MGW_RDT_UINT16_3POINT: // 2 bytes, integer unsigned, value x1000   (3 digits after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 1000;
                        return value;
                    }
                case MGW_RDT_UINT32: // 4 bytes, integer number unsigned
                    {
                        return BitConverter.ToUInt32(mgw_rx_data, 0);
                    }
                case MGW_RDT_UINT32_1POINT: // 4 bytes, integer unsigned, value x10   (1 digit after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 10;
                        return value;
                    }
                case MGW_RDT_UINT32_2POINT: // 4 bytes, integer unsigned, value x100   (2 digits after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 100;
                        return value;
                    }
                case MGW_RDT_UINT32_3POINT: // 4 bytes, integer unsigned, value x1000   (3 digits after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 1000;
                        return value;
                    }
            }
            return 0;
        }

        public static string GetDataFromPacket(byte[] mgw_rx_data, byte mgw_rx_data_type, string data)
        {
            switch (mgw_rx_data_type)
            {
                case MGW_RDT_NO_DATA: // No data
                    {
                        return null;
                    }
                case MGW_RDT_PERCENT: // 1 byte: 0 = 0% ; 255 = 100%
                    {
                        int percentage = mgw_rx_data[0] / 255;
                        return percentage.ToString() + "%";
                    }
                case MGW_RDT_UINT8: // 1 byte, integer number unsigned
                    {
                        int value = mgw_rx_data[0];
                        return value.ToString();
                    }
                case MGW_RDT_INT16_1POINT: // 2 bytes, signed with one decimal (0x00FF => 25.5; 0xFFFF => -0.1)
                    {
                        double value = BitConverter.ToInt16(mgw_rx_data, 2);
                        value = value / 10;
                        return value.ToString("0.0");
                    }
                case MGW_RDT_FLOAT: // 4 bytes, 32-bit floating-point number(IEEE 754)
                    {
                        return BitConverter.ToSingle(mgw_rx_data, 0).ToString();
                    }
                case MGW_RDT_UINT16: // 2 bytes, integer number unsigned
                    {
                        return BitConverter.ToUInt16(mgw_rx_data, 2).ToString();
                    }
                case MGW_RDT_UINT16_1POINT: // 2 bytes, integer unsigned, value x10   (1 digit after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 10;
                        return value.ToString("0.0");
                    }
                case MGW_RDT_UINT16_2POINT: // 2 bytes, integer unsigned, value x100   (2 digits after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 100;
                        return value.ToString("0.00");
                    }
                case MGW_RDT_UINT16_3POINT: // 2 bytes, integer unsigned, value x1000   (3 digits after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 1000;
                        return value.ToString("0.000");
                    }
                case MGW_RDT_UINT32: // 4 bytes, integer number unsigned
                    {
                        return BitConverter.ToUInt32(mgw_rx_data, 0).ToString();
                    }
                case MGW_RDT_UINT32_1POINT: // 4 bytes, integer unsigned, value x10   (1 digit after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 10;
                        return value.ToString("0.0");
                    }
                case MGW_RDT_UINT32_2POINT: // 4 bytes, integer unsigned, value x100   (2 digits after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 100;
                        return value.ToString("0.00");
                    }
                case MGW_RDT_UINT32_3POINT: // 4 bytes, integer unsigned, value x1000   (3 digits after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 1000;
                        return value.ToString("0.000");
                    }
                case MGW_RDT_RC_DATA: // 4 bytes(only with room controller) : two values, first temperature, then adjustment wheel
                    {
                        double[] values = new double[2];
                        values[0] = BitConverter.ToInt16(mgw_rx_data, 0);
                        values[0] = values[0] / 10;

                        values[1] = BitConverter.ToInt16(mgw_rx_data, 2);
                        values[1] = values[1] / 10;

                        return values[0].ToString() + ";" + values[1].ToString();
                    }
                case MGW_RDT_TIME: // 4 bytes: hour/minute/second/0; example: 23h 59m 59s: 23 59 59 00 = Hex(17 3B 3B 00)
                    {
                        return (mgw_rx_data[0].ToString() + ":" + mgw_rx_data[1].ToString() + ":" + mgw_rx_data[2].ToString());
                    }
                case MGW_RDT_DATE: // 4 bytes: day / weekday&month / century / year; weekday is placed in the high nibble of 2nd Byte, 0=monday, ... 6=sunday; example: sunday, december 31st 2005: 31 108 20 05 = Hex(1F 6C 14 05)
                    {
                        // We need to separate out the weekday from the month
                        byte maskMonth = 0x0F;      // 00001111
                        byte month = mgw_rx_data[1];
                        month &= maskMonth;

                        //We don't really need this data, as the any modern computer system can get the weekday from a date very easily!
                        //byte weekday = mgw_rx_data[1];
                        //byte maskWeekday = 0xF0;    // 11110000
                        //weekday &= maskWeekday;

                        //Return the data as a ISO 8601 formatted string
                        return mgw_rx_data[2].ToString("00") + mgw_rx_data[3].ToString("00") + "-" + month.ToString("00") + "-" + mgw_rx_data[0].ToString("00");
                    }
                default:
                    { return "Unhandled datatype! Sorry!"; }
            }
        }

        public static void PrintByte(byte[] bytesToPrint, string caption, bool minimalistic) // Used for printing byte arrays as HEX values with spaces between. Makes reading much easier!
        {
            DoLog($"{caption}: {FormatByteForPrint(bytesToPrint, minimalistic)}");
        }

        public static void PrintByte(byte[] bytesToPrint, string caption) // Shorter signature, defaults to minimalistic printing.
        {
            PrintByte(bytesToPrint, caption, true); // Defaults to true
        }

        private static string FormatByteForPrint(byte[] bytesToPrint, bool minimalistic) // Returns a string where the byte array has been written out in HEX with spaces between each value.
        {
            string formatted = "";
            if (bytesToPrint[0] == 0) { bytesToPrint = RemoveFirstByte(bytesToPrint); } // Catches the issue with outbound data having an extra 0x00 to start with

            int printLength = bytesToPrint[0];
            if (!minimalistic || printLength > bytesToPrint.Length) { printLength = bytesToPrint.Length; } // If set, we only print the intended data, not the entire buffer that we actually have

            for (int i = 0; i < printLength; i++)
            {
                formatted += (Convert.ToString(bytesToPrint[i], 16).ToUpper().PadLeft(2, '0') + " ");
            }
            return formatted;
        }

        private static byte[] RemoveFirstByte(byte[] arrayToFix) //Returns a byte array where the first byte has been removed.
        {
            byte[] result = new byte[arrayToFix.Length - 1];
            Array.Copy(arrayToFix, 1, result, 0, arrayToFix.Length - 1);
            return result;
        }

        private static byte[] AddZeroAsFirstByte(byte[] arrayToFix) //Returns a byte array where an extra 0x00 has been added at the beginning.
        {
            byte[] result = new byte[arrayToFix.Length + 1];
            Array.Copy(arrayToFix, 0, result, 1, arrayToFix.Length);
            return result;
        }

        private static byte[] AddRS232Bytes(byte[] arrayToFix) //Returns an array with the extra start and stop bytes required for RS232 communication.
        {
            byte[] result = new byte[arrayToFix.Length + 2];
            Array.Copy(arrayToFix, 0, result, 1, arrayToFix.Length);
            result[0] = Program.Settings.RS232_STARTBYTE;
            result[result.Length - 1] = Program.Settings.RS232_STOPTBYTE;
            return result;
        }

        private static byte[] RemoveRS232Bytes(byte[] arrayToFix) //Returns a shorter array with the RS232 bytes removed.
        {
            if (arrayToFix[0] == Program.Settings.RS232_STARTBYTE) //Check that the first byte actually IS a RS232 start byte
            {
                byte[] result = new byte[arrayToFix.Length - 2];
                Array.Copy(arrayToFix, 1, result, 0, arrayToFix.Length - 2);
                return result;
            }
            else //The array isn't properly RS232 formatted, best leave it alone...
            {
                return arrayToFix;
            }
        }

        public async static Task FakeData(byte[] FakeData)
        {
            DoLog("Faking data!",4);
            await IncommingData(FakeData);
        }

        #endregion

    }
}
