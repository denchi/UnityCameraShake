# Unity Camera Shake

A Unity package that runs camera-shake sources and applies their combined position and rotation offset to a transform.

`CameraShakeController` stores the active sources, advances each one in `LateUpdate`, sums `PositionOffset` and `RotationOffset`, and passes the totals to an `ICameraShakeSetter`. The included `CameraShakeSetter` assigns those totals to a transform's `localPosition` and `localEulerAngles`. Sources whose `IsFinished` is true are removed after they are applied.

Package id `com.deathbygravitystudio.shake`, version 0.1.0. Namespace `CameraShake`. Assembly `DeathByGravity.Shake`.

## Installation

In the Unity Editor, open **Window > Package Manager**, select **+ > Add package from git URL**, and enter:

```
https://github.com/denchi/UnityCameraShake.git
```

Or add the same URL to `Packages/manifest.json`:

```json
"com.deathbygravitystudio.shake": "https://github.com/denchi/UnityCameraShake.git"
```

## Usage

1. Create an asset with **Create > Camera Shake > Shake Asset**. Set `shakeType` and the fields for that type.
2. Add `CameraShakeController` to a GameObject.
3. Add `CameraShakeSetter` to the transform that should receive the shake. It assigns `localPosition` and `localEulerAngles` to the offset each frame, so put it on a transform whose local pose is reserved for shake, such as a child of the camera.
4. Call `SetCameraShakeSetter`. With no setter, the controller still updates sources and then skips applying the offset.

```csharp
using CameraShake;
using UnityEngine;

public class CameraShakeExample : MonoBehaviour
{
    [SerializeField] CameraShakeController controller;
    [SerializeField] CameraShakeSetter setter;
    [SerializeField] CameraShakeAsset impulseAsset;
    [SerializeField] CameraShakeAsset proximityAsset;

    const string ProximityId = "rumble";

    void Awake()
    {
        controller.SetCameraShakeSetter(setter);
    }

    // `direction` is normalized by ImpulseShake.FromDirection.
    // `distanceToPlayer` changes strength only when the asset's maxDistance is greater than 0.
    public void PlayDirectionalImpulse(Vector3 direction, float distanceToPlayer)
    {
        controller.AddShake(impulseAsset, direction, distanceToPlayer);
    }

    // No direction: the asset's positionImpulse and rotationImpulse are used.
    public void PlayVectorImpulse()
    {
        controller.AddShake(impulseAsset);
    }

    public void BeginProximity()
    {
        controller.AddProximityShake(ProximityId, proximityAsset);
    }

    public void SetProximityDistance(float distance)
    {
        controller.UpdateProximityShakeDistance(ProximityId, distance);
    }

    public void EndProximity()
    {
        controller.DisableProximityShake(ProximityId);
    }
}
```

`AddShake(CameraShakeAsset asset, Vector3? direction = null, float distanceToPlayer = 0f)` builds a source from the asset and returns it. `direction` and `distanceToPlayer` are read only when `shakeType` is `Impulse`. Any `ICameraShakeSource` can be added directly:

```csharp
controller.AddShake(new ImpulseShake(
    positionImpulse: new Vector3(0.15f, 0.05f, 0f),
    rotationImpulse: new Vector3(1f, 0f, 2f),
    duration: 0.4f,
    distanceFalloff: 1f));

controller.AddShake(new PerlinShake(duration: 0.6f, intensity: 1f, distanceFalloff: 1f));
controller.AddShake(new TraumaShake(initialTrauma: 0.8f, decayRate: 1.5f, distanceFalloff: 1f));
controller.AddShake(new ProximityShake(
    maxDistance: 10f,
    minDistance: 0.5f,
    maxIntensity: 1f,
    frequency: 4f));
```

