﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
    public class UnloadCmdInfo : TransferStep
    {
        public string UnloadAddress { get; set; } = "Empty";
        public int StageNum { get; set; }

        public UnloadCmdInfo():this(new MapInfo()) { }
        public UnloadCmdInfo(MapInfo theMapInfo) : base(theMapInfo)
        {
            type = EnumTransCmdType.Unload;
        }
    }
}
