using RedBjorn.Utils;
using System;
using UnityEngine;

namespace RedBjorn.SuperTiles.UI
{
    public class TurnPlayerOtherUI : MonoBehaviour
    {
        public float Delay = 2f;
        Action OnCompleted;

        void Start()
        {
            Invoke("Despawn", Delay);
        }

        public static void Show(Action onCompleted = null)
        {
            if (S.Battle.UI.TurnStartShowOther)
            {
                Spawner.Spawn(S.Prefabs.TurnStartOtherPanel).Init(onCompleted);
            }
            else
            {
                onCompleted.SafeInvoke();
            }
        }

        public void Init(Action onCompleted)
        {
            OnCompleted = onCompleted;
        }

        void Despawn()
        {
            Spawner.Despawn(gameObject);
            OnCompleted.SafeInvoke();
        }
    }
}
