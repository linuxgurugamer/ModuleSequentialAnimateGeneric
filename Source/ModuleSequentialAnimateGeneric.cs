using System;
using System.Collections.Generic;
using UnityEngine;


namespace ModuleSequentialAnimateGeneric
{
    public partial class ModuleSequentialAnimateGeneric : PartModule
    {
        internal static Dictionary<string, List<Sequence>> seqDict = null;
        Sequence.MovementStatus movementStatus = Sequence.MovementStatus.undef;
        List<Sequence> sequenceAnimations = null;

        Sequence firstSeq = null;
        Sequence lastSeq = null;

        #region KSPFields
        [KSPField]
        public string openEventGUIName = "#autoLOC_6001354";

        [KSPField]
        public string closeEventGUIName = "#autoLOC_6001354";

        [KSPField]
        public string actionGUIName = "#autoLOC_6001354";
        #endregion

        bool adjustLimits = false;
        void SetAdjustLimits(bool b = true)
        {
            if (b)
            {
                adjustLimits = !adjustLimits;
            }
            if (!Events["AdjustLimitsEvent"].active)
                adjustLimits = false;
            Events["AdjustLimitsEvent"].guiName = adjustLimits ? "Stop adjustment" : "Adjust max deployment limits";
            UpdateAdjustment();
        }
        #region KSPEvent
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Adjust max deployment limits")]
        public void AdjustLimitsEvent()
        {
            Log.Info("AdjustLimitsEvent");
            SetAdjustLimits();


        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "openEventGUIName")]
        public void OpenEvent()
        {
            Log.Info("OpenEvent");
            firstSeq.mag.Toggle();

            lastSeqnumEnabled = firstSeq.seqNum;
            SetMovementStatus(firstSeq, Sequence.MovementStatus.opening);

            SetEventStatus();
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "closeEventGUIName")]
        public void CloseEvent()
        {
            Log.Info("CloseEvent");

            lastSeq.mag.Toggle();
            lastSeqnumEnabled = lastSeq.seqNum;
            SetMovementStatus(lastSeq, Sequence.MovementStatus.closing);

            SetEventStatus();
        }

#if DEBUG
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Dump MSAG info")]
        public void DebugEvent()
        {
            Log.Info("DebugEvent");
            DumpConfig();
        }
#endif

        #endregion

        void SetMovementStatus(Sequence seq, Sequence.MovementStatus status)
        {
            movementStatus = status;
            while (seq != null)
            {
                seq.movementStatus = status;
                if (status == Sequence.MovementStatus.opening)
                    seq = seq.childIdx;
                else
                    seq = seq.parentIdx;
            }
        }

        #region Action
        [KSPAction("Action", KSPActionGroup.None)]
        public void ToggleAction(KSPActionParam param)
        {
            this.Action();
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "actionGUIName")]
        public void Action()
        {
            Log.Info("Action");

            switch (movementStatus)
            {
                case Sequence.MovementStatus.undef:
                    Log.Error("Action, movementStatus is undefined");
                    break;
                case Sequence.MovementStatus.closed:
                    firstSeq.mag.Toggle();
                    lastSeqnumEnabled = firstSeq.seqNum;
                    SetMovementStatus(firstSeq, Sequence.MovementStatus.opening);
                    break;

                case Sequence.MovementStatus.open:
                    lastSeq.mag.Toggle();
                    lastSeqnumEnabled = lastSeq.seqNum;
                    SetMovementStatus(lastSeq, Sequence.MovementStatus.closing);
                    break;
            }
            SetEventStatus();
        }
        #endregion

        void SetEventStatus()
        {
            // Log.Info("SetEventStatus, movementStatus: " + movementStatus);
            switch (movementStatus)
            {
                case Sequence.MovementStatus.undef:
                    Log.Error("SetMovementStatus, part: " + part.name + ", movementStatus is undefined");
                    break;
                case Sequence.MovementStatus.closed:
                    Events["OpenEvent"].active = true;
                    Events["CloseEvent"].active = false;
                    break;
                case Sequence.MovementStatus.closing:
                    Events["OpenEvent"].active = false;
                    Events["CloseEvent"].active = false;
                    break;
                case Sequence.MovementStatus.open:
                    Events["OpenEvent"].active = false;
                    Events["CloseEvent"].active = true;
                    break;
                case Sequence.MovementStatus.opening:
                    Events["OpenEvent"].active = false;
                    Events["CloseEvent"].active = false;
                    break;
            }
            Events["AdjustLimitsEvent"].active = Events["CloseEvent"].active;
            adjustLimits = false;
            SetAdjustLimits(false);
        }


        public new void Awake()
        {
            base.Awake();
            Log.Info("Awake");
            if (sequenceAnimations == null)
                sequenceAnimations = new List<Sequence>();
            if (seqDict == null)
            {
                seqDict = new Dictionary<string, List<Sequence>>();
            }
        }


        public void Start()
        {
            Log.Info("Start, part: " + part.name);
            {
                if (seqDict.ContainsKey(part.name))
                {
                    sequenceAnimations = seqDict[part.name];
                    foreach (var sa in sequenceAnimations)
                        UpdateAnimMovementStatus(sa);
                    SetUpParentChildValues();
#if DEBUG
                    DumpConfig();
#endif
                }
                else
                {
                    Log.Error("seqDict does NOT contain key: " + part.name);
                    foreach (var s in seqDict.Keys)
                        Log.Error("seqDict.key: " + s);
                }
            }


            Events["OpenEvent"].guiName = openEventGUIName;
            Events["CloseEvent"].guiName = closeEventGUIName;
            Events["Action"].guiName = actionGUIName;
            Events["OpenEvent"].active = true;
            Events["CloseEvent"].active = true;
            Events["Action"].active = false;

            Events["CloseEvent"].active = false;
            Events["AdjustLimitsEvent"].active = false;
            Sequence cur = firstSeq;
            while (cur != null)
            {
                {
                    cur.mag = GetAnimation(cur.animName);
                    if (cur.mag == null)
                        Log.Error("No animation found at seqNum: " + cur.seqNum);
                    else
                    {
                        UpdateAnimMovementStatus(cur);

                        if (movementStatus != Sequence.MovementStatus.undef && movementStatus != cur.movementStatus)
                            Log.Error("animations have different movement settings");
                        movementStatus = cur.movementStatus;

                    }
                }
                cur.mag.enabled = true;
                cur.mag.allowManualControl = true;

                cur.mag.Events["Toggle"].guiActive = false;
                cur.mag.Events["ToggleStaging"].guiActive = false;
                cur.mag.Events["Toggle"].guiActiveEditor = false;
                cur.mag.Events["ToggleStaging"].guiActiveEditor = false;

                cur.mag.Actions["ToggleAction"].active = false;

                cur.mag.Fields["status"].guiActive = false;

                cur = cur.childIdx;
            }

            SetEventStatus();
        }


