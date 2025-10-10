using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Linq;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Configures Addressables groups for the project
    /// </summary>
    public static class AddressablesSetup
    {
        [MenuItem("Tools/Art Pipeline/Setup Addressables Groups")]
        public static void SetupAddressablesGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressables Settings not found. Please initialize Addressables first.");
                return;
            }

            CreateGroupIfMissing(settings, "Characters");
            CreateGroupIfMissing(settings, "Weapons");
            CreateGroupIfMissing(settings, "Environment");
            CreateGroupIfMissing(settings, "UI");
            CreateGroupIfMissing(settings, "Audio");

            Debug.Log("Addressables groups setup complete");
        }

        private static void CreateGroupIfMissing(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.groups.FirstOrDefault(g => g.Name == groupName);
            if (group == null)
            {
                _ = settings.CreateGroup(groupName, false, false, true, null);
                Debug.Log($"Created Addressables group: {groupName}");
            }
        }

        [MenuItem("Tools/Art Pipeline/Mark Selected as Addressable")]
        public static void MarkSelectedAsAddressable()
        {
            var selected = Selection.objects;
            if (selected.Length == 0)
            {
                _ = EditorUtility.DisplayDialog("Addressables", "No assets selected", "OK");
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            foreach (var obj in selected)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                string guid = AssetDatabase.AssetPathToGUID(path);

                string groupName = DetermineGroup(path);
                var group = settings.groups.FirstOrDefault(g => g.Name == groupName);

                if (group != null)
                {
                    var entry = settings.CreateOrMoveEntry(guid, group);
                    entry.address = obj.name;
                    Debug.Log($"Marked {obj.name} as addressable in group {groupName}");
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        }

        private static string DetermineGroup(string assetPath)
        {
            if (assetPath.Contains("/Characters/"))
            {
                return "Characters";
            }

            if (assetPath.Contains("/Weapons/"))
            {
                return "Weapons";
            }

            if (assetPath.Contains("/Environment/"))
            {
                return "Environment";
            }

            if (assetPath.Contains("/UI/"))
            {
                return "UI";
            }

            if (assetPath.Contains("/Audio/"))
            {
                return "Audio";
            }

            return "Misc";
        }
    }
}
