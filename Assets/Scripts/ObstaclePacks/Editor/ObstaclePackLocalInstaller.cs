using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Team6.Project2.ObstaclePacks.Editor
{
    public static class ObstaclePackLocalInstaller
    {
        private const string SampleScenePath =
            "Assets/Scenes/SampleScene.unity";
        private const string PrefabFolder =
            "Assets/ObstaclePackContent/Prefabs";

        [MenuItem("Tools/Obstacle Packs/Install Into SampleScene")]
        public static void InstallIntoSampleScene()
        {
            GameObject bigPastry = CreateBigPastryIfMissing();

            Scene scene = SceneManager.GetActiveScene();

            if (scene.path != SampleScenePath)
            {
                throw new InvalidOperationException(
                    "请先打开 SampleScene，再执行障碍物包实装。");
            }

            if (scene.isDirty)
            {
                throw new InvalidOperationException(
                    "SampleScene 存在未保存修改。请先保存或撤销后再实装。");
            }

            GameObject manager = GameObject.Find("ObstacleManager");

            if (manager == null)
            {
                manager = new GameObject("ObstacleManager");
            }

            DisableOrRemoveOldSpawner(manager);

            ObstaclePackSpawner spawner =
                manager.GetComponent<ObstaclePackSpawner>();

            if (spawner == null)
            {
                spawner = manager.AddComponent<ObstaclePackSpawner>();
            }

            ConfigureSpawner(
                spawner,
                new[]
                {
                    Binding(PackObstacleType.BigPastry, bigPastry),
                    Binding(
                        PackObstacleType.SmallPastry,
                        LoadPrefab("SmallPastry")),
                    Binding(PackObstacleType.Fork, LoadPrefab("Fork")),
                    Binding(PackObstacleType.Knife, LoadPrefab("Knife")),
                    Binding(
                        PackObstacleType.PizzaCutter,
                        LoadPrefab("PizzaCutter")),
                    Binding(PackObstacleType.Cheese, LoadPrefab("Cheese"))
                });

            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(spawner);
            EditorSceneManager.MarkSceneDirty(scene);

            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("SampleScene 保存失败。");
            }

            Debug.Log(
                "障碍物包已实装到 SampleScene：旧生成器已停用，" +
                "六类预制体已绑定。",
                manager);
        }

        private static void DisableOrRemoveOldSpawner(GameObject manager)
        {
            MonoBehaviour[] behaviours = manager.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour != null &&
                    behaviour.GetType().Name == "ObstacleSpawner")
                {
                    behaviour.enabled = false;
                    EditorUtility.SetDirty(behaviour);
                }
            }

            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(manager) > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(manager);
            }
        }

        private static void ConfigureSpawner(
            ObstaclePackSpawner spawner,
            PrefabBindingData[] bindings)
        {
            SerializedObject serializedSpawner = new SerializedObject(spawner);
            SerializedProperty prefabBindings =
                serializedSpawner.FindProperty("prefabBindings");

            prefabBindings.arraySize = bindings.Length;

            for (int index = 0; index < bindings.Length; index++)
            {
                SerializedProperty element =
                    prefabBindings.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("obstacleType").enumValueIndex =
                    (int)bindings[index].ObstacleType;
                element.FindPropertyRelative("prefab").objectReferenceValue =
                    bindings[index].Prefab;
                element.FindPropertyRelative("forwardMovementMode").enumValueIndex =
                    (int)ObstaclePackSpawner.ForwardMovementMode.AddPackMover;
            }

            SetInteger(serializedSpawner, "laneCount", 5);
            SetFloat(serializedSpawner, "laneWidth", 2f);
            SetFloat(serializedSpawner, "spawnY", 0f);
            SetFloat(serializedSpawner, "spawnZ", 50f);
            SetFloat(serializedSpawner, "rowInterval", 1.25f);
            SetFloat(serializedSpawner, "obstacleSpeed", 10f);
            SetFloat(serializedSpawner, "destroyZ", -10f);
            SetFloat(
                serializedSpawner,
                "randomPackCheeseProbability",
                0.5f);
            SetBoolean(
                serializedSpawner,
                "preventImmediatePackRepeat",
                true);
            SetBoolean(serializedSpawner, "useFixedRandomSeed", false);
            SetInteger(serializedSpawner, "randomSeed", 12345);
            SetBoolean(serializedSpawner, "playOnStart", true);

            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PrefabBindingData Binding(
            PackObstacleType obstacleType,
            GameObject prefab)
        {
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"{obstacleType} 的预制体不存在。");
            }

            return new PrefabBindingData(obstacleType, prefab);
        }

        private static GameObject LoadPrefab(string name)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PrefabFolder}/{name}.prefab");
        }

        private static GameObject CreateBigPastryIfMissing()
        {
            const string path = PrefabFolder + "/BigPastry.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (existing != null)
                return existing;

            Material pastry = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/ObstaclePackContent/Materials/Pastry.mat");
            Material pastryTop = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/ObstaclePackContent/Materials/PastryTop.mat");

            if (pastry == null || pastryTop == null)
            {
                ObstaclePackPrefabBuilder.CreateMissingPrototypePrefabs();
                pastry = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/ObstaclePackContent/Materials/Pastry.mat");
                pastryTop = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/ObstaclePackContent/Materials/PastryTop.mat");
            }

            GameObject root = new GameObject("BigPastry");
            root.tag = "Obstacle";

            CreatePrimitive(
                PrimitiveType.Cylinder,
                root.transform,
                "PastryBody",
                new Vector3(0f, 0.45f, 0f),
                new Vector3(3.6f, 0.42f, 1.2f),
                pastry);
            CreatePrimitive(
                PrimitiveType.Sphere,
                root.transform,
                "PastryTop",
                new Vector3(0f, 0.9f, 0f),
                new Vector3(3.1f, 0.25f, 1f),
                pastryTop);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(3.8f, 1.1f, 1.35f);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        private static void CreatePrimitive(
            PrimitiveType primitiveType,
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;

            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void SetFloat(
            SerializedObject target,
            string propertyName,
            float value)
        {
            target.FindProperty(propertyName).floatValue = value;
        }

        private static void SetInteger(
            SerializedObject target,
            string propertyName,
            int value)
        {
            target.FindProperty(propertyName).intValue = value;
        }

        private static void SetBoolean(
            SerializedObject target,
            string propertyName,
            bool value)
        {
            target.FindProperty(propertyName).boolValue = value;
        }

        private readonly struct PrefabBindingData
        {
            public PrefabBindingData(
                PackObstacleType obstacleType,
                GameObject prefab)
            {
                ObstacleType = obstacleType;
                Prefab = prefab;
            }

            public PackObstacleType ObstacleType { get; }

            public GameObject Prefab { get; }
        }
    }
}
