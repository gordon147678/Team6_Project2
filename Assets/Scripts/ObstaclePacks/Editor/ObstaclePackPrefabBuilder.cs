using System;
using UnityEditor;
using UnityEngine;

namespace Team6.Project2.ObstaclePacks.Editor
{
    public static class ObstaclePackPrefabBuilder
    {
        private const string ContentRoot = "Assets/ObstaclePackContent";
        private const string MaterialFolder = ContentRoot + "/Materials";
        private const string PrefabFolder = ContentRoot + "/Prefabs";

        [MenuItem("Tools/Obstacle Packs/Create Missing Prototype Prefabs")]
        public static void CreateMissingPrototypePrefabs()
        {
            EnsureFolders();

            Material pastry = CreateMaterialIfMissing(
                "Pastry",
                new Color(0.88f, 0.48f, 0.18f));
            Material pastryTop = CreateMaterialIfMissing(
                "PastryTop",
                new Color(0.96f, 0.72f, 0.32f));
            Material metal = CreateMaterialIfMissing(
                "Metal",
                new Color(0.72f, 0.76f, 0.8f),
                metallic: 0.85f,
                smoothness: 0.75f);
            Material handle = CreateMaterialIfMissing(
                "Handle",
                new Color(0.28f, 0.09f, 0.04f),
                smoothness: 0.25f);
            Material pizzaHandle = CreateMaterialIfMissing(
                "PizzaHandle",
                new Color(0.75f, 0.08f, 0.06f),
                smoothness: 0.35f);
            Material cheese = CreateMaterialIfMissing(
                "Cheese",
                new Color(1f, 0.72f, 0.05f),
                smoothness: 0.2f);
            Material cheeseHole = CreateMaterialIfMissing(
                "CheeseHole",
                new Color(0.68f, 0.34f, 0.02f),
                smoothness: 0.1f);

            CreateSmallPastryPrefab(pastry, pastryTop);
            CreateForkPrefab(metal, handle);
            CreateKnifePrefab(metal, handle);
            CreatePizzaCutterPrefab(metal, pizzaHandle);
            CreateCheesePrefab(cheese, cheeseHole);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("缺失的障碍物原型预制体已创建。现有资源未被覆盖。");
        }

        private static void CreateSmallPastryPrefab(
            Material pastry,
            Material pastryTop)
        {
            const string path = PrefabFolder + "/SmallPastry.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            GameObject root = CreateRoot("SmallPastry", obstacleTag: true);

            CreatePrimitive(
                PrimitiveType.Cylinder,
                root.transform,
                "PastryBody",
                new Vector3(0f, 0.45f, 0f),
                new Vector3(0.72f, 0.42f, 0.72f),
                Quaternion.identity,
                pastry);
            CreatePrimitive(
                PrimitiveType.Sphere,
                root.transform,
                "PastryTop",
                new Vector3(0f, 0.88f, 0f),
                new Vector3(0.48f, 0.22f, 0.48f),
                Quaternion.identity,
                pastryTop);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(1.5f, 1f, 1.2f);

            SaveAndDestroy(root, path);
        }

        private static void CreateForkPrefab(Material metal, Material handle)
        {
            const string path = PrefabFolder + "/Fork.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            GameObject root = CreateRoot("Fork", obstacleTag: true);
            GameObject motionRoot = CreateMotionRoot(root, "ForkMotion");

            CreatePrimitive(
                PrimitiveType.Cube,
                motionRoot.transform,
                "Handle",
                new Vector3(0f, 1.15f, 0f),
                new Vector3(0.3f, 2.3f, 0.28f),
                Quaternion.identity,
                handle);
            CreatePrimitive(
                PrimitiveType.Cube,
                motionRoot.transform,
                "Head",
                new Vector3(0f, 2.35f, 0f),
                new Vector3(1.15f, 0.22f, 0.28f),
                Quaternion.identity,
                metal);

            for (int tine = 0; tine < 4; tine++)
            {
                float x = -0.45f + tine * 0.3f;
                CreatePrimitive(
                    PrimitiveType.Cube,
                    motionRoot.transform,
                    $"Tine{tine + 1}",
                    new Vector3(x, 2.78f, 0f),
                    new Vector3(0.12f, 0.75f, 0.16f),
                    Quaternion.identity,
                    metal);
            }

            AddTriggerCollider(
                motionRoot,
                new Vector3(0f, 1.45f, 0f),
                new Vector3(1.4f, 3f, 0.7f));
            AddVerticalMotion(root, motionRoot.transform, 1.8f, 3f, 1f);

            SaveAndDestroy(root, path);
        }

