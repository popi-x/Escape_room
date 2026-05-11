#if UNITY_EDITOR
using System.Collections.Generic;
using EscapeRoom.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomPropertyDrawer(typeof(ProgressFlagReference))]
public sealed class ProgressFlagReferenceDrawer : PropertyDrawer
{
    private readonly struct FlagOption
    {
        public FlagOption(string sceneName, string guid, string displayName)
        {
            SceneName = sceneName;
            Guid = guid;
            DisplayName = displayName;
        }

        public string SceneName { get; }
        public string Guid { get; }
        public string DisplayName { get; }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty sceneProperty = property.FindPropertyRelative("scene");
        SerializedProperty sceneNameProperty = property.FindPropertyRelative("sceneName");
        SerializedProperty flagGuidProperty = property.FindPropertyRelative("flagGuid");
        SerializedProperty displayNameProperty = property.FindPropertyRelative("displayName");

        Rect sceneRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect popupRect = new Rect(
            position.x,
            position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
            position.width,
            EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(sceneRect, sceneProperty, label);
        if (EditorGUI.EndChangeCheck())
        {
            Object sceneObject = sceneProperty.objectReferenceValue;
            sceneNameProperty.stringValue = sceneObject != null ? ((SceneAsset)sceneObject).name : string.Empty;
            flagGuidProperty.stringValue = string.Empty;
            displayNameProperty.stringValue = string.Empty;
        }

        SceneAsset sceneAsset = sceneProperty.objectReferenceValue as SceneAsset;
        List<FlagOption> options = GetFlagOptions(sceneAsset);
        options.Insert(0, new FlagOption(sceneNameProperty.stringValue, string.Empty, "None"));

        int currentIndex = 0;
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].Guid == flagGuidProperty.stringValue)
            {
                currentIndex = i;
                break;
            }
        }

        string[] optionLabels = new string[options.Count];
        for (int i = 0; i < options.Count; i++)
        {
            optionLabels[i] = options[i].DisplayName;
        }

        int nextIndex = EditorGUI.Popup(popupRect, "Progress Flag", currentIndex, optionLabels);
        if (nextIndex != currentIndex)
        {
            sceneNameProperty.stringValue = options[nextIndex].SceneName;
            flagGuidProperty.stringValue = options[nextIndex].Guid;
            displayNameProperty.stringValue = options[nextIndex].DisplayName;
        }

        EditorGUI.EndProperty();
    }

    private static List<FlagOption> GetFlagOptions(SceneAsset sceneAsset)
    {
        var options = new List<FlagOption>();
        if (sceneAsset == null)
        {
            return options;
        }

        string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return options;
        }

        bool wasAlreadyOpen = TryGetOpenScene(scenePath, out Scene openScene);
        if (EditorApplication.isPlayingOrWillChangePlaymode && !wasAlreadyOpen)
        {
            return options;
        }

        if (!wasAlreadyOpen)
        {
            openScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }

        try
        {
            GameObject[] roots = openScene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                ProgressFlagDefinition[] flags = roots[i].GetComponentsInChildren<ProgressFlagDefinition>(true);
                for (int j = 0; j < flags.Length; j++)
                {
                    ProgressFlagDefinition flag = flags[j];
                    if (string.IsNullOrWhiteSpace(flag.FlagGuid))
                    {
                        continue;
                    }

                    string hierarchyPath = GetHierarchyPath(flag.transform);
                    string displayName = string.IsNullOrWhiteSpace(flag.DisplayName)
                        ? hierarchyPath
                        : $"{flag.DisplayName} ({hierarchyPath})";

                    options.Add(new FlagOption(openScene.name, flag.FlagGuid, displayName));
                }
            }
        }
        finally
        {
            if (!wasAlreadyOpen && openScene.IsValid())
            {
                EditorSceneManager.CloseScene(openScene, true);
            }
        }

        return options;
    }

    private static bool TryGetOpenScene(string scenePath, out Scene scene)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene candidate = SceneManager.GetSceneAt(i);
            if (candidate.path == scenePath)
            {
                scene = candidate;
                return true;
            }
        }

        scene = default;
        return false;
    }

    private static string GetHierarchyPath(Transform target)
    {
        string path = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
#endif
