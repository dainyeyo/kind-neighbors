using System.Collections.Generic;
using KindNeighbors.Delivery;
using KindNeighbors.Dialogue;
using KindNeighbors.Flow;
using KindNeighbors.Interaction;
using KindNeighbors.Player;
using KindNeighbors.Core.Flags;
using KindNeighbors.Home;
using KindNeighbors.Save;
using KindNeighbors.Travel;
using KindNeighbors.UI;
using KindNeighbors.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KindNeighbors.EditorTools
{
    /// <summary>
    /// 프로토타입 회색 박스 씬을 생성한다. 다시 실행하면 씬을 처음부터 새로 만든다 (데이터 에셋은 유지).
    /// 맵 에셋이 정해지면 각 Greybox 오브젝트를 실제 모델로 교체하면 된다.
    /// </summary>
    public static partial class PrototypeSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Prototype.unity";
        const string MaterialFolder = "Assets/Materials/Greybox";
        const int IgnoreRaycastLayer = 2;

        static readonly Dictionary<string, Material> materials = new();

        [MenuItem("Kind Neighbors/Build Prototype Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            PrototypeData.EnsureFolder("Assets/Scenes");
            PrototypeData.EnsureFolder(MaterialFolder);
            materials.Clear();

            // 새 씬을 연 다음에 에셋을 불러와야 한다. 순서가 반대면 씬 전환 시 언로드된 에셋 참조가 null로 저장된다.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PrototypeData data = PrototypeData.LoadOrCreate();

            Light sun = CreateLighting();
            Transform map = new GameObject("Map").transform;
            CreateGround(map);
            CreateBuildings(map, out Transform bakeryNpcSpot, out Transform grandmaNpcSpot, out Transform mailboxSpot, out GameObject playerDoor);
            CreateForestEdge(map);

            // 시스템: 서로 직접 참조하지 않고 데이터 에셋(플래그, 이벤트 채널)만 공유한다
            var systems = new GameObject("Systems");

            var flow = systems.AddComponent<GameFlowController>();
            SetRef(flow, "config", data.flow);
            SetRef(flow, "flags", data.flags);
            SetRef(flow, "phaseChanged", data.phaseChanged);
            SetRef(flow, "advanceRequested", data.advanceRequested);
            SetRef(flow, "travelRequested", data.travelRequested);

            var lighting = systems.AddComponent<LightingController>();
            SetRef(lighting, "phaseChanged", data.phaseChanged);
            SetRef(lighting, "sun", sun);

            var deliveryManager = systems.AddComponent<DeliveryManager>();
            SetRef(deliveryManager, "flags", data.flags);
            SetRef(deliveryManager, "orderRequested", data.orderRequested);
            SetRef(deliveryManager, "deliverRequested", data.deliverRequested);
            SetRef(deliveryManager, "orderAccepted", data.orderAccepted);
            SetRef(deliveryManager, "orderCompleted", data.orderCompleted);

            var runner = systems.AddComponent<DialogueRunner>();
            SetRef(runner, "flags", data.flags);
            SetRef(runner, "dialogueRequested", data.dialogueRequested);
            SetRef(runner, "dialogueActive", data.inputLock);
            SetRef(systems.AddComponent<DialogueUI>(), "runner", runner);

            var nameEntry = systems.AddComponent<NameEntryUI>();
            SetRef(nameEntry, "flags", data.flags);
            SetRef(nameEntry, "inputLockRequested", data.inputLock);

            // 낮 NPC: 밤에는 사라진다. 직접 배달받는 NPC는 머리 위에 목적지 마커가 뜬다.
            Transform npcs = new GameObject("NPCs_Daytime").transform;
            CreateNpc("NPC_BakeryOwner", npcs, bakeryNpcSpot.position, bakeryNpcSpot.rotation, 1.5f, 0.8f,
                "NpcBakery", new Color(0.9f, 0.55f, 0.4f), data.bakerDialogue, data, deliveryManager, PrototypeData.BakerSpot);
            CreateNpc("NPC_Grandma", npcs, grandmaNpcSpot.position, grandmaNpcSpot.rotation, 1.2f, 0.75f,
                "NpcGrandma", new Color(0.65f, 0.55f, 0.75f), data.grandmaDialogue, data, deliveryManager, null);
            // 아이: 불 꺼진 솜의 집 앞 북쪽 길에서 논다. 털모자를 전하러 가는 길에 솜의 집을 지나치게 된다.
            CreateNpc("NPC_Child", npcs, new Vector3(3.2f, 0f, 12f), Quaternion.Euler(0f, 200f, 0f), 0.95f, 0.6f,
                "NpcChild", new Color(0.55f, 0.75f, 0.95f), data.childDialogue, data, deliveryManager, PrototypeData.ChildSpot);

            var npcSet = new GameObject("PhaseSet_DaytimeNPCs").AddComponent<PhaseObjectSet>();
            SetRef(npcSet, "phaseChanged", data.phaseChanged);
            SetArray(npcSet, "targets", npcs.gameObject);
            SetEnumArray(npcSet, "activeTimes", (int)TimeOfDay.Morning, (int)TimeOfDay.Evening);

            CreateMailbox(mailboxSpot, deliveryManager, PrototypeData.GrandmaMailbox);

            // 플레이어 집 바깥: 문(저녁 → 밤으로 넘기기 / 낮에 들어가기), 우편함(소식지)
            var homeDoor = playerDoor.AddComponent<PhaseAdvanceTrigger>();
            SetRef(homeDoor, "flow", flow);
            SetRef(homeDoor, "advanceRequested", data.advanceRequested);
            SetRef(homeDoor, "flags", data.flags);
            SetConditions(homeDoor, "conditions", PrototypeData.TimeIsNot(TimeOfDay.Night));
            AddTravel(playerDoor, "집에 들어가기", PrototypeData.SpawnInsideDoor, data, PrototypeData.TimeIs(TimeOfDay.Morning));
            CreateSpawnPoint(PrototypeData.SpawnOutsideDoor, null, new Vector3(0f, 0f, -14.3f), 0f);
            CreateHomeMailbox(new Vector3(1.9f, 0f, -14.9f), data);

            CreateVillageProps(map, data);
            Vector3 bedSpawn = BuildHome(data, flow);

            PlayerController player = CreatePlayer(bedSpawn, data, out PlayerInteractor interactor);

            var hud = systems.AddComponent<PrototypeHUD>();
            SetRef(hud, "player", player);
            SetRef(hud, "interactor", interactor);
            SetRef(hud, "phaseChanged", data.phaseChanged);
            SetRef(hud, "orderAccepted", data.orderAccepted);
            SetRef(hud, "orderCompleted", data.orderCompleted);
            SetRef(hud, "subtitle", data.subtitle);

            var reader = systems.AddComponent<ReaderUI>();
            SetRef(reader, "documentRequested", data.documentRequested);
            SetRef(reader, "inputLock", data.inputLock);

            var fader = systems.AddComponent<ScreenFader>();
            var travel = systems.AddComponent<TravelSystem>();
            SetRef(travel, "travelRequested", data.travelRequested);
            SetRef(travel, "inputLock", data.inputLock);
            SetRef(travel, "player", player);
            SetRef(travel, "fader", fader);

            EditorSceneManager.SaveScene(scene, ScenePath);
            SetBuildScenes(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PrototypeSceneBuilder] {ScenePath} 생성 완료");
        }

        [MenuItem("Kind Neighbors/Delete Save File")]
        public static void DeleteSave()
        {
            SaveSystem.Delete();
            Debug.Log($"[Save] 세이브 삭제: {SaveSystem.FilePath}");
        }

        // ---------- 환경 ----------

        static Light CreateLighting()
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.93f, 0.8f);
            sun.intensity = 0.9f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.5f, 0.47f);
            RenderSettings.ambientGroundColor = new Color(0.3f, 0.28f, 0.25f);
            return sun;
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

        static void CreateBuildings(Transform map, out Transform bakeryNpcSpot, out Transform grandmaNpcSpot, out Transform mailboxSpot, out GameObject playerDoor)
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
            grandmaNpcSpot = Spot("NpcSpot", grandma, new Vector3(-1.6f, 0f, 3.4f));

            // 솜의 집 (불 꺼진 집, 북동쪽)
            House("House_Som (Dark)", buildings, new Vector3(9f, 0f, 14f), -90f, new Vector3(5f, 3.5f, 5f),
                "HouseSom", new Color(0.35f, 0.33f, 0.35f), lightsOn: false);

            // 그 외 주민 집
            House("House_A", buildings, new Vector3(-8f, 0f, 14f), 90f, new Vector3(5f, 3.5f, 5f), "HouseA", new Color(0.7f, 0.85f, 0.95f));
            House("House_B", buildings, new Vector3(-8f, 0f, -12f), 90f, new Vector3(5f, 3.5f, 5f), "HouseB", new Color(0.95f, 0.85f, 0.7f));
            House("House_C", buildings, new Vector3(9f, 0f, -12f), -90f, new Vector3(5f, 3.5f, 5f), "HouseC", new Color(0.75f, 0.92f, 0.78f));

            // 플레이어 집 (남쪽 끝) — 문으로 시간대를 넘긴다 (집에 들어가기, 잠자기)
            Transform home = House("House_Player", buildings, new Vector3(0f, 0f, -18f), 0f, new Vector3(5f, 3.5f, 5f), "HousePlayer", new Color(0.95f, 0.95f, 0.88f));
            playerDoor = home.Find("Door").gameObject;
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

        /// <summary>
        /// 임시 NPC: 캡슐 몸 + 얼굴 방향 표시. 캐릭터 모델이 완성되면 Body 아래를 교체한다.
        /// destinationId가 있으면 직접 배달받는 목적지가 되어 머리 위에 마커가 뜬다.
        /// </summary>
        static void CreateNpc(string name, Transform parent, Vector3 position, Quaternion rotation, float height, float width,
            string matKey, Color color, DialogueGraph graph, PrototypeData data, DeliveryManager manager, string destinationId)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent);
            root.SetPositionAndRotation(position, rotation);

            var body = Prim(PrimitiveType.Capsule, "Body", root, position + Vector3.up * (height / 2f), new Vector3(width, height / 2f, width), matKey, color);
            body.transform.rotation = rotation;
            var face = Prim(PrimitiveType.Cube, "Face", root, position + Vector3.up * (height * 0.72f) + root.forward * (width / 2f),
                new Vector3(width * 0.4f, 0.12f, 0.1f), "Face", new Color(0.2f, 0.15f, 0.15f));
            face.transform.rotation = rotation;
            Object.DestroyImmediate(face.GetComponent<Collider>());

            var dialogue = root.gameObject.AddComponent<NpcDialogue>();
            SetRef(dialogue, "graph", graph);
            SetRef(dialogue, "flags", data.flags);
            SetRef(dialogue, "dialogueRequested", data.dialogueRequested);

            if (string.IsNullOrEmpty(destinationId))
                return;

            var marker = CreateMarker(root, position + Vector3.up * (height + 0.7f));
            var destination = root.gameObject.AddComponent<DeliveryDestination>();
            SetRef(destination, "manager", manager);
            SetString(destination, "destinationId", destinationId);
            SetBool(destination, "dropOffHere", false);
            SetRef(destination, "marker", marker);
        }

        static GameObject CreateMarker(Transform parent, Vector3 position)
        {
            var marker = Prim(PrimitiveType.Cube, "DestinationMarker", parent, position, Vector3.one * 0.35f, "Marker", new Color(0.3f, 0.85f, 0.95f));
            marker.transform.rotation = Quaternion.Euler(45f, 0f, 45f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            return marker;
        }

        static void CreateMailbox(Transform spot, DeliveryManager manager, string destinationId)
        {
            Transform mailbox = new GameObject("Mailbox_Grandma").transform;
            mailbox.SetPositionAndRotation(spot.position, spot.rotation);

            Prim(PrimitiveType.Cylinder, "Post", mailbox, spot.position + Vector3.up * 0.5f, new Vector3(0.12f, 0.5f, 0.12f), "Trunk", default);
            Prim(PrimitiveType.Cube, "Box", mailbox, spot.position + Vector3.up * 1.1f, new Vector3(0.5f, 0.4f, 0.7f), "Mailbox", new Color(0.85f, 0.3f, 0.3f))
                .transform.rotation = spot.rotation;

            var marker = CreateMarker(mailbox, spot.position + Vector3.up * 2.4f);

            var destination = mailbox.gameObject.AddComponent<DeliveryDestination>();
            SetRef(destination, "manager", manager);
            SetString(destination, "destinationId", destinationId);
            SetRef(destination, "marker", marker);
        }

        // ---------- 플레이어 ----------

        static PlayerController CreatePlayer(Vector3 position, PrototypeData data, out PlayerInteractor interactor)
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
            SetRef(controller, "inputLockRequested", data.inputLock);

            interactor = root.AddComponent<PlayerInteractor>();
            SetRef(interactor, "player", controller);

            return controller;
        }

        // ---------- 헬퍼 ----------

        static Transform House(string name, Transform parent, Vector3 position, float yaw, Vector3 size, string matKey, Color color, bool lightsOn = true)
        {
            string windowKey = lightsOn ? "Window" : "WindowDark";
            Color windowColor = lightsOn ? new Color(0.95f, 0.9f, 0.6f) : new Color(0.12f, 0.12f, 0.14f);
            Transform house = new GameObject(name).transform;
            house.SetParent(parent);
            house.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            BoxLocal("Walls", house, new Vector3(0f, size.y / 2f, 0f), size, matKey, color);
            BoxLocal("Roof", house, new Vector3(0f, size.y + 0.4f, 0f), new Vector3(size.x + 0.6f, 0.8f, size.z + 0.6f), "Roof", new Color(0.55f, 0.35f, 0.3f));
            BoxLocal("Door", house, new Vector3(0f, 1f, size.z / 2f + 0.05f), new Vector3(1.1f, 2f, 0.1f), "Door", new Color(0.5f, 0.35f, 0.22f));
            BoxLocal("Window_L", house, new Vector3(-size.x / 3.2f, size.y * 0.6f, size.z / 2f + 0.05f), new Vector3(0.9f, 0.9f, 0.1f), windowKey, windowColor);
            BoxLocal("Window_R", house, new Vector3(size.x / 3.2f, size.y * 0.6f, size.z / 2f + 0.05f), new Vector3(0.9f, 0.9f, 0.1f), windowKey, windowColor);
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

        static void SetBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).boolValue = value;
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

        static void SetEnumArray(Object target, string field, params int[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).enumValueIndex = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        /// <summary>이 씬을 첫 번째 빌드 씬으로 두고, 이미 지워진 씬 항목은 정리한다.</summary>
        static void SetBuildScenes(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path || AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path) == null);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
