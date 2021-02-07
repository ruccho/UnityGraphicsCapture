using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ruccho.GraphicsCapture.Editor
{
    [CustomEditor(typeof(CaptureTexture))]
    public class CaptureTextureEditor : UnityEditor.Editor
    {
        public CaptureTexture Target => target as CaptureTexture;

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            var currentTarget = Target.CurrentTarget;
            if (currentTarget == null)
            {
                EditorGUILayout.LabelField("None");
            }
            else
            {
                EditorGUILayout.LabelField(currentTarget.Description);
            }

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            if (GUILayout.Button("..."))
            {
                var rect = GUILayoutUtility.GetLastRect();
                rect.y += EditorGUIUtility.singleLineHeight;
                var menu = new GenericMenu();
                var targets = Utils.GetTargets();
                foreach (var target in targets)
                {
                    menu.AddItem(new GUIContent(target.Description), false, () =>
                    {
                        Target.SetTarget(target);
                    });
                }
                
                menu.DropDown(rect);
            }
            
            EditorGUI.EndDisabledGroup();
            
            GUILayout.EndHorizontal();
            
            
            base.OnInspectorGUI();
        }
    }
}