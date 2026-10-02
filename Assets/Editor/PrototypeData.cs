using System;
using System.Collections.Generic;
using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using KindNeighbors.Delivery;
using KindNeighbors.Dialogue;
using KindNeighbors.Flow;
using UnityEditor;
using UnityEngine;

namespace KindNeighbors.EditorTools
{
    /// <summary>
    /// 프로토타입에 필요한 데이터 에셋(이벤트 채널, 플래그, 시간대, 조명, 주문, 대화)을 만든다.
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

        public GameFlags flags;
        public PhaseEventChannel phaseChanged;
        public VoidEventChannel advanceRequested;
        public BoolEventChannel inputLock;
        public DialogueGraphEventChannel dialogueRequested;
        public DeliveryOrderEventChannel orderRequested;
        public DeliveryOrderEventChannel deliverRequested;
        public DeliveryOrderEventChannel orderAccepted;
        public DeliveryOrderEventChannel orderCompleted;
        public GameFlowConfig flow;
        public DeliveryOrder breadOrder;
        public DeliveryOrder hatOrder;
        public DeliveryOrder letterOrder;
        public DialogueGraph bakerDialogue;
        public DialogueGraph grandmaDialogue;
        public DialogueGraph childDialogue;

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

            // DAY 1 배달: 빵집 주인 → 할머니 → 아이 → 빵집 주인 으로 이어진다
            d.breadOrder = Order("Order_D1_Bread", "빵 바구니", "빵집 주인", "이웃 할머니", GrandmaMailbox);
            d.hatOrder = Order("Order_D1_Hat", "털모자", "이웃 할머니", "아이", ChildSpot);
            d.letterOrder = Order("Order_D1_Letter", "그림 편지", "아이", "빵집 주인", BakerSpot);

            d.flow = CreateFlow(d);
            d.bakerDialogue = CreateBakerDialogue(d);
            d.grandmaDialogue = CreateGrandmaDialogue(d);
            d.childDialogue = CreateChildDialogue(d);

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
                    if (n == 1)
                    {
                        // 배달 3건을 모두 끝내면 자동으로 저녁이 된다
                        p.completeWhen = new[] { Delivered(d.breadOrder), Delivered(d.hatOrder), Delivered(d.letterOrder) };
                        p.advancePrompt = string.Empty;
                    }
                    else
                    {
                        // Day 2, 3 배달은 M2 이후에 채운다
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
            var heardRule1 = new[] { new FlagEffect("heard.rule1", FlagOperation.Set, 1) };

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
                    Node("greet", "어머, 네가 새로 온 배달원 {player}구나! 마침 잘 왔어~", "ask"),
                    Choice("ask", "이 빵 바구니, 이웃 할머니네 우편함에 좀 넣어 줄래?",
                        ("네, 다녀올게요", "accept"),
                        ("지금은 좀…", "later")),
                    Node("accept", "고마워~ 할머니 댁은 광장에서 오른쪽 길 끝이야.", "rule1", actions: Do(giveBread)),
                    Node("rule1", "아, 그리고… 해 지기 전엔 꼭 집에 들어가렴~", null, effects: heardRule1),
                    Node("later", "그래그래, 천천히 해~ 빵은 식어도 맛있으니까!", null),
                    Node("remind_bread", "할머니네 우편함에 넣어 주면 돼~ 광장에서 오른쪽 길 끝이야.", null),
                    Node("after_bread", "할머니가 좋아하셨지? 할머니도 너한테 부탁할 게 있다던데~", null),

                    Node("letter_receive", "응? 아이가 나한테 그림을?", "letter_look", actions: Do(takeLetter)),
                    Node("letter_look", "어머, 우리 가게네! 이건 나, 이건 할머니, 이건 아이… 그리고 이 길쭉한 건 누굴까~?", "letter_end"),
                    Node("letter_end", "후후, 잘 그렸네. 오늘 정말 수고 많았어, {player}.", "rule1_again"),
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
                    Node("thanks_bread", "빵 잘 받았단다, 고마워라~", "ask_hat"),
                    Choice("ask_hat", "그 김에 이 털모자 좀 아이한테 갖다주겠니? 북쪽 길에서 놀고 있을 게다.",
                        ("네, 갖다줄게요", "accept_hat"),
                        ("나중에요", "later_hat")),
                    Node("accept_hat", "고맙구나. 요즘 밤바람이 차서 말이야~ 창문도 자꾸 덜컹거리고, 호호.", null, actions: Do(giveHat)),
                    Node("later_hat", "그래, 천천히 하렴~", null),
                    Node("hat_remind", "아이는 북쪽 길에 있을 게다. 불 꺼진 집 근처 말이야~", null),
                    Node("hat_done", "아이가 좋아하더냐? 호호, 다행이구나.", null),
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
                    Node("som_house", "저기 불 꺼진 집 보여? 솜이 살던 집이야.", "som_house2",
                        effects: new[] { new FlagEffect("heard.som_house", FlagOperation.Set, 1) }),
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

        static FlagCondition Accepted(DeliveryOrder order) => new(FlagKeys.Accepted(order.Id), Comparison.Equal, 1);
        static FlagCondition Delivered(DeliveryOrder order) => new(FlagKeys.Delivered(order.Id), Comparison.Equal, 1);

        static DialogueAction[] Do(params DialogueAction[] actions) => actions;

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

        // ---------- 헬퍼 ----------

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
