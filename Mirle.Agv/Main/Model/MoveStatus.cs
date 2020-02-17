using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class MoveStatus
    {
        public EnumMoveState MoveState { get; set; } = EnumMoveState.Idle;
        public int HeadDirection { get; set; } = 0;
    }
}
