using System.Collections.Generic;

namespace xComfortWingman.Protocol
{
    public static class PREDEFINED
    {
        public static readonly byte[] MGW_PRE_GET_TIMEACCOUNT = { 0x00, 0x04, 0xB2, 0x0A, 0x00 };
        public static readonly byte[] MGW_PRE_GET_COUNTER_RX = { 0x00, 0x04, 0xB2, 0x0B, 0x00 };
        public static readonly byte[] MGW_PRE_GET_COUNTER_TX = { 0x00, 0x04, 0xB2, 0x0C, 0x00 };
        public static readonly byte[] MGW_PRE_GET_SERIAL = { 0x00, 0x04, 0xB2, 0x0E, 0x00 };
        public static readonly byte[] MGW_PRE_GET_LED = { 0x00, 0x04, 0xB2, 0x0F, 0x00 };
        public static readonly byte[] MGW_PRE_GET_LED_STATUS = { 0x00, 0x04, 0xB2, 0x0F, 0x00 };
        public static readonly byte[] MGW_PRE_GET_LED_DEFAULT = { 0x00, 0x04, 0xB2, 0x0F, 0x01 };
        public static readonly byte[] MGW_PRE_GET_LED_REVERSED = { 0x00, 0x04, 0xB2, 0x0F, 0x02 };
        public static readonly byte[] MGW_PRE_GET_LED_OFF = { 0x00, 0x04, 0xB2, 0x0F, 0x03 };
        public static readonly byte[] MGW_PRE_GET_LED_DIM = { 0x00, 0x04, 0xB2, 0x1A, 0x00 };
        public static readonly byte[] MGW_PRE_GET_RELEASE = { 0x00, 0x04, 0xB2, 0x1B, 0x00 };
        public static readonly byte[] MGW_PRE_GET_REVISION = { 0x00, 0x04, 0xB2, 0x1B, 0x01 };
        public static readonly byte[] MGW_CT_SEND_RFSEQNO = { 0x00, 0x04, 0xB2, 0x1E, 0x00 };
        public static readonly byte[] MGW_CT_SEND_RFSEQNO_SET = { 0x00, 0x04, 0xB2, 0x1E, 0x01 };
        public static readonly byte[] MGW_CT_SEND_RFSEQNO_CLR = { 0x00, 0x04, 0xB2, 0x1E, 0x0F };
    }

    public static class MGW_TYPE
    {
        public const byte MGW_PT_TX = 0xB1;
        public const byte MGW_PT_CONFIG = 0xB2;
        public const byte MGW_PT_RX = 0xC1;
        public const byte MGW_PT_STATUS = 0xC3;
    }

    public class PT_TX
    {
        public class MGW_TX_EVENT 
        {
            public const byte MGW_TE_SWITCH = 0x0A;           // control switching actuator 
            public const byte MGW_TE_DIM = 0x0D;              // control dimming actuator 
            public const byte MGW_TE_JALO = 0x0E;             // control jalousie actuator 
            public const byte MGW_TE_PUSHBUTTON = 0x50;       // simulates a pushbutton 
            public const byte MGW_TE_REQUEST = 0x0B;          // request for MGW_RMT_STATUS 
            public const byte MGW_TE_BASIC_MODE = 0x80;       // Basic Mode specifc commands 
            public const byte MGW_TE_INT16_1POINT = 0x11;     // SendMGW_RDT_INT16_1POINT 
            public const byte MGW_TE_FLOAT = 0x1A;            // Send 4 Bytes floating-point value 
            public const byte MGW_TE_RM_TIME = 0x2A;          // Set Time of Room-Manager 
            public const byte MGW_TE_RM_DATE = 0x2B;          // Set Date of Room-Manager 
            public const byte MGW_TE_RC_DATA = 0x2C;          // Sends MGW_RDT_RC_DATA 
            public const byte MGW_TE_UINT32 = 0x30;           // Sends MGW_RDT_UINT32 
            public const byte MGW_TE_UINT32_1POINT = 0x31;    // S. MGW_RDT_UINT32_1POINT 
            public const byte MGW_TE_UINT32_2POINT = 0x32;    // S. MGW_RDT_UINT32_2POINT 
            public const byte MGW_TE_UINT32_3POINT = 0x33;    // S. MGW_RDT_UINT32_3POINT 
            public const byte MGW_TE_UINT16 = 0x40;           // Sends MGW_RDT_UINT16 
            public const byte MGW_TE_UINT16_1POINT = 0x41;    // S. MGW_RDT_UINT16_1POINT 
            public const byte MGW_TE_UINT16_2POINT = 0x42;    // S. MGW_RDT_UINT16_2POINT 
            public const byte MGW_TE_UINT16_3POINT = 0x43;    // S. MGW_RDT_UINT16_3POINT 
            public const byte MGW_TE_DIMPLEX_CONFIG = 0x44;   // setpoint temperatur and mode 
            public const byte MGW_TE_DIMPLEX_TEMP = 0x45;     // current temperature 
            public const byte MGW_TE_HRV_IN = 0x46;           // set HRV parameters 
        }

