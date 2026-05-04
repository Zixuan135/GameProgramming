using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.Map
{
    /// <summary>
    /// Generates the visual layer for the current grid map.
    /// MapManager remains the source of gameplay truth; this class only builds theme objects.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        private enum MapVisualTheme
        {
            CandyPark,
            JellyMaze
        }

        private const string GeneratedRootPrefix = "GeneratedMap_";
        private const string GroundRootName = "GroundRoot";
        private const string HardWallRootName = "HardWallRoot";
        private const string SoftWallRootName = "SoftWallRoot";
        private const string DecorationRootName = "DecorationRoot";
        private const string GoalRootName = "GoalRoot";

        [Header("Grid")]
        [SerializeField] private int mapWidth = GameConstants.DefaultMapWidth;
        [SerializeField] private int mapHeight = GameConstants.DefaultMapHeight;
        [SerializeField] private float cellSize = GameConstants.GridCellSize;

        [Header("Candy Park Prefabs")]
        [SerializeField] private GameObject groundTilePrefab;
        [SerializeField] private GameObject hardWallPrefab;
        [SerializeField] private GameObject softWallPrefab;

        [Header("Generation")]
        [SerializeField] private Transform generatedMapRoot;
        [SerializeField] private bool hideSceneAuthoredVisualRoots = true;
        [SerializeField] private bool generateGroundTiles = true;
        [SerializeField] private bool generateWalls = true;
        [SerializeField] private bool generateDecorations = true;
        [SerializeField, Min(0f)] private float decorationOuterPadding = 0.85f;

        private Material grassMaterial;
        private Material candyBlueMaterial;
        private Material creamMaterial;
        private Material jellyBlueMaterial;
        private Material shadowMaterial;
        private Material propPinkMaterial;
        private Material propMintMaterial;
        private Material propYellowMaterial;
        private Material jellyFloorMaterial;
        private Material jellyTileInsetMaterial;
        private Material jellyHardWallMaterial;
        private Material jellyHardWallHighlightMaterial;
        private Material jellySoftWallMaterial;
        private Material jellyGlowMaterial;
        private Material jellyDarkMaterial;
        private Material jellyPropCyanMaterial;
        private Material jellyPropPinkMaterial;

        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public float CellSize => cellSize;
        public Transform GeneratedMapRoot => generatedMapRoot;

        public void Generate(BattleMapType mapType)
        {
            MapManager mapManager = GetComponent<MapManager>();
            Generate(mapType, mapManager);
        }

        public void Generate(BattleMapType mapType, MapManager mapManager)
        {
            if (mapManager == null)
            {
                Debug.LogWarning("[MapGenerator] Cannot generate map theme visuals because MapManager is missing.");
                return;
            }

            mapWidth = Mathf.Max(1, mapManager.MapWidth);
            mapHeight = Mathf.Max(1, mapManager.MapHeight);
            cellSize = Mathf.Max(0.1f, mapManager.CellSize);
            MapVisualTheme visualTheme = ResolveVisualTheme(mapType);

            Transform root = ResolveGeneratedRoot(mapType);
            ClearInactiveGeneratedRoots(root);
            ClearGeneratedChildren(root);
            if (hideSceneAuthoredVisualRoots)
            {
                SetLegacySceneVisualRootsActive(root, false);
            }

            Transform groundRoot = CreateChildRoot(root, GroundRootName);
            Transform hardWallRoot = CreateChildRoot(root, HardWallRootName);
            Transform softWallRoot = CreateChildRoot(root, SoftWallRootName);
            Transform decorationRoot = CreateChildRoot(root, DecorationRootName);
            Transform goalRoot = CreateChildRoot(root, GoalRootName);

            GenerateGridVisuals(mapManager, groundRoot, hardWallRoot, softWallRoot, visualTheme);
            GenerateSinglePlayerGoalVisual(mapManager, goalRoot, visualTheme);
            if (generateDecorations)
            {
                GenerateDecorations(mapType, visualTheme, decorationRoot);
            }

            Debug.Log($"[MapGenerator] Generated {GetVisualThemeDisplayName(visualTheme)} visuals. Type: {mapType}, Size: {mapWidth}x{mapHeight}");
        }

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith(GeneratedRootPrefix, System.StringComparison.Ordinal))
                {
                    DestroyGeneratedObject(child.gameObject);
                }
            }

            generatedMapRoot = null;
        }

        private void GenerateGridVisuals(
            MapManager mapManager,
            Transform groundRoot,
            Transform hardWallRoot,
            Transform softWallRoot,
            MapVisualTheme visualTheme)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Vector2Int gridPosition = new Vector2Int(x, y);
                    GridCell cell = mapManager.GetCell(gridPosition);
                    if (cell == null)
                    {
                        continue;
                    }

                    if (generateGroundTiles)
                    {
                        GameObject selectedGroundPrefab = visualTheme == MapVisualTheme.CandyPark ? groundTilePrefab : null;
                        SpawnMapPiece(selectedGroundPrefab, groundRoot, mapManager.GridToWorld(gridPosition), $"Tile_{x:00}_{y:00}", name => CreateFallbackGroundTile(name, visualTheme));
                    }

                    if (!generateWalls)
                    {
                        continue;
                    }

                    if (cell.IsHardWall)
                    {
                        GameObject selectedHardWallPrefab = visualTheme == MapVisualTheme.CandyPark ? hardWallPrefab : null;
                        SpawnMapPiece(selectedHardWallPrefab, hardWallRoot, mapManager.GridToWorld(gridPosition), $"Wall_Hard_{x:00}_{y:00}", name => CreateFallbackHardWall(name, visualTheme));
                    }
                    else if (cell.IsSoftWall)
                    {
                        GameObject selectedSoftWallPrefab = visualTheme == MapVisualTheme.CandyPark ? softWallPrefab : null;
                        GameObject softWall = SpawnMapPiece(selectedSoftWallPrefab, softWallRoot, mapManager.GridToWorld(gridPosition), $"Wall_Soft_{x:00}_{y:00}", name => CreateFallbackSoftWall(name, visualTheme));
                        mapManager.RegisterSoftWallObject(gridPosition, softWall);
                    }
                }
            }
        }

        private GameObject SpawnMapPiece(
            GameObject prefab,
            Transform parent,
            Vector3 position,
            string objectName,
            System.Func<string, GameObject> fallbackFactory)
        {
            GameObject instance = prefab != null
                ? Instantiate(prefab, position, Quaternion.identity, parent)
                : fallbackFactory.Invoke(objectName);

            instance.name = objectName;
            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(parent, true);
            instanceTransform.position = position;
            instanceTransform.rotation = Quaternion.identity;
            return instance;
        }

        private void GenerateDecorations(BattleMapType mapType, MapVisualTheme visualTheme, Transform decorationRoot)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.JellyMaze:
                    GenerateJellyMazeDecorations(decorationRoot);
                    break;
                default:
                    GenerateCandyParkDecorations(mapType, decorationRoot);
                    break;
            }
        }

        private void GenerateSinglePlayerGoalVisual(MapManager mapManager, Transform goalRoot, MapVisualTheme visualTheme)
        {
            if (mapManager == null || goalRoot == null || GameManager.Instance == null ||
                GameManager.Instance.CurrentGameMode != GameMode.SinglePlayer)
            {
                return;
            }

            Vector2Int goalGrid = mapManager.GetSinglePlayerGoalGrid();
            Vector3 goalWorld = mapManager.GridToWorld(goalGrid);
            GameObject goal = new GameObject($"Goal_Exit_{goalGrid.x:00}_{goalGrid.y:00}");
            Transform goalTransform = goal.transform;
            goalTransform.SetParent(goalRoot);
            goalTransform.position = goalWorld;
            goalTransform.rotation = Quaternion.identity;

            Material baseMaterial = visualTheme == MapVisualTheme.JellyMaze ? GetJellyTileInsetMaterial() : GetPropMintMaterial();
            Material accentMaterial = visualTheme == MapVisualTheme.JellyMaze ? GetJellyGlowMaterial() : GetPropYellowMaterial();
            Material flagMaterial = visualTheme == MapVisualTheme.JellyMaze ? GetJellyPropPinkMaterial() : GetPropPinkMaterial();

            CreatePrimitiveChild(goalTransform, "GoalPad_Large", PrimitiveType.Cylinder, new Vector3(0f, 0.055f, 0f), new Vector3(0.98f, 0.045f, 0.98f), baseMaterial);
            CreatePrimitiveChild(goalTransform, "GoalRing_Outer", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(0.78f, 0.035f, 0.78f), accentMaterial);
            CreatePrimitiveChild(goalTransform, "GoalRing_Inner", PrimitiveType.Cylinder, new Vector3(0f, 0.165f, 0f), new Vector3(0.48f, 0.028f, 0.48f), GetCreamMaterial());

            CreatePrimitiveChild(goalTransform, "ExitBeacon_Pole", PrimitiveType.Cylinder, new Vector3(0f, 0.78f, 0f), new Vector3(0.055f, 0.68f, 0.055f), GetCreamMaterial());
            CreatePrimitiveChild(goalTransform, "ExitBeacon_Orb", PrimitiveType.Sphere, new Vector3(0f, 1.48f, 0f), new Vector3(0.32f, 0.32f, 0.32f), accentMaterial);
            CreatePrimitiveChild(goalTransform, "ExitBeacon_Highlight", PrimitiveType.Sphere, new Vector3(-0.11f, 1.58f, -0.11f), new Vector3(0.12f, 0.12f, 0.12f), GetCreamMaterial());

            CreatePrimitiveChild(goalTransform, "FlagPole_Left", PrimitiveType.Cylinder, new Vector3(-0.42f, 0.62f, -0.28f), new Vector3(0.04f, 0.58f, 0.04f), GetCreamMaterial());
            CreatePrimitiveChild(goalTransform, "Flag_Left", PrimitiveType.Cube, new Vector3(-0.2f, 0.92f, -0.28f), new Vector3(0.38f, 0.22f, 0.04f), flagMaterial);
            CreatePrimitiveChild(goalTransform, "FlagPole_Right", PrimitiveType.Cylinder, new Vector3(0.42f, 0.62f, 0.28f), new Vector3(0.04f, 0.58f, 0.04f), GetCreamMaterial());
            CreatePrimitiveChild(goalTransform, "Flag_Right", PrimitiveType.Cube, new Vector3(0.2f, 0.92f, 0.28f), new Vector3(0.38f, 0.22f, 0.04f), flagMaterial);

            GameObject arrowA = CreatePrimitiveChild(goalTransform, "ExitArrow_A", PrimitiveType.Cube, new Vector3(0f, 0.28f, 0.36f), new Vector3(0.52f, 0.055f, 0.16f), accentMaterial);
            arrowA.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
            GameObject arrowB = CreatePrimitiveChild(goalTransform, "ExitArrow_B", PrimitiveType.Cube, new Vector3(0f, 0.28f, 0.36f), new Vector3(0.52f, 0.055f, 0.16f), accentMaterial);
            arrowB.transform.localEulerAngles = new Vector3(0f, -45f, 0f);
            CreatePrimitiveChild(goalTransform, "SparkleOrb_Left", PrimitiveType.Sphere, new Vector3(-0.34f, 0.5f, 0.24f), new Vector3(0.18f, 0.18f, 0.18f), accentMaterial);
            CreatePrimitiveChild(goalTransform, "SparkleOrb_Right", PrimitiveType.Sphere, new Vector3(0.34f, 0.46f, -0.24f), new Vector3(0.16f, 0.16f, 0.16f), flagMaterial);

            AddDecorationAnimation(goal, goalTransform, true, 0.05f, 2.4f, new Vector3(0f, 42f, 0f), true, 0.055f, 3.1f);
        }

        private void GenerateCandyParkDecorations(BattleMapType mapType, Transform decorationRoot)
        {
            float minX = -decorationOuterPadding;
            float maxX = (mapWidth - 1) * cellSize + decorationOuterPadding;
            float minZ = -decorationOuterPadding;
            float maxZ = (mapHeight - 1) * cellSize + decorationOuterPadding;
            float centerX = (mapWidth - 1) * cellSize * 0.5f;
            float centerZ = (mapHeight - 1) * cellSize * 0.5f;

            CreateFenceLine(decorationRoot, new Vector3(centerX, 0f, minZ), mapWidth, true, "Fence_South");
            CreateFenceLine(decorationRoot, new Vector3(centerX, 0f, maxZ), mapWidth, true, "Fence_North");
            CreateFenceLine(decorationRoot, new Vector3(minX, 0f, centerZ), mapHeight, false, "Fence_West");
            CreateFenceLine(decorationRoot, new Vector3(maxX, 0f, centerZ), mapHeight, false, "Fence_East");

            CreateLollipopTree(decorationRoot, new Vector3(minX + 0.15f, 0f, minZ + 0.2f), "LollipopTree_SouthWest", GetThemeAccentColor(mapType, 0));
            CreateLollipopTree(decorationRoot, new Vector3(maxX - 0.15f, 0f, minZ + 0.25f), "LollipopTree_SouthEast", GetThemeAccentColor(mapType, 1));
            CreateLollipopTree(decorationRoot, new Vector3(minX + 0.2f, 0f, maxZ - 0.15f), "LollipopTree_NorthWest", GetThemeAccentColor(mapType, 2));
            CreateBalloonCluster(decorationRoot, new Vector3(maxX - 0.3f, 0f, maxZ - 0.1f), "BalloonCluster_NorthEast");
            CreateRoundBush(decorationRoot, new Vector3(centerX, 0f, maxZ - 0.1f), "RoundBush_NorthCenter");
            CreateSignBoard(decorationRoot, new Vector3(centerX, 0f, minZ + 0.1f), "Sign_CandyPark");

            CreateSmallCandyTree(decorationRoot, new Vector3(minX + 0.1f, 0f, centerZ - cellSize * 1.8f), "SmallTree_WestSouth");
            CreateSmallCandyTree(decorationRoot, new Vector3(maxX - 0.1f, 0f, centerZ + cellSize * 1.7f), "SmallTree_EastNorth");
            CreateToyBarrel(decorationRoot, new Vector3(minX + 0.25f, 0f, minZ + cellSize * 2.3f), "ToyBarrel_West");
            CreateToyBarrel(decorationRoot, new Vector3(maxX - 0.25f, 0f, maxZ - cellSize * 2.2f), "ToyBarrel_East");
            CreateCandyLamp(decorationRoot, new Vector3(centerX - cellSize * 2.4f, 0f, maxZ - 0.1f), "CandyLamp_NorthWest");
            CreateCandyLamp(decorationRoot, new Vector3(centerX + cellSize * 2.3f, 0f, minZ + 0.1f), "CandyLamp_SouthEast");
            CreateBackgroundCloud(decorationRoot, new Vector3(minX + 0.45f, 1.45f, maxZ - 0.05f), "Cloud_Background_NorthWest");
            CreateBackgroundCloud(decorationRoot, new Vector3(maxX - 0.45f, 1.25f, minZ + 0.05f), "Cloud_Background_SouthEast");
        }

        private void CreateFenceLine(Transform parent, Vector3 center, int lengthCells, bool horizontal, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = center;

            float length = Mathf.Max(1, lengthCells - 1) * cellSize;
            Vector3 railScale = horizontal ? new Vector3(length, 0.12f, 0.12f) : new Vector3(0.12f, 0.12f, length);
            Vector3 postStep = horizontal ? Vector3.right * cellSize * 2f : Vector3.forward * cellSize * 2f;
            int postCount = Mathf.Max(2, Mathf.CeilToInt(lengthCells / 2f) + 1);
            Vector3 start = -postStep * ((postCount - 1) * 0.5f);

            CreatePrimitiveChild(root.transform, "Rail_Top", PrimitiveType.Cube, new Vector3(0f, 0.55f, 0f), railScale, GetCreamMaterial());
            CreatePrimitiveChild(root.transform, "Rail_Bottom", PrimitiveType.Cube, new Vector3(0f, 0.32f, 0f), railScale, GetCandyBlueMaterial());

            for (int i = 0; i < postCount; i++)
            {
                CreatePrimitiveChild(root.transform, $"Post_{i:00}", PrimitiveType.Cube, start + postStep * i + Vector3.up * 0.38f, new Vector3(0.16f, 0.72f, 0.16f), GetCreamMaterial());
            }
        }

        private void CreateLollipopTree(Transform parent, Vector3 position, string objectName, Color candyColor)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            Material candyMaterial = CreateRuntimeMaterial(objectName + "_Candy", candyColor);
            CreatePrimitiveChild(root.transform, "Stick", PrimitiveType.Cylinder, new Vector3(0f, 0.48f, 0f), new Vector3(0.12f, 0.48f, 0.12f), GetCreamMaterial());
            CreatePrimitiveChild(root.transform, "CandyTop", PrimitiveType.Sphere, new Vector3(0f, 1.05f, 0f), new Vector3(0.62f, 0.62f, 0.62f), candyMaterial);
            CreatePrimitiveChild(root.transform, "CandyHighlight", PrimitiveType.Sphere, new Vector3(-0.16f, 1.18f, -0.16f), new Vector3(0.18f, 0.18f, 0.18f), GetCreamMaterial());
        }

        private void CreateBalloonCluster(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "String_A", PrimitiveType.Cylinder, new Vector3(-0.15f, 0.55f, 0f), new Vector3(0.025f, 0.55f, 0.025f), GetCreamMaterial());
            CreatePrimitiveChild(root.transform, "String_B", PrimitiveType.Cylinder, new Vector3(0.15f, 0.62f, 0f), new Vector3(0.025f, 0.62f, 0.025f), GetCreamMaterial());
            CreatePrimitiveChild(root.transform, "Balloon_Pink", PrimitiveType.Sphere, new Vector3(-0.2f, 1.2f, 0f), new Vector3(0.36f, 0.46f, 0.36f), GetPropPinkMaterial());
            CreatePrimitiveChild(root.transform, "Balloon_Mint", PrimitiveType.Sphere, new Vector3(0.18f, 1.35f, 0.04f), new Vector3(0.36f, 0.46f, 0.36f), GetPropMintMaterial());
            CreatePrimitiveChild(root.transform, "Balloon_Yellow", PrimitiveType.Sphere, new Vector3(0f, 1.55f, -0.12f), new Vector3(0.34f, 0.44f, 0.34f), GetPropYellowMaterial());
        }

        private void CreateRoundBush(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;
            CreatePrimitiveChild(root.transform, "Bush_Left", PrimitiveType.Sphere, new Vector3(-0.28f, 0.25f, 0f), new Vector3(0.58f, 0.38f, 0.48f), GetPropMintMaterial());
            CreatePrimitiveChild(root.transform, "Bush_Center", PrimitiveType.Sphere, new Vector3(0.08f, 0.32f, 0f), new Vector3(0.72f, 0.48f, 0.58f), GetPropMintMaterial());
            CreatePrimitiveChild(root.transform, "Bush_Right", PrimitiveType.Sphere, new Vector3(0.48f, 0.24f, 0f), new Vector3(0.48f, 0.32f, 0.42f), GetPropMintMaterial());
        }

        private void CreateSignBoard(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;
            CreatePrimitiveChild(root.transform, "Post_Left", PrimitiveType.Cube, new Vector3(-0.35f, 0.34f, 0f), new Vector3(0.12f, 0.68f, 0.12f), GetShadowMaterial());
            CreatePrimitiveChild(root.transform, "Post_Right", PrimitiveType.Cube, new Vector3(0.35f, 0.34f, 0f), new Vector3(0.12f, 0.68f, 0.12f), GetShadowMaterial());
            CreatePrimitiveChild(root.transform, "Board", PrimitiveType.Cube, new Vector3(0f, 0.78f, 0f), new Vector3(1.05f, 0.46f, 0.12f), GetPropYellowMaterial());
            CreatePrimitiveChild(root.transform, "BoardStripe", PrimitiveType.Cube, new Vector3(0f, 0.82f, -0.065f), new Vector3(0.82f, 0.08f, 0.035f), GetCandyBlueMaterial());
        }

        private void CreateSmallCandyTree(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "Trunk", PrimitiveType.Cylinder, new Vector3(0f, 0.36f, 0f), new Vector3(0.16f, 0.36f, 0.16f), GetShadowMaterial());
            CreatePrimitiveChild(root.transform, "LeafPuff_Center", PrimitiveType.Sphere, new Vector3(0f, 0.88f, 0f), new Vector3(0.56f, 0.46f, 0.56f), GetPropMintMaterial());
            CreatePrimitiveChild(root.transform, "LeafPuff_Left", PrimitiveType.Sphere, new Vector3(-0.28f, 0.75f, 0f), new Vector3(0.38f, 0.32f, 0.38f), GetPropMintMaterial());
            CreatePrimitiveChild(root.transform, "LeafPuff_Right", PrimitiveType.Sphere, new Vector3(0.28f, 0.75f, 0f), new Vector3(0.38f, 0.32f, 0.38f), GetPropMintMaterial());
            CreatePrimitiveChild(root.transform, "CandyDot", PrimitiveType.Sphere, new Vector3(0.12f, 0.96f, -0.28f), new Vector3(0.14f, 0.14f, 0.14f), GetPropPinkMaterial());
        }

        private void CreateToyBarrel(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "BarrelBody", PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(0.38f, 0.35f, 0.38f), GetPropYellowMaterial());
            CreatePrimitiveChild(root.transform, "BarrelTopBand", PrimitiveType.Cylinder, new Vector3(0f, 0.62f, 0f), new Vector3(0.42f, 0.05f, 0.42f), GetCandyBlueMaterial());
            CreatePrimitiveChild(root.transform, "BarrelBottomBand", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(0.42f, 0.05f, 0.42f), GetCandyBlueMaterial());
            CreatePrimitiveChild(root.transform, "BarrelLabel", PrimitiveType.Cube, new Vector3(0f, 0.37f, -0.38f), new Vector3(0.34f, 0.18f, 0.035f), GetCreamMaterial());
        }

        private void CreateCandyLamp(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "LampPost", PrimitiveType.Cylinder, new Vector3(0f, 0.48f, 0f), new Vector3(0.08f, 0.48f, 0.08f), GetCreamMaterial());
            CreatePrimitiveChild(root.transform, "LampBase", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(0.22f, 0.08f, 0.22f), GetShadowMaterial());
            GameObject lampHead = CreatePrimitiveChild(root.transform, "LampHead_Glow", PrimitiveType.Sphere, new Vector3(0f, 1f, 0f), new Vector3(0.34f, 0.34f, 0.34f), GetPropYellowMaterial());
            CreatePrimitiveChild(lampHead.transform, "LampHighlight", PrimitiveType.Sphere, new Vector3(-0.2f, 0.12f, -0.2f), new Vector3(0.22f, 0.22f, 0.22f), GetCreamMaterial());
            AddDecorationAnimation(root, lampHead.transform, true, 0.025f, 2.4f, Vector3.zero, true, 0.08f, 3.2f);
        }

        private void CreateBackgroundCloud(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "Cloud_Left", PrimitiveType.Sphere, new Vector3(-0.32f, 0f, 0f), new Vector3(0.48f, 0.28f, 0.26f), GetCreamMaterial());
            CreatePrimitiveChild(root.transform, "Cloud_Center", PrimitiveType.Sphere, new Vector3(0.04f, 0.08f, 0f), new Vector3(0.64f, 0.36f, 0.3f), GetCreamMaterial());
            CreatePrimitiveChild(root.transform, "Cloud_Right", PrimitiveType.Sphere, new Vector3(0.44f, -0.02f, 0f), new Vector3(0.44f, 0.26f, 0.24f), GetCreamMaterial());
            AddDecorationAnimation(root, root.transform, true, 0.06f, 0.8f, new Vector3(0f, 4f, 0f), false, 0f, 1f);
        }

        private void GenerateJellyMazeDecorations(Transform decorationRoot)
        {
            float minX = -decorationOuterPadding;
            float maxX = (mapWidth - 1) * cellSize + decorationOuterPadding;
            float minZ = -decorationOuterPadding;
            float maxZ = (mapHeight - 1) * cellSize + decorationOuterPadding;
            float centerX = (mapWidth - 1) * cellSize * 0.5f;
            float centerZ = (mapHeight - 1) * cellSize * 0.5f;

            CreateNeonGateLine(decorationRoot, new Vector3(centerX, 0f, minZ), mapWidth, true, "NeonGate_South");
            CreateNeonGateLine(decorationRoot, new Vector3(centerX, 0f, maxZ), mapWidth, true, "NeonGate_North");
            CreateNeonGateLine(decorationRoot, new Vector3(minX, 0f, centerZ), mapHeight, false, "NeonGate_West");
            CreateNeonGateLine(decorationRoot, new Vector3(maxX, 0f, centerZ), mapHeight, false, "NeonGate_East");

            CreateJellyCrystalCluster(decorationRoot, new Vector3(minX + 0.2f, 0f, minZ + 0.15f), "CrystalCluster_SouthWest");
            CreateJellyCrystalCluster(decorationRoot, new Vector3(maxX - 0.2f, 0f, maxZ - 0.15f), "CrystalCluster_NorthEast");
            CreateGlowTube(decorationRoot, new Vector3(maxX - 0.15f, 0f, minZ + 0.15f), "GlowTube_SouthEast", true);
            CreateGlowTube(decorationRoot, new Vector3(minX + 0.15f, 0f, maxZ - 0.15f), "GlowTube_NorthWest", false);
            CreateSignalBeacon(decorationRoot, new Vector3(centerX, 0f, maxZ - 0.05f), "SignalBeacon_North");
            CreateSignalBeacon(decorationRoot, new Vector3(centerX, 0f, minZ + 0.05f), "SignalBeacon_South");
            CreateEnergyBarrel(decorationRoot, new Vector3(minX + 0.2f, 0f, centerZ - cellSize * 1.7f), "EnergyBarrel_WestSouth");
            CreateEnergyBarrel(decorationRoot, new Vector3(maxX - 0.2f, 0f, centerZ + cellSize * 1.6f), "EnergyBarrel_EastNorth");
            CreateFloatingGlowOrb(decorationRoot, new Vector3(centerX - cellSize * 2f, 0f, maxZ - 0.02f), "FloatingOrb_NorthWest");
            CreateFloatingGlowOrb(decorationRoot, new Vector3(centerX + cellSize * 2.1f, 0f, minZ + 0.02f), "FloatingOrb_SouthEast");
            CreateHoloSign(decorationRoot, new Vector3(centerX, 0f, minZ + 0.05f), "HoloSign_JellyMaze");
            CreateDataTower(decorationRoot, new Vector3(maxX - 0.25f, 0f, centerZ - cellSize * 0.3f), "DataTower_East");
            CreateDataTower(decorationRoot, new Vector3(minX + 0.25f, 0f, centerZ + cellSize * 0.35f), "DataTower_West");
        }

        private void CreateNeonGateLine(Transform parent, Vector3 center, int lengthCells, bool horizontal, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = center;

            float length = Mathf.Max(1, lengthCells - 1) * cellSize;
            Vector3 railScale = horizontal ? new Vector3(length, 0.08f, 0.08f) : new Vector3(0.08f, 0.08f, length);
            Vector3 postStep = horizontal ? Vector3.right * cellSize * 2f : Vector3.forward * cellSize * 2f;
            int postCount = Mathf.Max(2, Mathf.CeilToInt(lengthCells / 2f) + 1);
            Vector3 start = -postStep * ((postCount - 1) * 0.5f);

            CreatePrimitiveChild(root.transform, "Rail_GlowTop", PrimitiveType.Cube, new Vector3(0f, 0.7f, 0f), railScale, GetJellyGlowMaterial());
            CreatePrimitiveChild(root.transform, "Rail_DarkBase", PrimitiveType.Cube, new Vector3(0f, 0.42f, 0f), railScale * 1.18f, GetJellyDarkMaterial());

            for (int i = 0; i < postCount; i++)
            {
                Vector3 localPosition = start + postStep * i + Vector3.up * 0.42f;
                CreatePrimitiveChild(root.transform, $"Post_Dark_{i:00}", PrimitiveType.Cube, localPosition, new Vector3(0.14f, 0.84f, 0.14f), GetJellyHardWallMaterial());
                CreatePrimitiveChild(root.transform, $"Post_Glow_{i:00}", PrimitiveType.Sphere, localPosition + Vector3.up * 0.46f, new Vector3(0.18f, 0.18f, 0.18f), GetJellyPropCyanMaterial());
            }
        }

        private void CreateJellyCrystalCluster(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            GameObject largeCrystal = CreatePrimitiveChild(root.transform, "Crystal_Large", PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(0.34f, 0.9f, 0.34f), GetJellyPropPinkMaterial());
            largeCrystal.transform.localEulerAngles = new Vector3(0f, 28f, 0f);
            GameObject smallCrystalA = CreatePrimitiveChild(root.transform, "Crystal_Small_A", PrimitiveType.Cube, new Vector3(-0.34f, 0.32f, 0.1f), new Vector3(0.24f, 0.64f, 0.24f), GetJellyPropCyanMaterial());
            smallCrystalA.transform.localEulerAngles = new Vector3(0f, -18f, 0f);
            GameObject smallCrystalB = CreatePrimitiveChild(root.transform, "Crystal_Small_B", PrimitiveType.Cube, new Vector3(0.35f, 0.26f, -0.08f), new Vector3(0.2f, 0.52f, 0.2f), GetJellyGlowMaterial());
            smallCrystalB.transform.localEulerAngles = new Vector3(0f, 42f, 0f);
            CreatePrimitiveChild(root.transform, "ClusterBase", PrimitiveType.Sphere, new Vector3(0f, 0.1f, 0f), new Vector3(0.78f, 0.16f, 0.58f), GetJellyDarkMaterial());
        }

        private void CreateGlowTube(Transform parent, Vector3 position, string objectName, bool horizontal)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            GameObject tube = CreatePrimitiveChild(root.transform, "Tube_GlowCore", PrimitiveType.Cylinder, new Vector3(0f, 0.52f, 0f), new Vector3(0.16f, 0.9f, 0.16f), GetJellyGlowMaterial());
            tube.transform.localEulerAngles = horizontal ? new Vector3(0f, 0f, 90f) : new Vector3(90f, 0f, 0f);
            GameObject casingA = CreatePrimitiveChild(root.transform, "Tube_Cap_A", PrimitiveType.Sphere, horizontal ? new Vector3(-0.48f, 0.52f, 0f) : new Vector3(0f, 0.52f, -0.48f), new Vector3(0.22f, 0.22f, 0.22f), GetJellyHardWallMaterial());
            GameObject casingB = CreatePrimitiveChild(root.transform, "Tube_Cap_B", PrimitiveType.Sphere, horizontal ? new Vector3(0.48f, 0.52f, 0f) : new Vector3(0f, 0.52f, 0.48f), new Vector3(0.22f, 0.22f, 0.22f), GetJellyHardWallMaterial());
            casingA.transform.localEulerAngles = Vector3.zero;
            casingB.transform.localEulerAngles = Vector3.zero;
        }

        private void CreateSignalBeacon(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "BeaconBase", PrimitiveType.Cylinder, new Vector3(0f, 0.18f, 0f), new Vector3(0.34f, 0.18f, 0.34f), GetJellyDarkMaterial());
            CreatePrimitiveChild(root.transform, "BeaconStem", PrimitiveType.Cylinder, new Vector3(0f, 0.58f, 0f), new Vector3(0.12f, 0.5f, 0.12f), GetJellyHardWallMaterial());
            CreatePrimitiveChild(root.transform, "BeaconLight", PrimitiveType.Sphere, new Vector3(0f, 1.08f, 0f), new Vector3(0.38f, 0.38f, 0.38f), GetJellyGlowMaterial());
            CreatePrimitiveChild(root.transform, "BeaconHalo", PrimitiveType.Sphere, new Vector3(0f, 1.08f, 0f), new Vector3(0.62f, 0.16f, 0.62f), GetJellyPropCyanMaterial());
            AddDecorationAnimation(root, root.transform, false, 0f, 1f, new Vector3(0f, 22f, 0f), true, 0.025f, 2.2f);
        }

        private void CreateEnergyBarrel(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "BarrelCore", PrimitiveType.Cylinder, new Vector3(0f, 0.38f, 0f), new Vector3(0.36f, 0.38f, 0.36f), GetJellyHardWallMaterial());
            CreatePrimitiveChild(root.transform, "BarrelTopGlow", PrimitiveType.Cylinder, new Vector3(0f, 0.68f, 0f), new Vector3(0.38f, 0.04f, 0.38f), GetJellyGlowMaterial());
            CreatePrimitiveChild(root.transform, "BarrelBottomGlow", PrimitiveType.Cylinder, new Vector3(0f, 0.1f, 0f), new Vector3(0.38f, 0.04f, 0.38f), GetJellyPropCyanMaterial());
            CreatePrimitiveChild(root.transform, "WarningStripe", PrimitiveType.Cube, new Vector3(0f, 0.4f, -0.36f), new Vector3(0.32f, 0.1f, 0.035f), GetJellyPropPinkMaterial());
        }

        private void CreateFloatingGlowOrb(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            GameObject orb = CreatePrimitiveChild(root.transform, "OrbCore", PrimitiveType.Sphere, new Vector3(0f, 1.05f, 0f), new Vector3(0.36f, 0.36f, 0.36f), GetJellyGlowMaterial());
            CreatePrimitiveChild(orb.transform, "OrbRing_X", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(0.42f, 0.018f, 0.42f), GetJellyPropCyanMaterial()).transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            CreatePrimitiveChild(orb.transform, "OrbRing_Z", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), new Vector3(0.42f, 0.018f, 0.42f), GetJellyPropPinkMaterial()).transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            AddDecorationAnimation(root, orb.transform, true, 0.08f, 1.8f, new Vector3(0f, 46f, 0f), true, 0.08f, 3.1f);
        }

        private void CreateHoloSign(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "SignPost_Left", PrimitiveType.Cube, new Vector3(-0.42f, 0.38f, 0f), new Vector3(0.08f, 0.76f, 0.08f), GetJellyDarkMaterial());
            CreatePrimitiveChild(root.transform, "SignPost_Right", PrimitiveType.Cube, new Vector3(0.42f, 0.38f, 0f), new Vector3(0.08f, 0.76f, 0.08f), GetJellyDarkMaterial());
            GameObject signPanel = CreatePrimitiveChild(root.transform, "HoloPanel", PrimitiveType.Cube, new Vector3(0f, 0.88f, 0f), new Vector3(1.08f, 0.42f, 0.04f), GetJellyTileInsetMaterial());
            CreatePrimitiveChild(signPanel.transform, "PanelStripe", PrimitiveType.Cube, new Vector3(0f, 0.16f, -0.05f), new Vector3(0.72f, 0.04f, 0.04f), GetJellyGlowMaterial());
            AddDecorationAnimation(root, signPanel.transform, true, 0.025f, 2f, Vector3.zero, true, 0.04f, 2.8f);
        }

        private void CreateDataTower(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "TowerBase", PrimitiveType.Cube, new Vector3(0f, 0.2f, 0f), new Vector3(0.44f, 0.4f, 0.44f), GetJellyDarkMaterial());
            CreatePrimitiveChild(root.transform, "TowerBody", PrimitiveType.Cube, new Vector3(0f, 0.72f, 0f), new Vector3(0.32f, 0.76f, 0.32f), GetJellyHardWallMaterial());
            GameObject antenna = CreatePrimitiveChild(root.transform, "AntennaGlow", PrimitiveType.Sphere, new Vector3(0f, 1.18f, 0f), new Vector3(0.24f, 0.24f, 0.24f), GetJellyGlowMaterial());
            CreatePrimitiveChild(root.transform, "PanelLight_A", PrimitiveType.Cube, new Vector3(0f, 0.62f, -0.18f), new Vector3(0.18f, 0.05f, 0.035f), GetJellyPropCyanMaterial());
            CreatePrimitiveChild(root.transform, "PanelLight_B", PrimitiveType.Cube, new Vector3(0f, 0.82f, -0.18f), new Vector3(0.18f, 0.05f, 0.035f), GetJellyPropPinkMaterial());
            AddDecorationAnimation(root, antenna.transform, true, 0.035f, 2.6f, new Vector3(0f, 30f, 0f), true, 0.08f, 3.4f);
        }

        private GameObject CreatePrimitiveChild(Transform parent, string objectName, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyGeneratedObject(collider);
            }

            return primitive;
        }

        private Transform CreateVisualRoot(Transform parent)
        {
            GameObject visualRootObject = new GameObject("VisualRoot");
            Transform visualRoot = visualRootObject.transform;
            visualRoot.SetParent(parent);
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
            return visualRoot;
        }

        private void ConfigureGeneratedWallFeedback(GameObject wallRoot, Transform visualRoot)
        {
            WallFeedback feedback = wallRoot.GetComponent<WallFeedback>();
            if (feedback == null)
            {
                feedback = wallRoot.AddComponent<WallFeedback>();
            }

            feedback.SetVisualRoot(visualRoot);
        }

        private void AddDecorationAnimation(
            GameObject decorationRoot,
            Transform animatedRoot,
            bool enableBob,
            float bobAmplitude,
            float bobSpeed,
            Vector3 rotationDegreesPerSecond,
            bool enableScalePulse,
            float scalePulseAmount,
            float scalePulseSpeed)
        {
            EnvironmentDecorationAnimator animator = decorationRoot.GetComponent<EnvironmentDecorationAnimator>();
            if (animator == null)
            {
                animator = decorationRoot.AddComponent<EnvironmentDecorationAnimator>();
            }

            animator.Configure(
                animatedRoot,
                enableBob,
                bobAmplitude,
                bobSpeed,
                rotationDegreesPerSecond,
                enableScalePulse,
                scalePulseAmount,
                scalePulseSpeed);
        }

        private GameObject CreateFallbackGroundTile(string objectName, MapVisualTheme visualTheme)
        {
            return visualTheme == MapVisualTheme.JellyMaze
                ? CreateJellyMazeGroundTile(objectName)
                : CreateCandyParkGroundTile(objectName);
        }

        private GameObject CreateCandyParkGroundTile(string objectName)
        {
            GameObject root = new GameObject(objectName);
            CreatePrimitiveChild(root.transform, "TileBase", PrimitiveType.Cube, new Vector3(0f, -0.045f, 0f), new Vector3(0.98f, 0.08f, 0.98f), GetGrassMaterial());
            CreatePrimitiveChild(root.transform, "TileInset", PrimitiveType.Cube, new Vector3(0f, 0.005f, 0f), new Vector3(0.64f, 0.025f, 0.64f), GetCandyBlueMaterial());
            return root;
        }

        private GameObject CreateJellyMazeGroundTile(string objectName)
        {
            GameObject root = new GameObject(objectName);
            CreatePrimitiveChild(root.transform, "TileBase_DarkJelly", PrimitiveType.Cube, new Vector3(0f, -0.052f, 0f), new Vector3(0.98f, 0.07f, 0.98f), GetJellyFloorMaterial());
            CreatePrimitiveChild(root.transform, "TileInset_GlowLane", PrimitiveType.Cube, new Vector3(0f, -0.004f, 0f), new Vector3(0.7f, 0.018f, 0.7f), GetJellyTileInsetMaterial());
            CreatePrimitiveChild(root.transform, "TileCenterDot", PrimitiveType.Sphere, new Vector3(0f, 0.018f, 0f), new Vector3(0.12f, 0.02f, 0.12f), GetJellyGlowMaterial());
            return root;
        }

        private GameObject CreateFallbackHardWall(string objectName, MapVisualTheme visualTheme)
        {
            return visualTheme == MapVisualTheme.JellyMaze
                ? CreateJellyMazeHardWall(objectName)
                : CreateCandyParkHardWall(objectName);
        }

        private GameObject CreateCandyParkHardWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.92f, 1f, 0.92f);
            collider.center = new Vector3(0f, 0.5f, 0f);
            Transform visualRoot = CreateVisualRoot(root.transform);
            CreatePrimitiveChild(visualRoot, "BottomShadow", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(0.96f, 0.12f, 0.96f), GetShadowMaterial());
            CreatePrimitiveChild(visualRoot, "BaseBlock", PrimitiveType.Cube, new Vector3(0f, 0.46f, 0f), new Vector3(0.9f, 0.86f, 0.9f), GetCreamMaterial());
            CreatePrimitiveChild(visualRoot, "TopHighlight", PrimitiveType.Cube, new Vector3(0f, 0.92f, 0f), new Vector3(0.68f, 0.08f, 0.68f), GetCandyBlueMaterial());
            ConfigureGeneratedWallFeedback(root, visualRoot);
            return root;
        }

        private GameObject CreateJellyMazeHardWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.92f, 1.02f, 0.92f);
            collider.center = new Vector3(0f, 0.51f, 0f);
            Transform visualRoot = CreateVisualRoot(root.transform);
            CreatePrimitiveChild(visualRoot, "BottomShadow", PrimitiveType.Cube, new Vector3(0f, 0.06f, 0f), new Vector3(0.98f, 0.12f, 0.98f), GetJellyDarkMaterial());
            CreatePrimitiveChild(visualRoot, "GlassBlock", PrimitiveType.Cube, new Vector3(0f, 0.48f, 0f), new Vector3(0.88f, 0.88f, 0.88f), GetJellyHardWallMaterial());
            CreatePrimitiveChild(visualRoot, "GlowCap", PrimitiveType.Cube, new Vector3(0f, 0.96f, 0f), new Vector3(0.68f, 0.08f, 0.68f), GetJellyHardWallHighlightMaterial());
            CreatePrimitiveChild(visualRoot, "CornerLight", PrimitiveType.Sphere, new Vector3(-0.28f, 0.78f, -0.28f), new Vector3(0.16f, 0.16f, 0.16f), GetJellyGlowMaterial());
            ConfigureGeneratedWallFeedback(root, visualRoot);
            return root;
        }

        private GameObject CreateFallbackSoftWall(string objectName, MapVisualTheme visualTheme)
        {
            return visualTheme == MapVisualTheme.JellyMaze
                ? CreateJellyMazeSoftWall(objectName)
                : CreateCandyParkSoftWall(objectName);
        }

        private GameObject CreateCandyParkSoftWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.82f, 0.78f, 0.82f);
            collider.center = new Vector3(0f, 0.39f, 0f);
            Transform visualRoot = CreateVisualRoot(root.transform);
            CreatePrimitiveChild(visualRoot, "JellyBody", PrimitiveType.Cube, new Vector3(0f, 0.39f, 0f), new Vector3(0.82f, 0.78f, 0.82f), GetJellyBlueMaterial());
            CreatePrimitiveChild(visualRoot, "JellyShine", PrimitiveType.Sphere, new Vector3(-0.22f, 0.68f, -0.22f), new Vector3(0.18f, 0.08f, 0.18f), GetCreamMaterial());
            ConfigureGeneratedWallFeedback(root, visualRoot);
            return root;
        }

        private GameObject CreateJellyMazeSoftWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.82f, 0.78f, 0.82f);
            collider.center = new Vector3(0f, 0.39f, 0f);
            Transform visualRoot = CreateVisualRoot(root.transform);
            CreatePrimitiveChild(visualRoot, "JellyCore", PrimitiveType.Cube, new Vector3(0f, 0.39f, 0f), new Vector3(0.82f, 0.78f, 0.82f), GetJellySoftWallMaterial());
            CreatePrimitiveChild(visualRoot, "GlowStripe_X", PrimitiveType.Cube, new Vector3(0f, 0.58f, -0.42f), new Vector3(0.62f, 0.08f, 0.04f), GetJellyGlowMaterial());
            CreatePrimitiveChild(visualRoot, "GlowStripe_Z", PrimitiveType.Cube, new Vector3(-0.42f, 0.34f, 0f), new Vector3(0.04f, 0.08f, 0.62f), GetJellyPropCyanMaterial());
            CreatePrimitiveChild(visualRoot, "BubbleShine", PrimitiveType.Sphere, new Vector3(-0.24f, 0.66f, -0.24f), new Vector3(0.2f, 0.08f, 0.2f), GetJellyHardWallHighlightMaterial());
            ConfigureGeneratedWallFeedback(root, visualRoot);
            return root;
        }

        private Transform ResolveGeneratedRoot(BattleMapType mapType, bool createIfMissing = true)
        {
            string rootName = GetGeneratedRootName(mapType);
            if (generatedMapRoot != null && generatedMapRoot.name == rootName)
            {
                return generatedMapRoot;
            }

            Transform existingRoot = transform.Find(rootName);
            if (existingRoot != null)
            {
                generatedMapRoot = existingRoot;
                return generatedMapRoot;
            }

            if (!createIfMissing)
            {
                return null;
            }

            GameObject rootObject = new GameObject(rootName);
            generatedMapRoot = rootObject.transform;
            generatedMapRoot.SetParent(transform);
            generatedMapRoot.localPosition = Vector3.zero;
            generatedMapRoot.localRotation = Quaternion.identity;
            generatedMapRoot.localScale = Vector3.one;
            return generatedMapRoot;
        }

        private void ClearInactiveGeneratedRoots(Transform activeRoot)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child == activeRoot)
                {
                    continue;
                }

                if (child.name.StartsWith(GeneratedRootPrefix, System.StringComparison.Ordinal))
                {
                    DestroyGeneratedObject(child.gameObject);
                }
            }
        }

        private Transform CreateChildRoot(Transform parent, string rootName)
        {
            GameObject rootObject = new GameObject(rootName);
            Transform childRoot = rootObject.transform;
            childRoot.SetParent(parent);
            childRoot.localPosition = Vector3.zero;
            childRoot.localRotation = Quaternion.identity;
            childRoot.localScale = Vector3.one;
            return childRoot;
        }

        private void ClearGeneratedChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                DestroyGeneratedObject(root.GetChild(i).gameObject);
            }
        }

        private void SetLegacySceneVisualRootsActive(Transform generatedRoot, bool isActive)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child == generatedRoot || child.name.StartsWith(GeneratedRootPrefix, System.StringComparison.Ordinal))
                {
                    continue;
                }

                if (child.name == "Ground_CandyParkBoard" || child.name == "WallVisualsRoot")
                {
                    child.gameObject.SetActive(isActive);
                }
            }
        }

        private void DestroyGeneratedObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private MapVisualTheme ResolveVisualTheme(BattleMapType mapType)
        {
            return mapType == BattleMapType.Maze ? MapVisualTheme.JellyMaze : MapVisualTheme.CandyPark;
        }

        private string GetGeneratedRootName(BattleMapType mapType)
        {
            return GeneratedRootPrefix + GetVisualThemeKey(ResolveVisualTheme(mapType));
        }

        private string GetVisualThemeKey(MapVisualTheme visualTheme)
        {
            return visualTheme == MapVisualTheme.JellyMaze ? "JellyMaze" : "CandyPark";
        }

        private string GetVisualThemeDisplayName(MapVisualTheme visualTheme)
        {
            return visualTheme == MapVisualTheme.JellyMaze ? "Jelly Maze" : "Candy Park";
        }

        private Color GetThemeAccentColor(BattleMapType mapType, int index)
        {
            switch (mapType)
            {
                case BattleMapType.OpenField:
                    return index % 2 == 0 ? new Color(0.5f, 0.95f, 0.42f) : new Color(0.38f, 0.88f, 1f);
                case BattleMapType.Maze:
                    return index % 2 == 0 ? new Color(0.75f, 0.55f, 1f) : new Color(1f, 0.52f, 0.78f);
                default:
                    return index % 2 == 0 ? new Color(1f, 0.52f, 0.78f) : new Color(0.35f, 0.85f, 1f);
            }
        }

        private Material GetGrassMaterial()
        {
            if (grassMaterial == null)
            {
                grassMaterial = CreateRuntimeMaterial("Mat_Runtime_Tile_GrassPastel", new Color(0.57f, 0.91f, 0.65f));
            }

            return grassMaterial;
        }

        private Material GetCandyBlueMaterial()
        {
            if (candyBlueMaterial == null)
            {
                candyBlueMaterial = CreateRuntimeMaterial("Mat_Runtime_Tile_CandyBlue", new Color(0.48f, 0.86f, 1f));
            }

            return candyBlueMaterial;
        }

        private Material GetCreamMaterial()
        {
            if (creamMaterial == null)
            {
                creamMaterial = CreateRuntimeMaterial("Mat_Runtime_Cream", new Color(1f, 0.95f, 0.76f));
            }

            return creamMaterial;
        }

        private Material GetJellyBlueMaterial()
        {
            if (jellyBlueMaterial == null)
            {
                jellyBlueMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyBlue", new Color(0.36f, 0.86f, 1f));
            }

            return jellyBlueMaterial;
        }

        private Material GetShadowMaterial()
        {
            if (shadowMaterial == null)
            {
                shadowMaterial = CreateRuntimeMaterial("Mat_Runtime_WarmShadow", new Color(0.45f, 0.35f, 0.24f));
            }

            return shadowMaterial;
        }

        private Material GetPropPinkMaterial()
        {
            if (propPinkMaterial == null)
            {
                propPinkMaterial = CreateRuntimeMaterial("Mat_Runtime_PropPink", new Color(1f, 0.48f, 0.76f));
            }

            return propPinkMaterial;
        }

        private Material GetPropMintMaterial()
        {
            if (propMintMaterial == null)
            {
                propMintMaterial = CreateRuntimeMaterial("Mat_Runtime_PropMint", new Color(0.48f, 0.95f, 0.72f));
            }

            return propMintMaterial;
        }

        private Material GetPropYellowMaterial()
        {
            if (propYellowMaterial == null)
            {
                propYellowMaterial = CreateRuntimeMaterial("Mat_Runtime_PropYellow", new Color(1f, 0.82f, 0.28f));
            }

            return propYellowMaterial;
        }

        private Material GetJellyFloorMaterial()
        {
            if (jellyFloorMaterial == null)
            {
                jellyFloorMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_FloorViolet", new Color(0.24f, 0.18f, 0.42f));
            }

            return jellyFloorMaterial;
        }

        private Material GetJellyTileInsetMaterial()
        {
            if (jellyTileInsetMaterial == null)
            {
                jellyTileInsetMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_TileCyan", new Color(0.18f, 0.78f, 0.95f), true, 0.55f);
            }

            return jellyTileInsetMaterial;
        }

        private Material GetJellyHardWallMaterial()
        {
            if (jellyHardWallMaterial == null)
            {
                jellyHardWallMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_HardWallViolet", new Color(0.43f, 0.34f, 0.9f), true, 0.2f);
            }

            return jellyHardWallMaterial;
        }

        private Material GetJellyHardWallHighlightMaterial()
        {
            if (jellyHardWallHighlightMaterial == null)
            {
                jellyHardWallHighlightMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_HardWallHighlight", new Color(0.78f, 0.96f, 1f), true, 0.65f);
            }

            return jellyHardWallHighlightMaterial;
        }

        private Material GetJellySoftWallMaterial()
        {
            if (jellySoftWallMaterial == null)
            {
                jellySoftWallMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_SoftWallMagenta", new Color(1f, 0.33f, 0.82f), true, 0.35f);
            }

            return jellySoftWallMaterial;
        }

        private Material GetJellyGlowMaterial()
        {
            if (jellyGlowMaterial == null)
            {
                jellyGlowMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_GlowCream", new Color(0.78f, 1f, 0.96f), true, 0.95f);
            }

            return jellyGlowMaterial;
        }

        private Material GetJellyDarkMaterial()
        {
            if (jellyDarkMaterial == null)
            {
                jellyDarkMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_DarkFrame", new Color(0.12f, 0.1f, 0.22f));
            }

            return jellyDarkMaterial;
        }

        private Material GetJellyPropCyanMaterial()
        {
            if (jellyPropCyanMaterial == null)
            {
                jellyPropCyanMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_PropCyan", new Color(0.18f, 0.92f, 1f), true, 0.75f);
            }

            return jellyPropCyanMaterial;
        }

        private Material GetJellyPropPinkMaterial()
        {
            if (jellyPropPinkMaterial == null)
            {
                jellyPropPinkMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_PropPink", new Color(1f, 0.42f, 0.9f), true, 0.55f);
            }

            return jellyPropPinkMaterial;
        }

        private Material CreateRuntimeMaterial(string materialName, Color color, bool useEmission = false, float emissionIntensity = 0f)
        {
            Material material = new Material(Shader.Find("Standard"))
            {
                name = materialName,
                color = color,
                hideFlags = HideFlags.HideAndDontSave
            };

            if (useEmission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * Mathf.Max(0.1f, emissionIntensity));
            }

            return material;
        }
    }
}
