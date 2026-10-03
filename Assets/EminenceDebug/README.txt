EMINENCE ANDROID DEBUG OVERLAY — UNITY SOURCE

Purpose
An in-game development visualization for your own Unity project. It does not draw over, inject into, or read memory from another app/game.

Files
- DebugTarget.cs: lightweight registration component for test objects.
- AndroidDebugOverlay.cs: touch menu, responsive virtual layout, and visualization.

Setup
1. Copy both scripts into Assets/Scripts in your Unity project.
2. Add DebugTarget to your own test characters. Assign targetCollider to the collider used by your game.
3. Set targetName and health from your own gameplay systems.
4. Add AndroidDebugOverlay to a scene object and assign the gameplay camera.
5. Make an Android Development Build to test. The overlay is disabled in non-development player builds.
6. Build the APK using Unity's Android Build Support.

Accuracy and performance
- Targets are maintained in a registry rather than searching the entire scene every GUI frame.
- Boxes are projected from the assigned collider's world-space bounds. These are axis-aligned bounds, so rotated or irregular colliders may still look approximate.
- Health is read from DebugTarget.health; connect it to your game's actual health value.
- Set drawEveryNFrames above 1 to reduce visualization work if needed, at the cost of less frequent updates.
- This is starter code and must be tested in your specific Unity version, scene, camera setup, orientation, and device. No code can guarantee zero errors or zero performance impact in every project.
- For production-quality touch UI, consider replacing IMGUI with a Unity Canvas, Canvas Scaler, and Safe Area layout.

Troubleshooting
- If the panel is absent, ensure the app is a Development Build and this component is active.
- If no targets appear, confirm the targets are active and have DebugTarget attached.
- If boxes are misplaced, assign the correct gameplay camera and collider.
