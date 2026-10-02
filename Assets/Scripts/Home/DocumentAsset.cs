using KindNeighbors.Core.Flags;
using KindNeighbors.Core.Text;
using UnityEngine;

namespace KindNeighbors.Home
{
    /// <summary>
    /// 읽을 수 있는 글 (소식지, 일지). 문단마다 조건을 붙여 진행 상황에 따라 내용이 늘어나거나 바뀐다.
    /// </summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Home/Document", fileName = "Doc_")]
    public class DocumentAsset : ScriptableObject
    {
        public string title;
        public ConditionalText[] paragraphs;
        [Tooltip("보이는 문단이 하나도 없을 때")]
        public string emptyText = "(아직 아무것도 적혀 있지 않다)";
        [Tooltip("켜면 문단 사이에 빈 줄 없이 한 줄씩 잇는다 (일지처럼 짧은 문장이 이어지는 글)")]
        public bool compact;

        public ReadableDocument Compose(GameFlags flags)
        {
            string body = ConditionalText.Compose(paragraphs, flags, compact ? "\n" : "\n\n");
            return new ReadableDocument(TextFormatter.Format(title, flags), string.IsNullOrEmpty(body) ? emptyText : body);
        }
    }

    /// <summary>화면에 띄울 완성된 글.</summary>
    public readonly struct ReadableDocument
    {
        public readonly string title;
        public readonly string body;

        public ReadableDocument(string title, string body)
        {
            this.title = title;
            this.body = body;
        }
    }
}
