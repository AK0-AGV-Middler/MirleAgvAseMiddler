using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class TransCmdInfo
    {
        private string cmdId;
        private string loadAddress;
        private string toAddress;
        private string cassetteId;
        private List<string> guideSectionsToLoad;
        private List<string> guideSectionsToEnd;
        private List<string> guideAddressesToLoad;
        private List<string> guideAddressesToEnd;
        private bool isLoad;
        private bool isUnload;
    }

    public class MoveCmdInfo
    {
        private string cmdId;
        private string moveEndAddress;//LoadAddress or UnloadAddress
        private List<MapSection> guideSections;//LoadSections or UnloadSections
        private MapSection section;
        private bool isPrecisePositioning;

        public string GetSectionId()
        {
            return section.sectionId;
        }
    }

    public class LoadCmdInfo
    {
        private string cmdId;
        private string loadAddress;
        private string cassetteId;
        private int stageNum;
    }

    public class UnloadCmdInfo
    {
        private string cmdId;
        private string toAddress;
        private string cassetteId;
        private int stageNum;
    }
}
