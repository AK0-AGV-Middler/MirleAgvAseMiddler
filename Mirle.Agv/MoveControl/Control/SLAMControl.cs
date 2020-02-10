using Mirle.Agv.Controller.Tools;
using Mirle.Agv.Model;
using Mirle.Agv.Model.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mirle.Agv.Controller
{
    public class SLAMControl
    {
        private Dictionary<EnumSLAMType, SLAM> allSLAM = new Dictionary<EnumSLAMType, SLAM>();
        public List<EnumSLAMType> AllSLAMList = new List<EnumSLAMType>();
        private SLAM usingSLAM = null;
        private string device = "MoveControl";
        private Mirle.Tools.MirleLogger mirleLogger = Mirle.Tools.MirleLogger.Instance;
        private SLAMConfig slamConfig;

        private void WriteLog(string category, string logLevel, string device, string carrierId, string message,
                             [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string classMethodName = String.Concat(GetType().Name, ":", memberName);
            mirleLogger.Log(new Mirle.Tools.LogFormat(category, logLevel, classMethodName, device, carrierId, message));
        }

        private void ReadSLAMConfigXML(string path)
        {
            if (path == null || path == "")
            {
                WriteLog("MoveControl", "5", device, "", "path == null or path == \"\".");
                return;
            }

            XmlDocument doc = new XmlDocument();

            if (!File.Exists(path))
            {
                WriteLog("MoveControl", "5", device, "", "找不到SLAMConfig.xml.");
                return;
            }

            slamConfig = new SLAMConfig();

            doc.Load(path);
            var rootNode = doc.DocumentElement;

            foreach (XmlNode item in rootNode.ChildNodes)
            {
                switch (item.Name)
                {
                    case "UsingSLAMType":
                        if (item.InnerText != "")
                            slamConfig.UsingSLAMType = (EnumSLAMType)Enum.Parse(typeof(EnumSLAMType), item.InnerText);
                        break;
                    case "SLAM_Nav350ConfigPath":
                        slamConfig.SLAM_Nav350ConfigPath = item.InnerText;
                        break;
                    case "SLAM_Sick":
                        slamConfig.SLAM_SickConfigPath = item.InnerText;
                        break;
                    case "SLAM_R2S":
                        slamConfig.SLAM_R2SConfigPath = item.InnerText;
                        break;
                    default:
                        break;
                }
            }
        }

        public bool InitailSLAM(string configPath)
        {
            //ReadSLAMConfigXML(configPath);
            return true;
        }

        public void CloseSLAM()
        {
            foreach (EnumSLAMType type in AllSLAMList)
            {
                allSLAM[type].CloseSLAM();
            }
        }

        public bool ChangeUsingSLAM(EnumSLAMType type)
        {
            if (allSLAM.ContainsKey(type))
            {
                usingSLAM = allSLAM[type];
                return true;
            }
            else
                return false;
        }

        public bool GetAGVPosition(ref AGVPosition agvPosition)
        {
            if (usingSLAM == null)
                return false;

            agvPosition = usingSLAM.GetAGVPosition();

            if (agvPosition == null)
                return false;

            return true;
        }
    }
}