        public class MGW_TX_EVENT_DATA
        {
            public const byte MGW_TED_OFF = 0x00;             //	SWITCH: off
            public const byte MGW_TED_ON = 0x01;              //	SWITCH: on

            public const byte MGW_TED_STOP = 0x00;            //	DIM: keep current level
            public const byte MGW_TED_DARKER = 0x04;          //	DIM: dim softly down
            public const byte MGW_TED_BRIGHTER = 0x0F;        //	DIM: dim softly up

            public const byte MGW_TED_CLOSE = 0x00;           //	JALO: moving to closed pos.
            public const byte MGW_TED_OPEN = 0x01;            //	JALO: moving to opened pos.
            public const byte MGW_TED_JSTOP = 0x02;           //	JALO: stop [same cmd as STEP_CLOSE]
            public const byte MGW_TED_STEP_CLOSE = 0x10;      //	JALO: move a step to closed pos.
            public const byte MGW_TED_STEP_OPEN = 0x11;       //	JALO:move a step to opened pos.
            //	PUSHBUTTON:
            public const byte MGW_TED_UP = 0x50;              //	PB: shortly pressed, upper button
            public const byte MGW_TED_DOWN = 0x51;            //	PB: shortly pressed, lower button
            public const byte MGW_TED_UP_PRESSED = 0x54;      //	PB: pressed, upper button
            public const byte MGW_TED_UP_RELEASED = 0x55;     //	PB: released, upper button
            public const byte MGW_TED_DOWN_PRESSED = 0x56;    //	PB: pressed, lower button
            public const byte MGW_TED_DOWN_RELEASED = 0x57;   //    PB: released, lower button

            public const byte MGW_TED_DUMMY = 0x00;           //	REQUEST: get current status

            public const byte MGW_TED_LEARNMODE_ON = 0x01;    //	BASIC_MODE: see chapter 6
            public const byte MGW_TED_LEARNMODE_OFF = 0x00;   //	BASIC_MODE: see chapter 6
            public const byte MGW_TED_ASSIGN_ACTUATOR = 0x10; //	BASIC_MODE: see chapter 6
            public const byte MGW_TED_REMOVE_ACTUATOR = 0x20; //	BASIC_MODE: see chapter 6
            public const byte MGW_TED_REMOVE_SENSORS = 0x30;  //	BASIC_MODE: see chapter 6


            public const byte MGW_TED_PERCENT = 0x40;         //	DIM: set dim-level directly (for Dimplex, follow with MGW_TED_PERCENT_##)

            //	DIMPLEX_TEMP
            public const byte MGW_TED_PERCENT_0 = 0x0;        // DIMPLEX TEMP 0%
            public const byte MGW_TED_PERCENT_2 = 0x1;        // DIMPLEX TEMP 2%
            public const byte MGW_TED_PERCENT_5 = 0x2;        // DIMPLEX TEMP 5%
            public const byte MGW_TED_PERCENT_8 = 0x3;        // DIMPLEX TEMP 8%
            public const byte MGW_TED_PERCENT_11 = 0x4;       // DIMPLEX TEMP 11%
            public const byte MGW_TED_PERCENT_13 = 0x5;       // DIMPLEX TEMP 13%
            public const byte MGW_TED_PERCENT_16 = 0x6;       // DIMPLEX TEMP 16%
            public const byte MGW_TED_PERCENT_19 = 0x7;       // DIMPLEX TEMP 19%
            public const byte MGW_TED_PERCENT_22 = 0x8;       // DIMPLEX TEMP 22%
            public const byte MGW_TED_PERCENT_25 = 0x9;       // DIMPLEX TEMP 25%
            public const byte MGW_TED_PERCENT_27 = 0xA;       // DIMPLEX TEMP 27%
            public const byte MGW_TED_PERCENT_30 = 0xB;       // DIMPLEX TEMP 30%
            public const byte MGW_TED_PERCENT_33 = 0xC;       // DIMPLEX TEMP 33%
            public const byte MGW_TED_PERCENT_36 = 0xD;       // DIMPLEX TEMP 36%
            public const byte MGW_TED_PERCENT_38 = 0xE;       // DIMPLEX TEMP 38%
            public const byte MGW_TED_PERCENT_41 = 0xF;       // DIMPLEX TEMP 41%
            public const byte MGW_TED_PERCENT_44 = 0x10;      // DIMPLEX TEMP 44%
            public const byte MGW_TED_PERCENT_47 = 0x11;      // DIMPLEX TEMP 47%
            public const byte MGW_TED_PERCENT_50 = 0x12;      // DIMPLEX TEMP 50%
            public const byte MGW_TED_PERCENT_52 = 0x13;      // DIMPLEX TEMP 52%
            public const byte MGW_TED_PERCENT_55 = 0x14;      // DIMPLEX TEMP 55%
            public const byte MGW_TED_PERCENT_58 = 0x15;      // DIMPLEX TEMP 58%
            public const byte MGW_TED_PERCENT_61 = 0x16;      // DIMPLEX TEMP 61%
            public const byte MGW_TED_PERCENT_63 = 0x17;      // DIMPLEX TEMP 63%
            public const byte MGW_TED_PERCENT_66 = 0x18;      // DIMPLEX TEMP 66%
            public const byte MGW_TED_PERCENT_69 = 0x19;      // DIMPLEX TEMP 69%
            public const byte MGW_TED_PERCENT_72 = 0x1A;      // DIMPLEX TEMP 72%
            public const byte MGW_TED_PERCENT_75 = 0x1B;      // DIMPLEX TEMP 75%
            public const byte MGW_TED_PERCENT_77 = 0x1C;      // DIMPLEX TEMP 77%
            public const byte MGW_TED_PERCENT_80 = 0x1D;      // DIMPLEX TEMP 80%
            public const byte MGW_TED_PERCENT_83 = 0x1E;      // DIMPLEX TEMP 83%
            public const byte MGW_TED_PERCENT_86 = 0x1F;      // DIMPLEX TEMP 86%
            public const byte MGW_TED_PERCENT_88 = 0x20;      // DIMPLEX TEMP 88%
            public const byte MGW_TED_PERCENT_91 = 0x21;      // DIMPLEX TEMP 91%
            public const byte MGW_TED_PERCENT_94 = 0x22;      // DIMPLEX TEMP 94%
            public const byte MGW_TED_PERCENT_97 = 0x23;      // DIMPLEX TEMP 97%
            public const byte MGW_TED_PERCENT_100 = 0x24;     // DIMPLEX TEMP 100%

