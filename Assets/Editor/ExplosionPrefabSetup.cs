using BubbleTown.Core;
using BubbleTown.Gameplay;
using UnityEditor;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Creates the placeholder center explosion prefab and wires it into the bomb prefab.
    /// </summary>
    public static class ExplosionPrefabSetup
    {
        private const string BombPrefabPath = "Assets/Prefabs/Gameplay/Bomb.prefab";
        private const string ExplosionPrefabPath = "Assets/Prefabs/Gameplay/ExplosionCenter.prefab";
        private const string ExplosionMaterialPath = "Assets/Materials/ExplosionCenterPlaceholder.mat";

        [MenuItem("BubbleTown/Setup/Ensure Explosion Center Prefab")]
        public static void EnsureExplosionCenterPrefab()
        {
            Material explosionMaterial = EnsureExplosionMaterial();
            ExplosionController explosionPrefab = EnsurePlaceholderExplosionPrefab(explosionMaterial);
            ConfigureBombPrefab(explosionPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[ExplosionPrefabSetup] Placeholder ExplosionCenter prefab is ready.");
        }

        public static void EnsureExplosionCenterPrefabFromBatchmode()
        {
            EnsureExplosionCenterPrefab();
        }

        private static Material EnsureExplosionMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(ExplosionMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Standard"));
            material.name = "ExplosionCenterPlaceholder";
            material.color = new Color(1f, 0.52f, 0.08f, 0.85f);
            AssetDatabase.CreateAsset(material, ExplosionMaterialPath);
            return material;
        }

        private static ExplosionController EnsurePlaceholderExplosionPrefab(Material explosionMaterial)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExplosionPrefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab.GetComponent<ExplosionController>();
            }

            GameObject explosionObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            explosionObject.name = "ExplosionCenter";
            explosionObject.transform.localScale = Vector3.one;

            MeshRenderer renderer = explosionObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = explosionMaterial;
            }

            SphereCollider collider = explosionObject.GetComponent<SphereCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.radius = 0.55f;
            }

            Rigidbody rigidbody = explosionObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            ExplosionController controller = explosionObject.AddComponent<ExplosionController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("lifeSeconds").floatValue = GameConstants.DefaultExplosionDuration;
            serializedController.FindProperty("visualRoot").objectReferenceValue = explosionObject.transform;
            serializedController.FindProperty("startScale").vector3Value = new Vector3(0.35f, 0.35f, 0.35f);
            serializedController.FindProperty("peakScale").vector3Value = new Vector3(1.15f, 1.15f, 1.15f);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(explosionObject, ExplosionPrefabPath);
            Object.DestroyImmediate(explosionObject);
            return prefab != null ? prefab.GetComponent<ExplosionController>() : controller;
        }

        private static void ConfigureBombPrefab(ExplosionController explosionPrefab)
        {
            if (explosionPrefab == null)
            {
                return;
            }

            GameObject bombPrefabContents = PrefabUtility.LoadPrefabContents(BombPrefabPath);
            BombController bombController = bombPrefabContents.GetComponent<BombController>();
            if (bombController == null)
            {
                PrefabUtility.UnloadPrefabContents(bombPrefabContents);
                return;
            }

            SerializedObject serializedBomb = new SerializedObject(bombController);
            serializedBomb.FindProperty("explosionPrefab").objectReferenceValue = explosionPrefab;
            serializedBomb.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(bombPrefabContents, BombPrefabPath);
            PrefabUtility.UnloadPrefabContents(bombPrefabContents);
        }
    }
}
