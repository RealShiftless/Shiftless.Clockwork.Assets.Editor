using System.Collections;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement
{
    public class AssetPropertyEnumerator : IEnumerator<string>
    {
        private Dictionary<string, object> _properties;
        private int _index = -1
            ;
        public KeyValuePair<string, object> Entry => _properties.ElementAt(_index);
        public string Current => $"{Entry.Key}: {Entry.Value}";

        object IEnumerator.Current => Current;

        internal AssetPropertyEnumerator(Dictionary<string, object> properties)
        {
            _properties = properties;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            _index++;
            return _index < _properties.Count;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
