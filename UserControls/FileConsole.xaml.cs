using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Shiftless.Clockwork.Assets.Editor.UserControls
{
    /// <summary>
    /// Interaction logic for FileConsole.xaml
    /// </summary>
    public partial class FileConsole : UserControl
    {
        public FileConsole()
        {
            InitializeComponent();
        }

        public void WriteLine(string line)
        {
            Debug.WriteLine(line);

            Body.Children.Add(new TextBlock() { Text = $"[{DateTime.Now:HH:mm:ss}] {line}" });
            ScrollView.ScrollToEnd();
        }

        internal void Collapse()
        {
            ScrollView.Visibility = ScrollView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            ShowHideButton.Content = ScrollView.Visibility == Visibility.Visible ? "Hide" : "Show";
        }

        private void ShowHideButton_Click(object sender, RoutedEventArgs e)
        {
            Collapse();
        }
    }
}
