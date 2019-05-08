using System;
using System.Collections.Generic;
using System.Text;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_MSG_TYPE;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_DATA_TYPE;

namespace xComfortWingman
{
     class DeviceTypeList
    {
        public List<DeviceType> ListDeviceTypes()
        {
            List<DeviceType> allDeviceTypes = new List<DeviceType>
            {
                new DeviceType(1, "push-button single", "PB", (1), new int[] { 1 }, new int[] { 0 }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(2, "push-button dual", "PB", (2), new int[] { 2 }, new int[] { 0, 1 }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(3, "push-button quad", "PB", (4), new int[] { 3 }, new int[] { 0, 1, 2, 3 }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(4, "Room Controller ( /w Switch)", "RC", (5), new int[] { 0 }, new int[] { 0 }, new byte[] { MGW_RMT_TOO_COLD, MGW_RMT_TOO_WARM }, new byte[] { MGW_RDT_RC_DATA }, "NoComment"),
                new DeviceType(5, "Room Controller ( /w Switch)", "RC", (5), new int[] { 0 }, new int[] { 1 }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_RC_DATA }, "NoComment"),
                new DeviceType(6, "Switching Actuator", "SA", (16), new int[] { 0 }, new int[] { }, new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = OFF / ON]"),
                new DeviceType(7, "Dimming Actuator", "DA", (17), new int[] { 0 }, new int[] { }, new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = 0..100]"),
                new DeviceType(8, "Jalousie Actuator", "JA", (18), new int[] { 0 }, new int[] { }, new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = STOP / OPEN / CLOSE]"),
                new DeviceType(9, "Binary Input, 230V", "BI 230", (19), new int[] { 0 }, new int[] { 0, 2 }, new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(10, "Binary Input, 230V", "BI 230", (19), new int[] { 0 }, new int[] { 1, 3 }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(11, "Binary Input, 230V", "BI 230", (19), new int[] { 1 }, new int[] { 0, 3 }, new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(12, "Binary Input, 230V", "BI 230", (19), new int[] { 1 }, new int[] { 1, 2 }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(13, "Binary Input, 230V", "BI 230", (19), new int[] { 0, 1 }, new int[] { 0, 1, 2, 3 }, new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = OFF / ON]"),
                new DeviceType(14, "Binary Input, Battery", "BI Batt", (20), new int[] { 0 }, new int[] { 0, 2 }, new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(15, "Binary Input, Battery", "BI Batt", (20), new int[] { 0 }, new int[] { 1, 3 }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(16, "Binary Input, Battery", "BI Batt", (20), new int[] { 1 }, new int[] { 0, 3 }, new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(17, "Binary Input, Battery", "BI Batt", (20), new int[] { 1 }, new int[] { 1, 2 }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(18, "Remote Control 12 Channel (old design)", "Rt 12 old", (21), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new int[] { }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(19, "Home-Manager", "HM", (22), new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 }, new int[] { }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(20, "Temperature Input", "TI", (23), new int[] { 0, 1 }, new int[] { 0 }, new byte[] { MGW_RMT_TOO_COLD, MGW_RMT_TOO_WARM }, new byte[] { MGW_RDT_INT16_1POINT }, "NoComment"),
                new DeviceType(21, "Temperature Input", "TI", (23), new int[] { 0, 1 }, new int[] { 1 }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_INT16_1POINT }, "NoComment"),
                new DeviceType(22, "Analog Input", "AI", (24), new int[] { 0, 1 }, new int[] { 0 }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF }, new byte[] { MGW_RDT_FLOAT }, "NoComment"),
                new DeviceType(23, "Analog Input", "AI", (24), new int[] { }, new int[] { 1 }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_FLOAT }, "NoComment"),
                new DeviceType(24, "Analog Input", "AI", (24), new int[] { }, new int[] { 2 }, new byte[] { MGW_RMT_FORCED }, new byte[] { MGW_RDT_PERCENT }, "NoComment"),
                new DeviceType(25, "Analog Input", "AI", (24), new int[] { 0, 1 }, new int[] { 0 }, new byte[] { MGW_RMT_TOO_COLD, MGW_RMT_TOO_WARM }, new byte[] { MGW_RDT_INT16_1POINT }, "NoComment"),
                new DeviceType(26, "Analog Input", "AI", (24), new int[] { 0, 1 }, new int[] { 1 }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_INT16_1POINT }, "NoComment"),
                new DeviceType(27, "Analog Actuator", "AA", (25), new int[] { 0 }, new int[] { }, new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = 0..100]"),
                new DeviceType(28, "Room-Manager", "RM", (26), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148 }, new int[] { }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(29, "Jalousie Actuator with Security", "JA S", (27), new int[] { 0 }, new int[] { 0, 1 }, new byte[] { MGW_RMT_STATUS }, new byte[] { MGW_RDT_NO_DATA }, "[INFO_SHORT = STOP / OPEN / CLOSE]"),
                new DeviceType(30, "Communication Interface", "CI", (28), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 }, new int[] { }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(31, "Motion Detector", "MD", (29), new int[] { 0, 1 }, new int[] { }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(32, "Remote Control 2 Channel small", "Rt 2", (48), new int[] { 0, 1 }, new int[] { }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(33, "Remote Control 12 Channel", "Rt 12", (49), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new int[] { }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(34, "Remote Control 12 Channel w/ display", "Rt 12 d", (50), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new int[] { }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(35, "Room Controller w/ Switch/Humidity", "RC s/h", (51), new int[] { 0 }, new int[] { 0 }, new byte[] { MGW_RMT_TOO_COLD, MGW_RMT_TOO_WARM }, new byte[] { MGW_RDT_RC_DATA }, "NoComment"),
                new DeviceType(36, "Room Controller w/ Switch/Humidity", "RC s/h", (51), new int[] { 0 }, new int[] { 1 }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_RC_DATA }, "NoComment"),
                new DeviceType(37, "Room Controller w/ Switch/Humidity", "RC s/h", (51), new int[] { 1 }, new int[] { 0 }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_FLOAT }, "NoComment"),
                new DeviceType(38, "Room Controller w/ Switch/Humidity", "RC s/h", (51), new int[] { 1 }, new int[] { 1 }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_FLOAT }, "NoComment"),
                new DeviceType(39, "Router (no communication possible, just ignore it)", "Router", (52), new int[] { }, new int[] { }, new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(40, "Impulse Input", "ImpI", (53), new int[] { 0, 1 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32 }, "[totalnumberofImpulses]"),
                new DeviceType(41, "EMS", "EMS", (54), new int[] { 0 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32_3POINT }, "[Energy, MGW_RMT_kWh]"),
                new DeviceType(42, "EMS", "EMS", (54), new int[] { 1 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16_1POINT }, "[Power, MGW_RMT_W]"),
                new DeviceType(43, "EMS", "EMS", (54), new int[] { 2 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16_3POINT }, "[Current, MGW_RMT_A]"),
                new DeviceType(44, "EMS", "EMS", (54), new int[] { 3 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16_2POINT }, "[Voltage, MGW_RMT_V]"),
                new DeviceType(45, "E-Raditor Actuator", "RadAct", (55), new int[] { 0, 1, 2 }, new int[] { }, new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(46, "Remote Control Alarm Pushbutton", "Rt 1", (56), new int[] { 0 }, new int[] { }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF, MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_DOWN_PRESSED, MGW_RMT_DOWN_RELEASED }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(47, "BOSCOS (Bed/Chair Occupancy Sensor)", "BOSCOS", (57), new int[] { 0, 1 }, new int[] { }, new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(48, "MEP", "MEP", (62), new int[] { 0, 1, 2, 3 }, new int[] { }, new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(49, "MEP", "MEP", (62), new int[] { 10 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT8 }, "[currenttariffinuse]"),
                new DeviceType(50, "MEP", "MEP", (62), new int[] { 11 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32_3POINT }, "[totalEnergy, MGW_RMT_Wh]"),
                new DeviceType(51, "MEP", "MEP", (62), new int[] { 13 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16 }, "[totalPower, MGW_RMT_W]"),
                new DeviceType(52, "MEP", "MEP", (62), new int[] { 15, 16, 17, 18, 19, 20, 21 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32_3POINT }, "[Energy, MGW_RMT_Wh]"),
                new DeviceType(53, "MEP", "MEP", (62), new int[] { 35, 36, 37, 38, 39, 40, 41, 42, 43, 44 }, new int[] { }, new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(54, "HRV", "HRV", (65), new int[] { 0 }, new int[] { 0 }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_HRV_OUT }, "NoComment"),
                new DeviceType(55, "Rosetta Sensor", "Ros Sens", (68), new int[] { 0 }, new int[] { 0, 2 }, new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_ROSETTA }, "NoComment"),
                new DeviceType(56, "Rosetta Sensor", "Ros Sens", (68), new int[] { 0 }, new int[] { 1, 3 }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_ROSETTA }, "NoComment"),
                new DeviceType(57, "Rosetta Sensor", "Ros Sens", (68), new int[] { 1 }, new int[] { 0, 3 }, new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(58, "Rosetta Sensor", "Ros Sens", (68), new int[] { 1 }, new int[] { 1, 2 }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(59, "Rosetta Router", "Ros Rout", (69), new int[] { }, new int[] { }, new byte[] { }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(60, "Multi Channel Heating Actuator", "MCHA", (71), new int[] { 0, 1 }, new int[] { 0 }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(61, "Multi Channel Heating Actuator", "MCHA", (71), new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, new int[] { 0 }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(62, "Multi Channel Heating Actuator", "MCHA", (71), new int[] { 14, 15 }, new int[] { 0 }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(63, "Communication Interface USB", "CI Stick", (72), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 }, new int[] { }, new byte[] { }, new byte[] { }, "NoComment"),
                new DeviceType(64, "Switching Actuator New Generation", "SA-NG", (74), new int[] { 0 }, new int[] { }, new byte[] { MGW_RMT_STATUS }, new byte[] { }, "[INFO_SHORT = OFF(0x02) / ON(0x03)]"),
                new DeviceType(65, "Switching Actuator New Generation", "SA-NG", (74), new int[] { 1 }, new int[] { 0 }, new byte[] { MGW_RMT_UP_PRESSED, MGW_RMT_UP_RELEASED, MGW_RMT_SINGLE_ON }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(66, "Switching Actuator New Generation", "SA-NG", (74), new int[] { 1 }, new int[] { 1 }, new byte[] { MGW_RMT_SWITCH_ON, MGW_RMT_SWITCH_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment"),
                new DeviceType(67, "Switching Actuator New Generation", "SA-NG", (74), new int[] { 2 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT16_1POINT }, "[Power, MGW_RMT_W]"),
                new DeviceType(68, "Switching Actuator New Generation", "SA-NG", (74), new int[] { 3 }, new int[] { }, new byte[] { MGW_RMT_VALUE }, new byte[] { MGW_RDT_UINT32_3POINT }, "[Energy, MGW_RMT_kWh]"),
                new DeviceType(69, "Switching Actuator New Generation", "SA-NG", (74), new int[] { 4 }, new int[] { }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF }, new byte[] { MGW_RDT_NO_DATA }, "[LoadError]"),
                new DeviceType(70, "Router New Generation", "RA-NG", (75), new int[] { 1, 2, 3, 4, 5 }, new int[] { }, new byte[] { MGW_RMT_ON, MGW_RMT_OFF }, new byte[] { MGW_RDT_NO_DATA }, "NoComment")
            };
            return allDeviceTypes;
        }
        
    }
}
