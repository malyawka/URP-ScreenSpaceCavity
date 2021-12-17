using UnityEditor;
using UnityEngine;
using System.IO;

namespace MalyaWka.ScreenSpaceCavity.Editor
{
    [CustomEditor(typeof(Renders.ScreenSpaceCavity))]
    public class ScreenSpaceCavityEditor : UnityEditor.Editor
    {
        #region Serialized Properties
        private SerializedProperty m_CavityType;
        private SerializedProperty m_Degug;
        private SerializedProperty m_CurvatureScale;
        private SerializedProperty m_CurvatureRidge;
        private SerializedProperty m_CurvatureValley;
        private SerializedProperty m_CavityDistance;
        private SerializedProperty m_CavityAttenuation;
        private SerializedProperty m_CavityRidge;
        private SerializedProperty m_CavityValley;
        private SerializedProperty m_CavitySamples;
        #endregion
        
        private bool m_IsInitialized = false;

        private struct Styles
        {
            public static GUIContent CavityType = EditorGUIUtility.TrTextContent("Type", "...");
            public static GUIContent CurvatureScale = EditorGUIUtility.TrTextContent("Scale", "...");
            public static GUIContent CurvatureRidge = EditorGUIUtility.TrTextContent("Ridge", "...");
            public static GUIContent CurvatureValley = EditorGUIUtility.TrTextContent("Valley", "...");
            public static GUIContent CavityDistance = EditorGUIUtility.TrTextContent("Distance", "...");
            public static GUIContent CavityAttenuation = EditorGUIUtility.TrTextContent("Attenuation", "...");
            public static GUIContent CavityRidge = EditorGUIUtility.TrTextContent("Ridge", "...");
            public static GUIContent CavityValley = EditorGUIUtility.TrTextContent("Valley", "...");
            public static GUIContent CavitySamples = EditorGUIUtility.TrTextContent("Samples", "...");
        }

        private void Init()
        {
            SerializedProperty settings = serializedObject.FindProperty("m_Settings");
            m_CavityType = settings.FindPropertyRelative("cavityType");
            m_Degug = settings.FindPropertyRelative("debug");
            m_CurvatureScale = settings.FindPropertyRelative("curvatureScale");
            m_CurvatureRidge = settings.FindPropertyRelative("curvatureRidge");
            m_CurvatureValley = settings.FindPropertyRelative("curvatureValley");
            m_CavityDistance = settings.FindPropertyRelative("cavityDistance");
            m_CavityAttenuation = settings.FindPropertyRelative("cavityAttenuation");
            m_CavityRidge = settings.FindPropertyRelative("cavityRidge");
            m_CavityValley = settings.FindPropertyRelative("cavityValley");
            m_CavitySamples = settings.FindPropertyRelative("cavitySamples");
            m_IsInitialized = true;
        }
        
        public override void OnInspectorGUI()
        {
            if (!m_IsInitialized)
            {
                Init();
            }
            
            serializedObject.Update();
            
            BeginBox("General");
            EditorGUILayout.PropertyField(m_CavityType, Styles.CavityType);
            EditorGUILayout.PropertyField(m_Degug);
            EndBox();

            switch (m_CavityType.enumValueIndex)
            {
                case 0:
                    BeginBox("Curvature");
                    m_CurvatureScale.floatValue = RoundToNearestHalf(EditorGUILayout.Slider(Styles.CurvatureScale,
                        m_CurvatureScale.floatValue, 0.0f, 5.0f));
                    m_CurvatureRidge.floatValue =
                        EditorGUILayout.Slider(Styles.CurvatureRidge, m_CurvatureRidge.floatValue, 0.0f, 2.0f);
                    m_CurvatureValley.floatValue =
                        EditorGUILayout.Slider(Styles.CurvatureValley, m_CurvatureValley.floatValue, 0.0f, 2.0f);
                    EndBox();
                    
                    BeginBox("Cavity");
                    m_CavityDistance.floatValue = 
                        EditorGUILayout.Slider(Styles.CavityDistance, m_CavityDistance.floatValue, 0.0f, 1f);
                    m_CavityAttenuation.floatValue = 
                        EditorGUILayout.Slider(Styles.CavityAttenuation, m_CavityAttenuation.floatValue, 0.0f, 1f);
                    m_CavityRidge.floatValue = 
                        EditorGUILayout.Slider(Styles.CavityRidge, m_CavityRidge.floatValue, 0.0f, 2.5f);
                    m_CavityValley.floatValue = 
                        EditorGUILayout.Slider(Styles.CavityValley, m_CavityValley.floatValue, 0.0f, 2.5f);
                    m_CavitySamples.intValue = 
                        EditorGUILayout.IntSlider(Styles.CavitySamples, m_CavitySamples.intValue, 1, 12);
                    EndBox();
                    break;
                case 1:
                    BeginBox("Curvature");
                    m_CurvatureScale.floatValue = RoundToNearestHalf(EditorGUILayout.Slider(Styles.CurvatureScale,
                        m_CurvatureScale.floatValue, 0.0f, 5.0f));
                    m_CurvatureRidge.floatValue =
                        EditorGUILayout.Slider(Styles.CurvatureRidge, m_CurvatureRidge.floatValue, 0.0f, 2.0f);
                    m_CurvatureValley.floatValue =
                        EditorGUILayout.Slider(Styles.CurvatureValley, m_CurvatureValley.floatValue, 0.0f, 2.0f);
                    EndBox();
                    break;
                case 2:
                    BeginBox("Cavity");
                    m_CavityDistance.floatValue = 
                        EditorGUILayout.Slider(Styles.CavityDistance, m_CavityDistance.floatValue, 0.0f, 1f);
                    m_CavityAttenuation.floatValue = 
                        EditorGUILayout.Slider(Styles.CavityAttenuation, m_CavityAttenuation.floatValue, 0.0f, 1f);
                    m_CavityRidge.floatValue = 
                        EditorGUILayout.Slider(Styles.CavityRidge, m_CavityRidge.floatValue, 0.0f, 2.5f);
                    m_CavityValley.floatValue = 
                        EditorGUILayout.Slider(Styles.CavityValley, m_CavityValley.floatValue, 0.0f, 2.5f);
                    m_CavitySamples.intValue = 
                        EditorGUILayout.IntSlider(Styles.CavitySamples, m_CavitySamples.intValue, 1, 12);
                    EndBox();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void BeginBox(string boxTitle = "")
        {
            GUIStyle style = new GUIStyle("HelpBox");
            style.padding.left = 5;
            style.padding.right = 5;
            style.padding.top = 5;
            style.padding.bottom = 5;

            GUILayout.BeginVertical(style);

            if (!string.IsNullOrEmpty(boxTitle))
            {
                DrawBoldLabel(boxTitle);
            }

            EditorGUI.indentLevel++;
        }
        
        private void EndBox()
        {
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
        }
        
        private void DrawBoldLabel(string text)
        {
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }
        
        private static float RoundToNearestHalf(float a)
        {
            return a = Mathf.Round(a * 2f) * 0.5f;
        }
    }
}
