using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Controller.Tools
{
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
                //log ex
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
