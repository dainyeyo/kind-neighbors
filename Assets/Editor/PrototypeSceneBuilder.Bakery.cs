using KindNeighbors.Core.Flags;
using KindNeighbors.Delivery;
using KindNeighbors.Flow;
using KindNeighbors.World;
using UnityEngine;

namespace KindNeighbors.EditorTools
{
    /// <summary>빵집 내부(일터)와 프롤로그(버스 정류장).</summary>
    public static partial class PrototypeSceneBuilder
    {
        // 빵집 내부도 집처럼 마을과 떨어진 별도 공간에 두고 문에서 전환한다
        static readonly Vector3 BakeryOrigin = new(0f, 0f, -300f);

        static readonly Color BakeryWall = new(0.95f, 0.88f, 0.76f);
        static readonly Color Metal = new(0.55f, 0.58f, 0.6f);

        static void BuildBakery(PrototypeData data, DeliveryManager deliveryManager)
        {
            Transform shop = new GameObject("Bakery_Interior").transform;
            shop.position = BakeryOrigin;

            // 껍데기: 10 x 8 x 3.2
            BoxLocal("Floor", shop, new Vector3(0f, -0.1f, 0f), new Vector3(10.4f, 0.2f, 8.4f), "BakeryFloor", new Color(0.78f, 0.66f, 0.55f));
            BoxLocal("Ceiling", shop, new Vector3(0f, 3.3f, 0f), new Vector3(10.4f, 0.2f, 8.4f), "BakeryWall", BakeryWall);
            BoxLocal("Wall_Left", shop, new Vector3(-5.1f, 1.6f, 0f), new Vector3(0.2f, 3.2f, 8.4f), "BakeryWall", default);
            BoxLocal("Wall_Right", shop, new Vector3(5.1f, 1.6f, 0f), new Vector3(0.2f, 3.2f, 8.4f), "BakeryWall", default);
            BoxLocal("Wall_Front", shop, new Vector3(0f, 1.6f, -4.1f), new Vector3(10.4f, 3.2f, 0.2f), "BakeryWall", default);
            BoxLocal("Wall_Back", shop, new Vector3(0f, 1.6f, 4.1f), new Vector3(10.4f, 3.2f, 0.2f), "BakeryWall", default);

            var door = BoxLocal("Door_Inside", shop, new Vector3(0f, 1f, -3.97f), new Vector3(1.2f, 2f, 0.08f), "Door", default);
            AddTravel(door, "나가기", PrototypeData.SpawnBakeryOutside, data);
            CreateSpawnPoint(PrototypeData.SpawnBakeryInside, shop, BakeryOrigin + new Vector3(0f, 0f, -3f), 0f);

            // 매장: 카운터와 빵집 주인 (아침 조회, 주문 배정, 저녁 정산, 그림 편지 받기)
            BoxLocal("Counter", shop, new Vector3(0f, 0.5f, 0.6f), new Vector3(4f, 1f, 0.8f), "Counter", new Color(0.62f, 0.45f, 0.3f));
            BoxLocal("BreadShelf", shop, new Vector3(-1.2f, 1.3f, 3.75f), new Vector3(3.4f, 0.08f, 0.6f), "Desk", default);
            for (int i = 0; i < 5; i++)
                NoCollider(BoxLocal($"Bread_{i}", shop, new Vector3(-2.5f + i * 0.65f, 1.43f, 3.75f), new Vector3(0.45f, 0.18f, 0.3f), "GiftBread", default));
            CreateNpc("NPC_BakeryOwner", shop, BakeryOrigin + new Vector3(0f, 0f, 1.6f), Quaternion.Euler(0f, 180f, 0f), 1.5f, 0.8f,
                "NpcBakery", new Color(0.9f, 0.55f, 0.4f), data.bakerDialogue, data, deliveryManager, PrototypeData.BakerSpot);

            // 게시판 (왼쪽 벽): 그날 배달 목록
            var board = BoxLocal("OrderBoard", shop, new Vector3(-4.97f, 1.6f, -2f), new Vector3(0.08f, 1f, 1.5f), "Cork", new Color(0.7f, 0.55f, 0.38f));
            for (int i = 0; i < 3; i++)
                NoCollider(BoxLocal($"Note_{i}", shop, new Vector3(-4.92f, 1.75f - i * 0.28f, -2.4f + i * 0.35f), new Vector3(0.02f, 0.22f, 0.3f), "Paper", default));
            AddReadable(board, data, data.orderBoard, "게시판 보기");

            // 배달원 수칙 (오른쪽 벽): 2·3번 위의 전단은 그 규칙을 들은 날부터 떨어져 있다
            var poster = BoxLocal("RulesPoster", shop, new Vector3(4.98f, 1.7f, -2.3f), new Vector3(0.05f, 1.1f, 0.8f), "Paper", default);
            AddReadable(poster, data, data.rulesPoster, "수칙 읽기");
            var flyer2 = NoCollider(BoxLocal("Flyer_OverRule2", shop, new Vector3(4.94f, 1.72f, -2.25f), new Vector3(0.02f, 0.2f, 0.6f), "ClothPink", default));
            var flyer3 = NoCollider(BoxLocal("Flyer_OverRule3", shop, new Vector3(4.94f, 1.45f, -2.35f), new Vector3(0.02f, 0.2f, 0.6f), "YellowThread", default));
            Toggle("Toggle_FlyerRule2", shop, data, flyer2, PrototypeData.Not(PrototypeData.HeardRule2));
            Toggle("Toggle_FlyerRule3", shop, data, flyer3, PrototypeData.Not(PrototypeData.HeardRule3));

            // 휴게실 (오른쪽 안쪽): 탁자, 동료 둘, 사물함 셋
            BoxLocal("BreakTable", shop, new Vector3(3.2f, 0.4f, 1.6f), new Vector3(1.6f, 0.8f, 1f), "Desk", default);
            CreateNpc("NPC_Senior", shop, BakeryOrigin + new Vector3(2.1f, 0f, 0.8f), Quaternion.Euler(0f, 210f, 0f), 1.4f, 0.7f,
                "NpcSenior", new Color(0.95f, 0.7f, 0.62f), data.seniorDialogue, data, deliveryManager, null);
            CreateNpc("NPC_Quiet", shop, BakeryOrigin + new Vector3(4.3f, 0f, 0.4f), Quaternion.Euler(0f, 250f, 0f), 1.55f, 0.65f,
                "NpcQuiet", new Color(0.5f, 0.55f, 0.6f), data.quietDialogue, data, deliveryManager, null);
            var quiet = shop.Find("NPC_Quiet").gameObject.AddComponent<ScaleByDay>();
            SetRef(quiet, "flags", data.flags);
            SetRef(quiet, "phaseChanged", data.phaseChanged);

            for (int i = 0; i < 3; i++)
            {
                var locker = BoxLocal($"Locker_{i}", shop, new Vector3(2.4f + i * 0.9f, 1f, 3.7f), new Vector3(0.8f, 2f, 0.6f), "Locker", Metal);
                if (i != 1)
                    continue;
                // 가운데 사물함: 이름표가 테이프로 가려져 있다
                NoCollider(BoxLocal("NameTape", shop, new Vector3(2.4f + 0.9f, 1.75f, 3.39f), new Vector3(0.35f, 0.08f, 0.02f), "Paper", default));
                AddReadable(locker, data, data.somLocker, "사물함 살펴보기");
            }

            // 창고 (왼쪽 안쪽): 숲 경계 도시락 바구니. 날마다 하나씩 줄어든다.
            for (int i = 0; i < 3; i++)
            {
                var basket = BoxLocal($"LunchBasket_{i}", shop, new Vector3(-4.3f + i * 0.6f, 0.2f, 3.3f), new Vector3(0.5f, 0.4f, 0.4f), "Basket", new Color(0.72f, 0.55f, 0.32f));
                Toggle($"Toggle_LunchBasket_{i}", shop, data, basket, new FlagCondition(FlagKeys.Day, Comparison.LessOrEqual, 3 - i));
            }

            PointLight("Light_Shop", shop, BakeryOrigin + new Vector3(-1.5f, 2.9f, 0.5f), new Color(1f, 0.88f, 0.7f), 1.1f, 9f, LightShadows.Soft);
            PointLight("Light_BreakRoom", shop, BakeryOrigin + new Vector3(3f, 2.9f, 1.5f), new Color(1f, 0.85f, 0.65f), 0.9f, 7f, LightShadows.None);
        }

