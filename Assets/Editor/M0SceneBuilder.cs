using System.Collections.Generic;
using KindNeighbors.Delivery;
using KindNeighbors.Interaction;
using KindNeighbors.Player;
using KindNeighbors.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KindNeighbors.EditorTools
{
    /// <summary>
    /// M0 프로토타입용 회색 박스 씬을 생성한다. 다시 실행하면 씬을 처음부터 새로 만든다.
    /// 맵 에셋이 정해지면 각 Greybox 오브젝트를 실제 모델로 교체하면 된다.
    /// </summary>
    public static class M0SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/M0_Prototype.unity";
        const string MaterialFolder = "Assets/Materials/Greybox";
        const string OrderFolder = "Assets/Data/Orders";
        const int IgnoreRaycastLayer = 2;

        static readonly Dictionary<string, Material> materials = new();

        [MenuItem("Kind Neighbors/Build M0 Prototype Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder("Assets/Scenes");
            EnsureFolder(MaterialFolder);
            EnsureFolder(OrderFolder);
            materials.Clear();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            Transform map = new GameObject("Map").transform;
            CreateGround(map);
            CreateBuildings(map, out Transform bakeryNpcSpot, out Transform mailboxSpot);
            CreateForestEdge(map);

            var systems = new GameObject("Systems");
            var deliveryManager = systems.AddComponent<DeliveryManager>();

            DeliveryOrder breadOrder = CreateOrAssignOrder("Order_D1_Bread", "빵 바구니", "빵집 주인", "이웃 할머니", "grandma_house");
            CreateBakeryOwner(bakeryNpcSpot, deliveryManager, breadOrder);
            CreateMailbox(mailboxSpot, deliveryManager, "grandma_house");

            PlayerController player = CreatePlayer(new Vector3(0f, 0f, -8f), out PlayerInteractor interactor);

            var hud = systems.AddComponent<PrototypeHUD>();
            SetRef(hud, "player", player);
            SetRef(hud, "interactor", interactor);
            SetRef(hud, "deliveryManager", deliveryManager);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[M0SceneBuilder] {ScenePath} 생성 완료");
        }

        // ---------- 환경 ----------

        static void CreateLighting()
        {
            var sun = new GameObject("Sun (Day)").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.93f, 0.8f);
            sun.intensity = 0.9f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.5f, 0.47f);
            RenderSettings.ambientGroundColor = new Color(0.3f, 0.28f, 0.25f);
        }

        static void CreateGround(Transform map)
        {
            Box("Ground", map, new Vector3(0f, -0.5f, 5f), new Vector3(80f, 1f, 80f), "Grass", new Color(0.55f, 0.72f, 0.45f));

            // 광장 + 길
            var plaza = Prim(PrimitiveType.Cylinder, "Plaza", map, new Vector3(0f, 0.01f, 0f), new Vector3(12f, 0.02f, 12f), "Path", new Color(0.72f, 0.64f, 0.5f));
            Object.DestroyImmediate(plaza.GetComponent<Collider>());
            Box("Road_North", map, new Vector3(0f, 0.01f, 16f), new Vector3(3f, 0.02f, 24f), "Path", default);
            Box("Road_West", map, new Vector3(-12f, 0.01f, 0f), new Vector3(14f, 0.02f, 3f), "Path", default);
            Box("Road_East", map, new Vector3(12f, 0.01f, 0f), new Vector3(14f, 0.02f, 3f), "Path", default);
            Box("Road_South", map, new Vector3(0f, 0.01f, -10f), new Vector3(3f, 0.02f, 10f), "Path", default);
        }

        static void CreateBuildings(Transform map, out Transform bakeryNpcSpot, out Transform mailboxSpot)
        {
            Transform buildings = new GameObject("Buildings").transform;
            buildings.SetParent(map);

            // 빵집 (서쪽, 광장을 향함)
            Transform bakery = House("Bakery", buildings, new Vector3(-16f, 0f, 0f), 90f, new Vector3(7f, 4f, 6f),
                "Bakery", new Color(0.95f, 0.78f, 0.6f));
            bakeryNpcSpot = Spot("NpcSpot", bakery, new Vector3(0f, 0f, 4.2f));

            // 이웃 할머니 집 (동쪽)
            Transform grandma = House("House_Grandma", buildings, new Vector3(16f, 0f, 0f), -90f, new Vector3(6f, 3.5f, 5f),
                "HouseGrandma", new Color(0.8f, 0.7f, 0.9f));
            mailboxSpot = Spot("MailboxSpot", grandma, new Vector3(1.8f, 0f, 3.8f));

            // 솜의 집 (불 꺼진 집, 북동쪽)
            House("House_Som (Dark)", buildings, new Vector3(9f, 0f, 14f), -90f, new Vector3(5f, 3.5f, 5f),
                "HouseSom", new Color(0.35f, 0.33f, 0.35f));

            // 그 외 주민 집
            House("House_A", buildings, new Vector3(-8f, 0f, 14f), 90f, new Vector3(5f, 3.5f, 5f), "HouseA", new Color(0.7f, 0.85f, 0.95f));
            House("House_B", buildings, new Vector3(-8f, 0f, -12f), 90f, new Vector3(5f, 3.5f, 5f), "HouseB", new Color(0.95f, 0.85f, 0.7f));
            House("House_C", buildings, new Vector3(9f, 0f, -12f), -90f, new Vector3(5f, 3.5f, 5f), "HouseC", new Color(0.75f, 0.92f, 0.78f));

            // 플레이어 집 (남쪽 끝)
            House("House_Player", buildings, new Vector3(0f, 0f, -18f), 0f, new Vector3(5f, 3.5f, 5f), "HousePlayer", new Color(0.95f, 0.95f, 0.88f));
        }

        static void CreateForestEdge(Transform map)
        {
            Transform forest = new GameObject("ForestEdge").transform;
            forest.SetParent(map);
            var rng = new System.Random(7);
            for (int i = 0; i < 26; i++)
            {
                float x = -36f + i * 2.9f + (float)rng.NextDouble();
                float z = 32f + (float)rng.NextDouble() * 6f;
                float height = 4f + (float)rng.NextDouble() * 3f;
                Transform tree = new GameObject($"Tree_{i:00}").transform;
                tree.SetParent(forest);
                tree.position = new Vector3(x, 0f, z);
                Prim(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(x, height * 0.25f, z), new Vector3(0.5f, height * 0.25f, 0.5f), "Trunk", new Color(0.45f, 0.33f, 0.24f));
                Prim(PrimitiveType.Sphere, "Leaves", tree, new Vector3(x, height * 0.65f, z), Vector3.one * (2.2f + (float)rng.NextDouble()), "Leaves", new Color(0.25f, 0.45f, 0.3f));
            }

            // 숲 경계 배달 지점 (DAY 3 특별 배달용 자리 표시)
            Spot("ForestDropSpot", forest, new Vector3(0f, 0f, 30f));
        }

        // ---------- 상호작용 대상 ----------

        static void CreateBakeryOwner(Transform spot, DeliveryManager manager, DeliveryOrder order)
        {
            var npc = Prim(PrimitiveType.Capsule, "NPC_BakeryOwner", null, spot.position + Vector3.up * 0.75f, new Vector3(0.8f, 0.75f, 0.8f),
                "NpcBakery", new Color(0.9f, 0.55f, 0.4f));
            npc.transform.rotation = spot.rotation;
            Prim(PrimitiveType.Cube, "Face", npc.transform, npc.transform.position + npc.transform.forward * 0.4f + Vector3.up * 0.3f,
                new Vector3(0.3f, 0.15f, 0.1f), "Face", new Color(0.2f, 0.15f, 0.15f)).transform.rotation = spot.rotation;
            Object.DestroyImmediate(npc.transform.Find("Face").GetComponent<Collider>());

            var client = npc.AddComponent<DeliveryClient>();
            SetRef(client, "manager", manager);
            SetArray(client, "orders", order);
        }

        static void CreateMailbox(Transform spot, DeliveryManager manager, string destinationId)
        {
            Transform mailbox = new GameObject("Mailbox_Grandma").transform;
            mailbox.SetPositionAndRotation(spot.position, spot.rotation);

            Prim(PrimitiveType.Cylinder, "Post", mailbox, spot.position + Vector3.up * 0.5f, new Vector3(0.12f, 0.5f, 0.12f), "Trunk", default);
            Prim(PrimitiveType.Cube, "Box", mailbox, spot.position + Vector3.up * 1.1f, new Vector3(0.5f, 0.4f, 0.7f), "Mailbox", new Color(0.85f, 0.3f, 0.3f))
                .transform.rotation = spot.rotation;

            var marker = Prim(PrimitiveType.Cube, "DestinationMarker", mailbox, spot.position + Vector3.up * 2.4f, Vector3.one * 0.35f, "Marker", new Color(0.3f, 0.85f, 0.95f));
            marker.transform.rotation = Quaternion.Euler(45f, 0f, 45f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            var destination = mailbox.gameObject.AddComponent<DeliveryDestination>();
            SetRef(destination, "manager", manager);
            SetString(destination, "destinationId", destinationId);
            SetRef(destination, "marker", marker);
        }

        // ---------- 플레이어 ----------

        static PlayerController CreatePlayer(Vector3 position, out PlayerInteractor interactor)
        {
            var root = new GameObject("Player");
            root.transform.position = position;
            root.layer = IgnoreRaycastLayer;

            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.4f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.7f, 0f);

            // 임시 바디: 캐릭터 모델이 완성되면 Body 아래를 교체
            Transform body = new GameObject("Body").transform;
            body.SetParent(root.transform, false);
            var capsule = Prim(PrimitiveType.Capsule, "Placeholder", body, position + Vector3.up * 0.7f, new Vector3(0.7f, 0.7f, 0.7f),
                "Player", new Color(0.98f, 0.98f, 0.95f));
            Object.DestroyImmediate(capsule.GetComponent<Collider>());
            var nose = Prim(PrimitiveType.Cube, "FacingNose", body, position + new Vector3(0f, 1.05f, 0.35f), new Vector3(0.15f, 0.1f, 0.15f),
                "Face", default);
            Object.DestroyImmediate(nose.GetComponent<Collider>());
            SetLayerRecursive(body.gameObject, IgnoreRaycastLayer);

            Transform pivot = new GameObject("CameraPivot").transform;
            pivot.SetParent(root.transform, false);
            pivot.localPosition = new Vector3(0f, 1.25f, 0f);

            var camGo = new GameObject("PlayerCamera") { tag = "MainCamera" };
            camGo.transform.SetParent(pivot, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 65f;
            camGo.AddComponent<AudioListener>();

            var controller = root.AddComponent<PlayerController>();
            SetRef(controller, "body", body);
            SetRef(controller, "cameraPivot", pivot);
            SetRef(controller, "playerCamera", cam);

            interactor = root.AddComponent<PlayerInteractor>();
            SetRef(interactor, "player", controller);

            return controller;
        }

        // ---------- 헬퍼 ----------

        static Transform House(string name, Transform parent, Vector3 position, float yaw, Vector3 size, string matKey, Color color)
        {
            Transform house = new GameObject(name).transform;
            house.SetParent(parent);
            house.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            BoxLocal("Walls", house, new Vector3(0f, size.y / 2f, 0f), size, matKey, color);
            BoxLocal("Roof", house, new Vector3(0f, size.y + 0.4f, 0f), new Vector3(size.x + 0.6f, 0.8f, size.z + 0.6f), "Roof", new Color(0.55f, 0.35f, 0.3f));
            BoxLocal("Door", house, new Vector3(0f, 1f, size.z / 2f + 0.05f), new Vector3(1.1f, 2f, 0.1f), "Door", new Color(0.5f, 0.35f, 0.22f));
            BoxLocal("Window_L", house, new Vector3(-size.x / 3.2f, size.y * 0.6f, size.z / 2f + 0.05f), new Vector3(0.9f, 0.9f, 0.1f), "Window", new Color(0.95f, 0.9f, 0.6f));
            BoxLocal("Window_R", house, new Vector3(size.x / 3.2f, size.y * 0.6f, size.z / 2f + 0.05f), new Vector3(0.9f, 0.9f, 0.1f), "Window", default);
            return house;
        }

        static Transform Spot(string name, Transform parent, Vector3 localPosition)
        {
            Transform spot = new GameObject(name).transform;
            spot.SetParent(parent, false);
            spot.localPosition = localPosition;
            return spot;
        }

        static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, string matKey, Color color) =>
            Prim(PrimitiveType.Cube, name, parent, position, scale, matKey, color);

        static GameObject BoxLocal(string name, Transform parent, Vector3 localPosition, Vector3 scale, string matKey, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = GetMaterial(matKey, color);
            go.isStatic = true;
            return go;
        }

        static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, string matKey, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = GetMaterial(matKey, color);
            return go;
        }

        /// <summary>같은 키는 같은 머티리얼을 재사용한다. color가 default면 이미 만든 머티리얼을 그대로 쓴다.</summary>
        static Material GetMaterial(string key, Color color)
        {
            if (materials.TryGetValue(key, out var cached))
                return cached;

            string path = $"{MaterialFolder}/M_{key}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }
            if (color != default)
            {
                mat.color = color;
                mat.SetFloat("_Glossiness", 0.1f);
                EditorUtility.SetDirty(mat);
            }
            materials[key] = mat;
            return mat;
        }

        static DeliveryOrder CreateOrAssignOrder(string assetName, string item, string client, string recipient, string destinationId)
        {
            string path = $"{OrderFolder}/{assetName}.asset";
            var order = AssetDatabase.LoadAssetAtPath<DeliveryOrder>(path);
            if (order == null)
            {
                order = ScriptableObject.CreateInstance<DeliveryOrder>();
                AssetDatabase.CreateAsset(order, path);
            }
            order.itemName = item;
            order.clientName = client;
            order.recipientName = recipient;
            order.destinationId = destinationId;
            EditorUtility.SetDirty(order);
            return order;
        }

        static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArray(Object target, string field, params Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        static void AddSceneToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path)) return;
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
