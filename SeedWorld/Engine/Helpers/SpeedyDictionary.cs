using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    public sealed class SpeedyDictionary
    {
        private Dictionary<Vector3, object> cache = new Dictionary<Vector3, object>();
        private Vector3 key = Vector3.One;
        private object value = null;

        public void Add(Vector3 key, object value)
        {
            cache.Add(key, value);
        }

        public void Remove(Vector3 key)
        {
            cache.Remove(key);
        }

        public object Get(Vector3 key)
        {
            // check if the keys match.
            if (this.key == key)
                return this.value;

            // get the value if the keys didn’t match.
            object value = null;
            cache.TryGetValue(key, out value);

            // store the current item as last item.
            this.key = key;
            this.value = value;

            return value;
        }
    }
}
