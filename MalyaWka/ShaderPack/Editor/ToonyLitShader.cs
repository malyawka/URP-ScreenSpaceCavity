using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Rendering.Universal;
using UnityEngine.Rendering;

namespace MalyaWka.ShaderPack.Editor
{
    public class ToonyLitShader : ShaderGUI
    {
        #region EnumsAndClasses

        public enum SurfaceType
        {
            Opaque,
            Transparent
        }

        public enum BlendMode
        {
            Alpha,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Premultiply, // Physically plausible transparency mode, implemented as alpha pre-multiply
            Additive,
            Multiply
        }

        public enum RenderFace
        {
            Front = 2,
            Back = 1,
            Both = 0
        }

        public enum ZWriteMode
        {
            Default = 0,
            On = 1,
            Off = 2
        }
        
        protected class Styles
        {
            // Catergories
            public static readonly GUIContent SurfaceOptions =
                new GUIContent("Surface Options", "Controls how Universal RP renders the Material on a screen.");

            public static readonly GUIContent SurfaceInputs = new GUIContent("Surface Inputs",
                "These settings describe the look and feel of the surface itself.");

            public static readonly GUIContent AdvancedLabel = new GUIContent("Advanced",
                "These settings affect behind-the-scenes rendering and underlying calculations.");

            public static readonly GUIContent surfaceType = new GUIContent("Surface Type",
                "Select a surface type for your texture. Choose between Opaque or Transparent.");

            public static readonly GUIContent blendingMode = new GUIContent("Blending Mode",
                "Controls how the color of the Transparent surface blends with the Material color in the background.");

            public static readonly GUIContent cullingText = new GUIContent("Render Face",
                "Specifies which faces to cull from your geometry. Front culls front faces. Back culls backfaces. None means that both sides are rendered.");

            public static readonly GUIContent alphaClipText = new GUIContent("Alpha Clipping",
                "Makes your Material act like a Cutout shader. Use this to create a transparent effect with hard edges between opaque and transparent areas.");

            public static readonly GUIContent alphaClipThresholdText = new GUIContent("Threshold",
                "Sets where the Alpha Clipping starts. The higher the value is, the brighter the  effect is when clipping starts.");

            public static readonly GUIContent baseMap = new GUIContent("Base Map",
                "Specifies the base Material and/or Color of the surface. If you’ve selected Transparent or Alpha Clipping under Surface Options, your Material uses the Texture’s alpha channel or color.");

            public static readonly GUIContent queueSlider = new GUIContent("Render Order Priority",
                "Determines the chronological rendering order for a Material. High values are rendered first.");
            
            public static readonly GUIContent cavityEnabled = new GUIContent("Enable Cavity",
                "Select cavity enabled or not for this material.");
            
            public static readonly GUIContent zWriteMode = new GUIContent("Z Write Mode",
                "Select this if want rewrite default mode for z write.");
            
            public static readonly GUIContent topEnabled = new GUIContent("Always On Top",
                "Select always on top enabled or not for this material.");
        }

        #endregion
        
        #region Variables

        protected MaterialEditor materialEditor { get; set; }

        protected MaterialProperty surfaceTypeProp { get; set; }

        protected MaterialProperty blendModeProp { get; set; }

        protected MaterialProperty cullingProp { get; set; }

        protected MaterialProperty alphaClipProp { get; set; }

        protected MaterialProperty alphaCutoffProp { get; set; }

        protected MaterialProperty baseMapProp { get; set; }

        protected MaterialProperty baseColorProp { get; set; }

        protected MaterialProperty highlightColorProp { get; set; }

        protected MaterialProperty shadowColorProp { get; set; }
        
        protected MaterialProperty queueOffsetProp { get; set; }
        
        protected MaterialProperty cavityProp { get; set; }
        
        protected MaterialProperty zWriteModeProp { get; set; }
        
        protected MaterialProperty topProp { get; set; }

        public bool m_FirstTimeApply = true;

        private const string k_KeyPrefix = "UniversalRP:Material:UI_State:";

        private string m_HeaderStateKey = null;

        protected string headerStateKey { get { return m_HeaderStateKey; } }

        SavedBool m_SurfaceOptionsFoldout;

        SavedBool m_SurfaceInputsFoldout;

        SavedBool m_AdvancedFoldout;

        #endregion
        
        private const int queueOffsetRange = 50;

        #region GeneralFunctions

        public virtual void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SetMaterialKeywords(material);
        }
        
