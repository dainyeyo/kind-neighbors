using KindNeighbors.Core.Flags;
using UnityEngine;

namespace KindNeighbors.World
{
    /// <summary>
    /// 조건이 참일 때만 대상 오브젝트들을 켠다. 선반의 답례품, 커튼 열림/닫힘, 램프 불빛, 창틀의 노란 실 등.
    /// 플래그가 바뀔 때마다 다시 검사한다. 이 컴포넌트 자신은 대상과 다른 오브젝트에 두어야 한다.
    /// </summary>
    public class FlagObjectToggle : MonoBehaviour
    {
        [SerializeField] GameFlags flags;
        [SerializeField] FlagCondition[] conditions;
        [SerializeField] GameObject[] targets;

        void OnEnable()
        {
            flags.Changed += OnFlagChanged;
            Refresh();
        }

        void OnDisable() => flags.Changed -= OnFlagChanged;

        // 세이브를 불러오는 경우 Changed 이벤트 이전 상태가 남아 있을 수 있으므로 시작 시점에도 맞춘다
        void Start() => Refresh();

        void OnFlagChanged(string key, int value) => Refresh();

        void Refresh()
        {
            bool active = FlagCondition.All(conditions, flags);
            foreach (var target in targets)
                if (target != null && target.activeSelf != active)
                    target.SetActive(active);
        }
    }
}
