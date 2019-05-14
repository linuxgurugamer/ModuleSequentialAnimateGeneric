

namespace ModuleSequentialAnimateGeneric
{
    public class Sequence
    {
        public enum MovementStatus { undef, closing, closed, opening, open };

        internal int seqNum;
        internal Sequence parentIdx;
        internal Sequence childIdx;
        internal string animName;
        internal string parent;
        internal float closed;
        internal float allowChildAnimAt;
        internal float allowParentAnimAt;
        internal float minDeployLimit;
        internal ModuleAnimateGeneric mag;
        internal ModuleAnimateGeneric.animationStates animState = ModuleAnimateGeneric.animationStates.LOCKED;
        internal MovementStatus movementStatus;

        public Sequence()
        {
            seqNum = 0;
            parentIdx = null;
            childIdx = null;
            animName = "";
            parent = "";
            closed = 0;
            allowChildAnimAt = 0;
            allowParentAnimAt = 1;
            minDeployLimit = 0;
            mag = null;
            movementStatus = MovementStatus.undef;
        }

        //
        // Return the normalized animation status
        // 
        public MovementStatus AnimStatus(Sequence.MovementStatus curMovement)
        {
            MovementStatus rc;
            if (mag.animTime > 0 && mag.animTime < 1)
            {
                rc = curMovement;
            }
            else
            {
                rc = ((mag.animTime == closed) ? Sequence.MovementStatus.closed : Sequence.MovementStatus.open);
            }
            return rc;
        }
    }

}
