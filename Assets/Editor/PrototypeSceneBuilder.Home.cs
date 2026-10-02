using KindNeighbors.Core.Flags;
using KindNeighbors.Dialogue;
using KindNeighbors.Flow;
using KindNeighbors.Home;
using KindNeighbors.Travel;
using KindNeighbors.World;
using UnityEditor;
using UnityEngine;

namespace KindNeighbors.EditorTools
{
    /// <summary>집 내부(방 하나 + 창밖 풍경)와 마을의 낮/밤 소품.</summary>
    public static partial class PrototypeSceneBuilder
    {
        // 집 내부는 마을과 떨어진 별도 공간에 두고 문에서 전환한다
        static readonly Vector3 HomeOrigin = new(0f, 0f, -200f);

        static readonly Color WallColor = new(0.86f, 0.82f, 0.74f);
        static readonly Color WoodColor = new(0.55f, 0.42f, 0.3f);
        static readonly Color ShadowColor = new(0.05f, 0.05f, 0.07f);

        /// <returns>아침에 일어나는 자리 (플레이어 초기 위치로도 쓴다)</returns>
        static Vector3 BuildHome(PrototypeData data, GameFlowController flow)
        {
            Transform room = new GameObject("Home_Interior").transform;
            room.position = HomeOrigin;

            BuildRoomShell(room);
            Transform window = BuildWindow(room, data);
            BuildWindowView(room);
            BuildFurniture(room, data, flow);
            BuildDrawing(room, data);
            BuildLights(room, data);
            BuildNightEvents(room, window, data);

            CreateSpawnPoint(PrototypeData.SpawnInsideDoor, room, HomeOrigin + new Vector3(0f, 0f, -1.4f), 0f);
            return CreateSpawnPoint(PrototypeData.SpawnBed, room, HomeOrigin + new Vector3(-0.6f, 0f, 1.0f), 30f).position;
        }

        static void BuildRoomShell(Transform room)
        {
            BoxLocal("Floor", room, new Vector3(0f, -0.1f, 0f), new Vector3(6.4f, 0.2f, 6.4f), "HomeFloor", WoodColor);
            BoxLocal("Ceiling", room, new Vector3(0f, 3.1f, 0f), new Vector3(6.4f, 0.2f, 6.4f), "HomeWall", WallColor);
            BoxLocal("Wall_Left", room, new Vector3(-3.1f, 1.5f, 0f), new Vector3(0.2f, 3f, 6.4f), "HomeWall", default);
            BoxLocal("Wall_Right", room, new Vector3(3.1f, 1.5f, 0f), new Vector3(0.2f, 3f, 6.4f), "HomeWall", default);
            BoxLocal("Wall_Front", room, new Vector3(0f, 1.5f, -3.1f), new Vector3(6.4f, 3f, 0.2f), "HomeWall", default);

            // 뒷벽은 창문 구멍(x -0.8~0.8, y 1~2.2)을 남기고 네 조각으로 만든다
            BoxLocal("Wall_Back_L", room, new Vector3(-2.0f, 1.5f, 3.1f), new Vector3(2.4f, 3f, 0.2f), "HomeWall", default);
            BoxLocal("Wall_Back_R", room, new Vector3(2.0f, 1.5f, 3.1f), new Vector3(2.4f, 3f, 0.2f), "HomeWall", default);
            BoxLocal("Wall_Back_Bottom", room, new Vector3(0f, 0.5f, 3.1f), new Vector3(1.6f, 1f, 0.2f), "HomeWall", default);
            BoxLocal("Wall_Back_Top", room, new Vector3(0f, 2.6f, 3.1f), new Vector3(1.6f, 0.8f, 0.2f), "HomeWall", default);
        }

