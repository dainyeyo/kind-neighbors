using System;
using System.Collections.Generic;
using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using KindNeighbors.Delivery;
using KindNeighbors.Dialogue;
using KindNeighbors.Flow;
using KindNeighbors.Home;
using KindNeighbors.Travel;
using UnityEditor;
using UnityEngine;

namespace KindNeighbors.EditorTools
{
    /// <summary>
    /// 프로토타입에 필요한 데이터 에셋(이벤트 채널, 플래그, 시간대, 조명, 주문, 대화, 글)을 만든다.
    /// 이미 있는 에셋은 건드리지 않으므로 인스펙터에서 고친 내용은 유지된다.
    /// 처음 값으로 되돌리고 싶으면 해당 에셋을 지우고 다시 빌드하면 된다.
    /// </summary>
    public class PrototypeData
    {
        const string Root = "Assets/Data";

        // 목적지 id (DeliveryOrder.destinationId ↔ DeliveryDestination.destinationId)
        public const string GrandmaMailbox = "grandma_house";
        public const string ChildSpot = "child";
        public const string BakerSpot = "baker";

        // SpawnPoint id
        public const string SpawnBed = "home_bed";
        public const string SpawnInsideDoor = "home_inside_door";
        public const string SpawnOutsideDoor = "home_outside_door";
        /// <summary>외곽 종점 정류장 (숙소 앞)</summary>
        public const string SpawnBusStop = "bus_stop";
        /// <summary>마을 광장의 정류장</summary>
        public const string SpawnVillageStop = "village_stop";
        public const string SpawnBakeryInside = "bakery_inside";
        public const string SpawnBakeryOutside = "bakery_outside";

        // 데이터에서 쓰는 플래그 키 (코드가 직접 읽는 키는 FlagKeys에 있다)
        public const string HasBag = "has.bag";
        public const string GiftBread = "gift.bread";
        public const string GiftYarn = "gift.yarn";
        public const string GiftDrawing = "gift.drawing";
        public const string HeardRule1 = "heard.rule1";
        public const string HeardRule2 = "heard.rule2";
        public const string SilhouetteDone = "event.d1_silhouette";
        public const string SilhouetteSeen = "seen.d1_silhouette";
        public const string KnockActive = "knock.d2";
        public const string KnockDone = "event.d2_knock";
        public const string WindowOpened = "window.opened.d2";
        public const string WindowIgnored = "window.ignored.d2";
        public const string HeardRule3 = "heard.rule3";
        public const string MetBaker = "met.baker";
        public const string Briefed = "briefed.d1";
        public const string PrologueSubtitleShown = "seen.prologue_bus";
        /// <summary>1이면 외곽 종점(숙소 쪽), 0이면 마을. 버스를 탈 때와 시간대가 시작될 때 갱신된다.</summary>
        public const string AtOutskirts = "at.outskirts";
        public static string CameHome(int day) => $"home.d{day}";
        public static string Settled(int day) => $"settled.d{day}";

        public GameFlags flags;
        public PhaseEventChannel phaseChanged;
        public VoidEventChannel advanceRequested;
        public BoolEventChannel inputLock;
        public DialogueGraphEventChannel dialogueRequested;
        public DeliveryOrderEventChannel orderRequested;
        public DeliveryOrderEventChannel deliverRequested;
        public DeliveryOrderEventChannel orderAccepted;
        public DeliveryOrderEventChannel orderCompleted;
        public TravelEventChannel travelRequested;
        public DocumentEventChannel documentRequested;
        public StringEventChannel subtitle;
        public GameFlowConfig flow;
        public DeliveryOrder breadOrder;
        public DeliveryOrder hatOrder;
        public DeliveryOrder letterOrder;
        public DialogueGraph bakerDialogue;
        public DialogueGraph grandmaDialogue;
        public DialogueGraph childDialogue;
        public DialogueGraph windowDialogue;
        public DialogueGraph bakerBusStopDialogue;
        public DialogueGraph seniorDialogue;
        public DialogueGraph quietDialogue;
        public DocumentAsset newsletter;
        public DocumentAsset myJournal;
        public DocumentAsset somJournal;
        public DocumentAsset suitcase;
        public DocumentAsset orderBoard;
        public DocumentAsset rulesPoster;
        public DocumentAsset somLocker;
        public ObjectiveList objectives;

        public static PrototypeData LoadOrCreate()
        {
            var d = new PrototypeData();

            d.flags = Asset<GameFlags>($"{Root}/GameFlags.asset");

            d.phaseChanged = Asset<PhaseEventChannel>($"{Root}/Events/Evt_PhaseChanged.asset");
            d.advanceRequested = Asset<VoidEventChannel>($"{Root}/Events/Evt_PhaseAdvanceRequested.asset");
            d.inputLock = Asset<BoolEventChannel>($"{Root}/Events/Evt_InputLock.asset");
            d.dialogueRequested = Asset<DialogueGraphEventChannel>($"{Root}/Events/Evt_DialogueRequested.asset");
            d.orderRequested = Asset<DeliveryOrderEventChannel>($"{Root}/Events/Evt_OrderRequested.asset");
            d.deliverRequested = Asset<DeliveryOrderEventChannel>($"{Root}/Events/Evt_DeliverRequested.asset");
            d.orderAccepted = Asset<DeliveryOrderEventChannel>($"{Root}/Events/Evt_OrderAccepted.asset");
            d.orderCompleted = Asset<DeliveryOrderEventChannel>($"{Root}/Events/Evt_OrderCompleted.asset");
            d.travelRequested = Asset<TravelEventChannel>($"{Root}/Events/Evt_TravelRequested.asset");
            d.documentRequested = Asset<DocumentEventChannel>($"{Root}/Events/Evt_DocumentRequested.asset");
            d.subtitle = Asset<StringEventChannel>($"{Root}/Events/Evt_Subtitle.asset");

            // DAY 1 배달: 빵집 주인 → 할머니 → 아이 → 빵집 주인 으로 이어진다
            d.breadOrder = Order("Order_D1_Bread", "빵 바구니", "빵집 주인", "이웃 할머니", GrandmaMailbox);
            d.hatOrder = Order("Order_D1_Hat", "털모자", "이웃 할머니", "아이", ChildSpot);
            d.letterOrder = Order("Order_D1_Letter", "그림 편지", "아이", "빵집 주인", BakerSpot);

            d.flow = CreateFlow(d);
            d.bakerDialogue = CreateBakerDialogue(d);
            d.grandmaDialogue = CreateGrandmaDialogue(d);
            d.childDialogue = CreateChildDialogue(d);
            d.windowDialogue = CreateWindowDialogue();
            d.bakerBusStopDialogue = CreateBakerBusStopDialogue();
            d.seniorDialogue = CreateSeniorDialogue();
            d.quietDialogue = CreateQuietDialogue();
            d.newsletter = CreateNewsletter();
            d.myJournal = CreateMyJournal(d);
            d.somJournal = CreateSomJournal();
            d.suitcase = CreateSuitcase();
            d.orderBoard = CreateOrderBoard(d);
            d.rulesPoster = CreateRulesPoster();
            d.somLocker = CreateSomLocker();
            d.objectives = CreateObjectives(d);

            AssetDatabase.SaveAssets();
            return d;
        }

