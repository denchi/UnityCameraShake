using UnityEngine;

namespace CameraShake
{
    public class PerlinShake : ICameraShakeSource
    {
        float duration, timer, intensity;
        Vector3 seed;
        private float distanceFalloff = 1f;
        private AnimationCurve dampingCurve = AnimationCurve.Linear(0, 1, 1, 0);

        public Vector3 PositionOffset { get; private set; }
        public Vector3 RotationOffset { get; private set; }
        public bool IsFinished => timer >= duration;

        public PerlinShake(float duration, float intensity, float distanceFalloff = 1f, AnimationCurve dampingCurve = null)
        {
            this.duration = duration;
            this.intensity = intensity;
            this.seed = new Vector3(Random.value, Random.value, Random.value);
            this.distanceFalloff = distanceFalloff;
            if (dampingCurve != null)
                this.dampingCurve = dampingCurve;
        }

        public void UpdateShake(float deltaTime)
        {
            float m = 0.5f;
            timer += deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float decay = dampingCurve.Evaluate(1f - t) * distanceFalloff;

            float noiseX = Mathf.PerlinNoise(seed.x, Time.time * m);
            float noiseY = Mathf.PerlinNoise(seed.y, Time.time * m);
            float noiseZ = Mathf.PerlinNoise(seed.z, Time.time * m);

            PositionOffset = new Vector3(noiseX - 0.5f, noiseY - 0.5f, noiseZ - 0.5f) * (intensity * decay);
            RotationOffset = Random.onUnitSphere * m; // optional rotation noise
        }
    }
}