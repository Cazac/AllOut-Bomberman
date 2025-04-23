# AllOut-Bomberman

- Not moving inside of a collision trigger will dodge the damage
- Walls do not block the propagation spread of explosion blasts / will not stop damage
- Some Scene grabbing code is not ideally performant (Though still only on awakes) as I had some issues with [Serialized]
- No SFX / Audio is used
- Leaving a match midgame can cause issues as I was not sure where the OnPlayerLeave would be so that I could update the logic
- Lobbies can stall if players do not fight, started a deathmatch timer system, but ran out of time :frowning:

Enjoy and have fun!
Let me know if everything is working properly!
