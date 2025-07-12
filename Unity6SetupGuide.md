# Unity 6 Setup Guide - Fixed for Compilation Errors

## 🚨 Problem Solved!
The compilation errors were caused by missing packages. I've created simplified scripts that work with Unity 6's default packages.

## Quick Fix (2 minutes)

### Step 1: Use the Simplified Scripts
The following scripts work without additional packages:
- ✅ `SimplePlayerController.cs` - Player movement (uses legacy Input)
- ✅ `SimpleWeaponController.cs` - Weapon system (uses legacy Input)
- ✅ `SimpleGameUI.cs` - UI system (uses basic Unity UI)
- ✅ `QuickStart.cs` - Auto-setup script
- ✅ `GameManager.cs` - Core game logic
- ✅ `Health.cs` - Health system
- ✅ `Enemy.cs` - Enemy AI
- ✅ `Projectile.cs` - Bullet system

### Step 2: Auto-Setup (Easiest Method)
1. **In Unity, create an empty GameObject**
2. **Add the `QuickStart` component**
3. **Press Play** - it will automatically create everything!

### Step 3: Manual Setup (If Needed)
1. **Create Player:**
   - Create Empty → Name "Player"
   - Add `SimplePlayerController` component
   - Add Character Controller component
   - Add Camera as child

2. **Create Game Manager:**
   - Create Empty → Name "GameManager"
   - Add `GameManager` component

3. **Create UI:**
   - Create Empty → Name "UIManager"
   - Add `SimpleGameUI` component

## Controls
- **WASD** - Move
- **Mouse** - Look around
- **Space** - Jump
- **Left Click** - Shoot
- **Right Click** - Aim
- **R** - Reload
- **Shift** - Sprint

## Package Installation (Optional - For Advanced Features)

If you want the advanced features later, install these packages:

1. **Open Package Manager:** Window → Package Manager
2. **Install these packages:**
   - **Input System** (for advanced input)
   - **TextMeshPro** (for better text)
   - **UI Toolkit** (for advanced UI)

## Common Issues & Solutions

**"Script has errors"**
- ✅ **FIXED:** Use the "Simple" versions of scripts
- Check Console window for any remaining errors

**"Can't move"**
- Make sure Player has `SimplePlayerController` component
- Check if Character Controller is attached

**"No camera view"**
- Make sure Camera is child of Player
- Check Camera position and rotation

**"Scripts not showing in Add Component"**
- Wait for Unity to compile (progress bar at bottom)
- Check Console for compilation errors

## Testing Your Game

1. **Press Play** in Unity
2. **Move around** with WASD
3. **Look around** with mouse
4. **Jump** with Space
5. **Shoot** with Left Click (when weapon is set up)

## Next Steps

Once your basic scene is working:
1. **Add enemies** using the Enemy script
2. **Create weapon data** using WeaponData scriptable objects
3. **Add audio** for footsteps, shooting, etc.
4. **Add particle effects** for muzzle flash, impacts

## Unity 6 Tips

- **HDRP** is enabled by default for realistic graphics
- **Legacy Input Manager** is used by default (no packages needed)
- **Basic UI** works without TextMeshPro
- **Performance:** Keep objects under 1000 for good performance

## Build Your Game

1. **File → Build Settings**
2. **Choose platform** (PC, Mac, Linux)
3. **Click "Build"**
4. **Choose output folder**
5. **Your game is ready to play!**

---

**Need Help?**
- Check Unity Console for error messages
- Use Unity's built-in documentation (Help → Scripting Reference)
- Ask me about any specific issues!

The simplified scripts should work immediately without any package installation! 