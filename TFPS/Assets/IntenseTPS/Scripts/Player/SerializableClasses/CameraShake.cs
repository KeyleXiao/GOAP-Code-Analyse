using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class CameraShaker
    {
        private static float _minMultip = .07f;
        private float shakeStartedAt = Mathf.Infinity;
        public bool HasEnded { get; private set; }
        public Vector3 LastShake { get; private set; }

        public float shakeTimer;

        public float maxShakeAmount;
        public bool useX, useY, useZ;

        public CameraShaker(CameraShaker _shaker, bool startShake = true)
        {
            shakeTimer = _shaker.shakeTimer;
            maxShakeAmount = _shaker.maxShakeAmount;
            useX = _shaker.useX;
            useY = _shaker.useY;
            useZ = _shaker.useZ;

            if (startShake)
                StartShake();
        }

        public CameraShaker(float _shakeTimer, float _maxShake, bool _useX, bool _useY, bool _useZ)
        {
            shakeTimer = _shakeTimer;
            maxShakeAmount = _maxShake;
            useX = _useX;
            useY = _useY;
            useZ = _useZ;
        }

        public void StartShake()
        {
            shakeStartedAt = Time.time;
            HasEnded = false;
        }

        public Vector3 Shake()
        {
            if (HasEnded)
                return Vector3.zero;
            float timeMultip = Mathf.Lerp(
                1, 0, (Time.time - shakeStartedAt) / (Time.time + (shakeTimer < .001f ? .001f : shakeTimer) - shakeStartedAt)
                );
            float maxRandShake = timeMultip * maxShakeAmount;
            Vector3 randShake = new Vector3(
                 useX ? Random.Range(-maxRandShake, maxRandShake) : 0,
                 useY ? Random.Range(-maxRandShake, maxRandShake) : 0,
                 useZ ? Random.Range(-maxRandShake, maxRandShake) : 0
                 );
            if (timeMultip < _minMultip)
            {
                HasEnded = true;
                LastShake = Vector3.zero;
                return Vector3.zero;
            }
            LastShake = randShake;
            return randShake;
        }

        public static CameraShaker operator *(CameraShaker _shake, float multiplier)
        {
            return new CameraShaker(_shake.shakeTimer, _shake.maxShakeAmount * multiplier, _shake.useX, _shake.useY, _shake.useZ);
        }
    }
}