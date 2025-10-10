using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Automatically validates assets on import
    /// </summary>
    public class AssetImportValidator : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool hasValidationErrors = false;
            List<string> failedAssets = new();

            foreach (string assetPath in importedAssets)
            {
                if (!AssetValidator.IsArtAsset(assetPath))
                {
                    continue;
                }

                var result = AssetValidator.ValidateAsset(assetPath);

                if (!result.IsValid)
                {
                    Debug.LogError($"❌ Import Validation Failed for {assetPath}:\n{result}");
                    hasValidationErrors = true;
                    failedAssets.Add(Path.GetFileName(assetPath));

                    TryAutoFixAsset(assetPath, result);
                }
                else if (result.Warnings.Count > 0 || result.Suggestions.Count > 0)
                {
                    Debug.LogWarning($"⚠ Import Validation Warnings for {assetPath}:\n{result}");
                }
            }

            if (hasValidationErrors && !Application.isBatchMode)
            {
                string message = $"Validation failed for {failedAssets.Count} assets:\n{string.Join("\n", failedAssets.Take(5))}";
                if (failedAssets.Count > 5)
                {
                    message += $"\n... and {failedAssets.Count - 5} more";
                }

                _ = EditorUtility.DisplayDialog("Import Validation Failed", message, "OK");
            }
        }

        private static void TryAutoFixAsset(string assetPath, ValidationResult result)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new System.ArgumentException($"'{nameof(assetPath)}' cannot be null or empty.", nameof(assetPath));
            }

            if (result is null)
            {
                throw new System.ArgumentNullException(nameof(result));
            }

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                return;
            }

            bool shouldReimport = false;

            if (importer is TextureImporter textureImporter)
            {
                if (assetPath.ToLower().Contains("_normal"))
                {
                    AssetImportPresets.ApplyNormalMapSettings(textureImporter);
                    shouldReimport = true;
                }
                else
                {
                    AssetImportPresets.ApplyTextureSettings(textureImporter);
                    shouldReimport = true;
                }
            }
            else if (importer is AudioImporter audioImporter)
            {
                if (assetPath.Contains("/SFX/"))
                {
                    AssetImportPresets.ApplySFXSettings(audioImporter);
                    shouldReimport = true;
                }
                else if (assetPath.Contains("/Music/"))
                {
                    AssetImportPresets.ApplyMusicSettings(audioImporter);
                    shouldReimport = true;
                }
                else if (assetPath.Contains("/Voice/"))
                {
                    AssetImportPresets.ApplyVoiceSettings(audioImporter);
                    shouldReimport = true;
                }
            }

            if (shouldReimport)
            {
                importer.SaveAndReimport();
                Debug.Log($"🛠️ Auto-fixed import settings for: {Path.GetFileName(assetPath)}");
            }
        }
    }
}
