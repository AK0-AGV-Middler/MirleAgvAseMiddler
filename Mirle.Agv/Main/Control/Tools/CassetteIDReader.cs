using Keyence.AutoID.SDK;
using System;
using System.Text.RegularExpressions;

namespace Mirle.AgvAseMiddler.Controller.Tools
{
    class CassetteIDReader
    {
        private ReaderAccessor reader;

        public string Ip { get; set; } = "192.168.1.123";
        public bool ConnectionState { get; private set; } = false;

        public void Connect()
        {
            try
            {
                reader = new ReaderAccessor(Ip);
                ConnectionState = reader.Connect();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

        }

        public bool ReadBarcode(ref string cassetteID)//20190801_Rudy cassetteID =>改為ref方式
        {
            try
            {
                string receivedData = reader.ExecCommand("LON");

                if (string.IsNullOrEmpty(receivedData))
                {
                    cassetteID = "ERROR";
                    receivedData = reader.ExecCommand("LOFF");
                    return false;
                }
                else
                {
                    cassetteID = Regex.Replace(receivedData, "\r", "");
                    return true;
                }
            }
            catch (Exception ex)
            {
                cassetteID = "ERROR";
                //eqTool.Fun_Log(eqTool.MyLogKind.GeneralProcess, NLog.LogLevel.Fatal, null, "@Drv_IdReader_Keyence_BL1301_N_L20[{0}]:   \\ ReadBarcode \\      Exception= {1},  StackTrace= {2}", miControlIndex.ToString(), ex.Message, ex.StackTrace);
                var msg = ex.StackTrace;
                return false;
            }
        }
    }
}
