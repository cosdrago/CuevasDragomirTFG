using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// RedBjorn.Utils.ILogger implementation which wraps UnityEngine.Debug.Log with customizable enable/disable logic
    /// </summary>
    public abstract class Logger : RedBjorn.Utils.ILogger
    {
        public string Prefix;
        public virtual bool IsEnabled => true;

        public void SetPrefix(string prefix)
        {
            Prefix = prefix;
        }

        public void Error(object message)
        {
            if (IsEnabled && S.Log.Error)
            {
                Debug.LogError(Prefix + message);
            }
        }

        public void Info(object message)
        {
            if (IsEnabled && S.Log.Info)
            {
                Debug.Log(Prefix + message);
            }
        }

        public void Warning(object message)
        {
            if (IsEnabled && S.Log.Warning)
            {
                Debug.LogWarning(Prefix + message);
            }
        }
    }
}