        /// <summary>창문: 커튼, DAY 2 창문 대화, 커튼 치기/걷기가 모두 이 오브젝트에 붙는다.</summary>
        static Transform BuildWindow(Transform room, PrototypeData data)
        {
            Transform window = new GameObject("Window").transform;
            window.SetParent(room, false);

            // 막혀 있지만 보이지 않는 판: 상호작용 Raycast를 받고 플레이어가 나가지 못하게 한다
            var pane = BoxLocal("Pane (Invisible)", window, new Vector3(0f, 1.6f, 3.1f), new Vector3(1.6f, 1.2f, 0.2f), "HomeWall", default);
            pane.GetComponent<Renderer>().enabled = false;

            Color frame = new(0.95f, 0.95f, 0.92f);
            BoxLocal("Frame_V", window, new Vector3(0f, 1.6f, 3.02f), new Vector3(0.06f, 1.2f, 0.06f), "WindowFrame", frame);
            BoxLocal("Frame_H", window, new Vector3(0f, 1.6f, 3.02f), new Vector3(1.6f, 0.06f, 0.06f), "WindowFrame", default);
            BoxLocal("Sill", window, new Vector3(0f, 1.0f, 2.95f), new Vector3(1.9f, 0.06f, 0.3f), "WindowFrame", default);

            Color curtainColor = new(0.42f, 0.48f, 0.64f);
            Transform closed = Group("Curtain_Closed", window);
            BoxLocal("Panel_L", closed, new Vector3(-0.42f, 1.6f, 2.9f), new Vector3(0.86f, 1.5f, 0.05f), "Curtain", curtainColor);
            BoxLocal("Panel_R", closed, new Vector3(0.42f, 1.6f, 2.9f), new Vector3(0.86f, 1.5f, 0.05f), "Curtain", default);
            Transform open = Group("Curtain_Open", window);
            BoxLocal("Panel_L", open, new Vector3(-1.12f, 1.6f, 2.9f), new Vector3(0.4f, 1.5f, 0.1f), "Curtain", default);
            BoxLocal("Panel_R", open, new Vector3(1.12f, 1.6f, 2.9f), new Vector3(0.4f, 1.5f, 0.1f), "Curtain", default);
            Toggle("Toggle_CurtainClosed", room, data, closed.gameObject, PrototypeData.Is(FlagKeys.CurtainClosed));
            Toggle("Toggle_CurtainOpen", room, data, open.gameObject, new FlagCondition(FlagKeys.CurtainClosed, Comparison.Equal, 0));

            // DAY 2: 창문을 열면 창틀에 노란 실 한 가닥이 남는다
            var thread = BoxLocal("YellowThread", window, new Vector3(0.35f, 1.04f, 2.98f), new Vector3(0.3f, 0.012f, 0.012f), "YellowThread", new Color(1f, 0.85f, 0.15f));
            Object.DestroyImmediate(thread.GetComponent<Collider>());
            Toggle("Toggle_YellowThread", room, data, thread, PrototypeData.Is(PrototypeData.WindowOpened));

            // 상호작용 순서: 두드리는 중이면 창문 대화가 먼저, 아니면 커튼
            var dialogue = window.gameObject.AddComponent<NpcDialogue>();
            SetRef(dialogue, "graph", data.windowDialogue);
            SetRef(dialogue, "flags", data.flags);
            SetRef(dialogue, "dialogueRequested", data.dialogueRequested);
            SetString(dialogue, "promptOverride", "창문 살펴보기");
            AddFlagToggle(window.gameObject, data, FlagKeys.CurtainClosed, "커튼 치기", "커튼 걷기");

            // 두드릴 때 흔들리므로 정적 배칭에서 뺀다
            SetStaticRecursive(window.gameObject, false);
            return window;
        }

