using System;
using System.Linq;


namespace ModuleSequentialAnimateGeneric
{
    public partial class ModuleSequentialAnimateGeneric : PartModule
    {
        override public void OnLoad(ConfigNode node)
        {
            Log.Info("OnLoad, Part: " + part.name);
            base.OnLoad(node);

            if (!node.HasNode("SEQUENCE"))
            {
                Log.Error("Node does not have SEQUENCE");
                return;
            }
            var nodes = node.GetNodes();
            for (int i = 0; i < nodes.Count(); i++)
            {
                ConfigNode n = node.nodes[i];

                if (n.name == "SEQUENCE")
                {
                    Sequence seq = new Sequence();

                    try { seq.seqNum = Convert.ToUInt16(n.GetValue("seqNum")); } catch (Exception) { seq.seqNum = -1; }
                    try { seq.parent = n.GetValue("parent"); } catch { seq.parent = ""; }
                    try { seq.closed = (float)Convert.ToDouble(n.GetValue("closed")); } catch (Exception) { seq.closed = 0; }

                    try { seq.animName = n.GetValue("animName"); } catch { seq.animName = ""; }

                    try { seq.allowChildAnimAt = (float)Convert.ToDouble(n.GetValue("allowChildAnimAt")); } catch (Exception) { seq.allowChildAnimAt = 0; }
                    try { seq.allowParentAnimAt = (float)Convert.ToDouble(n.GetValue("allowParentAnimAt")); } catch (Exception) { seq.allowParentAnimAt = 0; }
                    seq.allowChildAnimAt = Math.Min(1f, Math.Max(0, seq.allowChildAnimAt));
                    seq.allowParentAnimAt = Math.Min(1f, Math.Max(0, seq.allowParentAnimAt));

                    try { seq.minDeployLimit = (float)Convert.ToDouble(n.GetValue("minDeployLimit")); } catch (Exception) { seq.minDeployLimit = 0; }
                    if (seq.minDeployLimit < 1)
                        seq.minDeployLimit *= 100f;
                    seq.minDeployLimit = Math.Min(100f, Math.Max(0, seq.minDeployLimit));

                    seq.closed = Math.Min(1f, Math.Max(0, seq.closed));

                    seq.mag = GetAnimation(seq.animName);
                    if (seq.mag == null)
                        Log.Error("No animation found at seqNum: " + seq.seqNum);
                    else
                    {
                        UpdateAnimMovementStatus(seq);


                        if (movementStatus != Sequence.MovementStatus.undef && movementStatus != seq.movementStatus)
                        {
                            Log.Error("animations have different movement settings");
                            Log.Error("Part: " + part.name + ", movementStatus in OnLoad: " + movementStatus);
                        }
                        movementStatus = seq.movementStatus;
                    }
                    sequenceAnimations.Add(seq);
                }
            }

            SetUpParentChildValues();
            if (HighLogic.LoadedScene == GameScenes.LOADING)
            {
                if (!seqDict.ContainsKey(part.name))
                {
                    seqDict.Add(part.name, sequenceAnimations);
                }
                else
                    Log.Error("OnLoad: seqDict already contains: " + part.name);
            }
#if DEBUG
            DumpConfig();
#endif
        }

        void UpdateAnimMovementStatus(Sequence seq)
        {
            seq.movementStatus = seq.AnimStatus(movementStatus);
            
            if (movementStatus == Sequence.MovementStatus.undef)
                movementStatus = seq.movementStatus;
        }

        void SetUpParentChildValues()
        {
            Log.Info("SetUpParentChildValues");
            // Now set up the parent/child values and the Animation values
            foreach (var s in sequenceAnimations)
            {
                foreach (var s1 in sequenceAnimations)
                {
                    if (s1 != s)
                    {
                        if (s.parent == s1.animName)
                        {
                            s.parentIdx = s1;
                            s1.childIdx = s;
                            break;
                        }
                    }
                }
            }
            // Find the first and last animation
            foreach (var s in sequenceAnimations)
            {
                if (s.parentIdx == null) //s.parent == null || s.parent == "")
                    firstSeq = s;

                if (s.childIdx == null)
                    lastSeq = s;
            }
            if (firstSeq == null)
                Log.Error("Part: " + part.name + ", firstSeq is null");
            if (lastSeq == null)
                Log.Error("Part: " + part.name + ", lastSeq is null");            
        }

        override public void OnSave(ConfigNode node)
        {
            // At this point, part.name looks like:
            //      mk25BeulgaNoseBuildV (Untitled Space Craft)
            // All we need is the first part for the partname
            //
            
            string partname = part.name.Split(new char[] { ' ' }, 2)[0];

            base.OnSave(node);
            try
            {
                sequenceAnimations = seqDict[part.name];
                foreach (var seq in sequenceAnimations)
                {
                    ConfigNode n = new ConfigNode();
                    n.AddValue("seqNum", seq.seqNum);
                    if (seq.parent != null) n.AddValue("parent", seq.parent);
                    if (seq.animName != null) n.AddValue("animName", seq.animName);
                    n.AddValue("closed", seq.closed);
                    n.AddValue("allowChildAnimAt", seq.allowChildAnimAt.ToString("N3"));
                    n.AddValue("allowParentAnimAt", seq.allowParentAnimAt.ToString("N3"));
                    n.AddValue("minDeployLimit", seq.minDeployLimit.ToString("N3"));
                    
                    node.AddNode("SEQUENCE", n);
                }
            }
            catch (Exception ex)
            {
                Log.Error("OnSave Exception: " + ex.Message);
            }
        }
    }
}
