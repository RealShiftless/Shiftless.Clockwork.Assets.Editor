using System.Windows.Controls;
using System.Windows.Input;

namespace Shiftless.Clockwork.Assets.Editor.UserControls.Settings
{
    /// <summary>
    /// Interaction logic for Int32Control.xaml
    /// </summary>
    public partial class Int32Control : UserControl, ISettingControl
    {
        // Events
        public event Action<object>? ValueChanged;


        // Constructor
        public Int32Control()
        {
            InitializeComponent();
        }


        // Func
        public UserControl AsControl() => this;

        public void Awake(object obj)
        {
            if (obj is not int value)
                throw new Exception(); // TODO: better exception

            Body.Text = value.ToString();
        }

        private void Body_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
                e.Handled = true;
        }

        private void Body_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValueChanged?.Invoke(int.Parse(Body.Text));
        }
    }
}
