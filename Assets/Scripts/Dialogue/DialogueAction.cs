using UnityEngine;

namespace KindNeighbors.Dialogue
{
    /// <summary>
    /// 대화 노드에서 실행하는 동작. 플래그 변경만으로 부족한 일(주문 넘기기, 시간대 넘기기 등)을
    /// 다른 시스템을 직접 참조하지 않고 이벤트 채널로 요청할 때 상속해서 쓴다.
    /// </summary>
    public abstract class DialogueAction : ScriptableObject
    {
        public abstract void Execute();
    }
}
