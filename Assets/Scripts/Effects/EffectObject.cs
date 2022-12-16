using UnityEngine;

namespace Effects
{
    public class EffectObject : MonoBehaviour
    {
        public float Speed;

        private float _startTime;
        private float _journeyLength;
        private Vector3 _startPosition;
        private Vector3? _target;

        public void SetTarget(Vector3? target)
        {
            _target = target;

            _startTime = Time.time;
            _startPosition = transform.localPosition;
            _journeyLength = Vector3.Distance(_startPosition, _target.Value);
        }

        private void FixedUpdate()
        {
            if (_target != null)
            {
                Vector3 center = (_startPosition + _target.Value) * 0.5f;
                center -= new Vector3(-1f, 1f, 0);

                Vector3 riseRelCenter = _startPosition - center;
                Vector3 setRelCenter = _target.Value - center;

                float distCovered = (Time.time - _startTime) * Speed;
                float fractionOfJourney = distCovered / _journeyLength;

                transform.localPosition = Vector3.Slerp(riseRelCenter, setRelCenter, fractionOfJourney);
                transform.localPosition += center;

                if (Vector3.Distance(transform.localPosition, _target.Value) < 0.1f)
                    Destroy(gameObject);
            }
        }
    }
}
