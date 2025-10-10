# CHROME FLESH - Complete Development Guide

## 🎯 Project Overview

A modern 3D First-Person Shooter built in Unity with a scalable architecture, automated asset pipeline, and professional development workflows. This project emphasizes code quality, performance optimization, and team collaboration.

---

## 🚀 Quick Start

### Prerequisites
- **Unity**: 6.2 or newer
- **IDE**: Visual Studio 2022 or Rider
- **Version Control**: Git with LFS enabled
- **Platform**: Windows/macOS (Windows recommended for full toolchain)

### First-Time Setup
1. **Clone Repository**
   ```bash
   git clone [repository-url]
   cd CHROME-FLESH
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Add project folder
   - Use Unity 6.2 or newer

3. **Run Initial Validation**
   - Open Unity Editor
   - Go to `Tools > Art Pipeline > Validate All Assets`
   - Fix any validation errors before starting

4. **Test Boot Sequence**
   - Open `Assets/Scenes/Bootstrap.unity`
   - Press Play - should load MainMenu scene
   - Check Console for service initialization logs

---

## 🏗️ Architecture Overview

### Core Systems
```
Core/
├── Boot/              # Game initialization
├── Services/          # Dependency injection & service locator
├── Events/            # Message bus system
├── Input/             # Input abstraction layer
└── Configuration/     # Runtime configuration
```

### Key Design Patterns
- **Service Locator**: Global access to core systems
- **Event Bus**: Decoupled communication
- **Dependency Injection**: Testable, modular code
- **ScriptableObjects**: Data-driven configuration

### Service Access
```csharp
// Anywhere in your code:
var eventBus = ServiceLocator.Resolve<IEventBus>();
var inputService = ServiceLocator.Resolve<IInputService>();
```

---

## 👨‍💻 For Programmers

### Code Structure
```
Assets/Scripts/
├── Core/              # Framework systems
├── Gameplay/          # Game-specific logic
├── UI/                # User interface
└── Utilities/         # Helper classes
```

### Development Workflow

1. **Create New Feature**
   ```csharp
   // 1. Define events (if needed)
   public class PlayerHealthChangedEvent
   {
       public float CurrentHealth;
       public float MaxHealth;
   }

   // 2. Create system
   public class HealthSystem : MonoBehaviour
   {
       private IEventBus _eventBus;

       private void Start()
       {
           _eventBus = ServiceLocator.Resolve<IEventBus>();
       }
   }
   ```

2. **Follow Naming Conventions**
   - Interfaces: `I[Name]` (`IHealthSystem`)
   - Classes: `PascalCase` (`PlayerController`)
   - Events: `[Subject][Action]Event` (`PlayerShootEvent`)

3. **Use Event System**
   ```csharp
   // Publishing
   _eventBus.Publish(new PlayerShootEvent { Weapon = currentWeapon });

   // Subscribing
   _eventBus.Subscribe<PlayerShootEvent>(OnPlayerShoot);
   ```

### Key Services

| Service | Purpose | Access |
|---------|---------|---------|
| `IEventBus` | Pub/sub messaging | `ServiceLocator.Resolve<IEventBus>()` |
| `IInputService` | Abstracted input | `ServiceLocator.Resolve<IInputService>()` |
| `ISceneService` | Scene management | `ServiceLocator.Resolve<ISceneService>()` |

---

## 🎨 For Artists

### Asset Pipeline Overview

```
Assets/Art/
├── Characters/        # Hero, enemies, NPCs
├── Environment/       # Props, architecture, terrain
├── Weapons/          # Firearms, melee weapons
└── UI/               # Icons, fonts, sprites
```

### Quick Start Guide

1. **Add New Asset**
   - Place in correct folder (see Naming Conventions)
   - Follow naming rules: `[Prefix]_[Category]_[Name]_[Suffix]`
   - Validator will auto-check on import

2. **Naming Examples**
   ```
   ✅ T_Weapon_Rifle_Albedo.png
   ✅ SM_Prop_Crate_01.fbx
   ✅ PF_Enemy_Grunt.prefab
   ✅ AUD_SFX_Weapon_Shotgun.wav
   ❌ assault rifle texture.png (no prefix, spaces)
   ```

3. **Generate LODs**
   - Select model in scene
   - `Tools > Art Pipeline > Generate LODs for Selected`
   - Configure in `LOD Settings Editor`

### Validation Rules
- ✅ Correct naming prefix
- ✅ Proper folder location
- ✅ Optimal file sizes
- ✅ Correct import settings
- ❌ No missing references

Run validation: `Tools > Art Pipeline > Validate Selected Assets`

---

## 🔧 For Technical Artists

### Pipeline Tools

| Tool | Purpose | Access |
|------|---------|---------|
| **Asset Validator** | Quality control | `Tools > Art Pipeline > Validate All Assets` |
| **LOD Generator** | Auto LOD creation | `Tools > Art Pipeline > Generate LODs` |
| **Addressables Setup** | Asset bundling | `Tools > Art Pipeline > Setup Addressables` |
| **Import Presets** | Auto-optimization | Applied on asset import |

### Configuration

1. **LOD Settings**
   - Edit: `Resources/ArtPipeline/LODSettings.asset`
   - Or use: `Tools > Art Pipeline > LOD Settings Editor`

2. **Naming Conventions**
   - Rules: `ArtPipeline/Configuration/NamingConventions.cs`
   - Docs: `ArtPipeline/Documentation/NamingConventions.md`

3. **Import Presets**
   - Textures: Automatic compression based on type
   - Models: Optimized import settings
   - Audio: Platform-specific compression

### Customization
```csharp
// Add new naming rules in NamingConventions.cs
public static readonly Dictionary<string, string[]> PrefixRules = new()
{
    { "VFX_", new[] { "Art/Effects/" } }, // New rule
};
```

---

## 🎮 For Designers

### Configuration System

1. **ScriptableObject Configs**
   - Location: `Assets/Resources/Configs/`
   - Types: Game balance, input mapping, audio settings

2. **Create New Config**
   ```csharp
   [CreateAssetMenu(menuName = "Configs/WeaponConfig")]
   public class WeaponConfig : ScriptableObject
   {
       public float Damage;
       public float FireRate;
       public AudioClip ShootSound;
   }
   ```

3. **Access in Game**
   ```csharp
   var weaponConfig = Resources.Load<WeaponConfig>("Configs/AssaultRifleConfig");
   ```

### Scene Management

1. **Add New Level**
   - Create scene in `Assets/Scenes/`
   - Update build settings
   - Use `ISceneService` for loading

2. **Scene Loading**
   ```csharp
   var sceneService = ServiceLocator.Resolve<ISceneService>();
   StartCoroutine(sceneService.LoadSceneAsync("Level_02"));
   ```

---

## 📦 Asset Management

### Addressables System

**Groups:**
- `Characters` - Player and enemy prefabs
- `Weapons` - All weapon assets
- `Environment_Level_XX` - Level-specific content
- `UI` - Interface elements
- `Audio_XXX` - Organized audio content

**Usage:**
```csharp
// Mark asset as addressable
// Tools > Art Pipeline > Mark Selected as Addressable

