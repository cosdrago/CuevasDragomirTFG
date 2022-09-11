using RedBjorn.Utils;
using System;
using System.Collections;
using UnityEngine;

namespace RedBjorn.SuperTiles.Items.Projectiles
{
    /// <summary>
    /// Projectile view with ray movement behaviour
    /// </summary>
    public class LaserRayView : ProjectileView
    {
        public float ReachDelay;
        public float Speed = 10f;

        Action OnReached;
        Action OnDestroyed;

        public override void FireTarget(Vector3 target, float speed, Action onReached, Action onDestroyed)
        {
            Speed = speed;
            if (Mathf.Abs(Speed) > 0.0001f)
            {
                Speed = 10f;
            }
            ReachDelay = (transform.position - target).magnitude / Speed;
            OnReached = onReached;
            OnDestroyed = onDestroyed;
            StartCoroutine(RayLife());
        }

        IEnumerator RayLife()
        {
            var duration = ReachDelay;
            while (duration > 0)
            {
                yield return null;
                duration -= Time.deltaTime;
                transform.position += transform.forward * Time.deltaTime * Speed;
            }
            OnReached.SafeInvoke();
            Spawner.Despawn(gameObject);
            OnDestroyed.SafeInvoke();
        }
    }
}