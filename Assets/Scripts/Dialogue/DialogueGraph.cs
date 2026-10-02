using System;
using System.Collections.Generic;
using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.Dialogue
{
    /// <summary>
    /// NPC 한 명의 대화 그래프. 노드는 id로 서로를 가리킨다 (nextId, choice.nextId).
    /// 같은 NPC라도 entries 중 조건(Day, 시간대, 플래그)이 처음으로 맞는 항목의 노드에서 시작한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Dialogue/Dialogue Graph", fileName = "Dlg_")]
    public class DialogueGraph : ScriptableObject
    {
        public string speakerName;

        [Tooltip("위에서부터 검사해 조건이 처음 맞는 항목에서 시작한다. 마지막에 조건 없는 기본 항목을 두면 좋다.")]
        public List<DialogueEntry> entries = new();

        public List<DialogueNode> nodes = new();

        public DialogueNode FindNode(string id) =>
            string.IsNullOrEmpty(id) ? null : nodes.Find(n => n.id == id);

        public DialogueNode FindStartNode(GameFlags flags)
        {
            foreach (var entry in entries)
                if (FlagCondition.All(entry.conditions, flags))
                    return FindNode(entry.startNodeId);
            return null;
        }
    }

    [Serializable]
    public class DialogueEntry
    {
        public FlagCondition[] conditions;
        public string startNodeId;
    }

    [Serializable]
    public class DialogueNode
    {
        public string id;
        [Tooltip("비어 있으면 그래프의 speakerName")]
        public string speaker;
        [TextArea(2, 4)] public string text;

        [Tooltip("선택지가 없을 때 다음 노드. 비어 있으면 대화 종료.")]
        public string nextId;
        public List<DialogueChoice> choices = new();

        [Tooltip("이 노드에 들어올 때 적용")]
        public FlagEffect[] effects;
        [Tooltip("이 노드에 들어올 때 실행 (주문 넘기기 등)")]
        public DialogueAction[] actions;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public string nextId;
        [Tooltip("모두 참일 때만 선택지가 보인다")]
        public FlagCondition[] conditions;
    }
}
