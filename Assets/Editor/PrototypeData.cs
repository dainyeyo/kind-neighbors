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
        public static string CameHome(int day) => $"home.d{day}";

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
        public DocumentAsset newsletter;
        public DocumentAsset myJournal;
        public DocumentAsset somJournal;

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
            d.newsletter = CreateNewsletter();
            d.myJournal = CreateMyJournal(d);
            d.somJournal = CreateSomJournal();

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

            var phases = new List<PhaseDefinition>();
            for (int dayNumber = 1; dayNumber <= 3; dayNumber++)
            {
                int n = dayNumber;
                phases.Add(Asset<PhaseDefinition>($"{Root}/Flow/Phases/Phase_D{n}_1_Morning.asset", p =>
                {
                    p.phase = new GamePhase(n, TimeOfDay.Morning);
                    p.lighting = day;
                    p.spawnOnEnter = SpawnBed; // 매일 아침 집에서 일어난다
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

            return Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Baker.asset", g =>
            {
                g.speakerName = "빵집 주인";
                g.entries = new List<DialogueEntry>
                {
                    Entry("done", Delivered(d.letterOrder)),
                    Entry("letter_receive", Accepted(d.letterOrder)),
                    Entry("after_bread", Delivered(d.breadOrder)),
                    Entry("remind_bread", Accepted(d.breadOrder)),
                    Entry("greet"),
                };
                g.nodes = new List<DialogueNode>
                {
                    Node("greet", "어머, 네가 새로 온 배달원 {player}구나! 마침 잘 왔어~", "bag"),
                    Node("bag", "자, 이 가방부터 받으렴. 전 배달원이 쓰던 거야. 아직 튼튼해~", "ask", effects: Set(HasBag)),
                    Choice("ask", "그럼 첫 배달! 이 빵 바구니, 이웃 할머니네 우편함에 좀 넣어 줄래?",
                        ("네, 다녀올게요", "accept"),
                        ("지금은 좀…", "later")),
                    Node("accept", "고마워~ 할머니 댁은 광장에서 오른쪽 길 끝이야.", "rule1", actions: Do(giveBread)),
                    Node("rule1", "아, 그리고… 해 지기 전엔 꼭 집에 들어가렴~", null, effects: heardRule1),
                    Node("later", "그래그래, 천천히 해~ 빵은 식어도 맛있으니까!", null),
                    Node("remind_bread", "할머니네 우편함에 넣어 주면 돼~ 광장에서 오른쪽 길 끝이야.", null),
                    Node("after_bread", "할머니가 좋아하셨지? 할머니도 너한테 부탁할 게 있다던데~", null),

                    Node("letter_receive", "응? 아이가 나한테 그림을?", "letter_look", actions: Do(takeLetter)),
                    Node("letter_look", "어머, 우리 가게네! 이건 나, 이건 할머니, 이건 아이… 그리고 이 길쭉한 건 누굴까~?", "letter_gift"),
                    Node("letter_gift", "후후, 잘 그렸네. 이 그림은 네가 가지렴~ 집에 걸어 두면 좋겠다.", "letter_end", effects: Set(GiftDrawing)),
                    Node("letter_end", "오늘 정말 수고 많았어, {player}. 이건 오늘 일당 대신이야~ 갓 구운 빵!", "rule1_again", effects: Set(GiftBread)),
                    Node("rule1_again", "벌써 해가 기우네. 해 지기 전엔 꼭 집에 들어가렴~", null, effects: heardRule1),
                    Node("done", "오늘 수고 많았어~ 해 지기 전엔 꼭 집에 들어가렴~", null),
                };
            });
        }

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
                    Choice("ask_hat", "그 김에 이 털모자 좀 아이한테 갖다주겠니? 북쪽 길에서 놀고 있을 게다.",
                        ("네, 갖다줄게요", "accept_hat"),
                        ("나중에요", "later_hat")),
                    Node("accept_hat", "고맙구나. 요즘 밤바람이 차서 말이야~ 창문도 자꾸 덜컹거리고, 호호.", null, actions: Do(giveHat)),
                    Node("later_hat", "그래, 천천히 하렴~", null),
                    Node("hat_remind", "아이는 북쪽 길에 있을 게다. 불 꺼진 집 근처 말이야~", null),
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
                    Node("som_house", "저기 불 꺼진 집 보여? 솜이 살던 집이야.", "som_house2", effects: Set("heard.som_house")),
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
                    Line("빵집 주인께 낡은 배달 가방을 받았다. 전 배달원이 쓰던 거라고 했다.", Is(HasBag)),
                    Line("빵을 할머니께 배달했다.", Delivered(d.breadOrder)),
                    Line("할머니가 답례로 털실 뭉치를 주셨다.", Is(GiftYarn)),
                    Line("할머니의 털모자를 아이에게 전했다. 아이는 불 꺼진 집 앞에서 놀고 있었다.", Delivered(d.hatOrder)),
                    Line("아이가 그린 그림을 빵집 주인께 전했다. 빵집 주인이 그림을 내게 주었다.", Is(GiftDrawing)),
                    Line("해 지기 전에 집에 돌아왔다.", Is(CameHome(1))),
                    Line("창밖으로 무언가 지나갔다. 키가 아주 컸다.", Is(SilhouetteSeen)),
                    Line("밤에 밖에서 느린 발소리가 났다.", Is(SilhouetteDone), Not(SilhouetteSeen)),

                    Line("— 둘째 날 —", DayAtLeast(2)),
                    Line("오늘도 배달을 했다.", DayAtLeast(2)),
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
                    Line("— 첫째 날 —\n빵집 주인께 배달 가방을 받았다. 새것이다.\n빵을 할머니께 배달했다.\n할머니가 답례로 털실 뭉치를 주셨다.\n해 지기 전에 집에 돌아왔다.\n창밖으로 키 큰 누군가가 지나갔다. 아마 이웃이겠지.",
                        Is(CameHome(1))),
                    Line("— 둘째 날 —\n할머니가 밤에 똑똑 소리가 나면 열지 말라고 하셨다. 바람이라고.\n창문에서 똑똑 소리가 났다. 빵집 주인 목소리였다.\n…열어 봤다.",
                        Is(CameHome(2))),
                    Line("— 셋째 날 —\n요즘 다들 내가 커 보인다고 한다.\n할머니가 노란 목도리를 떠 주셨다. 따뜻하다.\n오늘 밤엔 숲 경계로 도시락을 가져간다. 이번엔 받는 사람",
                        Is(CameHome(3))),
                };
                doc.emptyText = "(가방 안주머니에 낡은 수첩이 있다. 아직은 펼쳐 볼 마음이 들지 않는다.)";
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
        static FlagCondition Not(string key) => new(key, Comparison.Equal, 0);
        public static FlagCondition DayIs(int day) => new(FlagKeys.Day, Comparison.Equal, day);
        static FlagCondition DayAtLeast(int day) => new(FlagKeys.Day, Comparison.GreaterOrEqual, day);
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
