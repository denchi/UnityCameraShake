using UnityEngine;

namespace CameraShake
{
    public class CameraShakeSetter : MonoBehaviour, ICameraShakeSetter
    {
        public void SetShakePositionOffset(Vector3 offset)
        {
            transform.localPosition = offset;
        }

        public void SetShakeRotationOffset(Vector3 offset)
        {
            transform.localEulerAngles = offset;
        }
    }
}