        /// <summary>창밖 풍경: 마을 뒷길 한 토막. 가로등 하나가 밤의 실루엣을 비춘다.</summary>
        static void BuildWindowView(Transform room)
        {
            Transform view = Group("WindowView", room);
            BoxLocal("Ground", view, new Vector3(0f, -0.05f, 12f), new Vector3(34f, 0.1f, 18f), "Grass", default);
            BoxLocal("Path", view, new Vector3(0f, 0.01f, 5.2f), new Vector3(34f, 0.02f, 2.2f), "Path", default);
            // 길 바로 뒤의 이웃집 벽: 가로등에 비쳐 밝게 보이고, 그 앞을 지나는 형체가 검은 실루엣으로 드러난다
            BoxLocal("House_Back_Wall", view, new Vector3(0f, 1.9f, 9f), new Vector3(14f, 3.8f, 0.6f), "HouseB", default);
            NoCollider(BoxLocal("House_Back_Window", view, new Vector3(-3.5f, 2.1f, 8.68f), new Vector3(1f, 0.9f, 0.05f), "Window", default));
            BoxLocal("House_Far_L", view, new Vector3(-7f, 1.75f, 13f), new Vector3(5f, 3.5f, 5f), "HouseA", default);
            BoxLocal("House_Far_R", view, new Vector3(7.5f, 1.75f, 15f), new Vector3(5f, 3.5f, 5f), "HouseC", default);
            for (int i = 0; i < 7; i++)
            {
                float x = -12f + i * 4f;
                BoxLocal($"Tree_{i}", view, new Vector3(x, 2.5f, 20f + (i % 2) * 1.5f), new Vector3(1.8f, 5f, 1.8f), "Leaves", default);
            }
        }

        static void BuildFurniture(Transform room, PrototypeData data, GameFlowController flow)
        {
            // 침대: 밤에만 잠들 수 있다 (그날 밤 이벤트가 끝나야 함)
            var bed = BoxLocal("Bed", room, new Vector3(-2.0f, 0.3f, 1.7f), new Vector3(1.3f, 0.6f, 2.2f), "Bed", new Color(0.85f, 0.8f, 0.75f));
            NoCollider(BoxLocal("Pillow", room, new Vector3(-2.0f, 0.68f, 2.5f), new Vector3(0.8f, 0.18f, 0.4f), "Pillow", new Color(0.95f, 0.95f, 0.98f)));
            var sleep = bed.AddComponent<PhaseAdvanceTrigger>();
            SetRef(sleep, "flow", flow);
            SetRef(sleep, "advanceRequested", data.advanceRequested);
            SetRef(sleep, "flags", data.flags);
            SetConditions(sleep, "conditions", PrototypeData.TimeIs(TimeOfDay.Night));

            // 책상 위 내 일지
            BoxLocal("Desk", room, new Vector3(2.0f, 0.4f, 2.3f), new Vector3(1.6f, 0.8f, 0.8f), "Desk", WoodColor * 1.15f);
            var journal = BoxLocal("MyJournal", room, new Vector3(2.2f, 0.84f, 2.3f), new Vector3(0.35f, 0.06f, 0.45f), "Journal", new Color(0.3f, 0.45f, 0.35f));
            AddReadable(journal, data, data.myJournal, "일지 읽기");

            // 배달 가방 (안주머니에 솜의 일지). 가방을 받은 뒤에만 집에 있다.
            Transform bagRoot = Group("Bag", room);
            var bag = BoxLocal("CourierBag", bagRoot, new Vector3(1.0f, 0.25f, 2.55f), new Vector3(0.55f, 0.5f, 0.3f), "Bag", new Color(0.45f, 0.3f, 0.2f));
            AddReadable(bag, data, data.somJournal, "가방 안주머니 살펴보기", PrototypeData.Is(PrototypeData.HasBag));
            Toggle("Toggle_Bag", room, data, bagRoot.gameObject, PrototypeData.Is(PrototypeData.HasBag));

            // 짐가방: 사연은 암시만 (편도 버스표)
            var suitcase = BoxLocal("Suitcase", room, new Vector3(-2.55f, 0.25f, -2.3f), new Vector3(0.8f, 0.5f, 0.35f), "Suitcase", new Color(0.35f, 0.33f, 0.38f));
            AddReadable(suitcase, data, data.suitcase, "짐가방 살펴보기");

            // 선반과 답례품
            BoxLocal("Shelf", room, new Vector3(2.85f, 1.4f, -0.6f), new Vector3(0.4f, 0.06f, 1.8f), "Desk", default);
            var bread = BoxLocal("Gift_Bread", room, new Vector3(2.85f, 1.52f, -1.15f), new Vector3(0.3f, 0.18f, 0.22f), "GiftBread", new Color(0.85f, 0.65f, 0.35f));
            Toggle("Toggle_GiftBread", room, data, bread, PrototypeData.Is(PrototypeData.GiftBread));
            var yarn = Prim(PrimitiveType.Sphere, "Gift_Yarn", room, HomeOrigin + new Vector3(2.85f, 1.55f, -0.55f), Vector3.one * 0.22f, "GiftYarn", new Color(0.85f, 0.5f, 0.6f));
            Toggle("Toggle_GiftYarn", room, data, yarn, PrototypeData.Is(PrototypeData.GiftYarn));

            // 안쪽 문: 낮에는 나가기, 밤에는 잠그기/풀기
            Color doorColor = new(0.5f, 0.35f, 0.22f);
            var door = BoxLocal("Door_Inside", room, new Vector3(0f, 1f, -2.97f), new Vector3(1.1f, 2f, 0.08f), "Door", doorColor);
            AddTravel(door, "나가기", PrototypeData.SpawnOutsideDoor, data, PrototypeData.TimeIs(TimeOfDay.Morning));
            AddFlagToggle(door, data, FlagKeys.DoorLocked, "문 잠그기", "잠금 풀기", PrototypeData.TimeIs(TimeOfDay.Night));
            var bolt = BoxLocal("Bolt_Locked", room, new Vector3(0.42f, 1.05f, -2.9f), new Vector3(0.22f, 0.05f, 0.05f), "Bolt", new Color(0.7f, 0.7f, 0.72f));
            Object.DestroyImmediate(bolt.GetComponent<Collider>());
            Toggle("Toggle_Bolt", room, data, bolt, PrototypeData.Is(FlagKeys.DoorLocked));
        }

