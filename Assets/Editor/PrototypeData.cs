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

        public GameFlags flags;
        public PhaseEventChannel phaseChanged;
        public VoidEventChannel advanceRequested;
        public BoolEventChannel dialogueActive;
        public DialogueGraphEventChannel dialogueRequested;
        public DeliveryOrderEventChannel orderRequested;
        public DeliveryOrderEventChannel orderAccepted;
        public DeliveryOrderEventChannel orderCompleted;
        public GameFlowConfig flow;
        public DeliveryOrder breadOrder;
        public DialogueGraph bakerDialogue;

        public static PrototypeData LoadOrCreate()
        {
            var d = new PrototypeData();

            d.flags = Asset<GameFlags>($"{Root}/GameFlags.asset");

            d.phaseChanged = Asset<PhaseEventChannel>($"{Root}/Events/Evt_PhaseChanged.asset");
            d.advanceRequested = Asset<VoidEventChannel>($"{Root}/Events/Evt_PhaseAdvanceRequested.asset");
            d.dialogueActive = Asset<BoolEventChannel>($"{Root}/Events/Evt_DialogueActive.asset");
            d.dialogueRequested = Asset<DialogueGraphEventChannel>($"{Root}/Events/Evt_DialogueRequested.asset");
            d.orderRequested = Asset<DeliveryOrderEventChannel>($"{Root}/Events/Evt_OrderRequested.asset");
            d.orderAccepted = Asset<DeliveryOrderEventChannel>($"{Root}/Events/Evt_OrderAccepted.asset");
            d.orderCompleted = Asset<DeliveryOrderEventChannel>($"{Root}/Events/Evt_OrderCompleted.asset");

            d.breadOrder = Asset<DeliveryOrder>($"{Root}/Orders/Order_D1_Bread.asset", o =>
            {
                o.itemName = "빵 바구니";
                o.clientName = "빵집 주인";
                o.recipientName = "이웃 할머니";
                o.destinationId = "grandma_house";
            });

            d.flow = CreateFlow(d.breadOrder);
            d.bakerDialogue = CreateBakerDialogue(d);

            AssetDatabase.SaveAssets();
            return d;
        }

        // ---------- 시간대 ----------

        static GameFlowConfig CreateFlow(DeliveryOrder breadOrder)
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
                        // 배달을 끝내면 자동으로 저녁이 된다
                        p.completeWhen = new[] { new FlagCondition(FlagKeys.Delivered(breadOrder.Id), Comparison.Equal, 1) };
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

        static DialogueGraph CreateBakerDialogue(PrototypeData d)
        {
            var giveBread = Asset<RequestOrderAction>($"{Root}/Dialogue/Actions/Action_RequestOrder_D1_Bread.asset", a =>
            {
                a.order = d.breadOrder;
                a.orderRequested = d.orderRequested;
            });

            string delivered = FlagKeys.Delivered(d.breadOrder.Id);
            string accepted = FlagKeys.Accepted(d.breadOrder.Id);

            return Asset<DialogueGraph>($"{Root}/Dialogue/Dlg_Baker.asset", g =>
            {
                g.speakerName = "빵집 주인";
                g.entries = new List<DialogueEntry>
                {
                    Entry("after", new FlagCondition(delivered, Comparison.Equal, 1)),
                    Entry("remind", new FlagCondition(accepted, Comparison.Equal, 1)),
                    Entry("greet"),
                };
                g.nodes = new List<DialogueNode>
                {
                    Node("greet", "어머, 새로 온 배달원이구나! 마침 잘 왔어~", "ask"),
                    Choice("ask", "이 빵 바구니, 이웃 할머니네 우편함에 좀 넣어 줄래?",
                        ("네, 다녀올게요", "accept"),
                        ("지금은 좀…", "later")),
                    Node("accept", "고마워~ 할머니 댁은 광장에서 오른쪽 길 끝이야.", "rule1", actions: new DialogueAction[] { giveBread }),
                    Node("rule1", "아, 그리고… 해 지기 전엔 꼭 집에 들어가렴~", null,
                        effects: new[] { new FlagEffect("heard.rule1", FlagOperation.Set, 1) }),
                    Node("later", "그래그래, 천천히 해~ 빵은 식어도 맛있으니까!", null),
                    Node("remind", "할머니네 우편함에 넣어 주면 돼~ 광장에서 오른쪽 길 끝이야.", null),
                    Node("after", "할머니가 좋아하셨지? 수고했어~", "rule1"),
                };
            });
        }

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