            //	DIMPLEX_CONFIG
            public const byte MGW_TED_DPLMODE_BACKUP = 0x01;  //	setpoint , current temp. internal
            public const byte MGW_TED_DPLMODE_OFFICE = 0x02;  //	setpoint , current temp. internal
            public const byte MGW_TED_DPLMODE_CMF_EXT = 0x03; //	setpoint, current temp. external
            public const byte MGW_TED_DPLMODE_ECO_EXT = 0x04; //	setpoint, curr. temp. ext., LED on
            public const byte MGW_TED_DPLMODE_OFF = 0x05;     //	OFF                 
        }

        public class MGW_TX_PRIORITY
        {
            public const byte MGW_TP_DEFAULT  =	0x0;        //(= MGW_TP_STANDARD)
            public const byte MGW_TP_LOWEST = 0x1;
            public const byte MGW_TP_LOW = 0x2;
            public const byte MGW_TP_STANDARD = 0x3;
            public const byte MGW_TP_HIGH = 0x4;
            public const byte MGW_TP_HIGHEST = 0x5;

        }
    }

    public static class PT_CONFIG
    {
        public static class MGW_CF_TYPE
        {
            public const byte MGW_CT_CONNEX = 0x02;           //	select interfaceselect RS232 baud
            public const byte MGW_CT_RS232_BAUD = 0x03;       //	rate
            public const byte MGW_CT_SEND_OK_MRF = 0x04;      //	send MGW_STS_OK_MRF (if Txcmd accepted and sent)
            public const byte MGW_CT_RS232_FLOW = 0x05;       //	Use RTS/CTS Flow Control
            public const byte MGW_CT_RS232_CRC = 0x06;        //	Use 2 Byte CRC for RS232
            public const byte MGW_CT_TIMEACCOUNT = 0x0A;      //	Reads current Timeaccount (in %)
            public const byte MGW_CT_COUNTER_RX = 0x0B;       //	Reads current Rx-msg-counter
            public const byte MGW_CT_COUNTER_TX = 0x0C;       //	Reads current Tx-msg-counter
            public const byte MGW_CT_SERIAL = 0x0E;           //	get serial no.
            public const byte MGW_CT_LED = 0x0F;              //	change LED behaviour
            public const byte MGW_CT_LED_DIM = 0x1A;          //	dim LEDs
            public const byte MGW_CT_RELEASE = 0x1B;          //	get release/revision no.
            public const byte MGW_CT_SEND_CLASS = 0x1D;       //	set to send Tg-class
            public const byte MGW_CT_SEND_RFSEQNO = 0x1E;     //	set to send RF sequence no
            public const byte MGW_CT_BACK_TO_FACTORY = 0x1F;  //	reset settings
        }

