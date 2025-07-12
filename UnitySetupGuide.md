# Unity 6 Setup Guide for Elite Shooter

## Initial Setup (5 minutes)

### 1. Create Basic Scene Structure

**Player Setup:**
1. In Hierarchy, right-click → Create Empty → Name it "Player"
2. Right-click on Player → 3D Object → Capsule (this is your body)
3. Right-click on Player → Camera (this is your view)
4. Select Player object → Inspector → Add Component → Search "Player Controller"

**Game Manager:**
1. Create Empty → Name it "GameManager"
2. Add Component → "Game Manager"

**UI Setup:**
1. Right-click in Hierarchy → UI → Canvas
2. Right-click on Canvas → UI → Text (for score display)
3. Right-click on Canvas → UI → Slider (for health bar)

### 2. Configure Player

**Player Controller Settings:**
- Walk Speed: 5
- Sprint Speed: 8
- Jump Height: 2
- Mouse Sensitivity: 2
- Ground Mask: Default (Layer 0)

**Camera Setup:**
- Position: (0, 1.6, 0) - eye level
- Rotation: (0, 0, 0)
- Field of View: 60

### 3. Add Weapon System

**Weapon Setup:**
1. Create Empty → Name it "WeaponHolder"
2. Make it child of Camera
3. Position: (0.5, -0.3, 0.5) - weapon position
4. Add Component → "Weapon Controller"

### 4. Create Ground

1. Create Empty → Name it "Environment"
2. Right-click → 3D Object → Plane (this is your ground)
3. Scale: (10, 1, 10) for a large area
4. Add a material (right-click in Project → Create → Material)

### 5. Add Lighting

1. In Hierarchy, you should see "Directional Light"
2. Position: (0, 10, 0)
3. Rotation: (45, 45, 0)
4. Intensity: 1

## Quick Test (2 minutes)

1. **Press Play button** (top center)
2. **Controls:**
   - WASD: Move
   - Mouse: Look around
   - Space: Jump
   - Left Click: Shoot (when weapon is set up)

## Common Issues & Solutions

**"Script has errors"**
- Check Console window (Window → General → Console)
- Make sure all script files are in Assets/Scripts folder

**"Can't move"**
- Make sure Player has Character Controller component
- Check if Player Controller script is attached

**"No camera view"**
- Make sure Camera is child of Player
- Check Camera position and rotation

**"Scripts not showing in Add Component"**
- Wait for Unity to compile scripts
- Check Console for compilation errors

## Next Steps

1. **Add enemies** using EnemySpawner script
2. **Create weapon data** using WeaponData scriptable objects
3. **Add UI elements** using GameUI script
4. **Set up audio** for footsteps, shooting, etc.

## Unity 6 Specific Notes

- HDRP is enabled by default
- Use Universal Render Pipeline (URP) if HDRP is too demanding
- New Input System is recommended over old Input Manager
- Use TextMeshPro for all text elements

## Performance Tips

- Keep objects in the scene under 1000 for good performance
- Use LOD (Level of Detail) for complex models
- Enable occlusion culling for large scenes
- Use object pooling for bullets and effects

## Build Your Game

1. File → Build Settings
2. Choose platform (PC, Mac, Linux)
3. Click "Build"
4. Choose output folder
5. Your game is ready to play!

---

**Need Help?**
- Check Unity Console for errors
- Use Unity's built-in documentation (Help → Scripting Reference)
- Search Unity Forums for specific issues 