using System;
using System.Collections.Generic;
using System.Text;

namespace BachelorPad
{
    enum DataTypes
    {
        BOOLEAN, BYTE, INT, FLOAT, STRING, BOOLEAN_ARRAY, BYTE_ARRAY, INT_ARRAY, FLOAT_ARRAY, STRING_ARRAY
    }

    enum ActionTypes
    {
       OFF, ON, TOGGLE, READ, UP, DOWN, LEFT, RIGHT, OPEN, CLOSE, HOME, DIM, COLOR, SPEAK, PLAY, LISTEN, LAUNCH, SCRIPT, POST, GET
    }

    enum ThingTypes
    {
        SWITCH, LIGHT, BUTTON, TEMPERATURE, THERMOSTAT, ROOMCONTROLLER, XCOMFORT, RF433, WEBHOOK, API, USB, SERIAL, HOMECONTROLLER, GOOGLEASSISTANT, ALEXA, CORTANA, SIRI, ASSISTANT, SPEAKER
    }

    class SmartThing
    {
        public int id;
        public string name;
        public string description;
        public SmartValue smartValue;
        public DateTime timestamp;
        
        private object GetData() 
        {
            switch (this.smartValue.dataType)
            {
                case DataTypes.BOOLEAN: { return this.smartValue.valBoolean;}
                case DataTypes.BYTE: { return this.smartValue.valByte; }
                case DataTypes.INT: { return this.smartValue.valInt; }
                case DataTypes.FLOAT: { return this.smartValue.valFloat; }
                case DataTypes.STRING: { return this.smartValue.valString; }
                case DataTypes.BOOLEAN_ARRAY: { return this.smartValue.valBooleanArray; }
                case DataTypes.BYTE_ARRAY: { return this.smartValue.valByteArray; }
                case DataTypes.INT_ARRAY: { return this.smartValue.valIntArray; }
                case DataTypes.FLOAT_ARRAY: { return this.smartValue.valFloatArray; }
                case DataTypes.STRING_ARRAY: { return this.smartValue.valStringArray; }
            }
            return ("Default return value!");
        }
    }

    class SmartValue
    {
        public int id;
        public string name;
        public string description;

        public DataTypes dataType;

        public bool valBoolean;
        public byte valByte;
        public int valInt;
        public float valFloat;
        public string valString;

        public bool[] valBooleanArray;
        public byte[] valByteArray;
        public int[] valIntArray;
        public float[] valFloatArray;
        public string[] valStringArray;
    }

    class SmartAction
    {
        public int id;
        public string name;             // TurnONorOFF
        public string description;      // Turn something ON or OFF
        public ActionTypes actionType;  // ON or OFF (or toggle?)
        public bool hasFeedback;        // Does this action need any feedback to know that it's carried out?
        public bool returnsValue;      
        //TODO: Add more stuff here!
    }
}
