using RedBjorn.Utils;
using System;
using System.Collections;
using UnityEngine;

namespace RedBjorn.SuperTiles.Items.Projectiles
{
    /// <summary>
    /// Projectile view with staightforward movement behaviour from instantiated point to target point
    /// </summary>
    public class BulletView : ProjectileView
    {
        public float Speed = 1f;

        Vector3 Target;
        Action OnReached;
        Action OnDestroyed;

        public override void FireTarget(Vector3 target, float speed, Action onReached, Action onDestroyed)
        {
            Speed = speed;
            if (Mathf.Abs(speed) < 0.0001f)
            {
                Speed = 10f;
            }
            Target = target;
            OnReached = onReached;
            OnDestroyed = onDestroyed;
            StartCoroutine(ReachTarget());
        }

        IEnumerator ReachTarget()
        {
            var originDirection = (Target - transform.position).normalized;
            var currentDirection = originDirection;
            var step = originDirection * Speed;
            while (Vector3.Dot(originDirection, currentDirection) > 0)
            {
                yield return null;
                transform.position += step * Time.deltaTime;
                currentDirection = Target - transform.position;
            }
            OnReached.SafeInvoke();
            Spawner.Despawn(gameObject);
            OnDestroyed.SafeInvoke();
        }
    }
}
