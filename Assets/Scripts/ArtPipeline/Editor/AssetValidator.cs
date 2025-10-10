using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Main asset validation system using centralized naming conventions
    /// </summary>
    public static class AssetValidator
    {
        [MenuItem("Tools/Art Pipeline/Validate All Assets", false, 1)]
        public static void ValidateAllAssets()
        {
            string[] allAssets = AssetDatabase.FindAssets("")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => !path.StartsWith("Assets/Plugins") &&
                              !path.StartsWith("Assets/Editor") &&
                              !path.StartsWith("Assets/ArtPipeline/Documentation"))
                .ToArray();

            Dictionary<string, ValidationResult> results = new();
            int passed = 0, failed = 0;

            EditorUtility.DisplayProgressBar("Validating Assets", "Starting validation...", 0f);

            try
            {
                for (int i = 0; i < allAssets.Length; i++)
                {
                    string assetPath = allAssets[i];
                    if (ShouldSkipValidation(assetPath))
                    {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar("Validating Assets", Path.GetFileName(assetPath), (float)i / allAssets.Length);

                    var result = ValidateAsset(assetPath);
                    results[assetPath] = result;

                    if (result.IsValid)
                    {
                        passed++;
                    }
                    else
                    {
                        failed++;
                    }
                }

                Debug.Log($"=== ASSET VALIDATION COMPLETE ===");
                Debug.Log($"✅ Passed: {passed}");
                Debug.Log($"❌ Failed: {failed}");
                Debug.Log($"📊 Total Validated: {results.Count}");

                List<KeyValuePair<string, ValidationResult>> failures = results.Where(r => !r.Value.IsValid).ToList();
                if (failures.Count > 0)
                {
                    Debug.LogError($"=== VALIDATION FAILURES ({failures.Count}) ===");
                    foreach (var failure in failures)
                    {
                        Debug.LogError($"{failure.Key}: {failure.Value.ToShortString()}");
                        if (failure.Value.Errors.Count > 0)
                        {
                            foreach (string error in failure.Value.Errors.Take(3))
                            {
                                Debug.LogError($"   - {error}");
                            }
                        }
                    }
                }

                List<KeyValuePair<string, ValidationResult>> warnings = results.Where(r => r.Value.Warnings.Count > 0).ToList();
                if (warnings.Count > 0)
                {
                    Debug.LogWarning($"=== VALIDATION WARNINGS ({warnings.Count}) ===");
                    foreach (var warning in warnings.Take(10))
                    {
                        Debug.LogWarning($"{warning.Key}: {warning.Value.Warnings.Count} warnings");
                    }
                }

                if (failures.Count > 0)
                {
                    if (Application.isBatchMode)
                    {
                        EditorApplication.Exit(1);
                    }
                    else
                    {
                        _ = EditorUtility.DisplayDialog("Validation Failed",
                            $"{failures.Count} assets failed validation. Check console for details.", "OK");
                    }
                }
                else
                {
                    if (Application.isBatchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                    else
                    {
                        _ = EditorUtility.DisplayDialog("Validation Complete",
                            $"All assets passed validation!\nPassed: {passed}\nWarnings: {warnings.Count}", "OK");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Tools/Art Pipeline/Validate Selected Assets", false, 2)]
        public static void ValidateSelectedAssets()
        {
            var selectedObjects = Selection.objects;
            if (selectedObjects.Length == 0)
            {
                _ = EditorUtility.DisplayDialog("Validation", "No assets selected", "OK");
                return;
            }

            List<(string, ValidationResult)> results = new();

            foreach (var obj in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (ShouldSkipValidation(path))
                {
                    continue;
                }

                var result = ValidateAsset(path);
                results.Add((path, result));

                string message = $"{Path.GetFileName(path)}: {result.ToShortString()}";
                if (result.IsValid)
                {
                    if (result.Warnings.Count > 0 || result.Suggestions.Count > 0)
                    {
                        Debug.LogWarning(message);
                    }
                    else
                    {
                        Debug.Log(message);
                    }
                }
                else
                {
                    Debug.LogError(message);
                }
            }

            int failed = results.Count(r => !r.Item2.IsValid);
            int warnings = results.Sum(r => r.Item2.Warnings.Count);

            if (failed > 0)
            {
                _ = EditorUtility.DisplayDialog("Validation Complete",
                    $"{failed} of {results.Count} selected assets failed validation.", "OK");
            }
            else
            {
                _ = EditorUtility.DisplayDialog("Validation Complete",
                    $"All selected assets passed!\nWarnings: {warnings}", "OK");
            }
        }

        [MenuItem("Assets/Validate Asset", false, 20)]
        public static void ValidateAssetFromContext()
        {
            var selected = Selection.activeObject;
            if (selected == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(selected);
            var result = ValidateAsset(path);

            if (result.IsValid)
            {
                if (result.Warnings.Count > 0 || result.Suggestions.Count > 0)
                {
                    _ = EditorUtility.DisplayDialog("Asset Validation",
                        $"✅ Validation passed with notes:\n{result}", "OK");
                }
                else
                {
                    _ = EditorUtility.DisplayDialog("Asset Validation", "✅ Validation passed!", "OK");
                }
            }
            else
            {
                _ = EditorUtility.DisplayDialog("Asset Validation",
                    $"❌ Validation failed:\n{result}", "OK");
            }
        }

        public static ValidationResult ValidateAsset(string assetPath)
        {
            ValidationResult result = new();
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath).ToLower();

            if (assetPath.EndsWith(".meta") || Directory.Exists(assetPath))
            {
                return result;
            }

            ValidateNamingConvention(assetPath, fileName, result);

            ValidateFolderLocation(assetPath, fileName, result);

            ValidateFileSize(assetPath, extension, result);

            ValidateImportSettings(assetPath, result);

            if (assetPath.EndsWith(".prefab"))
            {
                ValidatePrefabReferences(assetPath, result);
            }

            if (IsTextureFile(assetPath))
            {
                ValidateTextureSpecifics(assetPath, result);
            }

            if (IsModelFile(assetPath))
            {
                ValidateModelSpecifics(assetPath, result);
            }

            if (IsAudioFile(assetPath))
            {
                ValidateAudioSpecifics(assetPath, result);
            }

            return result;
        }

        private static bool ShouldSkipValidation(string assetPath)
        {
            if (assetPath.EndsWith(".cs"))
            {
                return true;
            }

            if (assetPath.EndsWith(".shader"))
            {
                return true;
            }

            if (assetPath.EndsWith(".compute"))
            {
                return true;
            }

            if (assetPath.Contains("/Scripts/"))
            {
                return true;
            }

            if (assetPath.Contains("/Editor/"))
            {
                return true;
            }

            if (assetPath.Contains("/ArtPipeline/Documentation/"))
            {
                return true;
            }

            return false;
        }

        private static void ValidateNamingConvention(string assetPath, string fileName, ValidationResult result)
        {
            if (!IsArtAsset(assetPath))
            {
                return;
            }

            bool hasValidPrefix = false;
            string usedPrefix = null;

            foreach (string prefix in Configuration.NamingConventions.PrefixRules.Keys)
            {
                if (fileName.StartsWith(prefix))
                {
                    hasValidPrefix = true;
                    usedPrefix = prefix;
                    break;
                }
            }

            if (!hasValidPrefix)
            {
                string expectedPrefixes = string.Join(", ", Configuration.NamingConventions.PrefixRules.Keys);
                result.AddError($"Invalid naming: '{fileName}' should start with one of: {expectedPrefixes}");

                string suggestedPrefix = SuggestPrefix(assetPath);
                if (!string.IsNullOrEmpty(suggestedPrefix))
                {
                    string suggestedName = suggestedPrefix + fileName;
                    result.AddSuggestion($"Suggested name: {suggestedName}");
                }
            }
            else
            {
                ValidatePrefixSpecificRules(assetPath, fileName, usedPrefix, result);
            }

            if (fileName.Contains(" "))
            {
                result.AddError("Asset names cannot contain spaces. Use PascalCase or underscores.");
            }

            if (fileName.Contains("__"))
            {
                result.AddWarning("Asset name contains consecutive underscores. Use single underscores only.");
            }

            var invalidChars = System.Text.RegularExpressions.Regex.Matches(fileName, @"[^a-zA-Z0-9_]");
            if (invalidChars.Count > 0)
            {
                result.AddError("Asset names can only contain letters, numbers, and underscores.");
            }

            if (fileName.Length > 64)
            {
                result.AddWarning("Asset name is very long. Consider using a shorter, more descriptive name.");
            }
        }

        private static void ValidatePrefixSpecificRules(string assetPath, string fileName, string prefix, ValidationResult result)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new System.ArgumentException($"'{nameof(assetPath)}' cannot be null or empty.", nameof(assetPath));
            }

            switch (prefix)
            {
                case "T_":
                    ValidateTextureSuffix(fileName, result);
                    break;
                case "AUD_SFX_":
                case "AUD_MUS_":
                case "AUD_VOX_":
                    ValidateAudioNaming(fileName, prefix, result);
                    break;
                case "PFV_":
                    ValidatePrefabVariantNaming(fileName, result);
                    break;
                default:
                    break;
            }
        }

        private static void ValidateTextureSuffix(string fileName, ValidationResult result)
        {
            string[] parts = fileName.Split('_');
            if (parts.Length >= 3)
            {
                string potentialSuffix = "_" + parts[^1];

                if (!Configuration.NamingConventions.IsValidTextureSuffix(potentialSuffix))
                {
                    string validSuffixes = string.Join(", ", Configuration.NamingConventions.TextureSuffixRules.Keys);
                    result.AddWarning($"Unusual texture suffix '{potentialSuffix}'. Standard suffixes: {validSuffixes}");

                    string suggestedSuffix = SuggestTextureSuffix(fileName);
                    if (!string.IsNullOrEmpty(suggestedSuffix))
                    {
                        result.AddSuggestion($"Consider using suffix: {suggestedSuffix}");
                    }
                }
            }
            else
            {
                result.AddWarning("Texture name should include a texture type suffix (e.g., _Albedo, _Normal)");
            }
        }

        private static void ValidateAudioNaming(string fileName, string prefix, ValidationResult result)
        {
            string remainingName = fileName[prefix.Length..];
            string[] parts = remainingName.Split('_');

            if (parts.Length < 2)
            {
                result.AddWarning("Audio files should include category and specific name (e.g., AUD_SFX_Weapon_RifleShot)");
            }

            if (prefix == "AUD_SFX_")
            {
                string[] validCategories = new[] { "Weapon", "Character", "UI", "Environment", "Vehicle" };
                if (parts.Length > 0 && !validCategories.Contains(parts[0]))
                {
                    result.AddSuggestion($"Consider using a standard category: {string.Join(", ", validCategories)}");
                }
            }
        }

        private static void ValidatePrefabVariantNaming(string fileName, ValidationResult result)
        {
            string basePrefabName = fileName.Replace("PFV_", "PF_");
            int lastUnderscore = basePrefabName.LastIndexOf('_');

            if (lastUnderscore is (-1) or < 4) // Minimum: PF_XX_V
            {
                result.AddError("Prefab variant name should follow pattern: PFV_BaseName_Variant");
            }
        }

        private static void ValidateFolderLocation(string assetPath, string fileName, ValidationResult result)
        {
            foreach (string prefix in Configuration.NamingConventions.PrefixRules.Keys)
            {
                if (fileName.StartsWith(prefix))
                {
                    string[] allowedFolders = Configuration.NamingConventions.GetAllowedFolders(prefix);
                    bool inCorrectFolder = allowedFolders.Any(folder => assetPath.Contains(folder));

                    if (!inCorrectFolder)
                    {
                        result.AddError($"Asset in wrong folder: '{fileName}' should be in: {string.Join(" or ", allowedFolders)}");

                        string suggestedFolder = allowedFolders.FirstOrDefault();
                        if (!string.IsNullOrEmpty(suggestedFolder))
                        {
                            result.AddSuggestion($"Move to: {suggestedFolder}");
                        }
                    }
                    break;
                }
            }
        }

        private static void ValidateFileSize(string assetPath, string extension, ValidationResult result)
        {
            long maxSize = Configuration.NamingConventions.GetMaxFileSize(extension);

            try
            {
                FileInfo fileInfo = new(assetPath);
                if (fileInfo.Exists && fileInfo.Length > maxSize)
                {
                    double sizeMB = fileInfo.Length / 1024.0 / 1024.0;
                    double maxSizeMB = maxSize / 1024.0 / 1024.0;
                    result.AddError($"File too large: {sizeMB:F1}MB exceeds limit of {maxSizeMB:F1}MB");

                    if (IsTextureFile(assetPath))
                    {
                        result.AddSuggestion("Consider reducing texture resolution or using more efficient compression");
                    }
                    else if (IsAudioFile(assetPath))
                    {
                        result.AddSuggestion("Consider using compressed audio format or reducing quality");
                    }
                    else if (IsModelFile(assetPath))
                    {
                        result.AddSuggestion("Consider optimizing mesh or using LODs");
                    }
                }
            }
            catch (System.Exception ex)
            {
                result.AddWarning($"Could not check file size: {ex.Message}");
            }
        }

        private static void ValidateImportSettings(string assetPath, ValidationResult result)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer is TextureImporter textureImporter)
            {
                ValidateTextureSettings(textureImporter, result);
            }
            else if (importer is ModelImporter modelImporter)
            {
                ValidateModelSettings(modelImporter, result);
            }
            else if (importer is AudioImporter audioImporter)
            {
                ValidateAudioSettings(audioImporter, result);
            }
        }

        private static void ValidateTextureSettings(TextureImporter importer, ValidationResult result)
        {
            if (importer.textureType == TextureImporterType.Sprite && !importer.sRGBTexture)
            {
                result.AddWarning("Sprite texture should have sRGB enabled for correct color space");
            }

            if (!importer.mipmapEnabled && importer.textureType != TextureImporterType.Sprite)
            {
                result.AddWarning("Consider enabling mipmaps for better performance at distance");
            }

            var platformSettings = importer.GetPlatformTextureSettings("Standalone");
            if (platformSettings.overridden && platformSettings.format == TextureImporterFormat.Automatic)
            {
                result.AddSuggestion("Consider specifying explicit texture compression format for better control");
            }

            if (importer.maxTextureSize > 4096)
            {
                result.AddWarning("Very large texture size. Consider if 4K+ resolution is necessary.");
            }
        }

        private static void ValidateModelSettings(ModelImporter importer, ValidationResult result)
        {
            if (importer.isReadable)
            {
                result.AddWarning("Mesh is readable - disable unless needed for runtime modification (performance impact)");
            }

            if (importer.meshCompression == ModelImporterMeshCompression.Off)
            {
                result.AddSuggestion("Consider enabling mesh compression to reduce build size");
            }

            if (importer.generateSecondaryUV)
            {
                result.AddSuggestion("Secondary UVs generated - ensure they are needed for lightmapping or other effects");
            }
        }

        private static void ValidateAudioSettings(AudioImporter importer, ValidationResult result)
        {
            var settings = importer.defaultSampleSettings;
            FileInfo fileInfo = new(importer.assetPath);

            if (settings.loadType == AudioClipLoadType.DecompressOnLoad &&
                settings.compressionFormat == AudioCompressionFormat.PCM &&
                !importer.forceToMono && fileInfo.Length > 1 * 1024 * 1024) // 1MB
            {
                result.AddWarning("Large PCM audio will decompress on load - consider using CompressedInMemory or forcing to mono");
            }

            if (fileInfo.Length > 2 * 1024 * 1024 && // 2MB
                settings.loadType != AudioClipLoadType.Streaming &&
                settings.loadType != AudioClipLoadType.CompressedInMemory)
            {
                result.AddWarning("Large audio file should use Streaming or CompressedInMemory load type");
            }

            var standaloneSettings = importer.GetOverrideSampleSettings("Standalone");

            if (standaloneSettings.loadType == AudioClipLoadType.Streaming && standaloneSettings.preloadAudioData)
            {
                result.AddWarning("Streaming audio should not preload data - this defeats streaming benefits");
            }
        }

        private static void ValidatePrefabReferences(string assetPath, ValidationResult result)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                result.AddError("Could not load prefab for reference validation");
                return;
            }

            var dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { prefab });
            List<string> missingRefs = new();

            foreach (var dependency in dependencies)
            {
                if (dependency == null)
                {
                    missingRefs.Add("Missing reference detected");
                    break;
                }
            }

            if (missingRefs.Count > 0)
            {
                result.AddError("Prefab contains missing references!");
            }

            List<Transform> nestedPrefabs = prefab.GetComponentsInChildren<Transform>(true)
                .Where(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject))
                .ToList();

            if (nestedPrefabs.Count > 5) // Arbitrary threshold
            {
                result.AddWarning("Prefab has many nested prefabs - consider simplifying hierarchy for performance");
            }
        }

        private static void ValidateTextureSpecifics(string assetPath, ValidationResult result)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
            {
                return;
            }

            if (!IsPowerOfTwo(texture.width) || !IsPowerOfTwo(texture.height))
            {
                result.AddWarning("Texture dimensions are not power of two - may cause compression issues");
            }

            if (assetPath.Contains("/UI/") && texture.width != texture.height)
            {
                result.AddSuggestion("UI textures are often square - consider using power-of-two square dimensions");
            }
        }

        private static void ValidateModelSpecifics(string assetPath, ValidationResult result)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (model == null)
            {
                return;
            }

            var meshFilters = model.GetComponentsInChildren<MeshFilter>();
            var skinnedMeshes = model.GetComponentsInChildren<SkinnedMeshRenderer>();

            int totalTriangles = 0;
            foreach (var filter in meshFilters)
            {
                if (filter.sharedMesh != null)
                {
                    totalTriangles += filter.sharedMesh.triangles.Length / 3;
                }
            }
            foreach (var skinned in skinnedMeshes)
            {
                if (skinned.sharedMesh != null)
                {
                    totalTriangles += skinned.sharedMesh.triangles.Length / 3;
                }
            }

            if (totalTriangles > 100000)
            {
                result.AddError($"Very high triangle count: {totalTriangles}. Consider heavy optimization.");
            }
            else if (totalTriangles > 50000)
            {
                result.AddWarning($"High triangle count: {totalTriangles}. Consider optimization.");
            }
            else if (totalTriangles < 100)
            {
                result.AddSuggestion($"Very low triangle count: {totalTriangles}. May not need LODs.");
            }

            var lodGroup = model.GetComponentInChildren<LODGroup>();
            if (lodGroup == null && totalTriangles > 2000)
            {
                result.AddSuggestion("Consider adding LOD Group for better performance");
            }
        }

        private static void ValidateAudioSpecifics(string assetPath, ValidationResult result)
        {
            var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (audioClip == null)
            {
                return;
            }

            if (audioClip.length > 30 && assetPath.Contains("/SFX/"))
            {
                result.AddWarning("SFX audio clip is very long (>30s). Consider if this should be music or ambient.");
            }

            if (audioClip.length < 0.1 && assetPath.Contains("/Music/"))
            {
                result.AddError("Music audio clip is very short (<0.1s). This might be an error.");
            }

            if (audioClip.frequency < 44100)
            {
                result.AddSuggestion("Audio sample rate is below 44.1kHz. Consider using higher quality for better sound.");
            }
        }

        #region Helper Methods

        public static bool IsArtAsset(string assetPath) =>
                   assetPath.Contains("/Art/") ||
                   assetPath.Contains("/Audio/") ||
                   assetPath.Contains("/Materials/") ||
                   assetPath.Contains("/Prefabs/") ||
                   IsTextureFile(assetPath) ||
                   IsModelFile(assetPath) ||
                   IsAudioFile(assetPath);

        private static bool IsTextureFile(string assetPath)
        {
            string ext = Path.GetExtension(assetPath).ToLower();
            return ext is ".png" or ".jpg" or ".jpeg" or ".tga" or ".psd" or ".exr";
        }

        private static bool IsModelFile(string assetPath)
        {
            string ext = Path.GetExtension(assetPath).ToLower();
            return ext is ".fbx" or ".obj" or ".blend" or ".max" or ".ma" or ".mb";
        }

        private static bool IsAudioFile(string assetPath)
        {
            string ext = Path.GetExtension(assetPath).ToLower();
            return ext is ".wav" or ".mp3" or ".ogg" or ".aiff";
        }

        private static bool IsPowerOfTwo(int x) => (x & (x - 1)) == 0;

        private static string SuggestPrefix(string assetPath)
        {
            if (assetPath.Contains("/Textures/"))
            {
                return "T_";
            }

            if (assetPath.Contains("/Models/"))
            {
                if (assetPath.Contains("/Characters/"))
                {
                    return "SK_";
                }

                return "SM_";
            }
            if (assetPath.Contains("/Materials/"))
            {
                return "M_";
            }

            if (assetPath.Contains("/Prefabs/"))
            {
                return "PF_";
            }

            if (assetPath.Contains("/Audio/SFX/"))
            {
                return "AUD_SFX_";
            }

            if (assetPath.Contains("/Audio/Music/"))
            {
                return "AUD_MUS_";
            }

            if (assetPath.Contains("/Audio/Voice/"))
            {
                return "AUD_VOX_";
            }

            if (assetPath.Contains("/Animations/"))
            {
                return "A_";
            }

            if (assetPath.Contains("/UI/Fonts/"))
            {
                return "F_";
            }

            if (assetPath.Contains("/Configs/"))
            {
                return "SO_";
            }

            return null;
        }

        private static string SuggestTextureSuffix(string fileName)
        {
            string lowerName = fileName.ToLower();

            if (lowerName.Contains("normal") || lowerName.Contains("nrm") || lowerName.Contains("norm"))
            {
                return "_Normal";
            }

            if (lowerName.Contains("albedo") || lowerName.Contains("diffuse") || lowerName.Contains("base") || lowerName.Contains("color"))
            {
                return "_Albedo";
            }

            if (lowerName.Contains("metal") || lowerName.Contains("metallic"))
            {
                return "_Metal";
            }

            if (lowerName.Contains("rough") || lowerName.Contains("gloss"))
            {
                return "_Roughness";
            }

            if (lowerName.Contains("ao") || lowerName.Contains("occlusion"))
            {
                return "_AO";
            }

            if (lowerName.Contains("emiss") || lowerName.Contains("glow"))
            {
                return "_Emissive";
            }

            if (lowerName.Contains("mask"))
            {
                return "_Mask";
            }

            if (lowerName.Contains("detail"))
            {
                return "_Detail";
            }

            return "_Albedo";
        }

        #endregion
    }
}