#if DEBUG
        internal void DumpConfig()
        {
            if (this.part != null)
            {
                if (this.part.name != null)
                    Log.Info("DumpConfig, part: " + this.part.name);
                if (this.part.partInfo != null)
                    Log.Info("DumpConfig, part.partInfo.title: " + this.part.partInfo.title);
            }
            Log.Info("DumpConfig, movementStatus: " + movementStatus);
            Sequence cur = firstSeq;
            while (cur != null)
            {
                var m = cur.AnimStatus(movementStatus);

                Log.Info("seqNum: " + cur.seqNum + ", animName: " + cur.animName + ", parent: " + cur.parent + ", allowChildAnimAt: " + cur.allowChildAnimAt +
                    ", movementStatus: " + cur.movementStatus + ", anim status based on animTime: " + m);
                Log.Info("cur.lastAnimTime: " + cur.lastAnimTime + ", mag.animTime: " + cur.mag.animTime);


                cur = cur.childIdx;
            }
        }
#endif
        ModuleAnimateGeneric GetAnimation(string s)
        {
            foreach (var m in this.part.FindModulesImplementing<ModuleAnimateGeneric>())
            {
                if (m.animationName == s)
                {
                    return m;
                }
            }
            return null;
        }

        public void UpdateAdjustment()
        {
            Sequence cur = firstSeq;
            while (cur != null)
            {
                cur.mag.Fields["deployPercent"].guiActive = adjustLimits;
                cur.mag.Fields["deployPercent"].guiActiveEditor = adjustLimits;

                cur = cur.childIdx;
            }
        }


        float progress;
        int lastSeqnumEnabled;

        double lastMessageTime = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        public void FixedUpdate()
        {
            bool anyMoving = false;
            Sequence cur = null;

            if (adjustLimits)
            {
                cur = firstSeq;
                while (cur != null)
                {
                    if (cur.mag.deployPercent < cur.minDeployLimit)
                    {
                        // This logic is to not spam the screen with messages
                        if ((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds - lastMessageTime > .75f)
                        {
                            ScreenMessages.PostScreenMessage("Minimum limit exceeded", 0.5f, ScreenMessageStyle.UPPER_CENTER);
                            lastMessageTime = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        }
                        cur.mag.deployPercent = Mathf.Max(cur.mag.deployPercent, cur.minDeployLimit);
                    }


                    cur = cur.childIdx;
                }
            }

#if DEBUG
            Sequence prev = null;
#endif
            cur = null;

            bool enableNext = false;

            switch (movementStatus)
            {
                case Sequence.MovementStatus.undef:
                    Log.Error("FixedUpdate, Part: " + part.name + ", movementStatus is undefined");
                    break;
                case Sequence.MovementStatus.closed: return;
                case Sequence.MovementStatus.closing: cur = lastSeq; break;
                case Sequence.MovementStatus.open: return;
                case Sequence.MovementStatus.opening: cur = firstSeq; break;
            }

            while (cur != null)
            {

                if (enableNext)
                {
                    if ((movementStatus == Sequence.MovementStatus.opening && lastSeqnumEnabled < cur.seqNum) ||
                        movementStatus == Sequence.MovementStatus.closing && lastSeqnumEnabled > cur.seqNum)
                    {
                        Log.Info("movementStatus: " + movementStatus + ", cur.seqNum: " + cur.seqNum + ", lastSeqNumEnabled: " + lastSeqnumEnabled);
                        if (cur.mag.aniState != ModuleAnimateGeneric.animationStates.MOVING &&
                        ((movementStatus == Sequence.MovementStatus.opening && cur.AnimStatus(movementStatus) != Sequence.MovementStatus.open) ||
                        (movementStatus == Sequence.MovementStatus.closing && cur.AnimStatus(movementStatus) != Sequence.MovementStatus.closed))
                        )
                        {
                            Log.Info("enableNext, cur.seqNum: " + cur.seqNum + ", movementStatus: " + movementStatus + ", cur.AnimStatus(movementStatus): " + cur.AnimStatus(movementStatus) + "," +

                                "mag.animTime: " + cur.mag.animTime + ", mag.deployPercent: " + cur.mag.deployPercent
                                );
                            cur.AnimStatus(movementStatus);
                            cur.mag.Toggle();
                            lastSeqnumEnabled = cur.seqNum;

                            cur.animState = ModuleAnimateGeneric.animationStates.MOVING;
                            cur.mag.aniState = ModuleAnimateGeneric.animationStates.MOVING;
                        }
                    }
                    enableNext = false;
                }

                progress = (cur.closed == 1) ? cur.closed - cur.mag.Progress : cur.mag.Progress;
                switch (movementStatus)
                {

                    case Sequence.MovementStatus.opening:
                        {
                            if (cur.mag.aniState == ModuleAnimateGeneric.animationStates.MOVING)
                            {
                                cur.animState = cur.mag.aniState;
                                if ((cur.allowChildAnimAt <= progress || progress >= cur.mag.deployPercent / 100f) && cur.childIdx != null)
                                {
                                    Log.Info("1-enableNext = true, cur.seqNum:" + cur.seqNum + ", progress: " + progress +
                                        ", cur.mag.deployPercent: " + cur.mag.deployPercent);
                                    enableNext = true;
                                }

                                anyMoving = true;
                            }
                            else
                            {
                                if (cur.AnimStatus(Sequence.MovementStatus.opening) == Sequence.MovementStatus.open)
                                    cur.movementStatus = Sequence.MovementStatus.open;
                            }

#if DEBUG
                            prev = cur;
#endif
                            cur.UpdateAnimTime();
                            cur = cur.childIdx;

                        }
                        break;
                    case Sequence.MovementStatus.closing:
                        {
                            if (cur.mag.aniState == ModuleAnimateGeneric.animationStates.MOVING)
                            {

                                cur.animState = cur.mag.aniState;
                                if (cur.allowParentAnimAt >= progress && cur.parentIdx != null)
                                {
                                    Log.Info("3-enableNext = true, movementStatus: " + movementStatus + ", cur.allowParentAnimAt: " + cur.allowParentAnimAt + ", progress: " + progress);

                                    enableNext = true;
                                }

                                anyMoving = true;
                            }
                            else
                            {
                                if (cur.AnimStatus(Sequence.MovementStatus.closing) == Sequence.MovementStatus.closed)
                                    cur.movementStatus = Sequence.MovementStatus.closed;
                            }

#if DEBUG
                            prev = cur;
#endif
                            cur.UpdateAnimTime();
                            cur = cur.parentIdx;

                        }
                        break;
                }
            }

            if (!anyMoving)
            {
                switch (movementStatus)
                {
                    case Sequence.MovementStatus.undef:
                        Log.Error("FixedUpdate 2, Part: " + part.name + ", movementStatus is undefined");
                        break;
                    case Sequence.MovementStatus.closing:
                        cur = lastSeq;

                        while (cur != null)
                        {
                            if (cur.movementStatus != Sequence.MovementStatus.closed)
                            {
                                anyMoving = true;
                            }

                            cur = cur.parentIdx;
                        }
                        break;

                    case Sequence.MovementStatus.opening:
                        cur = firstSeq;
                        while (cur != null)
                        {
                            if (cur.movementStatus != Sequence.MovementStatus.open)
                                anyMoving = true;

                            cur = cur.childIdx;
                        }
                        break;
                }
                if (!anyMoving)
                {
                    switch (movementStatus)
                    {
                        case Sequence.MovementStatus.undef:
                            Log.Error("FixedUpdate 3, Part: " + part.name + ", movementStatus is undefined");
                            break;
                        case Sequence.MovementStatus.closing:
                            movementStatus = Sequence.MovementStatus.closed;
                            break;
                        case Sequence.MovementStatus.opening:
                            movementStatus = Sequence.MovementStatus.open;
                            break;
                    }
                }
                SetEventStatus();
            }
        }

    }
}
