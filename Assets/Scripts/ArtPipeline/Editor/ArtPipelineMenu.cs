using ArtPipeline.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Central menu for art pipeline tools
    /// </summary>
    public static class ArtPipelineMenu
    {
        [MenuItem("Tools/Art Pipeline/Validate All Assets", false, 1)]
        private static void ValidateAllAssets() => AssetValidator.ValidateAllAssets();

        [MenuItem("Tools/Art Pipeline/Validate Selected Assets", false, 2)]
        private static void ValidateSelectedAssets() => AssetValidator.ValidateSelectedAssets();

        [MenuItem("Assets/Validate Asset", false, 20)]
        private static void ValidateAssetFromContext() => AssetValidator.ValidateAssetFromContext();

        [MenuItem("Tools/Art Pipeline/Generate LODs for Selected", false, 20)]
        private static void GenerateLODs() => LODGenerator.GenerateLODsForSelected();

        [MenuItem("Tools/Art Pipeline/Generate LODs for All in Folder", false, 21)]
        private static void GenerateLODsForFolder() => LODGenerator.GenerateLODsForFolder();

        [MenuItem("Tools/Art Pipeline/Analyze Mesh Complexity", false, 22)]
        private static void AnalyzeMeshComplexity() => LODGenerator.AnalyzeSelectedMesh();

        [MenuItem("Tools/Art Pipeline/Setup Addressables Groups", false, 40)]
        private static void SetupAddressables() => AddressablesSetup.SetupAddressablesGroups();

        [MenuItem("Tools/Art Pipeline/Mark Selected as Addressable", false, 41)]
        private static void MarkAsAddressable() => AddressablesSetup.MarkSelectedAsAddressable();

        [MenuItem("Tools/Art Pipeline/Generate Naming Documentation", false, 90)]
        private static void GenerateDocumentation() => DocumentationGenerator.GenerateNamingDocumentation();

        [MenuItem("Tools/Art Pipeline/Open Naming Documentation", false, 91)]
        private static void OpenDocumentation() => DocumentationGenerator.OpenNamingDocumentation();

        [MenuItem("Tools/Art Pipeline/Open Artists README", false, 92)]
        private static void OpenReadme()
        {
            string readmePath = "Assets/ArtPipeline/Documentation/Artists_README.md";
            if (System.IO.File.Exists(readmePath))
            {
                _ = AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(readmePath));
            }
            else
            {
                _ = EditorUtility.DisplayDialog("README", "Artists_README.md not found in ArtPipeline/Documentation folder", "OK");
            }
        }
    }
}
