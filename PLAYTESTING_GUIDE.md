# 🎮 Complete FPS Game Playtesting Guide

## **Quick Start - 3 Steps to Play Test**

### **Step 1: Scene Setup**
1. **Open Unity** and your project
2. **Create a new scene** or open existing scene
3. **Add GameInitializer** to any GameObject in the scene
4. **Press Play** - everything will be set up automatically!

### **Step 2: Basic Controls**
- **WASD**: Move around
- **Mouse**: Look around
- **Left Click**: Shoot
- **R**: Reload weapon
- **Escape**: Pause menu

### **Step 3: What to Test**
- ✅ **Movement**: Walk around the arena
- ✅ **Shooting**: Click to fire projectiles
- ✅ **Enemies**: Red cubes that move toward you
- ✅ **Power-ups**: Yellow spheres you can collect
- ✅ **Health**: Take damage from enemies
- ✅ **UI**: Check HUD information
- ✅ **Pause**: Press Escape for menu

---

## **🎯 Complete Testing Checklist**

### **Phase 1: Foundation Testing**
- [ ] **Player Movement**
  - [ ] WASD movement works smoothly
  - [ ] Mouse look responds correctly
  - [ ] No clipping through walls
  - [ ] Movement speed feels good

- [ ] **Weapon System**
  - [ ] Left click fires projectiles
  - [ ] Projectiles travel in correct direction
  - [ ] Reload (R key) works
  - [ ] Ammo counter updates

- [ ] **Level Generation**
  - [ ] Arena generates properly
  - [ ] Cover objects are placed
  - [ ] No overlapping objects
  - [ ] Arena size is appropriate

### **Phase 2: Core Gameplay Testing**
- [ ] **Health System**
  - [ ] Player starts with 100 health
  - [ ] Taking damage reduces health
  - [ ] Health bar updates correctly
  - [ ] Death occurs at 0 health

- [ ] **Enemy AI**
  - [ ] Enemies spawn correctly
  - [ ] Enemies move toward player
  - [ ] Enemies attack when close
  - [ ] Enemies die when shot

- [ ] **Combat Mechanics**
  - [ ] Shooting enemies deals damage
  - [ ] Enemy attacks damage player
  - [ ] Score increases when killing enemies
  - [ ] Combat feels responsive

### **Phase 3: Graphics & Polish Testing**
- [ ] **Particle Effects**
  - [ ] Explosions appear when shooting
  - [ ] Impact effects on surfaces
  - [ ] Power-up collection effects
  - [ ] Performance remains good

- [ ] **Audio System**
  - [ ] Shooting sounds play
  - [ ] Footstep sounds (if implemented)
  - [ ] Enemy death sounds
  - [ ] Power-up collection sounds

- [ ] **Enhanced UI**
  - [ ] Health bar displays correctly
  - [ ] Ammo counter works
  - [ ] Score display updates
  - [ ] FPS counter shows

### **Phase 4: Advanced Features Testing**
- [ ] **Multiple Enemy Types**
  - [ ] Different enemy colors appear
  - [ ] Enemies have different behaviors
  - [ ] Enemy types unlock progressively
  - [ ] Boss enemies spawn correctly

- [ ] **Power-up System**
  - [ ] Power-ups spawn in arena
  - [ ] Collecting power-ups works
  - [ ] Power-up effects apply correctly
  - [ ] Power-ups have visual effects

- [ ] **Level Progression**
  - [ ] Waves progress correctly
  - [ ] Difficulty increases over time
  - [ ] Boss waves occur
  - [ ] Level completion works

- [ ] **Menu System**
  - [ ] Escape opens pause menu
  - [ ] Resume game works
  - [ ] Restart level works
  - [ ] Quit game works

---

## **🔧 Advanced Testing Scenarios**

### **Scenario 1: Combat Testing**
1. **Spawn multiple enemies**
2. **Test different weapon ranges**
3. **Try to survive as long as possible**
4. **Check if all systems work together**

### **Scenario 2: Power-up Testing**
1. **Collect all power-ups in the arena**
2. **Test each power-up effect**
3. **Verify effects expire correctly**
4. **Check UI updates for active effects**

### **Scenario 3: Performance Testing**
1. **Spawn many enemies at once**
2. **Monitor FPS counter**
3. **Check for frame drops**
4. **Verify memory usage**

### **Scenario 4: Menu Testing**
1. **Open pause menu during combat**
2. **Test all menu options**
3. **Verify game state preservation**
4. **Check settings functionality**

---

## **🐛 Common Issues & Solutions**

### **Issue: Player can't move**
**Solution**: Check if SimplePlayerController is attached to Player GameObject

### **Issue: Can't shoot**
**Solution**: Verify SimpleWeaponController component is present

### **Issue: Enemies don't spawn**
**Solution**: Ensure SimpleEnemySpawner is in the scene

### **Issue: No UI elements**
**Solution**: Check if Canvas and EnhancedGameUI are created

### **Issue: Performance problems**
**Solution**: Look for PerformanceOptimizer in scene and check FPS counter

### **Issue: Audio not working**
**Solution**: Verify AudioManager is present and audio sources are set up

---

## **📊 Testing Metrics**

### **Performance Targets**
- **Target FPS**: 60 FPS minimum
- **Memory Usage**: Under 500MB
- **Load Time**: Under 10 seconds
- **Response Time**: Under 100ms for inputs

### **Gameplay Metrics**
- **Enemy Spawn Rate**: 1-3 enemies per 10 seconds
- **Power-up Spawn Rate**: 1 power-up per 15 seconds
- **Player Health**: 100 starting, 0 = death
- **Ammo Capacity**: 30 rounds, reload time 2 seconds

---

## **🎮 Testing Commands**

### **Console Commands** (if implemented)
```
// Spawn enemies
spawn_enemy 5

// Give player health
heal_player 50

// Add ammo
add_ammo 30

// Spawn power-up
spawn_powerup health

// Toggle god mode
god_mode true
```

### **Debug Information**
Check the Console for:
- System initialization messages
- Enemy spawn notifications
- Power-up collection logs
- Performance warnings
- Error messages

---

## **📝 Testing Report Template**

### **Test Session Information**
- **Date**: _______________
- **Tester**: _______________
- **Duration**: _______________
- **Build Version**: _______________

### **Test Results**
- **Movement**: ⭐⭐⭐⭐⭐ (5/5)
- **Combat**: ⭐⭐⭐⭐⭐ (5/5)
- **Enemy AI**: ⭐⭐⭐⭐⭐ (5/5)
- **Power-ups**: ⭐⭐⭐⭐⭐ (5/5)
- **UI/UX**: ⭐⭐⭐⭐⭐ (5/5)
- **Performance**: ⭐⭐⭐⭐⭐ (5/5)
- **Audio**: ⭐⭐⭐⭐⭐ (5/5)

### **Issues Found**
1. **Issue**: _______________
   - **Severity**: High/Medium/Low
   - **Steps to Reproduce**: _______________
   - **Expected vs Actual**: _______________

### **Suggestions**
- _______________
- _______________
- _______________

---

## **🚀 Ready to Test!**

Your complete FPS game is now ready for comprehensive playtesting. Use the GameInitializer script to automatically set up all systems, then follow this guide to test every feature thoroughly.

**Happy Testing! 🎯** 