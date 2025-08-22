Unity Version
- Unity 6.1.4f1

🎮 Controls (Prototype)
- Move: WASD / Arrow Keys
- Jump: Space

🛠️ Authoring & Extending Enemies
Create Config & Locomotion assets
- Config: Create → Enemies → Configs
- Locomotion: Create → Enemies → Locomotion

In the Scene there is EnemiesHandler Object which has the EnemyHandler Component in order to add or remove enemies from the scene make sure to update Enemy Handler so that Enemies are initialized properly.
Assets/_Configs -> contain the currently created asset files for the enemies
Assets/_Prefabs/Enemies -> Contains the 3 Enemies

Notes :
Enemies currently are using Navmesh for the scope of this prototype.
Stomping is fairly simple and does not include advanced system for better precision
Player Controller is fairly basic only for debugging purpose

----------------------------------------

Focus of this prototype was to implement an extendable Enemies System, for that Component based system is used with loose coupling.
Scriptable Objects are used to make it Designer Friendly
