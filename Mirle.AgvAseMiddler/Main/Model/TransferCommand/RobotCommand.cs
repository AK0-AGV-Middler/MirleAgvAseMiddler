﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.AgvAseMiddler.Controller;

namespace Mirle.AgvAseMiddler.Model.TransferSteps
{
    public class RobotCommand : TransferStep
    {
        public string PortAddressId { get; set; } = "";
        public string CassetteId { get; set; } = "";
        public EnumSlotNumber SlotNumber { get; set; } = EnumSlotNumber.A;
        public int RobotNgRetryTimes { get; set; } = 1;
        public EnumPioDirection PioDirection { get; set; } = EnumPioDirection.None;
        public bool IsEqPio { get; set; }
        public ushort ForkSpeed { get; set; } = 100;       

        public RobotCommand(AgvcTransCmd agvcTransCmd) : base(agvcTransCmd.CommandId)
        {
            this.CassetteId = agvcTransCmd.CassetteId;
            this.SlotNumber = agvcTransCmd.SlotNumber;
            this.RobotNgRetryTimes = agvcTransCmd.RobotNgRetryTimes;
        }

    }
}