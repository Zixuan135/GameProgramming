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
            SnowfieldPlayground,
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
        // Generated Candy Park pieces now carry the richer illustrated-board style by default.
        [SerializeField] private bool useCandyParkPrefabs;

        [Header("Generation")]
        [SerializeField] private Transform generatedMapRoot;
        [SerializeField] private bool hideSceneAuthoredVisualRoots = true;
        [SerializeField] private bool generateGroundTiles = true;
        [SerializeField] private bool generateWalls = true;
        [SerializeField] private bool generateDecorations = true;
        [SerializeField, Min(0f)] private float decorationOuterPadding = 0.85f;

        private Material grassMaterial;
        private Material candyBlueMaterial;
        private Material candyTileInsetMaterial;
        private Material candyCreamEdgeMaterial;
        private Material candyGlossMaterial;
        private Material candyWaferMaterial;
        private Material candyWaferShadowMaterial;
        private Material candyJellyDeepMaterial;
        private Material candyJellyGlowMaterial;
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
        private Material snowFloorMaterial;
        private Material snowTileInsetMaterial;
        private Material snowHardWallMaterial;
        private Material snowHardWallHighlightMaterial;
        private Material snowSoftWallMaterial;
        private Material snowSoftWallRibbonMaterial;
        private Material snowIceMaterial;
        private Material snowWoodMaterial;
        private Material snowPropBlueMaterial;
        private Material snowPropPinkMaterial;
        private Material snowTileEdgeMaterial;
        private Material snowPackedShadowMaterial;
        private Material snowflakeMarkMaterial;
        private Material snowGiftSideMaterial;

        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public float CellSize => cellSize;
        public Transform GeneratedMapRoot => generatedMapRoot;

        /// <summary>
        /// Purpose: Performs generate for this component.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        public void Generate(BattleMapType mapType)
        {
            MapManager mapManager = GetComponent<MapManager>();
            Generate(mapType, mapManager);
        }

        /// <summary>
        /// Purpose: Performs generate for this component.
        /// Inputs: `mapType`, `mapManager`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <param name="mapManager">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Performs clear for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Performs generate grid visuals for this component.
        /// Inputs: `mapManager`, `groundRoot`, `hardWallRoot`, `softWallRoot`, `visualTheme`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapManager">Input value used by this method.</param>
        /// <param name="groundRoot">Input value used by this method.</param>
        /// <param name="hardWallRoot">Input value used by this method.</param>
        /// <param name="softWallRoot">Input value used by this method.</param>
        /// <param name="visualTheme">Input value used by this method.</param>
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
                        GameObject selectedGroundPrefab = visualTheme == MapVisualTheme.CandyPark && useCandyParkPrefabs ? groundTilePrefab : null;
                        SpawnMapPiece(selectedGroundPrefab, groundRoot, mapManager.GridToWorld(gridPosition), $"Tile_{x:00}_{y:00}", name => CreateFallbackGroundTile(name, visualTheme));
                    }

                    if (!generateWalls)
                    {
                        continue;
                    }

                    if (cell.IsHardWall)
                    {
                        GameObject selectedHardWallPrefab = visualTheme == MapVisualTheme.CandyPark && useCandyParkPrefabs ? hardWallPrefab : null;
                        SpawnMapPiece(selectedHardWallPrefab, hardWallRoot, mapManager.GridToWorld(gridPosition), $"Wall_Hard_{x:00}_{y:00}", name => CreateFallbackHardWall(name, visualTheme));
                    }
                    else if (cell.IsSoftWall)
                    {
                        GameObject selectedSoftWallPrefab = visualTheme == MapVisualTheme.CandyPark && useCandyParkPrefabs ? softWallPrefab : null;
                        GameObject softWall = SpawnMapPiece(selectedSoftWallPrefab, softWallRoot, mapManager.GridToWorld(gridPosition), $"Wall_Soft_{x:00}_{y:00}", name => CreateFallbackSoftWall(name, visualTheme));
                        mapManager.RegisterSoftWallObject(gridPosition, softWall);
                    }
                }
            }
        }

        /// <summary>
        /// Purpose: Spawns map piece.
        /// Inputs: `prefab`, `parent`, `position`, `objectName`, `fallbackFactory`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="prefab">Input value used by this method.</param>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="fallbackFactory">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
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

        /// <summary>
        /// Purpose: Performs generate decorations for this component.
        /// Inputs: `mapType`, `visualTheme`, `decorationRoot`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <param name="visualTheme">Input value used by this method.</param>
        /// <param name="decorationRoot">Input value used by this method.</param>
        private void GenerateDecorations(BattleMapType mapType, MapVisualTheme visualTheme, Transform decorationRoot)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    GenerateSnowfieldDecorations(decorationRoot);
                    break;
                case MapVisualTheme.JellyMaze:
                    GenerateJellyMazeDecorations(decorationRoot);
                    break;
                default:
                    GenerateCandyParkDecorations(mapType, decorationRoot);
                    break;
            }
        }

        /// <summary>
        /// Purpose: Performs generate single player goal visual for this component.
        /// Inputs: `mapManager`, `goalRoot`, `visualTheme`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapManager">Input value used by this method.</param>
        /// <param name="goalRoot">Input value used by this method.</param>
        /// <param name="visualTheme">Input value used by this method.</param>
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

            Material baseMaterial = ResolveGoalBaseMaterial(visualTheme);
            Material accentMaterial = ResolveGoalAccentMaterial(visualTheme);
            Material flagMaterial = ResolveGoalFlagMaterial(visualTheme);

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

        /// <summary>
        /// Purpose: Performs generate candy park decorations for this component.
        /// Inputs: `mapType`, `decorationRoot`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <param name="decorationRoot">Input value used by this method.</param>
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
            CreateCandyPlanet(decorationRoot, new Vector3(maxX - 0.22f, 0f, minZ + 0.05f), "CandyPlanet_SouthEast", GetPropMintMaterial());
            CreateCandyPlanet(decorationRoot, new Vector3(minX + 0.22f, 0f, centerZ + cellSize * 0.35f), "CandyPlanet_WestRail", GetCandyBlueMaterial());
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

        /// <summary>
        /// Purpose: Creates fence line.
        /// Inputs: `parent`, `center`, `lengthCells`, `horizontal`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="center">Input value used by this method.</param>
        /// <param name="lengthCells">Input value used by this method.</param>
        /// <param name="horizontal">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
        private void CreateFenceLine(Transform parent, Vector3 center, int lengthCells, bool horizontal, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = center;

            float length = Mathf.Max(1, lengthCells - 1) * cellSize + 0.35f;
            Vector3 railScale = new Vector3(0.055f, length * 0.5f, 0.055f);
            Vector3 railRotation = horizontal ? new Vector3(0f, 0f, 90f) : new Vector3(90f, 0f, 0f);
            Vector3 postStep = horizontal ? Vector3.right * cellSize * 2f : Vector3.forward * cellSize * 2f;
            int postCount = Mathf.Max(2, Mathf.CeilToInt(lengthCells / 2f) + 1);
            Vector3 start = -postStep * ((postCount - 1) * 0.5f);

            GameObject topRail = CreatePrimitiveChild(root.transform, "Rail_Top_CreamRod", PrimitiveType.Cylinder, new Vector3(0f, 0.58f, 0f), railScale, GetCandyGlossMaterial());
            topRail.transform.localEulerAngles = railRotation;
            GameObject bottomRail = CreatePrimitiveChild(root.transform, "Rail_Bottom_CreamRod", PrimitiveType.Cylinder, new Vector3(0f, 0.34f, 0f), railScale, GetCreamMaterial());
            bottomRail.transform.localEulerAngles = railRotation;

            for (int i = 0; i < postCount; i++)
            {
                Vector3 localPosition = start + postStep * i + Vector3.up * 0.38f;
                CreatePrimitiveChild(root.transform, $"Post_Cream_{i:00}", PrimitiveType.Cylinder, localPosition, new Vector3(0.07f, 0.36f, 0.07f), GetCreamMaterial());
                CreatePrimitiveChild(root.transform, $"Post_Cap_{i:00}", PrimitiveType.Sphere, localPosition + Vector3.up * 0.39f, new Vector3(0.16f, 0.12f, 0.16f), i % 2 == 0 ? GetPropYellowMaterial() : GetCandyBlueMaterial());
            }
        }

        /// <summary>
        /// Purpose: Creates lollipop tree.
        /// Inputs: `parent`, `position`, `objectName`, `candyColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="candyColor">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates balloon cluster.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates a candy planet ornament similar to the reference board rail decorations.
        /// Inputs: `parent`, `position`, `objectName`, `planetMaterial`; may also read serialized fields and current runtime state.
        /// Output: no return value; creates a visual-only ornament.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="planetMaterial">Input value used by this method.</param>
        private void CreateCandyPlanet(Transform parent, Vector3 position, string objectName, Material planetMaterial)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "Tripod_Left", PrimitiveType.Cylinder, new Vector3(-0.18f, 0.24f, 0f), new Vector3(0.035f, 0.28f, 0.035f), GetCreamMaterial()).transform.localEulerAngles = new Vector3(0f, 0f, -18f);
            CreatePrimitiveChild(root.transform, "Tripod_Right", PrimitiveType.Cylinder, new Vector3(0.18f, 0.24f, 0f), new Vector3(0.035f, 0.28f, 0.035f), GetCreamMaterial()).transform.localEulerAngles = new Vector3(0f, 0f, 18f);

            GameObject ornament = new GameObject("PlanetOrnament");
            Transform ornamentTransform = ornament.transform;
            ornamentTransform.SetParent(root.transform);
            ornamentTransform.localPosition = Vector3.zero;
            ornamentTransform.localRotation = Quaternion.identity;
            ornamentTransform.localScale = Vector3.one;

            CreatePrimitiveChild(ornamentTransform, "PlanetCandy", PrimitiveType.Sphere, new Vector3(0f, 0.78f, 0f), new Vector3(0.42f, 0.34f, 0.42f), planetMaterial);
            GameObject ring = CreatePrimitiveChild(ornamentTransform, "CandyRing", PrimitiveType.Cylinder, new Vector3(0f, 0.78f, 0f), new Vector3(0.52f, 0.018f, 0.52f), GetPropYellowMaterial());
            ring.transform.localEulerAngles = new Vector3(18f, 0f, 90f);
            CreatePrimitiveChild(ornamentTransform, "PlanetHighlight", PrimitiveType.Sphere, new Vector3(-0.18f, 0.92f, -0.16f), new Vector3(0.14f, 0.12f, 0.14f), GetCandyGlossMaterial());
            AddDecorationAnimation(root, ornamentTransform, true, 0.045f, 2.1f, new Vector3(0f, 26f, 0f), true, 0.04f, 2.8f);
        }

        /// <summary>
        /// Purpose: Creates round bush.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
        private void CreateRoundBush(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;
            CreatePrimitiveChild(root.transform, "Bush_Left", PrimitiveType.Sphere, new Vector3(-0.28f, 0.25f, 0f), new Vector3(0.58f, 0.38f, 0.48f), GetPropMintMaterial());
            CreatePrimitiveChild(root.transform, "Bush_Center", PrimitiveType.Sphere, new Vector3(0.08f, 0.32f, 0f), new Vector3(0.72f, 0.48f, 0.58f), GetPropMintMaterial());
            CreatePrimitiveChild(root.transform, "Bush_Right", PrimitiveType.Sphere, new Vector3(0.48f, 0.24f, 0f), new Vector3(0.48f, 0.32f, 0.42f), GetPropMintMaterial());
        }

        /// <summary>
        /// Purpose: Creates sign board.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates small candy tree.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates toy barrel.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates candy lamp.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates background cloud.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Generates decorative snowfield props outside the playable grid.
        /// Inputs: decorationRoot receives visual-only objects around the arena border.
        /// Output: no return value; creates snow fences, pine trees, gifts, lamps, and snow clouds.
        /// </summary>
        /// <param name="decorationRoot">Parent transform for generated snowfield decorations.</param>
        private void GenerateSnowfieldDecorations(Transform decorationRoot)
        {
            float minX = -decorationOuterPadding;
            float maxX = (mapWidth - 1) * cellSize + decorationOuterPadding;
            float minZ = -decorationOuterPadding;
            float maxZ = (mapHeight - 1) * cellSize + decorationOuterPadding;
            float centerX = (mapWidth - 1) * cellSize * 0.5f;
            float centerZ = (mapHeight - 1) * cellSize * 0.5f;

            CreateSnowFenceLine(decorationRoot, new Vector3(centerX, 0f, minZ), mapWidth, true, "SnowFence_South");
            CreateSnowFenceLine(decorationRoot, new Vector3(centerX, 0f, maxZ), mapWidth, true, "SnowFence_North");
            CreateSnowFenceLine(decorationRoot, new Vector3(minX, 0f, centerZ), mapHeight, false, "SnowFence_West");
            CreateSnowFenceLine(decorationRoot, new Vector3(maxX, 0f, centerZ), mapHeight, false, "SnowFence_East");

            CreateSnowPineTree(decorationRoot, new Vector3(minX + 0.15f, 0f, minZ + 0.2f), "SnowPine_SouthWest", GetSnowPropBlueMaterial());
            CreateSnowPineTree(decorationRoot, new Vector3(maxX - 0.15f, 0f, maxZ - 0.2f), "SnowPine_NorthEast", GetSnowPropPinkMaterial());
            CreateSnowman(decorationRoot, new Vector3(minX + 0.25f, 0f, centerZ + cellSize * 1.5f), "Snowman_West");
            CreateSnowman(decorationRoot, new Vector3(maxX - 0.25f, 0f, centerZ - cellSize * 1.4f), "Snowman_East");
            CreateGiftCrate(decorationRoot, new Vector3(centerX - cellSize * 2.2f, 0f, minZ + 0.12f), "GiftCrate_SouthWest", GetSnowPropPinkMaterial());
            CreateGiftCrate(decorationRoot, new Vector3(centerX + cellSize * 2.1f, 0f, maxZ - 0.12f), "GiftCrate_NorthEast", GetSnowPropBlueMaterial());
            CreateIceLamp(decorationRoot, new Vector3(centerX, 0f, maxZ - 0.08f), "IceLamp_North");
            CreateIceLamp(decorationRoot, new Vector3(centerX, 0f, minZ + 0.08f), "IceLamp_South");
            CreateSnowCloud(decorationRoot, new Vector3(minX + 0.4f, 1.35f, maxZ - 0.05f), "SnowCloud_NorthWest");
            CreateSnowCloud(decorationRoot, new Vector3(maxX - 0.4f, 1.25f, minZ + 0.05f), "SnowCloud_SouthEast");
        }

        /// <summary>
        /// Purpose: Creates a frosty fence line that frames the Snowfield map without blocking gameplay.
        /// Inputs: parent stores the object; center, lengthCells, and horizontal define placement.
        /// Output: no return value; creates a decorative rail and posts.
        /// </summary>
        /// <param name="parent">Parent transform for the fence line.</param>
        /// <param name="center">World position for the fence line root.</param>
        /// <param name="lengthCells">Length measured in grid cells.</param>
        /// <param name="horizontal">True for an X-axis fence; false for a Z-axis fence.</param>
        /// <param name="objectName">Generated object name.</param>
        private void CreateSnowFenceLine(Transform parent, Vector3 center, int lengthCells, bool horizontal, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = center;

            float length = Mathf.Max(1, lengthCells - 1) * cellSize + 0.4f;
            Vector3 railScale = new Vector3(0.052f, length * 0.5f, 0.052f);
            Vector3 railRotation = horizontal ? new Vector3(0f, 0f, 90f) : new Vector3(90f, 0f, 0f);
            Vector3 postStep = horizontal ? Vector3.right * cellSize * 2f : Vector3.forward * cellSize * 2f;
            int postCount = Mathf.Max(2, Mathf.CeilToInt(lengthCells / 2f) + 1);
            Vector3 start = -postStep * ((postCount - 1) * 0.5f);

            GameObject topRail = CreatePrimitiveChild(root.transform, "Rail_WoodTop", PrimitiveType.Cylinder, new Vector3(0f, 0.58f, 0f), railScale, GetSnowWoodMaterial());
            topRail.transform.localEulerAngles = railRotation;
            GameObject bottomRail = CreatePrimitiveChild(root.transform, "Rail_WoodBase", PrimitiveType.Cylinder, new Vector3(0f, 0.32f, 0f), railScale, GetSnowWoodMaterial());
            bottomRail.transform.localEulerAngles = railRotation;
            GameObject snowRidge = CreatePrimitiveChild(root.transform, "Rail_SnowRidge", PrimitiveType.Cylinder, new Vector3(0f, 0.66f, 0f), new Vector3(0.046f, length * 0.46f, 0.046f), GetSnowFloorMaterial());
            snowRidge.transform.localEulerAngles = railRotation;

            for (int i = 0; i < postCount; i++)
            {
                Vector3 localPosition = start + postStep * i + Vector3.up * 0.38f;
                CreatePrimitiveChild(root.transform, $"Post_Wood_{i:00}", PrimitiveType.Cylinder, localPosition, new Vector3(0.07f, 0.36f, 0.07f), GetSnowWoodMaterial());
                CreatePrimitiveChild(root.transform, $"Post_SnowCap_{i:00}", PrimitiveType.Sphere, localPosition + Vector3.up * 0.4f, new Vector3(0.2f, 0.12f, 0.2f), GetSnowFloorMaterial());
                if (i % 2 == 0)
                {
                    CreatePrimitiveChild(root.transform, $"Post_IceBead_{i:00}", PrimitiveType.Sphere, localPosition + Vector3.up * 0.2f, new Vector3(0.09f, 0.09f, 0.09f), GetSnowIceMaterial());
                }
            }
        }

        /// <summary>
        /// Purpose: Creates a simple chibi pine tree with snow caps.
        /// Inputs: parent, position, objectName, and accentMaterial define the tree.
        /// Output: no return value; creates a visual-only tree object.
        /// </summary>
        /// <param name="parent">Parent transform for the tree.</param>
        /// <param name="position">World position outside the grid.</param>
        /// <param name="objectName">Generated object name.</param>
        /// <param name="accentMaterial">Color accent for ornaments.</param>
        private void CreateSnowPineTree(Transform parent, Vector3 position, string objectName, Material accentMaterial)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "Trunk", PrimitiveType.Cylinder, new Vector3(0f, 0.34f, 0f), new Vector3(0.16f, 0.34f, 0.16f), GetSnowWoodMaterial());
            CreatePrimitiveChild(root.transform, "Pine_Lower", PrimitiveType.Cylinder, new Vector3(0f, 0.72f, 0f), new Vector3(0.52f, 0.28f, 0.52f), GetSnowPropBlueMaterial());
            CreatePrimitiveChild(root.transform, "Pine_Middle", PrimitiveType.Cylinder, new Vector3(0f, 0.98f, 0f), new Vector3(0.4f, 0.24f, 0.4f), GetSnowIceMaterial());
            CreatePrimitiveChild(root.transform, "Pine_Top", PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(0.28f, 0.22f, 0.28f), GetSnowPropBlueMaterial());
            CreatePrimitiveChild(root.transform, "SnowCap", PrimitiveType.Sphere, new Vector3(0f, 1.35f, 0f), new Vector3(0.28f, 0.1f, 0.28f), GetSnowFloorMaterial());
            CreatePrimitiveChild(root.transform, "TinyOrnament", PrimitiveType.Sphere, new Vector3(0.18f, 1.02f, -0.2f), new Vector3(0.12f, 0.12f, 0.12f), accentMaterial);
        }

        /// <summary>
        /// Purpose: Creates a readable snowman decoration.
        /// Inputs: parent, position, and objectName define placement and naming.
        /// Output: no return value; creates a visual-only snowman.
        /// </summary>
        /// <param name="parent">Parent transform for the snowman.</param>
        /// <param name="position">World position outside the grid.</param>
        /// <param name="objectName">Generated object name.</param>
        private void CreateSnowman(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "Body", PrimitiveType.Sphere, new Vector3(0f, 0.42f, 0f), new Vector3(0.5f, 0.42f, 0.5f), GetSnowFloorMaterial());
            CreatePrimitiveChild(root.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.88f, 0f), new Vector3(0.34f, 0.34f, 0.34f), GetSnowFloorMaterial());
            CreatePrimitiveChild(root.transform, "Scarf", PrimitiveType.Cube, new Vector3(0f, 0.7f, -0.27f), new Vector3(0.48f, 0.08f, 0.04f), GetSnowPropPinkMaterial());
            CreatePrimitiveChild(root.transform, "Nose", PrimitiveType.Cube, new Vector3(0f, 0.88f, -0.32f), new Vector3(0.08f, 0.06f, 0.14f), GetPropYellowMaterial());
            AddDecorationAnimation(root, root.transform, true, 0.025f, 1.6f, new Vector3(0f, 8f, 0f), false, 0f, 1f);
        }

        /// <summary>
        /// Purpose: Creates a gift-style crate decoration that matches the snowfield soft wall language.
        /// Inputs: parent, position, objectName, and ribbonMaterial define the crate.
        /// Output: no return value; creates a decorative gift crate.
        /// </summary>
        /// <param name="parent">Parent transform for the gift crate.</param>
        /// <param name="position">World position outside the grid.</param>
        /// <param name="objectName">Generated object name.</param>
        /// <param name="ribbonMaterial">Ribbon material color.</param>
        private void CreateGiftCrate(Transform parent, Vector3 position, string objectName, Material ribbonMaterial)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "GiftShadow", PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), new Vector3(0.62f, 0.08f, 0.62f), GetSnowPackedShadowMaterial());
            CreatePrimitiveChild(root.transform, "GiftBody", PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), new Vector3(0.56f, 0.46f, 0.56f), GetSnowSoftWallMaterial());
            CreatePrimitiveChild(root.transform, "GiftSideShade", PrimitiveType.Cube, new Vector3(0f, 0.3f, -0.29f), new Vector3(0.4f, 0.36f, 0.028f), GetSnowGiftSideMaterial());
            CreatePrimitiveChild(root.transform, "Ribbon_X", PrimitiveType.Cube, new Vector3(0f, 0.32f, -0.305f), new Vector3(0.1f, 0.4f, 0.036f), ribbonMaterial);
            CreatePrimitiveChild(root.transform, "Ribbon_Z", PrimitiveType.Cube, new Vector3(-0.305f, 0.32f, 0f), new Vector3(0.036f, 0.4f, 0.1f), ribbonMaterial);
            CreatePrimitiveChild(root.transform, "SnowCap", PrimitiveType.Cube, new Vector3(0f, 0.56f, 0f), new Vector3(0.54f, 0.06f, 0.54f), GetSnowFloorMaterial());
            CreatePrimitiveChild(root.transform, "Bow_Left", PrimitiveType.Sphere, new Vector3(-0.12f, 0.63f, -0.18f), new Vector3(0.14f, 0.06f, 0.1f), ribbonMaterial);
            CreatePrimitiveChild(root.transform, "Bow_Right", PrimitiveType.Sphere, new Vector3(0.12f, 0.63f, -0.18f), new Vector3(0.14f, 0.06f, 0.1f), ribbonMaterial);
        }

        /// <summary>
        /// Purpose: Creates a glowing ice lamp decoration.
        /// Inputs: parent, position, and objectName define the lamp.
        /// Output: no return value; creates a gently animated visual-only lamp.
        /// </summary>
        /// <param name="parent">Parent transform for the lamp.</param>
        /// <param name="position">World position outside the grid.</param>
        /// <param name="objectName">Generated object name.</param>
        private void CreateIceLamp(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "LampBase", PrimitiveType.Cylinder, new Vector3(0f, 0.14f, 0f), new Vector3(0.22f, 0.08f, 0.22f), GetSnowWoodMaterial());
            CreatePrimitiveChild(root.transform, "LampPost", PrimitiveType.Cylinder, new Vector3(0f, 0.48f, 0f), new Vector3(0.08f, 0.46f, 0.08f), GetSnowWoodMaterial());
            GameObject lampHead = CreatePrimitiveChild(root.transform, "IceGlow", PrimitiveType.Sphere, new Vector3(0f, 0.96f, 0f), new Vector3(0.32f, 0.32f, 0.32f), GetSnowIceMaterial());
            CreatePrimitiveChild(lampHead.transform, "GlowHighlight", PrimitiveType.Sphere, new Vector3(-0.16f, 0.1f, -0.16f), new Vector3(0.14f, 0.14f, 0.14f), GetCreamMaterial());
            AddDecorationAnimation(root, lampHead.transform, true, 0.03f, 2.2f, Vector3.zero, true, 0.07f, 3.5f);
        }

        /// <summary>
        /// Purpose: Creates a soft floating snow cloud.
        /// Inputs: parent, position, and objectName define placement and naming.
        /// Output: no return value; creates an animated cloud outside the grid.
        /// </summary>
        /// <param name="parent">Parent transform for the snow cloud.</param>
        /// <param name="position">World position outside the grid.</param>
        /// <param name="objectName">Generated object name.</param>
        private void CreateSnowCloud(Transform parent, Vector3 position, string objectName)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreatePrimitiveChild(root.transform, "Cloud_Left", PrimitiveType.Sphere, new Vector3(-0.34f, 0f, 0f), new Vector3(0.5f, 0.28f, 0.26f), GetSnowFloorMaterial());
            CreatePrimitiveChild(root.transform, "Cloud_Center", PrimitiveType.Sphere, new Vector3(0.04f, 0.08f, 0f), new Vector3(0.68f, 0.36f, 0.3f), GetSnowFloorMaterial());
            CreatePrimitiveChild(root.transform, "Cloud_Right", PrimitiveType.Sphere, new Vector3(0.46f, -0.02f, 0f), new Vector3(0.44f, 0.26f, 0.24f), GetSnowTileInsetMaterial());
            CreatePrimitiveChild(root.transform, "SnowDot", PrimitiveType.Sphere, new Vector3(0.02f, -0.24f, 0f), new Vector3(0.08f, 0.08f, 0.08f), GetSnowIceMaterial());
            AddDecorationAnimation(root, root.transform, true, 0.07f, 0.9f, new Vector3(0f, 5f, 0f), false, 0f, 1f);
        }

        /// <summary>
        /// Purpose: Performs generate jelly maze decorations for this component.
        /// Inputs: `decorationRoot`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="decorationRoot">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates neon gate line.
        /// Inputs: `parent`, `center`, `lengthCells`, `horizontal`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="center">Input value used by this method.</param>
        /// <param name="lengthCells">Input value used by this method.</param>
        /// <param name="horizontal">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates jelly crystal cluster.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates glow tube.
        /// Inputs: `parent`, `position`, `objectName`, `horizontal`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="horizontal">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates signal beacon.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates energy barrel.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates floating glow orb.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates holo sign.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates data tower.
        /// Inputs: `parent`, `position`, `objectName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="position">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates primitive child.
        /// Inputs: `parent`, `objectName`, `primitiveType`, `localPosition`, `localScale`, `material`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="primitiveType">Input value used by this method.</param>
        /// <param name="localPosition">Input value used by this method.</param>
        /// <param name="localScale">Input value used by this method.</param>
        /// <param name="material">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
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

        /// <summary>
        /// Purpose: Creates visual root.
        /// Inputs: `parent`; may also read serialized fields and current runtime state.
        /// Output: a `Transform` value.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <returns>a `Transform` value.</returns>
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

        /// <summary>
        /// Purpose: Configures generated wall feedback for the current battle or scene.
        /// Inputs: `wallRoot`, `visualRoot`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="wallRoot">Input value used by this method.</param>
        /// <param name="visualRoot">Input value used by this method.</param>
        private void ConfigureGeneratedWallFeedback(GameObject wallRoot, Transform visualRoot)
        {
            WallFeedback feedback = wallRoot.GetComponent<WallFeedback>();
            if (feedback == null)
            {
                feedback = wallRoot.AddComponent<WallFeedback>();
            }

            feedback.SetVisualRoot(visualRoot);
        }

        /// <summary>
        /// Purpose: Performs add decoration animation for this component.
        /// Inputs: `decorationRoot`, `animatedRoot`, `enableBob`, `bobAmplitude`, `bobSpeed`, `rotationDegreesPerSecond`, `enableScalePulse`, `scalePulseAmount`, `scalePulseSpeed`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="decorationRoot">Input value used by this method.</param>
        /// <param name="animatedRoot">Input value used by this method.</param>
        /// <param name="enableBob">Input value used by this method.</param>
        /// <param name="bobAmplitude">Input value used by this method.</param>
        /// <param name="bobSpeed">Input value used by this method.</param>
        /// <param name="rotationDegreesPerSecond">Input value used by this method.</param>
        /// <param name="enableScalePulse">Input value used by this method.</param>
        /// <param name="scalePulseAmount">Input value used by this method.</param>
        /// <param name="scalePulseSpeed">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates fallback ground tile.
        /// Inputs: `objectName`, `visualTheme`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="visualTheme">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
        private GameObject CreateFallbackGroundTile(string objectName, MapVisualTheme visualTheme)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    return CreateSnowfieldGroundTile(objectName);
                case MapVisualTheme.JellyMaze:
                    return CreateJellyMazeGroundTile(objectName);
                default:
                    return CreateCandyParkGroundTile(objectName);
            }
        }

        /// <summary>
        /// Purpose: Creates candy park ground tile.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
        private GameObject CreateCandyParkGroundTile(string objectName)
        {
            GameObject root = new GameObject(objectName);
            bool checker = TryParseGeneratedGridPosition(objectName, out int x, out int y) && (x + y) % 2 != 0;
            Material insetMaterial = checker ? GetCandyTileInsetMaterial() : GetCandyWaferMaterial();

            CreatePrimitiveChild(root.transform, "TileBase_WaferCream", PrimitiveType.Cube, new Vector3(0f, -0.052f, 0f), new Vector3(0.98f, 0.07f, 0.98f), GetCandyWaferMaterial());
            CreatePrimitiveChild(root.transform, "TileInset_CreamPressed", PrimitiveType.Cube, new Vector3(0f, -0.004f, 0f), new Vector3(0.66f, 0.018f, 0.66f), insetMaterial);
            CreatePrimitiveChild(root.transform, "TileCaramelEdge_Front", PrimitiveType.Cube, new Vector3(0f, -0.004f, -0.49f), new Vector3(0.74f, 0.018f, 0.018f), GetCandyCreamEdgeMaterial());
            CreatePrimitiveChild(root.transform, "TileCaramelEdge_Right", PrimitiveType.Cube, new Vector3(0.49f, -0.004f, 0f), new Vector3(0.018f, 0.018f, 0.74f), GetCandyCreamEdgeMaterial());
            CreatePrimitiveChild(root.transform, "TileSoftShadow_Front", PrimitiveType.Cube, new Vector3(0f, -0.035f, -0.49f), new Vector3(0.82f, 0.025f, 0.02f), GetCandyWaferShadowMaterial());

            if (ShouldAddCandyTileAccent(x, y))
            {
                CreatePrimitiveChild(root.transform, "TinyCandyGem", PrimitiveType.Sphere, new Vector3(0.22f, 0.018f, 0.22f), new Vector3(0.06f, 0.018f, 0.06f), ResolveCandySprinkleMaterial(x + y));
            }

            return root;
        }

        /// <summary>
        /// Purpose: Creates jelly maze ground tile.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
        private GameObject CreateJellyMazeGroundTile(string objectName)
        {
            GameObject root = new GameObject(objectName);
            CreatePrimitiveChild(root.transform, "TileBase_DarkJelly", PrimitiveType.Cube, new Vector3(0f, -0.052f, 0f), new Vector3(0.98f, 0.07f, 0.98f), GetJellyFloorMaterial());
            CreatePrimitiveChild(root.transform, "TileInset_GlowLane", PrimitiveType.Cube, new Vector3(0f, -0.004f, 0f), new Vector3(0.7f, 0.018f, 0.7f), GetJellyTileInsetMaterial());
            CreatePrimitiveChild(root.transform, "TileCenterDot", PrimitiveType.Sphere, new Vector3(0f, 0.018f, 0f), new Vector3(0.12f, 0.02f, 0.12f), GetJellyGlowMaterial());
            return root;
        }

        /// <summary>
        /// Purpose: Creates a snow-and-ice floor tile for the Snowfield Playground theme.
        /// Inputs: `objectName` names the generated tile.
        /// Output: a new themed tile GameObject with no gameplay collider.
        /// </summary>
        /// <param name="objectName">Name assigned to the generated floor tile.</param>
        /// <returns>A Snowfield floor tile GameObject.</returns>
        private GameObject CreateSnowfieldGroundTile(string objectName)
        {
            GameObject root = new GameObject(objectName);
            bool checker = TryParseGeneratedGridPosition(objectName, out int x, out int y) && (x + y) % 2 != 0;
            Material tileFaceMaterial = checker ? GetSnowTileInsetMaterial() : GetSnowFloorMaterial();

            CreatePrimitiveChild(root.transform, "TileBase_FrostedIce", PrimitiveType.Cube, new Vector3(0f, -0.052f, 0f), new Vector3(0.98f, 0.07f, 0.98f), GetSnowTileInsetMaterial());
            CreatePrimitiveChild(root.transform, "TileFace_PaleIce", PrimitiveType.Cube, new Vector3(0f, -0.004f, 0f), new Vector3(0.82f, 0.018f, 0.82f), tileFaceMaterial);
            CreatePrimitiveChild(root.transform, "TileGrout_North", PrimitiveType.Cube, new Vector3(0f, 0.004f, 0.47f), new Vector3(0.86f, 0.014f, 0.018f), GetSnowTileEdgeMaterial());
            CreatePrimitiveChild(root.transform, "TileGrout_East", PrimitiveType.Cube, new Vector3(0.47f, 0.004f, 0f), new Vector3(0.018f, 0.014f, 0.86f), GetSnowTileEdgeMaterial());

            if (ShouldAddSnowflakeTileAccent(x, y))
            {
                CreateSnowflakeMark(root.transform, new Vector3(0f, 0.018f, 0f), $"Snowflake_{x:00}_{y:00}", 0.3f + ((x + y) % 3) * 0.035f);
            }

            return root;
        }

        /// <summary>
        /// Purpose: Chooses which Snowfield floor tiles receive thin snowflake etching.
        /// Inputs: grid coordinates parsed from generated object names.
        /// Output: true when an ice tile should receive a decorative mark.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <returns>True when the tile should receive a snowflake mark.</returns>
        private bool ShouldAddSnowflakeTileAccent(int x, int y)
        {
            return (x + y) % 2 == 0 || (x * 3 + y * 5) % 7 == 0;
        }

        /// <summary>
        /// Purpose: Creates a flat geometric snowflake decal for icy Snowfield floor tiles.
        /// Inputs: parent, local position, object name, and mark size.
        /// Output: no return value; adds visual-only thin primitives.
        /// </summary>
        /// <param name="parent">Parent transform for the snowflake pieces.</param>
        /// <param name="localPosition">Local center of the snowflake.</param>
        /// <param name="objectName">Name prefix for generated pieces.</param>
        /// <param name="size">Approximate arm length.</param>
        private void CreateSnowflakeMark(Transform parent, Vector3 localPosition, string objectName, float size)
        {
            Material markMaterial = GetSnowflakeMarkMaterial();
            GameObject armA = CreatePrimitiveChild(parent, objectName + "_ArmA", PrimitiveType.Cube, localPosition, new Vector3(size, 0.008f, 0.018f), markMaterial);
            armA.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            GameObject armB = CreatePrimitiveChild(parent, objectName + "_ArmB", PrimitiveType.Cube, localPosition, new Vector3(size, 0.008f, 0.018f), markMaterial);
            armB.transform.localEulerAngles = new Vector3(0f, 60f, 0f);
            GameObject armC = CreatePrimitiveChild(parent, objectName + "_ArmC", PrimitiveType.Cube, localPosition, new Vector3(size, 0.008f, 0.018f), markMaterial);
            armC.transform.localEulerAngles = new Vector3(0f, -60f, 0f);
            CreatePrimitiveChild(parent, objectName + "_Core", PrimitiveType.Sphere, localPosition + Vector3.up * 0.002f, new Vector3(0.055f, 0.012f, 0.055f), markMaterial);
        }

        /// <summary>
        /// Purpose: Creates fallback hard wall.
        /// Inputs: `objectName`, `visualTheme`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="visualTheme">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
        private GameObject CreateFallbackHardWall(string objectName, MapVisualTheme visualTheme)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    return CreateSnowfieldHardWall(objectName);
                case MapVisualTheme.JellyMaze:
                    return CreateJellyMazeHardWall(objectName);
                default:
                    return CreateCandyParkHardWall(objectName);
            }
        }

        /// <summary>
        /// Purpose: Creates candy park hard wall.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
        private GameObject CreateCandyParkHardWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.92f, 1f, 0.92f);
            collider.center = new Vector3(0f, 0.5f, 0f);
            Transform visualRoot = CreateVisualRoot(root.transform);
            TryParseGeneratedGridPosition(objectName, out int x, out int y);

            CreatePrimitiveChild(visualRoot, "BottomCaramelShadow", PrimitiveType.Cube, new Vector3(0f, 0.07f, 0f), new Vector3(0.96f, 0.11f, 0.96f), GetCandyWaferShadowMaterial());
            CreatePrimitiveChild(visualRoot, "CreamWaferBody", PrimitiveType.Cube, new Vector3(0f, 0.42f, 0f), new Vector3(0.88f, 0.76f, 0.88f), GetCandyWaferMaterial());
            CreatePrimitiveChild(visualRoot, "PillowTop_Dome", PrimitiveType.Sphere, new Vector3(0f, 0.84f, 0f), new Vector3(0.78f, 0.18f, 0.78f), GetCandyWaferMaterial());
            CreatePrimitiveChild(visualRoot, "WaferTopBloom", PrimitiveType.Sphere, new Vector3(-0.12f, 0.91f, -0.12f), new Vector3(0.34f, 0.06f, 0.34f), GetCandyGlossMaterial());
            CreatePrimitiveChild(visualRoot, "TopCreamButton", PrimitiveType.Cylinder, new Vector3(0f, 0.96f, 0f), new Vector3(0.3f, 0.02f, 0.3f), GetCandyTileInsetMaterial());
            CreatePrimitiveChild(visualRoot, "TopCaramelRim_Front", PrimitiveType.Cube, new Vector3(0f, 0.84f, -0.42f), new Vector3(0.58f, 0.026f, 0.024f), GetCandyCreamEdgeMaterial());
            CreatePrimitiveChild(visualRoot, "TopCaramelRim_Right", PrimitiveType.Cube, new Vector3(0.42f, 0.84f, 0f), new Vector3(0.024f, 0.026f, 0.58f), GetCandyCreamEdgeMaterial());
            CreatePrimitiveChild(visualRoot, "WarmEdge_Front", PrimitiveType.Cube, new Vector3(0f, 0.15f, -0.45f), new Vector3(0.72f, 0.05f, 0.03f), GetCandyCreamEdgeMaterial());
            CreatePrimitiveChild(visualRoot, "WarmEdge_Right", PrimitiveType.Cube, new Vector3(0.45f, 0.15f, 0f), new Vector3(0.03f, 0.05f, 0.72f), GetCandyCreamEdgeMaterial());
            CreatePrimitiveChild(visualRoot, "WaferSideShadow_Front", PrimitiveType.Cube, new Vector3(0f, 0.36f, -0.456f), new Vector3(0.72f, 0.22f, 0.018f), GetCandyWaferShadowMaterial());
            CreatePrimitiveChild(visualRoot, "WaferSideShadow_Right", PrimitiveType.Cube, new Vector3(0.456f, 0.36f, 0f), new Vector3(0.018f, 0.22f, 0.72f), GetCandyWaferShadowMaterial());
            CreatePrimitiveChild(visualRoot, "WaferFaceGloss", PrimitiveType.Cube, new Vector3(-0.2f, 0.66f, -0.457f), new Vector3(0.3f, 0.035f, 0.016f), GetCandyGlossMaterial());

            GameObject veinA = CreatePrimitiveChild(visualRoot, "CaramelVein_Front_A", PrimitiveType.Cube, new Vector3(-0.16f, 0.54f, -0.456f), new Vector3(0.28f, 0.024f, 0.018f), GetCandyCreamEdgeMaterial());
            veinA.transform.localEulerAngles = new Vector3(0f, 0f, 14f);
            GameObject veinB = CreatePrimitiveChild(visualRoot, "CaramelVein_Right_B", PrimitiveType.Cube, new Vector3(0.456f, 0.62f, 0.13f), new Vector3(0.018f, 0.024f, 0.24f), GetCandyCreamEdgeMaterial());
            veinB.transform.localEulerAngles = new Vector3(0f, 18f, 0f);

            CreatePrimitiveChild(visualRoot, "TopCandyGem", PrimitiveType.Sphere, new Vector3(0.16f, 1.02f, 0.06f), new Vector3(0.075f, 0.034f, 0.075f), ResolveCandySprinkleMaterial(x + y));
            GameObject sprinkle = CreatePrimitiveChild(visualRoot, "TopCandySprinkle", PrimitiveType.Cube, new Vector3(-0.18f, 1.01f, -0.1f), new Vector3(0.12f, 0.025f, 0.045f), ResolveCandySprinkleMaterial(x * 3 + y + 1));
            sprinkle.transform.localEulerAngles = new Vector3(0f, 35f, 0f);
            ConfigureGeneratedWallFeedback(root, visualRoot);
            return root;
        }

        /// <summary>
        /// Purpose: Creates jelly maze hard wall.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
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

        /// <summary>
        /// Purpose: Creates an icy packed-snow hard wall for the Snowfield Playground theme.
        /// Inputs: `objectName` names the generated wall.
        /// Output: a hard wall GameObject with a collider and blocked-hit feedback support.
        /// </summary>
        /// <param name="objectName">Name assigned to the generated hard wall.</param>
        /// <returns>A Snowfield hard wall GameObject.</returns>
        private GameObject CreateSnowfieldHardWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.92f, 1.08f, 0.92f);
            collider.center = new Vector3(0f, 0.54f, 0f);

            Transform visualRoot = CreateVisualRoot(root.transform);
            CreatePrimitiveChild(visualRoot, "ColdBaseShadow", PrimitiveType.Cube, new Vector3(0f, 0.055f, 0f), new Vector3(0.98f, 0.11f, 0.98f), GetSnowPackedShadowMaterial());
            CreatePrimitiveChild(visualRoot, "TallColdSnowBlock", PrimitiveType.Cube, new Vector3(0f, 0.48f, 0f), new Vector3(0.9f, 0.86f, 0.9f), GetSnowHardWallMaterial());
            CreatePrimitiveChild(visualRoot, "DarkIceSide_Front", PrimitiveType.Cube, new Vector3(0f, 0.48f, -0.465f), new Vector3(0.78f, 0.68f, 0.03f), GetSnowPackedShadowMaterial());
            CreatePrimitiveChild(visualRoot, "DarkIceSide_Right", PrimitiveType.Cube, new Vector3(0.465f, 0.48f, 0f), new Vector3(0.03f, 0.68f, 0.78f), GetSnowPackedShadowMaterial());
            CreatePrimitiveChild(visualRoot, "WideSnowCap", PrimitiveType.Sphere, new Vector3(0f, 0.92f, 0f), new Vector3(0.86f, 0.24f, 0.86f), GetSnowHardWallHighlightMaterial());
            CreatePrimitiveChild(visualRoot, "TopSoftCenter", PrimitiveType.Sphere, new Vector3(-0.08f, 1f, -0.08f), new Vector3(0.46f, 0.09f, 0.46f), GetSnowFloorMaterial());
            CreatePrimitiveChild(visualRoot, "FrontPackedSnowLip", PrimitiveType.Cube, new Vector3(0f, 0.8f, -0.47f), new Vector3(0.72f, 0.07f, 0.045f), GetSnowHardWallHighlightMaterial());
            CreatePrimitiveChild(visualRoot, "RightPackedSnowLip", PrimitiveType.Cube, new Vector3(0.47f, 0.8f, 0f), new Vector3(0.045f, 0.07f, 0.72f), GetSnowHardWallHighlightMaterial());
            CreatePrimitiveChild(visualRoot, "IceCrack_Front_A", PrimitiveType.Cube, new Vector3(-0.14f, 0.44f, -0.482f), new Vector3(0.34f, 0.022f, 0.018f), GetSnowTileEdgeMaterial()).transform.localEulerAngles = new Vector3(0f, 0f, 24f);
            CreatePrimitiveChild(visualRoot, "IceCrack_Front_B", PrimitiveType.Cube, new Vector3(0.16f, 0.58f, -0.482f), new Vector3(0.24f, 0.02f, 0.018f), GetSnowTileEdgeMaterial()).transform.localEulerAngles = new Vector3(0f, 0f, -28f);
            CreatePrimitiveChild(visualRoot, "CornerSnowBall", PrimitiveType.Sphere, new Vector3(-0.3f, 0.94f, -0.3f), new Vector3(0.16f, 0.11f, 0.16f), GetSnowFloorMaterial());
            CreatePrimitiveChild(visualRoot, "TinyIceChip", PrimitiveType.Sphere, new Vector3(0.22f, 1f, 0.18f), new Vector3(0.07f, 0.035f, 0.07f), GetSnowIceMaterial());
            ConfigureGeneratedWallFeedback(root, visualRoot);
            return root;
        }

        /// <summary>
        /// Purpose: Creates fallback soft wall.
        /// Inputs: `objectName`, `visualTheme`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="visualTheme">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
        private GameObject CreateFallbackSoftWall(string objectName, MapVisualTheme visualTheme)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    return CreateSnowfieldSoftWall(objectName);
                case MapVisualTheme.JellyMaze:
                    return CreateJellyMazeSoftWall(objectName);
                default:
                    return CreateCandyParkSoftWall(objectName);
            }
        }

        /// <summary>
        /// Purpose: Creates candy park soft wall.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
        private GameObject CreateCandyParkSoftWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.82f, 0.78f, 0.82f);
            collider.center = new Vector3(0f, 0.39f, 0f);
            Transform visualRoot = CreateVisualRoot(root.transform);
            CreatePrimitiveChild(visualRoot, "JellyBaseShadow", PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.84f, 0.08f, 0.84f), GetCandyJellyDeepMaterial());
            CreatePrimitiveChild(visualRoot, "JellyBody_GlassBlue", PrimitiveType.Cube, new Vector3(0f, 0.31f, 0f), new Vector3(0.74f, 0.54f, 0.74f), GetJellyBlueMaterial());
            CreatePrimitiveChild(visualRoot, "JellySideShadow_Front", PrimitiveType.Cube, new Vector3(0f, 0.31f, -0.382f), new Vector3(0.58f, 0.36f, 0.028f), GetCandyJellyDeepMaterial());
            CreatePrimitiveChild(visualRoot, "JellySideShadow_Right", PrimitiveType.Cube, new Vector3(0.382f, 0.31f, 0f), new Vector3(0.028f, 0.36f, 0.58f), GetCandyJellyDeepMaterial());
            CreatePrimitiveChild(visualRoot, "JellyBody_FrontGlow", PrimitiveType.Cube, new Vector3(-0.08f, 0.48f, -0.386f), new Vector3(0.42f, 0.09f, 0.018f), GetCandyJellyGlowMaterial());
            CreatePrimitiveChild(visualRoot, "JellyBody_LeftGlow", PrimitiveType.Cube, new Vector3(-0.386f, 0.48f, -0.08f), new Vector3(0.018f, 0.09f, 0.42f), GetCandyJellyGlowMaterial());
            CreatePrimitiveChild(visualRoot, "JellyRoundedCap", PrimitiveType.Sphere, new Vector3(0f, 0.62f, 0f), new Vector3(0.74f, 0.18f, 0.74f), GetJellyBlueMaterial());
            CreatePrimitiveChild(visualRoot, "JellyTopGlow", PrimitiveType.Cylinder, new Vector3(0f, 0.72f, 0f), new Vector3(0.36f, 0.014f, 0.36f), GetCandyJellyGlowMaterial());
            CreatePrimitiveChild(visualRoot, "JellyShine_Large", PrimitiveType.Sphere, new Vector3(-0.22f, 0.67f, -0.22f), new Vector3(0.2f, 0.055f, 0.2f), GetCandyGlossMaterial());
            CreatePrimitiveChild(visualRoot, "JellyShine_Side", PrimitiveType.Cube, new Vector3(0f, 0.54f, -0.39f), new Vector3(0.36f, 0.035f, 0.018f), GetCandyGlossMaterial());
            ConfigureGeneratedWallFeedback(root, visualRoot);
            return root;
        }

        /// <summary>
        /// Purpose: Parses generated tile or wall names like `Tile_03_07` into grid coordinates.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: true when coordinates were found.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <param name="x">Parsed x coordinate.</param>
        /// <param name="y">Parsed y coordinate.</param>
        /// <returns>True if the object name contains generated grid coordinates.</returns>
        private bool TryParseGeneratedGridPosition(string objectName, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (string.IsNullOrEmpty(objectName))
            {
                return false;
            }

            string[] parts = objectName.Split('_');
            if (parts.Length < 3)
            {
                return false;
            }

            return int.TryParse(parts[parts.Length - 2], out x) &&
                   int.TryParse(parts[parts.Length - 1], out y);
        }

        /// <summary>
        /// Purpose: Chooses sparse floor candy accents so the board stays readable.
        /// Inputs: `x`, `y`; may also read serialized fields and current runtime state.
        /// Output: true when a tiny decorative gem should be added.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <returns>True if the tile should receive a small candy accent.</returns>
        private bool ShouldAddCandyTileAccent(int x, int y)
        {
            return (x * 7 + y * 11) % 17 == 0;
        }

        /// <summary>
        /// Purpose: Resolves a rotating candy sprinkle material.
        /// Inputs: `index`; may also read serialized fields and current runtime state.
        /// Output: a candy-colored Material.
        /// </summary>
        /// <param name="index">Input value used by this method.</param>
        /// <returns>A themed sprinkle material.</returns>
        private Material ResolveCandySprinkleMaterial(int index)
        {
            switch (Mathf.Abs(index) % 4)
            {
                case 0:
                    return GetPropPinkMaterial();
                case 1:
                    return GetPropYellowMaterial();
                case 2:
                    return GetCandyBlueMaterial();
                default:
                    return GetPropMintMaterial();
            }
        }

        /// <summary>
        /// Purpose: Creates jelly maze soft wall.
        /// Inputs: `objectName`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="objectName">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
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

        /// <summary>
        /// Purpose: Creates a gift-crate soft wall for the Snowfield Playground theme.
        /// Inputs: `objectName` names the generated wall.
        /// Output: a breakable soft wall GameObject with a collider and destruction feedback support.
        /// </summary>
        /// <param name="objectName">Name assigned to the generated soft wall.</param>
        /// <returns>A Snowfield soft wall GameObject.</returns>
        private GameObject CreateSnowfieldSoftWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.74f, 0.68f, 0.74f);
            collider.center = new Vector3(0f, 0.34f, 0f);

            Transform visualRoot = CreateVisualRoot(root.transform);
            CreatePrimitiveChild(visualRoot, "GiftSmallShadow", PrimitiveType.Cube, new Vector3(0f, 0.045f, 0f), new Vector3(0.78f, 0.08f, 0.78f), GetSnowPackedShadowMaterial());
            CreatePrimitiveChild(visualRoot, "BrightGiftBody", PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), new Vector3(0.68f, 0.52f, 0.68f), GetSnowSoftWallMaterial());
            CreatePrimitiveChild(visualRoot, "GiftWarmSide_Front", PrimitiveType.Cube, new Vector3(0f, 0.3f, -0.352f), new Vector3(0.5f, 0.4f, 0.03f), GetSnowGiftSideMaterial());
            CreatePrimitiveChild(visualRoot, "GiftWarmSide_Right", PrimitiveType.Cube, new Vector3(0.352f, 0.3f, 0f), new Vector3(0.03f, 0.4f, 0.5f), GetSnowGiftSideMaterial());
            CreatePrimitiveChild(visualRoot, "BigBlueRibbon_Front", PrimitiveType.Cube, new Vector3(0f, 0.31f, -0.372f), new Vector3(0.18f, 0.44f, 0.04f), GetSnowSoftWallRibbonMaterial());
            CreatePrimitiveChild(visualRoot, "BigPinkRibbon_Right", PrimitiveType.Cube, new Vector3(0.372f, 0.31f, 0f), new Vector3(0.04f, 0.44f, 0.18f), GetSnowPropPinkMaterial());
            CreatePrimitiveChild(visualRoot, "ThinSnowTop", PrimitiveType.Cube, new Vector3(0f, 0.58f, 0f), new Vector3(0.56f, 0.045f, 0.56f), GetSnowFloorMaterial());
            CreatePrimitiveChild(visualRoot, "SnowDrift_BackLeft", PrimitiveType.Sphere, new Vector3(-0.18f, 0.62f, 0.12f), new Vector3(0.24f, 0.07f, 0.18f), GetSnowHardWallHighlightMaterial());
            CreatePrimitiveChild(visualRoot, "TopRibbon_Blue", PrimitiveType.Cube, new Vector3(0f, 0.62f, -0.02f), new Vector3(0.16f, 0.035f, 0.58f), GetSnowSoftWallRibbonMaterial());
            CreatePrimitiveChild(visualRoot, "Bow_LeftPink", PrimitiveType.Sphere, new Vector3(-0.17f, 0.64f, -0.31f), new Vector3(0.16f, 0.06f, 0.1f), GetSnowPropPinkMaterial());
            CreatePrimitiveChild(visualRoot, "Bow_RightPink", PrimitiveType.Sphere, new Vector3(0.17f, 0.64f, -0.31f), new Vector3(0.16f, 0.06f, 0.1f), GetSnowPropPinkMaterial());
            CreatePrimitiveChild(visualRoot, "BreakableGiftDot", PrimitiveType.Sphere, new Vector3(-0.24f, 0.5f, -0.33f), new Vector3(0.06f, 0.03f, 0.06f), GetSnowPropPinkMaterial());
            ConfigureGeneratedWallFeedback(root, visualRoot);
            return root;
        }

        /// <summary>
        /// Purpose: Resolves generated root from the current runtime state.
        /// Inputs: `mapType`, `createIfMissing`; may also read serialized fields and current runtime state.
        /// Output: a `Transform` value.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <param name="createIfMissing">Input value used by this method.</param>
        /// <returns>a `Transform` value.</returns>
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

        /// <summary>
        /// Purpose: Clears inactive generated roots.
        /// Inputs: `activeRoot`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="activeRoot">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Creates child root.
        /// Inputs: `parent`, `rootName`; may also read serialized fields and current runtime state.
        /// Output: a `Transform` value.
        /// </summary>
        /// <param name="parent">Input value used by this method.</param>
        /// <param name="rootName">Input value used by this method.</param>
        /// <returns>a `Transform` value.</returns>
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

        /// <summary>
        /// Purpose: Clears generated children.
        /// Inputs: `root`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="root">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Sets legacy scene visual roots active.
        /// Inputs: `generatedRoot`, `isActive`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="generatedRoot">Input value used by this method.</param>
        /// <param name="isActive">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Destroys generated object.
        /// Inputs: `target`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="target">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Chooses the base material for the SinglePlayer exit goal.
        /// Inputs: `visualTheme` identifies the active map theme.
        /// Output: a themed Material used by the large goal pad.
        /// </summary>
        /// <param name="visualTheme">Active map visual theme.</param>
        /// <returns>The base material for the goal visual.</returns>
        private Material ResolveGoalBaseMaterial(MapVisualTheme visualTheme)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    return GetSnowTileInsetMaterial();
                case MapVisualTheme.JellyMaze:
                    return GetJellyTileInsetMaterial();
                default:
                    return GetPropMintMaterial();
            }
        }

        /// <summary>
        /// Purpose: Chooses the accent material for the SinglePlayer exit goal.
        /// Inputs: `visualTheme` identifies the active map theme.
        /// Output: a themed Material used by rings, arrows, and the beacon orb.
        /// </summary>
        /// <param name="visualTheme">Active map visual theme.</param>
        /// <returns>The accent material for the goal visual.</returns>
        private Material ResolveGoalAccentMaterial(MapVisualTheme visualTheme)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    return GetSnowIceMaterial();
                case MapVisualTheme.JellyMaze:
                    return GetJellyGlowMaterial();
                default:
                    return GetPropYellowMaterial();
            }
        }

        /// <summary>
        /// Purpose: Chooses the flag material for the SinglePlayer exit goal.
        /// Inputs: `visualTheme` identifies the active map theme.
        /// Output: a themed Material used by the small goal flags.
        /// </summary>
        /// <param name="visualTheme">Active map visual theme.</param>
        /// <returns>The flag material for the goal visual.</returns>
        private Material ResolveGoalFlagMaterial(MapVisualTheme visualTheme)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    return GetSnowPropPinkMaterial();
                case MapVisualTheme.JellyMaze:
                    return GetJellyPropPinkMaterial();
                default:
                    return GetPropPinkMaterial();
            }
        }

        /// <summary>
        /// Purpose: Resolves visual theme from the current runtime state.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: a `MapVisualTheme` value.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <returns>a `MapVisualTheme` value.</returns>
        private MapVisualTheme ResolveVisualTheme(BattleMapType mapType)
        {
            switch (mapType)
            {
                case BattleMapType.OpenField:
                    return MapVisualTheme.SnowfieldPlayground;
                case BattleMapType.Maze:
                    return MapVisualTheme.JellyMaze;
                default:
                    return MapVisualTheme.CandyPark;
            }
        }

        /// <summary>
        /// Purpose: Gets generated root name.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string GetGeneratedRootName(BattleMapType mapType)
        {
            return GeneratedRootPrefix + GetVisualThemeKey(ResolveVisualTheme(mapType));
        }

        /// <summary>
        /// Purpose: Gets visual theme key.
        /// Inputs: `visualTheme`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="visualTheme">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string GetVisualThemeKey(MapVisualTheme visualTheme)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    return "SnowfieldPlayground";
                case MapVisualTheme.JellyMaze:
                    return "JellyMaze";
                default:
                    return "CandyPark";
            }
        }

        /// <summary>
        /// Purpose: Gets visual theme display name.
        /// Inputs: `visualTheme`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="visualTheme">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string GetVisualThemeDisplayName(MapVisualTheme visualTheme)
        {
            switch (visualTheme)
            {
                case MapVisualTheme.SnowfieldPlayground:
                    return "Snowfield Playground";
                case MapVisualTheme.JellyMaze:
                    return "Jelly Maze";
                default:
                    return "Candy Park";
            }
        }

        /// <summary>
        /// Purpose: Gets theme accent color.
        /// Inputs: `mapType`, `index`; may also read serialized fields and current runtime state.
        /// Output: a `Color` value.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <param name="index">Input value used by this method.</param>
        /// <returns>a `Color` value.</returns>
        private Color GetThemeAccentColor(BattleMapType mapType, int index)
        {
            switch (mapType)
            {
                case BattleMapType.OpenField:
                    return index % 2 == 0 ? new Color(0.38f, 0.88f, 1f) : new Color(1f, 0.58f, 0.82f);
                case BattleMapType.Maze:
                    return index % 2 == 0 ? new Color(0.75f, 0.55f, 1f) : new Color(1f, 0.52f, 0.78f);
                default:
                    return index % 2 == 0 ? new Color(1f, 0.52f, 0.78f) : new Color(0.35f, 0.85f, 1f);
            }
        }

        /// <summary>
        /// Purpose: Gets grass material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetGrassMaterial()
        {
            if (grassMaterial == null)
            {
                grassMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_TileMint", new Color(0.48f, 0.74f, 0.6f), false, 0f, 0.32f);
            }

            return grassMaterial;
        }

        /// <summary>
        /// Purpose: Gets candy blue material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCandyBlueMaterial()
        {
            if (candyBlueMaterial == null)
            {
                candyBlueMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_ClearBlue", new Color(0.04f, 0.46f, 0.62f), false, 0f, 0.42f);
            }

            return candyBlueMaterial;
        }

        /// <summary>
        /// Purpose: Gets candy park cream tile inset material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCandyTileInsetMaterial()
        {
            if (candyTileInsetMaterial == null)
            {
                candyTileInsetMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_TileCreamInset", new Color(0.74f, 0.78f, 0.58f), false, 0f, 0.24f);
            }

            return candyTileInsetMaterial;
        }

        /// <summary>
        /// Purpose: Gets candy park caramel edge material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCandyCreamEdgeMaterial()
        {
            if (candyCreamEdgeMaterial == null)
            {
                candyCreamEdgeMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_CaramelEdge", new Color(0.36f, 0.19f, 0.07f), false, 0f, 0.18f);
            }

            return candyCreamEdgeMaterial;
        }

        /// <summary>
        /// Purpose: Gets candy park glossy white highlight material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCandyGlossMaterial()
        {
            if (candyGlossMaterial == null)
            {
                candyGlossMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_CrispGloss", new Color(0.72f, 0.72f, 0.58f), false, 0f, 0.26f);
            }

            return candyGlossMaterial;
        }

        /// <summary>
        /// Purpose: Gets candy park wafer block material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCandyWaferMaterial()
        {
            if (candyWaferMaterial == null)
            {
                candyWaferMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_WaferCream", new Color(0.7f, 0.6f, 0.39f), false, 0f, 0.28f);
            }

            return candyWaferMaterial;
        }

        /// <summary>
        /// Purpose: Gets candy park wafer shadow material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCandyWaferShadowMaterial()
        {
            if (candyWaferShadowMaterial == null)
            {
                candyWaferShadowMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_WaferShadow", new Color(0.27f, 0.15f, 0.06f), false, 0f, 0.14f);
            }

            return candyWaferShadowMaterial;
        }

        /// <summary>
        /// Purpose: Gets candy park deep jelly side material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCandyJellyDeepMaterial()
        {
            if (candyJellyDeepMaterial == null)
            {
                candyJellyDeepMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_JellyDeepBlue", new Color(0f, 0.16f, 0.28f), false, 0f, 0.28f);
            }

            return candyJellyDeepMaterial;
        }

        /// <summary>
        /// Purpose: Gets candy park bright jelly top glow material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCandyJellyGlowMaterial()
        {
            if (candyJellyGlowMaterial == null)
            {
                candyJellyGlowMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_JellyTopHighlight", new Color(0.22f, 0.58f, 0.66f), false, 0f, 0.36f);
            }

            return candyJellyGlowMaterial;
        }

        /// <summary>
        /// Purpose: Gets cream material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetCreamMaterial()
        {
            if (creamMaterial == null)
            {
                creamMaterial = CreateRuntimeMaterial("Mat_Runtime_Cream", new Color(1f, 0.95f, 0.76f));
            }

            return creamMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly blue material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyBlueMaterial()
        {
            if (jellyBlueMaterial == null)
            {
                jellyBlueMaterial = CreateRuntimeMaterial("Mat_Runtime_CandyPark_JellyBlue", new Color(0.02f, 0.36f, 0.52f), false, 0f, 0.36f);
            }

            return jellyBlueMaterial;
        }

        /// <summary>
        /// Purpose: Gets shadow material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetShadowMaterial()
        {
            if (shadowMaterial == null)
            {
                shadowMaterial = CreateRuntimeMaterial("Mat_Runtime_WarmShadow", new Color(0.45f, 0.35f, 0.24f), false, 0f, 0.2f);
            }

            return shadowMaterial;
        }

        /// <summary>
        /// Purpose: Gets prop pink material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetPropPinkMaterial()
        {
            if (propPinkMaterial == null)
            {
                propPinkMaterial = CreateRuntimeMaterial("Mat_Runtime_PropPink", new Color(1f, 0.48f, 0.76f));
            }

            return propPinkMaterial;
        }

        /// <summary>
        /// Purpose: Gets prop mint material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetPropMintMaterial()
        {
            if (propMintMaterial == null)
            {
                propMintMaterial = CreateRuntimeMaterial("Mat_Runtime_PropMint", new Color(0.48f, 0.95f, 0.72f));
            }

            return propMintMaterial;
        }

        /// <summary>
        /// Purpose: Gets prop yellow material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetPropYellowMaterial()
        {
            if (propYellowMaterial == null)
            {
                propYellowMaterial = CreateRuntimeMaterial("Mat_Runtime_PropYellow", new Color(1f, 0.82f, 0.28f));
            }

            return propYellowMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly floor material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyFloorMaterial()
        {
            if (jellyFloorMaterial == null)
            {
                jellyFloorMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_FloorViolet", new Color(0.24f, 0.18f, 0.42f));
            }

            return jellyFloorMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly tile inset material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyTileInsetMaterial()
        {
            if (jellyTileInsetMaterial == null)
            {
                jellyTileInsetMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_TileCyan", new Color(0.18f, 0.78f, 0.95f), true, 0.55f);
            }

            return jellyTileInsetMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly hard wall material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyHardWallMaterial()
        {
            if (jellyHardWallMaterial == null)
            {
                jellyHardWallMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_HardWallViolet", new Color(0.43f, 0.34f, 0.9f), true, 0.2f);
            }

            return jellyHardWallMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly hard wall highlight material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyHardWallHighlightMaterial()
        {
            if (jellyHardWallHighlightMaterial == null)
            {
                jellyHardWallHighlightMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_HardWallHighlight", new Color(0.78f, 0.96f, 1f), true, 0.65f);
            }

            return jellyHardWallHighlightMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly soft wall material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellySoftWallMaterial()
        {
            if (jellySoftWallMaterial == null)
            {
                jellySoftWallMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_SoftWallMagenta", new Color(1f, 0.33f, 0.82f), true, 0.35f);
            }

            return jellySoftWallMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly glow material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyGlowMaterial()
        {
            if (jellyGlowMaterial == null)
            {
                jellyGlowMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_GlowCream", new Color(0.78f, 1f, 0.96f), true, 0.95f);
            }

            return jellyGlowMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly dark material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyDarkMaterial()
        {
            if (jellyDarkMaterial == null)
            {
                jellyDarkMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_DarkFrame", new Color(0.12f, 0.1f, 0.22f));
            }

            return jellyDarkMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly prop cyan material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyPropCyanMaterial()
        {
            if (jellyPropCyanMaterial == null)
            {
                jellyPropCyanMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_PropCyan", new Color(0.18f, 0.92f, 1f), true, 0.75f);
            }

            return jellyPropCyanMaterial;
        }

        /// <summary>
        /// Purpose: Gets jelly prop pink material.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material GetJellyPropPinkMaterial()
        {
            if (jellyPropPinkMaterial == null)
            {
                jellyPropPinkMaterial = CreateRuntimeMaterial("Mat_Runtime_JellyMaze_PropPink", new Color(1f, 0.42f, 0.9f), true, 0.55f);
            }

            return jellyPropPinkMaterial;
        }

        /// <summary>
        /// Purpose: Gets the soft snow material used by Snowfield floor and snow caps.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield floor material.</returns>
        private Material GetSnowFloorMaterial()
        {
            if (snowFloorMaterial == null)
            {
                snowFloorMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_SnowFloor", new Color(0.84f, 0.93f, 0.98f), false, 0f, 0.32f);
            }

            return snowFloorMaterial;
        }

        /// <summary>
        /// Purpose: Gets the pale ice inset material used on Snowfield floor tiles.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield tile inset material.</returns>
        private Material GetSnowTileInsetMaterial()
        {
            if (snowTileInsetMaterial == null)
            {
                snowTileInsetMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_IceTile", new Color(0.54f, 0.8f, 0.93f), false, 0f, 0.48f);
            }

            return snowTileInsetMaterial;
        }

        /// <summary>
        /// Purpose: Gets the main packed-snow hard wall material.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield hard wall material.</returns>
        private Material GetSnowHardWallMaterial()
        {
            if (snowHardWallMaterial == null)
            {
                snowHardWallMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_HardWallPackedSnow", new Color(0.56f, 0.72f, 0.84f), false, 0f, 0.24f);
            }

            return snowHardWallMaterial;
        }

        /// <summary>
        /// Purpose: Gets the bright cap material for Snowfield hard walls.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield hard wall highlight material.</returns>
        private Material GetSnowHardWallHighlightMaterial()
        {
            if (snowHardWallHighlightMaterial == null)
            {
                snowHardWallHighlightMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_HardWallHighlight", new Color(0.95f, 0.99f, 1f), false, 0f, 0.34f);
            }

            return snowHardWallHighlightMaterial;
        }

        /// <summary>
        /// Purpose: Gets the warm gift-crate body material used by Snowfield soft walls.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield soft wall material.</returns>
        private Material GetSnowSoftWallMaterial()
        {
            if (snowSoftWallMaterial == null)
            {
                snowSoftWallMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_SoftWallGift", new Color(1f, 0.6f, 0.12f), false, 0f, 0.26f);
            }

            return snowSoftWallMaterial;
        }

        /// <summary>
        /// Purpose: Gets the blue ribbon material used by Snowfield gift-style soft walls.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield soft wall ribbon material.</returns>
        private Material GetSnowSoftWallRibbonMaterial()
        {
            if (snowSoftWallRibbonMaterial == null)
            {
                snowSoftWallRibbonMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_SoftWallRibbonBlue", new Color(0.02f, 0.42f, 0.72f), false, 0f, 0.32f);
            }

            return snowSoftWallRibbonMaterial;
        }

        /// <summary>
        /// Purpose: Gets the glowing ice material shared by Snowfield props, goals, and highlights.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield ice material.</returns>
        private Material GetSnowIceMaterial()
        {
            if (snowIceMaterial == null)
            {
                snowIceMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_GlowIce", new Color(0.34f, 0.72f, 0.9f), false, 0f, 0.5f);
            }

            return snowIceMaterial;
        }

        /// <summary>
        /// Purpose: Gets the warm wood material for Snowfield fences and braces.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield wood material.</returns>
        private Material GetSnowWoodMaterial()
        {
            if (snowWoodMaterial == null)
            {
                snowWoodMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_WarmWood", new Color(0.34f, 0.24f, 0.17f), false, 0f, 0.22f);
            }

            return snowWoodMaterial;
        }

        /// <summary>
        /// Purpose: Gets the blue accent material for Snowfield environment props.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield blue prop material.</returns>
        private Material GetSnowPropBlueMaterial()
        {
            if (snowPropBlueMaterial == null)
            {
                snowPropBlueMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_PropBlue", new Color(0.22f, 0.6f, 0.82f), false, 0f, 0.34f);
            }

            return snowPropBlueMaterial;
        }

        /// <summary>
        /// Purpose: Gets the pink accent material for Snowfield props and goal flags.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield pink prop material.</returns>
        private Material GetSnowPropPinkMaterial()
        {
            if (snowPropPinkMaterial == null)
            {
                snowPropPinkMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_PropPink", new Color(0.98f, 0.28f, 0.62f), false, 0f, 0.28f);
            }

            return snowPropPinkMaterial;
        }

        /// <summary>
        /// Purpose: Gets the cool grout material used between Snowfield ice tiles.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield tile edge material.</returns>
        private Material GetSnowTileEdgeMaterial()
        {
            if (snowTileEdgeMaterial == null)
            {
                snowTileEdgeMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_IceTileEdge", new Color(0.38f, 0.58f, 0.7f), false, 0f, 0.22f);
            }

            return snowTileEdgeMaterial;
        }

        /// <summary>
        /// Purpose: Gets the blue-gray packed shadow material for snow blocks and gifts.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield packed shadow material.</returns>
        private Material GetSnowPackedShadowMaterial()
        {
            if (snowPackedShadowMaterial == null)
            {
                snowPackedShadowMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_PackedShadow", new Color(0.18f, 0.27f, 0.36f), false, 0f, 0.18f);
            }

            return snowPackedShadowMaterial;
        }

        /// <summary>
        /// Purpose: Gets the subtle blue material for Snowfield floor snowflake etching.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield snowflake mark material.</returns>
        private Material GetSnowflakeMarkMaterial()
        {
            if (snowflakeMarkMaterial == null)
            {
                snowflakeMarkMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_SnowflakeMark", new Color(0.42f, 0.68f, 0.82f), false, 0f, 0.42f);
            }

            return snowflakeMarkMaterial;
        }

        /// <summary>
        /// Purpose: Gets the warmer shaded side material for Snowfield gift soft walls.
        /// Inputs: no direct parameters; creates the material lazily on first use.
        /// Output: a cached Material instance.
        /// </summary>
        /// <returns>The Snowfield gift side material.</returns>
        private Material GetSnowGiftSideMaterial()
        {
            if (snowGiftSideMaterial == null)
            {
                snowGiftSideMaterial = CreateRuntimeMaterial("Mat_Runtime_Snowfield_GiftSideShade", new Color(0.82f, 0.36f, 0.08f), false, 0f, 0.22f);
            }

            return snowGiftSideMaterial;
        }

        /// <summary>
        /// Purpose: Creates runtime material.
        /// Inputs: `materialName`, `color`, `useEmission`, `emissionIntensity`, `smoothness`; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <param name="materialName">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        /// <param name="useEmission">Input value used by this method.</param>
        /// <param name="emissionIntensity">Input value used by this method.</param>
        /// <param name="smoothness">Input value used by this method.</param>
        /// <returns>a `Material` value.</returns>
        private Material CreateRuntimeMaterial(string materialName, Color color, bool useEmission = false, float emissionIntensity = 0f, float smoothness = 0.5f)
        {
            Material material = new Material(Shader.Find("Standard"))
            {
                name = materialName,
                color = color,
                hideFlags = HideFlags.HideAndDontSave
            };

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", Mathf.Clamp01(smoothness));
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            if (useEmission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * Mathf.Max(0.1f, emissionIntensity));
            }

            return material;
        }
    }
}
