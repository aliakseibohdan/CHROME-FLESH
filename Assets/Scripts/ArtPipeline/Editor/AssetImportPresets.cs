using UnityEngine;
using UnityEditor;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Defines asset import presets and validation rules
    /// </summary>
    public static class AssetImportPresets
    {
        #region Texture Settings
        public static void ApplyTextureSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = true;
            importer.mipmapFilter = TextureImporterMipFilter.BoxFilter;
            importer.compressionQuality = 100;

            var androidSettings = importer.GetPlatformTextureSettings("Android");
            androidSettings.overridden = true;
            androidSettings.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(androidSettings);

            var standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
            standaloneSettings.overridden = true;
            standaloneSettings.format = TextureImporterFormat.BC7;
            importer.SetPlatformTextureSettings(standaloneSettings);
        }

        public static void ApplyNormalMapSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.NormalMap;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = true;

            var androidSettings = importer.GetPlatformTextureSettings("Android");
            androidSettings.overridden = true;
            androidSettings.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(androidSettings);

            var standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
            standaloneSettings.overridden = true;
            standaloneSettings.format = TextureImporterFormat.BC5;
            importer.SetPlatformTextureSettings(standaloneSettings);
        }
        #endregion

        #region Model Settings
        public static void ApplyStaticMeshSettings(ModelImporter importer)
        {
            importer.globalScale = 1.0f;
            importer.useFileScale = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.isReadable = false;
            importer.optimizeMeshPolygons = true;
            importer.importBlendShapes = false;
            importer.generateSecondaryUV = true;
        }

        public static void ApplyCharacterMeshSettings(ModelImporter importer)
        {
            importer.globalScale = 1.0f;
            importer.useFileScale = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.isReadable = false;
            importer.optimizeMeshPolygons = true;
            importer.importBlendShapes = true;
            importer.generateSecondaryUV = true;
            importer.animationType = ModelImporterAnimationType.Human;
        }
        #endregion

        #region Audio Settings
        public static void ApplySFXSettings(AudioImporter importer)
        {
            importer.forceToMono = true;
            importer.loadInBackground = false;

            var defaultSettings = importer.defaultSampleSettings;
            defaultSettings.loadType = AudioClipLoadType.CompressedInMemory;
            defaultSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            defaultSettings.quality = 0.7f;
            defaultSettings.preloadAudioData = true;

            importer.defaultSampleSettings = defaultSettings;

            // Apply same settings to all platforms for SFX
            var platformSettings = importer.GetOverrideSampleSettings("Standalone");
            platformSettings.loadType = AudioClipLoadType.CompressedInMemory;
            platformSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            platformSettings.quality = 0.7f;
            platformSettings.preloadAudioData = true;
            _ = importer.SetOverrideSampleSettings("Standalone", platformSettings);

            var androidSettings = importer.GetOverrideSampleSettings("Android");
            androidSettings.loadType = AudioClipLoadType.CompressedInMemory;
            androidSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            androidSettings.quality = 0.7f;
            androidSettings.preloadAudioData = true;
            _ = importer.SetOverrideSampleSettings("Android", androidSettings);
        }

        public static void ApplyMusicSettings(AudioImporter importer)
        {
            importer.forceToMono = false;
            importer.loadInBackground = true;

            var defaultSettings = importer.defaultSampleSettings;
            defaultSettings.loadType = AudioClipLoadType.Streaming;
            defaultSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            defaultSettings.quality = 0.9f;
            defaultSettings.preloadAudioData = false; // Don't preload for streaming

            importer.defaultSampleSettings = defaultSettings;

            // Apply same settings to all platforms for Music
            var platformSettings = importer.GetOverrideSampleSettings("Standalone");
            platformSettings.loadType = AudioClipLoadType.Streaming;
            platformSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            platformSettings.quality = 0.9f;
            platformSettings.preloadAudioData = false;
            _ = importer.SetOverrideSampleSettings("Standalone", platformSettings);

            var androidSettings = importer.GetOverrideSampleSettings("Android");
            androidSettings.loadType = AudioClipLoadType.Streaming;
            androidSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            androidSettings.quality = 0.9f;
            androidSettings.preloadAudioData = false;
            _ = importer.SetOverrideSampleSettings("Android", androidSettings);
        }

        public static void ApplyVoiceSettings(AudioImporter importer)
        {
            importer.forceToMono = true;
            importer.loadInBackground = false;

            var defaultSettings = importer.defaultSampleSettings;
            defaultSettings.loadType = AudioClipLoadType.DecompressOnLoad;
            defaultSettings.compressionFormat = AudioCompressionFormat.PCM;
            defaultSettings.preloadAudioData = true;

            importer.defaultSampleSettings = defaultSettings;

            // Voice needs low latency, so use PCM decompress on load
            var platformSettings = importer.GetOverrideSampleSettings("Standalone");
            platformSettings.loadType = AudioClipLoadType.DecompressOnLoad;
            platformSettings.compressionFormat = AudioCompressionFormat.PCM;
            platformSettings.preloadAudioData = true;
            _ = importer.SetOverrideSampleSettings("Standalone", platformSettings);

            var androidSettings = importer.GetOverrideSampleSettings("Android");
            androidSettings.loadType = AudioClipLoadType.CompressedInMemory;
            androidSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            androidSettings.quality = 0.8f;
            androidSettings.preloadAudioData = true;
            _ = importer.SetOverrideSampleSettings("Android", androidSettings);
        }
        #endregion
    }
}
