using CopyFilesWPF.Model;
using CopyFilesWPF.View;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using System.IO;
using System;

namespace CopyFilesWPF.Presenter
{
    internal class MainWindowPresenter : IMainWindowPresenter
    {
        private readonly IMainWindowView _mainWindowView;
        private readonly MainWindowModel _mainWindowModel;

        public MainWindowPresenter(IMainWindowView mainWindowView)
        {
            _mainWindowView = mainWindowView;
            _mainWindowModel = new MainWindowModel();
        }

        public void ChooseFileFromButtonClick(string path)
        {
            _mainWindowModel.FilePath.pathFrom = path;
        }

        public void ChooseFileToButtonClick(string path)
        {
            _mainWindowModel.FilePath.PathTo = path;
        }

        // порефакторить этот метод, убрать хардкод, разделить на более мелкие методы
        public void CopyButtonClick()
        {
            _mainWindowModel.FilePath.pathFrom = _mainWindowView.MainWindowView.FromTextBox.Text;
            _mainWindowModel.FilePath.PathTo = _mainWindowView.MainWindowView.ToTextBox.Text;
            _mainWindowView.MainWindowView.FromTextBox.Text = "";
            _mainWindowView.MainWindowView.ToTextBox.Text = "";
            _mainWindowView.MainWindowView.Height = _mainWindowView.MainWindowView.Height + 60;
            var newPanel = new Grid();
            newPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
            newPanel.ColumnDefinitions.Add(new ColumnDefinition());
            newPanel.ColumnDefinitions.Add(new ColumnDefinition());
            newPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            newPanel.RowDefinitions.Add(new RowDefinition());
            var nameFile = new TextBlock
            {
                Text = Path.GetFileName(_mainWindowModel.FilePath.pathFrom),
                Margin = new Thickness(5, 0, 5, 0)
            };
            Grid.SetRow(nameFile, 0);
            Grid.SetColumn(nameFile, 0);
            newPanel.Children.Add(nameFile);
            var progressBar = new ProgressBar
            {
                Margin = new Thickness(10, 10, 10, 10)
            };
            Grid.SetRow(progressBar, 1);
            newPanel.Children.Add(progressBar);
            var pauseB = new Button
            {
                Content = "Pause",
                Margin = new Thickness(5),
                Tag = newPanel
            };
            pauseB.Click += PauseCancelClick;
            Grid.SetRow(pauseB, 1);
            Grid.SetColumn(pauseB, 1);
            newPanel.Children.Add(pauseB);
            var cancelB = new Button
            {
                Content = "Cancel",
                Margin = new Thickness(5),
                Tag = newPanel
            };
            cancelB.Click += PauseCancelClick;
            Grid.SetRow(cancelB, 1);
            Grid.SetColumn(cancelB, 2);
            newPanel.Children.Add(cancelB);
            DockPanel.SetDock(newPanel, Dock.Top);
            newPanel.Height = 60;
            _mainWindowView.MainWindowView.MainPanel.Children.Add(newPanel);
            _mainWindowModel.CopyFile(ProgressChanged, ModelOnComplete, newPanel);
        }

        private void PauseCancelClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Button clickedButton = (Button)sender;
            FileCopier fileCopier = GetFileCopierFromButton(clickedButton);

            if (clickedButton.Content is string buttonContent)
            {
                if (buttonContent.Equals("Cancel"))
                {
                    fileCopier.CancelFlag = true;
                }

                else if (buttonContent.Equals("Pause"))
                {
                    fileCopier.PauseFlag.Reset();
                }

                else
                {
                    fileCopier.PauseFlag.Set();
                }
            }
        }

        private FileCopier GetFileCopierFromButton(Button button)
        {
            if (button.Tag is Grid grid && grid.Tag is FileCopier fileCopier)
            {
                return fileCopier;
            }
            throw new InvalidOperationException("Error");
        }

        private void ModelOnComplete(Grid panel)
        {
            _mainWindowView.MainWindowView.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    _mainWindowView.MainWindowView.Height = _mainWindowView.MainWindowView.Height - 60;
                    _mainWindowView.MainWindowView.MainPanel.Children.Remove(panel);
                    _mainWindowView.MainWindowView.CopyButton.IsEnabled = true;
                }
            );
        }

        private void ProgressChanged(double percentage, ref bool cancelFlag, Grid panel)
        {
            _mainWindowView.MainWindowView.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    UpdateProgressBar(panel, percentage);
                    UpdateButtons(panel);
                }
            );
        }

        private void UpdateProgressBar(Grid panel, double percentage)
        {
            foreach (var el in panel.Children)
            {
                if (el is ProgressBar bar)
                {
                    bar.Value = percentage;
                }
            }
        }

        private void UpdateButtons(Grid panel)
        {
            foreach (var el in panel.Children)
            {
                if (el is Button button)
                {
                    string buttonText = button.Content.ToString();
                    if (buttonText.Equals("Resume") && !button.IsEnabled)
                    {
                        button.Content = "Pause";
                        button.IsEnabled = true;
                    }
                    else if (buttonText.Equals("Pause") && !button.IsEnabled)
                    {
                        button.Content = "Resume";
                        button.IsEnabled = true;
                    }
                }
            }
        }
    }
}