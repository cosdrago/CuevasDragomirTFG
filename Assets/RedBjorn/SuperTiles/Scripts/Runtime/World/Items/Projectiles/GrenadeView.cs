using RedBjorn.Utils;
using System;
using System.Collections;
using UnityEngine;

namespace RedBjorn.SuperTiles.Items.Projectiles
{
    /// <summary>
    /// Projectile view with parabolic movement trajectory from instantiated point to target point
    /// </summary>
    public class GrenadeView : ProjectileView
    {
        public float InitSpeedXZ = 1f;
        public GameObject ModelPrefab;
        public GameObject FXPrefab;
        public float FXDuration = 2f;

        const float constG = 9.8f;

        GameObject Model;
        GameObject FX;

        Vector3 Target;

        Action OnReached;
        Action OnDestroyed;

        void Awake()
        {
            if (ModelPrefab)
            {
                Model = Spawner.Spawn(ModelPrefab, transform);
                Model.transform.localPosition = Vector3.zero;
                Model.transform.localRotation = Quaternion.identity;
                Model.SetActive(false);
            }

            if (FXPrefab)
            {
                FX = Spawner.Spawn(FXPrefab, transform);
                FX.transform.localPosition = Vector3.zero;
                FX.transform.localRotation = Quaternion.identity;
                FX.SetActive(false);
            }
        }

        public override void FireTarget(Vector3 target, float speed, Action onReached, Action onDestroyed)
        {
            if (Mathf.Abs(speed) > 0.0001f)
            {
                InitSpeedXZ = speed;
            }
            Target = target;
            OnReached = onReached;
            OnDestroyed = onDestroyed;
            StartCoroutine(GrenadeLife());
        }

        IEnumerator GrenadeLife()
        {
            if (Model)
            {
                Model.SetActive(true);
            }
            var origin = transform.position;
            var xDelta = Target.x - origin.x;
            var zDelta = Target.z - origin.z;
            var xzDistance = Mathf.Sqrt(xDelta * xDelta + zDelta * zDelta);
            var maxtime = xzDistance / InitSpeedXZ;
            var speedX = InitSpeedXZ * xDelta / xzDistance;
            var speedZ = InitSpeedXZ * zDelta / xzDistance;
            var speedY = (constG / 2f * maxtime * maxtime - origin.y) / maxtime;

            var time = 0f;
            while (time <= maxtime)
            {
                var oldPosition = transform.position;
                transform.position = origin + new Vector3(speedX * time, speedY * time - constG / 2f * time * time, speedZ * time);
                var step = transform.position - oldPosition;
                transform.localRotation = Quaternion.Euler(0f, Mathf.Atan2(step.x, step.z) * Mathf.Rad2Deg, 0f);
                if (Model)
                {
                    var stepXZ = Mathf.Sqrt(step.x * step.x + step.z * step.z);
                    Model.transform.localRotation = Quaternion.Euler(-Mathf.Atan2(step.y, stepXZ) * Mathf.Rad2Deg, 0f, 0f);
                }
                yield return null;
                time += Time.deltaTime;
            }
            if (Model)
            {
                Model.SetActive(false);
            }
            transform.position = Target;
            OnReached.SafeInvoke();
            if (FX)
            {
                FX.SetActive(true);
            }
            yield return new WaitForSeconds(FXDuration);
            Spawner.Despawn(gameObject);
            OnDestroyed.SafeInvoke();
        }
    }
}