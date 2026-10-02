using System.Collections.Generic;
using KindNeighbors.Core.Events;
using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.Dialogue
{
    /// <summary>
    /// 대화 그래프를 순회한다. 노드에 들어갈 때 효과와 동작을 적용하고,
    /// 선택지가 있으면 숫자 키, 없으면 계속 키로 다음 노드로 이동한다.
    /// </summary>
    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] DialogueGraphEventChannel dialogueRequested;
        [Tooltip("대화 시작 시 true, 종료 시 false. 플레이어 입력 잠금 등에 쓴다.")]
        [SerializeField] BoolEventChannel dialogueActive;
        [SerializeField] KeyCode continueKey = KeyCode.E;

        readonly List<DialogueChoice> availableChoices = new();
        DialogueGraph graph;
        int lastInputFrame;

        public bool IsRunning => Current != null;
        public DialogueNode Current { get; private set; }
        public IReadOnlyList<DialogueChoice> Choices => availableChoices;

        public string CurrentSpeaker =>
            Current == null ? string.Empty : string.IsNullOrEmpty(Current.speaker) ? graph.speakerName : Current.speaker;

        void OnEnable() => dialogueRequested.Raised += OnDialogueRequested;
        void OnDisable() => dialogueRequested.Raised -= OnDialogueRequested;

        void OnDialogueRequested(DialogueGraph requested)
        {
            if (IsRunning)
                return;

            DialogueNode start = requested.FindStartNode(flags);
            if (start == null)
                return;

            graph = requested;
            // 대화를 시작한 키 입력이 같은 프레임에 첫 줄을 넘기지 않도록 한다
            lastInputFrame = Time.frameCount;
            dialogueActive.Raise(true);
            EnterNode(start);
        }

        void Update()
        {
            if (!IsRunning || Time.frameCount == lastInputFrame)
                return;

            if (availableChoices.Count > 0)
            {
                for (int i = 0; i < availableChoices.Count && i < 9; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    {
                        Choose(i);
                        return;
                    }
            }
            else if (Input.GetKeyDown(continueKey) || Input.GetKeyDown(KeyCode.Space))
            {
                lastInputFrame = Time.frameCount;
                EnterNode(graph.FindNode(Current.nextId));
            }
        }

        public void Choose(int index)
        {
            lastInputFrame = Time.frameCount;
            EnterNode(graph.FindNode(availableChoices[index].nextId));
        }

        void EnterNode(DialogueNode node)
        {
            Current = node;
            availableChoices.Clear();

            if (node == null)
            {
                graph = null;
                dialogueActive.Raise(false);
                return;
            }

            FlagEffect.ApplyAll(node.effects, flags);
            if (node.actions != null)
                foreach (var action in node.actions)
                    if (action != null)
                        action.Execute();

            foreach (var choice in node.choices)
                if (FlagCondition.All(choice.conditions, flags))
                    availableChoices.Add(choice);
        }
    }
}