        static DeliveryOrder Order(string assetName, string item, string client, string recipient, string destinationId) =>
            Asset<DeliveryOrder>($"{Root}/Orders/{assetName}.asset", o =>
            {
                o.itemName = item;
                o.clientName = client;
                o.recipientName = recipient;
                o.destinationId = destinationId;
            });

        // ---------- 시간대 ----------

        static GameFlowConfig CreateFlow(PrototypeData d)
        {
            var day = Asset<LightingPreset>($"{Root}/Flow/Lighting/Light_Day.asset", p =>
            {
                p.sunColor = new Color(1f, 0.93f, 0.8f);
                p.sunIntensity = 0.9f;
                p.sunRotation = new Vector3(45f, -35f, 0f);
                p.ambientSky = new Color(0.55f, 0.6f, 0.7f);
                p.ambientEquator = new Color(0.5f, 0.5f, 0.47f);
                p.ambientGround = new Color(0.3f, 0.28f, 0.25f);
                p.useSkybox = true;
            });
            var evening = Asset<LightingPreset>($"{Root}/Flow/Lighting/Light_Evening.asset", p =>
            {
                p.sunColor = new Color(1f, 0.58f, 0.38f);
                p.sunIntensity = 0.75f;
                p.sunRotation = new Vector3(10f, -70f, 0f);
                p.ambientSky = new Color(0.5f, 0.4f, 0.45f);
                p.ambientEquator = new Color(0.45f, 0.33f, 0.3f);
                p.ambientGround = new Color(0.2f, 0.15f, 0.13f);
                p.fog = true;
                p.fogColor = new Color(0.75f, 0.5f, 0.4f);
                p.fogDensity = 0.012f;
                p.useSkybox = true;
            });
            var night = Asset<LightingPreset>($"{Root}/Flow/Lighting/Light_Night.asset", p =>
            {
                p.sunColor = new Color(0.45f, 0.55f, 0.85f);
                p.sunIntensity = 0.18f;
                p.sunRotation = new Vector3(35f, 150f, 0f);
                p.ambientSky = new Color(0.08f, 0.1f, 0.18f);
                p.ambientEquator = new Color(0.06f, 0.07f, 0.11f);
                p.ambientGround = new Color(0.03f, 0.03f, 0.04f);
                p.fog = true;
                p.fogColor = new Color(0.04f, 0.05f, 0.09f);
                p.fogDensity = 0.035f;
                p.useSkybox = false;
                p.backgroundColor = new Color(0.03f, 0.04f, 0.08f);
            });

            var phases = new List<PhaseDefinition>
            {
                // 프롤로그: 해 질 녘 막차로 도착. 빵집 주인을 만난 뒤 숙소 문에서 쉬면 DAY 1 아침이 된다.
                Asset<PhaseDefinition>($"{Root}/Flow/Phases/Phase_D0_Prologue.asset", p =>
                {
                    p.phase = new GamePhase(0, TimeOfDay.Evening);
                    p.lighting = evening;
                    p.spawnOnEnter = SpawnBusStop;
                    p.completeWhen = new[] { Is(MetBaker) };
                    p.advancePrompt = "숙소에 들어가 쉬기";
                    p.onEnter = new[] { new FlagEffect(AtOutskirts, FlagOperation.Set, 1) };
                }),
            };
            for (int dayNumber = 1; dayNumber <= 3; dayNumber++)
            {
                int n = dayNumber;
                phases.Add(Asset<PhaseDefinition>($"{Root}/Flow/Phases/Phase_D{n}_1_Morning.asset", p =>
                {
                    p.phase = new GamePhase(n, TimeOfDay.Morning);
                    p.lighting = day;
                    p.spawnOnEnter = SpawnBed; // 매일 아침 집(외곽)에서 일어난다
                    p.onEnter = new[] { new FlagEffect(AtOutskirts, FlagOperation.Set, 1) };
                    if (n == 1)
                    {
                        // 배달 3건을 모두 끝내면 자동으로 저녁이 된다
                        p.completeWhen = new[] { Delivered(d.breadOrder), Delivered(d.hatOrder), Delivered(d.letterOrder) };
                        p.advancePrompt = string.Empty;
                    }
                    else
                    {
                        // Day 2, 3 배달은 아직 정하지 않았다
                        p.advancePrompt = "(임시) 저녁까지 일하기";
                    }
                }));
                phases.Add(Asset<PhaseDefinition>($"{Root}/Flow/Phases/Phase_D{n}_2_Evening.asset", p =>
                {
                    p.phase = new GamePhase(n, TimeOfDay.Evening);
                    p.lighting = evening;
                    p.advancePrompt = "집에 들어가기";
                    // 빵집에서 정산을 받아야 집에 들어갈 수 있다 (DAY 2, 3은 배달 내용이 정해지면 추가)
                    if (n == 1)
                        p.completeWhen = new[] { Is(Settled(1)) };
                }));
                phases.Add(Asset<PhaseDefinition>($"{Root}/Flow/Phases/Phase_D{n}_3_Night.asset", p =>
                {
                    p.phase = new GamePhase(n, TimeOfDay.Night);
                    p.lighting = night;
                    p.advancePrompt = "잠자기";
                    p.spawnOnEnter = SpawnInsideDoor;
                    // 밤마다 커튼은 걷히고 불은 켜지고 문은 열린 채로 시작한다
                    p.onEnter = new[]
                    {
                        new FlagEffect(FlagKeys.CurtainClosed, FlagOperation.Set, 0),
                        new FlagEffect(FlagKeys.LampOff, FlagOperation.Set, 0),
                        new FlagEffect(FlagKeys.DoorLocked, FlagOperation.Set, 0),
                        new FlagEffect(CameHome(n), FlagOperation.Set, 1),
                        new FlagEffect(AtOutskirts, FlagOperation.Set, 1),
                    };
                    // 그날 밤 이벤트가 끝나야 잠들 수 있다
                    if (n == 1)
                        p.completeWhen = new[] { Is(SilhouetteDone) };
                    else if (n == 2)
                        p.completeWhen = new[] { Is(KnockDone) };
                }));
            }

            return Asset<GameFlowConfig>($"{Root}/Flow/GameFlow.asset", f => f.phases = phases.ToArray());
        }

