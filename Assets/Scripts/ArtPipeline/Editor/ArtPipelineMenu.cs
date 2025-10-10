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
        public static void ValidateAllAssets() => AssetValidator.ValidateAllAssets();

        [MenuItem("Tools/Art Pipeline/Validate Selected Assets", false, 2)]
        public static void ValidateSelectedAssets() => AssetValidator.ValidateSelectedAssets();

        [MenuItem("Tools/Art Pipeline/Generate LODs for Selected", false, 20)]
        public static void GenerateLODs() => LODGenerator.GenerateLODsForSelected();

        [MenuItem("Tools/Art Pipeline/Setup Addressables", false, 40)]
        public static void SetupAddressables() => AddressablesSetup.SetupAddressablesGroups();

        [MenuItem("Tools/Art Pipeline/Mark Selected as Addressable", false, 41)]
        public static void MarkAsAddressable() => AddressablesSetup.MarkSelectedAsAddressable();

        [MenuItem("Tools/Art Pipeline/Open Artists README", false, 100)]
        public static void OpenReadme()
        {
            string readmePath = "Assets/Artists_README.md";
            if (System.IO.File.Exists(readmePath))
            {
                _ = AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(readmePath));
            }
            else
            {
                _ = EditorUtility.DisplayDialog("README", "Artists_README.md not found in Assets folder", "OK");
            }
        }

        [MenuItem("Tools/Art Pipeline/Generate Naming Documentation", false, 90)]
        public static void GenerateDocumentation() => DocumentationGenerator.GenerateNamingDocumentation();

        [MenuItem("Tools/Art Pipeline/Open Naming Documentation", false, 91)]
        public static void OpenDocumentation() => DocumentationGenerator.OpenNamingDocumentation();
    }
}
