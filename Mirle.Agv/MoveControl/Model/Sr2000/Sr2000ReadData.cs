using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class Sr2000ReadData
    {
        public string SendCmd { get; set; }
        public string ReceivedData { get; set; }

        public BarcodeData Barcode1 { get; set; }
        public BarcodeData Barcode2 { get; set; }

        public MapPosition TargetCenter { get; set; }

        public AGVPosition AGV { get; set; }
        public double AngleOfMapInReader { get; set; }

        public ThetaSectionDeviation ReviseData { get; set; }

        public int ScanTime { get; set; }
        public DateTime GetDataTime { get; set; }
        public uint Count { get; set; }

        public Sr2000ReadData(string[] splitResult, string sendCmd, string receivedData, uint count)
        {
            try
            {
                GetDataTime = DateTime.Now;
                SendCmd = sendCmd;
                ReceivedData = receivedData;
                Barcode1 = new BarcodeData(Int32.Parse(Regex.Replace(splitResult[0], "[^0-9]", "")), double.Parse(splitResult[1]), double.Parse(splitResult[2]));
                Barcode2 = new BarcodeData(Int32.Parse(Regex.Replace(splitResult[3], "[^0-9]", "")), double.Parse(splitResult[4]), double.Parse(splitResult[5]));

                ScanTime = Int32.Parse(Regex.Replace(splitResult[6], "[^0-9]", ""));
                AGV = null;
                ReviseData = null;
                Count = count;
            }
            catch
            {
                GetDataTime = DateTime.Now;
                SendCmd = sendCmd;
                ReceivedData = receivedData;
                AGV = null;
                ReviseData = null;
                Count = count;
            }
        }

        public Sr2000ReadData(string sendCmd, string receivedData, uint count = 0)
        {
            GetDataTime = DateTime.Now;
            SendCmd = sendCmd;
            ReceivedData = receivedData;
            ReviseData = null;
            Count = count;
        }
    }
}
