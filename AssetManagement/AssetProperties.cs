using System.Collections;
using System.Reflection;
using System.Windows.Controls;

namespace Shiftless.Clockwork.Assets.Editor.AssetManagement
{
    public readonly struct AssetProperties : IEnumerable<string>
    {
        private readonly Dictionary<string, object> _properties;


        public AssetProperties(params (string, object)[] properties)
        {
            _properties = [];

            foreach ((string name, object value) in properties)
            {
                _properties.Add(name, value);
            }
        }

        public AssetProperties(object obj)
        {
            _properties = [];
            PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
                _properties.Add(property.Name, property.GetValue(obj) ?? "null");

            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
                _properties.Add(field.Name, field.GetValue(obj) ?? "null");
        }

        public StackPanel GetPanel()
        {
            StackPanel panel = new StackPanel();
            foreach ((string name, object value) in _properties)
            {
                StackPanel propPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };

                propPanel.Children.Add(new TextBlock()
                {
                    Text = name + ':',
                    Width = 100
                });
                propPanel.Children.Add(new TextBlock()
                {
                    Text = value.ToString(),
                    Width = 150
                });

                panel.Children.Add(propPanel);
            }

            return panel;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator() => new AssetPropertyEnumerator(_properties);
        IEnumerator IEnumerable.GetEnumerator() => new AssetPropertyEnumerator(_properties);
    }
}