        /// <summary>
        /// 아이의 그림: 마을 사람들과 길쭉한 형체. 형체는 밤마다(화면이 가려진 동안) 배달원 쪽으로 다가온다.
        /// 그림은 왼쪽 벽에 걸리고, 방 안에서 보면 z가 작을수록 왼쪽이다.
        /// </summary>
        static void BuildDrawing(Transform room, PrototypeData data)
        {
            Transform drawing = Group("Drawing", room);
            const float x = -2.97f;
            const float ground = 1.42f;

            BoxLocal("Paper", drawing, new Vector3(x, 1.7f, -0.6f), new Vector3(0.03f, 0.75f, 1.3f), "Paper", new Color(0.97f, 0.96f, 0.9f));
            NoCollider(BoxLocal("Tree", drawing, new Vector3(x + 0.02f, ground + 0.15f, -1.12f), new Vector3(0.01f, 0.3f, 0.08f), "DrawGreen", new Color(0.3f, 0.6f, 0.35f)));
            NoCollider(BoxLocal("House_1", drawing, new Vector3(x + 0.02f, ground + 0.1f, -0.82f), new Vector3(0.01f, 0.2f, 0.18f), "DrawOrange", new Color(0.95f, 0.6f, 0.3f)));
            NoCollider(BoxLocal("House_2", drawing, new Vector3(x + 0.02f, ground + 0.1f, -0.42f), new Vector3(0.01f, 0.2f, 0.18f), "DrawPurple", new Color(0.65f, 0.5f, 0.8f)));
            NoCollider(BoxLocal("Baker", drawing, new Vector3(x + 0.02f, ground + 0.06f, -0.68f), new Vector3(0.01f, 0.12f, 0.05f), "DrawOrange", default));
            NoCollider(BoxLocal("Grandma", drawing, new Vector3(x + 0.02f, ground + 0.05f, -0.56f), new Vector3(0.01f, 0.1f, 0.05f), "DrawPurple", default));
            NoCollider(BoxLocal("Child", drawing, new Vector3(x + 0.02f, ground + 0.04f, -0.28f), new Vector3(0.01f, 0.08f, 0.04f), "DrawBlue", new Color(0.4f, 0.6f, 0.95f)));
            NoCollider(BoxLocal("Courier", drawing, new Vector3(x + 0.02f, ground + 0.06f, -0.1f), new Vector3(0.01f, 0.12f, 0.05f), "DrawInk", new Color(0.2f, 0.2f, 0.22f)));
            var figure = NoCollider(BoxLocal("TallFigure", drawing, new Vector3(x + 0.025f, ground + 0.2f, -1.12f), new Vector3(0.01f, 0.4f, 0.04f), "DrawBlack", new Color(0.03f, 0.03f, 0.03f)));
            figure.isStatic = false; // 밤마다 자리를 옮긴다
            Toggle("Toggle_Drawing", room, data, drawing.gameObject, PrototypeData.Is(PrototypeData.GiftDrawing));

            // 형체의 자리: 숲 가장자리 → 집들 사이 → 배달원 바로 옆
            Transform control = Group("DrawingFigureControl", room);
            var stages = new[]
            {
                Anchor("Stage_Forest", control, new Vector3(x + 0.025f, ground + 0.2f, -1.02f)),
                Anchor("Stage_Village", control, new Vector3(x + 0.025f, ground + 0.2f, -0.62f)),
                Anchor("Stage_Courier", control, new Vector3(x + 0.025f, ground + 0.2f, -0.17f)),
            };
            var drawingFigure = control.gameObject.AddComponent<DrawingFigure>();
            SetRef(drawingFigure, "flags", data.flags);
            SetRef(drawingFigure, "phaseChanged", data.phaseChanged);
            SetRef(drawingFigure, "figure", figure.transform);
            SetArray(drawingFigure, "stages", stages);
        }

