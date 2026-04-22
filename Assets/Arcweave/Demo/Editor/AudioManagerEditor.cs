using UnityEditor;
using UnityEngine;

namespace Arcweave
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var manager = (AudioManager)target;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);

            bool isPlaying = Application.isPlaying;

            using (new EditorGUI.DisabledScope(!isPlaying))
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(manager.IsPaused ? "Resume All" : "Pause All"))
                    {
                        if (manager.IsPaused) manager.ResumeAll();
                        else manager.PauseAll();
                    }

                    if (GUILayout.Button("Stop All (Fade)"))
                        manager.StopAll();

                    if (GUILayout.Button("Stop Immediate"))
                        manager.StopAllImmediate();
                }
            }

            if (!isPlaying)
            {
                EditorGUILayout.HelpBox("Controls available in Play Mode.", MessageType.Info);
                return;
            }

            // Active sources list
            var sources = manager.ActiveSources;
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField($"Active Loops ({sources.Count})", EditorStyles.boldLabel);

            if (sources.Count == 0)
            {
                EditorGUILayout.LabelField("— none —", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                foreach (var kvp in sources)
                {
                    if (kvp.Value == null) continue;
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        string clipName = kvp.Value.clip != null ? kvp.Value.clip.name : "—";
                        EditorGUILayout.LabelField(clipName, GUILayout.ExpandWidth(true));
                        EditorGUILayout.LabelField(
                            $"vol {kvp.Value.volume:F2}  {(kvp.Value.isPlaying ? "▶" : "⏸")}",
                            EditorStyles.miniLabel, GUILayout.Width(90));
                    }
                }
            }

            // Repaint continuously while in play mode so volume/state stay live.
            if (isPlaying) Repaint();
        }
    }
}
