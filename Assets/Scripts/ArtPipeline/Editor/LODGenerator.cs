using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Automated LOD generation with mesh simplification
    /// </summary>
    public static class LODGenerator
    {
        private static readonly float[] _lodPercentages = { 1.0f, 0.5f, 0.25f, 0.1f }; // LOD0 to LOD3
        private static readonly float[] _lodScreenRelativeHeights = { 0.6f, 0.3f, 0.15f, 0.05f };

        [MenuItem("Tools/Art Pipeline/Generate LODs for Selected", false, 20)]
        public static void GenerateLODsForSelected()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                _ = EditorUtility.DisplayDialog("LOD Generator", "Please select a GameObject", "OK");
                return;
            }

            if (!selected.TryGetComponent<Renderer>(out _))
            {
                _ = EditorUtility.DisplayDialog("LOD Generator", "Selected object has no Renderer", "OK");
                return;
            }

            GenerateLODs(selected);
        }

        [MenuItem("Tools/Art Pipeline/Generate LODs for All in Folder", false, 21)]
        public static void GenerateLODsForFolder()
        {
            var selectedFolder = Selection.activeObject;
            if (selectedFolder == null)
            {
                _ = EditorUtility.DisplayDialog("LOD Generator", "Please select a folder", "OK");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(selectedFolder);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                _ = EditorUtility.DisplayDialog("LOD Generator", "Selected object is not a folder", "OK");
                return;
            }

            string[] assetGUIDs = AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });
            int processed = 0;
            foreach (var (assetPath, prefab) in from guid in assetGUIDs
                                                let assetPath = AssetDatabase.GUIDToAssetPath(guid)
                                                let prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)
                                                where prefab != null && PrefabUtility.IsPartOfAnyPrefab(prefab)
                                                select (assetPath, prefab))
            {
                GenerateLODsForPrefab(prefab, assetPath);
                processed++;
            }

            Debug.Log($"Generated LODs for {processed} prefabs in folder: {folderPath}");
        }

        private static void GenerateLODsForPrefab(GameObject prefab, string assetPath)
        {
            if (prefab.TryGetComponent<LODGroup>(out _))
            {
                Debug.Log($"Skipping {prefab.name} - already has LOD Group");
                return;
            }

            if (string.IsNullOrEmpty(assetPath))
            {
                throw new System.ArgumentException($"'{nameof(assetPath)}' cannot be null or empty.", nameof(assetPath));
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            try
            {
                GenerateLODs(instance);

                PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
                Debug.Log($"Generated LODs for prefab: {prefab.name}");
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        public static void GenerateLODs(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogError("No renderers found in selected object or its children");
                return;
            }

            if (!target.TryGetComponent<LODGroup>(out var lodGroup))
            {
                lodGroup = target.AddComponent<LODGroup>();
            }

            List<LOD> lods = new();
            List<Renderer[]> lodRenderers = new();

            for (int i = 0; i < _lodPercentages.Length; i++)
            {
                float lodPercentage = _lodPercentages[i];
                float screenRelativeHeight = _lodScreenRelativeHeights[i];

                List<Renderer> lodLevelRenderers = new();

                foreach (var originalRenderer in renderers)
                {
                    if (originalRenderer == null)
                    {
                        continue;
                    }

                    var lodRenderer = CreateLODRenderer(originalRenderer, lodPercentage, i);
                    if (lodRenderer != null)
                    {
                        lodLevelRenderers.Add(lodRenderer);
                    }
                }

                if (lodLevelRenderers.Count > 0)
                {
                    lodRenderers.Add(lodLevelRenderers.ToArray());
                    lods.Add(new LOD(screenRelativeHeight, lodLevelRenderers.ToArray()));
                }
            }

            if (lods.Count > 0)
            {
                lodGroup.SetLODs(lods.ToArray());
                lodGroup.RecalculateBounds();

                lodGroup.animateCrossFading = false;
                lodGroup.fadeMode = LODFadeMode.None;

                Debug.Log($"Generated {lods.Count} LOD levels for {target.name}");

                EditorUtility.SetDirty(target);
                if (PrefabUtility.IsPartOfAnyPrefab(target))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }
            }
            else
            {
                Debug.LogError("Failed to generate any LOD levels");
                if (lodGroup != null)
                {
                    Object.DestroyImmediate(lodGroup);
                }
            }
        }

        private static Renderer CreateLODRenderer(Renderer originalRenderer, float quality, int lodLevel)
        {
            if (originalRenderer is MeshRenderer meshRenderer)
            {
                return CreateMeshLODRenderer(meshRenderer, quality, lodLevel);
            }
            else if (originalRenderer is SkinnedMeshRenderer skinnedRenderer)
            {
                return CreateSkinnedLODRenderer(skinnedRenderer, quality, lodLevel);
            }

            return null;
        }

        private static Renderer CreateMeshLODRenderer(MeshRenderer originalRenderer, float quality, int lodLevel)
        {
            var originalFilter = originalRenderer.GetComponent<MeshFilter>();
            if (originalFilter == null || originalFilter.sharedMesh == null)
            {
                return null;
            }

            var originalMesh = originalFilter.sharedMesh;
            var originalTransform = originalRenderer.transform;
            var parent = originalTransform.parent;

            GameObject lodObject = new($"{originalRenderer.name}_LOD{lodLevel}");
            lodObject.transform.SetParent(parent);
            lodObject.transform.SetLocalPositionAndRotation(originalTransform.localPosition, originalTransform.localRotation);
            lodObject.transform.localScale = originalTransform.localScale;

            var lodRenderer = lodObject.AddComponent<MeshRenderer>();
            var lodFilter = lodObject.AddComponent<MeshFilter>();

            lodRenderer.sharedMaterials = originalRenderer.sharedMaterials;

            lodRenderer.shadowCastingMode = originalRenderer.shadowCastingMode;
            lodRenderer.receiveShadows = originalRenderer.receiveShadows;
            lodRenderer.lightProbeUsage = originalRenderer.lightProbeUsage;
            lodRenderer.reflectionProbeUsage = originalRenderer.reflectionProbeUsage;

            var simplifiedMesh = SimplifyMesh(originalMesh, quality, lodLevel);
            lodFilter.sharedMesh = simplifiedMesh;

            // Disable by default (will be controlled by LOD Group)
            lodRenderer.enabled = false;

            return lodRenderer;
        }

        private static Renderer CreateSkinnedLODRenderer(SkinnedMeshRenderer originalRenderer, float quality, int lodLevel)
        {
            if (originalRenderer.sharedMesh == null)
            {
                return null;
            }

            var originalTransform = originalRenderer.transform;
            var parent = originalTransform.parent;

            GameObject lodObject = new($"{originalRenderer.name}_LOD{lodLevel}");
            lodObject.transform.SetParent(parent);
            lodObject.transform.SetLocalPositionAndRotation(originalTransform.localPosition, originalTransform.localRotation);
            lodObject.transform.localScale = originalTransform.localScale;

            var lodRenderer = lodObject.AddComponent<SkinnedMeshRenderer>();

            lodRenderer.sharedMaterials = originalRenderer.sharedMaterials;
            lodRenderer.sharedMesh = SimplifyMesh(originalRenderer.sharedMesh, quality, lodLevel);
            lodRenderer.bones = originalRenderer.bones;
            lodRenderer.rootBone = originalRenderer.rootBone;
            lodRenderer.quality = originalRenderer.quality;
            lodRenderer.updateWhenOffscreen = originalRenderer.updateWhenOffscreen;

            if (originalRenderer.sharedMesh.blendShapeCount > 0)
            {
                // Blend shapes are preserved in the simplification process
            }

            lodRenderer.enabled = false;

            return lodRenderer;
        }

        private static Mesh SimplifyMesh(Mesh originalMesh, float quality, int lodLevel)
        {
            var simplifiedMesh = Object.Instantiate(originalMesh);
            simplifiedMesh.name = $"{originalMesh.name}_LOD{lodLevel}";

            // For LOD0, we might not need to simplify (quality = 1.0)
            if (quality >= 0.99f)
            {
                return simplifiedMesh;
            }

            // Use Unity's built-in mesh compression as a form of simplification

            _ = originalMesh.vertices;
            int[] triangles = originalMesh.triangles;

            // Simple triangle reduction (this is a basic approach - for production, use a proper simplification algorithm)
            if (quality < 1.0f && triangles.Length > 0)
            {
                _ = Mathf.Max((int)(triangles.Length * quality / 3), 1) * 3;

                // This is a placeholder for actual mesh simplification
                // TODO:
                // 1. Unity's Mesh.CombineMeshes with optimization
                // 2. A third-party mesh simplification library, like Mesh Simplifier or Simplygon
                // 3. Custom simplification algorithm

                // For now, we'll use a naive approach of removing every other triangle
                // This is NOT production-quality but demonstrates the concept
                int[] simplifiedTriangles = ReduceTrianglesNaive(triangles, quality);
                simplifiedMesh.triangles = simplifiedTriangles;

                simplifiedMesh.RecalculateNormals();
                simplifiedMesh.RecalculateBounds();
                simplifiedMesh.RecalculateTangents();
            }

            return simplifiedMesh;
        }

        /// <summary>
        /// Naive triangle reduction - removes triangles in a pattern
        /// This is for demonstration only. We will use a proper simplification algorithm in production.
        /// </summary>
        private static int[] ReduceTrianglesNaive(int[] triangles, float quality)
        {
            if (quality >= 0.75f)
            {
                // Remove 25% of triangles
                return RemoveEveryNthTriangle(triangles, 4);
            }
            else if (quality >= 0.5f)
            {
                // Remove 50% of triangles
                return RemoveEveryNthTriangle(triangles, 2);
            }
            else if (quality >= 0.25f)
            {
                // Remove 75% of triangles
                int[] halfRemoved = RemoveEveryNthTriangle(triangles, 2);
                return RemoveEveryNthTriangle(halfRemoved, 2);
            }
            else
            {
                // Keep only 10% of triangles
                return RemoveEveryNthTriangle(triangles, 10);
            }
        }

        private static int[] RemoveEveryNthTriangle(int[] triangles, int n)
        {
            List<int> result = new();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Keep the triangle if it's not the nth one
                if (i / 3 % n != 0)
                {
                    result.Add(triangles[i]);
                    result.Add(triangles[i + 1]);
                    result.Add(triangles[i + 2]);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Validates if a mesh is suitable for LOD generation
        /// </summary>
        public static bool IsMeshSuitableForLOD(Mesh mesh)
        {
            if (mesh == null)
            {
                return false;
            }

            if (mesh.triangles.Length < 100)
            {
                Debug.LogWarning($"Mesh {mesh.name} has very low triangle count ({mesh.triangles.Length / 3}), may not need LODs");
                return false;
            }

            if (!mesh.isReadable)
            {
                Debug.LogWarning($"Mesh {mesh.name} is not readable. Enable 'Read/Write' in import settings for LOD generation");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates recommended LOD levels based on mesh complexity
        /// </summary>
        public static int CalculateRecommendedLODCount(Mesh mesh)
        {
            if (mesh == null)
            {
                return 0;
            }

            int triangleCount = mesh.triangles.Length / 3;

            if (triangleCount < 500)
            {
                return 1;    // Very simple - no LODs needed
            }

            if (triangleCount < 2000)
            {
                return 2;    // Simple - 2 LODs
            }

            if (triangleCount < 10000)
            {
                return 3;    // Medium - 3 LODs
            }

            return 4;        // Complex - 4 LODs
        }

        [MenuItem("Tools/Art Pipeline/Analyze Mesh Complexity", false, 22)]
        public static void AnalyzeSelectedMesh()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                return;
            }

            var renderers = selected.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Mesh mesh = null;
                if (renderer is MeshRenderer meshRenderer)
                {
                    var filter = meshRenderer.GetComponent<MeshFilter>();
                    mesh = filter != null ? filter.sharedMesh : null;
                }
                else if (renderer is SkinnedMeshRenderer skinnedRenderer)
                {
                    mesh = skinnedRenderer.sharedMesh;
                }

                if (mesh != null)
                {
                    int triangles = mesh.triangles.Length / 3;
                    int vertices = mesh.vertexCount;
                    int recommendedLODs = CalculateRecommendedLODCount(mesh);

                    Debug.Log($"{renderer.name}: {triangles} triangles, {vertices} vertices - Recommended LODs: {recommendedLODs}");
                }
            }
        }
    }
}
