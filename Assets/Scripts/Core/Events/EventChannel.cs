using System;
using UnityEngine;

namespace KindNeighbors.Core.Events
{
    /// <summary>
    /// ScriptableObject 기반 이벤트 채널 (옵저버 패턴).
    /// 발행자와 구독자는 서로를 모르고 같은 채널 에셋만 참조한다.
    /// 구독자는 반드시 OnDisable에서 구독을 해제해야 한다 (SO는 씬보다 오래 살아남는다).
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        public event Action<T> Raised;

        public void Raise(T value) => Raised?.Invoke(value);
    }
}
