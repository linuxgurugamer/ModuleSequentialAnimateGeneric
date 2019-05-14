	
	    internal int seqNum;		// reference only
        internal string animName;	// Name of animation
        internal string parent;		// name of parent animation
        internal double allowChildAnimAt;	// Min % for this to open before a child will start to open
        internal double maxToAllowChildAnim;	// Max % for this to open before a child will not move


	MODULE
	{
		name = ModuleSequentialAnimateGeneric

		SEQUENCE
		{
			seqNum = 1
			animName = BelugaRamp
			allowChildAnimAt = 45
			maxToAllowChildAnim = 95
		}
		SEQUENCE
		{
			seqNum = 2
			animName = RampExtend
			parent = BelugaRamp
		}

	}
