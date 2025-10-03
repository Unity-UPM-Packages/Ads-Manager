using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    [CustomPropertyDrawer(typeof(MaxUnitID))]
    public class MaxUnitIDDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            string androidKey = property.propertyPath + "_android";
            if (!foldoutStates.ContainsKey(androidKey)) foldoutStates.Add(androidKey, false);

            foldoutStates[androidKey] = EditorGUILayout.Foldout(foldoutStates[androidKey], "Android Unit IDs", true);
            if (foldoutStates[androidKey])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("bannerIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("interIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("interOpenIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("rewardIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("mrecIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("mrecOpenIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("appOpenIds"), true);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0; // Returning 0 makes the drawer take up the space of its contents.
        }
    }

    [CustomPropertyDrawer(typeof(AdmobUnitID))]
    public class AdmobUnitIDDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            string androidKey = property.propertyPath + "_android";
            if (!foldoutStates.ContainsKey(androidKey)) foldoutStates.Add(androidKey, false);

            foldoutStates[androidKey] = EditorGUILayout.Foldout(foldoutStates[androidKey], "Android Unit IDs", true);
            if (foldoutStates[androidKey])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("bannerIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("interIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("rewardIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("mrecIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("interOpenIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("mrecOpenIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("appOpenIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeUnityIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeOverlayIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeBannerIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeInterIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeRewardIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeMrecIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeAppOpenIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeInterOpenIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeMrecOpenIds"), true);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("nativeVideoIds"), true);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0; // Returning 0 makes the drawer take up the space of its contents.
        }
    }
}