using System;
using System.IO;
using System.Reflection;

namespace Mirle.Agv.Controller.Tools
{
    [Serializable]
    public class CsvHandler
    {
        private string filePath;

        public CsvHandler(string filePath)
        {
            this.filePath = filePath;
        }

        public string[] GetAllRows()
        {
            try
            {
                return File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                LoggerAgent.Instance.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                return new string[1];
            }
        }

        public string[] SearchRowByIndex(int index)
        {
            string[] allRows = File.ReadAllLines(filePath);
            if (index > allRows.Length)
            {
                return new string[1];
            }
            else
            {
                return allRows[index].Split(',');
            }
        }

        public void WriteAllRows(string[] allRows)
        {
            File.WriteAllLines(filePath, allRows);
        }
    }
}
