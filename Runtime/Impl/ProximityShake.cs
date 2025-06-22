using UnityEngine;

namespace CameraShake
{
    public class ProximityShake : ICameraShakeSource
    {
        private float _maxDistance;
        private float _minDistance;
        private float _maxIntensity;
        private float _frequency;
        private bool _isEnabled = true;
        private Vector3 _seed;
        private float _currentDistance;

        public Vector3 PositionOffset { get; private set; }
        public Vector3 RotationOffset { get; private set; }
        public bool IsFinished => !_isEnabled;

        public ProximityShake(
            float maxDistance, 
            float minDistance = 0.5f, 
            float maxIntensity = 1f,
            float frequency = 4f)
        {
            _maxDistance = Mathf.Max(maxDistance, 0.1f);
            _minDistance = Mathf.Clamp(minDistance, 0.1f, _maxDistance);
            _maxIntensity = maxIntensity;
            _frequency = frequency;
            _seed = new Vector3(Random.value, Random.value, Random.value);
            _currentDistance = _maxDistance;
        }

        public void UpdateShake(float deltaTime)
        {
            if (!_isEnabled)
            {
                PositionOffset = Vector3.zero;
                RotationOffset = Vector3.zero;
                return;
            }

            float intensityFactor = CalculateIntensityFactor(_currentDistance);

            float noiseX = Mathf.PerlinNoise(_seed.x, Time.time * _frequency) * 2f - 1f;
            float noiseY = Mathf.PerlinNoise(_seed.y, Time.time * _frequency) * 2f - 1f;
            float noiseZ = Mathf.PerlinNoise(_seed.z, Time.time * _frequency) * 2f - 1f;

            PositionOffset = new Vector3(noiseX, noiseY, 0f) * intensityFactor * 0.3f;
            RotationOffset = new Vector3(noiseY * 1.5f, noiseX * 1.5f, noiseZ) * intensityFactor * 2f;
        }

        private float CalculateIntensityFactor(float distance)
        {
            if (distance >= _maxDistance) return 0f;
            if (distance <= _minDistance) return _maxIntensity;
            return Mathf.Lerp(0f, _maxIntensity, 1f - ((distance - _minDistance) / (_maxDistance - _minDistance)));
        }

        public void UpdateDistanceToPlayer(float distance)
        {
            _currentDistance = distance;
        }

        public void Disable()
        {
            _isEnabled = false;
        }
    }
}