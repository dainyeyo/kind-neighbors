using System.Collections.Generic;
using KindNeighbors.Core.Flags;
using KindNeighbors.Delivery;
using KindNeighbors.Flow;
using KindNeighbors.World;
using UnityEditor;
using UnityEngine;

namespace KindNeighbors.EditorTools
{
    /// <summary>
    /// 마을(광장 중심, 북적임)과 외곽 종점(숙소, 솜의 집, 정류장, 숲 경계).
    /// 두 곳은 멀리 떨어진 별도 공간이고 버스(화면 전환)로 오간다.
    /// </summary>
    public static partial class PrototypeSceneBuilder
    {
        static readonly Vector3 OutskirtsOrigin = new(400f, 0f, 0f);
        const float ShopRing = 17f;

        static readonly Color Stone = new(0.72f, 0.7f, 0.66f);
        static readonly Color ShutterColor = new(0.45f, 0.4f, 0.36f);

        // ================= 마을 =================

        /// <returns>빵집 건물 (문에 빵집 출입을 붙인다)</returns>
        static Transform BuildVillage(PrototypeData data, DeliveryManager deliveryManager, out Transform grandmaNpcSpot, out Transform mailboxSpot)
        {
            Transform village = new GameObject("Village").transform;
            village.gameObject.AddComponent<VillageArea>();

            // 바닥, 광장, 분수
            Box("Ground", village, new Vector3(0f, -0.5f, 0f), new Vector3(100f, 1f, 100f), "Grass", new Color(0.55f, 0.72f, 0.45f));
            NoCollider(Prim(PrimitiveType.Cylinder, "Plaza", village, new Vector3(0f, 0.01f, 0f), new Vector3(22f, 0.02f, 22f), "Path", new Color(0.72f, 0.64f, 0.5f)));
            Prim(PrimitiveType.Cylinder, "Fountain_Base", village, new Vector3(0f, 0.2f, 0f), new Vector3(5f, 0.2f, 5f), "Stone", Stone);
            NoCollider(Prim(PrimitiveType.Cylinder, "Fountain_Water", village, new Vector3(0f, 0.41f, 0f), new Vector3(4.4f, 0.02f, 4.4f), "Water", new Color(0.45f, 0.65f, 0.85f)));
            Prim(PrimitiveType.Cylinder, "Fountain_Pillar", village, new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 0.6f, 0.6f), "Stone", default);
            Prim(PrimitiveType.Cylinder, "Fountain_Bowl", village, new Vector3(0f, 1.55f, 0f), new Vector3(1.6f, 0.08f, 1.6f), "Stone", default);

            // 광장을 빙 둘러싼 가게들 (모두 광장을 향한다). 정남쪽은 정류장 자리.
            var shutters = new List<GameObject>();
            Transform shops = Group("Shops", village);
            Transform bakery = Shop("Bakery", shops, -90f, new Vector3(7f, 4.5f, 6f), "Bakery", new Color(0.95f, 0.78f, 0.6f), new Color(0.85f, 0.45f, 0.3f), shutters, chimney: true);
            Transform grocery = Shop("Grocery", shops, -45f, new Vector3(6f, 3.8f, 5f), "Grocery", new Color(0.75f, 0.88f, 0.7f), new Color(0.4f, 0.65f, 0.35f), shutters, chimney: true);
            Transform post = Shop("PostOffice", shops, 0f, new Vector3(7f, 4.2f, 5f), "PostOffice", new Color(0.85f, 0.4f, 0.35f), new Color(0.95f, 0.95f, 0.9f), shutters, chimney: true);
            Transform florist = Shop("FlowerShop", shops, 45f, new Vector3(5.5f, 3.6f, 5f), "FlowerShop", new Color(0.95f, 0.8f, 0.85f), new Color(0.85f, 0.45f, 0.6f), shutters);
            Transform cafe = Shop("Cafe", shops, 90f, new Vector3(6f, 3.8f, 5.5f), "Cafe", new Color(0.85f, 0.75f, 0.6f), new Color(0.45f, 0.3f, 0.2f), shutters, chimney: true);
            Transform clinic = Shop("Clinic", shops, 135f, new Vector3(6f, 4f, 5f), "Clinic", new Color(0.9f, 0.93f, 0.95f), new Color(0.35f, 0.55f, 0.8f), shutters);
            Transform tailor = Shop("Tailor", shops, -135f, new Vector3(5.5f, 3.8f, 5f), "Tailor", new Color(0.7f, 0.75f, 0.9f), new Color(0.55f, 0.45f, 0.75f), shutters);