        /// <summary>
        /// 프롤로그: 해 질 녘 북쪽 끝 버스 정류장. 남쪽 숙소까지 걸어가는 길에 불 꺼진 솜의 집을 지나친다.
        /// 빵집 주인은 정류장에 마중 나와 있고, 이야기를 나누면 숙소 앞으로 먼저 가 있다.
        /// </summary>
        static void BuildPrologue(Transform map, PrototypeData data, DeliveryManager deliveryManager)
        {
            Transform stop = Group("BusStop", map);
            // 플레이어는 남쪽을 보고 시작하고 어깨 카메라는 오른쪽(-x)에 있으므로, 정류장은 왼쪽(+x)에 둔다
            BoxLocal("Shelter_Roof", stop, new Vector3(3f, 2.4f, 27.6f), new Vector3(2.6f, 0.1f, 1.3f), "Roof", default);
            BoxLocal("Shelter_Back", stop, new Vector3(3f, 1.2f, 28.2f), new Vector3(2.6f, 2.4f, 0.08f), "LampPole", default);
            BoxLocal("Shelter_Bench", stop, new Vector3(3f, 0.4f, 27.9f), new Vector3(2f, 0.1f, 0.45f), "Desk", default);
            Prim(PrimitiveType.Cylinder, "Sign_Pole", stop, new Vector3(1.3f, 1.2f, 26.8f), new Vector3(0.08f, 1.2f, 0.08f), "LampPole", default);
            NoCollider(Prim(PrimitiveType.Cylinder, "Sign", stop, new Vector3(1.3f, 2.45f, 26.8f), new Vector3(0.6f, 0.03f, 0.6f), "MailboxHome", default)).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            CreateSpawnPoint(PrototypeData.SpawnBusStop, stop, new Vector3(0f, 0f, 27f), 180f);

            Transform npcs = Group("NPCs_Prologue", map);
            CreateNpc("NPC_Baker_BusStop", npcs, new Vector3(0f, 0f, 25f), Quaternion.Euler(0f, 0f, 0f), 1.5f, 0.8f,
                "NpcBakery", default, data.bakerBusStopDialogue, data, deliveryManager, null);
            CreateNpc("NPC_Baker_Home", npcs, new Vector3(-1.6f, 0f, -14.2f), Quaternion.Euler(0f, 0f, 0f), 1.5f, 0.8f,
                "NpcBakery", default, data.bakerHomeDialogue, data, deliveryManager, null);
            Toggle("Toggle_BakerAtHome", npcs, data, npcs.Find("NPC_Baker_Home").gameObject, PrototypeData.Is(PrototypeData.MetBaker));

            var prologueSet = PhaseSet("PhaseSet_Prologue", map, data, new[] { TimeOfDay.Evening }, npcs.gameObject);
            SetInt(prologueSet, "fromDay", 0);
            SetInt(prologueSet, "toDay", 0);

            var busLeft = new GameObject("Subtitle_BusLeft").AddComponent<ConditionalSubtitle>();
            busLeft.transform.SetParent(stop, false);
            SetRef(busLeft, "flags", data.flags);
            SetRef(busLeft, "subtitle", data.subtitle);
            SetConditions(busLeft, "conditions", PrototypeData.DayIs(0), PrototypeData.Is(FlagKeys.HasName));
            SetString(busLeft, "text", "막차가 떠났다. 손에는 전단 한 장과 편도 버스표.");
            SetString(busLeft, "onceFlag", PrototypeData.PrologueSubtitleShown);
        }
    }
}