        private static void CreateKnifePrefab(Material metal, Material handle)
        {
            const string path = PrefabFolder + "/Knife.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            GameObject root = CreateRoot("Knife", obstacleTag: true);
            GameObject motionRoot = CreateMotionRoot(root, "KnifeMotion");

            CreatePrimitive(
                PrimitiveType.Cube,
                motionRoot.transform,
                "Blade",
                new Vector3(-0.45f, 1.05f, 0f),
                new Vector3(2.8f, 1.35f, 0.18f),
                Quaternion.Euler(0f, 0f, -8f),
                metal);
            CreatePrimitive(
                PrimitiveType.Cube,
                motionRoot.transform,
                "Handle",
                new Vector3(1.35f, 1.25f, 0f),
                new Vector3(1.1f, 0.48f, 0.42f),
                Quaternion.identity,
                handle);

            AddTriggerCollider(
                motionRoot,
                new Vector3(0f, 1.05f, 0f),
                new Vector3(3.8f, 1.8f, 0.75f));
            AddVerticalMotion(root, motionRoot.transform, 2.2f, 3.6f, 1f);

            SaveAndDestroy(root, path);
        }

        private static void CreatePizzaCutterPrefab(
            Material metal,
            Material handle)
        {
            const string path = PrefabFolder + "/PizzaCutter.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            GameObject root = CreateRoot("PizzaCutter", obstacleTag: true);
            GameObject motionRoot = CreateMotionRoot(root, "PizzaCutterMotion");

            CreatePrimitive(
                PrimitiveType.Cylinder,
                motionRoot.transform,
                "Wheel",
                new Vector3(0f, 0.65f, 0f),
                new Vector3(0.65f, 0.1f, 0.65f),
                Quaternion.Euler(90f, 0f, 0f),
                metal);
            CreatePrimitive(
                PrimitiveType.Cube,
                motionRoot.transform,
                "Handle",
                new Vector3(0.55f, 1.55f, 0f),
                new Vector3(0.35f, 1.5f, 0.35f),
                Quaternion.Euler(0f, 0f, -28f),
                handle);

            AddTriggerCollider(
                motionRoot,
                new Vector3(0.2f, 1f, 0f),
                new Vector3(1.6f, 2.25f, 0.75f));
            AddRollingMotion(root, motionRoot.transform);

            SaveAndDestroy(root, path);
        }

        private static void CreateCheesePrefab(
            Material cheese,
            Material cheeseHole)
        {
            const string path = PrefabFolder + "/Cheese.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            GameObject root = CreateRoot("Cheese", obstacleTag: false);

            CreatePrimitive(
                PrimitiveType.Cube,
                root.transform,
                "CheeseBody",
                new Vector3(0f, 0.55f, 0f),
                new Vector3(1.25f, 1.05f, 0.9f),
                Quaternion.Euler(0f, 12f, 0f),
                cheese);

            Vector3[] holePositions =
            {
                new Vector3(-0.32f, 0.72f, -0.47f),
                new Vector3(0.32f, 0.42f, -0.47f),
                new Vector3(0.12f, 0.88f, -0.47f)
            };

            for (int index = 0; index < holePositions.Length; index++)
            {
                CreatePrimitive(
                    PrimitiveType.Sphere,
                    root.transform,
                    $"Hole{index + 1}",
                    holePositions[index],
                    Vector3.one * 0.16f,
                    Quaternion.identity,
                    cheeseHole);
            }

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 0.55f, 0f);
            collider.size = new Vector3(1.4f, 1.15f, 1f);

