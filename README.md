# Rewindable Scripts

The player control scripts for the Rewindable game. I can't publish the entire game as it relies on 3rd party libraries I can't distribute. 

## Game 
https://willschroeder.itch.io/rewindable 
https://youtu.be/L1nychVh42s

## Outline

The [player core](https://github.com/willschroeder/RewindableScripts/blob/master/Scripts/Player/PlayerCore.cs) checks for collision state, and passes that to abilities, such as [jump](https://github.com/willschroeder/RewindableScripts/blob/master/Scripts/Player/PlayerJump.cs). Each abilitiy modifies the velocity vector, which the core applies each frame. 

Each ability also impliments the [rewindable interface](https://github.com/willschroeder/RewindableScripts/blob/master/Scripts/Rewind/IRewindable.cs) which the [rewinder](https://github.com/willschroeder/RewindableScripts/blob/master/Scripts/Rewind/Rewinder.cs) uses to pull the state back to the frame. 