        // ---------- 대화 ----------
        // 시작 지점(entries)은 위에서부터 검사하므로, 진행이 더 많이 된 상태를 위에 둔다.

        static DialogueGraph CreateBakerDialogue(PrototypeData d)
        {
            var giveBread = RequestOrder(d, d.breadOrder);
            var takeLetter = DeliverOrder(d, d.letterOrder);
            var heardRule1 = Set(HeardRule1);

            // 빵집 카운터의 주인: 아침 조회(동료 소개, 수칙 1) → 가방 받은 뒤 첫 주문 → 저녁 정산
            return Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Baker.asset", g =>
            {
                g.speakerName = "빵집 주인";
                g.entries = new List<DialogueEntry>
                {
                    Entry("d3", DayIs(3)),
                    Entry("d2", DayIs(2)),
                    Entry("done", Is(Settled(1))),
                    Entry("settle", TimeIs(TimeOfDay.Evening)),
                    Entry("letter_receive", Accepted(d.letterOrder), Not(FlagKeys.Delivered(d.letterOrder.Id))),
                    Entry("after_bread", Delivered(d.breadOrder)),
                    Entry("remind_bread", Accepted(d.breadOrder)),
                    Entry("ask", Is(HasBag)),
                    Entry("need_bag", Is(Briefed)),
                    Entry("greet"),
                };
                g.nodes = new List<DialogueNode>
                {
                    // 아침 조회
                    Node("greet", "왔구나, {player}! 잘 잤니? 숙소는 좀 춥지 않았고?", "greet2"),
                    Node("greet2", "여기가 우리 빵집이자 배달 사무소야~ 저기 휴게실에 있는 둘이 네 동료들이고.", "greet3"),
                    Node("greet3", "보리한테 가방부터 받아 오렴. 그리고… 해 지기 전엔 꼭 집에 들어가렴~", null,
                        effects: new[] { new FlagEffect(Briefed, FlagOperation.Set, 1), new FlagEffect(HeardRule1, FlagOperation.Set, 1) }),
                    Node("need_bag", "보리한테 가방부터 받아 오렴~ 휴게실에 있을 거야.", null),

                    // 첫 주문
                    Choice("ask", "가방 받았구나! 잘 어울린다~ 그럼 첫 배달! 이 빵 바구니, 이웃 할머니네 우편함에 좀 넣어 줄래?",
                        ("네, 다녀올게요", "accept"),
                        ("지금은 좀…", "later")),
                    Node("accept", "고마워~ 할머니 댁은 광장에서 찻집 옆 골목으로 들어가면 있어.", null, actions: Do(giveBread)),
                    Node("later", "그래그래, 천천히 해~ 빵은 식어도 맛있으니까!", null),
                    Node("remind_bread", "할머니네 우편함에 넣어 주면 돼~ 찻집 옆 동쪽 골목이야.", null),
                    Node("after_bread", "할머니가 좋아하셨지? 할머니도 너한테 부탁할 게 있다던데~", null),

                    Node("letter_receive", "응? 아이가 나한테 그림을?", "letter_look", actions: Do(takeLetter)),
                    Node("letter_look", "어머, 우리 가게네! 이건 나, 이건 할머니, 이건 아이… 그리고 이 길쭉한 건 누굴까~?", "letter_gift"),
                    Node("letter_gift", "후후, 잘 그렸네. 이 그림은 네가 가지렴~ 집에 걸어 두면 좋겠다.", null, effects: Set(GiftDrawing)),

                    // 저녁 정산
                    Node("settle", "오늘 정말 수고 많았어, {player}. 첫날인데 다 해냈네!", "settle2"),
                    Node("settle2", "이건 오늘 일당 대신이야~ 갓 구운 빵!", "rule1_again",
                        effects: new[] { new FlagEffect(GiftBread, FlagOperation.Set, 1), new FlagEffect(Settled(1), FlagOperation.Set, 1) }),
                    Node("rule1_again", "벌써 해가 기우네. 막차 놓치지 말고, 해 지기 전엔 꼭 집에 들어가렴~", null, effects: heardRule1),
                    Node("done", "얼른 가~ 막차 놓치면 큰일이야!", null),

                    // DAY 2, 3 (배달 내용은 아직)
                    Node("d2", "잘 잤니, {player}? …누리 요즘 좀 큰 것 같지 않니~? 후후.", "d2_work"),
                    Node("d2_work", "오늘 배달은 아직 준비 중이야. 동네 한 바퀴 돌고 오렴~", null),
                    Node("d3", "오늘도 잘 부탁해~ 아, 오늘 저녁엔 특별한 배달이 하나 있단다.", null),
                };
            });
        }

