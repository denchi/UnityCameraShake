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
}