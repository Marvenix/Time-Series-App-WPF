using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.AxisRules;
using ScottPlot.Colormaps;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Time_Series_App_WPF.Model;
using Time_Series_App_WPF.View;
using Time_Series_App_WPF.ViewModel;

namespace Time_Series_App_WPF
{
    public partial class MainWindow : Window
    {
        private Coordinates _mouseDownCoordinates;
        private Coordinates _mouseUpCoordinates;
        private Coordinates[]? _coordinates;
        private CoordinateRect _mouseSelectionRect => new(_mouseDownCoordinates, _mouseUpCoordinates);
        private Coordinates _reversedBottomRight => new(_mouseSelectionRect.BottomRight.Y, _mouseSelectionRect.BottomRight.X);
        private Point _cords;
        private bool _mouseIsDown = false;
        private Polygon? _rectangle;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.MessageBoxRequest += new EventHandler<MessageBoxEventArgs>(ShowMessageBox);
            viewModel.AnnotationWindowRequest += new EventHandler(ShowAnnotationWindow);
            viewModel.ProgramInfoWindowRequest += new EventHandler(ShowProgramInfoWindow);
        }

        private void ShowMessageBox(object? sender, MessageBoxEventArgs e)
        {
            MessageBox.Show(this, e.Message, e.Caption);
        }

        private void ShowAnnotationWindow(object? sender, EventArgs e)
        {
            var annotationWindow = App.AppHost!.Services.GetRequiredService<AnnotationWindow>();
            annotationWindow.ShowDialog();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainWindowViewModel;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = (string)Application.Current.TryFindResource("OpenFileDialog-Filter");
            
            if (openFileDialog.ShowDialog() == true && File.Exists(openFileDialog.FileName))
            {
                viewModel!.OpenFileCommand.Execute(openFileDialog.FileName);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;

            if (Navigation.IsChecked!.Value)
            {
                double scrollOffset = scrollViewer.VerticalOffset - (e.Delta * .5);
                scrollViewer.ScrollToVerticalOffset(scrollOffset);
                e.Handled = true;
                return;
            }

            if (Zoom.IsChecked!.Value)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
                return;
            }
        }

        private void ShowProgramInfoWindow(object? sender, EventArgs e)
        {
            var programInfoWindow = App.AppHost!.Services.GetRequiredService<ProgramInfoWindow>();
            programInfoWindow.ShowDialog();
        }

        private void ContentPresenter_MouseUp(object? sender, MouseEventArgs e)
        {
            if ((bool)!Annotation.IsChecked!)
                return;

            if (sender is WpfPlot plotControl)
            {
                var viewModel = DataContext as MainWindowViewModel;

                var madeAnnotation = new MadeAnnotation()
                {
                    Id = new Guid(_rectangle!.Label),
                    Annotation = viewModel!.SelectedAnnotation!,
                    Start = (float)Math.Min(_mouseDownCoordinates.X, _mouseUpCoordinates.X),
                    End = (float)Math.Max(_mouseDownCoordinates.X, _mouseUpCoordinates.X),
                    Duration = (float)Math.Max(_mouseDownCoordinates.X, _mouseUpCoordinates.X) - (float)Math.Min(_mouseDownCoordinates.X, _mouseUpCoordinates.X),
                    Channel = plotControl.Plot.Axes.Title.Label.Text,
                    ChannelId = plotControl.Uid
                };

                viewModel!.AddMadeAnnotationCommand.Execute(madeAnnotation);

                _mouseIsDown = false;
                _mouseDownCoordinates = Coordinates.NaN;
                _mouseUpCoordinates = Coordinates.NaN;
                plotControl.Refresh();
                plotControl.Interaction.Enable();
            }
        }

        private void ContentPresenter_MouseDown(object? sender, MouseEventArgs e)
        {
            if ((bool)!Annotation.IsChecked!)
                return;

            if (sender is WpfPlot plotControl)
            {
                _mouseIsDown = true;

                _rectangle = plotControl.Plot.Add.Rectangle(0, 0, 0, 0);

                var viewModel = DataContext as MainWindowViewModel;

                var colorToConvert = (System.Windows.Media.Color)ColorConverter.ConvertFromString(viewModel!.SelectedAnnotation!.Color);

                _rectangle.FillStyle.Color = new ScottPlot.Color(colorToConvert.R, colorToConvert.G, colorToConvert.B).WithAlpha(.3);
                _rectangle.Label = Guid.NewGuid().ToString();
                _rectangle.IsVisible = true;

                _cords = e.GetPosition(plotControl);
                _coordinates = new Coordinates[4];

                _mouseDownCoordinates = plotControl.Plot.GetCoordinates((float)_cords.X, (float)_cords.Y);
                _mouseDownCoordinates.Y = (float)plotControl.Plot.Axes.Left.Max;

                plotControl.Interaction.Disable();
            }
        }

        private void ContentPresenter_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_mouseIsDown || (bool)!Annotation.IsChecked!)
                return;

            if (sender is WpfPlot plotControl)
            {
                _cords = e.GetPosition(plotControl);
                _mouseUpCoordinates = plotControl.Plot.GetCoordinates((float)_cords.X, (float)_cords.Y);
                _mouseUpCoordinates.Y = (float)plotControl.Plot.Axes.Left.Min;
                _coordinates![0] = _mouseSelectionRect.BottomLeft;
                _coordinates![1] = _mouseSelectionRect.TopLeft;
                _coordinates![2] = _mouseSelectionRect.TopRight;
                _coordinates![3] = _reversedBottomRight;
                _rectangle!.UpdateCoordinates(_coordinates);
                plotControl.Refresh();
            }
        }
    }
}