        public static class MGW_CF_MODE
        {
            //Case of MGW_CT_CONNEX
            public const byte MGW_CM_AUTO =	0x01;           //	autmatic selection of interface (default) 
            public const byte MGW_CM_USB = 0x02;            //	select USB interface
            public const byte MGW_CM_RS232 = 0x03;          //	select RS232 interface
            public const byte MGW_CM_STATUS = 0x00;         //	get back current selection
            //Case of MGW_CT_RS232_BAUD                     //	select baud rates for RS232 interf.:
            public const byte MGW_CM_BD1200 = 0x01;         //   1200 Baud
            public const byte MGW_CM_BD2400 = 0x02;         //   2400 Baud
            public const byte MGW_CM_BD4800 = 0x03;         //   4800 Baud	
            public const byte MGW_CM_BD9600 = 0x04;         //	 9600 Baud
            public const byte MGW_CM_BD14400 = 0x05;        //	14400 Baud
            public const byte MGW_CM_BD19200 = 0x06;        //	19200 Baud
            public const byte MGW_CM_BD38400 = 0x07;        //	38400 Baud (actually 37.500 Bit/s))
            public const byte MGW_CM_BD57600 = 0x08;        //	57600 Baud (default)   
            //public const byte MGW_CM_STATUS = 0x00;       //	
            //Case of MGW_CT_TIMEACCAOUNT                   //	remaining no. of Tx-msg within 1h
            //public const byte MGW_CM_STATUS = 0x00;       //	in % (max. 1000 msg/h)
            //Case of MGW_CT_SEND_OK_MRF and
            //Case of MGW_CT_SEND_CLASS and
            //Case of MGW_CT_SEND_RFSEQNO and
            //Case of MGW_CT_RS232_FLOW and
            //Case of MGW_CT_RS232_CRC:
            public const byte MGW_CM_CLEAR =	0x0F;       //	(default f._RS232_FLOW, _CRC)
            public const byte MGW_CM_SET   =	0x01;       //	(default for SEND_OK_MRF) 
            //public const byte MGW_CM_STATUS = 0x00; 	
            //Case of MGW_CT_COUNTER_RX and                 //	Get according values
            //Case of MGW_CT_COUNTER_TX and
            //Case of MGW_CT_COUNTER_SERIAL and
            public const byte MGW_CM_GET = 0x00;            //	
            //Case of MGW_CT_LED:	                        //	LED standard mode (default)
            public const byte MGW_CM_LED_STANDARD = 0x01;   //	switch green LED to "reverse" fct.
            public const byte MGW_CM_REVERSE_GREEN = 0x02;  //	switch LEDs completely off
            public const byte MGW_CM_LED_OFF = 0x03;        //	returns status of LED-mode
            //public const byte MGW_CM_STATUS = 0x00; 	
            //Case of MGW_CT_LED_DIM:	=		;           //	Dim-Level in percent
                                                            //  Percent value(1 – 100 = 0x01 – 0x64)
            //public const byte MGW_CM_STATUS = 0x00;       //	get back current setting
            //Case of MGW_CT_RELEASE:
            //public const byte MGW_CM_GET = 0x00;          //	Get Release-No
            public const byte MGW_CM_GET_REVISION = 0x10;   //	Get Revision-No
            //Case of MGW_CT_BACK_TO_FACTORY:
            public const byte MGW_CM_BTF_GW = 0x0F;         //	Only Gateway-Settings
            public const byte MGW_CM_BTF_MRF = 0xF0;        //	Only RF-Settings
            public const byte MGW_CM_BTF_ALL = 0xFF;        //	All Settings
        }
    }

    public static class PT_RX
    {

        public class Packet
        {
            public byte MGW_LEN { get; set; }           //  1   12      Length of the packet in Byte
            public byte MGW_TYPE { get; set; }          //  1   0xC1    MGW_PT_RX
            public byte MGW_RX_DATAPOINT { get; set; }  //  1   (1-99)  Data point of origin of radiogram
            public byte MGW_RX_MSG_TYPE { get; set; }   //  1
            public byte MGW_RX_DATA_TYPE { get; set; }  //  1
            public byte MGW_RX_INFO_SHORT { get; set; } //  1           Fast access to data, in particular to MGW_RMT_STATUS
            public byte[] MGW_RX_DATA { get; set; }     //  4           Complete data value, format according to DATA_TYPE, High byte first
            public byte MGW_RX_RSSI { get; set; }       //  1           Received Signal Strength Indication: < 50 very good; > 90 very bad
            public byte MGW_RX_BATTERY { get; set; }    //  1           Bit 1-5: Current Battery Status
                                                        //              Bit 6: may contain Tg-class; if 1, it is cyclic;
                                                        //              Bit 7-8: Reserved
            public byte MGW_RX_EXTENDED { get; set; }   //  1           Bit 1-4: RF sequence number, if MGW_CT_SEND_RFSEQNO is enabled
                                                        //              Bit 5-8: Reserved

