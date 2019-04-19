using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class Carrier
    {
        private string Id;
        private string Type;
        private int Size;
        private bool EmptyFlag;
        private int numPieces;
        private int stageNum;

        public string GetId()
        {
            return Id;            
        }

        public int GetStageNum()
        {
            return stageNum;
        }

        public Carrier Clone()
        {
            Carrier aCarrier = new Carrier();
            aCarrier.Id = this.Id;
            aCarrier.Type = this.Type;
            aCarrier.Size = this.Size;
            aCarrier.EmptyFlag = this.EmptyFlag;
            aCarrier.numPieces = this.numPieces;
            aCarrier.stageNum = this.stageNum;

            return aCarrier;            
        }
    }
}
