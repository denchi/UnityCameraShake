using UnityEngine;

namespace CameraShake
{
    [CreateAssetMenu(menuName = "Camera Shake/Shake Asset")]
    public class CameraShakeAsset : ScriptableObject
    {
        public enum ShakeType { Impulse, Perlin, Trauma, Proximity }
        public ShakeType shakeType;

        // Common
        [Header("Common Settings")]
        public float duration = 0.5f;
        public AnimationCurve dampingCurve = AnimationCurve.Linear(0, 1, 1, 0);
        public float distanceFalloff = 1f;
        public float maxDistance = 0f;

        // Impulse
        [Header("Impulse Settings")]
        public Vector3 positionImpulse;
        public Vector3 rotationImpulse;
        public float positionMagnitude = 1f;
        public float rotationMagnitude = 1f;

        // Perlin
        [Header("Perlin Settings")]
        public float intensity = 1f;

        // Trauma
        [Header("Trauma Settings")]
        public float trauma = 1f;
        public float decayRate = 1f;

        // Proximity
        [Header("Proximity Settings")]
        public float proximityMinDistance = 0.5f;
        public float proximityMaxDistance = 10f;
        public float proximityIntensity = 1f;
        public float proximityFrequency = 4f;
    }
}