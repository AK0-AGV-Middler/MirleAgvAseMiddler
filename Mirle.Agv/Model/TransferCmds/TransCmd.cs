﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
    public abstract class TransCmd
    {
        protected MapInfo theMapInfo ;
        protected EnumTransCmdType type;
        public string CmdId { get; set; } = "Empty";
        public string CstId { get; set; } = "Empty";

        public EnumTransCmdType GetCommandType()
        {
            return type;
        }

        public TransCmd Clone()
        {
            return ExtensionMethods.DeepClone(this);           
        }

        public TransCmd(MapInfo theMapInfo)
        {
            this.theMapInfo = theMapInfo;
        }
    }
}