            CheesePickup pickup = root.AddComponent<CheesePickup>();
            AudioClip pickupSound = AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/audio/吃到奶酪_[cut_1sec].mp3");

            SerializedObject pickupObject = new SerializedObject(pickup);
            pickupObject.FindProperty("pickupSound").objectReferenceValue =
                pickupSound;
            pickupObject.ApplyModifiedPropertiesWithoutUndo();

            SaveAndDestroy(root, path);
        }

        private static void AddVerticalMotion(
            GameObject root,
            Transform motionRoot,
            float travelDistance,
            float cycleDuration,
            float topHoldDuration)
        {
            ObstaclePackVerticalOscillator oscillator =
                root.AddComponent<ObstaclePackVerticalOscillator>();
            ConfigureObjectReference(oscillator, "movingPart", motionRoot);
            ConfigureFloat(oscillator, "travelDistance", travelDistance);
            ConfigureFloat(oscillator, "cycleDuration", cycleDuration);
            ConfigureFloat(oscillator, "topHoldDuration", topHoldDuration);
            AddMotionGate(root, oscillator);
        }

        private static void AddRollingMotion(
            GameObject root,
            Transform motionRoot)
        {
            ObstaclePackRollingOscillator oscillator =
                root.AddComponent<ObstaclePackRollingOscillator>();
            ConfigureObjectReference(oscillator, "movingPart", motionRoot);
            ConfigureVector3(oscillator, "localTravelAxis", Vector3.right);
            ConfigureFloat(oscillator, "travelDistance", 8f);
            ConfigureFloat(oscillator, "cycleDuration", 2.2f);
            AddMotionGate(root, oscillator);
        }

        private static void AddMotionGate(
            GameObject root,
            Behaviour controlledBehaviour)
        {
            ObstaclePackMotionGate gate =
                root.AddComponent<ObstaclePackMotionGate>();
            SerializedObject gateObject = new SerializedObject(gate);
            SerializedProperty behaviours =
                gateObject.FindProperty("controlledBehaviours");
            behaviours.arraySize = 1;
            behaviours.GetArrayElementAtIndex(0).objectReferenceValue =
                controlledBehaviour;
            gateObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateRoot(
            string name,
            bool obstacleTag)
        {
            GameObject root = new GameObject(name);

            if (obstacleTag)
            {
                root.tag = "Obstacle";
            }

            return root;
        }

        private static GameObject CreateMotionRoot(
            GameObject root,
            string name)
        {
            GameObject motionRoot = new GameObject(name);
            motionRoot.transform.SetParent(root.transform, false);
            motionRoot.tag = "Obstacle";
            return motionRoot;
        }

        private static void AddTriggerCollider(
            GameObject target,
            Vector3 center,
            Vector3 size)
        {
            BoxCollider collider = target.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = center;
            collider.size = size;
        }

        private static GameObject CreatePrimitive(
            PrimitiveType primitiveType,
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = localRotation;
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

            return primitive;
        }

        private static Material CreateMaterialIfMissing(
            string name,
            Color color,
            float metallic = 0f,
            float smoothness = 0.3f)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (existing != null)
                return existing;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                throw new InvalidOperationException("找不到可用的 Lit Shader。");
            }

            Material material = new Material(shader)
            {
                name = name,
                color = color
            };

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConfigureObjectReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureFloat(
            UnityEngine.Object target,
            string propertyName,
            float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureVector3(
            UnityEngine.Object target,
            string propertyName,
            Vector3 value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).vector3Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SaveAndDestroy(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "ObstaclePackContent");
            EnsureFolder(ContentRoot, "Materials");
            EnsureFolder(ContentRoot, "Prefabs");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