            // 골목 너머: 동쪽 주택가(할머니), 북쪽 공터(아이), 서쪽 주택가, 남서쪽 우물
            Transform homes = Group("Houses", village);
            Transform grandma = House("House_Grandma", homes, Ring(112f, 31f), 112f + 180f, new Vector3(6f, 3.5f, 5f), "HouseGrandma", new Color(0.8f, 0.7f, 0.9f));
            mailboxSpot = Spot("MailboxSpot", grandma, new Vector3(1.8f, 0f, 3.8f));
            grandmaNpcSpot = Spot("NpcSpot", grandma, new Vector3(-1.6f, 0f, 3.4f));
            Transform eastA = House("House_East_A", homes, Ring(98f, 33f), 98f + 180f, new Vector3(5f, 3.5f, 5f), "HouseA", new Color(0.7f, 0.85f, 0.95f));
            Transform eastB = House("House_East_B", homes, Ring(128f, 32f), 128f + 180f, new Vector3(5f, 3.3f, 5f), "HouseB", new Color(0.95f, 0.85f, 0.7f));
            Transform northA = House("House_North_A", homes, Ring(8f, 33f), 8f + 180f, new Vector3(5f, 3.5f, 5f), "HouseC", new Color(0.75f, 0.92f, 0.78f));
            Transform northB = House("House_North_B", homes, Ring(40f, 33f), 40f + 180f, new Vector3(5f, 3.4f, 5f), "HouseA", default);
            Transform westA = House("House_West_A", homes, Ring(-60f, 31f), -60f + 180f, new Vector3(5f, 3.5f, 5f), "HouseB", default);
            Transform westB = House("House_West_B", homes, Ring(-80f, 33f), -80f + 180f, new Vector3(5.5f, 3.6f, 5f), "HouseC", default);
            Transform westC = House("House_West_C", homes, Ring(-102f, 32f), -102f + 180f, new Vector3(5f, 3.3f, 5f), "HouseA", default);
            foreach (var house in new[] { eastA, northA, westB })
                AddChimney(house, house.Find("Roof").localPosition.y, 2.5f);

            // 길: 광장에서 각 골목으로
            Transform paths = Group("Paths", village);
            PathStrip(paths, Ring(180f, 10f), Ring(180f, 18f), 3.5f);   // 정류장
            PathStrip(paths, Ring(112f, 10f), Ring(112f, 28f), 2.5f);   // 동쪽 주택가
            PathStrip(paths, Ring(22f, 10f), Ring(22f, 27f), 2.5f);     // 북쪽 공터
            PathStrip(paths, Ring(-80f, 10f), Ring(-80f, 28f), 2.5f);   // 서쪽 주택가
            PathStrip(paths, Ring(-155f, 10f), Ring(-155f, 22f), 2f);   // 우물

            BuildPlayground(village, Ring(22f, 30f));
            Prim(PrimitiveType.Cylinder, "Well", village, Ring(-155f, 24f) + Vector3.up * 0.45f, new Vector3(1.6f, 0.45f, 1.6f), "Stone", default);

            BuildPlazaProps(village, data, shutters);
            BuildVillageStop(village, data);
            BuildForestRing(village, 44f);

            // 저녁: 덧창과 좌판 덮개가 하나둘 닫힌다 (플레이어가 마을에 있을 때)
            var closing = new GameObject("EveningClosing").AddComponent<PhaseDelayedToggle>();
            closing.transform.SetParent(village, false);
            SetRef(closing, "flags", data.flags);
            SetRef(closing, "phaseChanged", data.phaseChanged);
            SetArray(closing, "targets", shutters.ToArray());

            // 이름 있는 주민: 할머니, 아이
            Transform npcs = Group("NPCs_Daytime", village);
            CreateNpc("NPC_Grandma", npcs, grandmaNpcSpot.position, grandmaNpcSpot.rotation, 1.2f, 0.75f,
                "NpcGrandma", new Color(0.65f, 0.55f, 0.75f), data.grandmaDialogue, data, deliveryManager, null);
            Vector3 lot = Ring(22f, 30f);
            CreateNpc("NPC_Child", npcs, lot + new Vector3(-1.5f, 0f, -2.2f), Quaternion.Euler(0f, 202f, 0f), 0.95f, 0.6f,
                "NpcChild", new Color(0.55f, 0.75f, 0.95f), data.childDialogue, data, deliveryManager, PrototypeData.ChildSpot);
            var npcSet = PhaseSet("PhaseSet_DaytimeNPCs", village, data, new[] { TimeOfDay.Morning, TimeOfDay.Evening }, npcs.gameObject);
            SetInt(npcSet, "fromDay", 1);

            BuildResidents(village, data, bakery, grocery, florist, cafe, clinic, tailor, westA, northA, northB);
            return bakery;
        }

