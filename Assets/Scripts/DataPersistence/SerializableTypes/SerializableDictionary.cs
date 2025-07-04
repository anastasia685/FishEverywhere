using UnityEngine;
using System.Collections.Generic;

namespace Serializable
{
    [System.Serializable]
    public class Dictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] List<TKey> keys = new List<TKey>();
        [SerializeField] List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach(KeyValuePair<TKey, TValue> kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
        public void OnAfterDeserialize()
        {
            this.Clear();

            for(int i = 0; i < keys.Count; i++)
            {
                this.Add(keys[i], values[i]);
            }
        }
    }
}