        /// <summary>프롤로그: 해 질 녘 외곽 종점으로 마중 나온 빵집 주인. 숙소는 정류장 바로 앞, 옆은 솜의 집이다.</summary>
        static DialogueGraph CreateBakerBusStopDialogue() =>
            Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Baker_BusStop.asset", g =>
            {
                g.speakerName = "빵집 주인";
                g.entries = new List<DialogueEntry>
                {
                    Entry("remind", Is(MetBaker)),
                    Entry("arrive"),
                };
                g.nodes = new List<DialogueNode>
                {
                    // 규칙 1을 처음 듣는 순간. 주인공은 아직 그게 규칙인 줄 모른다.
                    Node("arrive", "어머, 해 지기 전에 왔네! 다행이다~", "arrive2"),
                    Node("arrive2", "네가 {player}구나. 전단 보고 온 거지? 반가워~", "arrive3"),
                    Node("arrive3", "…짐이 그거 하나야? 후후, 괜찮아. 여기 오는 사람들은 다 사정이 있지~ 안 물어볼게.", "arrive4"),
                    Node("arrive4", "숙소는 바로 여기, 저 하얀 집이야. 배달원은 다 여기서 시작해~", "arrive5"),
                    Node("arrive5", "옆집은… 지금은 비어 있어.", "arrive6"),
                    Node("arrive6", "내일 아침 버스 타고 마을로 오렴. 내리면 바로 광장이고, 빵집은 광장 서쪽이야~", "arrive7"),
                    Node("arrive7", "그럼 푹 자~ 나는 막차 타고 들어갈게!", null, effects: Set(MetBaker)),
                    Node("remind", "얼른 들어가 쉬렴~ 해 떨어지기 전에!", null),
                };
            });

        // ---------- 배경 주민 한 마디 (위에서부터 처음 맞는 한 줄) ----------

        public static ConditionalText[] BarksFlorist() => new[]
        {
            Line("해 진다~ 얼른 들어가야지.", TimeIs(TimeOfDay.Evening)),
            Line("노란 꽃은 안 팔아~ 다 숲으로 가거든.", DayIs(3)),
            Line("어젯밤 바람 소리 들었니? 똑똑, 똑똑~ 후후.", DayIs(2)),
            Line("새 배달원이구나! 오늘 꽃이 예쁘게 폈어~"),
        };

        public static ConditionalText[] BarksGrocer() => new[]
        {
            Line("막차 놓치지 마, 신입~", TimeIs(TimeOfDay.Evening)),
            Line("오늘 저녁엔 도시락이 하나 더 나간다며? 수고가 많네~", DayIs(3)),
            Line("솜이랑 똑같은 가방이네~ 잘 어울려!", Is(HasBag)),
            Line("어서 와~ 사과 하나 줄까? 아, 배달 중이구나!"),
        };

        public static ConditionalText[] BarksGossipA() => new[]
        {
            Line("오늘은 여기까지~ 들어가자.", TimeIs(TimeOfDay.Evening)),
            Line("…그래서 그 집 막내가 창문을 열었대~"),
        };

        public static ConditionalText[] BarksGossipB() => new[]
        {
            Line("응응, 내일 봐~ 해 지기 전에!", TimeIs(TimeOfDay.Evening)),
            Line("어머~ 그럼 이제 밤에 나가겠네~ 후후."),
        };

        public static ConditionalText[] BarksSweeper() => new[]
        {
            Line("자, 다들 들어가자~ 해 진다.", TimeIs(TimeOfDay.Evening)),
            Line("해 지기 전엔 다 들어가야지~ 그게 편해."),
        };

        public static ConditionalText[] BarksShopper() => new[]
        {
            Line("아이고, 벌써 저녁이네~", TimeIs(TimeOfDay.Evening)),
            Line("호두빵 나왔대~ 얼른 가 봐야지!"),
        };

        public static ConditionalText[] BarksKidA() => new[]
        {
            Line("엄마가 들어오래~!", TimeIs(TimeOfDay.Evening)),
            Line("어젯밤에 누가 내 이름 불렀어! 대답 안 했지롱~", DayAtLeast(2)),
            Line("잡았다~! 이번엔 네가 술래!"),
        };

        public static ConditionalText[] BarksKidB() => new[]
        {
            Line("내일 또 놀자~!", TimeIs(TimeOfDay.Evening)),
            Line("배달원 언니야? 오빠야? 아무튼 안녕~!"),
        };

        public static ConditionalText[] BarksCat() => new[]
        {
            Line("…냐.", TimeIs(TimeOfDay.Evening)),
            Line("…(숲 쪽을 한참 보고 있다)", DayAtLeast(2)),
            Line("…냥."),
        };

        /// <summary>선배 배달원 보리: 수다스럽고 다정하다. 솜을 늘 현재형으로 말한다.</summary>
        static DialogueGraph CreateSeniorDialogue() =>
            Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Senior.asset", g =>
            {
                g.speakerName = "보리";
                g.entries = new List<DialogueEntry>
                {
                    Entry("d3_opened", DayIs(3), Is(WindowOpened)),
                    Entry("d3_ignored", DayIs(3), Is(WindowIgnored)),
                    Entry("d3", DayIs(3)),
                    Entry("d2_seen", DayIs(2), Is(SilhouetteSeen)),
                    Entry("d2", DayIs(2)),
                    Entry("evening", TimeIs(TimeOfDay.Evening)),
                    Entry("working", Is(HasBag)),
                    Entry("bag"),
                };
                g.nodes = new List<DialogueNode>
                {
                    Node("bag", "네가 신입이구나! 난 보리, 3년 차야~ 모르는 거 있으면 다 물어봐!", "bag2"),
                    Node("bag2", "자, 이거. 솜 거야. 이제 네 거~", "bag3", effects: Set(HasBag)),
                    Node("bag3", "솜은 밤 근무라서 이제 안 써! 진짜 성실한 애야~ 길도 진짜 잘 알고.", "bag4"),
                    Node("bag4", "아, 저 벽에 수칙 붙어 있지? 꼭 읽어 둬~ 1번은 오늘 들었지?", null),
                    Node("working", "길 모르겠으면 게시판 봐~ 아, 솜은 게시판 안 보고도 다 외웠는데!", null),
                    Node("evening", "오늘 수고했어! 막차 놓치지 마~ 수칙 1번!", null),

                    Node("d2_seen", "어젯밤에 창밖으로 뭐 지나갔지? 키 큰 거.", "d2_seen2"),
                    Node("d2_seen2", "아~ 그거 솜일 거야. 신입 왔다니까 인사하러 왔나 봐! 후후.", null),
                    Node("d2", "어젯밤 푹 잤어? 첫날은 원래 피곤해~ 솜도 첫날엔 바로 곯아떨어졌대.", null),

                    Node("d3_opened", "어머, 창문 열어 봤어? 솜이 좋아했겠다~", "d3_opened2"),
                    Node("d3_opened2", "…너 오늘 좀 커 보인다? 후후, 기분 탓인가.", null),
                    Node("d3_ignored", "어젯밤에 똑똑 소리 났지? 안 열었어? 잘했어! 수칙 2번!", null),
                    Node("d3", "오늘이 벌써 사흘째네~ 이제 완전 우리 마을 사람이다!", null),
                };
            });

        /// <summary>조용한 동료 누리: 말수가 적고, 규칙을 대충 지킨다. 날마다 조금씩 커 보인다.</summary>
        static DialogueGraph CreateQuietDialogue() =>
            Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Quiet.asset", g =>
            {
                g.speakerName = "누리";
                g.entries = new List<DialogueEntry>
                {
                    Entry("d3", DayIs(3)),
                    Entry("d2", DayIs(2)),
                    Entry("evening", TimeIs(TimeOfDay.Evening)),
                    Entry("bag", Is(HasBag)),
                    Entry("hello"),
                };
                g.nodes = new List<DialogueNode>
                {
                    Node("hello", "…누리.", "hello2"),
                    Node("hello2", "…보리한테 가 봐. 가방.", null),
                    Node("bag", "…그 가방, 솜 거네.", "bag2"),
                    Node("bag2", "…어깨끈 길이는 안 고쳐도 돼. 금방 맞게 돼.", null),
                    Node("evening", "…숲 쪽 다녀올게.", "evening2"),
                    Node("evening2", "…응? 해? 괜찮아. 나는.", null),
                    Node("d2", "…창문 소리 나도 신경 쓰지 마.", "d2b"),
                    Node("d2b", "…나는 가끔 열어 봐.", null),
                    Node("d3", "…오늘 저녁 도시락, 네 차례래.", "d3b"),
                    Node("d3b", "…받는 사람 칸은 비워 둬. 원래 그래.", null),
                };
            });

        static DialogueGraph CreateGrandmaDialogue(PrototypeData d)
        {
            var giveHat = RequestOrder(d, d.hatOrder);

            return Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Grandma.asset", g =>
            {
                g.speakerName = "이웃 할머니";
                g.entries = new List<DialogueEntry>
                {
                    // GDD: DAY 2에 할머니가 규칙 2를 전한다
                    Entry("rule2", DayIs(2)),
                    Entry("hat_done", Delivered(d.hatOrder)),
                    Entry("hat_remind", Accepted(d.hatOrder)),
                    Entry("thanks_bread", Delivered(d.breadOrder)),
                    Entry("wait_bread", Accepted(d.breadOrder)),
                    Entry("hello"),
                };
                g.nodes = new List<DialogueNode>
                {
                    Node("hello", "어서 오렴~ 새로 온 배달원이구나. 빵집 주인이 칭찬을 많이 하더구나.", null),
                    Node("wait_bread", "빵은 우편함에 넣어 주면 된단다~ 손이 느려서 말이야, 호호.", null),
                    Node("thanks_bread", "빵 잘 받았단다, 고마워라~ 이건 답례로 털실 뭉치란다.", "ask_hat", effects: Set(GiftYarn)),
                    Choice("ask_hat", "그 김에 이 털모자 좀 아이한테 갖다주겠니? 우체국 뒤 북쪽 공터에서 놀고 있을 게다.",
                        ("네, 갖다줄게요", "accept_hat"),
                        ("나중에요", "later_hat")),
                    Node("accept_hat", "고맙구나. 요즘 밤바람이 차서 말이야~ 창문도 자꾸 덜컹거리고, 호호.", null, actions: Do(giveHat)),
                    Node("later_hat", "그래, 천천히 하렴~", null),
                    Node("hat_remind", "아이는 우체국 뒤 북쪽 공터에 있을 게다~", null),
                    Node("hat_done", "아이가 좋아하더냐? 호호, 다행이구나.", null),
                    Node("rule2", "어서 오렴~ 아 참, 밤에 창문에서 똑똑, 똑똑 하면 열지 마~ 그냥 바람이야.", null, effects: Set(HeardRule2)),
                };
            });
        }

        static DialogueGraph CreateChildDialogue(PrototypeData d)
        {
            var takeHat = DeliverOrder(d, d.hatOrder);
            var giveLetter = RequestOrder(d, d.letterOrder);

            return Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Child.asset", g =>
            {
                g.speakerName = "아이";
                g.entries = new List<DialogueEntry>
                {
                    Entry("bye", Delivered(d.letterOrder)),
                    Entry("letter_remind", Accepted(d.letterOrder)),
                    Entry("ask_letter", Delivered(d.hatOrder)),
                    Entry("receive_hat", Accepted(d.hatOrder)),
                    Entry("play"),
                };
                g.nodes = new List<DialogueNode>
                {
                    Node("play", "놀자! …아, 일하는 중이구나? 그럼 나중에 놀자~", null),

                    Node("receive_hat", "우와, 할머니 모자다!", "som_house", actions: Do(takeHat)),
                    // GDD: 불 꺼진 집을 지나칠 때 주민이 밝은 말투로 말한다
                    Node("som_house", "너 종점 숙소에 살지? 그 옆에 불 꺼진 집 있잖아. 솜이 살던 집이야.", "som_house2", effects: Set("heard.som_house")),
                    Node("som_house2", "솜은 이제 밤에 나가~ 그래서 낮엔 없어!", "ask_letter"),
                    Choice("ask_letter", "아 맞다! 이거 빵집에 갖다줄래? 내가 그린 그림이야.",
                        ("그래, 갖다줄게", "accept_letter"),
                        ("나중에", "later_letter")),
                    Node("accept_letter", "헤헤, 빵집 주인님이 좋아할 거야!", null, actions: Do(giveLetter)),
                    Node("later_letter", "치, 그럼 나중에 꼭이야!", null),
                    Node("letter_remind", "빨리 갖다줘~ 빵집 주인님한테!", null),
                    Node("bye", "빵집 주인님이 좋아했어? 헤헤. 내일 또 놀자, {player}!", null),
                };
            });
        }

        /// <summary>DAY 2 밤 창문. 두드리는 동안에만 시작 지점이 있다.</summary>
        static DialogueGraph CreateWindowDialogue() =>
            Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Window.asset", g =>
            {
                g.speakerName = string.Empty;
                g.entries = new List<DialogueEntry>
                {
                    Entry("curtain", Is(KnockActive), Is(FlagKeys.CurtainClosed)),
                    Entry("voice", Is(KnockActive)),
                };
                g.nodes = new List<DialogueNode>
                {
                    Choice("curtain", "커튼 너머에서 똑똑, 똑똑.",
                        ("커튼을 걷는다", "reveal"),
                        ("가만히 있는다", "stay")),
                    Node("reveal", "창밖에 누군가 서 있다. 키가 아주 크다.", "voice",
                        effects: new[] { new FlagEffect(FlagKeys.CurtainClosed, FlagOperation.Set, 0) }),
                    Node("stay", "…", null),
                    Speaker(Choice("voice", "배달 왔어요~ 따뜻한 빵이에요~ 창문 좀 열어 줄래요~?",
                        ("창문을 연다", "open"),
                        ("열지 않는다", "ignore")), "창밖의 목소리"),
                    Node("open", "창문을 열었다. …아무도 없다.", "thread", effects: new[]
                    {
                        new FlagEffect(FlagKeys.RuleViolations, FlagOperation.Add, 1),
                        new FlagEffect(WindowOpened, FlagOperation.Set, 1),
                        new FlagEffect(KnockActive, FlagOperation.Set, 0),
                        new FlagEffect(KnockDone, FlagOperation.Set, 1),
                    }),
                    Node("thread", "창틀에 노란 실 한 가닥이 걸려 있다.", null),
                    Node("ignore", "창문을 열지 않았다. …소리가 멎었다.", null, effects: new[]
                    {
                        new FlagEffect(WindowIgnored, FlagOperation.Set, 1),
                        new FlagEffect(KnockActive, FlagOperation.Set, 0),
                        new FlagEffect(KnockDone, FlagOperation.Set, 1),
                    }),
                };
            });

        // ---------- 글 (소식지, 일지) ----------

        static DocumentAsset CreateNewsletter() =>
            Asset<DocumentAsset>($"{Root}/Home/Doc_Newsletter.asset", doc =>
            {
                doc.title = "숲가 마을 소식";
                doc.paragraphs = new[]
                {
                    Line("새 배달원을 환영해요! 이름은 {player}래요. 마주치면 반갑게 인사해 주세요~", DayIs(1)),
                    Line("빵집 이번 주 추천: 호두빵. 갓 구운 건 오전에만!", DayIs(1)),
                    Line("해 지기 전 귀가는 우리 모두의 약속! 늦으면 다들 걱정해요~", DayIs(1)),

                    Line("숲 경계 도시락 당번: 이번 주도 배달원이 수고해 줄 거예요~", DayIs(2)),
                    Line("밤에 창문이 덜컹거려도 걱정 마세요. 바람이에요! 바람은 창문을 열어 달라고 하지 않으니까요~", DayIs(2)),
                    Line("할머니네 털실 나눔: 노란색은 다 떨어졌대요.", DayIs(2)),

                    Line("솜이의 노란 목도리를 보신 분은 그냥 두세요~ 솜이가 아직 쓰고 있대요.", DayIs(3)),
                    Line("오늘도 숲 경계 도시락 배달이 있어요. 놓고만 오면 돼요~", DayIs(3)),
                };
                doc.emptyText = "(오늘은 소식지가 오지 않았다)";
            });

        static DocumentAsset CreateMyJournal(PrototypeData d) =>
            Asset<DocumentAsset>($"{Root}/Home/Doc_MyJournal.asset", doc =>
            {
                doc.title = "{player}의 배달 일지";
                doc.compact = true;
                doc.paragraphs = new[]
                {
                    Line("— 첫째 날 —"),
                    Line("빵집에서 동료 두 명을 만났다. 보리 선배와 누리.", Is(Briefed)),
                    Line("보리 선배에게 낡은 배달 가방을 받았다. 솜이라는 사람이 쓰던 거라고 했다. 밤 근무라서 이제 안 쓴다고.", Is(HasBag)),
                    Line("빵을 할머니께 배달했다.", Delivered(d.breadOrder)),
                    Line("할머니가 답례로 털실 뭉치를 주셨다.", Is(GiftYarn)),
                    Line("할머니의 털모자를 아이에게 전했다. 아이가 내 숙소 옆 불 꺼진 집이 솜의 집이라고 했다.", Delivered(d.hatOrder)),
                    Line("아이가 그린 그림을 빵집 주인께 전했다. 빵집 주인이 그림을 내게 주었다.", Is(GiftDrawing)),
                    Line("해 지기 전에 집에 돌아왔다.", Is(CameHome(1))),
                    Line("창밖으로 무언가 지나갔다. 키가 아주 컸다.", Is(SilhouetteSeen)),
                    Line("밤에 밖에서 느린 발소리가 났다.", Is(SilhouetteDone), Not(SilhouetteSeen)),

                    Line("— 둘째 날 —", DayAtLeast(2)),
                    Line("오늘도 배달을 했다.", Is(CameHome(2))),
                    Line("할머니가 밤에 똑똑 소리가 나면 열지 말라고 하셨다. 바람이라고.", Is(HeardRule2)),
                    Line("창문에서 소리가 났다. 열지 않았다. 바람이었을 것이다.", Is(WindowIgnored)),
                    Line("창문을 열었다. 아무도 없었다. 창틀에 노란 실이 걸려 있었다.", Is(WindowOpened)),

                    Line("— 셋째 날 —", DayAtLeast(3)),
                };
            });

        /// <summary>전 배달원(솜)의 일지. 밤마다 한 장씩 더 읽을 수 있다. 처음엔 내 일지와 거의 같은 문장이다.</summary>
        static DocumentAsset CreateSomJournal() =>
            Asset<DocumentAsset>($"{Root}/Home/Doc_SomJournal.asset", doc =>
            {
                doc.title = "배달 일지 (표지의 이름이 긁혀 있다)";
                doc.paragraphs = new[]
                {
                    Line("— 첫째 날 —\n빵집 주인께 배달 가방을 받았다. 새것이다.\n보리도 오늘 처음 왔다고 한다. 동기가 생겼다.\n빵을 할머니께 배달했다.\n할머니가 답례로 털실 뭉치를 주셨다.\n해 지기 전에 집에 돌아왔다.\n창밖으로 키 큰 누군가가 지나갔다. 아마 이웃이겠지.",
                        Is(CameHome(1))),
                    Line("— 둘째 날 —\n할머니가 밤에 똑똑 소리가 나면 열지 말라고 하셨다. 바람이라고.\n창문에서 똑똑 소리가 났다. 빵집 주인 목소리였다.\n…열어 봤다.",
                        Is(CameHome(2))),
                    Line("— 셋째 날 —\n요즘 다들 내가 커 보인다고 한다.\n할머니가 노란 목도리를 떠 주셨다. 따뜻하다.\n오늘 밤엔 숲 경계로 도시락을 가져간다. 이번엔 받는 사람",
                        Is(CameHome(3))),
                };
                doc.emptyText = "(가방 안주머니에 낡은 수첩이 있다. 아직은 펼쳐 볼 마음이 들지 않는다.)";
            });

        /// <summary>주인공의 짐가방. 사연은 암시만 한다: 편도 버스표 하나만 읽을 수 있다.</summary>
        static DocumentAsset CreateSuitcase() =>
            Asset<DocumentAsset>($"{Root}/Home/Doc_Suitcase.asset", doc =>
            {
                doc.title = "짐가방";
                doc.compact = true;
                doc.paragraphs = new[]
                {
                    Line("겉주머니에 버스표가 꽂혀 있다."),
                    Line("[ 편도 · 종점: 숲가 마을 ]"),
                    Line(" "),
                    Line("나머지는… 아직 열고 싶지 않다."),
                };
            });

        /// <summary>빵집 게시판: 그날 배달 목록. 끝낸 주문에는 도장이 찍힌다.</summary>
        static DocumentAsset CreateOrderBoard(PrototypeData d) =>
            Asset<DocumentAsset>($"{Root}/Bakery/Doc_OrderBoard.asset", doc =>
            {
                doc.title = "오늘의 배달";
                doc.compact = true;
                doc.paragraphs = new[]
                {
                    Line("□ 빵 바구니 — 빵집 → 이웃 할머니 (우편함)", DayIs(1), Not(FlagKeys.Delivered(d.breadOrder.Id))),
                    Line("■ 빵 바구니 — 빵집 → 이웃 할머니  [완료]", DayIs(1), Delivered(d.breadOrder)),
                    Line("□ 털모자 — 이웃 할머니 → 아이", DayIs(1), Not(FlagKeys.Delivered(d.hatOrder.Id))),
                    Line("■ 털모자 — 이웃 할머니 → 아이  [완료]", DayIs(1), Delivered(d.hatOrder)),
                    Line("□ 그림 편지 — 아이 → 빵집", DayIs(1), Not(FlagKeys.Delivered(d.letterOrder.Id))),
                    Line("■ 그림 편지 — 아이 → 빵집  [완료]", DayIs(1), Delivered(d.letterOrder)),
                    Line("(오늘 배달은 준비 중)", DayAtLeast(2)),
                    Line(" "),
                    Line("※ 숲 경계 도시락 — 담당: 누리", DayAtLeast(1), new FlagCondition(FlagKeys.Day, Comparison.Less, 3)),
                    Line("※ 숲 경계 도시락 — 담당: {player}", DayIs(3)),
                };
            });

        /// <summary>
        /// 배달원 수칙 포스터. 처음엔 1번만 읽히고, 2·3번 위에는 전단이 덧붙어 있다.
        /// 그 규칙을 들은 날부터 전단이 떨어져 있다.
        /// </summary>
        static DocumentAsset CreateRulesPoster() =>
            Asset<DocumentAsset>($"{Root}/Bakery/Doc_RulesPoster.asset", doc =>
            {
                doc.title = "배달원 수칙";
                doc.paragraphs = new[]
                {
                    Line("1. 해 지기 전에 집에 들어갈 것."),
                    Line("2. ▒▒▒▒▒▒▒▒▒▒ (위에 '호두빵 할인' 전단이 덧붙어 있다)", Not(HeardRule2)),
                    Line("2. 창문을 두 번 두드리면 열지 말 것.", Is(HeardRule2)),
                    Line("3. ▒▒▒▒▒▒▒▒▒▒ (위에 '노란 목도리를 찾습니다' 전단이 덧붙어 있다)", Not(HeardRule3)),
                    Line("3. 이름을 불러도 대답하지 말 것.", Is(HeardRule3)),
                    Line("※ 수칙을 지키지 않은 배달원은 밤 근무로 전환될 수 있습니다."),
                };
            });

        static DocumentAsset CreateSomLocker() =>
            Asset<DocumentAsset>($"{Root}/Bakery/Doc_SomLocker.asset", doc =>
            {
                doc.title = "사물함";
                doc.compact = true;
                doc.paragraphs = new[]
                {
                    Line("이름표가 테이프로 가려져 있다."),
                    Line("테이프 끝으로 'ㅅ' 한 획이 비친다."),
                    Line("잠겨 있다. 안에서 털실 냄새가 난다."),
                };
            });

        /// <summary>HUD의 '할 일'. 위에서부터 처음 맞는 한 줄.</summary>
        static ObjectiveList CreateObjectives(PrototypeData d) =>
            Asset<ObjectiveList>($"{Root}/Flow/Objectives.asset", o =>
            {
                o.objectives = new[]
                {
                    // 프롤로그
                    Line("정류장에 마중 나온 사람에게 말 걸기", DayIs(0), Not(MetBaker)),
                    Line("정류장 앞 하얀 집(숙소)에 들어가 쉬기", DayIs(0)),

                    // 밤
                    Line("잠자리에 들기", TimeIs(TimeOfDay.Night)),

                    // DAY 1
                    Line("숙소에 들어가기", DayIs(1), Is(Settled(1)), Is(AtOutskirts)),
                    Line("광장 정류장에서 막차 타고 숙소로 돌아가기", DayIs(1), Is(Settled(1))),
                    Line("버스 타고 마을로 돌아가 정산 받기", DayIs(1), TimeIs(TimeOfDay.Evening), Is(AtOutskirts)),
                    Line("빵집으로 돌아가 정산 받기", DayIs(1), TimeIs(TimeOfDay.Evening)),
                    Line("정류장에서 버스 타고 마을로 출근하기", DayIs(1), Not(Briefed), Is(AtOutskirts)),
                    Line("빵집으로 출근하기 (광장 서쪽)", DayIs(1), Not(Briefed)),
                    Line("휴게실의 보리에게 가방 받기", DayIs(1), Not(HasBag)),
                    Line("빵집 주인에게 첫 주문 받기", DayIs(1), Not(FlagKeys.Accepted(d.breadOrder.Id))),
                    Line("오늘의 배달 마치기 (빵집 게시판 참고)", DayIs(1)),

                    // DAY 2, 3 (배달 내용은 아직)
                    Line("(임시) 마을을 돌아본 뒤 숙소 문 앞에서 하루 넘기기", DayAtLeast(2), TimeIs(TimeOfDay.Morning)),
                    Line("숙소에 들어가기", DayAtLeast(2), TimeIs(TimeOfDay.Evening), Is(AtOutskirts)),
                    Line("막차 타고 숙소로 돌아가기", DayAtLeast(2), TimeIs(TimeOfDay.Evening)),
                };
            });

        // ---------- 헬퍼 ----------

        static RequestOrderAction RequestOrder(PrototypeData d, DeliveryOrder order) =>
            Asset<RequestOrderAction>($"{Root}/Dialogue/Actions/Action_RequestOrder_{ShortId(order)}.asset", a =>
            {
                a.order = order;
                a.orderRequested = d.orderRequested;
            });

        static DeliverOrderAction DeliverOrder(PrototypeData d, DeliveryOrder order) =>
            Asset<DeliverOrderAction>($"{Root}/Dialogue/Actions/Action_DeliverOrder_{ShortId(order)}.asset", a =>
            {
                a.order = order;
                a.deliverRequested = d.deliverRequested;
            });

        static string ShortId(DeliveryOrder order) => order.Id.Replace("Order_", "");

        public static FlagCondition Is(string key) => new(key, Comparison.Equal, 1);
        public static FlagCondition Not(string key) => new(key, Comparison.Equal, 0);
        public static FlagCondition DayIs(int day) => new(FlagKeys.Day, Comparison.Equal, day);
        public static FlagCondition DayAtLeast(int day) => new(FlagKeys.Day, Comparison.GreaterOrEqual, day);
        public static FlagCondition TimeIs(TimeOfDay time) => new(FlagKeys.TimeOfDay, Comparison.Equal, (int)time);
        public static FlagCondition TimeIsNot(TimeOfDay time) => new(FlagKeys.TimeOfDay, Comparison.NotEqual, (int)time);
        static FlagCondition Accepted(DeliveryOrder order) => Is(FlagKeys.Accepted(order.Id));
        static FlagCondition Delivered(DeliveryOrder order) => Is(FlagKeys.Delivered(order.Id));

        static FlagEffect[] Set(string key) => new[] { new FlagEffect(key, FlagOperation.Set, 1) };
        static DialogueAction[] Do(params DialogueAction[] actions) => actions;
        static ConditionalText Line(string text, params FlagCondition[] conditions) => new(text, conditions);

        static DialogueEntry Entry(string startNodeId, params FlagCondition[] conditions) =>
            new() { startNodeId = startNodeId, conditions = conditions };

        static DialogueNode Node(string id, string text, string nextId, FlagEffect[] effects = null, DialogueAction[] actions = null) =>
            new() { id = id, text = text, nextId = nextId, effects = effects, actions = actions };

        static DialogueNode Choice(string id, string text, params (string text, string nextId)[] choices)
        {
            var node = new DialogueNode { id = id, text = text };
            foreach (var (choiceText, nextId) in choices)
                node.choices.Add(new DialogueChoice { text = choiceText, nextId = nextId });
            return node;
        }

        static DialogueNode Speaker(DialogueNode node, string speaker)
        {
            node.speaker = speaker;
            return node;
        }

        /// <summary>경로에 에셋이 있으면 그대로 쓰고, 없을 때만 만들어 init으로 초기값을 채운다.</summary>
        static T Asset<T>(string path, Action<T> init = null) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            EnsureFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            asset.name = System.IO.Path.GetFileNameWithoutExtension(path);
            init?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