        public virtual void FindProperties(MaterialProperty[] properties)
        {
            surfaceTypeProp = FindProperty("_Surface", properties);
            blendModeProp = FindProperty("_Blend", properties);
            cullingProp = FindProperty("_Cull", properties);
            alphaClipProp = FindProperty("_AlphaClip", properties);
            alphaCutoffProp = FindProperty("_Cutoff", properties);
            baseMapProp = FindProperty("_BaseMap", properties, false);
            baseColorProp = FindProperty("_BaseColor", properties, false);
            highlightColorProp = FindProperty("_HighlightColor", properties, false);
            shadowColorProp = FindProperty("_ShadowColor", properties, false);
            queueOffsetProp = FindProperty("_QueueOffset", properties, false);
            cavityProp = FindProperty("_CavityEnabled", properties, false);
            zWriteModeProp = FindProperty("_ZWriteMode", properties, false);
            topProp = FindProperty("_AlwaysOnTop", properties, false);
        }
        
        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException("materialEditorIn");

            FindProperties(properties); 
            materialEditor = materialEditorIn;
            Material material = materialEditor.target as Material;

            if (m_FirstTimeApply)
            {
                OnOpenGUI(material, materialEditorIn);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }
        
        public virtual void OnOpenGUI(Material material, MaterialEditor materialEditor)
        {
            m_HeaderStateKey = k_KeyPrefix + material.shader.name; 
            m_SurfaceOptionsFoldout = new SavedBool($"{m_HeaderStateKey}.SurfaceOptionsFoldout", true);
            m_SurfaceInputsFoldout = new SavedBool($"{m_HeaderStateKey}.SurfaceInputsFoldout", true);
            m_AdvancedFoldout = new SavedBool($"{m_HeaderStateKey}.AdvancedFoldout", false);

            foreach (var obj in  materialEditor.targets)
                MaterialChanged((Material)obj);
        }
        
        public void ShaderPropertiesGUI(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            EditorGUI.BeginChangeCheck();

            m_SurfaceOptionsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SurfaceOptionsFoldout.value, Styles.SurfaceOptions);
            if(m_SurfaceOptionsFoldout.value){
                DrawSurfaceOptions(material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_SurfaceInputsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SurfaceInputsFoldout.value, Styles.SurfaceInputs);
            if (m_SurfaceInputsFoldout.value)
            {
                DrawSurfaceInputs(material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            DrawAdditionalFoldouts(material);

            m_AdvancedFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_AdvancedFoldout.value, Styles.AdvancedLabel);
            if (m_AdvancedFoldout.value)
            {
                DrawAdvancedOptions(material);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in  materialEditor.targets)
                    MaterialChanged((Material)obj);
            }
        }
        
        #endregion
        
        #region DrawingFunctions

        public virtual void DrawSurfaceOptions(Material material)
        {
            DoPopup(Styles.surfaceType, surfaceTypeProp, Enum.GetNames(typeof(SurfaceType)));
            if ((SurfaceType)material.GetFloat("_Surface") == SurfaceType.Transparent)
                DoPopup(Styles.blendingMode, blendModeProp, Enum.GetNames(typeof(BlendMode)));

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = cullingProp.hasMixedValue;
            var culling = (RenderFace)cullingProp.floatValue;
            culling = (RenderFace)EditorGUILayout.EnumPopup(Styles.cullingText, culling);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(Styles.cullingText.text);
                cullingProp.floatValue = (float)culling;
                material.doubleSidedGI = (RenderFace)cullingProp.floatValue != RenderFace.Front;
            }

            EditorGUI.showMixedValue = false;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = alphaClipProp.hasMixedValue;
            var alphaClipEnabled = EditorGUILayout.Toggle(Styles.alphaClipText, Mathf.Approximately(alphaClipProp.floatValue, 1));
            if (EditorGUI.EndChangeCheck())
                alphaClipProp.floatValue = alphaClipEnabled ? 1 : 0;
            EditorGUI.showMixedValue = false;

