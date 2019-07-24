﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
    public class LoadCmdInfo : TransferStep
    {
        public string LoadAddress { get; set; } = "Empty";
        public int StageNum { get; set; }

        public LoadCmdInfo():this(new MapInfo()) { }
        public LoadCmdInfo(MapInfo theMapInfo) : base(theMapInfo)
        {
            type = EnumTransCmdType.Load;
        }
    }
}
