// 목적
// Unit의 1차원 좌표를 기준으로 실제 GameObject를 이동시킨다
// 이동은 읽기 전용이며 도메인 상태를 변경하지 않는다
//
// 사용 규칙
// BattleLoopRunner가 매 프레임 SetTargetX로 목표 좌표를 던지면
// 이 컴포넌트가 부드럽게 보간 이동한다

using UnityEngine;

namespace Game.Battle.Presentation
{
    public sealed class UnitView : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 8f;    // 초당 이동 속도
        [SerializeField] bool instantOnFirstSync = true;

        Vector3 _target;
        bool _hasTarget;

        public void SetTargetX(float worldX)
        {
            if (!_hasTarget && instantOnFirstSync)
            {
                var p0 = transform.position;
                transform.position = new Vector3(worldX, p0.y, p0.z);
                _hasTarget = true;
                _target = transform.position;
                return;
            }
            var p = transform.position;
            _target = new Vector3(worldX, p.y, p.z);
            _hasTarget = true;
        }

        void Update()
        {
            if (!_hasTarget) return;
            var p = transform.position;
            if (Mathf.Abs(p.x - _target.x) < 0.001f)
            {
                transform.position = _target;
                return;
            }
            transform.position = Vector3.MoveTowards(p, _target, moveSpeed * Time.deltaTime);
        }
    }
}
