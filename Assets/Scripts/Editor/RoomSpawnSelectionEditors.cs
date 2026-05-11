#if UNITY_EDITOR
using System.Collections.Generic;
using EscapeRoom.Debugging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoom.Rooms.Editor
{
    [CustomEditor(typeof(RoomPortal))]
    public sealed class RoomPortalEditor : UnityEditor.Editor
    {
        private SerializedProperty targetSceneProperty;
        private SerializedProperty transitionOnTriggerEnterProperty;
        private SerializedProperty triggerFilterModeProperty;
        private SerializedProperty allowedTriggerRootsProperty;
        private SerializedProperty allowedTriggerLayersProperty;
        private SerializedProperty startLockedProperty;
        private SerializedProperty requiredFlagsProperty;
        private SerializedProperty requireAllFlagsProperty;
        private SerializedProperty onBlockedProperty;
        private SerializedProperty onUnlockedProperty;
        private SerializedProperty onLockedProperty;
        private SerializedProperty onTransitionRequestedProperty;

        private void OnEnable()
        {
            targetSceneProperty = serializedObject.FindProperty("targetScene");
            transitionOnTriggerEnterProperty = serializedObject.FindProperty("transitionOnTriggerEnter");
            triggerFilterModeProperty = serializedObject.FindProperty("triggerFilterMode");
            allowedTriggerRootsProperty = serializedObject.FindProperty("allowedTriggerRoots");
            allowedTriggerLayersProperty = serializedObject.FindProperty("allowedTriggerLayers");
            startLockedProperty = serializedObject.FindProperty("startLocked");
            requiredFlagsProperty = serializedObject.FindProperty("requiredFlags");
            requireAllFlagsProperty = serializedObject.FindProperty("requireAllFlags");
            onBlockedProperty = serializedObject.FindProperty("onBlocked");
            onUnlockedProperty = serializedObject.FindProperty("onUnlocked");
            onLockedProperty = serializedObject.FindProperty("onLocked");
            onTransitionRequestedProperty = serializedObject.FindProperty("onTransitionRequested");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Only", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(targetSceneProperty);
            RoomSpawnEditorUtility.DrawSpawnPopup(
                serializedObject,
                ((RoomPortal)target).TargetSceneAsset,
                serializedObject.FindProperty("targetSpawnGuid"),
                serializedObject.FindProperty("targetSpawnDisplayName"),
                "Target Spawn Point");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Trigger", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(transitionOnTriggerEnterProperty);
            if (transitionOnTriggerEnterProperty.boolValue)
            {
                EditorGUILayout.PropertyField(triggerFilterModeProperty);

                RoomPortal.TriggerFilterMode triggerFilterMode =
                    (RoomPortal.TriggerFilterMode)triggerFilterModeProperty.enumValueIndex;
                if (triggerFilterMode == RoomPortal.TriggerFilterMode.SpecificObjects)
                {
                    EditorGUILayout.PropertyField(allowedTriggerRootsProperty, true);
                }
                else if (triggerFilterMode == RoomPortal.TriggerFilterMode.LayerMask)
                {
                    EditorGUILayout.PropertyField(allowedTriggerLayersProperty);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Lock State", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(startLockedProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Requirements", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(requiredFlagsProperty, true);
            EditorGUILayout.PropertyField(requireAllFlagsProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onBlockedProperty);
            EditorGUILayout.PropertyField(onUnlockedProperty);
            EditorGUILayout.PropertyField(onLockedProperty);
            EditorGUILayout.PropertyField(onTransitionRequestedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RoomFlowController))]
    public sealed class RoomFlowControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty initialSceneProperty;
        private SerializedProperty playerRootProperty;
        private SerializedProperty playerHeadProperty;
        private SerializedProperty unloadPreviousRoomProperty;
        private SerializedProperty setLoadedRoomAsActiveSceneProperty;
        private SerializedProperty trackVisitedRoomsProperty;

        private void OnEnable()
        {
            initialSceneProperty = serializedObject.FindProperty("initialScene");
            playerRootProperty = serializedObject.FindProperty("playerRoot");
            playerHeadProperty = serializedObject.FindProperty("playerHead");
            unloadPreviousRoomProperty = serializedObject.FindProperty("unloadPreviousRoom");
            setLoadedRoomAsActiveSceneProperty = serializedObject.FindProperty("setLoadedRoomAsActiveScene");
            trackVisitedRoomsProperty = serializedObject.FindProperty("trackVisitedRooms");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Only", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initialSceneProperty);
            RoomSpawnEditorUtility.DrawSpawnPopup(
                serializedObject,
                ((RoomFlowController)target).InitialSceneAsset,
                serializedObject.FindProperty("initialSpawnGuid"),
                serializedObject.FindProperty("initialSpawnDisplayName"),
                "Initial Spawn Point");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playerRootProperty);
            EditorGUILayout.PropertyField(playerHeadProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(unloadPreviousRoomProperty);
            EditorGUILayout.PropertyField(setLoadedRoomAsActiveSceneProperty);
            EditorGUILayout.PropertyField(trackVisitedRoomsProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RoomDebugController))]
    public sealed class RoomDebugControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty selectedRoomSceneProperty;
        private SerializedProperty roomFlowControllerProperty;
        private SerializedProperty logActionsProperty;

        private void OnEnable()
        {
            selectedRoomSceneProperty = serializedObject.FindProperty("selectedRoomScene");
            roomFlowControllerProperty = serializedObject.FindProperty("roomFlowController");
            logActionsProperty = serializedObject.FindProperty("logActions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Only", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(selectedRoomSceneProperty);
            RoomSpawnEditorUtility.DrawSpawnPopup(
                serializedObject,
                ((RoomDebugController)target).SelectedRoomSceneAsset,
                serializedObject.FindProperty("selectedSpawnGuid"),
                serializedObject.FindProperty("selectedSpawnDisplayName"),
                "Selected Spawn Point");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(roomFlowControllerProperty);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(logActionsProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }

    internal static class RoomSpawnEditorUtility
    {
        private readonly struct SpawnOption
        {
            public SpawnOption(string guid, string displayName)
            {
                Guid = guid;
                DisplayName = displayName;
            }

            public string Guid { get; }
            public string DisplayName { get; }
        }

        public static void DrawSpawnPopup(
            SerializedObject serializedObject,
            SceneAsset sceneAsset,
            SerializedProperty spawnGuidProperty,
            SerializedProperty spawnDisplayNameProperty,
            string label)
        {
            List<SpawnOption> options = GetSpawnOptions(sceneAsset);
            options.Insert(0, new SpawnOption(string.Empty, "First Spawn Point"));

            string currentGuid = spawnGuidProperty.stringValue;
            int currentIndex = 0;
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Guid == currentGuid)
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

            int nextIndex = EditorGUILayout.Popup(new GUIContent(label), currentIndex, optionLabels);
            if (nextIndex != currentIndex)
            {
                spawnGuidProperty.stringValue = options[nextIndex].Guid;
                spawnDisplayNameProperty.stringValue = options[nextIndex].DisplayName;
            }

            if (sceneAsset != null && options.Count == 1)
            {
                EditorGUILayout.HelpBox("This scene has no RoomSpawnPoint yet. It will fall back to the current player position.", MessageType.Warning);
            }
        }

        private static List<SpawnOption> GetSpawnOptions(SceneAsset sceneAsset)
        {
            var options = new List<SpawnOption>();
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
                    RoomSpawnPoint[] spawnPoints = roots[i].GetComponentsInChildren<RoomSpawnPoint>(true);
                    for (int j = 0; j < spawnPoints.Length; j++)
                    {
                        RoomSpawnPoint spawnPoint = spawnPoints[j];
                        string guid = spawnPoint.SpawnGuid;
                        if (string.IsNullOrWhiteSpace(guid))
                        {
                            continue;
                        }

                        string hierarchyPath = GetHierarchyPath(spawnPoint.transform);
                        string displayName = string.IsNullOrWhiteSpace(spawnPoint.DisplayName)
                            ? hierarchyPath
                            : $"{spawnPoint.DisplayName} ({hierarchyPath})";

                        options.Add(new SpawnOption(guid, displayName));
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
}
#endif
