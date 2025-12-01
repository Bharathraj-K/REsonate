# RESONATE! 🎯

A fast-paced resonance-based action game where precision and timing are everything. Link enemies of the same color to create devastating chain reactions and survive increasingly challenging waves.

<img width="894" height="596" alt="Screenshot 2025-12-01 191401" src="https://github.com/user-attachments/assets/89eaaed8-9628-42f3-9fde-73cffd8a9d99" />


## 🎮 Gameplay

Master the **resonance system** to link and destroy enemies using color-matching mechanics. Create explosive chain reactions and use strategic timing to clear the battlefield efficiently.

### Core Mechanics
- **Color Linking**: Connect enemies of the same color to destroy them
- **Chain Reactions**: Cluster Drones trigger area destruction when eliminated via resonance
- **Overload Mode**: Devastating special ability that destroys all linked enemies instantly
- **Dynamic Combat**: Face multiple enemy types with unique behaviors and movement patterns

## 🌈 Color System

The game features a vibrant three-color system:
- **🔥 HotOrange** (Red) - Aggressive enemies
- **💜 Magenta** (Green) - Balanced enemies  
- **🔷 Cyan** (Blue) - Fast enemies

Cycle through colors by clicking to match and link enemies strategically.

## 👾 Enemy Types

| Enemy Type | Behavior | Special Ability |
|------------|----------|----------------|
| **Basic Node** | Simple movement | Foundation enemy |
| **Cluster Drone** | Group spawning | Chain destruction on resonance kill |
| **Flicker** | Teleportation | Unpredictable positioning |
| **Resonant Hunter** | Player tracking | Aggressive pursuit |
| **Charger** | High-speed attacks | Rapid movement |

## 🎮 Controls

- **Mouse Movement**: Aim at enemies
- **Left Click**: Cycle colors (HotOrange → Magenta → Cyan → repeat)
- **Spacebar**: Activate Overload Mode (when charged)
- **Auto-targeting**: Automatic enemy linking when colors match

## ✨ Features

- 🎯 **Precision-based gameplay** with color-matching mechanics
- ⚡ **Chain destruction system** for massive combo potential
- 🔥 **Overload Mode** special ability system
- 📊 **High score tracking** for competitive play
- 🎵 **Spatial 3D audio** with immersive sound design
- 🎨 **Particle effects** powered by Cartoon FX
- 🖱️ **Mouse-only controls** for accessibility

## 🛠️ Technical Details

### Built With
- **Unity 3D** - Game engine
- **C#** - Programming language
- **Cartoon FX Remaster** - Visual effects
- **TextMeshPro** - UI text rendering

### System Architecture
- **AudioManager**: Singleton audio system with 3D spatial sound
- **ResonanceSystem**: Core linking and enemy destruction logic
- **EffectsManager**: Particle system management
- **ScoreManager**: High score tracking and combo system
- **Enemy System**: Modular enemy types with inheritance

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3+ LTS
- Modern web browser (for WebGL build)

### Installation
1. Clone the repository
```bash
git clone https://github.com/yourusername/resonate-game.git
```

2. Open in Unity
```bash
cd resonate-game
# Open with Unity Hub or Unity Editor
```

3. Build and Run
- For WebGL: File → Build Settings → WebGL → Build
- For standalone: File → Build Settings → PC, Mac & Linux → Build

## 📱 Platform Support

- **Web Browser** (WebGL) - Primary platform
- **Windows** (Standalone)
- **macOS** (Standalone)
- **Linux** (Standalone)

## 🎯 Game Strategy Tips

1. **Color Management**: Plan your color switches to maximize chain potential
2. **Cluster Priority**: Target Cluster Drones with resonance kills for area damage
3. **Overload Timing**: Save Overload Mode for overwhelming enemy waves
4. **Positioning**: Use enemy movement patterns to create optimal linking opportunities
5. **Chain Building**: Connect multiple enemies before triggering destruction

## 📊 Scoring System

- **Base Enemy Kill**: 100 points
- **Combo Multiplier**: Increases with consecutive kills
- **Chain Destruction**: Bonus points for Cluster Drone chains
- **Overload Bonus**: Extra points during Overload Mode
- **Survival Time**: Additional score over time

## 🔧 Development

### Project Structure
```
Assets/
├── Scripts/
│   ├── Player/          # Player controller and health
│   ├── Enemies/         # Enemy types and AI
│   ├── Systems/         # Core game systems
│   └── UI/              # User interface
├── Scenes/
│   ├── MainMenu.unity   # Main menu scene
│   └── SampleScene.unity # Gameplay scene
├── Audio/               # Sound effects and music
├── Materials/           # Visual materials
└── Prefabs/            # Game object prefabs
```

### Key Systems
- **ResonanceSystem.cs**: Enemy linking and destruction
- **AudioManager.cs**: 3D spatial audio management  
- **EffectsManager.cs**: Particle effects coordination
- **ClusterDrone.cs**: Chain destruction mechanics

## 🎵 Audio Design

Immersive **3D spatial audio** system featuring:
- Position-based sound effects
- Dynamic music transitions
- Audio source pooling for performance
- Menu and gameplay music tracks

## 🐛 Known Issues

- Chain destruction may occasionally not trigger (investigating)
- WebGL builds may have slight audio latency
- High enemy counts can impact performance on lower-end devices

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎮 Play Now

[**🕹️ Play RESONATE! in your browser**](https://boltxcosmo.itch.io/resonate)

## 📞 Contact

- **Developer**: Bharathraj-K
- **Email**: alexmercer8843@gmail.com

---

⭐ **Star this repository if you enjoyed the game!** ⭐

*Made with ❤️ and Unity*
