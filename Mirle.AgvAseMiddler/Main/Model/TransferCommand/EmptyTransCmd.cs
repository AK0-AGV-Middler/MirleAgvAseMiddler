﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
{
    [Serializable]
    public class EmptyTransferStep : TransferStep
    {
        public EmptyTransferStep() : this("") { }
        public EmptyTransferStep(string cmdId) : base(cmdId) => type = EnumTransferStepType.Empty;
    }
}