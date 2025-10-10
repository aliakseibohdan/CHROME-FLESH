using UnityEngine;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Configurable LOD settings for different asset types
    /// </summary>
    [CreateAssetMenu(fileName = "LODSettings", menuName = "Art Pipeline/LOD Settings")]
    public class LODSettings : ScriptableObject
    {
        [Header("Character LOD Settings")]
        [InspectorName("Character LODs")]
        public LODProfile characterLODs = new()
        {
            levels = 4,
            screenRelativeHeights = new[] { 0.6f, 0.3f, 0.15f, 0.05f },
            qualityPercentages = new[] { 1.0f, 0.5f, 0.25f, 0.1f }
        };

        [Header("Environment LOD Settings")]
        [InspectorName("Environment LODs")]
        public LODProfile environmentLODs = new()
        {
            levels = 3,
            screenRelativeHeights = new[] { 0.5f, 0.2f, 0.05f },
            qualityPercentages = new[] { 1.0f, 0.3f, 0.1f }
        };

        [Header("Weapon LOD Settings")]
        [InspectorName("Weapon LODs")]
        public LODProfile weaponLODs = new()
        {
            levels = 2,
            screenRelativeHeights = new[] { 0.4f, 0.1f },
            qualityPercentages = new[] { 1.0f, 0.4f }
        };

        [Header("General Settings")]
        [InspectorName("Generate Colliders For LODs")]
        [Tooltip("Should colliders be generated for each LOD level?")]
        public bool generateCollidersForLODs = false;

        [InspectorName("Preserve UVs")]
        [Tooltip("Maintain UV coordinates during mesh simplification")]
        public bool preserveUVs = true;

        [InspectorName("Preserve Normals")]
        [Tooltip("Maintain normal maps during mesh simplification")]
        public bool preserveNormals = true;

        [InspectorName("Maximum Simplification Error")]
        [Tooltip("Higher values allow more aggressive simplification but may cause visual artifacts")]
        [Range(0.001f, 0.1f)]
        public float maximumSimplificationError = 0.01f;

        [InspectorName("Enable LOD Cross-fading")]
        [Tooltip("Smooth transitions between LOD levels")]
        public bool enableCrossFading = false;

        [InspectorName("Cross-fade Transition Time")]
        [Tooltip("Duration of LOD cross-fade transitions in seconds")]
        [Range(0.1f, 2.0f)]
        public float crossFadeTransitionTime = 0.5f;
    }

    [System.Serializable]
    public struct LODProfile
    {
        [Range(1, 4)]
        [Tooltip("Number of LOD levels to generate")]
        public int levels;

        [InspectorName("Screen Relative Heights")]
        [Tooltip("Screen size thresholds for each LOD level (0-1)")]
        public float[] screenRelativeHeights;

        [InspectorName("Quality Percentages")]
        [Tooltip("Mesh quality for each LOD level (0-1)")]
        public float[] qualityPercentages;
    }
}
