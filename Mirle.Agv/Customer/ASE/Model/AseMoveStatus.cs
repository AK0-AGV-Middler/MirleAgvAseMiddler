using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.AgvAseMiddler.Model
{
    public class AseMoveStatus
    {
        public EnumAseMoveState AseMoveState { get; set; } = EnumAseMoveState.Idle;
        public int HeadDirection { get; set; } = 0;
        public bool IsMoving { get; set; } = false;
        public MapPosition CurMapPosition { get; set; } = new MapPosition();
        public int Speed { get; set; } = 0;
        
        public AseMoveStatus() { }

        public AseMoveStatus(AseMoveStatus aseMoveStatus)
        {
            this.AseMoveState = aseMoveStatus.AseMoveState;
            this.CurMapPosition = aseMoveStatus.CurMapPosition;
            this.HeadDirection = aseMoveStatus.HeadDirection;
            this.IsMoving = aseMoveStatus.IsMoving;
            this.Speed = aseMoveStatus.Speed;
        }
    }
}
