using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player
{
    public class LayersWithDefValue<TVal>
    {
        private List<Layer<TVal>> layers;

        public TVal LastValue { get; private set; }
        public string LastId { get; private set; }
        public string Name { get; private set; }

        public LayersWithDefValue(TVal _constantValue, string _name = "")
        {
            layers = new List<Layer<TVal>>();
            Name = _name;
            Override("Default", short.MaxValue, _constantValue);
        }

        public void Override(string _key, short _priority, TVal _value)
        {
            if (IsOverridenWithKey(_key))
            {
                Debug.Log("Key already exists (skipping Add):" + _key + " " + ToString());
                return;
            }
            layers.Add(new Layer<TVal>(_key, _priority, _value));

            SortLayersAndGetLasts();
        }

        public bool Contains(string _key)
        {
            return layers.Find(x => x.Key == _key) != null;
        }

        public void Release(string _key)
        {
            if (!IsOverridenWithKey(_key))
            {
                Debug.Log("Key does not exists (skipping remove):" + _key + " " + ToString());
                return;
            }

            if (layers.Count == 1)
            {
                Debug.Log("You have tried to remove constant key, this is not allowed");
                return;
            }

            layers.Remove(layers.Find(x => x.Key == _key));
            SortLayersAndGetLasts();
        }

        private void SortLayersAndGetLasts()
        {
            layers = layers.OrderByDescending(x => x.Priority).ToList();
            LastValue = layers.Last().Value;
            LastId = layers.Last().Key;
        }

        public void Modify(string _key, TVal _value)
        {
            if (!IsOverridenWithKey(_key))
            {
                Debug.Log("Key does not exists (skipping modify):" + _key + " " + ToString());
                return;
            }

            layers.Find(x => x.Key == _key).Value = _value;
            SortLayersAndGetLasts();
        }

        public bool IsOverridenWithKey(string id)
        {
            return layers.Find(x => x.Key == id) != null;
        }

        private class Layer<Tval>
        {
            public short Priority { get; private set; }
            public string Key { get; private set; }
            public Tval Value { get; set; }

            public Layer(string _key, short _priority, Tval _value)
            {
                Priority = _priority;
                Key = _key;
                Value = _value;
            }
        }
    }
}