            //                 Length        Type           Datapoint              Msg type              Data type              Info short               {   Data 0          Data 1          Data 2          Data 3   }      RSSI            Battery        Extended
            public Packet(byte mGW_LEN, byte mGW_TYPE, byte mGW_RX_DATAPOINT, byte mGW_RX_MSG_TYPE, byte mGW_RX_DATA_TYPE, byte mGW_RX_INFO_SHORT, byte[] mGW_RX_DATA, byte mGW_RX_RSSI, byte mGW_RX_BATTERY, byte mGW_RX_EXTENDED)
            {
                MGW_LEN = mGW_LEN;
                MGW_TYPE = mGW_TYPE;
                MGW_RX_DATAPOINT = mGW_RX_DATAPOINT;
                MGW_RX_MSG_TYPE = mGW_RX_MSG_TYPE;
                MGW_RX_DATA_TYPE = mGW_RX_DATA_TYPE;
                MGW_RX_INFO_SHORT = mGW_RX_INFO_SHORT;
                MGW_RX_DATA = mGW_RX_DATA;
                MGW_RX_RSSI = mGW_RX_RSSI;
                MGW_RX_BATTERY = mGW_RX_BATTERY;
                MGW_RX_EXTENDED = mGW_RX_EXTENDED;
            }
        }

        public class MGW_RX_MSG_TYPE
        {
            public const byte MGW_RMT_ON = 0x50;              // "On"
            public const byte MGW_RMT_OFF = 0x51;             // "Off"
            public const byte MGW_RMT_SWITCH_ON = 0x52;       // "On"
            public const byte MGW_RMT_SWITCH_OFF = 0x53;      // "Off"
            public const byte MGW_RMT_UP_PRESSED = 0x54;      // "Up" is pressed
            public const byte MGW_RMT_UP_RELEASED = 0x55;     // "Up" is released
            public const byte MGW_RMT_DOWN_PRESSED = 0x56;    // "Down" is pressed
            public const byte MGW_RMT_DOWN_RELEASED = 0x57;   // "Down" is released
            public const byte MGW_RMT_FORCED = 0x5A;          // Fixed value
            public const byte MGW_RMT_SINGLE_ON = 0x5B;       // Single contact
            public const byte MGW_RMT_VALUE = 0x62;           // Analogue value
            public const byte MGW_RMT_TOO_COLD = 0x63;        // "Cold"
            public const byte MGW_RMT_TOO_WARM = 0x64;        // "Warm"
            public const byte MGW_RMT_STATUS = 0x70;          // Data contains Info about current Status
            public const byte MGW_RMT_BASIC_MODE = 0x80;      // Confirmation: Assigned or Removed RF-Device

            public static string GetNameFromByte(byte type)
            {
                Dictionary<int, string> dict = new Dictionary<int, string>
                {
                    { 0x50, "'On'" },
                    { 0x51, "'Off'" },
                    { 0x52, "'On'" },
                    { 0x53, "'Off'" },
                    { 0x54, "'Up' is pressed" },
                    { 0x55, "'Up' is released" },
                    { 0x56, "'Down' is pressed" },
                    { 0x57, "'Down' is released" },
                    { 0x5A, "Fixed value" },
                    { 0x5B, "Single contact" },
                    { 0x62, "Analogue value" },
                    { 0x63, "'Cold'" },
                    { 0x64, "'Warm'" },
                    { 0x70, "Data contains Info about current Status" },
                    { 0x80, "Confirmation: Assigned or Removed RF-Device" }
                };

                dict.TryGetValue(System.Convert.ToInt32(type), out string tmp);
                return tmp;
            }

            public static string GetTechnicalNameFromByte(byte type)
            {
                Dictionary<int, string> dict = new Dictionary<int, string>
                {
                    { 0x50, "MGW_RMT_ON" },
                    { 0x51, "MGW_RMT_OFF" },
                    { 0x52, "MGW_RMT_SWITCH_ON" },
                    { 0x53, "MGW_RMT_SWITCH_OFF" },
                    { 0x54, "MGW_RMT_UP_PRESSED" },
                    { 0x55, "MGW_RMT_UP_RELEASED" },
                    { 0x56, "MGW_RMT_DOWN_PRESSED" },
                    { 0x57, "MGW_RMT_DOWN_RELEASED" },
                    { 0x5A, "MGW_RMT_FORCED" },
                    { 0x5B, "MGW_RMT_SINGLE_ON" },
                    { 0x62, "MGW_RMT_VALUE" },
                    { 0x63, "MGW_RMT_TOO_COLD" },
                    { 0x64, "MGW_RMT_TOO_WARM" },
                    { 0x70, "MGW_RMT_STATUS" },
                    { 0x80, "MGW_RMT_BASIC_MODE" },
                };

                dict.TryGetValue(System.Convert.ToInt32(type), out string tmp);
                return tmp;
            }
        }

