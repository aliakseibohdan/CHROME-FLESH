using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Folder Structure Creator for Unity Editor
/// Creates a complete folder hierarchy based on text input format
/// </summary>
public class FolderStructureCreator : EditorWindow
{
    private string _folderStructureText = "";
    private Vector2 _scrollPosition;
    private const string _defaultStructure = @"Assets/
├── Art/
│   ├── Characters/
│   │   ├── Hero/
│   │   │   ├── Models/
│   │   │   ├── Textures/
│   │   │   ├── Materials/
│   │   │   ├── Animations/
│   │   │   └── Prefabs/
│   │   └── Enemy/
│   ├── Environment/
│   │   ├── Props/
│   │   ├── Architecture/
│   │   ├── Terrain/
│   │   └── Vegetation/
│   ├── Weapons/
│   │   ├── Models/
│   │   ├── Textures/
│   │   └── Prefabs/
│   └── UI/
│       ├── Icons/
│       ├── Fonts/
│       └── Sprites/
├── Audio/
│   ├── Music/
│   ├── SFX/
│   │   ├── Weapons/
│   │   ├── Characters/
│   │   ├── UI/
│   │   └── Ambient/
│   └── Voice/
├── Prefabs/
│   ├── Characters/
│   ├── Weapons/
│   ├── Environment/
│   └── System/
├── Materials/
│   ├── Characters/
│   ├── Environment/
│   └── Effects/
└── Addressables/
    ├── Characters/
    ├── Environment/
    ├── Weapons/
    └── UI/";

    /// <summary>
    /// Menu item to open the Folder Structure Creator window
    /// </summary>
    [MenuItem("Tools/Folder Structure Creator")]
    public static void ShowWindow()
    {
        var window = GetWindow<FolderStructureCreator>("Folder Creator");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Folder Structure Creator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Paste your folder structure in the text format below and click 'Create Folders'.", MessageType.Info);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Folder Structure:");
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
        _folderStructureText = EditorGUILayout.TextArea(_folderStructureText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("Load Default Structure", GUILayout.Height(30)))
        {
            _folderStructureText = _defaultStructure;
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Create Folders", GUILayout.Height(40)))
        {
            CreateFolderStructure();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Note: Existing folders will be skipped. All operations are safe and non-destructive.", MessageType.Warning);
    }

    private void CreateFolderStructure()
    {
        if (string.IsNullOrEmpty(_folderStructureText))
        {
            Debug.LogError("Folder structure text is empty!");
            return;
        }

        try
        {
            var folderPaths = ParseFolderStructure(_folderStructureText);
            int createdCount = CreateFoldersFromPaths(folderPaths);

            Debug.Log($"Folder structure creation completed! Created {createdCount} new folders.");

            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating folder structure: {e.Message}");
        }
    }

    private List<string> ParseFolderStructure(string structureText)
    {
        List<string> folderPaths = new();
        string[] lines = structureText.Split('\n');

        Stack<string> pathStack = new();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string cleanLine = CleanLine(line);
            if (string.IsNullOrEmpty(cleanLine))
            {
                continue;
            }

            int indentLevel = CalculateIndentLevel(line);

            while (pathStack.Count > indentLevel)
            {
                _ = pathStack.Pop();
            }

            string currentPath = BuildCurrentPath(pathStack, cleanLine);

            if (!folderPaths.Contains(currentPath))
            {
                folderPaths.Add(currentPath);
            }

            if (cleanLine.EndsWith("/"))
            {
                pathStack.Push(cleanLine.TrimEnd('/'));
            }
            else
            {
                pathStack.Push(cleanLine);
            }
        }

        return folderPaths;
    }

    private string CleanLine(string line)
    {
        string clean = Regex.Replace(line, @"[├│└──]", "").Trim();

        clean = clean.Trim();

        return clean;
    }

    private int CalculateIndentLevel(string line)
    {
        int indent = 0;
        bool foundNonSpace = false;

        foreach (char c in line)
        {
            if (!foundNonSpace && char.IsWhiteSpace(c))
            {
                if (c == ' ')
                {
                    indent++;
                }
                else if (c == '\t')
                {
                    indent += 4;
                }
            }
            else if (c is '├' or '│' or '└')
            {
                indent += 4;
            }
            else if (!char.IsWhiteSpace(c))
            {
                foundNonSpace = true;
            }
        }

        return indent / 4;
    }

    private string BuildCurrentPath(Stack<string> pathStack, string currentFolder)
    {
        List<string> pathParts = new(pathStack);
        pathParts.Reverse();

        string path = string.Join("/", pathParts);

        if (!string.IsNullOrEmpty(path))
        {
            path += "/" + currentFolder.TrimEnd('/');
        }
        else
        {
            path = currentFolder.TrimEnd('/');
        }

        return path;
    }

    private int CreateFoldersFromPaths(List<string> folderPaths)
    {
        int createdCount = 0;

        foreach (string folderPath in folderPaths)
        {
            if (CreateFolderSafe(folderPath))
            {
                createdCount++;
            }
        }

        return createdCount;
    }

    private bool CreateFolderSafe(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogWarning("Attempted to create folder with empty path.");
            return false;
        }

        if (AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.Log($"Folder already exists: {folderPath}");
            return false;
        }

        try
        {
            string parentFolder = Path.GetDirectoryName(folderPath);
            string newFolderName = Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parentFolder) && !AssetDatabase.IsValidFolder(parentFolder))
            {
                Debug.LogWarning($"Parent folder doesn't exist: {parentFolder}. Cannot create: {folderPath}");
                return false;
            }

            string result = AssetDatabase.CreateFolder(parentFolder, newFolderName);

            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log($"Created folder: {folderPath}");
                return true;
            }
            else
            {
                Debug.LogWarning($"Failed to create folder: {folderPath}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating folder '{folderPath}': {e.Message}");
            return false;
        }
    }
}
