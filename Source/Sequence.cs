

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
        internal double lastAnimTime;

        internal MovementStatus movementStatus;

        ModuleAnimateGeneric _mag;

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
            lastAnimTime = 0f;
            _mag = null;
            movementStatus = MovementStatus.undef;
        }

        internal ModuleAnimateGeneric mag {
            get { return _mag; }
            set { _mag = value; if (_mag != null) lastAnimTime = _mag.animTime; }
        }

        public bool MagMoving    { get { return (_mag.animTime != lastAnimTime); } } 

        public void UpdateAnimTime()
        {
            lastAnimTime = _mag.animTime;
        }

        //
        // Return the normalized animation status
        // 
        public MovementStatus AnimStatus(Sequence.MovementStatus curMovement)
        {
            MovementStatus rc;
            if (MagMoving)
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