        public static class MGW_RX_DATA_TYPE
        {
            public const byte MGW_RDT_NO_DATA = 0x00;         // No data
            public const byte MGW_RDT_PERCENT = 0x01;         // 1 byte: 0 = 0% ; 255 = 100%
            public const byte MGW_RDT_UINT8 = 0x02;           // 1 byte, integer number unsigned
            public const byte MGW_RDT_INT16_1POINT = 0x03;    // 2 bytes, signed with one decimal (0x00FF => 25.5; 0xFFFF => -0.1)
            public const byte MGW_RDT_FLOAT = 0x04;           // 4 bytes, 32-bit floating-point number(IEEE 754)
            public const byte MGW_RDT_UINT16 = 0x0D;          // 2 bytes, integer number unsigned
            public const byte MGW_RDT_UINT16_1POINT = 0x21;   // 2 bytes, integer unsigned, value x10   (1 digit after point)
            public const byte MGW_RDT_UINT16_2POINT = 0x22;   // 2 bytes, integer unsigned, value x100  (2 digits after point)
            public const byte MGW_RDT_UINT16_3POINT = 0x23;   // 2 bytes, integer unsigned, value x1000 (3 digits after point)
            public const byte MGW_RDT_UINT32 = 0x0E;          // 4 bytes, integer number unsigned
            public const byte MGW_RDT_UINT32_1POINT = 0x0F;   // 4 bytes, integer unsigned, value x10   (1 digit after point)
            public const byte MGW_RDT_UINT32_2POINT = 0x10;   // 4 bytes, integer unsigned, value x100  (2 digits after point)
            public const byte MGW_RDT_UINT32_3POINT = 0x11;   // 4 bytes, integer unsigned, value x1000 (3 digits after point)
            public const byte MGW_RDT_RC_DATA = 0x17;         // 4 bytes(only with room controller) : two values, first temperature, then adjustment wheel
            public const byte MGW_RDT_TIME = 0x1E;            // 4 bytes: hour/minute/second/0; example: 23h 59m 59s: 23 59 59 00 = Hex(17 3B 3B 00)
            public const byte MGW_RDT_DATE = 0x1F;            // 4 bytes: day / weekday&month / century / year; weekday is placed in the high nibble of 2nd Byte, 0=monday, ... 6=sunday; example: sunday, december 31st 2005: 31 108 20 05 = Hex(1F 6C 14 05)
            public const byte MGW_RDT_ROSETTA = 0x35;         // 4 bytes: 
                                                        //      1st: PRC_Number 
                                                        //      2nd,Bitmask 0bbb tttt: bbb: battery status of PRC(see chapter 4.1.5); tttt: Tg-no of PRC(0..15)
                                                        //      3rd: RSSI of PRC(see chapter 4.1.4)
                                                        //      4th: Status(currently always = 0x00)
            public const byte MGW_RDT_HRV_OUT = 0x37;         // 4 bytes: 
                                                        //      1st: Error Status
                                                        //      2nd: Valve position(0% to 100%)
                                                        //      3rd, Bitmask rrrr tttt: rrrr: Request index; tttt: bit 11 to 8 of HRV-Temperature
                                                        //      4th: bit 7 to 0 of HRV-Temperature        
            public static string GetNameFromByte(byte type)
            {
                Dictionary<int, string> dict = new Dictionary<int, string>
                {
                    { 0x00, "MGW_RDT_NO_DATA" },
                    { 0x01, "MGW_RDT_PERCENT" },
                    { 0x02, "MGW_RDT_UINT8" },
                    { 0x03, "MGW_RDT_INT16_1POINT" },
                    { 0x04, "MGW_RDT_FLOAT" },
                    { 0x0D, "MGW_RDT_UINT16" },
                    { 0x21, "MGW_RDT_UINT16_1POINT" },
                    { 0x22, "MGW_RDT_UINT16_2POINT" },
                    { 0x23, "MGW_RDT_UINT16_3POINT" },
                    { 0x0E, "MGW_RDT_UINT32" },
                    { 0x0F, "MGW_RDT_UINT32_1POINT" },
                    { 0x10, "MGW_RDT_UINT32_2POINT" },
                    { 0x11, "MGW_RDT_UINT32_3POINT" },
                    { 0x17, "MGW_RDT_RC_DATA" },
                    { 0x1E, "MGW_RDT_TIME" },
                    { 0x1F, "MGW_RDT_DATE" },
                    { 0x35, "MGW_RDT_ROSETTA" }
                };

                dict.TryGetValue(System.Convert.ToInt32(type), out string tmp);
                return tmp;
            }
            public static string GetCommonNameFromByte(byte type)
            {
                Dictionary<int, string> dict = new Dictionary<int, string>
                {                       //integer, float, boolean, string, enum, color
                    { 0x00, "nothing" },
                    { 0x01, "percent" },
                    { 0x02, "integer" },
                    { 0x03, "decimal" },
                    { 0x04, "float" },
                    { 0x0D, "integer" },
                    { 0x21, "decimal" },
                    { 0x22, "decimal" },
                    { 0x23, "decimal" },
                    { 0x0E, "integer" },
                    { 0x0F, "decimal" },
                    { 0x10, "decimal" },
                    { 0x11, "decimal" },
                    { 0x17, "integer" },
                    { 0x1E, "time" },
                    { 0x1F, "date" },
                    { 0x35, "rosetta" }
                };

                dict.TryGetValue(System.Convert.ToInt32(type), out string tmp);
                return tmp;
            }

