using UnityEngine;

namespace CameraShake
{
    public interface ICameraShakeSource
    {
        Vector3 PositionOffset { get; }
        Vector3 RotationOffset { get; }
        bool IsFinished { get; }
        void UpdateShake(float deltaTime);
    }

    // Interface for time-based shakes
    public interface ITimedShake
    {
        float TimeLeft { get; set; }
        float Duration { get; }
    }
}
