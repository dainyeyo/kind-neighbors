using KindNeighbors.Core.Events;
using UnityEngine;

namespace KindNeighbors.Home
{
    /// <summary>글 읽기 요청. ReadableObject가 발행하고 ReaderUI가 구독한다.</summary>
    [CreateAssetMenu(menuName = "Kind Neighbors/Events/Document Event Channel", fileName = "Evt_")]
    public class DocumentEventChannel : EventChannel<ReadableDocument> { }
}
