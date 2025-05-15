using Microsoft.Win32;
using Shiftless.Clockwork.Assets.Editor.AssetManagement;
using Shiftless.Clockwork.Assets.Editor.AssetManagement.Builders;
using Shiftless.Clockwork.Assets.Editor.AssetManagement.Sorting;
using Shiftless.Clockwork.Assets.Editor.UserControls.Settings;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Shiftless.Clockwork.Assets.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string VERSION = "inDev 1";

        Project? _project = null;

        private Project? Project
        {
            get => _project;
            set
            {
                _project = value;

                if (_project == null)
                    SaveMenuItem.IsEnabled = false;
                else
                    SaveMenuItem.IsEnabled = true;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            FileConsole.WriteLine($"Clockwork.Assets Editor {VERSION} initialized!");

            string[] args = Environment.GetCommandLineArgs();
            string? dir = null;
            foreach (string arg in args)
            {
                if (!arg.EndsWith(".cwat"))
                    continue;

                dir = Path.GetDirectoryName(arg) ?? throw new Exception("Invalid directory?!");
                FileConsole.WriteLine($"Loading project at {dir}");
                Project = new Project(FileConsole, dir);
                break;
            }

            Activated += MainWindow_Activated;
            DirectoryView.SelectedItemChanged += DirectoryView_SelectedItemChanged;

            TitleBarCloseButton.Background.Opacity = 0;
            TitleBarMaximizeButton.Background.Opacity = 0;
            TitleBarMinimizeButton.Background.Opacity = 0;

            FileConsole.Collapse();
        }

        private void RefreshFileTree()
        {
            if (!Project.IsActive) return;

            FileConsole.WriteLine("Refreshing file tree...");

            Project.Assets.Refresh();
            Project.Assets.Sort(SortModes.Alphabetic);

            TreeViewItem root = Project.Assets.BuildTreeUi();
            root.IsExpanded = true;

            if (DirectoryView.Items.Count > 0)
                CheckExpanded((TreeViewItem)DirectoryView.Items[0], root);

            DirectoryView.Items.Clear();
            DirectoryView.Items.Add(root);

        }
        private void CheckExpanded(TreeViewItem old, TreeViewItem next)
        {

            for (int i = 0; i < old.Items.Count; i++)
            {
                TreeViewItem oldItem = (TreeViewItem)old.Items[i];

                for (int j = 0; j < next.Items.Count; j++)
                {
                    TreeViewItem nextItem = (TreeViewItem)next.Items[j];

                    if (oldItem.Header.ToString() != nextItem.Header.ToString())
                        continue;

                    if (next.Items.Count > 0)
                        CheckExpanded(oldItem, nextItem);

                    nextItem.IsExpanded = oldItem.IsExpanded;
                    break;
                }
            }
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            if (!Project.IsActive) return;

            RefreshFileTree();
        }

        private void DirectoryView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!Project.IsActive) return;

            TreeViewItem item = (TreeViewItem)DirectoryView.SelectedItem;

            if (item == null)
                return;

            if (item.Header is not AssetTreeNodeControl node)
                return;

            if (node.Node is not AssetTreeFile file)
                return;

            /*
            foreach (string property in file.Handle.Properties)
            {
                PropertyPanel.Children.Add(new Label() { Content = property, Padding = new(0, 0, 0, 2) });
            }
            */

            SettingsPanel? panel = file.Handle.Settings?.GetPanel();

            NameTextBlock.Text = file.Handle.Name;

            if (panel != null)
            {
                panel.Bind(file.Handle.Settings);
                SettingsBody.Child = panel;
            }

            PropertiesBody.Child = file.Handle.Properties.GetPanel();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

        }

        private void Build_Click(object sender, RoutedEventArgs e)
        {
            Project.Build();
            RefreshFileTree();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!Project.IsActive) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (!Project.Context.TryGetAssetBuilder(file, out AssetBuilder? builder))
                    {
                        FileConsole.WriteLine("Could not import file: " + file + ", no builder was found!");
                        continue;
                    }

                    string fileName = Path.GetFileName(file);
                    string localPath = AssetUtils.LocalizePath(fileName);
                    string fullPath = AssetUtils.GetFullPath(localPath);
                    string directory = Path.GetDirectoryName(fullPath) ?? throw new Exception("Asset directory name null?!");

                    if (File.Exists(fullPath))
                    {
                        FileConsole.WriteLine($"Asset of name {localPath} already exists!");
                        continue;
                    }

                    FileConsole.WriteLine($"Importing file: {fileName} -> {localPath} ({fullPath})");

                    Directory.CreateDirectory(directory);
                    File.Copy(file, fullPath);
                    // do your thing here
                }
            }

            RefreshFileTree();
        }

        private void Clean_Click(object sender, RoutedEventArgs e) => Project?.Clean();

        private void Save_Click(object sender, RoutedEventArgs e) => Project?.Save();

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "Clockwork Asset Tree (*.cwat)|*.cwat";
            fileDialog.DefaultExt = ".cwat";
            fileDialog.AddExtension = true;
            fileDialog.Multiselect = false;

            if (fileDialog.ShowDialog() == false)
                return;

            if (Project != null)
            {
                switch (MessageBox.Show("Save Project?", "", MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        Project.Save();
                        Project.Dispose();
                        Project = null;
                        break;

                    case MessageBoxResult.No:
                        Project.Dispose();
                        Project = null;
                        break;

                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            string directory = Path.GetDirectoryName(fileDialog.FileName) ?? throw new Exception("Invalid path?!");
            Project = new(FileConsole, directory);
            RefreshFileTree();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog()
            {
                Filter = "Clockwork Asset Tree (*.cwat)|*.cwat",
                DefaultExt = ".cwat",
                AddExtension = true
            };

            if (fileDialog.ShowDialog() != true)
                return;

            if (Project != null)
            {
                switch (MessageBox.Show("Save Project?", "Save Project Dialog", MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        Project.Save();
                        Project.Dispose();
                        Project = null;
                        break;

                    case MessageBoxResult.No:
                        Project.Dispose();
                        Project = null;
                        break;

                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            string directory = Path.GetDirectoryName(fileDialog.FileName) ?? throw new Exception("Invalid path?!");
            Project = new(FileConsole, directory);
            RefreshFileTree();
        }

        private void TitleBarButtonMouseEnter(object sender, MouseEventArgs e)
        {
            Border border = (Border)sender;

            border.Background.Opacity = 1f;
        }

        private void TitleBarButtonMouseLeave(object sender, MouseEventArgs e)
        {
            Border border = (Border)sender;

            border.Background.Opacity = 0f;
        }

        private void TitleBarCloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void TitleBarMaximizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                BorderThickness = new System.Windows.Thickness(0);
            }
            else
            {
                // Adjust the window size to avoid going beyond the screen bounds when maximized
                WindowState = WindowState.Maximized;
                BorderThickness = new System.Windows.Thickness(7, 7, 7, 47);
            }
        }

        private void TitleBarMinimizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }

        }
    }
}