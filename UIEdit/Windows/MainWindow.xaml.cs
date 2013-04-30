using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using UIEdit.Controllers;
using UIEdit.Models;

namespace UIEdit.Windows {
    public partial class MainWindow {
        public ProjectController ProjectController { get; set; }
        public SourceFile CurrentSourceFile { get; set; }
        public LayoutController LayoutController { get; set; }

        public MainWindow() {
            InitializeComponent();
            Dispatcher.UnhandledException += ApplicationOnDispatcherUnhandledException;
            ProjectController = new ProjectController();
            LayoutController = new LayoutController();
            LbDialogs.SelectionChanged += LbDialogsOnSelectionChanged;
            TeFile.TextChanged += TeFileOnTextChanged;
            TxtSearch.TextChanged += TxtSearchOnTextChanged;
        }

        private void ApplicationOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            File.AppendAllText("error.log", string.Format(@"{0} {1} {2}{4}{3}{4}{4}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), e.Exception, e.Exception.Message, e.Exception.StackTrace, Environment.NewLine));
        }

        private void TxtSearchOnTextChanged(object sender, TextChangedEventArgs e) {
            if (ProjectController.Files == null) return;
            LbDialogs.ItemsSource = null;
            var files = TxtSearch.Text == "" 
                ? ProjectController.Files.OrderBy(t=>t.ShortFileName)
                : ProjectController.Files.Where(f=>f.ShortFileName.Contains(TxtSearch.Text)).OrderBy(t => t.ShortFileName);
            LbDialogs.ItemsSource = files;
            if (CurrentSourceFile == null) return;
            var i = -1;
            foreach (var sourceFile in files) {
                i++;
                if (sourceFile.FileName != CurrentSourceFile.FileName) continue;
                LbDialogs.SelectedIndex = i;
                break;
            }
        }

        private void TeFileOnTextChanged(object sender, EventArgs e) {
            LockEditor(TeFile.IsModified);
            var exceptionParse = LayoutController.Parse(TeFile.Text, ProjectController.SurfacesPath);
            if (exceptionParse == null)
                LayoutController.RefreshLayout(DialogCanvas);
            else {
                DialogCanvas.Children.Clear();
                DialogCanvas.Children.Add(
                    new TextBlock {
                        Text = exceptionParse.Message,
                        MaxWidth = DialogCanvas.ActualWidth,
                        Margin = new Thickness(0),
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush { Color = Colors.Red },
                    });
            }
        }

        private void LockEditor(bool state) {
            GbSourceFiles.IsEnabled = !state;
            EditorButtonPanel.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LbDialogsOnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0) return;
            CurrentSourceFile = (SourceFile) e.AddedItems[0];
            TeFile.Text = ProjectController.GetSourceFileContent(CurrentSourceFile);
            LockEditor(false);
        }

        private void BtnCommit_OnClick(object sender, RoutedEventArgs e) {
            ProjectController.SaveSourceFileContent(CurrentSourceFile, TeFile.Text);
            LockEditor(false);
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e) {
            TeFile.Text = ProjectController.GetSourceFileContent(CurrentSourceFile);
            LockEditor(false);
        }

        private void BtnClearFilter_OnClick(object sender, RoutedEventArgs e) {
            TxtSearch.Text = "";
            TxtSearch.Focus();
        }

        private void BtnInterfacesPath_OnClick(object sender, RoutedEventArgs e) {
            ProjectController.SetLocationInterfaces();
            TbInterfacesPath.Text = ProjectController.InterfacesPath;
            LbDialogs.ItemsSource = ProjectController.Files;
        }

        private void BtnSurfacesPath_OnClick(object sender, RoutedEventArgs e) {
            ProjectController.SetLocationSurfaces();
            TbSurfacesPath.Text = ProjectController.SurfacesPath;
        }

        private void BtnGotoGithub_OnClick(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/perfectdev/UIEdit");
        }
    }
}
