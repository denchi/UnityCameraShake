using System.Collections.Generic;
using UnityEngine;

namespace CameraShake
{
    public class CameraShakeController : MonoBehaviour
    {
        private readonly List<ICameraShakeSource> _activeShakes = new();
        private readonly Dictionary<string, ProximityShake> _proximityShakes = new();
        private Vector3 _positionOffset;
        private Vector3 _rotationOffset;
        private ICameraShakeSetter _cameraShakeSetter;
        
        //
        
        public void AddShake(ICameraShakeSource source)
        {
            _activeShakes.Add(source);
        }

        public ICameraShakeSource AddShake(CameraShakeAsset asset, Vector3? direction = null, float distanceToPlayer = 0f)
        {
            ICameraShakeSource shake = null;
            switch (asset.shakeType)
            {
                case CameraShakeAsset.ShakeType.Impulse:
                    if (direction.HasValue)
                    {
                        shake = ImpulseShake.FromDirection(
                            direction.Value,
                            asset.positionMagnitude,
                            asset.rotationMagnitude,
                            asset.duration,
                            distanceToPlayer,
                            asset.maxDistance,
                            asset.dampingCurve
                        );
                    }
                    else
                    {
                        shake = new ImpulseShake(
                            asset.positionImpulse,
                            asset.rotationImpulse,
                            asset.duration,
                            asset.distanceFalloff,
                            asset.dampingCurve
                        );
                    }
                    break;
                case CameraShakeAsset.ShakeType.Perlin:
                    shake = new PerlinShake(
                        asset.duration,
                        asset.intensity,
                        asset.distanceFalloff,
                        asset.dampingCurve
                    );
                    break;
                case CameraShakeAsset.ShakeType.Trauma:
                    shake = new TraumaShake(
                        asset.trauma,
                        asset.decayRate,
                        asset.distanceFalloff,
                        asset.dampingCurve
                    );
                    break;
                case CameraShakeAsset.ShakeType.Proximity:
                    shake = new ProximityShake(
                        asset.proximityMaxDistance,
                        asset.proximityMinDistance,
                        asset.proximityIntensity,
                        asset.proximityFrequency
                    );
                    break;
                // Add more shake types as needed
            }
            if (shake != null)
                _activeShakes.Add(shake);

            return shake;
        }

        // Proximity shake management
        public void AddProximityShake(string id, CameraShakeAsset asset)
        {
            if (asset.shakeType != CameraShakeAsset.ShakeType.Proximity)
                return;
            var shake = new ProximityShake(
                asset.proximityMaxDistance,
                asset.proximityMinDistance,
                asset.proximityIntensity,
                asset.proximityFrequency
            );
            _proximityShakes[id] = shake;
            _activeShakes.Add(shake);
        }

        public void UpdateProximityShakeDistance(string id, float distance)
        {
            if (_proximityShakes.TryGetValue(id, out var shake))
                shake.UpdateDistanceToPlayer(distance);
        }

        public void DisableProximityShake(string id)
        {
            if (_proximityShakes.TryGetValue(id, out var shake))
            {
                shake.Disable();
                _proximityShakes.Remove(id);
            }
        }
        
        public void SetCameraShakeSetter(ICameraShakeSetter source)
        {
            _cameraShakeSetter = source;
        }
        
        //
        
        private void LateUpdate()
        {
            Vector3 totalPos = Vector3.zero;
            Vector3 totalRot = Vector3.zero;

            foreach (var shake in _activeShakes)
            {
                shake.UpdateShake(Time.deltaTime);
                
                totalPos += shake.PositionOffset;
                totalRot += shake.RotationOffset;
            }

            _positionOffset = totalPos;
            _rotationOffset = totalRot;

            // Apply to actual camera
            ApplyShake();
            Cleanup();
        }

        private void ApplyShake()
        {
            if (_cameraShakeSetter == null) 
                return;
            
            _cameraShakeSetter.SetShakePositionOffset(_positionOffset);
            _cameraShakeSetter.SetShakeRotationOffset(_rotationOffset);
        }

        private void Cleanup()
        {
            _activeShakes.RemoveAll(s => s.IsFinished);
        }

        public void RemoveShake(ICameraShakeSource source)
        {
            _activeShakes.Remove(source);
        }
    }
}