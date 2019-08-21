using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mirle.Agv.Controller.Tools
{
    [Serializable]
    public class XmlHandler
    {
        public void WriteXml(object o, string aFilename)
        {           
            string filePath = Path.Combine(Environment.CurrentDirectory, aFilename);
            XmlSerializer ser = new XmlSerializer(o.GetType());
            FileStream fileStream = File.Create(filePath);

            ser.Serialize(fileStream, o);
            fileStream.Close();
        }

        public T ReadXml<T>(string aFilename)
        {           
            string filePath = Path.Combine(Environment.CurrentDirectory, aFilename);
            XmlSerializer ser = new XmlSerializer(typeof(T));
            StreamReader fileStream = new StreamReader(filePath);
            return (T)ser.Deserialize(fileStream);
        }
    }
}