            public static string GetHomieNameFromByte(byte type)
            {
                Dictionary<int, string> dict = new Dictionary<int, string>
                {                       //integer, float, boolean, string, enum, color
                    { 0x00, "string" },
                    { 0x01, "integer" },
                    { 0x02, "integer" },
                    { 0x03, "float" },
                    { 0x04, "float" },
                    { 0x0D, "integer" },
                    { 0x21, "float" },
                    { 0x22, "float" },
                    { 0x23, "float" },
                    { 0x0E, "integer" },
                    { 0x0F, "float" },
                    { 0x10, "float" },
                    { 0x11, "float" },
                    { 0x17, "integer" },
                    { 0x1E, "string" },
                    { 0x1F, "string" },
                    { 0x35, "string" }
                };

                dict.TryGetValue(System.Convert.ToInt32(type), out string tmp);
                return tmp;
            }
        }

        public static class MGW_RX_INFO_SHORT
        {
            //Switching Actuator, Binary Input (mains powered)
            public const byte MGW_RIS_OFF = 0x00;
            public const byte MGW_RIS_ON = 0x01;

            //Jalousie actuator - Please note that OPEN and CLOSE only appears during the movement! To determine the absolute position of the jalousie, you have to "calculate" it in your application!
            public const byte MGW_RIS_STOP = 0x00;
            public const byte MGW_RIS_OPEN = 0x01;
            public const byte MGW_RIS_CLOSE = 0x02;

            //Dimming Actuator
            //0:100, the current dim-level in percent
        }

        public static class MGW_RX_RSSI
        {
            //The RSSI value (Received Signal Strength Indication) has the unit “-dBm”; therefore higher values are worse:

            //Good      ≤ 67            ≤ 0x43
            //Normal    67 < X ≤ 75     0x43 < X ≤ 0x4B
            //Weak      75 < X ≤ 90     0x4B < X ≤ 0x5A
            //Very Weak 90 < X ≤ 120    0x5A < X ≤ 0x78

            public const byte GOOD = 0x43;
            public const byte NORMAL = 0x4B;
            public const byte WEAK = 0x5A;
            public const byte VERY_WEAK = 0x78;
        }

        public static class MGW_RX_BATTERY
        {
            //If MGW_CT_SEND_CLASS is SET, and the Telegram is cyclic, Bit 6 is set to 1; therefore add 0x20 to all values
            public const byte MGW_RB_NA = 0x00;   // Not available(no information)
            public const byte MGW_RB_0 =0x01;     // Empty
            public const byte MGW_RB_25 =0x02;    // very weak ->Change it immediately
            public const byte MGW_RB_50 =0x03;    // weak -> Change it
            public const byte MGW_RB_75 =0x04;    // Good
            public const byte MGW_RB_100 =0x05;   // New
            public const byte MGW_RB_PWR =0x10;   // mains-operated
        }

        public static class MGW_RX_BATTERY_Bit6Set
        {
            //If MGW_CT_SEND_CLASS is SET, and the Telegram is cyclic, Bit 6 is set to 1; therefore add 0x20 to all values
            public const byte MGW_RB_NA = 0x20;   // Not available(no information)
            public const byte MGW_RB_0 = 0x21;     // Empty
            public const byte MGW_RB_25 = 0x22;    // very weak ->Change it immediately
            public const byte MGW_RB_50 = 0x23;    // weak -> Change it
            public const byte MGW_RB_75 = 0x24;    // Good
            public const byte MGW_RB_100 = 0x25;   // New
            public const byte MGW_RB_PWR = 0x30;   // mains-operated
        }
    }

    public static class PT_STATUS
    {

        public class Packet
        {
            public byte MGW_LEN { get; set; }           //  1   0x08    Length of the packet in Byte
            public byte MGW_TYPE { get; set; }          //  1   0xC3    MGW_PT_STATUS
            public byte MGW_ST_TYPE { get; set; }       //  1           Indicates the type of Status messsage
            public byte MGW_ST_STATUS { get; set; }     //  1
            public byte[] MGW_ST_DATA { get; set; }     //  4           Contains the data for the status

            //                 Length        Type           StatusType        Status                StatusData
            public Packet(byte mGW_LEN, byte mGW_TYPE, byte mGW_ST_TYPE, byte mGW_ST_STATUS, byte[] mGW_ST_DATA)
            {
                MGW_LEN = mGW_LEN;
                MGW_TYPE = mGW_TYPE;
                MGW_ST_TYPE = mGW_ST_TYPE;
                MGW_ST_STATUS = mGW_ST_STATUS;
                MGW_ST_DATA = mGW_ST_DATA;
                
            }
        }


