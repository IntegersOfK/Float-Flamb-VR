MixCast VR plugin for Unity - v1.0.0
(c) Blueprint Reality Inc., 2017. All rights reserved

Quickstart:

1) Attach the "MixCastCameras" prefab to your CameraRig GameObject
2) Add the "MixCast UI" prefab to your UI GameObject(s)


Requirements:

- Unity 5.5 or above
- SteamVR plugin by Valve Corporation v1.2.0 or above

The MixCast VR Studio application must be installed and run prior to enabling MixCast VR output in your application at runtime.



Extras:
MixCast also comes with some extra prefabs and scripts to aid with mixed reality development and production.

Slate UI prefab: If dropped in a scene, provides a film slate style display that can be called up via keypress to aid in the capture of multiple takes in a row. Inspect the attached script for more details.

RotateRoomControls script: If attached to your Room Transform ([CameraRig] object in SteamVR by default), allows users to adjust room Y-rotation for the purposes of more control over camera shots. May interfere with other teleportation systems.