        static void BuildLights(Transform room, PrototypeData data)
        {
            // 천장 등: 불 끄기/켜기 스위치는 문 옆
            NoCollider(BoxLocal("CeilingLamp", room, new Vector3(0f, 2.92f, 0f), new Vector3(0.5f, 0.15f, 0.5f), "LampShade", new Color(1f, 0.92f, 0.7f)));
            var lamp = PointLight("LampLight", room, HomeOrigin + new Vector3(0f, 2.6f, 0f), new Color(1f, 0.85f, 0.6f), 1.3f, 9f, LightShadows.Soft);
            Toggle("Toggle_Lamp", room, data, lamp, new FlagCondition(FlagKeys.LampOff, Comparison.Equal, 0));
            var lampSwitch = BoxLocal("LampSwitch", room, new Vector3(0.95f, 1.3f, -2.98f), new Vector3(0.12f, 0.18f, 0.04f), "WindowFrame", default);
            AddFlagToggle(lampSwitch, data, FlagKeys.LampOff, "불 끄기", "불 켜기");

            // 낮에는 창으로 들어오는 빛을 대신하는 보조광
            var fill = PointLight("DaylightFill", room, HomeOrigin + new Vector3(0f, 2.4f, 1.8f), new Color(0.85f, 0.9f, 1f), 0.7f, 8f, LightShadows.None);
            PhaseSet("PhaseSet_HomeDaylight", room, data, new[] { TimeOfDay.Morning, TimeOfDay.Evening }, fill);

            // 창밖 가로등: 저녁부터 켜진다
            Transform lamppost = Group("StreetLamp_View", room);
            Prim(PrimitiveType.Cylinder, "Pole", lamppost, HomeOrigin + new Vector3(2.6f, 1.5f, 7.6f), new Vector3(0.12f, 1.5f, 0.12f), "LampPole", new Color(0.2f, 0.2f, 0.22f));
            var glow = PointLight("Glow", lamppost, HomeOrigin + new Vector3(2.2f, 3.1f, 7.4f), new Color(1f, 0.8f, 0.5f), 2.6f, 9f, LightShadows.Soft);
            PhaseSet("PhaseSet_ViewLamp", room, data, new[] { TimeOfDay.Evening, TimeOfDay.Night }, glow);
        }

