# CHROME FLESH - Art Pipeline Guide

## 📁 Folder Structure
- All assets must go in their designated folders
- No loose files in root directories
- Use subfolders for organization

## 🏷️ Naming Conventions
### Textures
- Use descriptive names: `T_Weapon_AssaultRifle_Albedo`
- Include texture type: Albedo, Normal, Metal, Roughness
- Resolution: Power of 2 (1024, 2048, 4096)

### Models
- Static Meshes: `SM_Prop_Crate_01`
- Skeletal Meshes: `SK_Character_Soldier`
- Collision Meshes: `UCX_Prop_Crate_01`

### Prefabs
- Base: `PF_Weapon_AssaultRifle`
- Variants: `PFV_Weapon_AssaultRifle_Desert`

## ⚙️ Import Settings

### Textures
- Compression: BC7 for color, BC5 for normals
- sRGB: ON for albedo, OFF for metal/roughness
- Generate Mip Maps: ON

### Models
- Scale Factor: 1.0
- Read/Write: OFF (unless needed for runtime modification)
- Generate Colliders: OFF (use separate collision meshes)

## 🔊 Audio Import Settings

### Sound Effects (SFX)
- Load Type: Compressed In Memory
- Compression: Vorbis (Quality: 70%)
- Preload Audio Data: YES
- Force To Mono: YES (unless spatial audio is needed)

### Music
- Load Type: Streaming  
- Compression: Vorbis (Quality: 90%)
- Preload Audio Data: NO
- Force To Mono: NO

### Voice
- Load Type: Decompress On Load (PC) / Compressed In Memory (Mobile)
- Compression: PCM (PC) / Vorbis (Mobile)
- Preload Audio Data: YES
- Force To Mono: YES

### File Size Limits
- SFX: ≤ 2MB per clip
- Music: ≤ 20MB per track  
- Voice: ≤ 5MB per line

## 🎯 LOD (Level of Detail) Guidelines

### When to Use LODs
- **Characters**: 4 LOD levels (10k+ triangles)
- **Weapons**: 2-3 LOD levels (2k-5k triangles)  
- **Environment Props**: 3 LOD levels (1k-10k triangles)
- **Small Props**: 0-1 LOD levels (<500 triangles)

### Triangle Reduction Targets
- **LOD1**: 50% of original triangles
- **LOD2**: 25% of original triangles  
- **LOD3**: 10% of original triangles
- **LOD4**: 5% of original triangles (distant objects)

### Screen Coverage Thresholds
- **LOD0**: 60% screen height (close)
- **LOD1**: 30% screen height (medium)
- **LOD2**: 15% screen height (far)
- **LOD3**: 5% screen height (very far)

### Usage
1. Select GameObject in scene or prefab
2. Run `Tools > Art Pipeline > Generate LODs for Selected`
3. For batch processing: `Tools > Art Pipeline > Generate LODs for All in Folder`
4. Analyze complexity: `Tools > Art Pipeline > Analyze Mesh Complexity`

### Best Practices
- Enable "Read/Write" on mesh import settings for LOD generation
- Test LOD transitions in-game for visual quality
- Use simpler LODs for mobile platforms
- Consider disabling shadows on distant LODs for performance

## 🔄 Prefab Rules
- Create base prefabs for core assets
- Use variants for cosmetic changes
- Nest prefabs logically
- Mark as Addressable when needed for dynamic loading

## 🚫 Validation Rules
Assets will be automatically validated on import:
- ✅ Correct naming convention
- ✅ Proper folder location  
- ✅ Optimal import settings
- ✅ Reasonable file sizes
- ❌ No missing references

Run validation manually: `Tools > Art Pipeline > Validate All Assets`

## 📚 Documentation

### Naming Conventions
- Complete guide: `Assets/Scripts/ArtPipeline/Documentation/NamingConventions.md`
- Generate updated docs: `Tools > Art Pipeline > Generate Naming Documentation`
- Open documentation: `Tools > Art Pipeline > Open Naming Documentation`

### Rule Updates
When adding new asset types or changing conventions:
1. Update `NamingConventions.cs` 
2. Run documentation generator
3. Verify validation still passes
4. Update this README if needed
