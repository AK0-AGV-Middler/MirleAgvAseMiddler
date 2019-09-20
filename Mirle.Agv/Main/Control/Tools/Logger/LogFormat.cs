using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Controller.Tools
{
    [Serializable]
    public class LogFormat
    {
        public string Category { get; set; } = "Category";
        public string LogLevel { get; set; } = "9";
        public string ClassFunctionName { get; set; } = "ClassFunctionName";
        public string Device { get; set; } = "Device";
        public string CarrierId { get; set; } = "CarrierId";
        public string Message { get; set; } = "Message";

        public LogFormat(string Category, string LogLevel, string ClassFunctionName, string Device, string CarrierId, string Message)
        {
            this.Category = Category;
            this.LogLevel = LogLevel;
            this.ClassFunctionName = ClassFunctionName;
            this.Device = Device;
            this.CarrierId = CarrierId;
            this.Message = Message;
        }

        public LogFormat(string Message) : this("Category", "9", "ClassFunctionName", "Device", "CarrierId", Message) { }
    }

}
