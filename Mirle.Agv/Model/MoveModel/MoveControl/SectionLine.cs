﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    public class SectionLine
    {
        public MapPosition Start { get; set; }
        public MapPosition End { get; set; }
        public double Distance { get; set; }
        public double EncoderStart { get; set; }
        public double EncoderEnd { get; set; }
        public bool DirFlag { get; set; }

        public SectionLine(MapPosition start, MapPosition end, double encoderStart, bool dirFlag)
        {
            Start = start;
            End = end;
            Distance = Math.Sqrt(Math.Pow(Start.X - End.X, 2) + Math.Pow(Start.Y - End.Y, 2));
            DirFlag = dirFlag;
            EncoderStart = encoderStart;
            EncoderEnd = encoderStart + (DirFlag ? Distance : -Distance);
        }
    }
}
