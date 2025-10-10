using System.IO;
using ArtPipeline.Editor.Configuration;
using UnityEditor;
using UnityEngine;

namespace ArtPipeline.Editor.Tools
{
    /// <summary>
    /// Generates naming convention documentation from centralized rules
    /// </summary>
    public static class DocumentationGenerator
    {
        [MenuItem("Tools/Art Pipeline/Generate Naming Documentation")]
        public static void GenerateNamingDocumentation()
        {
            string docsPath = "Assets/Scripts/ArtPipeline/Documentation";

            if (!Directory.Exists(docsPath))
            {
                _ = Directory.CreateDirectory(docsPath);
            }

            string markdownContent = NamingConventions.GenerateMarkdownDocumentation();

            string filePath = Path.Combine(docsPath, "NamingConventions_Generated.md");
            File.WriteAllText(filePath, markdownContent);

            UpdateMainDocumentation();

            AssetDatabase.Refresh();
            Debug.Log($"Naming conventions documentation generated: {filePath}");
        }

        private static void UpdateMainDocumentation()
        {
            string mainDocPath = "Assets/Scripts/ArtPipeline/Documentation/NamingConventions.md";
            if (File.Exists(mainDocPath))
            {
                _ = File.ReadAllText(mainDocPath);

                // Find and replace the generated sections (we might want to use markers)
                // For now, we'll just log that manual update might be needed
                Debug.Log("Please manually update the main NamingConventions.md with any new rules");
            }
            else
            {
                File.WriteAllText(mainDocPath, GetFullDocumentationContent());
            }
        }

        private static string GetFullDocumentationContent() => NamingConventions.GenerateMarkdownDocumentation();

        [MenuItem("Tools/Art Pipeline/Open Naming Documentation")]
        public static void OpenNamingDocumentation()
        {
            string docPath = "Assets/ArtPipeline/Documentation/NamingConventions.md";
            if (File.Exists(docPath))
            {
                _ = AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(docPath));
            }
            else
            {
                Debug.LogWarning("Naming conventions documentation not found. Generating...");
                GenerateNamingDocumentation();
            }
        }
    }
}