        static Transform Shop(string name, Transform parent, float angle, Vector3 size, string matKey, Color wall, Color accent, List<GameObject> shutters, bool chimney = false)
        {
            Transform shop = House(name, parent, Ring(angle, ShopRing + size.z / 2f), angle + 180f, size, matKey, wall);
            float front = size.z / 2f;

            // 간판과 차양
            BoxLocal("Sign", shop, new Vector3(0f, size.y - 0.4f, front + 0.12f), new Vector3(Mathf.Min(2.6f, size.x * 0.5f), 0.55f, 0.1f), $"{matKey}_Accent", accent);
            NoCollider(BoxLocal("Awning", shop, new Vector3(0f, 2.3f, front + 0.65f), new Vector3(size.x * 0.85f, 0.08f, 1.3f), $"{matKey}_Accent", default));

            // 저녁에 닫히는 덧창 (처음엔 꺼 둔다)
            Transform shutter = Group("Shutters", shop);
            NoCollider(BoxLocal("Shutter_Door", shutter, new Vector3(0f, 1f, front + 0.1f), new Vector3(1.2f, 2.05f, 0.05f), "Shutter", ShutterColor));
            NoCollider(BoxLocal("Shutter_L", shutter, new Vector3(-size.x / 3.2f, size.y * 0.6f, front + 0.1f), new Vector3(1f, 1f, 0.05f), "Shutter", default));
            NoCollider(BoxLocal("Shutter_R", shutter, new Vector3(size.x / 3.2f, size.y * 0.6f, front + 0.1f), new Vector3(1f, 1f, 0.05f), "Shutter", default));
            shutter.gameObject.SetActive(false);
            shutters.Add(shutter.gameObject);

            if (chimney)
                AddChimney(shop, size.y + 0.4f, size.x * 0.3f);
            return shop;
        }

        /// <summary>굴뚝과 연기 (낮과 저녁에만 피어오른다)</summary>
        static void AddChimney(Transform house, float roofY, float offsetX)
        {
            BoxLocal("Chimney", house, new Vector3(offsetX, roofY + 0.6f, -0.8f), new Vector3(0.5f, 1.1f, 0.5f), "Roof", default);
            Smoke(house, house.TransformPoint(new Vector3(offsetX, roofY + 1.2f, -0.8f)));
        }

