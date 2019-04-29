using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Control;

namespace Mirle.Agv.Model.TransferCmds
{
    public abstract class TransCmd
    {
        protected EnumTransCmdType type;
        public string CmdId { get; set; }

        public TransCmd()
        {
        }

        public EnumTransCmdType GetType()
        {
            return type;
        }

        public TransCmd Clone()
        {
            switch (type)
            {
                case EnumTransCmdType.Move:
                    MoveCmdInfo moveCmd = (MoveCmdInfo)this;
                    return moveCmd;
                case EnumTransCmdType.Load:
                    LoadCmdInfo loadCmdInfo = (LoadCmdInfo)this;
                    return loadCmdInfo;
                case EnumTransCmdType.Unload:
                    UnloadCmdInfo unloadCmdInfo = (UnloadCmdInfo)this;
                    return unloadCmdInfo;
                default:
                    EmptyTransCmd emptyTransCmd = (EmptyTransCmd)this;
                    return emptyTransCmd;
            }            
        }
    }
}
