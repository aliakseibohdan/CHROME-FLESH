using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Automatically validates assets on import
    /// </summary>
    public class AssetImportValidator : AssetPostprocessor
    {
        private static readonly HashSet<string> _processedAssetsInThisBatch = new();
        private static bool _isProcessingBatch = false;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (_isProcessingBatch)
            {
                return;
            }

            _isProcessingBatch = true;
            _processedAssetsInThisBatch.Clear();

            try
            {
                bool hasValidationErrors = false;
                List<string> failedAssets = new();

                foreach (string assetPath in importedAssets)
                {
                    if (_processedAssetsInThisBatch.Contains(assetPath))
                    {
                        continue;
                    }

                    if (!IsArtAsset(assetPath))
                    {
                        continue;
                    }

                    _ = _processedAssetsInThisBatch.Add(assetPath);

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

                if (hasValidationErrors && !Application.isBatchMode && failedAssets.Count > 0)
                {
                    // Use delay call to avoid showing dialog during asset import
                    EditorApplication.delayCall += () =>
                    {
                        string message = $"Validation failed for {failedAssets.Count} assets:\n{string.Join("\n", failedAssets.Take(5))}";
                        if (failedAssets.Count > 5)
                        {
                            message += $"\n... and {failedAssets.Count - 5} more";
                        }

                        _ = EditorUtility.DisplayDialog("Import Validation Failed", message, "OK");
                    };
                }
            }
            finally
            {
                _isProcessingBatch = false;
            }
        }

        private static void TryAutoFixAsset(string assetPath, ValidationResult result)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new ArgumentException($"'{nameof(assetPath)}' cannot be null or empty.", nameof(assetPath));
            }

            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                return;
            }

            bool shouldReimport = false;

            if (importer is TextureImporter textureImporter)
            {
                if (assetPath.ToLower().Contains("_normal") && textureImporter.textureType != TextureImporterType.NormalMap)
                {
                    AssetImportPresets.ApplyNormalMapSettings(textureImporter);
                    shouldReimport = true;
                    Debug.Log($"🛠️ Auto-fixed normal map settings for: {Path.GetFileName(assetPath)}");
                }
            }
            else if (importer is AudioImporter audioImporter)
            {
                var settings = audioImporter.defaultSampleSettings;
                bool needsFix = false;

                if (assetPath.Contains("/SFX/") && settings.loadType != AudioClipLoadType.CompressedInMemory)
                {
                    AssetImportPresets.ApplySFXSettings(audioImporter);
                    needsFix = true;
                }
                else if (assetPath.Contains("/Music/") && settings.loadType != AudioClipLoadType.Streaming)
                {
                    AssetImportPresets.ApplyMusicSettings(audioImporter);
                    needsFix = true;
                }
                else if (assetPath.Contains("/Voice/") && settings.loadType == AudioClipLoadType.Streaming)
                {
                    AssetImportPresets.ApplyVoiceSettings(audioImporter);
                    needsFix = true;
                }

                if (needsFix)
                {
                    shouldReimport = true;
                    Debug.Log($"🛠️ Auto-fixed audio settings for: {Path.GetFileName(assetPath)}");
                }
            }

            if (shouldReimport)
            {
                _ = _processedAssetsInThisBatch.Add(assetPath);

                // Save and reimport - this will trigger OnPostprocessAllAssets again
                // but _isProcessingBatch and _processedAssetsInThisBatch will prevent loops
                importer.SaveAndReimport();
            }
        }

        private static bool IsArtAsset(string assetPath) =>
                   assetPath.Contains("/Art/") ||
                   assetPath.Contains("/Audio/") ||
                   assetPath.Contains("/Materials/") ||
                   assetPath.Contains("/Prefabs/") ||
                   assetPath.EndsWith(".fbx") ||
                   assetPath.EndsWith(".png") ||
                   assetPath.EndsWith(".jpg") ||
                   assetPath.EndsWith(".tga") ||
                   assetPath.EndsWith(".wav") ||
                   assetPath.EndsWith(".mp3") ||
                   assetPath.EndsWith(".controller");
    }
}
