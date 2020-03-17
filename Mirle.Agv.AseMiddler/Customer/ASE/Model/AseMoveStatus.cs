using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.AseMiddler.Model
{
    public class AseMoveStatus
    {
        public EnumAseMoveState AseMoveState { get; set; } = EnumAseMoveState.Idle;
        public int HeadDirection { get; set; } = 0;
        public int MovingDirection { get; set; } = 0;       
        public MapSection LastSection { get; set; } = new MapSection();
        public MapAddress LastAddress { get; set; } = new MapAddress();
        public MapPosition LastMapPosition { get; set; } = new MapPosition();
        public MapAddress NeerlyAddress { get; set; } = new MapAddress();
        public int Speed { get; set; } = 0;
        public bool IsMoveEnd { get; set; } = false;
        
        public AseMoveStatus() { }

        public AseMoveStatus(AseMoveStatus aseMoveStatus)
        {
            this.AseMoveState = aseMoveStatus.AseMoveState;           
            this.HeadDirection = aseMoveStatus.HeadDirection;
            this.MovingDirection = aseMoveStatus.MovingDirection;
            this.Speed = aseMoveStatus.Speed;
            this.LastSection = aseMoveStatus.LastSection;
            this.LastAddress = aseMoveStatus.LastAddress;
            this.LastMapPosition = aseMoveStatus.LastMapPosition;
            this.NeerlyAddress = aseMoveStatus.NeerlyAddress;
            this.IsMoveEnd = aseMoveStatus.IsMoveEnd;
        }
    }
}
