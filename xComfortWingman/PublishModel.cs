using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    public class PublishModel
    {
        public string PublishPath { get; set; }
        public string Payload { get; set; }

        public PublishModel(string publishPath, string payload)
        {
            PublishPath = publishPath;
            Payload = payload;
        }
    }
}
