using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Alarm.Core
{
    public partial class Application : IDictionary<string, object>
    {
        public Dictionary<string, object> Env = [];

        public object this[string key] { get => ((IDictionary<string, object>)Env)[key]; set => ((IDictionary<string, object>)Env)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, object>)Env).Keys;

        public ICollection<object> Values => ((IDictionary<string, object>)Env).Values;

        public int Count => ((ICollection<KeyValuePair<string, object>>)Env).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, object>>)Env).IsReadOnly;

        public void Add(string key, object value)
        {
            ((IDictionary<string, object>)Env).Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>)Env).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, object>>)Env).Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)Env).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, object>)Env).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)Env).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)Env).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, object>)Env).Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)Env).Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return ((IDictionary<string, object>)Env).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Env).GetEnumerator();
        }
    }
}
