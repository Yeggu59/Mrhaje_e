// ����
// Unit�� 1���� ��ǥ�� �������� ���� GameObject�� �̵���Ų��
// �̵��� �б� �����̸� ������ ���¸� �������� �ʴ´�
//
// ��� ��Ģ
// BattleLoopRunner�� �� ������ SetTargetX�� ��ǥ ��ǥ�� ������
// �� ������Ʈ�� �ε巴�� ���� �̵��Ѵ�

using UnityEngine;

namespace Game.Battle.Presentation
{
    public sealed class UnitView : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 8f;    // �ʴ� �̵� �ӵ�
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
