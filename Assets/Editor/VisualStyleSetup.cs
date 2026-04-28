using BubbleTown.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Applies the Phase 2 visual style guide to current placeholder materials and Battle lighting.
    /// Run this after creating or regenerating placeholder art prefabs.
    /// </summary>
    public static class VisualStyleSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";
        private const string MaterialFolder = "Assets/Materials";
        private const string VisualStyleRootName = "VisualStyleRoot";
        private const string BattleRootName = "BattleRoot";

        [MenuItem("BubbleTown/Setup/Apply Phase 2 Visual Style")]
        public static void ApplyPhase2VisualStyle()
        {
            ApplyMaterialPalette();
            ApplyBattleSceneLighting();
            AssetDatabase.SaveAssets();
            Debug.Log("[VisualStyleSetup] Phase 2 visual style has been applied.");
        }

        public static void ApplyPhase2VisualStyleFromBatchmode()
        {
            ApplyPhase2VisualStyle();
        }

        private static void ApplyMaterialPalette()
        {
            ApplyCharacterMaterials();
            ApplyMapMaterials();
            ApplyBombMaterials();
            ApplyExplosionMaterials();
            ApplyItemMaterials();
        }

        private static void ApplyCharacterMaterials()
        {
            ApplyMaterial("Mat_Character_Skin_Peach", "#FFC997", "#000000", 0.28f, false);
            ApplyMaterial("Mat_Character_Face_Dark", "#24324A", "#000000", 0.18f, false);
            ApplyMaterial("Mat_Character_Player1_Body", "#67C7FF", "#000000", 0.46f, false);
            ApplyMaterial("Mat_Character_Player1_Accent", "#FFD34D", "#000000", 0.34f, false);
            ApplyMaterial("Mat_Character_Player2_Body", "#FF6B5E", "#000000", 0.44f, false);
            ApplyMaterial("Mat_Character_Player2_Accent", "#FFD34D", "#000000", 0.34f, false);
            ApplyMaterial("Mat_Character_AI_Body", "#9B8CFF", "#000000", 0.48f, false);
            ApplyMaterial("Mat_Character_AI_Accent", "#FF63B7", "#000000", 0.34f, false);
        }

        private static void ApplyMapMaterials()
        {
            ApplyMaterial("Mat_Tile_GrassPastel", "#91E8A6", "#000000", 0.34f, false);
            ApplyMaterial("Mat_Tile_CandyBlue", "#7EDBFF", "#000000", 0.34f, false);
            ApplyMaterial("Mat_Tile_CheckerAccent", "#FFF2CC", "#000000", 0.28f, false);
            ApplyMaterial("Mat_Wall_Hard_Cream", "#FFF2CC", "#000000", 0.48f, false);
            ApplyMaterial("Mat_Wall_Hard_Highlight", "#FFF9E8", "#000000", 0.42f, false);
            ApplyMaterial("Mat_Wall_Hard_Shadow", "#B8864D", "#000000", 0.24f, false);
            ApplyMaterial("Mat_Wall_Soft_JellyBlue", "#5DDCFF", "#123B4A", 0.62f, true);
        }

        private static void ApplyBombMaterials()
        {
            ApplyMaterial("Mat_Bomb_Body_BubbleNavy", "#24324A", "#081629", 0.62f, true);
            ApplyMaterial("Mat_Bomb_Highlight_Cyan", "#2FE7D6", "#0A6C78", 0.48f, true);
            ApplyMaterial("Mat_Bomb_TopCap_Cream", "#FFF2CC", "#4A3215", 0.36f, true);
            ApplyMaterial("Mat_Bomb_Fuse_Cocoa", "#5A4B42", "#000000", 0.22f, false);
            ApplyMaterial("Mat_Bomb_Spark_Yellow", "#FFD34D", "#FF7A1A", 0.3f, true);
        }

        private static void ApplyExplosionMaterials()
        {
            ApplyMaterial("Mat_Explosion_Core_Cream", "#FFF2CC", "#FF8A1E", 0.38f, true);
            ApplyMaterial("Mat_Explosion_Bubble_Cyan", "#2FE7D6", "#0DBCD1", 0.58f, true);
            ApplyMaterial("Mat_Explosion_Arm_Orange", "#FF9A3D", "#FF4F24", 0.42f, true);
            ApplyMaterial("Mat_Explosion_Spark_Pink", "#FF63B7", "#FF2D7A", 0.34f, true);
        }

        private static void ApplyItemMaterials()
        {
            ApplyMaterial("Mat_Item_Common_Glow_Cream", "#FFF2CC", "#FFD34D", 0.38f, true);
            ApplyMaterial("Mat_Item_BombCount_Body_Cyan", "#2FE7D6", "#0A6C78", 0.56f, true);
            ApplyMaterial("Mat_Item_BombCount_Icon_Cream", "#FFF2CC", "#FFD34D", 0.34f, true);
            ApplyMaterial("Mat_Item_BombCount_MiniBomb_Navy", "#24324A", "#081629", 0.46f, true);
            ApplyMaterial("Mat_Item_Range_Body_Orange", "#FF8A3D", "#FF4F24", 0.5f, true);
            ApplyMaterial("Mat_Item_Range_Icon_Yellow", "#FFD34D", "#FF8A1E", 0.34f, true);
            ApplyMaterial("Mat_Item_Range_Spark_Pink", "#FF63B7", "#FF2D7A", 0.32f, true);
            ApplyMaterial("Mat_Item_Speed_Body_Lime", "#83E66B", "#2D8F2F", 0.52f, true);
            ApplyMaterial("Mat_Item_Speed_Icon_White", "#FFF9E8", "#FFD34D", 0.3f, true);
            ApplyMaterial("Mat_Item_Speed_Wing_Cyan", "#2FE7D6", "#0A6C78", 0.42f, true);
            ApplyMaterial("Mat_Item_Shield_Body_Sky", "#67C7FF", "#0A6C78", 0.56f, true);
            ApplyMaterial("Mat_Item_Shield_Icon_White", "#FFF9E8", "#67C7FF", 0.32f, true);
            ApplyMaterial("Mat_Item_Shield_Spark_Cyan", "#2FE7D6", "#0DBCD1", 0.42f, true);
            ApplyMaterial("Mat_Item_Invincible_Body_Gold", "#FFD34D", "#FF9A1A", 0.56f, true);
            ApplyMaterial("Mat_Item_Invincible_Icon_Cream", "#FFF2CC", "#FFD34D", 0.34f, true);
            ApplyMaterial("Mat_Item_Invincible_Aura_Pink", "#FF63B7", "#FF2D7A", 0.45f, true);
        }

        private static void ApplyMaterial(string materialName, string albedoHex, string emissionHex, float smoothness, bool useEmission)
        {
            string materialPath = MaterialFolder + "/" + materialName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Debug.LogWarning($"[VisualStyleSetup] Material not found: {materialPath}");
                return;
            }

            Color albedo = ParseHtmlColor(albedoHex, Color.white);
            Color emission = ParseHtmlColor(emissionHex, Color.black);

            material.color = albedo;
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", Mathf.Clamp01(smoothness));
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                if (useEmission)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", emission);
                }
                else
                {
                    material.SetColor("_EmissionColor", Color.black);
                }
            }

            EditorUtility.SetDirty(material);
        }

        private static void ApplyBattleSceneLighting()
        {
            var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            Camera mainCamera = Camera.main != null ? Camera.main : Object.FindObjectOfType<Camera>();
            Light directionalLight = FindOrCreateDirectionalLight();

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ParseHtmlColor("#BFEFFF", Color.white);
            RenderSettings.ambientEquatorColor = ParseHtmlColor("#FFEBC2", Color.white);
            RenderSettings.ambientGroundColor = ParseHtmlColor("#7C7198", Color.gray);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.sun = directionalLight;

            if (directionalLight != null)
            {
                directionalLight.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
                directionalLight.color = ParseHtmlColor("#FFF4DD", Color.white);
                directionalLight.intensity = 1.18f;
                directionalLight.shadows = LightShadows.Soft;
                directionalLight.shadowStrength = 0.42f;
                EditorUtility.SetDirty(directionalLight);
            }

            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = ParseHtmlColor("#BDEEFF", Color.cyan);
                EditorUtility.SetDirty(mainCamera);
            }

            BattleVisualStyleController controller = EnsureVisualStyleController(mainCamera, directionalLight);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Light FindOrCreateDirectionalLight()
        {
            Light[] lights = Object.FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    return lights[i];
                }
            }

            GameObject lightObject = new GameObject("Directional Light");
            return lightObject.AddComponent<Light>();
        }

        private static BattleVisualStyleController EnsureVisualStyleController(Camera mainCamera, Light directionalLight)
        {
            GameObject root = GameObject.Find(VisualStyleRootName);
            if (root == null)
            {
                root = new GameObject(VisualStyleRootName);
            }

            GameObject battleRoot = GameObject.Find(BattleRootName);
            if (battleRoot != null && root.transform.parent != battleRoot.transform)
            {
                root.transform.SetParent(battleRoot.transform);
            }

            BattleVisualStyleController controller = root.GetComponent<BattleVisualStyleController>();
            if (controller == null)
            {
                controller = root.AddComponent<BattleVisualStyleController>();
            }

            SerializedObject serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("targetCamera").objectReferenceValue = mainCamera;
            serializedObject.FindProperty("directionalLight").objectReferenceValue = directionalLight;
            serializedObject.FindProperty("autoFindReferences").boolValue = true;
            serializedObject.FindProperty("directionalLightEuler").vector3Value = new Vector3(50f, -35f, 0f);
            serializedObject.FindProperty("directionalLightColor").colorValue = ParseHtmlColor("#FFF4DD", Color.white);
            serializedObject.FindProperty("directionalLightIntensity").floatValue = 1.18f;
            serializedObject.FindProperty("shadowStrength").floatValue = 0.42f;
            serializedObject.FindProperty("candySkyColor").colorValue = ParseHtmlColor("#BDEEFF", Color.cyan);
            serializedObject.FindProperty("candyAmbientSky").colorValue = ParseHtmlColor("#BFEFFF", Color.white);
            serializedObject.FindProperty("candyAmbientEquator").colorValue = ParseHtmlColor("#FFEBC2", Color.white);
            serializedObject.FindProperty("candyAmbientGround").colorValue = ParseHtmlColor("#9EB89E", Color.gray);
            serializedObject.FindProperty("jellySkyColor").colorValue = ParseHtmlColor("#2E336B", Color.blue);
            serializedObject.FindProperty("jellyAmbientSky").colorValue = ParseHtmlColor("#5A61B8", Color.blue);
            serializedObject.FindProperty("jellyAmbientEquator").colorValue = ParseHtmlColor("#479EC7", Color.cyan);
            serializedObject.FindProperty("jellyAmbientGround").colorValue = ParseHtmlColor("#261F42", Color.gray);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static Color ParseHtmlColor(string htmlColor, Color fallback)
        {
            return ColorUtility.TryParseHtmlString(htmlColor, out Color color) ? color : fallback;
        }
    }
}
