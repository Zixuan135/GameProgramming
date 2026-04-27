using BubbleTown.Core;
using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Map
{
    /// <summary>
    /// Generates the visual layer for the current grid map.
    /// MapManager remains the source of gameplay truth; this class only builds theme objects.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        private const string GeneratedRootName = "GeneratedMap_CandyPark";
        private const string GroundRootName = "GroundRoot";
        private const string HardWallRootName = "HardWallRoot";
        private const string SoftWallRootName = "SoftWallRoot";
        private const string DecorationRootName = "DecorationRoot";

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
        [SerializeField, Min(0f)] private float decorationOuterPadding = 1.35f;

        private Material grassMaterial;
        private Material candyBlueMaterial;
        private Material creamMaterial;
        private Material jellyBlueMaterial;
        private Material shadowMaterial;
        private Material propPinkMaterial;
        private Material propMintMaterial;
        private Material propYellowMaterial;

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
                Debug.LogWarning("[MapGenerator] Cannot generate Candy Park visuals because MapManager is missing.");
                return;
            }

            mapWidth = Mathf.Max(1, mapManager.MapWidth);
            mapHeight = Mathf.Max(1, mapManager.MapHeight);
            cellSize = Mathf.Max(0.1f, mapManager.CellSize);

            Transform root = ResolveGeneratedRoot();
            ClearGeneratedChildren(root);
            if (hideSceneAuthoredVisualRoots)
            {
                SetLegacySceneVisualRootsActive(root, false);
            }

            Transform groundRoot = CreateChildRoot(root, GroundRootName);
            Transform hardWallRoot = CreateChildRoot(root, HardWallRootName);
            Transform softWallRoot = CreateChildRoot(root, SoftWallRootName);
            Transform decorationRoot = CreateChildRoot(root, DecorationRootName);

            GenerateGridVisuals(mapManager, groundRoot, hardWallRoot, softWallRoot);
            if (generateDecorations)
            {
                GenerateCandyParkDecorations(mapType, decorationRoot);
            }

            Debug.Log($"[MapGenerator] Generated Candy Park visuals. Type: {mapType}, Size: {mapWidth}x{mapHeight}");
        }

        public void Clear()
        {
            Transform root = ResolveGeneratedRoot(false);
            if (root == null)
            {
                return;
            }

            ClearGeneratedChildren(root);
        }

        private void GenerateGridVisuals(
            MapManager mapManager,
            Transform groundRoot,
            Transform hardWallRoot,
            Transform softWallRoot)
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
                        SpawnMapPiece(groundTilePrefab, groundRoot, mapManager.GridToWorld(gridPosition), $"Tile_{x:00}_{y:00}", CreateFallbackGroundTile);
                    }

                    if (!generateWalls)
                    {
                        continue;
                    }

                    if (cell.IsHardWall)
                    {
                        SpawnMapPiece(hardWallPrefab, hardWallRoot, mapManager.GridToWorld(gridPosition), $"Wall_Hard_{x:00}_{y:00}", CreateFallbackHardWall);
                    }
                    else if (cell.IsSoftWall)
                    {
                        GameObject softWall = SpawnMapPiece(softWallPrefab, softWallRoot, mapManager.GridToWorld(gridPosition), $"Wall_Soft_{x:00}_{y:00}", CreateFallbackSoftWall);
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

            CreateLollipopTree(decorationRoot, new Vector3(minX - 0.85f, 0f, minZ - 0.25f), "LollipopTree_SouthWest", GetThemeAccentColor(mapType, 0));
            CreateLollipopTree(decorationRoot, new Vector3(maxX + 0.75f, 0f, minZ - 0.15f), "LollipopTree_SouthEast", GetThemeAccentColor(mapType, 1));
            CreateLollipopTree(decorationRoot, new Vector3(minX - 0.75f, 0f, maxZ + 0.15f), "LollipopTree_NorthWest", GetThemeAccentColor(mapType, 2));
            CreateBalloonCluster(decorationRoot, new Vector3(maxX + 0.95f, 0f, maxZ + 0.35f), "BalloonCluster_NorthEast");
            CreateRoundBush(decorationRoot, new Vector3(centerX, 0f, maxZ + 0.45f), "RoundBush_NorthCenter");
            CreateSignBoard(decorationRoot, new Vector3(centerX, 0f, minZ - 0.55f), "Sign_CandyPark");
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

        private GameObject CreateFallbackGroundTile(string objectName)
        {
            GameObject root = new GameObject(objectName);
            CreatePrimitiveChild(root.transform, "TileBase", PrimitiveType.Cube, new Vector3(0f, -0.045f, 0f), new Vector3(0.98f, 0.08f, 0.98f), GetGrassMaterial());
            CreatePrimitiveChild(root.transform, "TileInset", PrimitiveType.Cube, new Vector3(0f, 0.005f, 0f), new Vector3(0.64f, 0.025f, 0.64f), GetCandyBlueMaterial());
            return root;
        }

        private GameObject CreateFallbackHardWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.92f, 1f, 0.92f);
            collider.center = new Vector3(0f, 0.5f, 0f);
            CreatePrimitiveChild(root.transform, "BottomShadow", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(0.96f, 0.12f, 0.96f), GetShadowMaterial());
            CreatePrimitiveChild(root.transform, "BaseBlock", PrimitiveType.Cube, new Vector3(0f, 0.46f, 0f), new Vector3(0.9f, 0.86f, 0.9f), GetCreamMaterial());
            CreatePrimitiveChild(root.transform, "TopHighlight", PrimitiveType.Cube, new Vector3(0f, 0.92f, 0f), new Vector3(0.68f, 0.08f, 0.68f), GetCandyBlueMaterial());
            return root;
        }

        private GameObject CreateFallbackSoftWall(string objectName)
        {
            GameObject root = new GameObject(objectName);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.82f, 0.78f, 0.82f);
            collider.center = new Vector3(0f, 0.39f, 0f);
            CreatePrimitiveChild(root.transform, "JellyBody", PrimitiveType.Cube, new Vector3(0f, 0.39f, 0f), new Vector3(0.82f, 0.78f, 0.82f), GetJellyBlueMaterial());
            CreatePrimitiveChild(root.transform, "JellyShine", PrimitiveType.Sphere, new Vector3(-0.22f, 0.68f, -0.22f), new Vector3(0.18f, 0.08f, 0.18f), GetCreamMaterial());
            return root;
        }

        private Transform ResolveGeneratedRoot(bool createIfMissing = true)
        {
            if (generatedMapRoot != null)
            {
                return generatedMapRoot;
            }

            Transform existingRoot = transform.Find(GeneratedRootName);
            if (existingRoot != null)
            {
                generatedMapRoot = existingRoot;
                return generatedMapRoot;
            }

            if (!createIfMissing)
            {
                return null;
            }

            GameObject rootObject = new GameObject(GeneratedRootName);
            generatedMapRoot = rootObject.transform;
            generatedMapRoot.SetParent(transform);
            generatedMapRoot.localPosition = Vector3.zero;
            generatedMapRoot.localRotation = Quaternion.identity;
            generatedMapRoot.localScale = Vector3.one;
            return generatedMapRoot;
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
                if (child == generatedRoot)
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

        private Material CreateRuntimeMaterial(string materialName, Color color)
        {
            Material material = new Material(Shader.Find("Standard"))
            {
                name = materialName,
                color = color,
                hideFlags = HideFlags.HideAndDontSave
            };
            return material;
        }
    }
}
