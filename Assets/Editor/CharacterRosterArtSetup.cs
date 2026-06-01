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
            Material glass = EnsureTransparentMaterial("Mat_Char_Glass_Bubble", new Color(0.62f, 0.92f, 1f, 0.44f));
            Material bunnyInner = EnsureMaterial("Mat_Char_BunnyInnerFixed", new Color(1f, 0.86f, 0.94f, 1f), 0.2f);
            Material starFixed = EnsureMaterial("Mat_Char_StarFixed", new Color(1f, 0.9f, 0.18f, 1f), 0.25f);
            Material cheekFixed = EnsureMaterial("Mat_Char_CheekFixed", new Color(1f, 0.55f, 0.48f, 1f), 0.18f);
            Material mouthFixed = EnsureMaterial("Mat_Char_MouthFixed", new Color(1f, 0.36f, 0.28f, 1f), 0.16f);
            Material creamFixed = EnsureMaterial("Mat_Char_CreamFixed", new Color(1f, 0.94f, 0.74f, 1f), 0.24f);
            Material metalFixed = EnsureMaterial("Mat_Char_MetalFixed", new Color(0.66f, 0.68f, 0.66f, 1f), 0.36f);
            Material darkTrimFixed = EnsureMaterial("Mat_Char_DarkTrimFixed", new Color(0.18f, 0.11f, 0.07f, 1f), 0.18f);

            for (int i = 0; i < Characters.Length; i++)
            {
                CharacterDefinition definition = Characters[i];
                Material body = EnsureMaterial($"Mat_Char_{definition.PrefabName}_Body", definition.BodyColor, 0.25f);
                Material accent = EnsureMaterial($"Mat_Char_{definition.PrefabName}_Accent", definition.AccentColor, 0.22f);
                Material lineFixed = EnsureMaterial($"Mat_Char_{definition.PrefabName}_LineFixed", GetLineColor(definition.BodyColor), 0.08f);
                GameObject prefab = BuildCharacterPrefab(
                    definition,
                    skin,
                    face,
                    whiteFixed,
                    blackFixed,
                    glass,
                    bunnyInner,
                    starFixed,
                    cheekFixed,
                    mouthFixed,
                    creamFixed,
                    metalFixed,
                    darkTrimFixed,
                    lineFixed,
                    body,
                    accent);
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
            Material cheekFixed,
            Material mouthFixed,
            Material creamFixed,
            Material metalFixed,
            Material darkTrimFixed,
            Material lineFixed,
            Material body,
            Material accent)
        {
            string prefabPath = $"{CharacterPrefabFolder}/{definition.PrefabName}.prefab";
            GameObject root = new GameObject(definition.PrefabName);
            Transform visualRoot = CreateChild(root.transform, "VisualRoot");
            visualRoot.localScale = new Vector3(1.08f, 1.08f, 1.08f);

            CreateBaseBody(visualRoot, skin, face, body, accent, whiteFixed, blackFixed, cheekFixed, mouthFixed, darkTrimFixed, lineFixed);
            AddCharacterSpecificParts(
                visualRoot,
                definition.CharacterId,
                body,
                accent,
                face,
                whiteFixed,
                blackFixed,
                glass,
                bunnyInner,
                starFixed,
                cheekFixed,
                creamFixed,
                metalFixed,
                darkTrimFixed,
                lineFixed);
            ConfigureAnimator(root, visualRoot);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateBaseBody(
            Transform visualRoot,
            Material skin,
            Material face,
            Material body,
            Material accent,
            Material whiteFixed,
            Material blackFixed,
            Material cheekFixed,
            Material mouthFixed,
            Material darkTrimFixed,
            Material lineFixed)
        {
            CreatePart(visualRoot, "Body_OutlineFixed", PrimitiveType.Capsule, new Vector3(0f, 0.42f, -0.018f), new Vector3(0.50f, 0.39f, 0.46f), lineFixed);
            CreatePart(visualRoot, "Body_RoundedSuit", PrimitiveType.Capsule, new Vector3(0f, 0.43f, 0.02f), new Vector3(0.46f, 0.36f, 0.42f), body);
            CreatePart(visualRoot, "Body_TummyPatch_OutlineFixed", PrimitiveType.Sphere, new Vector3(0f, 0.48f, 0.315f), new Vector3(0.30f, 0.25f, 0.060f), lineFixed);
            CreatePart(visualRoot, "Body_TummyPatch", PrimitiveType.Sphere, new Vector3(0f, 0.49f, 0.35f), new Vector3(0.255f, 0.215f, 0.055f), accent);
            CreatePart(visualRoot, "Head_HoodOutlineFixed", PrimitiveType.Sphere, new Vector3(0f, 0.93f, -0.015f), new Vector3(0.70f, 0.66f, 0.62f), lineFixed);
            CreatePart(visualRoot, "Head_ColorHood", PrimitiveType.Sphere, new Vector3(0f, 0.94f, 0.025f), new Vector3(0.65f, 0.61f, 0.57f), body);
            CreatePart(visualRoot, "Face_WindowOutlineFixed", PrimitiveType.Sphere, new Vector3(0f, 0.90f, 0.365f), new Vector3(0.49f, 0.39f, 0.082f), lineFixed);
            CreatePart(visualRoot, "Face_RoundedWindow", PrimitiveType.Sphere, new Vector3(0f, 0.90f, 0.410f), new Vector3(0.43f, 0.34f, 0.070f), skin);
            CreatePart(visualRoot, "HoodLowerLip_Fixed", PrimitiveType.Cube, new Vector3(0f, 0.704f, 0.418f), new Vector3(0.35f, 0.030f, 0.030f), lineFixed);

            CreatePart(visualRoot, "Arm_Left_Upper", PrimitiveType.Capsule, new Vector3(-0.38f, 0.48f, 0.10f), new Vector3(0.12f, 0.22f, 0.12f), body, Quaternion.Euler(0f, 0f, 70f));
            CreatePart(visualRoot, "Arm_Right_Upper", PrimitiveType.Capsule, new Vector3(0.38f, 0.48f, 0.10f), new Vector3(0.12f, 0.22f, 0.12f), body, Quaternion.Euler(0f, 0f, -70f));
            CreatePart(visualRoot, "Hand_Left_OutlineFixed", PrimitiveType.Sphere, new Vector3(-0.49f, 0.39f, 0.18f), new Vector3(0.17f, 0.15f, 0.14f), lineFixed);
            CreatePart(visualRoot, "Hand_Right_OutlineFixed", PrimitiveType.Sphere, new Vector3(0.49f, 0.39f, 0.18f), new Vector3(0.17f, 0.15f, 0.14f), lineFixed);
            CreatePart(visualRoot, "Hand_Left_Glove", PrimitiveType.Sphere, new Vector3(-0.49f, 0.39f, 0.22f), new Vector3(0.15f, 0.13f, 0.13f), body);
            CreatePart(visualRoot, "Hand_Right_Glove", PrimitiveType.Sphere, new Vector3(0.49f, 0.39f, 0.22f), new Vector3(0.15f, 0.13f, 0.13f), body);
            CreatePart(visualRoot, "Leg_Left", PrimitiveType.Capsule, new Vector3(-0.16f, 0.18f, 0.02f), new Vector3(0.13f, 0.18f, 0.13f), body);
            CreatePart(visualRoot, "Leg_Right", PrimitiveType.Capsule, new Vector3(0.16f, 0.18f, 0.02f), new Vector3(0.13f, 0.18f, 0.13f), body);
            CreatePart(visualRoot, "Boot_Left", PrimitiveType.Sphere, new Vector3(-0.18f, 0.07f, 0.16f), new Vector3(0.23f, 0.12f, 0.28f), body);
            CreatePart(visualRoot, "Boot_Right", PrimitiveType.Sphere, new Vector3(0.18f, 0.07f, 0.16f), new Vector3(0.23f, 0.12f, 0.28f), body);
            CreatePart(visualRoot, "BootSole_Left_Fixed", PrimitiveType.Cube, new Vector3(-0.18f, 0.015f, 0.17f), new Vector3(0.25f, 0.025f, 0.27f), darkTrimFixed);
            CreatePart(visualRoot, "BootSole_Right_Fixed", PrimitiveType.Cube, new Vector3(0.18f, 0.015f, 0.17f), new Vector3(0.25f, 0.025f, 0.27f), darkTrimFixed);

            CreatePart(visualRoot, "EyeWhite_Left_Fixed", PrimitiveType.Cube, new Vector3(-0.13f, 0.97f, 0.465f), new Vector3(0.086f, 0.125f, 0.022f), whiteFixed);
            CreatePart(visualRoot, "EyeWhite_Right_Fixed", PrimitiveType.Cube, new Vector3(0.13f, 0.97f, 0.465f), new Vector3(0.086f, 0.125f, 0.022f), whiteFixed);
            CreatePart(visualRoot, "Eye_Left", PrimitiveType.Cube, new Vector3(-0.13f, 0.965f, 0.493f), new Vector3(0.045f, 0.095f, 0.018f), face);
            CreatePart(visualRoot, "Eye_Right", PrimitiveType.Cube, new Vector3(0.13f, 0.965f, 0.493f), new Vector3(0.045f, 0.095f, 0.018f), face);
            CreatePart(visualRoot, "Cheek_Left_Fixed", PrimitiveType.Sphere, new Vector3(-0.235f, 0.83f, 0.468f), new Vector3(0.085f, 0.050f, 0.018f), cheekFixed);
            CreatePart(visualRoot, "Cheek_Right_Fixed", PrimitiveType.Sphere, new Vector3(0.235f, 0.83f, 0.468f), new Vector3(0.085f, 0.050f, 0.018f), cheekFixed);
            CreatePart(visualRoot, "Mouth_SmileDark", PrimitiveType.Cube, new Vector3(0f, 0.755f, 0.482f), new Vector3(0.15f, 0.048f, 0.018f), face);
            CreatePart(visualRoot, "Mouth_Tongue_Fixed", PrimitiveType.Cube, new Vector3(0.025f, 0.738f, 0.505f), new Vector3(0.076f, 0.022f, 0.012f), mouthFixed);
            CreatePart(visualRoot, "Belt_Fixed", PrimitiveType.Cube, new Vector3(0f, 0.31f, 0.25f), new Vector3(0.48f, 0.065f, 0.05f), darkTrimFixed);
            CreatePart(visualRoot, "BeltBuckle", PrimitiveType.Cube, new Vector3(0f, 0.31f, 0.30f), new Vector3(0.13f, 0.09f, 0.04f), accent);
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
            Material starFixed,
            Material cheekFixed,
            Material creamFixed,
            Material metalFixed,
            Material darkTrimFixed,
            Material lineFixed)
        {
            switch (characterId)
            {
                case "bubble_ranger":
                    AddBackpack(visualRoot, "BubbleTank", new Vector3(0.31f, 0.56f, -0.19f), glass, accent, darkTrimFixed);
                    CreatePart(visualRoot, "BubbleHelmet_RingOutline_Fixed", PrimitiveType.Cylinder, new Vector3(0f, 1.12f, 0.045f), new Vector3(0.43f, 0.060f, 0.43f), lineFixed);
                    CreatePart(visualRoot, "BubbleHelmet_Dome_Glass", PrimitiveType.Sphere, new Vector3(0f, 1.34f, 0.06f), new Vector3(0.43f, 0.43f, 0.43f), glass);
                    CreatePart(visualRoot, "BubbleHelmet_Ring", PrimitiveType.Cylinder, new Vector3(0f, 1.12f, 0.08f), new Vector3(0.39f, 0.055f, 0.39f), body);
                    CreatePart(visualRoot, "BubbleHelmet_Shine_Fixed", PrimitiveType.Sphere, new Vector3(-0.15f, 1.48f, 0.35f), new Vector3(0.10f, 0.075f, 0.025f), whiteFixed);
                    CreatePart(visualRoot, "BubbleHelmet_SmallShine_Fixed", PrimitiveType.Sphere, new Vector3(0.16f, 1.43f, 0.35f), new Vector3(0.050f, 0.038f, 0.016f), whiteFixed);
                    CreatePart(visualRoot, "BubbleEarPad_Left", PrimitiveType.Sphere, new Vector3(-0.40f, 0.98f, 0.12f), new Vector3(0.14f, 0.18f, 0.12f), glass);
                    CreatePart(visualRoot, "BubbleEarPad_Right", PrimitiveType.Sphere, new Vector3(0.40f, 0.98f, 0.12f), new Vector3(0.14f, 0.18f, 0.12f), glass);
                    CreatePart(visualRoot, "BubbleCannon_BarrelOutline_Fixed", PrimitiveType.Cylinder, new Vector3(-0.60f, 0.58f, 0.36f), new Vector3(0.16f, 0.29f, 0.16f), lineFixed, Quaternion.Euler(90f, 0f, 0f));
                    CreatePart(visualRoot, "BubbleCannon_Barrel", PrimitiveType.Cylinder, new Vector3(-0.60f, 0.58f, 0.40f), new Vector3(0.135f, 0.27f, 0.135f), body, Quaternion.Euler(90f, 0f, 0f));
                    CreatePart(visualRoot, "BubbleCannon_Muzzle_Glass", PrimitiveType.Cylinder, new Vector3(-0.60f, 0.58f, 0.65f), new Vector3(0.17f, 0.060f, 0.17f), glass, Quaternion.Euler(90f, 0f, 0f));
                    CreatePart(visualRoot, "BubbleCannon_Core", PrimitiveType.Sphere, new Vector3(-0.60f, 0.58f, 0.70f), new Vector3(0.105f, 0.105f, 0.035f), accent);
                    CreatePart(visualRoot, "BubbleCannon_Grip", PrimitiveType.Cube, new Vector3(-0.52f, 0.40f, 0.36f), new Vector3(0.08f, 0.18f, 0.08f), darkTrimFixed);
                    break;
                case "bear_blaster":
                    AddBackpack(visualRoot, "BearRocketPack", new Vector3(0.31f, 0.52f, -0.18f), body, accent, darkTrimFixed);
                    CreatePart(visualRoot, "BearEar_Left_OutlineFixed", PrimitiveType.Sphere, new Vector3(-0.31f, 1.25f, 0.00f), new Vector3(0.245f, 0.245f, 0.205f), lineFixed);
                    CreatePart(visualRoot, "BearEar_Right_OutlineFixed", PrimitiveType.Sphere, new Vector3(0.31f, 1.25f, 0.00f), new Vector3(0.245f, 0.245f, 0.205f), lineFixed);
                    CreatePart(visualRoot, "BearEar_Left", PrimitiveType.Sphere, new Vector3(-0.31f, 1.25f, 0.035f), new Vector3(0.22f, 0.22f, 0.19f), body);
                    CreatePart(visualRoot, "BearEar_Right", PrimitiveType.Sphere, new Vector3(0.31f, 1.25f, 0.035f), new Vector3(0.22f, 0.22f, 0.19f), body);
                    CreatePart(visualRoot, "BearEarInner_Left_Fixed", PrimitiveType.Sphere, new Vector3(-0.31f, 1.25f, 0.16f), new Vector3(0.115f, 0.105f, 0.045f), accent);
                    CreatePart(visualRoot, "BearEarInner_Right_Fixed", PrimitiveType.Sphere, new Vector3(0.31f, 1.25f, 0.16f), new Vector3(0.115f, 0.105f, 0.045f), accent);
                    CreatePart(visualRoot, "BearMuzzle_OutlineFixed", PrimitiveType.Sphere, new Vector3(0f, 1.10f, 0.465f), new Vector3(0.21f, 0.13f, 0.035f), lineFixed);
                    CreatePart(visualRoot, "BearMuzzle_Fixed", PrimitiveType.Sphere, new Vector3(0f, 1.10f, 0.50f), new Vector3(0.18f, 0.11f, 0.035f), creamFixed);
                    CreatePart(visualRoot, "BearNose_Fixed", PrimitiveType.Sphere, new Vector3(0f, 1.13f, 0.535f), new Vector3(0.050f, 0.033f, 0.018f), darkTrimFixed);
                    AddPawPrint(visualRoot, "BearChestPaw_Fixed", new Vector3(0f, 0.56f, 0.385f), 0.085f, creamFixed);
                    AddPawPrint(visualRoot, "BearBlasterPaw_Fixed", new Vector3(-0.62f, 0.61f, 0.72f), 0.060f, creamFixed);
                    CreatePart(visualRoot, "BearBlaster_BarrelOutline_Fixed", PrimitiveType.Cylinder, new Vector3(-0.62f, 0.58f, 0.39f), new Vector3(0.19f, 0.32f, 0.19f), lineFixed, Quaternion.Euler(90f, 0f, 0f));
                    CreatePart(visualRoot, "BearBlaster_Barrel", PrimitiveType.Cylinder, new Vector3(-0.62f, 0.58f, 0.43f), new Vector3(0.165f, 0.30f, 0.165f), body, Quaternion.Euler(90f, 0f, 0f));
                    CreatePart(visualRoot, "BearBlaster_Muzzle", PrimitiveType.Cylinder, new Vector3(-0.62f, 0.58f, 0.72f), new Vector3(0.165f, 0.060f, 0.165f), accent, Quaternion.Euler(90f, 0f, 0f));
                    CreatePart(visualRoot, "BearBlaster_Grip", PrimitiveType.Cube, new Vector3(-0.53f, 0.39f, 0.38f), new Vector3(0.08f, 0.19f, 0.08f), darkTrimFixed);
                    CreatePart(visualRoot, "BearBeltPouch_Fixed", PrimitiveType.Cube, new Vector3(0.27f, 0.29f, 0.35f), new Vector3(0.19f, 0.14f, 0.08f), creamFixed);
                    break;
                case "frog_hopper":
                    AddBackpack(visualRoot, "FrogJumpTank", new Vector3(0.33f, 0.54f, -0.18f), accent, body, darkTrimFixed);
                    CreatePart(visualRoot, "FrogEyeBulb_Left_OutlineFixed", PrimitiveType.Sphere, new Vector3(-0.26f, 1.28f, 0.045f), new Vector3(0.235f, 0.235f, 0.195f), lineFixed);
                    CreatePart(visualRoot, "FrogEyeBulb_Right_OutlineFixed", PrimitiveType.Sphere, new Vector3(0.26f, 1.28f, 0.045f), new Vector3(0.235f, 0.235f, 0.195f), lineFixed);
                    CreatePart(visualRoot, "FrogEyeBulb_Left", PrimitiveType.Sphere, new Vector3(-0.26f, 1.28f, 0.085f), new Vector3(0.205f, 0.205f, 0.180f), body);
                    CreatePart(visualRoot, "FrogEyeBulb_Right", PrimitiveType.Sphere, new Vector3(0.26f, 1.28f, 0.085f), new Vector3(0.205f, 0.205f, 0.180f), body);
                    CreatePart(visualRoot, "FrogEyeWhite_Left_Fixed", PrimitiveType.Sphere, new Vector3(-0.26f, 1.28f, 0.225f), new Vector3(0.135f, 0.135f, 0.035f), whiteFixed);
                    CreatePart(visualRoot, "FrogEyeWhite_Right_Fixed", PrimitiveType.Sphere, new Vector3(0.26f, 1.28f, 0.225f), new Vector3(0.135f, 0.135f, 0.035f), whiteFixed);
                    CreatePart(visualRoot, "FrogPupil_Left_Fixed", PrimitiveType.Sphere, new Vector3(-0.26f, 1.28f, 0.265f), new Vector3(0.065f, 0.065f, 0.018f), blackFixed);
                    CreatePart(visualRoot, "FrogPupil_Right_Fixed", PrimitiveType.Sphere, new Vector3(0.26f, 1.28f, 0.265f), new Vector3(0.065f, 0.065f, 0.018f), blackFixed);
                    CreatePart(visualRoot, "FrogNostril_Left_Fixed", PrimitiveType.Sphere, new Vector3(-0.060f, 1.16f, 0.47f), new Vector3(0.026f, 0.026f, 0.012f), darkTrimFixed);
                    CreatePart(visualRoot, "FrogNostril_Right_Fixed", PrimitiveType.Sphere, new Vector3(0.060f, 1.16f, 0.47f), new Vector3(0.026f, 0.026f, 0.012f), darkTrimFixed);
                    CreatePart(visualRoot, "FrogSmile_Fixed", PrimitiveType.Cube, new Vector3(0f, 1.075f, 0.485f), new Vector3(0.13f, 0.020f, 0.014f), darkTrimFixed);
                    CreatePart(visualRoot, "FrogPogo_Handle", PrimitiveType.Cylinder, new Vector3(-0.64f, 0.64f, 0.30f), new Vector3(0.040f, 0.23f, 0.040f), accent, Quaternion.Euler(0f, 0f, 90f));
                    CreatePart(visualRoot, "FrogPogo_Post_Fixed", PrimitiveType.Cylinder, new Vector3(-0.64f, 0.36f, 0.28f), new Vector3(0.040f, 0.34f, 0.040f), metalFixed);
                    for (int i = 0; i < 5; i++)
                    {
                        CreatePart(visualRoot, $"FrogPogo_Spring_{i}_Fixed", PrimitiveType.Cylinder, new Vector3(-0.64f, 0.22f + i * 0.057f, 0.28f), new Vector3(0.085f, 0.013f, 0.085f), darkTrimFixed);
                    }
                    CreatePart(visualRoot, "FrogPogo_Foot", PrimitiveType.Cylinder, new Vector3(-0.64f, 0.09f, 0.28f), new Vector3(0.15f, 0.038f, 0.15f), accent);
                    break;
                case "gear_kid":
                    AddBackpack(visualRoot, "GearToolPack", new Vector3(0.34f, 0.54f, -0.18f), metalFixed, body, darkTrimFixed);
                    CreatePart(visualRoot, "SafetyHelmet_DomeOutline_Fixed", PrimitiveType.Sphere, new Vector3(0f, 1.20f, 0.00f), new Vector3(0.54f, 0.27f, 0.47f), lineFixed);
                    CreatePart(visualRoot, "SafetyHelmet_Dome", PrimitiveType.Sphere, new Vector3(0f, 1.20f, 0.04f), new Vector3(0.50f, 0.24f, 0.44f), body);
                    CreatePart(visualRoot, "SafetyHelmet_FrontRimOutline_Fixed", PrimitiveType.Cube, new Vector3(0f, 1.08f, 0.285f), new Vector3(0.66f, 0.085f, 0.145f), lineFixed);
                    CreatePart(visualRoot, "SafetyHelmet_FrontRim", PrimitiveType.Cube, new Vector3(0f, 1.08f, 0.335f), new Vector3(0.60f, 0.075f, 0.13f), body);
                    CreatePart(visualRoot, "SafetyHelmet_CenterStripe", PrimitiveType.Cube, new Vector3(0f, 1.25f, 0.13f), new Vector3(0.09f, 0.085f, 0.46f), accent);
                    AddGearBadge(visualRoot, "HelmetGear_Fixed", new Vector3(0f, 1.22f, 0.41f), 0.098f, creamFixed, accent);
                    AddGearBadge(visualRoot, "ChestGear_Fixed", new Vector3(0f, 0.55f, 0.395f), 0.105f, creamFixed, accent);
                    CreatePart(visualRoot, "GearKid_EarPad_Left", PrimitiveType.Sphere, new Vector3(-0.40f, 0.98f, 0.12f), new Vector3(0.14f, 0.17f, 0.10f), accent);
                    CreatePart(visualRoot, "GearKid_EarPad_Right", PrimitiveType.Sphere, new Vector3(0.40f, 0.98f, 0.12f), new Vector3(0.14f, 0.17f, 0.10f), accent);
                    CreatePart(visualRoot, "Wrench_Handle_OutlineFixed", PrimitiveType.Cylinder, new Vector3(-0.62f, 0.46f, 0.32f), new Vector3(0.060f, 0.36f, 0.060f), lineFixed, Quaternion.Euler(0f, 0f, -20f));
                    CreatePart(visualRoot, "Wrench_Handle_Fixed", PrimitiveType.Cylinder, new Vector3(-0.62f, 0.46f, 0.37f), new Vector3(0.047f, 0.34f, 0.047f), metalFixed, Quaternion.Euler(0f, 0f, -20f));
                    CreatePart(visualRoot, "Wrench_Head_Left_Fixed", PrimitiveType.Cube, new Vector3(-0.72f, 0.74f, 0.37f), new Vector3(0.10f, 0.18f, 0.060f), metalFixed, Quaternion.Euler(0f, 0f, -25f));
                    CreatePart(visualRoot, "Wrench_Head_Right_Fixed", PrimitiveType.Cube, new Vector3(-0.56f, 0.76f, 0.37f), new Vector3(0.10f, 0.18f, 0.060f), metalFixed, Quaternion.Euler(0f, 0f, 25f));
                    CreatePart(visualRoot, "GearBeltPouch", PrimitiveType.Cube, new Vector3(0.29f, 0.28f, 0.35f), new Vector3(0.18f, 0.15f, 0.08f), accent);
                    break;
                case "bunny_pop":
                    AddBackpack(visualRoot, "BunnyBubblePack", new Vector3(0.32f, 0.54f, -0.18f), body, accent, darkTrimFixed);
                    CreatePart(visualRoot, "BunnyEar_Left_OutlineFixed", PrimitiveType.Capsule, new Vector3(-0.22f, 1.45f, -0.005f), new Vector3(0.155f, 0.43f, 0.13f), lineFixed, Quaternion.Euler(0f, 0f, -13f));
                    CreatePart(visualRoot, "BunnyEar_Right_OutlineFixed", PrimitiveType.Capsule, new Vector3(0.22f, 1.45f, -0.005f), new Vector3(0.155f, 0.43f, 0.13f), lineFixed, Quaternion.Euler(0f, 0f, 13f));
                    CreatePart(visualRoot, "BunnyEar_Left", PrimitiveType.Capsule, new Vector3(-0.22f, 1.45f, 0.04f), new Vector3(0.13f, 0.40f, 0.12f), body, Quaternion.Euler(0f, 0f, -13f));
                    CreatePart(visualRoot, "BunnyEar_Right", PrimitiveType.Capsule, new Vector3(0.22f, 1.45f, 0.04f), new Vector3(0.13f, 0.40f, 0.12f), body, Quaternion.Euler(0f, 0f, 13f));
                    CreatePart(visualRoot, "BunnyEarInner_Left_Fixed", PrimitiveType.Capsule, new Vector3(-0.22f, 1.45f, 0.13f), new Vector3(0.066f, 0.29f, 0.050f), bunnyInner, Quaternion.Euler(0f, 0f, -13f));
                    CreatePart(visualRoot, "BunnyEarInner_Right_Fixed", PrimitiveType.Capsule, new Vector3(0.22f, 1.45f, 0.13f), new Vector3(0.066f, 0.29f, 0.050f), bunnyInner, Quaternion.Euler(0f, 0f, 13f));
                    CreatePart(visualRoot, "BunnyMuzzle_OutlineFixed", PrimitiveType.Sphere, new Vector3(0f, 1.12f, 0.465f), new Vector3(0.19f, 0.12f, 0.035f), lineFixed);
                    CreatePart(visualRoot, "BunnyMuzzle_Fixed", PrimitiveType.Sphere, new Vector3(0f, 1.12f, 0.50f), new Vector3(0.16f, 0.10f, 0.035f), bunnyInner);
                    CreatePart(visualRoot, "BunnyBow_Left", PrimitiveType.Sphere, new Vector3(-0.10f, 0.63f, 0.39f), new Vector3(0.12f, 0.088f, 0.045f), accent, Quaternion.Euler(0f, 0f, -20f));
                    CreatePart(visualRoot, "BunnyBow_Right", PrimitiveType.Sphere, new Vector3(0.10f, 0.63f, 0.39f), new Vector3(0.12f, 0.088f, 0.045f), accent, Quaternion.Euler(0f, 0f, 20f));
                    CreatePart(visualRoot, "BunnyBow_Center", PrimitiveType.Sphere, new Vector3(0f, 0.63f, 0.43f), new Vector3(0.062f, 0.062f, 0.035f), accent);
                    CreatePart(visualRoot, "BunnyBlaster_BarrelOutline_Fixed", PrimitiveType.Cylinder, new Vector3(-0.62f, 0.58f, 0.39f), new Vector3(0.16f, 0.30f, 0.16f), lineFixed, Quaternion.Euler(90f, 0f, 0f));
                    CreatePart(visualRoot, "BunnyBlaster_Barrel", PrimitiveType.Cylinder, new Vector3(-0.62f, 0.58f, 0.43f), new Vector3(0.13f, 0.28f, 0.13f), body, Quaternion.Euler(90f, 0f, 0f));
                    CreatePart(visualRoot, "BunnyBlaster_Muzzle", PrimitiveType.Sphere, new Vector3(-0.62f, 0.58f, 0.70f), new Vector3(0.14f, 0.14f, 0.055f), accent);
                    CreatePart(visualRoot, "BunnyBlaster_Head", PrimitiveType.Sphere, new Vector3(-0.62f, 0.76f, 0.58f), new Vector3(0.11f, 0.080f, 0.055f), bunnyInner);
                    CreatePart(visualRoot, "BunnyPouch", PrimitiveType.Cube, new Vector3(0.26f, 0.30f, 0.36f), new Vector3(0.18f, 0.15f, 0.08f), accent);
                    break;
                case "star_mage":
                    CreatePart(visualRoot, "MageHat_BrimOutline_Fixed", PrimitiveType.Cylinder, new Vector3(0f, 1.12f, 0.045f), new Vector3(0.58f, 0.065f, 0.50f), lineFixed);
                    CreatePart(visualRoot, "MageHat_Brim", PrimitiveType.Cylinder, new Vector3(0f, 1.12f, 0.095f), new Vector3(0.53f, 0.055f, 0.46f), body);
                    CreatePart(visualRoot, "MageHat_BrimFrontOutline_Fixed", PrimitiveType.Cube, new Vector3(0f, 1.10f, 0.37f), new Vector3(0.72f, 0.072f, 0.13f), lineFixed);
                    CreatePart(visualRoot, "MageHat_BrimFront", PrimitiveType.Cube, new Vector3(0f, 1.10f, 0.42f), new Vector3(0.66f, 0.060f, 0.12f), body);
                    CreatePart(visualRoot, "MageHat_TopOutline_Fixed", PrimitiveType.Capsule, new Vector3(0.08f, 1.39f, 0.00f), new Vector3(0.205f, 0.43f, 0.20f), lineFixed, Quaternion.Euler(0f, 0f, -14f));
                    CreatePart(visualRoot, "MageHat_Top", PrimitiveType.Capsule, new Vector3(0.08f, 1.39f, 0.04f), new Vector3(0.18f, 0.40f, 0.18f), body, Quaternion.Euler(0f, 0f, -14f));
                    CreatePart(visualRoot, "MageHat_Tip", PrimitiveType.Sphere, new Vector3(0.22f, 1.63f, 0.055f), new Vector3(0.14f, 0.085f, 0.10f), body);
                    CreatePart(visualRoot, "MageHat_Band", PrimitiveType.Cube, new Vector3(0f, 1.13f, 0.395f), new Vector3(0.50f, 0.058f, 0.045f), accent);
                    AddStarBadge(visualRoot, "MageHatStar_Fixed", new Vector3(-0.15f, 1.28f, 0.43f), 0.120f, starFixed);
                    AddStarBadge(visualRoot, "MageChestStar_Fixed", new Vector3(0f, 0.56f, 0.395f), 0.120f, starFixed);
                    CreatePart(visualRoot, "MageCape_Back", PrimitiveType.Cube, new Vector3(0f, 0.46f, -0.25f), new Vector3(0.56f, 0.58f, 0.07f), lineFixed);
                    CreatePart(visualRoot, "MageCape_Front", PrimitiveType.Cube, new Vector3(0f, 0.46f, -0.20f), new Vector3(0.51f, 0.54f, 0.055f), body);
                    CreatePart(visualRoot, "MageStaff_Shaft", PrimitiveType.Cylinder, new Vector3(-0.63f, 0.46f, 0.33f), new Vector3(0.040f, 0.43f, 0.040f), body);
                    CreatePart(visualRoot, "MageStaff_Orb", PrimitiveType.Sphere, new Vector3(-0.63f, 0.91f, 0.34f), new Vector3(0.18f, 0.18f, 0.18f), accent);
                    AddStarBadge(visualRoot, "MageStaffStar_Fixed", new Vector3(-0.63f, 0.93f, 0.50f), 0.115f, starFixed);
                    CreatePart(visualRoot, "MageBook", PrimitiveType.Cube, new Vector3(0.29f, 0.29f, 0.35f), new Vector3(0.18f, 0.19f, 0.08f), body);
                    CreatePart(visualRoot, "MageBook_Clasp_Fixed", PrimitiveType.Cube, new Vector3(0.35f, 0.29f, 0.41f), new Vector3(0.038f, 0.17f, 0.025f), starFixed);
                    break;
            }
        }

        private static void AddBackpack(
            Transform parent,
            string prefix,
            Vector3 localPosition,
            Material shell,
            Material accent,
            Material strap)
        {
            CreatePart(parent, prefix + "_Shell", PrimitiveType.Cube, localPosition, new Vector3(0.22f, 0.34f, 0.16f), shell);
            CreatePart(parent, prefix + "_Cap", PrimitiveType.Cylinder, localPosition + new Vector3(0f, 0.20f, 0.01f), new Vector3(0.12f, 0.035f, 0.12f), accent);
            CreatePart(parent, prefix + "_Nozzle_Fixed", PrimitiveType.Cylinder, localPosition + new Vector3(0f, -0.01f, 0.11f), new Vector3(0.06f, 0.055f, 0.06f), strap, Quaternion.Euler(90f, 0f, 0f));
            CreatePart(parent, prefix + "_StrapTop_Fixed", PrimitiveType.Cube, localPosition + new Vector3(-0.16f, 0.10f, 0.13f), new Vector3(0.045f, 0.22f, 0.035f), strap);
            CreatePart(parent, prefix + "_StrapBottom_Fixed", PrimitiveType.Cube, localPosition + new Vector3(-0.16f, -0.12f, 0.13f), new Vector3(0.045f, 0.16f, 0.035f), strap);
        }

        private static void AddPawPrint(Transform parent, string prefix, Vector3 localPosition, float size, Material material)
        {
            CreatePart(parent, prefix + "_Pad", PrimitiveType.Sphere, localPosition, new Vector3(size * 1.10f, size * 0.78f, size * 0.26f), material);
            CreatePart(parent, prefix + "_ToeLeft", PrimitiveType.Sphere, localPosition + new Vector3(-size * 0.72f, size * 0.82f, size * 0.05f), new Vector3(size * 0.42f, size * 0.42f, size * 0.18f), material);
            CreatePart(parent, prefix + "_ToeMid", PrimitiveType.Sphere, localPosition + new Vector3(0f, size * 1.02f, size * 0.05f), new Vector3(size * 0.44f, size * 0.44f, size * 0.18f), material);
            CreatePart(parent, prefix + "_ToeRight", PrimitiveType.Sphere, localPosition + new Vector3(size * 0.72f, size * 0.82f, size * 0.05f), new Vector3(size * 0.42f, size * 0.42f, size * 0.18f), material);
        }

        private static void AddStarBadge(Transform parent, string prefix, Vector3 localPosition, float size, Material material)
        {
            CreatePart(parent, prefix + "_Center", PrimitiveType.Sphere, localPosition, new Vector3(size * 0.62f, size * 0.62f, size * 0.20f), material);
            CreatePart(parent, prefix + "_Vertical", PrimitiveType.Cube, localPosition, new Vector3(size * 0.44f, size * 1.65f, size * 0.18f), material);
            CreatePart(parent, prefix + "_Horizontal", PrimitiveType.Cube, localPosition, new Vector3(size * 1.65f, size * 0.44f, size * 0.18f), material);
            CreatePart(parent, prefix + "_SlashA", PrimitiveType.Cube, localPosition, new Vector3(size * 1.32f, size * 0.34f, size * 0.18f), material, Quaternion.Euler(0f, 0f, 45f));
            CreatePart(parent, prefix + "_SlashB", PrimitiveType.Cube, localPosition, new Vector3(size * 1.32f, size * 0.34f, size * 0.18f), material, Quaternion.Euler(0f, 0f, -45f));
        }

        private static void AddGearBadge(
            Transform parent,
            string prefix,
            Vector3 localPosition,
            float size,
            Material toothMaterial,
            Material centerMaterial)
        {
            CreatePart(parent, prefix + "_Ring", PrimitiveType.Cylinder, localPosition, new Vector3(size, size * 0.16f, size), toothMaterial, Quaternion.Euler(90f, 0f, 0f));
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 0.25f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * size * 0.82f, Mathf.Sin(angle) * size * 0.82f, 0.01f);
                CreatePart(
                    parent,
                    prefix + "_Tooth" + i,
                    PrimitiveType.Cube,
                    localPosition + offset,
                    new Vector3(size * 0.38f, size * 0.16f, size * 0.16f),
                    toothMaterial,
                    Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg));
            }

            CreatePart(parent, prefix + "_Center", PrimitiveType.Cylinder, localPosition + new Vector3(0f, 0f, 0.02f), new Vector3(size * 0.42f, size * 0.12f, size * 0.42f), centerMaterial, Quaternion.Euler(90f, 0f, 0f));
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

        private static Color GetLineColor(Color baseColor)
        {
            Color darkInk = new Color(0.03f, 0.04f, 0.07f, 1f);
            Color lineColor = Color.Lerp(baseColor, darkInk, 0.62f);
            lineColor.a = 1f;
            return lineColor;
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
