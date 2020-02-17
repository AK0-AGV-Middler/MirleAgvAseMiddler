using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Mirle.AgvAseMiddler.Controller.Tools
{
    [Serializable]
    public class ConfigHandler
    {
        private const int STRING_BUILDER_SIZE = 32767;

        public string FilePath { get; private set; }
        public string SectionName { get; private set; }
        public string KeyName { get; private set; }
        public string DefaultValue { get; private set; }


        /// <summary>
        /// 建構子,預設檔案路徑為 StartupPath\config.ini
        /// </summary>
        public ConfigHandler()
        {
            try
            {
                FilePath = Application.StartupPath + @"\Configs.ini";
                CheckFilePathExist();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// 建構子,填入檔案路徑
        /// </summary>
        /// <param name="FilePath"></param>
        public ConfigHandler(string FilePath)
        {
            try
            {
                this.FilePath = FilePath;
                CheckFilePathExist();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        // 2.讀取資料      
        /// <summary>
        /// 讀取資料並轉型成string
        /// </summary>
        /// <param name="SectionName"></param>
        /// <param name="KeyName"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public string GetString(string SectionName, string KeyName, string DefaultValue)
        {
            try
            {
                SetupGetString(SectionName, KeyName, DefaultValue);

                string result = GetValue();

                return CheckResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return DefaultValue;
            }
        }

        /// <summary>
        /// 填入參數
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="keyName"></param>
        /// <param name="defaultValue"></param>
        private void SetupGetString(string sectionName, string keyName, string defaultValue)
        {
            this.SectionName = sectionName;
            this.KeyName = keyName;
            this.DefaultValue = defaultValue;
        }

        /// <summary>
        /// 檢查讀取結果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string CheckResult(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return DefaultValue;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// 透過Windows方法讀取Config內容
        /// </summary>
        /// <returns></returns>
        private string GetValue()
        {
            try
            {
                StringBuilder buffer = new StringBuilder(STRING_BUILDER_SIZE);

                GetIniData(SectionName, KeyName, DefaultValue, buffer, STRING_BUILDER_SIZE, FilePath);

                return buffer.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// 檢查Config檔案路徑是否存在，若否則跳出例外
        /// </summary>
        private void CheckFilePathExist()
        {
            if (!File.Exists(FilePath))
            {
                CheckDirPathExist();

                FilePath = Path.ChangeExtension(FilePath, @".ini");

                File.Create(FilePath);

                Console.WriteLine("Config file is not exist.");
                Console.WriteLine("Generate a file with FilePath.");
            }
        }

        /// <summary>
        /// 確認FilePath所在的資料夾，若無提供則幫補上，若提供的資料夾不存在則產生一個。
        /// </summary>
        private void CheckDirPathExist()
        {
            string dir = Path.GetDirectoryName(FilePath);

            if (string.IsNullOrWhiteSpace(dir))
            {
                FilePath = Path.Combine(@"D:\", FilePath);
                Console.WriteLine("FilePath is not full path.");
                Console.WriteLine("Reset FilePath into full path.");
            }
            else
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Console.WriteLine("The directory of FilePath is not exist.");
                    Console.WriteLine("Generate the directory in FilePath.");
                }
            }
        }

        /// <summary>
        /// 讀取資料並轉型成bool
        /// </summary>
        /// <param name="SectionName"></param>
        /// <param name="KeyName"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public bool GetBool(string SectionName, string KeyName, string DefaultValue)
        {
            try
            {
                var result = GetString(SectionName, KeyName, DefaultValue);
                return CheckIsTrue(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        private static bool CheckIsTrue(string result)
        {
            return (result.ToLower().Trim() == "true" || result == "5");
        }

        // 3.寫入資料

        /// <summary>
        /// 將string型態的數值寫入資料
        /// </summary>
        /// <param name="SectionName"></param>
        /// <param name="KeyName"></param>
        /// <param name="SetValue"></param>
        public void SetString(string SectionName, string KeyName, string SetValue)
        {
            try
            {
                SetIniData(SectionName, KeyName, SetValue, FilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return;
            }
        }

        /// <summary>
        /// 將bool型態的數值寫入資料
        /// </summary>
        /// <param name="SectionName"></param>
        /// <param name="KeyName"></param>
        /// <param name="SetValueInBool"></param>
        public void SetBool(string SectionName, string KeyName, bool SetValueInBool)
        {
            try
            {
                string SetValueInString = SetValueInBool.ToString();
                SetIniData(SectionName, KeyName, SetValueInString, FilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return;
            }
        }


        /// <summary>
        /// 引用windowsAPI/system32內的dll,讀取*.ini內的資料
        /// </summary>
        /// <param name="aSectionName"></param>
        /// <param name="aKeyName"></param>
        /// <param name="aDefault"></param>
        /// <param name="aReturnedString"></param>
        /// <param name="aSize"></param>
        /// <param name="aFileName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        public static extern int GetIniData(string aSectionName, string aKeyName, string aDefault, StringBuilder aReturnedString, int aSize, string aFileName);

        /// <summary>
        /// 引用windowsAPI/system32內的dll,寫入*.ini內的資料
        /// </summary>
        /// <param name="aSectionName"></param>
        /// <param name="aKeyName"></param>
        /// <param name="aString"></param>
        /// <param name="aFileName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        public static extern int SetIniData(string aSectionName, string aKeyName, string aString, string aFileName);

    }
}
