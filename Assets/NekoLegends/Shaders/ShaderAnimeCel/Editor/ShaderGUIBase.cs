#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLegends
{
    public class ShaderGUIBase : ShaderGUI
    {
        protected void ShowLogo()
        {
            // Displays a neat title header for the custom material inspector
            EditorGUILayout.Space();
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Neko Legends Cel Shader", titleStyle);
            
            // Draw a thin separator line
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space();
        }
    }
}
#endif