        public static class MGW_ST_TYPE
        {
            public const byte MGW_STT_CONNEX = 0x02;          //  (Gateway-Status)
            public const byte MGW_STT_RS232_BAUD = 0x03;
            public const byte MGW_STT_RS232_FLOW = 0x05;
            public const byte MGW_STT_RS232_CRC = 0x06;
            public const byte MGW_STT_ERROR = 0x09;
            public const byte MGW_STT_TIMEACCOUNT = 0x0A;
            public const byte MGW_STT_COUNTER_RX = 0x0B;
            public const byte MGW_STT_COUNTER_TX = 0x0C;
            public const byte MGW_STT_SEND_OK_MRF = 0x0D;
            public const byte MGW_STT_SERIAL = 0x0E;
            public const byte MGW_STT_LED = 0x0F;
            public const byte MGW_STT_LED_DIM = 0x1A;
            public const byte MGW_STT_RELEASE = 0x1B;
            public const byte MGW_STT_OK = 0x1C;
            public const byte MGW_STT_SEND_CLASS = 0x1D;
            public const byte MGW_STT_SEND_RFSEQNO = 0x1E;
        }

        public static class MGW_ST_STATUS
        {
            //	Case of MSTT_ERROR:				
            public const byte MGW_STS_GENERAL = 0x00;         //	General Error-Msg (DATA: specific code)
            public const byte MGW_STS_UNKNOWN = 0x01;         //	Msg Unknown(DATA: specific code)
            public const byte MGW_STS_DP_OOR = 0x02;          //	Datapoint out of range
            public const byte MGW_STS_BUSY_MRF = 0x03;        //	RF Busy (Tx Msg lost)
            public const byte MGW_STS_BUSY_MRF_RX = 0x04;     //	RF Busy (Rx in progress)
            public const byte MGW_STS_TX_MSG_LOST = 0x05;     //	Tx-Msg lost, repeat it (buffer full)
            public const byte MGW_STS_NO_ACK = 0x06;          //	RF ≥90: Timeout, no ACK received!

            //	Case of MSTT_TIMEACCOUNT:				
            public const byte MGW_STS_DATA = 0x00;            //	DATA contains timeaccount in %
            public const byte MGW_STS_IS_0 = 0x01;            //	no more Tx-msg possible
            public const byte MGW_STS_LESS_10 = 0x02;         //	timeaccount fell under 10%
            public const byte MGW_STS_MORE_15 = 0x03;         //	timeaccount climbed above 15%

            //Case of MSTT_SERIAL:				
            //public const byte MGW_STS_DATA = 0x00;          //	DATA contains serial number

            //	Case of MGW_STT_RELEASE:				
            //public const byte MGW_STS_DATA = 0x00;          //	DATA contains Release-Numbers
            public const byte MGW_STS_REVISION = 0x10;        //	DATA contains Revision-Numbers

            //	Case of MGW_STT_OK				
            public const byte MGW_STS_OK_MRF = 0x04;          //	Tx Msg successfully sent to Gw
            public const byte MGW_STS_OK_CONFIG = 0x05;       //	after CONFIG-pkt. without stat. rsp.
            public const byte MGW_STS_OK_BTF = 0xCE;          //	BackToFactory of MRF-Part OK
        }

        public static class MGW_ST_DATA
        {
            //	Case of MSTT_ERROR:					
            //	MGW_TX_PRIORITY	=	unchanged	;       //	inclucing sequence number	
            public const byte MGW_STD_OKMRF_NOINFO = 0x00;    //	RF Rel. 60	
            public const byte MGW_STD_OKMRF_ACK_DIRECT = 0x10; //	RF ≥90: ACK from controlled device	
            public const byte MGW_STD_OKMRF_ACK_ROUTED = 0x20; //	RF ≥90: ACK from routing device	
            public const byte MGW_STD_OKMRF_ACK = 0x30;       //	N/A	
            public const byte MGW_STD_OKMRF_ACK_BM = 0x40;    //	RF ≥91: ACK, device in learnmode	
            public const byte MGW_STD_OKMRF_DPREMOVED = 0x80; //	RF ≥90: Basic Mode: DP removed	
                                                        //	Case of _TIMEACCOUNT – _DATA:				example: 06 00 01 0A means:	
                                                        //	Timeaccount in percent				RF-Version 6.0, Fw-Version 1.10	
                                                        //	Case of _SERIAL – DATA:				example: 00 3C 01 C8 means:	
                                                        //	32-bit Serial No.				Rev. Hw 0, RF 60, Firmwware 456	
                                                        //	Case of _RELEASE – DATA:				meaning of error code in error.h	
                                                        //	1					
                                                        //	st,2nd RF-Version; 3rd,4th Firmware-Version					
                                                        //	Case of _RELEASE – _REVISION					
                                                        //	1					
                                                        //	st Hw-Revision, 2nd RF-Rev.; 3rd,4th Fw-Rev.					
                                                        //	Case of MSTT_ERROR:					
                                                        //	1					
                                                        //	st : May contain more specific error code					
                                                        //	2					
                                                        //	nd: unchanged MGW_TX_PRIORITY Byte					
                                                        //	Case of all others: Empty					
        }
    }
}
