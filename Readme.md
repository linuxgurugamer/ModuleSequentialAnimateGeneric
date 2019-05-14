This mod provides the ability to control multiple animations in a single part.  The animations are performed sequentially
For each animation, there is a setting to specify at what point during the current animation's progress to start the next one.

The idea for this mod came from the necessity to control animations in a new part for the Mk2.5 Spaceplane
Parts, the Beluga nose parts.  One part has three animations, the other has two (second and third combined
into one).  The following discussions will be referring the the one with three animations:

Each animation in a part has a ModuleAnimateGeneric; this is the stock mod and is configured normally.
However, the three events:
	openEventGUIName, closeEventGUIName, actionGUIName 
	
will not be used, there will be a set of events provided by this mod.

An interesting quirk is that the animations in some parts have the part initially open, and some initially
closed.  There is a setting to tell the mod this, called closed.  For sanity's sake, the mod works on the
assumption that the part when closed is the beginning of the animation; this option tells the mod to reverse
that.

This mod is called (oddly enough): ModuleSequentialAnimateGeneric, and the config starts like this:

	MODULE
	{
		name = ModuleSequentialAnimateGeneric
		openEventGUIName = #autoLOC_502069 //#autoLOC_502069 = Open
		closeEventGUIName = #autoLOC_502051 //#autoLOC_502051 = Close
		actionGUIName = Toggle Doors

Each animation has a corresponding stanza called a SEQUENCE  The following is one sequence from the complete
module, with each line fully annotated:

		SEQUENCE
		{
			seqNum = 1						// Used for error reporting, best if it was sequential
			animName = BelugaDoorsal		// Name of the animation

			// the mod thinks of 0 as closed, and 1 as open
			// Since many part seem to have that reversed, the
			// following tells the mod what the animation value is
			// when closed
			
			closed = 1					

			// The following specifies the progress value for the animation
			// to be for the child's animatinon to start, 
			// When the progress has reached this level or higher, 
			// start the child animation.  This value goes from 0 to 1,
			// any value outside that will be clamped to the nearest
			//
			allowChildAnimAt = 0.55

			// Since this is the beginning of the sequences, the following is unneeded
			// and commented out.
			// The following specifies the progress value for the animation
			// to go below, at which time the parent animation will be started.
			// Value range is the same as the "allowChildAnimAt"
			// 
			//allowParentAnimAt = 0.2

			// The minDeployLimit specifies the minimum percentage that the animation can be adjusted to.
			// If the player attempts to move it below that, a message will be displayed and the value reset.
			// Also, for this value, you can use either a whole number between 0 and 100, or a normalized
			// number of between 0 and 1.  The mod assumes that a value of 1 is == 100%
			// The following two lines are identical
			minDeployLimit = 50 
			minDeployLimit = 0.50 
		}



