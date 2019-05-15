using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class Carrier
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public int Size{get;set;}       
        public int NumPieces { get; set; }
        public int StageNum { get; set; }

        public Carrier Clone()
        {
            //using (var memory = new MemoryStream())
            //{
            //    IFormatter formatter = new BinaryFormatter();
            //    formatter.Serialize(memory, this);
            //    memory.Seek(0, SeekOrigin.Begin);
            //    return (Carrier)formatter.Deserialize(memory);
            //}

            return ExtensionMethods.DeepClone(this);
        }
    }
}
