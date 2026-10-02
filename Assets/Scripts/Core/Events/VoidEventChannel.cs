using System;
using UnityEngine;

namespace KindNeighbors.Core.Events
{
    [CreateAssetMenu(menuName = "Kind Neighbors/Events/Void Event Channel", fileName = "Evt_")]
    public class VoidEventChannel : ScriptableObject
    {
        public event Action Raised;

        public void Raise() => Raised?.Invoke();
    }
}