        static void BuildNightEvents(Transform room, Transform window, PrototypeData data)
        {
            Vector3 windowCenter = HomeOrigin + new Vector3(0f, 1.6f, 3.1f);
            Transform windowTarget = Anchor("WindowLookTarget", room, windowCenter - HomeOrigin);

            // DAY 1: 창밖을 지나가는 실루엣
            var walker = TallFigure("Silhouette_D1", room, HomeOrigin + new Vector3(-7f, 0f, 5.5f));
            Transform pathRoot = Group("SilhouettePath", room);
            var path = new[]
            {
                Anchor("P0_Left", pathRoot, new Vector3(-7f, 0f, 5.6f)),
                Anchor("P1_Window", pathRoot, new Vector3(0f, 0f, 4.4f)),
                Anchor("P2_Right", pathRoot, new Vector3(7f, 0f, 5.6f)),
            };
            var silhouette = room.gameObject.AddComponent<SilhouetteEvent>();
            WireNightEvent(silhouette, data, 1, PrototypeData.SilhouetteDone);
            SetRef(silhouette, "figure", walker);
            SetArray(silhouette, "path", path);
            SetRef(silhouette, "window", windowTarget);
            SetString(silhouette, "seenFlag", PrototypeData.SilhouetteSeen);

            // DAY 2: 창 바로 앞에 서서 두드린다
            var knocker = TallFigure("Knocker_D2", room, HomeOrigin + new Vector3(0f, 0f, 3.9f));
            knocker.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var knock = room.gameObject.AddComponent<WindowKnockEvent>();
            WireNightEvent(knock, data, 2, PrototypeData.KnockDone);
            SetString(knock, "activeFlag", PrototypeData.KnockActive);
            SetString(knock, "ignoredFlag", PrototypeData.WindowIgnored);
            SetRef(knock, "figure", knocker);
            SetRef(knock, "windowVisual", window);
        }

        static void WireNightEvent(NightEvent nightEvent, PrototypeData data, int day, string doneFlag)
        {
            SetRef(nightEvent, "flags", data.flags);
            SetRef(nightEvent, "phaseChanged", data.phaseChanged);
            SetRef(nightEvent, "subtitle", data.subtitle);
            SetInt(nightEvent, "day", day);
            SetString(nightEvent, "doneFlag", doneFlag);
        }

