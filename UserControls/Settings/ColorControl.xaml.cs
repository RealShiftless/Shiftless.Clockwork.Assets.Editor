using Shiftless.Clockwork.Assets.Editor.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Shiftless.Clockwork.Assets.Editor.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for ColorControl.xaml
    /// </summary>
    public partial class ColorControl : UserControl, ISettingControl
    {
        // Events
        public event Action<object>? ValueChanged;


        // Constructor
        public ColorControl()
        {
            InitializeComponent();

            Body.TextChanged += Body_TextChanged;
        }

        private void Body_TextChanged(object sender, TextChangedEventArgs e)
        {
            Mathematics.Color color;

            string text = Body.Text;

            if (text.Length == 1)
            {
                byte c = Convert.ToByte(text + text, 16);

                color = new(c, c, c, 255);
            }
            else if (text.Length == 2)
            {
                byte c = Convert.ToByte(text, 16);

                color = new(c, c, c, 255);
            }
            else if (text.Length == 3)
            {
                byte r = Convert.ToByte($"{text[0]}{text[0]}", 16);
                byte g = Convert.ToByte($"{text[1]}{text[1]}", 16);
                byte b = Convert.ToByte($"{text[2]}{text[2]}", 16);

                color = new(r, g, b, 255);
            }
            else if (text.Length == 4)
            {
                byte r = Convert.ToByte($"{text[0]}{text[0]}", 16);
                byte g = Convert.ToByte($"{text[1]}{text[1]}", 16);
                byte b = Convert.ToByte($"{text[2]}{text[2]}", 16);
                byte a = Convert.ToByte($"{text[3]}{text[3]}", 16);

                color = new(r, g, b, a);
            }
            else if (text.Length == 6)
            {
                byte r = Convert.ToByte(text[0..2], 16);
                byte g = Convert.ToByte(text[2..4], 16);
                byte b = Convert.ToByte(text[4..6], 16);

                color = new(r, g, b, 255);
            }
            else if (text.Length == 8)
            {
                byte r = Convert.ToByte(text[0..2], 16);
                byte g = Convert.ToByte(text[2..4], 16);
                byte b = Convert.ToByte(text[4..6], 16);
                byte a = Convert.ToByte(text[6..8], 16);

                color = new(r, g, b, a);
            }
            else
            {
                return;
            }

            int index = Body.CaretIndex;
            Body.Text = Body.Text.ToUpper();
            Body.CaretIndex = index;

            ColorBorder.BorderBrush = new SolidColorBrush(new Mathematics.Color(0, 0, 0, color.A));
            ColorBorder.Background = new SolidColorBrush(color);
            ValueChanged?.Invoke(color);
        }


        // Func
        public UserControl AsControl() => this;

        public void Awake(object obj)
        {
            if (obj is not Mathematics.Color value)
                throw new Exception(); // TODO: better exception

            Body.Text = value.ToString();
            ColorBorder.Background = new SolidColorBrush(value);
        }

        private void Body_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!Regex.IsMatch(e.Text, "^[0-9a-fA-F]+$"))
                e.Handled = true;

        }

        private void Body_TextInput(object sender, TextCompositionEventArgs e)
        {
            
        }
    }
}