`ImpulseShake`, `PerlinShake`, and `TraumaShake` each take an optional `AnimationCurve`. Passing null keeps the default linear curve. `RemoveShake` takes a source off the active list. A custom applier implements `ICameraShakeSetter` (`SetShakePositionOffset` and `SetShakeRotationOffset`) and is passed to `SetCameraShakeSetter`.

## Shake types

Offsets from every active source are added together.

### Impulse

Holds an offset for `duration` seconds (at least 0.01) and scales it by `dampingCurve` over normalized time. The default curve is linear from 1 to 0. The source finishes when the timer reaches `duration`.

- **Directional** (`AddShake(asset, direction, distanceToPlayer)` or `ImpulseShake.FromDirection`): position offset is the normalized direction times `positionMagnitude`. Rotation is around the cross product of that direction and world up (world right when the cross product is zero), with length `rotationMagnitude`. When `maxDistance` is greater than 0, both are multiplied by `clamp01(1 - distanceToPlayer / maxDistance)`. The asset's `distanceFalloff` is not used.
- **Vector** (`AddShake(asset)` or `new ImpulseShake`): uses `positionImpulse` and `rotationImpulse` (degrees). The curve result is multiplied by `distanceFalloff`, which is a scalar, not a distance.

### Perlin

Perlin noise for `duration` seconds, scaled by `intensity`, `distanceFalloff`, and the damping curve. The curve is sampled with `Evaluate(1 - t)`, so the default curve starts at 0 and reaches full strength at the end. Position is Perlin noise centered on zero. Rotation is a new random unit vector each update, length 0.5, and is not multiplied by intensity or the curve. The source finishes when the timer reaches `duration`.

### Trauma

`trauma` is clamped to 0–1 and decreases by `decayRate` each second. Position (X/Y) and roll scale with trauma squared, Perlin noise, `distanceFalloff`, and `dampingCurve.Evaluate(1 - clamp01(trauma))`. The asset's `duration` is not used. The source finishes when trauma drops to 0 or below.

### Proximity

Continuous noise that stays active until disabled. Strength is `proximityIntensity` at or inside `proximityMinDistance`, 0 at or beyond `proximityMaxDistance`, and linear in between. `proximityFrequency` is the noise frequency. Position noise is on X/Y and rotation is roll.

`AddProximityShake(string id, CameraShakeAsset asset)` registers the shake under `id` only when `asset.shakeType` is `Proximity`. Update it with `UpdateProximityShakeDistance(string id, float distance)` and stop it with `DisableProximityShake(string id)`. Distance starts at the max distance, so strength is 0 until the first update.

`AddShake` on a proximity asset creates a `ProximityShake` that is not stored by id. `UpdateProximityShakeDistance` and `DisableProximityShake` will not find it.

`ProximityShake` raises `maxDistance` to at least 0.1 and clamps `minDistance` to 0.1 through `maxDistance`.

## Asset fields

| Field | Default | Used by |
| --- | --- | --- |
| `duration` | `0.5` | Impulse, Perlin |
| `dampingCurve` | linear, 1 → 0 | Impulse, Perlin, Trauma |
| `distanceFalloff` | `1` | Vector impulse, Perlin, Trauma |
| `maxDistance` | `0` | Directional impulse (`0` leaves strength unchanged) |
| `positionImpulse` | `(0, 0, 0)` | Vector impulse |
| `rotationImpulse` | `(0, 0, 0)` | Vector impulse, degrees |
| `positionMagnitude` | `1` | Directional impulse |
| `rotationMagnitude` | `1` | Directional impulse |
| `intensity` | `1` | Perlin |
| `trauma` | `1` | Trauma, clamped to 0–1 |
| `decayRate` | `1` | Trauma, units per second |
| `proximityMinDistance` | `0.5` | Proximity |
| `proximityMaxDistance` | `10` | Proximity |
| `proximityIntensity` | `1` | Proximity |
| `proximityFrequency` | `4` | Proximity |

## Licence

Released under the [MIT License](LICENSE). Copyright 2026 Denis Chiriac / Death By Gravity Studio.