// Load at runtime
Addressables.LoadAssetAsync<GameObject>("PF_Weapon_AssaultRifle");
```

### Prefab Variants Strategy

- **Base Prefabs**: Core functionality (`PF_Weapon_Rifle`)
- **Variants**: Cosmetic/behavior changes (`PFV_Weapon_Rifle_Desert`)

---

## 🔄 Development Workflows

### Daily Workflow

1. **Start Work**
   ```bash
   git pull
   Open Unity
   Run Tools > Art Pipeline > Validate All Assets
   ```

2. **Create Assets**
   - Follow naming conventions
   - Place in correct folders
   - Validation runs automatically on import

3. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat: Add assault rifle model"
   git push
   ```

### Branch Strategy
- `main` - Production-ready code
- `develop` - Integration branch
- `feature/xxx` - New features
- `art/xxx` - Asset updates

### Commit Message Convention
```
feat: New weapon system
art: Add enemy character models
fix: Resolve input mapping issue
docs: Update naming conventions
```

---

## 🧪 Testing & Validation

### Automated Checks

1. **Asset Validation**
   - Runs on import
   - CI/CD pipeline integration
   - Custom rules in `AssetValidator.cs`

2. **Manual Validation**
   ```bash
   # Run full validation
   Tools > Art Pipeline > Validate All Assets

   # Check specific assets
   Select assets → Right-click → Validate Asset
   ```

3. **CI/CD Pipeline**
   - Automatic validation on push
   - Addressables build verification
   - Exit codes: 0=success, 1=failure

