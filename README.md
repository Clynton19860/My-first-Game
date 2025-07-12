# 🎮 Unity FPS Game Project

## **Project Overview**
A first-person shooter game built with Unity, featuring basic movement, shooting mechanics, and a procedurally generated arena. This is a learning project for Unity development.

## **Current Status**
- ✅ Basic Unity project setup completed
- ✅ Core scripts imported and working
- ✅ Player movement and camera controls implemented
- ✅ Basic weapon system with projectiles
- ✅ Procedural level generation with arena, cover, and obstacles
- ✅ Simple enemy spawning system
- ✅ Basic UI framework

## **Features (Currently Implemented)**
- 🎯 Basic weapon mechanics with projectiles
- 🏃 Player movement (WASD) and mouse look
- 🎮 Simple arena generation with cover and obstacles
- 👾 Basic enemy spawning system
- 🎨 Simple UI framework
- 🔧 Modular script architecture

## **Development Roadmap**

### **Phase 1: Foundation (Week 1-2)**
- [x] Unity project setup
- [x] Core game architecture
- [x] Player movement and camera systems
- [x] Basic weapon framework
- [x] Simple level generation
- [x] Basic enemy spawning

### **Phase 2: Core Gameplay (Week 3-4)**
- [x] Advanced weapon systems (reload, different weapons) ✅
- [x] Enemy AI and behavior ✅
- [x] Health and damage systems ✅
- [x] Combat mechanics and feedback ✅
- [x] Score and progression systems ✅

### **Phase 3: Graphics & Polish (Week 5-6)**
- [ ] Better materials and lighting
- [ ] Particle effects for shooting and impacts
- [ ] UI and HUD systems
- [ ] Sound effects and audio
- [ ] Performance optimization

### **Phase 4: Advanced Features (Week 7-8)**
- [ ] Multiple enemy types
- [ ] Power-ups and collectibles
- [ ] Level variety and progression
- [ ] Menu systems
- [ ] Build and deployment

## **Project Structure**
```
game/
├── README.md                 # This file
├── Assets/                   # Unity assets folder
│   ├── Scripts/             # C# scripts
│   │   ├── Core/           # Core game systems
│   │   │   ├── GameManager.cs
│   │   │   ├── Health.cs
│   │   │   ├── LevelGenerator.cs
│   │   │   ├── PackageInstaller.cs
│   │   │   └── QuickStart.cs
│   │   ├── Player/         # Player-related scripts
│   │   │   └── SimplePlayerController.cs
│   │   ├── Weapons/        # Weapon systems
│   │   │   ├── Projectile.cs
│   │   │   └── SimpleWeaponController.cs
│   │   ├── Enemies/        # Enemy AI
│   │   │   ├── Enemy.cs
│   │   │   └── EnemySpawner.cs
│   │   └── UI/             # User interface
│   │       └── SimpleGameUI.cs
│   └── My first scene.unity # Main game scene
├── ProjectSettings/         # Unity project settings
└── Packages/               # Unity packages
```

## **Getting Started**

### **Prerequisites**
1. **Unity 2023.2 LTS** or newer
2. **Visual Studio Community** or **Visual Studio Code**
3. **Git** for version control

### **Installation Steps**
1. Download Unity Hub from [unity.com](https://unity.com)
2. Install Unity 2023.2 LTS
3. Clone this repository
4. Open the project in Unity
5. The project uses simplified scripts that don't require additional packages

### **First Run**
1. Open the project in Unity
2. Open the "My first scene" scene
3. Add a QuickStart component to any GameObject in the scene
4. Press Play to generate the arena and start the game
5. Use WASD to move, Mouse to look, Left Click to shoot

## **Controls**
- **WASD** - Movement
- **Mouse** - Look around
- **Left Click** - Shoot
- **Space** - Jump (if implemented)

## **Development Guidelines**

### **Code Standards**
- Use PascalCase for class names
- Use camelCase for variables and methods
- Add XML documentation for public methods
- Follow Unity naming conventions

### **Current Scripts Overview**
- **QuickStart.cs**: Main entry point that generates the arena and sets up the game
- **SimplePlayerController.cs**: Handles player movement and camera controls
- **SimpleWeaponController.cs**: Basic shooting mechanics with projectiles
- **LevelGenerator.cs**: Creates the arena with cover and obstacles
- **EnemySpawner.cs**: Spawns basic enemies
- **SimpleGameUI.cs**: Basic UI framework

## **Next Steps**
1. **Add Health System**: Implement player and enemy health with damage
2. **Improve Enemy AI**: Make enemies move and attack the player
3. **Add UI Elements**: Health bar, ammo counter, score display
4. **Sound Effects**: Add shooting sounds, footsteps, etc.
5. **Visual Polish**: Better materials, lighting, and effects

## **Troubleshooting**
- **Objects disappear after Play mode**: This is normal - runtime-generated objects only exist during Play mode
- **Missing packages**: This project uses simplified scripts that don't require additional Unity packages
- **Script errors**: Make sure all scripts are properly attached to GameObjects in the scene

## **Contributing**
1. Create a feature branch
2. Implement your changes
3. Test thoroughly
4. Submit a pull request

## **License**
This project is licensed under the MIT License - see LICENSE file for details.

## **Support**
For questions or issues, please create an issue in the repository or contact the development team.

---

**Happy Gaming! 🎮** 