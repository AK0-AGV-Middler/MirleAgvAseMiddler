using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Control.Tools
{
    public class LogFormat
    {
        public string Category { get; set; }
        public string LogLevel { get; set; }
        public string ClassFunctionName { get; set; }
        public string Device { get; set; }
        public string CarrierId { get; set; }
        public string Message { get; set; }

        public LogFormat(string Category, string LogLevel, string ClassFunctionName, string Device, string CarrierId, string Message)
        {
            this.Category = Category;
            this.LogLevel = LogLevel;
            this.ClassFunctionName = ClassFunctionName;
            this.Device = Device;
            this.CarrierId = CarrierId;
            this.Message = Message;
        }

        public LogFormat(string Message) : this("Category", "LogLevel", "ClassFunctionName", "Device", "CarrierId", Message) { }
    }

}