        /// <summary>밤의 것: 크고 길쭉한 어두운 형체. 처음에는 꺼져 있다.</summary>
        static GameObject TallFigure(string name, Transform parent, Vector3 position)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, true);
            root.transform.position = position;
            var body = Prim(PrimitiveType.Capsule, "Body", root.transform, position + Vector3.up * 1.7f, new Vector3(0.85f, 1.7f, 0.6f), "NightThing", ShadowColor);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            root.SetActive(false);
            return root;
        }

        // ---------- 마을 ----------

        /// <summary>플레이어 집 우편함: 매일 아침 소식지가 들어 있다.</summary>
        static void CreateHomeMailbox(Vector3 position, PrototypeData data)
        {
            Transform mailbox = new GameObject("Mailbox_Home").transform;
            mailbox.position = position;
            Prim(PrimitiveType.Cylinder, "Post", mailbox, position + Vector3.up * 0.5f, new Vector3(0.12f, 0.5f, 0.12f), "Trunk", default);
            Prim(PrimitiveType.Cube, "Box", mailbox, position + Vector3.up * 1.1f, new Vector3(0.5f, 0.4f, 0.7f), "MailboxHome", new Color(0.3f, 0.5f, 0.8f));
            AddReadable(mailbox.gameObject, data, data.newsletter, "소식지 읽기");
        }

        /// <summary>낮에만 있는 생활 소품과, 밤에 자리가 바뀌는 공, 저녁부터 켜지는 가로등.</summary>
        static void CreateVillageProps(Transform map, PrototypeData data)
        {
            Transform day = Group("Props_Day", map);
            // 빨랫줄 (House_B 옆)
            Prim(PrimitiveType.Cylinder, "LaundryPole_L", day, new Vector3(-12f, 0.9f, -8f), new Vector3(0.08f, 0.9f, 0.08f), "Trunk", default);
            Prim(PrimitiveType.Cylinder, "LaundryPole_R", day, new Vector3(-8f, 0.9f, -8f), new Vector3(0.08f, 0.9f, 0.08f), "Trunk", default);
            NoCollider(Prim(PrimitiveType.Cube, "Line", day, new Vector3(-10f, 1.75f, -8f), new Vector3(4f, 0.02f, 0.02f), "LampPole", new Color(0.2f, 0.2f, 0.22f)));
            NoCollider(Prim(PrimitiveType.Cube, "Cloth_1", day, new Vector3(-11f, 1.45f, -8f), new Vector3(0.7f, 0.6f, 0.03f), "ClothPink", new Color(0.95f, 0.7f, 0.75f)));
            NoCollider(Prim(PrimitiveType.Cube, "Cloth_2", day, new Vector3(-9.6f, 1.5f, -8f), new Vector3(0.6f, 0.5f, 0.03f), "ClothBlue", new Color(0.6f, 0.75f, 0.95f)));
            // 화분 (할머니 집 앞)
            for (int i = 0; i < 3; i++)
            {
                var pot = Prim(PrimitiveType.Cylinder, $"FlowerPot_{i}", day, new Vector3(12.4f, 0.2f, -3.2f + i * 0.6f), new Vector3(0.35f, 0.2f, 0.35f), "Pot", new Color(0.75f, 0.45f, 0.3f));
                NoCollider(Prim(PrimitiveType.Sphere, "Flower", pot.transform, pot.transform.position + Vector3.up * 0.35f, Vector3.one * 0.3f, "Flower", new Color(0.95f, 0.5f, 0.6f)));
            }
            // 광장의 공: 낮에는 광장, 밤에는 불 꺼진 집 앞
            Prim(PrimitiveType.Sphere, "Ball", day, new Vector3(2f, 0.25f, 1.5f), Vector3.one * 0.5f, "Ball", new Color(0.9f, 0.3f, 0.3f));
            PhaseSet("PhaseSet_DayProps", map, data, new[] { TimeOfDay.Morning, TimeOfDay.Evening }, day.gameObject);

            Transform night = Group("Props_Night", map);
            Prim(PrimitiveType.Sphere, "Ball_Moved", night, new Vector3(6.2f, 0.25f, 12.6f), Vector3.one * 0.5f, "Ball", default);
            PhaseSet("PhaseSet_NightProps", map, data, new[] { TimeOfDay.Night }, night.gameObject);

            // 가로등: 몇 개만 (GDD: 밤에는 가로등 몇 개만 켜짐)
            Transform lamps = Group("StreetLamps", map);
            var glows = new[]
            {
                StreetLamp(lamps, new Vector3(-6.5f, 0f, -6.5f)),
                StreetLamp(lamps, new Vector3(6.5f, 0f, 6.5f)),
                StreetLamp(lamps, new Vector3(1.8f, 0f, 20f)),
                StreetLamp(lamps, new Vector3(1.8f, 0f, -13f)),
            };
            PhaseSet("PhaseSet_StreetLamps", map, data, new[] { TimeOfDay.Evening, TimeOfDay.Night }, glows);
        }

        static GameObject StreetLamp(Transform parent, Vector3 position)
        {
            Transform lamp = new GameObject($"StreetLamp ({position.x:0},{position.z:0})").transform;
            lamp.SetParent(parent);
            lamp.position = position;
            Prim(PrimitiveType.Cylinder, "Pole", lamp, position + Vector3.up * 1.6f, new Vector3(0.12f, 1.6f, 0.12f), "LampPole", default);
            NoCollider(Prim(PrimitiveType.Cube, "Head", lamp, position + Vector3.up * 3.25f, new Vector3(0.35f, 0.2f, 0.35f), "LampShade", new Color(1f, 0.92f, 0.7f)));
            return PointLight("Glow", lamp, position + Vector3.up * 3f, new Color(1f, 0.8f, 0.5f), 2f, 11f, LightShadows.None);
        }

        // ---------- 컴포넌트 헬퍼 ----------

        static Transform CreateSpawnPoint(string id, Transform parent, Vector3 position, float yaw)
        {
            var go = new GameObject($"Spawn_{id}");
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            SetString(go.AddComponent<SpawnPoint>(), "id", id);
            return go.transform;
        }

        static void AddTravel(GameObject target, string prompt, string spawnId, PrototypeData data, params FlagCondition[] conditions)
        {
            var travel = target.AddComponent<TravelInteractable>();
            SetString(travel, "prompt", prompt);
            SetString(travel, "targetSpawnId", spawnId);
            SetRef(travel, "flags", data.flags);
            SetConditions(travel, "conditions", conditions);
            SetRef(travel, "travelRequested", data.travelRequested);
        }

        static void AddFlagToggle(GameObject target, PrototypeData data, string key, string promptOff, string promptOn, params FlagCondition[] conditions)
        {
            var toggle = target.AddComponent<FlagToggleInteractable>();
            SetRef(toggle, "flags", data.flags);
            SetString(toggle, "flagKey", key);
            SetString(toggle, "promptWhenOff", promptOff);
            SetString(toggle, "promptWhenOn", promptOn);
            SetConditions(toggle, "conditions", conditions);
        }

        static void AddReadable(GameObject target, PrototypeData data, DocumentAsset document, string prompt, params FlagCondition[] conditions)
        {
            var readable = target.AddComponent<ReadableObject>();
            SetRef(readable, "document", document);
            SetString(readable, "prompt", prompt);
            SetRef(readable, "flags", data.flags);
            SetConditions(readable, "conditions", conditions);
            SetRef(readable, "documentRequested", data.documentRequested);
        }

        /// <summary>조건이 참일 때만 target을 켜는 FlagObjectToggle을 별도 오브젝트에 만든다.</summary>
        static void Toggle(string name, Transform parent, PrototypeData data, GameObject target, params FlagCondition[] conditions)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var toggle = go.AddComponent<FlagObjectToggle>();
            SetRef(toggle, "flags", data.flags);
            SetConditions(toggle, "conditions", conditions);
            SetArray(toggle, "targets", target);
        }

        static PhaseObjectSet PhaseSet(string name, Transform parent, PrototypeData data, TimeOfDay[] times, params GameObject[] targets)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var set = go.AddComponent<PhaseObjectSet>();
            SetRef(set, "phaseChanged", data.phaseChanged);
            SetArray(set, "targets", targets);
            SetEnumArray(set, "activeTimes", System.Array.ConvertAll(times, t => (int)t));
            return set;
        }

        /// <param name="position">월드 좌표 (집 안이면 HomeOrigin을 더해야 한다)</param>
        static GameObject PointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range, LightShadows shadows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows;
            return go;
        }

        static Transform Group(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static Transform Anchor(string name, Transform parent, Vector3 localPosition)
        {
            var t = Group(name, parent);
            t.localPosition = localPosition;
            return t;
        }

        static void SetStaticRecursive(GameObject go, bool isStatic)
        {
            go.isStatic = isStatic;
            foreach (Transform child in go.transform)
                SetStaticRecursive(child.gameObject, isStatic);
        }

        static GameObject NoCollider(GameObject go)
        {
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static void SetInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetConditions(Object target, string field, params FlagCondition[] conditions)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = conditions.Length;
            for (int i = 0; i < conditions.Length; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("key").stringValue = conditions[i].key;
                element.FindPropertyRelative("comparison").enumValueIndex = (int)conditions[i].comparison;
                element.FindPropertyRelative("value").intValue = conditions[i].value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
