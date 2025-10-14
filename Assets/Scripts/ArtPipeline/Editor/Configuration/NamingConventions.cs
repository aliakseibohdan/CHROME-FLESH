using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArtPipeline.Editor.Configuration
{
    /// <summary>
    /// Centralized naming convention rules for asset validation
    /// </summary>
    public static class NamingConventions
    {
        #region Prefix Definitions

        public static readonly Dictionary<string, string[]> PrefixRules = new()
        {
            // Texture prefixes
            { "T_", new[] { "Art/", "Materials/", "UI/" } },
            
            // Model prefixes
            { "SM_", new[] { "Art/", "Environment/", "Props/", "Weapons/" } },
            { "SK_", new[] { "Art/Characters/" } },
            { "UCX_", new[] { "Art/", "Environment/", "Props/" } },
            
            // Material prefixes
            { "M_", new[] { "Materials/" } },
            { "M_Master_", new[] { "Materials/Masters/" } },
            
            // Prefab prefixes
            { "PF_", new[] { "Prefabs/", "Art/" } },
            { "PFV_", new[] { "Prefabs/", "Art/" } },
            
            // Audio prefixes
            { "AUD_SFX_", new[] { "Audio/SFX/" } },
            { "AUD_MUS_", new[] { "Audio/Music/" } },
            { "AUD_VOX_", new[] { "Audio/Voice/" } },
            
            // Animation prefixes
            { "A_", new[] { "Art/Characters/", "Art/Weapons/" } },

            // Animator Controllers prefixes
            { "AC_", new[] { "Art/Characters/", "Art/Weapons/", "Art/Environment/" } },
            
            // Font prefixes
            { "F_", new[] { "Art/UI/Fonts/" } },
            
            // ScriptableObject prefixes
            { "SO_", new[] { "Resources/Configs/", "ArtPipeline/Configuration/" } }
        };

        #endregion

        #region Suffix Definitions

        public static readonly Dictionary<string, string[]> TextureSuffixRules = new()
        {
            { "_Albedo", new[] { "Albedo", "Diffuse", "BaseColor" } },
            { "_Normal", new[] { "Normal", "Normals" } },
            { "_Metal", new[] { "Metal", "Metallic", "Metalness" } },
            { "_Roughness", new[] { "Roughness", "Rough" } },
            { "_AO", new[] { "AO", "AmbientOcclusion", "Occlusion" } },
            { "_Emissive", new[] { "Emissive", "Emission", "Glow" } },
            { "_Mask", new[] { "Mask", "Masks" } },
            { "_Detail", new[] { "Detail", "Details" } },
            { "_Height", new[] { "Height", "Displacement" } }
        };

        #endregion

        #region Category Definitions

        public static readonly Dictionary<string, string[]> CategoryRules = new()
        {
            // Character categories
            { "Hero", new[] { "MainCharacter", "Player" } },
            { "Enemy", new[] { "Enemies", "Opponents" } },
            { "NPC", new[] { "NonPlayerCharacter" } },
            
            // Weapon categories
            { "Weapon", new[] { "Gun", "Firearm" } },
            { "Rifle", new[] { "AssaultRifle" } },
            { "Pistol", new[] { "Handgun" } },
            { "Shotgun", new[] { "Shotgun" } },
            { "Sniper", new[] { "SniperRifle" } },
            
            // Environment categories
            { "Prop", new[] { "Props" } },
            { "Arch", new[] { "Architecture" } },
            { "Veg", new[] { "Vegetation" } },
            { "Terrain", new[] { "Landscape" } },
            
            // Audio categories
            { "SFX", new[] { "SoundEffects" } },
            { "MUS", new[] { "Music" } },
            { "VOX", new[] { "Voice" } }
        };

        #endregion

        #region File Size Limits

        public static readonly Dictionary<string, long> FileSizeLimits = new()
        {
            { ".png", 10 * 1024 * 1024 },    // 10MB
            { ".jpg", 5 * 1024 * 1024 },     // 5MB
            { ".tga", 15 * 1024 * 1024 },    // 15MB
            { ".fbx", 20 * 1024 * 1024 },    // 20MB
            { ".obj", 10 * 1024 * 1024 },    // 10MB
            { ".blend", 50 * 1024 * 1024 },  // 50MB
            { ".wav", 5 * 1024 * 1024 },     // 5MB
            { ".mp3", 3 * 1024 * 1024 },     // 3MB
            { ".prefab", 2 * 1024 * 1024 }   // 2MB
        };

        #endregion

        #region Validation Methods

        public static bool IsValidPrefix(string prefix) => PrefixRules.ContainsKey(prefix);

        public static string[] GetAllowedFolders(string prefix) => PrefixRules.ContainsKey(prefix) ? PrefixRules[prefix] : new string[0];

        public static bool IsValidTextureSuffix(string suffix) => TextureSuffixRules.ContainsKey(suffix);

        public static string GetRecommendedSuffix(string textureType)
        {
            foreach (var rule in TextureSuffixRules)
            {
                foreach (var _ in from alias in rule.Value
                                  where alias.Equals(textureType, StringComparison.OrdinalIgnoreCase)
                                  select new { })
                {
                    return rule.Key;
                }
            }
            return null;
        }

        public static long GetMaxFileSize(string extension) =>
                FileSizeLimits.ContainsKey(extension.ToLower()) ?
                FileSizeLimits[extension.ToLower()] : 5 * 1024 * 1024; // Default 5MB

        public static string GenerateExampleName(string prefix, string category, string assetName, string suffix = "") => $"{prefix}{category}_{assetName}{suffix}";

        #endregion

        #region Documentation Generation

        public static string GenerateMarkdownDocumentation()
        {
            StringBuilder md = new();

            _ = md.AppendLine("# Naming Conventions Reference");
            _ = md.AppendLine();
            _ = md.AppendLine("## Prefix Rules");
            _ = md.AppendLine();
            _ = md.AppendLine("| Prefix | Allowed Folders | Description |");
            _ = md.AppendLine("|--------|----------------|-------------|");

            foreach (var rule in PrefixRules)
            {
                string folders = string.Join(", ", rule.Value);
                string description = GetPrefixDescription(rule.Key);
                _ = md.AppendLine($"| `{rule.Key}` | `{folders}` | {description} |");
            }

            _ = md.AppendLine();
            _ = md.AppendLine("## Texture Suffix Rules");
            _ = md.AppendLine();
            _ = md.AppendLine("| Suffix | Aliases | Description |");
            _ = md.AppendLine("|--------|---------|-------------|");

            foreach (var rule in TextureSuffixRules)
            {
                string aliases = string.Join(", ", rule.Value);
                string description = GetSuffixDescription(rule.Key);
                _ = md.AppendLine($"| `{rule.Key}` | {aliases} | {description} |");
            }

            _ = md.AppendLine();
            _ = md.AppendLine("## File Size Limits");
            _ = md.AppendLine();
            _ = md.AppendLine("| File Type | Maximum Size |");
            _ = md.AppendLine("|-----------|-------------|");

            foreach (var rule in FileSizeLimits)
            {
                long sizeMB = rule.Value / 1024 / 1024;
                _ = md.AppendLine($"| `{rule.Key}` | {sizeMB}MB |");
            }

            return md.ToString();
        }

        private static string GetPrefixDescription(string prefix)
        {
            Dictionary<string, string> descriptions = new()
            {
                { "T_", "Textures (Albedo, Normal, etc.)" },
                { "SM_", "Static Meshes (Environment props)" },
                { "SK_", "Skeletal Meshes (Characters)" },
                { "UCX_", "Collision Meshes" },
                { "M_", "Material Instances" },
                { "M_Master_", "Master Materials" },
                { "PF_", "Prefabs" },
                { "PFV_", "Prefab Variants" },
                { "AUD_SFX_", "Sound Effects" },
                { "AUD_MUS_", "Music" },
                { "AUD_VOX_", "Voice Lines" },
                { "A_", "Animations" },
                { "F_", "Fonts" },
                { "SO_", "Scriptable Objects" }
            };

            return descriptions.ContainsKey(prefix) ? descriptions[prefix] : "Unknown prefix";
        }

        private static string GetSuffixDescription(string suffix)
        {
            Dictionary<string, string> descriptions = new()
            {
                { "_Albedo", "Base color texture" },
                { "_Normal", "Normal map for surface details" },
                { "_Metal", "Metallic map (white = metal)" },
                { "_Roughness", "Roughness map (white = rough)" },
                { "_AO", "Ambient occlusion map" },
                { "_Emissive", "Emissive/glow map" },
                { "_Mask", "Multi-purpose mask texture" },
                { "_Detail", "Detail map for close-up surfaces" },
                { "_Height", "Height map for displacement" }
            };

            return descriptions.ContainsKey(suffix) ? descriptions[suffix] : "Unknown suffix";
        }

        #endregion
    }
}