            if (Mathf.Approximately(alphaClipProp.floatValue, 1))
                materialEditor.ShaderProperty(alphaCutoffProp, Styles.alphaClipThresholdText, 1);
        }

        public virtual void DrawSurfaceInputs(Material material)
        {
            DrawBaseProperties(material);
        }

        public virtual void DrawAdvancedOptions(Material material)
        {
            materialEditor.EnableInstancingField();
            DrawCavityEnableField(material);
            DrawTopEnableField(material);
            DrawZWriteField(material);
            DrawQueueOffsetField();
        }

        protected void DrawZWriteField(Material material)
        {
            if (material.HasProperty("_ZWriteMode"))
            {
                DoPopup(Styles.zWriteMode, zWriteModeProp, Enum.GetNames(typeof(ZWriteMode)));
                if ((ZWriteMode) material.GetFloat("_ZWriteMode") == ZWriteMode.On)
                {
                    material.SetInt("_ZWrite", 1);
                }
                else if ((ZWriteMode) material.GetFloat("_ZWriteMode") == ZWriteMode.Off)
                {
                    material.SetInt("_ZWrite", 0);
                }
            }
        }

        protected void DrawCavityEnableField(Material material)
        {
            if (cavityProp != null)
            {
                EditorGUI.BeginChangeCheck();
                var cavity = EditorGUILayout.Toggle(Styles.cavityEnabled, Mathf.Approximately(cavityProp.floatValue, 1.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    if (cavity)
                    {
                        cavityProp.floatValue = 1.0f;
                        material.SetShaderPassEnabled("DepthOnly", true);
                        material.SetShaderPassEnabled("DepthNormals", true);
                    }
                    else
                    {
                        cavityProp.floatValue = 0.0f;            
                        material.SetShaderPassEnabled("DepthOnly", false);
                        material.SetShaderPassEnabled("DepthNormals", false);
                    }
                }
            }
        }
        
        protected void DrawTopEnableField(Material material)
        {
            if (topProp != null)
            {
                EditorGUI.BeginChangeCheck();
                var top = EditorGUILayout.Toggle(Styles.topEnabled, Mathf.Approximately(topProp.floatValue, 1.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    if (top)
                    {
                        topProp.floatValue = 1.0f;
                    }
                    else
                    {
                        topProp.floatValue = 0.0f;
                    }
                }
            }
        }
        
        protected void DrawQueueOffsetField()
        {
            if (queueOffsetProp != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = queueOffsetProp.hasMixedValue;
                var queue = EditorGUILayout.IntSlider(Styles.queueSlider, (int)queueOffsetProp.floatValue, -queueOffsetRange, queueOffsetRange);
                if (EditorGUI.EndChangeCheck())
                    queueOffsetProp.floatValue = queue;
                EditorGUI.showMixedValue = false;
            }
        }

        public virtual void DrawAdditionalFoldouts(Material material){}

        public virtual void DrawBaseProperties(Material material) 
        {
            if (baseMapProp != null && baseColorProp != null) // Draw the baseMap, most shader will have at least a baseMap
            {
                materialEditor.TexturePropertySingleLine(Styles.baseMap, baseMapProp, baseColorProp);
                // TODO Temporary fix for lightmapping, to be replaced with attribute tag.
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", baseMapProp.textureValue);
                    var baseMapTiling = baseMapProp.textureScaleAndOffset;
                    material.SetTextureScale("_MainTex", new Vector2(baseMapTiling.x, baseMapTiling.y));
                    material.SetTextureOffset("_MainTex", new Vector2(baseMapTiling.z, baseMapTiling.w));
                }
                materialEditor.ColorProperty(highlightColorProp, "Highlight Color");
                materialEditor.ColorProperty(shadowColorProp, "Shadow Color");
            }
            else if (baseMapProp == null && baseColorProp != null)
            {
                materialEditor.ColorProperty(baseColorProp, "Base Color");
                materialEditor.ColorProperty(highlightColorProp, "Highlight Color");
                materialEditor.ColorProperty(shadowColorProp, "Shadow Color");
            }
            else
            {
                materialEditor.ColorProperty(highlightColorProp, "Highlight Color");
                materialEditor.ColorProperty(shadowColorProp, "Shadow Color");
            }
        }

        protected static void DrawTileOffset(MaterialEditor materialEditor, MaterialProperty textureProp)
        {
            materialEditor.TextureScaleOffsetProperty(textureProp);
        }

        #endregion
        
        #region MaterialDataFunctions

        public static void SetMaterialKeywords(Material material, Action<Material> shadingModelFunc = null, Action<Material> shaderFunc = null)
        {
            // Clear all keywords for fresh start
            material.shaderKeywords = null;

            // Setup blending - consistent across all Universal RP shaders
            SetupMaterialBlendMode(material);

            // Shader specific keyword functions
            shadingModelFunc?.Invoke(material);
            shaderFunc?.Invoke(material);
        }

        public static void SetupMaterialBlendMode(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            bool alphaClip = false;
            if(material.HasProperty("_AlphaClip"))
                alphaClip = material.GetFloat("_AlphaClip") >= 0.5;

            if (alphaClip)
            {
                material.EnableKeyword("_ALPHATEST_ON");
            }
            else
            {
                material.DisableKeyword("_ALPHATEST_ON");
            }

            if (material.HasProperty("_Surface"))
            {
                SurfaceType surfaceType = (SurfaceType) material.GetFloat("_Surface");
                if (surfaceType == SurfaceType.Opaque)
                {
                    if (alphaClip)
                    {
                        material.renderQueue = (int) RenderQueue.AlphaTest;
                        material.SetOverrideTag("RenderType", "TransparentCutout");
                    }
                    else
                    {
                        material.renderQueue = (int) RenderQueue.Geometry;
                        material.SetOverrideTag("RenderType", "Opaque");
                    }

                    material.renderQueue += material.HasProperty("_QueueOffset") ? (int) material.GetFloat("_QueueOffset") : 0;
                    material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    if (material.HasProperty("_ZWriteMode") && (ZWriteMode) material.GetFloat("_ZWriteMode") == ZWriteMode.Default)
                    {
                        material.SetInt("_ZWrite", 1);
                    }
                }
                else
                {
                    BlendMode blendMode = (BlendMode) material.GetFloat("_Blend");

                    // Specific Transparent Mode Settings
                    switch (blendMode)
                    {
                        case BlendMode.Alpha:
                            material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                            material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case BlendMode.Premultiply:
                            material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                            material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case BlendMode.Additive:
                            material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                            material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.One);
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            break;
                        case BlendMode.Multiply:
                            material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.DstColor);
                            material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            material.EnableKeyword("_ALPHAMODULATE_ON");
                            break;
                    }

                    // General Transparent Material Settings
                    material.SetOverrideTag("RenderType", "Transparent");
                    if (material.HasProperty("_ZWriteMode") && (ZWriteMode) material.GetFloat("_ZWriteMode") == ZWriteMode.Default)
                    {
                        material.SetInt("_ZWrite", 0);
                    }
                    material.renderQueue = (int)RenderQueue.Transparent;
                    material.renderQueue += material.HasProperty("_QueueOffset") ? (int) material.GetFloat("_QueueOffset") : 0;
                }
            }
        }

        #endregion
        
        #region HelperFunctions

        public static void TwoFloatSingleLine(GUIContent title, MaterialProperty prop1, GUIContent prop1Label,
            MaterialProperty prop2, GUIContent prop2Label, MaterialEditor materialEditor, float labelWidth = 30f)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop1.hasMixedValue || prop2.hasMixedValue;
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.PrefixLabel(rect, title);
            var indent = EditorGUI.indentLevel;
            var preLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = labelWidth;
            Rect propRect1 = new Rect(rect.x + preLabelWidth, rect.y,
                (rect.width - preLabelWidth) * 0.5f, EditorGUIUtility.singleLineHeight);
            var prop1val = EditorGUI.FloatField(propRect1, prop1Label, prop1.floatValue);

            Rect propRect2 = new Rect(propRect1.x + propRect1.width, rect.y,
                propRect1.width, EditorGUIUtility.singleLineHeight);
            var prop2val = EditorGUI.FloatField(propRect2, prop2Label, prop2.floatValue);

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = preLabelWidth;

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(title.text);
                prop1.floatValue = prop1val;
                prop2.floatValue = prop2val;
            }

            EditorGUI.showMixedValue = false;
        }

        public void DoPopup(GUIContent label, MaterialProperty property, string[] options)
        {
            DoPopup(label, property, options, materialEditor);
        }

        public static void DoPopup(GUIContent label, MaterialProperty property, string[] options, MaterialEditor materialEditor)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            EditorGUI.showMixedValue = property.hasMixedValue;

            var mode = property.floatValue;
            EditorGUI.BeginChangeCheck();
            mode = EditorGUILayout.Popup(label, (int)mode, options);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(label.text);
                property.floatValue = mode;
            }

            EditorGUI.showMixedValue = false;
        }
        
        public static Rect TextureColorProps(MaterialEditor materialEditor, GUIContent label, MaterialProperty textureProp, MaterialProperty colorProp, bool hdr = false)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.showMixedValue = textureProp.hasMixedValue;
            materialEditor.TexturePropertyMiniThumbnail(rect, textureProp, label.text, label.tooltip);
            EditorGUI.showMixedValue = false;

            if (colorProp != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = colorProp.hasMixedValue;
                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                Rect rectAfterLabel = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y,
                    EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight);
                var col = EditorGUI.ColorField(rectAfterLabel, GUIContent.none, colorProp.colorValue, true,
                    false, hdr);
                EditorGUI.indentLevel = indentLevel;
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo(colorProp.displayName);
                    colorProp.colorValue = col;
                }
                EditorGUI.showMixedValue = false;
            }

            return rect;
        }
        
        public new static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties)
        {
            return FindProperty(propertyName, properties, true);
        }

        public new static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory)
        {
            for (int index = 0; index < properties.Length; ++index)
            {
                if (properties[index] != null && properties[index].name == propertyName)
                    return properties[index];
            }
            if (propertyIsMandatory)
                throw new ArgumentException("Could not find MaterialProperty: '" + propertyName + "', Num properties: " + (object) properties.Length);
            return null;
        }

        #endregion
    }
}
