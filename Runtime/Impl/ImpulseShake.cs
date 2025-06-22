using UnityEngine;

namespace CameraShake
{
    public class ImpulseShake : ICameraShakeSource, ITimedShake
    {
        private Vector3 initialPositionImpulse;
        private Vector3 initialRotationImpulse;
        private float duration;
        private float timer;
        private float distanceFalloff = 1f;
        private AnimationCurve dampingCurve = AnimationCurve.Linear(0, 1, 1, 0);

        public Vector3 PositionOffset { get; private set; }
        public Vector3 RotationOffset { get; private set; }
        public bool IsFinished => timer >= duration;
        public float Duration => duration;
        public float TimeLeft { get; set; }

        /// <summary>
        /// Creates an impulse shake.
        /// </summary>
        /// <param name="positionImpulse">Initial position impulse vector.</param>
        /// <param name="rotationImpulse">Initial rotation impulse vector (degrees).</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="distanceFalloff">Distance falloff factor.</param>
        /// <param name="dampingCurve">Custom damping curve (optional).</param>
        public ImpulseShake(
            Vector3 positionImpulse,
            Vector3 rotationImpulse,
            float duration,
            float distanceFalloff = 1f,
            AnimationCurve dampingCurve = null)
        {
            this.initialPositionImpulse = positionImpulse;
            this.initialRotationImpulse = rotationImpulse;
            this.duration = Mathf.Max(0.01f, duration);
            this.TimeLeft = this.duration;
            this.timer = 0f;
            this.distanceFalloff = distanceFalloff;
            if (dampingCurve != null)
                this.dampingCurve = dampingCurve;
        }

        /// <summary>
        /// Creates a direction-based impulse shake.
        /// </summary>
        /// <param name="direction">Normalized direction vector (e.g., from explosion to camera).</param>
        /// <param name="magnitude">Strength of the shake (position).</param>
        /// <param name="rotationMagnitude">Strength of the rotational shake.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="distanceToPlayer">Distance from the source to the player (for falloff calculation).</param>
        /// <param name="maxDistance">Maximum distance for falloff (0 for no falloff).</param>
        /// <param name="dampingCurve">Custom damping curve (optional).</param>
        public static ImpulseShake FromDirection(
            Vector3 direction,
            float magnitude,
            float rotationMagnitude,
            float duration,
            float distanceToPlayer = 0f,
            float maxDistance = 0f,
            AnimationCurve dampingCurve = null)
        {
            float falloff = 1f;
            if (maxDistance > 0f)
                falloff = Mathf.Clamp01(1f - (distanceToPlayer / maxDistance));
            Vector3 posImpulse = direction.normalized * magnitude * falloff;
            Vector3 rotAxis = Vector3.Cross(direction.normalized, Vector3.up);
            if (rotAxis == Vector3.zero) rotAxis = Vector3.right;
            Vector3 rotImpulse = rotAxis.normalized * rotationMagnitude * falloff;
            return new ImpulseShake(posImpulse, rotImpulse, duration, falloff, dampingCurve);
        }

        public void UpdateShake(float deltaTime)
        {
            timer += deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float decay = dampingCurve.Evaluate(t) * distanceFalloff;

            PositionOffset = initialPositionImpulse * decay;
            RotationOffset = initialRotationImpulse * decay;
        }
    }
}