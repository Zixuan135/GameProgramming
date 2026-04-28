using System.Collections.Generic;
using BubbleTown.Core;
using BubbleTown.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Creates the first Phase 2 map art placeholder set and wires it into the Battle scene.
    /// The generated assets stay simple, colorful, and easy to replace later.
    /// </summary>
    public static class MapArtSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";
        private const string MaterialFolder = "Assets/Materials/Map/CandyPark";
        private const string MapPrefabFolder = "Assets/Prefabs/Map/CandyPark";

        private const string GroundPrefabPath = MapPrefabFolder + "/Tile_Ground_CandyPark.prefab";
        private const string HardWallPrefabPath = MapPrefabFolder + "/Wall_Hard_RoundedBlock.prefab";
        private const string SoftWallPrefabPath = MapPrefabFolder + "/Wall_Soft_JellyCrate.prefab";

        private const string GroundMintMaterialPath = MaterialFolder + "/Mat_Tile_GrassPastel.mat";
        private const string GroundBlueMaterialPath = MaterialFolder + "/Mat_Tile_CandyBlue.mat";
        private const string GroundAccentMaterialPath = MaterialFolder + "/Mat_Tile_CheckerAccent.mat";
        private const string HardWallMaterialPath = MaterialFolder + "/Mat_Wall_Hard_Cream.mat";
        private const string HardWallHighlightMaterialPath = MaterialFolder + "/Mat_Wall_Hard_Highlight.mat";
        private const string HardWallShadowMaterialPath = MaterialFolder + "/Mat_Wall_Hard_Shadow.mat";
        private const string SoftWallMaterialPath = MaterialFolder + "/Mat_Wall_Soft_JellyBlue.mat";

        [MenuItem("BubbleTown/Setup/Ensure Phase 2 Map Art")]
        public static void EnsurePhase2MapArt()
        {
            EnsureFolders();

            Material groundMint = EnsureMaterial(GroundMintMaterialPath, "Mat_Tile_GrassPastel", new Color(0.55f, 0.92f, 0.67f), 0.2f);
            Material groundBlue = EnsureMaterial(GroundBlueMaterialPath, "Mat_Tile_CandyBlue", new Color(0.47f, 0.83f, 1f), 0.22f);
            Material groundAccent = EnsureMaterial(GroundAccentMaterialPath, "Mat_Tile_CheckerAccent", new Color(1f, 0.93f, 0.62f), 0.18f);
            Material hardWall = EnsureMaterial(HardWallMaterialPath, "Mat_Wall_Hard_Cream", new Color(1f, 0.86f, 0.55f), 0.28f);
            Material hardWallHighlight = EnsureMaterial(HardWallHighlightMaterialPath, "Mat_Wall_Hard_Highlight", new Color(1f, 0.96f, 0.78f), 0.25f);
            Material hardWallShadow = EnsureMaterial(HardWallShadowMaterialPath, "Mat_Wall_Hard_Shadow", new Color(0.62f, 0.47f, 0.32f), 0.18f);
            Material softWall = EnsureMaterial(SoftWallMaterialPath, "Mat_Wall_Soft_JellyBlue", new Color(0.23f, 0.82f, 0.95f), 0.55f);

            GameObject groundPrefab = EnsureGroundTilePrefab(groundMint, groundAccent);
            GameObject hardWallPrefab = EnsureHardWallPrefab(hardWall, hardWallHighlight, hardWallShadow);
            GameObject softWallPrefab = EnsureSoftWallPrefab(softWall);

            ConfigureBattleScene(groundPrefab, groundMint, groundBlue, groundAccent, hardWallPrefab, softWallPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[MapArtSetup] Phase 2 map art prefabs and Battle scene visuals are ready.");
        }

        public static void EnsurePhase2MapArtFromBatchmode()
        {
            EnsurePhase2MapArt();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "Map");
            EnsureFolder("Assets/Materials/Map", "CandyPark");
            EnsureFolder("Assets/Prefabs", "Map");
            EnsureFolder("Assets/Prefabs/Map", "CandyPark");
        }

        private static void EnsureFolder(string parentFolder, string childFolder)
        {
            string fullPath = parentFolder + "/" + childFolder;
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentFolder, childFolder);
            }
        }

        private static Material EnsureMaterial(string materialPath, string materialName, Color color, float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                material.name = materialName;
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.color = color;
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject EnsureGroundTilePrefab(Material groundMaterial, Material accentMaterial)
        {
            GameObject root = new GameObject("Tile_Ground_CandyPark");
            CreateCubeVisual(root.transform, "TileBase", new Vector3(0f, -0.045f, 0f), new Vector3(0.98f, 0.08f, 0.98f), groundMaterial);
            CreateCubeVisual(root.transform, "TileInset", new Vector3(0f, 0.005f, 0f), new Vector3(0.64f, 0.025f, 0.64f), accentMaterial);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, GroundPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject EnsureHardWallPrefab(Material bodyMaterial, Material highlightMaterial, Material shadowMaterial)
        {
            GameObject root = new GameObject("Wall_Hard_RoundedBlock");

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(0.92f, 1f, 0.92f);

            Transform visualRoot = CreateVisualRoot(root.transform);
            CreateCubeVisual(visualRoot, "BaseBlock", new Vector3(0f, 0.46f, 0f), new Vector3(0.9f, 0.86f, 0.9f), bodyMaterial);
            CreateCubeVisual(visualRoot, "TopCap", new Vector3(0f, 0.94f, 0f), new Vector3(0.72f, 0.14f, 0.72f), highlightMaterial);
            CreateCubeVisual(visualRoot, "BottomShadow", new Vector3(0f, 0.08f, 0f), new Vector3(0.96f, 0.12f, 0.96f), shadowMaterial);

            CreateSphereVisual(visualRoot, "CornerDot_NorthEast", new Vector3(0.36f, 0.78f, 0.36f), new Vector3(0.16f, 0.16f, 0.16f), highlightMaterial);
            CreateSphereVisual(visualRoot, "CornerDot_NorthWest", new Vector3(-0.36f, 0.78f, 0.36f), new Vector3(0.16f, 0.16f, 0.16f), highlightMaterial);
            CreateSphereVisual(visualRoot, "CornerDot_SouthEast", new Vector3(0.36f, 0.78f, -0.36f), new Vector3(0.16f, 0.16f, 0.16f), highlightMaterial);
            CreateSphereVisual(visualRoot, "CornerDot_SouthWest", new Vector3(-0.36f, 0.78f, -0.36f), new Vector3(0.16f, 0.16f, 0.16f), highlightMaterial);
            ConfigureWallFeedback(root, visualRoot, true);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, HardWallPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject EnsureSoftWallPrefab(Material softWallMaterial)
        {
            GameObject root = new GameObject("Wall_Soft_JellyCrate");
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = new Vector3(0.82f, 0.78f, 0.82f);

            Transform visualRoot = CreateVisualRoot(root.transform);
            CreateCubeVisual(visualRoot, "JellyBody", Vector3.zero, new Vector3(0.82f, 0.78f, 0.82f), softWallMaterial);
            ConfigureWallFeedback(root, visualRoot, false);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SoftWallPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Transform CreateVisualRoot(Transform parent)
        {
            GameObject visualRootObject = new GameObject("VisualRoot");
            visualRootObject.transform.SetParent(parent, false);
            visualRootObject.transform.localPosition = Vector3.zero;
            visualRootObject.transform.localRotation = Quaternion.identity;
            visualRootObject.transform.localScale = Vector3.one;
            return visualRootObject.transform;
        }

        private static void ConfigureWallFeedback(GameObject root, Transform visualRoot, bool isHardWall)
        {
            WallFeedback wallFeedback = root.GetComponent<WallFeedback>();
            if (wallFeedback == null)
            {
                wallFeedback = root.AddComponent<WallFeedback>();
            }

            SerializedObject serializedFeedback = new SerializedObject(wallFeedback);
            serializedFeedback.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            serializedFeedback.FindProperty("blockFeedbackSeconds").floatValue = isHardWall ? 0.18f : 0.12f;
            serializedFeedback.FindProperty("blockShakeDistance").floatValue = isHardWall ? 0.035f : 0.02f;
            serializedFeedback.FindProperty("blockScalePunch").floatValue = isHardWall ? 0.06f : 0.04f;
            serializedFeedback.FindProperty("destroyFeedbackSeconds").floatValue = 0.32f;
            serializedFeedback.FindProperty("destroyShakeDistance").floatValue = 0.06f;
            serializedFeedback.FindProperty("finalShrinkScale").floatValue = 0.08f;
            serializedFeedback.FindProperty("shardCount").intValue = isHardWall ? 0 : 6;
            serializedFeedback.FindProperty("shardLifetimeSeconds").floatValue = 0.55f;
            serializedFeedback.FindProperty("shardSize").floatValue = 0.14f;
            serializedFeedback.FindProperty("shardScatterForce").floatValue = 2.2f;
            serializedFeedback.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wallFeedback);
        }

        private static GameObject CreateCubeVisual(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = objectName;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            ConfigureRendererOnlyVisual(visual, material);
            return visual;
        }

        private static GameObject CreateSphereVisual(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = objectName;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            ConfigureRendererOnlyVisual(visual, material);
            return visual;
        }

        private static void ConfigureRendererOnlyVisual(GameObject visual, Material material)
        {
            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static void ConfigureBattleScene(
            GameObject groundPrefab,
            Material groundMint,
            Material groundBlue,
            Material groundAccent,
            GameObject hardWallPrefab,
            GameObject softWallPrefab)
        {
            var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);

            MapManager mapManager = Object.FindObjectOfType<MapManager>();
            MapGenerator mapGenerator = Object.FindObjectOfType<MapGenerator>();
            Transform mapRoot = GameObject.Find("MapRoot")?.transform;
            if (mapRoot == null)
            {
                GameObject mapRootObject = new GameObject("MapRoot");
                mapRoot = mapRootObject.transform;
            }

            RemoveLegacyObject("Ground");
            RemoveLegacyObject("TestHardWall_3_1");
            RemoveLegacyObject("TestSoftWall_1_3");
            RemoveLegacyObject("Ground_CandyParkBoard");
            RemoveLegacyObject("WallVisualsRoot");

            int mapWidth = mapGenerator != null ? mapGenerator.MapWidth : GameConstants.DefaultMapWidth;
            int mapHeight = mapGenerator != null ? mapGenerator.MapHeight : GameConstants.DefaultMapHeight;

            Transform groundRoot = new GameObject("Ground_CandyParkBoard").transform;
            groundRoot.SetParent(mapRoot, false);
            BuildGroundTiles(groundRoot, groundPrefab, groundMint, groundBlue, groundAccent, mapWidth, mapHeight);

            Transform wallRoot = new GameObject("WallVisualsRoot").transform;
            wallRoot.SetParent(mapRoot, false);
            BuildWallVisuals(wallRoot, mapManager, hardWallPrefab, softWallPrefab, mapWidth, mapHeight);

            if (mapManager != null)
            {
                SerializedObject serializedMapManager = new SerializedObject(mapManager);
                serializedMapManager.FindProperty("mapVisualRoot").objectReferenceValue = wallRoot;
                serializedMapManager.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(mapManager);
            }

            EditorUtility.SetDirty(mapRoot.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildGroundTiles(
            Transform groundRoot,
            GameObject groundPrefab,
            Material groundMint,
            Material groundBlue,
            Material groundAccent,
            int mapWidth,
            int mapHeight)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    GameObject tile = InstantiatePrefab(groundPrefab, groundRoot);
                    tile.name = $"Tile_{x:00}_{y:00}";
                    tile.transform.localPosition = new Vector3(x, 0f, y);
                    ApplyTileMaterials(tile, (x + y) % 2 == 0 ? groundMint : groundBlue, groundAccent);
                }
            }
        }

        private static void ApplyTileMaterials(GameObject tile, Material baseMaterial, Material accentMaterial)
        {
            Transform baseTile = tile.transform.Find("TileBase");
            if (baseTile != null && baseTile.TryGetComponent(out MeshRenderer baseRenderer))
            {
                baseRenderer.sharedMaterial = baseMaterial;
            }

            Transform inset = tile.transform.Find("TileInset");
            if (inset != null && inset.TryGetComponent(out MeshRenderer insetRenderer))
            {
                insetRenderer.sharedMaterial = accentMaterial;
            }
        }

        private static void BuildWallVisuals(
            Transform wallRoot,
            MapManager mapManager,
            GameObject hardWallPrefab,
            GameObject softWallPrefab,
            int mapWidth,
            int mapHeight)
        {
            HashSet<Vector2Int> hardWallCells = new HashSet<Vector2Int>();

            for (int x = 0; x < mapWidth; x++)
            {
                hardWallCells.Add(new Vector2Int(x, 0));
                hardWallCells.Add(new Vector2Int(x, mapHeight - 1));
            }

            for (int y = 0; y < mapHeight; y++)
            {
                hardWallCells.Add(new Vector2Int(0, y));
                hardWallCells.Add(new Vector2Int(mapWidth - 1, y));
            }

            AddInitialCells(mapManager, "initialHardWallCells", hardWallCells);

            foreach (Vector2Int cell in hardWallCells)
            {
                GameObject wall = InstantiatePrefab(hardWallPrefab, wallRoot);
                wall.name = $"Wall_Hard_{cell.x:00}_{cell.y:00}";
                wall.transform.localPosition = new Vector3(cell.x, 0f, cell.y);
            }

            List<Vector2Int> softWallCells = ReadInitialCells(mapManager, "initialSoftWallCells");
            for (int i = 0; i < softWallCells.Count; i++)
            {
                Vector2Int cell = softWallCells[i];
                GameObject wall = InstantiatePrefab(softWallPrefab, wallRoot);
                wall.name = $"Wall_Soft_{cell.x:00}_{cell.y:00}";
                wall.transform.localPosition = new Vector3(cell.x, 0.39f, cell.y);
            }
        }

        private static void AddInitialCells(MapManager mapManager, string propertyName, HashSet<Vector2Int> cells)
        {
            List<Vector2Int> initialCells = ReadInitialCells(mapManager, propertyName);
            for (int i = 0; i < initialCells.Count; i++)
            {
                cells.Add(initialCells[i]);
            }
        }

        private static List<Vector2Int> ReadInitialCells(MapManager mapManager, string propertyName)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            if (mapManager == null)
            {
                return cells;
            }

            SerializedObject serializedMapManager = new SerializedObject(mapManager);
            SerializedProperty array = serializedMapManager.FindProperty(propertyName);
            if (array == null || !array.isArray)
            {
                return cells;
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                cells.Add(array.GetArrayElementAtIndex(i).vector2IntValue);
            }

            return cells;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, parent);
            }

            instance.transform.localRotation = Quaternion.identity;
            return instance;
        }

        private static void RemoveLegacyObject(string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
    }
}