        static void BuildPlazaProps(Transform village, PrototypeData data, List<GameObject> shutters)
        {
            Transform props = Group("PlazaProps", village);

            // 좌판 3개 (광장 북쪽 반원). 저녁엔 덮개가 덮인다.
            var stallColors = new[] { new Color(0.9f, 0.45f, 0.4f), new Color(0.4f, 0.6f, 0.85f), new Color(0.55f, 0.75f, 0.45f) };
            var goodsColors = new[] { new Color(0.9f, 0.3f, 0.3f), new Color(0.95f, 0.65f, 0.25f), new Color(0.6f, 0.8f, 0.35f) };
            for (int i = 0; i < 3; i++)
            {
                float angle = -38f + i * 38f;
                Vector3 p = Ring(angle, 6.8f);
                Transform stall = new GameObject($"Stall_{i}").transform;
                stall.SetParent(props);
                stall.SetPositionAndRotation(p, Quaternion.Euler(0f, angle + 180f, 0f));
                BoxLocal("Table", stall, new Vector3(0f, 0.45f, 0f), new Vector3(2f, 0.9f, 1f), "Desk", default);
                foreach (var x in new[] { -0.95f, 0.95f })
                    NoCollider(BoxLocal("Post", stall, new Vector3(x, 1.25f, -0.45f), new Vector3(0.08f, 2.5f, 0.08f), "Trunk", default));
                NoCollider(BoxLocal("Canopy", stall, new Vector3(0f, 2.5f, 0f), new Vector3(2.4f, 0.1f, 1.5f), $"Stall_{i}", stallColors[i]));
                for (int g = 0; g < 5; g++)
                    NoCollider(BoxLocal("Goods", stall, new Vector3(-0.7f + g * 0.35f, 0.98f, 0.1f), Vector3.one * 0.22f, $"Goods_{i}", goodsColors[i]));
                var cover = NoCollider(BoxLocal("Cover", stall, new Vector3(0f, 1.0f, 0f), new Vector3(2.05f, 0.25f, 1.05f), "Shutter", default));
                cover.SetActive(false);
                shutters.Add(cover);
                SetStaticRecursive(stall.gameObject, false);
            }

            // 분수 둘레 벤치
            foreach (float angle in new[] { 135f, 225f, -60f })
            {
                Vector3 p = Ring(angle, 4.6f);
                var bench = BoxLocal($"Bench_{angle:0}", props, p + Vector3.up * 0.25f, new Vector3(1.8f, 0.12f, 0.5f), "Desk", default);
                bench.transform.rotation = Quaternion.Euler(0f, angle + 90f, 0f);
            }

            // 꽃집 앞 화단, 식료품점 앞 상자·통
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = Ring(45f, 13.4f) + Quaternion.Euler(0f, 45f + 90f, 0f) * Vector3.forward * (-1.8f + i * 1.2f);
                var pot = Prim(PrimitiveType.Cylinder, $"FlowerPot_{i}", props, p + Vector3.up * 0.2f, new Vector3(0.45f, 0.2f, 0.45f), "Pot", new Color(0.75f, 0.45f, 0.3f));
                var flower = NoCollider(Prim(PrimitiveType.Sphere, "Flower", pot.transform, p + Vector3.up * 0.55f, Vector3.one * 0.4f, i % 2 == 0 ? "Flower" : "FlowerWhite", i % 2 == 0 ? new Color(0.95f, 0.5f, 0.6f) : new Color(0.97f, 0.97f, 0.95f)));
                flower.AddComponent<Sway>();
            }
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = Ring(-45f, 13.2f) + new Vector3(-1.5f + i * 0.9f, 0f, 0.3f * i);
                Prim(i == 1 ? PrimitiveType.Cylinder : PrimitiveType.Cube, $"Crate_{i}", props, p + Vector3.up * 0.35f, new Vector3(0.7f, i == 1 ? 0.35f : 0.7f, 0.7f), "Crate", new Color(0.65f, 0.5f, 0.35f));
            }

            // 서쪽 주택가 빨랫줄 (바람에 흔들린다)
            Vector3 laundry = Ring(-70f, 25f);
            Prim(PrimitiveType.Cylinder, "LaundryPole_A", props, laundry + new Vector3(0f, 0.9f, -2f), new Vector3(0.08f, 0.9f, 0.08f), "Trunk", default);
            Prim(PrimitiveType.Cylinder, "LaundryPole_B", props, laundry + new Vector3(0f, 0.9f, 2f), new Vector3(0.08f, 0.9f, 0.08f), "Trunk", default);
            NoCollider(Prim(PrimitiveType.Cube, "Line", props, laundry + new Vector3(0f, 1.75f, 0f), new Vector3(0.02f, 0.02f, 4f), "LampPole", default));
            var clothColors = new[] { ("ClothPink", new Color(0.95f, 0.7f, 0.75f)), ("ClothBlue", new Color(0.6f, 0.75f, 0.95f)), ("ClothWhite", new Color(0.97f, 0.97f, 0.95f)) };
            for (int i = 0; i < 3; i++)
            {
                var hanger = new GameObject($"Cloth_{i}").transform;
                hanger.SetParent(props);
                hanger.position = laundry + new Vector3(0f, 1.75f, -1.3f + i * 1.3f);
                NoCollider(Prim(PrimitiveType.Cube, "Cloth", hanger, hanger.position + Vector3.down * 0.3f, new Vector3(0.03f, 0.6f, 0.8f), clothColors[i].Item1, clothColors[i].Item2));
                var sway = hanger.gameObject.AddComponent<Sway>();
                SetVector(sway, "axis", Vector3.forward);
            }

            // 광장 가로등 (저녁부터)
            var glows = new List<GameObject>();
            Transform lamps = Group("StreetLamps", village);
            for (int i = 0; i < 6; i++)
                glows.Add(StreetLamp(lamps, Ring(30f + i * 60f, 11.5f)));
            PhaseSet("PhaseSet_StreetLamps", village, data, new[] { TimeOfDay.Evening, TimeOfDay.Night }, glows.ToArray());

            // 마을 게시판 (정류장 옆): 오늘의 소식지
            var board = BoxLocal("NoticeBoard", props, Ring(165f, 13f) + Vector3.up * 1.3f, new Vector3(1.6f, 1.1f, 0.1f), "Cork", default);
            board.transform.rotation = Quaternion.Euler(0f, 165f + 180f, 0f);
            Prim(PrimitiveType.Cylinder, "NoticeBoard_Leg", props, Ring(165f, 13f) + Vector3.up * 0.4f, new Vector3(0.1f, 0.4f, 0.1f), "Trunk", default);
            AddReadable(board, data, data.newsletter, "마을 게시판 보기");
        }

        /// <summary>광장 남쪽 정류장: 아침엔 내리는 곳, 저녁엔 막차를 타는 곳.</summary>
        static void BuildVillageStop(Transform village, PrototypeData data)
        {
            Transform stop = Group("VillageStop", village);
            Vector3 c = Ring(180f, 16f);
            BoxLocal("Shelter_Roof", stop, c + new Vector3(-3f, 2.4f, -0.6f), new Vector3(2.8f, 0.1f, 1.4f), "Roof", default);
            BoxLocal("Shelter_Back", stop, c + new Vector3(-3f, 1.2f, -1.25f), new Vector3(2.8f, 2.4f, 0.08f), "Shelter", new Color(0.85f, 0.88f, 0.82f));
            BoxLocal("Shelter_Bench", stop, c + new Vector3(-3f, 0.4f, -0.95f), new Vector3(2.2f, 0.1f, 0.45f), "Desk", default);

            // 서 있는 버스: 아침엔 "숙소로", 저녁엔 "막차"
            var bus = BoxLocal("Bus", stop, c + new Vector3(3.2f, 1.4f, -1.2f), new Vector3(7f, 2.6f, 2.5f), "Bus", new Color(0.3f, 0.6f, 0.62f));
            bus.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            NoCollider(BoxLocal("Bus_Windows", stop, c + new Vector3(3.2f, 1.9f, 0.07f), new Vector3(6.4f, 0.8f, 0.02f), "WindowDark", default));
            var toHome = new FlagEffect(PrototypeData.AtOutskirts, FlagOperation.Set, 1);
            AddTravel(bus, "버스 타고 숙소로", PrototypeData.SpawnBusStop, data, PrototypeData.TimeIs(TimeOfDay.Morning));
            SetEffects(bus.GetComponent<KindNeighbors.Travel.TravelInteractable>(), "onTravel", toHome);
            AddTravel(bus, "막차 타고 숙소로", PrototypeData.SpawnBusStop, data, PrototypeData.TimeIs(TimeOfDay.Evening));
            SetEffects(bus.GetComponents<KindNeighbors.Travel.TravelInteractable>()[1], "onTravel", toHome);
            SetStaticRecursive(stop.gameObject, false);

            CreateSpawnPoint(PrototypeData.SpawnVillageStop, stop, c + new Vector3(0f, 0f, 1.8f), 0f);
        }

        static void BuildPlayground(Transform village, Vector3 center)
        {
            Transform lot = Group("Playground", village);
            NoCollider(Prim(PrimitiveType.Cylinder, "Lot", lot, center + Vector3.up * 0.01f, new Vector3(9f, 0.02f, 9f), "Sand", new Color(0.88f, 0.8f, 0.62f)));
            BoxLocal("Sandbox", lot, center + new Vector3(2f, 0.15f, 1f), new Vector3(2.4f, 0.3f, 2.4f), "Desk", default);
            foreach (var x in new[] { -1.2f, 1.2f })
                Prim(PrimitiveType.Cylinder, "SwingPost", lot, center + new Vector3(-2.5f + x, 1.1f, 1.5f), new Vector3(0.12f, 1.1f, 0.12f), "Trunk", default);
            NoCollider(Prim(PrimitiveType.Cube, "SwingBar", lot, center + new Vector3(-2.5f, 2.2f, 1.5f), new Vector3(2.6f, 0.1f, 0.1f), "Trunk", default));
            var seat = new GameObject("Swing").transform;
            seat.SetParent(lot);
            seat.position = center + new Vector3(-2.5f, 2.2f, 1.5f);
            NoCollider(Prim(PrimitiveType.Cube, "Seat", seat, seat.position + Vector3.down * 1.5f, new Vector3(0.6f, 0.06f, 0.3f), "Ball", default));
            var sway = seat.gameObject.AddComponent<Sway>();
            SetVector(sway, "axis", Vector3.right);
            SetFloat(sway, "angle", 12f);
            Prim(PrimitiveType.Sphere, "Ball", lot, center + new Vector3(1f, 0.25f, -1.5f), Vector3.one * 0.5f, "Ball", default);
        }

        static void BuildForestRing(Transform parent, float radius)
        {
            Transform forest = Group("ForestRing", parent);
            var rng = new System.Random(11);
            for (int i = 0; i < 48; i++)
            {
                float angle = i * 7.5f + (float)rng.NextDouble() * 4f;
                float r = radius + (float)rng.NextDouble() * 5f;
                Tree(forest, Ring(angle, r), 4f + (float)rng.NextDouble() * 3f, rng);
            }
        }

        static void Tree(Transform parent, Vector3 p, float height, System.Random rng)
        {
            Transform tree = new GameObject("Tree").transform;
            tree.SetParent(parent);
            tree.position = p;
            Prim(PrimitiveType.Cylinder, "Trunk", tree, p + Vector3.up * height * 0.25f, new Vector3(0.5f, height * 0.25f, 0.5f), "Trunk", default);
            Prim(PrimitiveType.Sphere, "Leaves", tree, p + Vector3.up * height * 0.65f, Vector3.one * (2.2f + (float)rng.NextDouble()), "Leaves", default);
        }

        /// <summary>
        /// 배경 주민: 생활하는 사람들. 지나가면 한 마디 하고, 저녁엔 집으로 들어간다.
        /// </summary>
        static void BuildResidents(Transform village, PrototypeData data, Transform bakery, Transform grocery, Transform florist,
            Transform cafe, Transform clinic, Transform tailor, Transform westHouse, Transform northA, Transform northB)
        {
            Transform residents = Group("Residents", village);
            Transform Door(Transform building) => building.Find("Door");

            Resident(residents, data, "꽃집 주인", PrototypeData.BarksFlorist(), florist.TransformPoint(new Vector3(-1.6f, 0f, 3.6f)), florist.eulerAngles.y + 30f,
                1.3f, 0.7f, "ResFlorist", new Color(0.95f, 0.65f, 0.75f), Door(florist));
            Resident(residents, data, "식료품점 주인", PrototypeData.BarksGrocer(), Ring(-38f, 8.2f), -38f, 1.4f, 0.8f,
                "ResGrocer", new Color(0.6f, 0.75f, 0.5f), Door(grocery));
            Resident(residents, data, "수다 떠는 주민", PrototypeData.BarksGossipA(), Ring(225f, 5.6f) + new Vector3(0.6f, 0f, 0f), 225f + 90f + 180f,
                1.25f, 0.75f, "ResGossipA", new Color(0.8f, 0.7f, 0.55f), Door(tailor));
            Resident(residents, data, "수다 떠는 주민", PrototypeData.BarksGossipB(), Ring(225f, 5.6f) + new Vector3(-0.6f, 0f, -0.9f), 225f + 90f,
                1.2f, 0.7f, "ResGossipB", new Color(0.7f, 0.6f, 0.8f), Door(clinic));

            Resident(residents, data, "마당 쓰는 주민", PrototypeData.BarksSweeper(), tailor.TransformPoint(new Vector3(1.5f, 0f, 4f)), 0f,
                1.3f, 0.7f, "ResSweeper", new Color(0.65f, 0.55f, 0.45f), Door(tailor),
                tailor.TransformPoint(new Vector3(-1.5f, 0f, 4.2f)), tailor.TransformPoint(new Vector3(1.8f, 0f, 4.5f)));
            Resident(residents, data, "장보는 주민", PrototypeData.BarksShopper(), Ring(-10f, 9f), 0f,
                1.3f, 0.75f, "ResShopper", new Color(0.85f, 0.55f, 0.45f), Door(westHouse),
                Ring(-10f, 9f), Ring(-38f, 9f), Ring(-60f, 11f), Ring(70f, 10f), Ring(30f, 9f));

            // 술래잡기하는 아이들: 분수 둘레를 뛰어다닌다
            var loop = new[] { Ring(0f, 4f), Ring(90f, 4f), Ring(180f, 4f), Ring(270f, 4f) };
            var loopShifted = new[] { loop[2], loop[3], loop[0], loop[1] };
            var kidA = Resident(residents, data, "아이", PrototypeData.BarksKidA(), loop[0], 90f, 0.85f, 0.55f, "ResKidA", new Color(0.95f, 0.75f, 0.4f), Door(northA), loop);
            var kidB = Resident(residents, data, "아이", PrototypeData.BarksKidB(), loop[2], 270f, 0.8f, 0.5f, "ResKidB", new Color(0.6f, 0.85f, 0.75f), Door(northB), loopShifted);
            foreach (var kid in new[] { kidA, kidB })
            {
                SetFloat(kid, "walkSpeed", 2.4f);
                SetFloat(kid, "waitAtPoint", 0.2f);
            }

            // 빵집 앞 고양이
            var cat = Resident(residents, data, "고양이", PrototypeData.BarksCat(), bakery.TransformPoint(new Vector3(2.2f, 0f, 4f)), 0f,
                0.35f, 0.45f, "ResCat", new Color(0.35f, 0.33f, 0.32f), Door(bakery),
                bakery.TransformPoint(new Vector3(2.2f, 0f, 4f)), bakery.TransformPoint(new Vector3(-2.5f, 0f, 5f)));
            SetFloat(cat, "walkSpeed", 0.6f);
            SetFloat(cat, "waitAtPoint", 6f);
            SetFloat(cat, "barkRadius", 2f);

            var set = PhaseSet("PhaseSet_Residents", village, data, new[] { TimeOfDay.Morning, TimeOfDay.Evening }, residents.gameObject);
            SetInt(set, "fromDay", 1);
        }

        static AmbientResident Resident(Transform parent, PrototypeData data, string speaker, ConditionalText[] barks, Vector3 position, float yaw,
            float height, float width, string matKey, Color color, Transform homeDoor, params Vector3[] waypoints)
        {
            Transform root = new GameObject($"Resident_{matKey}").transform;
            root.SetParent(parent);
            root.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            Transform body = Group("Body", root);
            var capsule = Prim(PrimitiveType.Capsule, "Capsule", body, position + Vector3.up * (height / 2f), new Vector3(width, height / 2f, width), matKey, color);
            capsule.transform.rotation = root.rotation;
            var face = NoCollider(Prim(PrimitiveType.Cube, "Face", body, position + Vector3.up * (height * 0.72f) + root.forward * (width / 2f),
                new Vector3(width * 0.4f, Mathf.Min(0.12f, height * 0.1f), 0.08f), "Face", default));
            face.transform.rotation = root.rotation;

            Transform waypointRoot = waypoints.Length > 0 ? Group($"Waypoints_{matKey}", parent) : null;
            var points = new Transform[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                points[i] = new GameObject($"P{i}").transform;
                points[i].SetParent(waypointRoot);
                points[i].position = waypoints[i];
            }

            var resident = root.gameObject.AddComponent<AmbientResident>();
            SetRef(resident, "flags", data.flags);
            SetRef(resident, "phaseChanged", data.phaseChanged);
            SetRef(resident, "subtitle", data.subtitle);
            SetString(resident, "speakerName", speaker);
            SetConditionalTexts(resident, "barks", barks);
            SetArray(resident, "waypoints", points);
            SetRef(resident, "homeDoor", homeDoor);
            SetRef(resident, "body", body.gameObject);
            return resident;
        }

        // ================= 외곽 종점 =================

        /// <summary>
        /// 숲 경계의 종점: 숙소와 불 꺼진 솜의 집, 길 건너 정류장. 밤의 것들이 지나가는 길목이다.
        /// </summary>
        /// <returns>숙소 문 (밤으로 넘기기 / 낮에 들어가기)</returns>
        static GameObject BuildOutskirts(PrototypeData data, DeliveryManager deliveryManager)
        {
            Vector3 o = OutskirtsOrigin;
            Transform area = new GameObject("Outskirts").transform;
            area.position = o;

            Box("Ground", area, o + new Vector3(0f, -0.5f, 0f), new Vector3(70f, 1f, 60f), "GrassDark", new Color(0.45f, 0.6f, 0.4f));
            Box("Road", area, o + new Vector3(-5f, 0.01f, 0f), new Vector3(60f, 0.02f, 4f), "Path", default);

            Transform home = House("House_Player", area, o + new Vector3(0f, 0f, -8f), 0f, new Vector3(5f, 3.5f, 5f), "HousePlayer", new Color(0.95f, 0.95f, 0.88f));
            House("House_Som (Dark)", area, o + new Vector3(-8f, 0f, -8f), 0f, new Vector3(5f, 3.5f, 5f), "HouseSom", new Color(0.35f, 0.33f, 0.35f), lightsOn: false);
            CreateHomeMailbox(o + new Vector3(1.9f, 0f, -4.9f), data);
            CreateSpawnPoint(PrototypeData.SpawnOutsideDoor, area, o + new Vector3(0f, 0f, -4.5f), 0f);

            // 길 건너 정류장: 버스 타고 마을로. 가로등은 저녁부터.
            Transform stop = Group("BusStop", area);
            // BoxLocal은 부모 기준 좌표다 (stop은 외곽 원점에 있다)
            BoxLocal("Shelter_Roof", stop, new Vector3(2.5f, 2.4f, 4.6f), new Vector3(2.8f, 0.1f, 1.4f), "Roof", default);
            var shelterBack = BoxLocal("Shelter_Back", stop, new Vector3(2.5f, 1.2f, 5.25f), new Vector3(2.8f, 2.4f, 0.08f), "Shelter", default);
            BoxLocal("Shelter_Bench", stop, new Vector3(2.5f, 0.4f, 4.95f), new Vector3(2.2f, 0.1f, 0.45f), "Desk", default);
            Prim(PrimitiveType.Cylinder, "Sign_Pole", stop, o + new Vector3(0.6f, 1.2f, 3.4f), new Vector3(0.08f, 1.2f, 0.08f), "LampPole", default);
            NoCollider(Prim(PrimitiveType.Cylinder, "Sign", stop, o + new Vector3(0.6f, 2.45f, 3.4f), new Vector3(0.6f, 0.03f, 0.6f), "MailboxHome", default))
                .transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            AddTravel(shelterBack, "버스 타고 마을로", PrototypeData.SpawnVillageStop, data,
                PrototypeData.TimeIsNot(TimeOfDay.Night), PrototypeData.DayAtLeast(1));
            SetEffects(shelterBack.GetComponent<KindNeighbors.Travel.TravelInteractable>(), "onTravel",
                new FlagEffect(PrototypeData.AtOutskirts, FlagOperation.Set, 0));
            var glow = StreetLamp(stop, o + new Vector3(4.4f, 0f, 3.4f));
            PhaseSet("PhaseSet_StopLamp", area, data, new[] { TimeOfDay.Evening, TimeOfDay.Night }, glow);
            CreateSpawnPoint(PrototypeData.SpawnBusStop, stop, o + new Vector3(1.5f, 0f, 2.8f), 180f);

            // 숲: 집 뒤, 길 건너, 길 서쪽 끝. 길은 숲으로 사라진다.
            Transform forest = Group("Forest", area);
            var rng = new System.Random(5);
            for (int i = 0; i < 70; i++)
            {
                float x = -32f + (float)rng.NextDouble() * 62f;
                float z = rng.NextDouble() < 0.5 ? -14f - (float)rng.NextDouble() * 14f : 9f + (float)rng.NextDouble() * 18f;
                Tree(forest, o + new Vector3(x, 0f, z), 4.5f + (float)rng.NextDouble() * 3.5f, rng);
            }
            for (int i = 0; i < 14; i++)
                Tree(forest, o + new Vector3(-28f - (float)rng.NextDouble() * 6f, 0f, -6f + i * 0.9f), 5f + (float)rng.NextDouble() * 3f, rng);
            Anchor("ForestDropSpot", area, new Vector3(-24f, 0f, 0f)); // DAY 3 특별 배달

            // 프롤로그: 정류장에 마중 나온 빵집 주인
            Transform npcs = Group("NPCs_Prologue", area);
            CreateNpc("NPC_Baker_BusStop", npcs, o + new Vector3(1.2f, 0f, 0.6f), Quaternion.Euler(0f, 0f, 0f), 1.5f, 0.8f,
                "NpcBakery", default, data.bakerBusStopDialogue, data, deliveryManager, null);
            var prologueSet = PhaseSet("PhaseSet_Prologue", area, data, new[] { TimeOfDay.Evening }, npcs.gameObject);
            SetInt(prologueSet, "fromDay", 0);
            SetInt(prologueSet, "toDay", 0);

            var busLeft = new GameObject("Subtitle_BusLeft").AddComponent<ConditionalSubtitle>();
            busLeft.transform.SetParent(stop, false);
            SetRef(busLeft, "flags", data.flags);
            SetRef(busLeft, "subtitle", data.subtitle);
            SetConditions(busLeft, "conditions", PrototypeData.DayIs(0), PrototypeData.Is(FlagKeys.HasName));
            SetString(busLeft, "text", "막차가 떠났다. 종점에는 집 두 채뿐이다. 하나는 불이 꺼져 있다.");
            SetString(busLeft, "onceFlag", PrototypeData.PrologueSubtitleShown);

            SetStaticRecursive(stop.gameObject, false);
            return home.Find("Door").gameObject;
        }

        // ================= 헬퍼 =================

        static Vector3 Ring(float angleDeg, float radius)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad) * radius, 0f, Mathf.Cos(rad) * radius);
        }

        static void PathStrip(Transform parent, Vector3 from, Vector3 to, float width)
        {
            Vector3 mid = (from + to) / 2f;
            var strip = NoCollider(Prim(PrimitiveType.Cube, "Path", parent, mid + Vector3.up * 0.012f, new Vector3(width, 0.02f, Vector3.Distance(from, to)), "Path", default));
            strip.transform.rotation = Quaternion.LookRotation(to - from);
        }

        static GameObject Smoke(Transform parent, Vector3 position)
        {
            var go = new GameObject("Smoke");
            go.transform.SetParent(parent, true);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(-90f, 0f, 0f));

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 4.5f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startColor = new Color(0.92f, 0.92f, 0.94f, 0.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 30;
            var emission = ps.emission;
            emission.rateOverTime = 2.5f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.1f;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.8f));
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
            return go;
        }

        static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetVector(Object target, string field, Vector3 value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).vector3Value = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetEffects(Object target, string field, params FlagEffect[] effects)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = effects.Length;
            for (int i = 0; i < effects.Length; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("key").stringValue = effects[i].key;
                element.FindPropertyRelative("operation").enumValueIndex = (int)effects[i].operation;
                element.FindPropertyRelative("value").intValue = effects[i].value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetConditionalTexts(Object target, string field, ConditionalText[] lines)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("text").stringValue = lines[i].text;
                var conditions = element.FindPropertyRelative("conditions");
                var source = lines[i].conditions ?? System.Array.Empty<FlagCondition>();
                conditions.arraySize = source.Length;
                for (int c = 0; c < source.Length; c++)
                {
                    var condition = conditions.GetArrayElementAtIndex(c);
                    condition.FindPropertyRelative("key").stringValue = source[c].key;
                    condition.FindPropertyRelative("comparison").enumValueIndex = (int)source[c].comparison;
                    condition.FindPropertyRelative("value").intValue = source[c].value;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
