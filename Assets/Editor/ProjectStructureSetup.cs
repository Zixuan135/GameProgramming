using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Keeps Phase 2 assets organized without changing runtime systems.
    /// This is intentionally light-touch: it creates expected folders and groups legacy scene visuals.
    /// </summary>
    public static class ProjectStructureSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";
        private const string BattleRootName = "BattleRoot";
        private const string LegacyVisualsRootName = "LegacyVisualsRoot_Disabled";

        [MenuItem("BubbleTown/Setup/Organize Project Structure")]
        public static void OrganizeProjectStructure()
        {
            EnsureAssetFolders();
            OrganizeBattleSceneLegacyVisuals();
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectStructureSetup] Project folders and Battle legacy visuals are organized.");
        }

        public static void OrganizeProjectStructureFromBatchmode()
        {
            OrganizeProjectStructure();
        }

        private static void EnsureAssetFolders()
        {
            EnsureFolderPath("Assets/Materials/Characters");
            EnsureFolderPath("Assets/Materials/Gameplay/Bombs");
            EnsureFolderPath("Assets/Materials/Gameplay/Explosions");
            EnsureFolderPath("Assets/Materials/Gameplay/Items");
            EnsureFolderPath("Assets/Materials/Map/CandyPark");
            EnsureFolderPath("Assets/Materials/Map/JellyMaze");
            EnsureFolderPath("Assets/Materials/Map/Shared");
            EnsureFolderPath("Assets/Materials/UI");

            EnsureFolderPath("Assets/Prefabs/Gameplay/Bombs");
            EnsureFolderPath("Assets/Prefabs/Gameplay/Explosions");
            EnsureFolderPath("Assets/Prefabs/Gameplay/Items");
            EnsureFolderPath("Assets/Prefabs/Map/CandyPark");
            EnsureFolderPath("Assets/Prefabs/Map/JellyMaze");
            EnsureFolderPath("Assets/Prefabs/Map/Shared");
            EnsureFolderPath("Assets/Prefabs/Environment/CandyPark");
            EnsureFolderPath("Assets/Prefabs/Environment/JellyMaze");
        }

        private static void EnsureFolderPath(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void OrganizeBattleSceneLegacyVisuals()
        {
            Scene scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            GameObject battleRoot = FindInScene(scene, BattleRootName);
            GameObject legacyRoot = FindInScene(scene, LegacyVisualsRootName);
            if (legacyRoot == null)
            {
                legacyRoot = new GameObject(LegacyVisualsRootName);
                if (battleRoot != null)
                {
                    legacyRoot.transform.SetParent(battleRoot.transform, false);
                }
            }

            MoveUnderLegacyRoot(scene, legacyRoot.transform, "Ground_CandyParkBoard");
            MoveUnderLegacyRoot(scene, legacyRoot.transform, "WallVisualsRoot");
            legacyRoot.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void MoveUnderLegacyRoot(Scene scene, Transform legacyRoot, string objectName)
        {
            GameObject target = FindInScene(scene, objectName);
            if (target == null || target.transform == legacyRoot)
            {
                return;
            }

            target.transform.SetParent(legacyRoot, true);
        }

        private static GameObject FindInScene(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject match = FindInChildrenIncludingInactive(roots[i].transform, objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static GameObject FindInChildrenIncludingInactive(Transform root, string objectName)
        {
            if (root.name == objectName)
            {
                return root.gameObject;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                GameObject match = FindInChildrenIncludingInactive(root.GetChild(i), objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
