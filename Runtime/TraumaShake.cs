using UnityEngine;

namespace CameraShake
{
    public class TraumaShake : ICameraShakeSource
    {
        float trauma;
        float decayRate;
        private float distanceFalloff = 1f;
        private AnimationCurve dampingCurve = AnimationCurve.Linear(0, 1, 1, 0);

        Vector3 seed = new(Random.value, Random.value, Random.value);
        public bool IsFinished => trauma <= 0f;

        public Vector3 PositionOffset { get; private set; }
        public Vector3 RotationOffset { get; private set; }

        public TraumaShake(float initialTrauma, float decayRate = 1f, float distanceFalloff = 1f, AnimationCurve dampingCurve = null)
        {
            this.trauma = Mathf.Clamp01(initialTrauma);
            this.decayRate = decayRate;
            this.distanceFalloff = distanceFalloff;
            if (dampingCurve != null)
                this.dampingCurve = dampingCurve;
        }

        public void UpdateShake(float deltaTime)
        {
            float shake = trauma * trauma;
            trauma -= decayRate * deltaTime;

            float t = 1f - Mathf.Clamp01(trauma);
            float decay = dampingCurve.Evaluate(t) * distanceFalloff;

            float noiseX = Mathf.PerlinNoise(seed.x, Time.time * 30f) * 2 - 1;
            float noiseY = Mathf.PerlinNoise(seed.y, Time.time * 30f) * 2 - 1;

            PositionOffset = new Vector3(noiseX, noiseY, 0f) * shake * 0.5f * decay;
            RotationOffset = new Vector3(0f, 0f, noiseX * shake * 2f * decay);
        }
    }
}