### Performance Guidelines

| Asset Type | Limits | Notes |
|------------|--------|-------|
| **Textures** | ≤10MB | Power of two dimensions |
| **Models** | ≤20MB | LODs required for >2K triangles |
| **Audio** | ≤5MB (SFX) | Streaming for music >2MB |
| **Prefabs** | ≤2MB | Minimize nested prefabs |

---

## 🛠️ Tools & Automation

### Available Tools

| Command | Purpose | Target |
|---------|---------|---------|
| `Validate All Assets` | Full project validation | All Team |
| `Generate LODs` | Auto LOD generation | Artists |
| `LOD Settings Editor` | Configure LOD profiles | Tech Artists |
| `Setup Addressables` | Initialize asset bundles | Programmers |
| `Generate Documentation` | Update naming docs | Tech Artists |

### Custom Tool Development

**Create New Tool:**
```csharp
[MenuItem("Tools/Custom/My Tool")]
public static void MyTool()
{
    // Tool implementation
}
```

**Location:** `Assets/Scripts/ArtPipeline/Editor/`

---

## 🐛 Troubleshooting

### Common Issues

1. **ServiceLocator Not Initialized**
   ```
   Error: ServiceLocator has not been initialized
   Fix: Ensure Bootstrap scene runs first in build settings
   ```

2. **Asset Validation Failures**
   ```
   Error: Invalid naming convention
   Fix: Use Tools > Art Pipeline > Open Naming Documentation
   ```

3. **Missing References**
   ```
   Error: Prefab contains missing references
   Fix: Reimport assets or check dependency chain
   ```

4. **Addressables Build Failures**
   ```
   Error: Addressables build failed
   Fix: Run Tools > Art Pipeline > Validate All Assets first
   ```

### Debug Tips

1. **Enable Verbose Logging**
   - Select `GameInitializer` in Bootstrap scene
   - Enable `Verbose Logging` in inspector

2. **Service Status**
   ```csharp
   Debug.Log($"EventBus registered: {ServiceLocator.IsRegistered<IEventBus>()}");
   ```

3. **Event Debugging**
   ```csharp
   _eventBus.Subscribe<AnyEvent>(e => Debug.Log($"Event: {e.GetType().Name}"));
   ```

---

## 📚 Documentation & Resources

### Key Documents
- **This README**: Project overview & workflows
- **Naming Conventions**: `Assets/Scripts/ArtPipeline/Documentation/NamingConventions.md`
- **Architecture Guide**: `Assets/Scripts/Documentation/Architecture.md`
- **Art Pipeline**: `Assets/Artists_README.md`

### Generated Documentation
- Update naming docs: `Tools > Art Pipeline > Generate Naming Documentation`
- Always regenerated when rules change

### Learning Resources
- Unity Input System documentation
- Addressables package guide
- ScriptableObject architecture patterns

---

## 🤝 Team Collaboration

### Communication
- **Daily Standups**: Progress and blockers
- **Art Reviews**: Weekly asset reviews
- **Tech Design**: System architecture discussions

### Quality Standards
- All assets must pass validation
- Code follows established patterns (0 errors, 0 warnings)
- Regular performance profiling
- Addressables for dynamic loading

### Support Channels
- Programmers → Technical artists for asset issues
- Artists → Programmers for tool requests
- All → Documentation for conventions

---

## 🚢 Release Process

### Pre-Release Checklist
1. [ ] Run full asset validation
2. [ ] Verify all scenes load correctly
3. [ ] Test on target platforms
4. [ ] Profile performance metrics
5. [ ] Update documentation
6. [ ] Build Addressables
7. [ ] Final QA pass

### Build Process
```bash
# 1. Validation
Tools > Art Pipeline > Validate All Assets

# 2. Build Addressables
Window > Asset Management > Addressables > Build

# 3. Player Build
File > Build Settings > Build
```

---

## 📞 Support

### Getting Help
1. Check documentation first
2. Search existing issues
3. Ask in appropriate channel (art/programming/design)
4. Create detailed bug report with reproduction steps

### Reporting Issues
```
Title: [Area] Brief description
Steps to reproduce:
1. 
2. 
Expected behavior:
Actual behavior:
Assets/Logs:
```

**Last Updated:** 2025.10.11

---

*This project follows professional game development practices and is designed to scale from prototype to production.*
