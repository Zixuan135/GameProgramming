using System;
using System.IO;
using BubbleTown.Characters;
using UnityEditor;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Builds the six original chibi character visual prefabs used by CharacterData.
    /// These prefabs are intentionally made from Unity primitives so they stay cheap
    /// to iterate on and can later be replaced by formal models under the same data assets.
    /// </summary>
    public static class CharacterRosterArtSetup
    {
        private const string MaterialFolder = "Assets/Materials/Characters";
        private const string RosterMaterialFolder = MaterialFolder + "/Roster";
        private const string CharacterPrefabFolder = "Assets/Prefabs/Characters";
        private const string CharacterDataFolder = "Assets/Resources/Characters";
        private const string RequestPath = "Temp/RunCharacterRosterArtSetup.request";
        private const string DonePath = "Temp/RunCharacterRosterArtSetup.done";
        private const string ErrorPath = "Temp/RunCharacterRosterArtSetup.error";

        private static readonly CharacterDefinition[] Characters =
        {
            new CharacterDefinition(
                "bubble_ranger",
                "Bubble Ranger",
                "Character_BubbleRanger",
                new Color(0.09f, 0.72f, 1f, 1f),
                new Color(0.96f, 0.98f, 0.42f, 1f)),
            new CharacterDefinition(
                "bear_blaster",
                "Bear Blaster",
                "Character_BearBlaster",
                new Color(1f, 0.28f, 0.22f, 1f),
                new Color(1f, 0.78f, 0.5f, 1f)),
            new CharacterDefinition(
                "frog_hopper",
                "Frog Hopper",
                "Character_FrogHopper",
                new Color(0.32f, 0.84f, 0.32f, 1f),
                new Color(0.88f, 1f, 0.52f, 1f)),
            new CharacterDefinition(
                "gear_kid",
                "Gear Kid",
                "Character_GearKid",
                new Color(1f, 0.78f, 0.16f, 1f),
                new Color(0.38f, 0.85f, 1f, 1f)),
            new CharacterDefinition(
                "bunny_pop",
                "Bunny Pop",
                "Character_BunnyPop",
                new Color(1f, 0.46f, 0.74f, 1f),
                new Color(1f, 0.88f, 0.95f, 1f)),
            new CharacterDefinition(
                "star_mage",
                "Star Mage",
                "Character_StarMage",
                new Color(0.56f, 0.38f, 1f, 1f),
                new Color(1f, 0.9f, 0.18f, 1f))
        };

        [InitializeOnLoadMethod]
        private static void RunRequestedSetupAfterCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(RequestPath))
                {
                    return;
                }

                try
                {
                    EnsureSixCharacterPrefabs();
                    File.WriteAllText(DonePath, DateTime.Now.ToString("O"));
                    if (File.Exists(ErrorPath))
                    {
                        File.Delete(ErrorPath);
                    }
                }
                catch (Exception exception)
                {
                    File.WriteAllText(ErrorPath, exception.ToString());
                    throw;
                }
                finally
                {
                    File.Delete(RequestPath);
                }
            };
        }

        [MenuItem("BubbleTown/Setup/Ensure Six Character Prefabs")]
        public static void EnsureSixCharacterPrefabs()
        {
            EnsureFolders();

            Material skin = EnsureMaterial("Mat_Char_Skin_Peach", new Color(1f, 0.82f, 0.62f, 1f), 0.18f);
            Material face = EnsureMaterial("Mat_Char_Face_Dark", new Color(0.08f, 0.2f, 0.28f, 1f), 0.05f);
            Material whiteFixed = EnsureMaterial("Mat_Char_WhiteFixed", Color.white, 0.2f);
            Material blackFixed = EnsureMaterial("Mat_Char_BlackFixed", new Color(0.06f, 0.12f, 0.16f, 1f), 0.06f);
            Material glass = EnsureTransparentMaterial("Mat_Char_Glass_Bubble", new Color(0.72f, 0.95f, 1f, 0.28f));
            Material bunnyInner = EnsureMaterial("Mat_Char_BunnyInnerFixed", new Color(1f, 0.86f, 0.94f, 1f), 0.2f);
            Material starFixed = EnsureMaterial("Mat_Char_StarFixed", new Color(1f, 0.9f, 0.18f, 1f), 0.25f);

            for (int i = 0; i < Characters.Length; i++)
            {
                CharacterDefinition definition = Characters[i];
                Material body = EnsureMaterial($"Mat_Char_{definition.PrefabName}_Body", definition.BodyColor, 0.28f);
                Material accent = EnsureMaterial($"Mat_Char_{definition.PrefabName}_Accent", definition.AccentColor, 0.22f);
                GameObject prefab = BuildCharacterPrefab(definition, skin, face, whiteFixed, blackFixed, glass, bunnyInner, starFixed, body, accent);
                UpdateCharacterData(definition, prefab);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterRosterArtSetup] Six chibi character prefabs are ready and linked to CharacterData.");
        }

        public static void EnsureSixCharacterPrefabsFromBatchmode()
        {
            EnsureSixCharacterPrefabs();
        }

        private static GameObject BuildCharacterPrefab(
            CharacterDefinition definition,
            Material skin,
            Material face,
            Material whiteFixed,
            Material blackFixed,
            Material glass,
            Material bunnyInner,
            Material starFixed,
            Material body,
            Material accent)
        {
            string prefabPath = $"{CharacterPrefabFolder}/{definition.PrefabName}.prefab";
            GameObject root = new GameObject(definition.PrefabName);
            Transform visualRoot = CreateChild(root.transform, "VisualRoot");

            CreateBaseBody(visualRoot, skin, face, body, accent);
            AddCharacterSpecificParts(visualRoot, definition.CharacterId, body, accent, face, whiteFixed, blackFixed, glass, bunnyInner, starFixed);
            ConfigureAnimator(root, visualRoot);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateBaseBody(Transform visualRoot, Material skin, Material face, Material body, Material accent)
        {
            CreatePart(visualRoot, "Body_ShortSuit", PrimitiveType.Capsule, new Vector3(0f, 0.4f, 0f), new Vector3(0.4f, 0.34f, 0.4f), body);
            CreatePart(visualRoot, "Head_BigRound", PrimitiveType.Sphere, new Vector3(0f, 0.91f, 0f), new Vector3(0.58f, 0.58f, 0.58f), skin);
            CreatePart(visualRoot, "Arm_Left", PrimitiveType.Capsule, new Vector3(-0.36f, 0.42f, 0.08f), new Vector3(0.11f, 0.2f, 0.11f), body, Quaternion.Euler(0f, 0f, 90f));
            CreatePart(visualRoot, "Arm_Right", PrimitiveType.Capsule, new Vector3(0.36f, 0.42f, 0.08f), new Vector3(0.11f, 0.2f, 0.11f), body, Quaternion.Euler(0f, 0f, 90f));
            CreatePart(visualRoot, "Foot_Left", PrimitiveType.Sphere, new Vector3(-0.17f, 0.09f, 0.12f), new Vector3(0.2f, 0.12f, 0.26f), body);
            CreatePart(visualRoot, "Foot_Right", PrimitiveType.Sphere, new Vector3(0.17f, 0.09f, 0.12f), new Vector3(0.2f, 0.12f, 0.26f), body);
            CreatePart(visualRoot, "Eye_Left", PrimitiveType.Cube, new Vector3(-0.12f, 0.96f, 0.29f), new Vector3(0.055f, 0.075f, 0.025f), face);
            CreatePart(visualRoot, "Eye_Right", PrimitiveType.Cube, new Vector3(0.12f, 0.96f, 0.29f), new Vector3(0.055f, 0.075f, 0.025f), face);
            CreatePart(visualRoot, "Mouth_Small", PrimitiveType.Cube, new Vector3(0f, 0.79f, 0.3f), new Vector3(0.13f, 0.035f, 0.025f), face);
            CreatePart(visualRoot, "FrontBadge_FacingMarker", PrimitiveType.Cube, new Vector3(0f, 0.45f, 0.24f), new Vector3(0.19f, 0.12f, 0.045f), accent);
        }

        private static void AddCharacterSpecificParts(
            Transform visualRoot,
            string characterId,
            Material body,
            Material accent,
            Material face,
            Material whiteFixed,
            Material blackFixed,
            Material glass,
            Material bunnyInner,
            Material starFixed)
        {
            switch (characterId)
            {
                case "bubble_ranger":
                    CreatePart(visualRoot, "BubbleHelmet_Glass", PrimitiveType.Sphere, new Vector3(0f, 0.93f, 0f), new Vector3(0.72f, 0.72f, 0.72f), glass);
                    CreatePart(visualRoot, "BubbleCap_Band", PrimitiveType.Cylinder, new Vector3(0f, 1.18f, 0.02f), new Vector3(0.38f, 0.055f, 0.38f), body);
                    CreatePart(visualRoot, "BubbleCap_TopDot", PrimitiveType.Sphere, new Vector3(0.16f, 1.28f, 0.04f), new Vector3(0.13f, 0.13f, 0.13f), accent);
                    CreatePart(visualRoot, "BubbleSide_Pop", PrimitiveType.Sphere, new Vector3(0.36f, 1.04f, 0.1f), new Vector3(0.14f, 0.14f, 0.14f), glass);
                    break;
                case "bear_blaster":
                    CreatePart(visualRoot, "BearHood_Top", PrimitiveType.Sphere, new Vector3(0f, 1.13f, 0f), new Vector3(0.5f, 0.22f, 0.46f), body);
                    CreatePart(visualRoot, "BearEar_Left", PrimitiveType.Sphere, new Vector3(-0.28f, 1.17f, 0.02f), new Vector3(0.21f, 0.21f, 0.21f), body);
                    CreatePart(visualRoot, "BearEar_Right", PrimitiveType.Sphere, new Vector3(0.28f, 1.17f, 0.02f), new Vector3(0.21f, 0.21f, 0.21f), body);
                    CreatePart(visualRoot, "BearEarInner_Left_Fixed", PrimitiveType.Sphere, new Vector3(-0.28f, 1.17f, 0.08f), new Vector3(0.1f, 0.1f, 0.06f), accent);
                    CreatePart(visualRoot, "BearEarInner_Right_Fixed", PrimitiveType.Sphere, new Vector3(0.28f, 1.17f, 0.08f), new Vector3(0.1f, 0.1f, 0.06f), accent);
                    break;
                case "frog_hopper":
                    CreatePart(visualRoot, "FrogHat_Cap", PrimitiveType.Sphere, new Vector3(0f, 1.13f, 0.02f), new Vector3(0.5f, 0.18f, 0.42f), body);
                    CreatePart(visualRoot, "FrogEyeBulb_Left", PrimitiveType.Sphere, new Vector3(-0.22f, 1.24f, 0.08f), new Vector3(0.19f, 0.19f, 0.19f), accent);
                    CreatePart(visualRoot, "FrogEyeBulb_Right", PrimitiveType.Sphere, new Vector3(0.22f, 1.24f, 0.08f), new Vector3(0.19f, 0.19f, 0.19f), accent);
                    CreatePart(visualRoot, "FrogPupil_Left_Fixed", PrimitiveType.Cube, new Vector3(-0.22f, 1.24f, 0.22f), new Vector3(0.045f, 0.045f, 0.02f), blackFixed);
                    CreatePart(visualRoot, "FrogPupil_Right_Fixed", PrimitiveType.Cube, new Vector3(0.22f, 1.24f, 0.22f), new Vector3(0.045f, 0.045f, 0.02f), blackFixed);
                    break;
                case "gear_kid":
                    CreatePart(visualRoot, "SafetyHelmet_Dome", PrimitiveType.Sphere, new Vector3(0f, 1.16f, 0.02f), new Vector3(0.52f, 0.22f, 0.44f), body);
                    CreatePart(visualRoot, "SafetyHelmet_Rim", PrimitiveType.Cube, new Vector3(0f, 1.07f, 0.2f), new Vector3(0.58f, 0.08f, 0.12f), body);
                    CreatePart(visualRoot, "Goggle_Left_Glass", PrimitiveType.Cube, new Vector3(-0.12f, 0.97f, 0.315f), new Vector3(0.12f, 0.075f, 0.025f), accent);
                    CreatePart(visualRoot, "Goggle_Right_Glass", PrimitiveType.Cube, new Vector3(0.12f, 0.97f, 0.315f), new Vector3(0.12f, 0.075f, 0.025f), accent);
                    CreatePart(visualRoot, "GoggleBridge_Fixed", PrimitiveType.Cube, new Vector3(0f, 0.97f, 0.32f), new Vector3(0.08f, 0.025f, 0.025f), face);
                    break;
                case "bunny_pop":
                    CreatePart(visualRoot, "BunnyHat_Top", PrimitiveType.Sphere, new Vector3(0f, 1.14f, 0f), new Vector3(0.5f, 0.2f, 0.42f), body);
                    CreatePart(visualRoot, "BunnyEar_Left", PrimitiveType.Capsule, new Vector3(-0.18f, 1.38f, 0.02f), new Vector3(0.12f, 0.34f, 0.12f), body, Quaternion.Euler(0f, 0f, -12f));
                    CreatePart(visualRoot, "BunnyEar_Right", PrimitiveType.Capsule, new Vector3(0.18f, 1.38f, 0.02f), new Vector3(0.12f, 0.34f, 0.12f), body, Quaternion.Euler(0f, 0f, 12f));
                    CreatePart(visualRoot, "BunnyEarInner_Left_Fixed", PrimitiveType.Capsule, new Vector3(-0.18f, 1.38f, 0.08f), new Vector3(0.055f, 0.24f, 0.055f), bunnyInner, Quaternion.Euler(0f, 0f, -12f));
                    CreatePart(visualRoot, "BunnyEarInner_Right_Fixed", PrimitiveType.Capsule, new Vector3(0.18f, 1.38f, 0.08f), new Vector3(0.055f, 0.24f, 0.055f), bunnyInner, Quaternion.Euler(0f, 0f, 12f));
                    break;
                case "star_mage":
                    CreatePart(visualRoot, "MageHat_Brim", PrimitiveType.Cylinder, new Vector3(0f, 1.11f, 0.03f), new Vector3(0.42f, 0.055f, 0.42f), body);
                    CreatePart(visualRoot, "MageHat_Top", PrimitiveType.Capsule, new Vector3(0.03f, 1.32f, 0.02f), new Vector3(0.16f, 0.34f, 0.16f), body, Quaternion.Euler(0f, 0f, -8f));
                    CreatePart(visualRoot, "MageHat_Band", PrimitiveType.Cube, new Vector3(0f, 1.12f, 0.26f), new Vector3(0.42f, 0.055f, 0.035f), accent);
                    CreatePart(visualRoot, "StarCharm_Vertical_Fixed", PrimitiveType.Cube, new Vector3(-0.14f, 1.28f, 0.2f), new Vector3(0.045f, 0.17f, 0.03f), starFixed);
                    CreatePart(visualRoot, "StarCharm_Horizontal_Fixed", PrimitiveType.Cube, new Vector3(-0.14f, 1.28f, 0.2f), new Vector3(0.17f, 0.045f, 0.03f), starFixed);
                    break;
            }
        }

        private static void ConfigureAnimator(GameObject root, Transform visualRoot)
        {
            CharacterVisualAnimator animator = root.AddComponent<CharacterVisualAnimator>();
            SerializedObject serializedAnimator = new SerializedObject(animator);
            SetObjectReference(serializedAnimator, "animatedRoot", visualRoot);
            SetFloat(serializedAnimator, "idleBobAmplitude", 0.034f);
            SetFloat(serializedAnimator, "idleBobSpeed", 3.35f);
            SetFloat(serializedAnimator, "moveBobAmplitude", 0.064f);
            SetFloat(serializedAnimator, "moveBobSpeed", 11f);
            SetFloat(serializedAnimator, "moveSwayDegrees", 7f);
            SetFloat(serializedAnimator, "moveScalePulse", 0.07f);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            SerializedProperty flashRenderers = serializedAnimator.FindProperty("flashRenderers");
            if (flashRenderers != null)
            {
                flashRenderers.arraySize = renderers.Length;
                for (int i = 0; i < renderers.Length; i++)
                {
                    flashRenderers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
                }
            }

            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(animator);
        }

        private static void UpdateCharacterData(CharacterDefinition definition, GameObject prefab)
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterData", new[] { CharacterDataFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (data == null || data.CharacterId != definition.CharacterId)
                {
                    continue;
                }

                SerializedObject serializedData = new SerializedObject(data);
                SetString(serializedData, "displayName", definition.DisplayName);
                SetObjectReference(serializedData, "prefab", prefab);
                SetObjectReference(serializedData, "icon", null);
                SetColor(serializedData, "themeColor", definition.BodyColor);
                SetFloat(serializedData, "moveSpeed", 4f);
                SetInt(serializedData, "maxBombCount", 1);
                SetInt(serializedData, "explosionRange", 2);
                serializedData.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(data);
                return;
            }

            Debug.LogWarning($"[CharacterRosterArtSetup] CharacterData for '{definition.CharacterId}' was not found.");
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child.transform;
        }

        private static GameObject CreatePart(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Quaternion? localRotation = null)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation ?? Quaternion.identity;
            part.transform.localScale = localScale;

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return part;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder(MaterialFolder, "Roster");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "Characters");
        }

        private static void EnsureFolder(string parentFolder, string childFolder)
        {
            string fullPath = parentFolder + "/" + childFolder;
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentFolder, childFolder);
            }
        }

        private static Material EnsureMaterial(string materialName, Color color, float smoothness)
        {
            string materialPath = $"{RosterMaterialFolder}/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                material.name = materialName;
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.color = color;
            SetStandardMaterialSurface(material, smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material EnsureTransparentMaterial(string materialName, Color color)
        {
            Material material = EnsureMaterial(materialName, color, 0.48f);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetStandardMaterialSurface(Material material, float smoothness)
        {
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.colorValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private readonly struct CharacterDefinition
        {
            public CharacterDefinition(string characterId, string displayName, string prefabName, Color bodyColor, Color accentColor)
            {
                CharacterId = characterId;
                DisplayName = displayName;
                PrefabName = prefabName;
                BodyColor = bodyColor;
                AccentColor = accentColor;
            }

            public string CharacterId { get; }
            public string DisplayName { get; }
            public string PrefabName { get; }
            public Color BodyColor { get; }
            public Color AccentColor { get; }
        }
    }
}
