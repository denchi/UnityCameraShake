using UnityEngine;

namespace CameraShake
{
    public interface ICameraShakeSetter
    {
        void SetShakePositionOffset(Vector3 offset);    
        void SetShakeRotationOffset(Vector3 offset);    
    }
}