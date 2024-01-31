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
        private bool _mouseHasMoved;
        private bool _mouseAtRectangleEdge;
        private bool _leftEdge;
        private bool _mouseLeftWhileEditing;
        private Polygon? _rectangle;
        private Polygon? _rectangleOnMouseDown;
        private Polygon? _rectangleSelected;
        private WpfPlot? _plot;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.MessageBoxRequest += new EventHandler<MessageBoxEventArgs>(ShowMessageBox);
            viewModel.AnnotationWindowRequest += new EventHandler(ShowAnnotationWindow);
            viewModel.ProgramInfoWindowRequest += new EventHandler(ShowProgramInfoWindow);
            viewModel.ChangeViewValuesRequest += new EventHandler(ChangeViewValues);
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

        private void ChangeViewValues(object? sender, EventArgs e)
        {
            _rectangleSelected = null;
            _plot = null;
        }
        private void ContentPresenter_MouseUp(object? sender, MouseEventArgs e)
        {
            if (sender is WpfPlot plotControl)
            {
                if (AnnotationMake.IsChecked!.Value && _mouseHasMoved) {
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

                    _mouseHasMoved = false;
                    _mouseIsDown = false;
                    _mouseDownCoordinates = Coordinates.NaN;
                    _mouseUpCoordinates = Coordinates.NaN;
                    plotControl.Refresh();
                    plotControl.Interaction.Enable();
                    return;
                }

                if (AnnotationMake.IsChecked!.Value && !_mouseHasMoved)
                {
                    plotControl.Plot.PlottableList.Remove(_rectangle!);

                    _mouseIsDown = false;
                    _mouseDownCoordinates = Coordinates.NaN;
                    _mouseUpCoordinates = Coordinates.NaN;
                    plotControl.Refresh();
                    plotControl.Interaction.Enable();
                    return;
                }

                if (AnnotationEdit.IsChecked!.Value && !_mouseHasMoved && !_mouseAtRectangleEdge)
                {
                    Cursor = Cursors.Arrow;

                    if (_rectangleOnMouseDown == null && _rectangleSelected != null)
                    {
                        _rectangleSelected.LineStyle.IsVisible = false;
                        _rectangleSelected = null;
                        _plot!.Refresh();
                        _plot = null;

                        _mouseIsDown = false;
                        _mouseDownCoordinates = Coordinates.NaN;

                        plotControl.Refresh();
                        return;
                    }

                    if (_rectangleOnMouseDown != null && _rectangleSelected == null)
                    {
                        _rectangleOnMouseDown.LineStyle.IsVisible = true;
                        _rectangleSelected = _rectangleOnMouseDown;
                        _rectangleOnMouseDown = null;
                        _plot = plotControl;

                        _mouseIsDown = false;
                        _mouseDownCoordinates = Coordinates.NaN;

                        plotControl.Refresh();
                        return;
                    }

                    if (_rectangleOnMouseDown != null && _rectangleSelected != null && _rectangleOnMouseDown != _rectangleSelected)
                    {
                        _rectangleSelected.LineStyle.IsVisible = false;
                        _rectangleOnMouseDown.LineStyle.IsVisible = true;
                        _rectangleSelected = _rectangleOnMouseDown;
                        _rectangleOnMouseDown = null;

                        _plot!.Refresh();
                        _plot = plotControl;

                        _mouseIsDown = false;
                        _mouseDownCoordinates = Coordinates.NaN;

                        plotControl.Refresh();
                        return;
                    }

                    if (_rectangleOnMouseDown != null && _rectangleSelected != null && _rectangleOnMouseDown == _rectangleSelected)
                    {
                        _rectangleOnMouseDown = null;
                        _rectangleSelected.LineStyle.IsVisible = false;
                        _rectangleSelected = null;
                        _plot = null;

                        _mouseIsDown = false;
                        _mouseDownCoordinates = Coordinates.NaN;

                        plotControl.Refresh();
                        return;
                    }
                }

                if (AnnotationEdit.IsChecked!.Value && _mouseHasMoved && (_mouseAtRectangleEdge || _mouseLeftWhileEditing))
                {
                    var viewModel = DataContext as MainWindowViewModel;

                    var editedAnnotation = new MadeAnnotation
                    {
                        Id = new Guid(_rectangleSelected!.Label),
                        Start = (float)_rectangleSelected.Coordinates[0].X,
                        End = (float)_rectangleSelected.Coordinates[3].X,
                        Duration = (float)(_rectangleSelected.Coordinates[3].X - _rectangleSelected.Coordinates[0].X)
                    };
                    Debug.WriteLine("edycja adnotacji!");
                    viewModel!.EditMadeAnnotationCommand.Execute(editedAnnotation);

                    _mouseIsDown = false;
                    _mouseHasMoved = false;
                    _mouseLeftWhileEditing = false;
                    plotControl.Interaction.Enable();
                    return;
                }

                if (AnnotationEdit.IsChecked!.Value && _mouseHasMoved && !_mouseAtRectangleEdge)
                {
                    _mouseIsDown = false;
                    _mouseHasMoved = false;
                    return;
                }
            }
        }

        private void ContentPresenter_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is WpfPlot plotControl)
            {
                if (AnnotationMake.IsChecked!.Value)
                {
                    _mouseIsDown = true;

                    _rectangle = plotControl.Plot.Add.Rectangle(0, 0, 0, 0);

                    var viewModel = DataContext as MainWindowViewModel;

                    var colorToConvert = (System.Windows.Media.Color)ColorConverter.ConvertFromString(viewModel!.SelectedAnnotation!.Color);

                    _rectangle.FillStyle.Color = new ScottPlot.Color(colorToConvert.R, colorToConvert.G, colorToConvert.B).WithAlpha(.3);
                    _rectangle.Label = Guid.NewGuid().ToString();
                    _rectangle.LineStyle = new LineStyle
                    {
                        AntiAlias = true,
                        Color = ScottPlot.Colors.Black,
                        Pattern = LinePattern.Solid,
                        Width = 2,
                        IsVisible = false
                    };
                    _rectangle.IsVisible = true;

                    _cords = e.GetPosition(plotControl);
                    _coordinates = new Coordinates[4];

                    _mouseDownCoordinates = plotControl.Plot.GetCoordinates((float)_cords.X, (float)_cords.Y);
                    _mouseDownCoordinates.Y = (float)plotControl.Plot.Axes.Left.Max;

                    plotControl.Interaction.Disable();
                    return;
                }

                if (AnnotationEdit.IsChecked!.Value && !_mouseAtRectangleEdge)
                {
                    _mouseIsDown = true;
                    _mouseHasMoved = false;
                    _cords = e.GetPosition(plotControl);
                    _mouseDownCoordinates = plotControl.Plot.GetCoordinates((float)_cords.X, (float)_cords.Y);

                    var polygons = plotControl.Plot.PlottableList.Where(x => x is Polygon && _mouseDownCoordinates.X >= ((Polygon)x).Coordinates[0].X 
                        && _mouseDownCoordinates.X <= ((Polygon)x).Coordinates[3].X);

                    if (polygons.Count() == 0)
                        _rectangleOnMouseDown = null;

                    if (polygons.Count() == 1)
                        _rectangleOnMouseDown = (Polygon?)polygons.First();

                    if (polygons.Count() > 1)
                    {
                        var minWidthPolygon = polygons.First();
                        foreach (var polygon in polygons)
                        {
                            var polygonLength = ((Polygon)polygon).Coordinates[3].X - ((Polygon)polygon).Coordinates[0].X;
                            var minWidthPolygonLength = ((Polygon)minWidthPolygon).Coordinates[3].X - ((Polygon)minWidthPolygon).Coordinates[0].X;

                            if (polygonLength < minWidthPolygonLength)
                                minWidthPolygon = polygon;
                        }
                        _rectangleOnMouseDown = (Polygon?)minWidthPolygon;
                    }
                    return;
                }

                if (AnnotationEdit.IsChecked!.Value && _mouseAtRectangleEdge)
                {
                    _coordinates = new Coordinates[4];
                    _mouseIsDown = true;
                    return;
                }
            }
        }

        private void ContentPresenter_MouseMove(object? sender, MouseEventArgs e)
        {
            if (sender is WpfPlot plotControl)
            {
                if (_mouseIsDown && AnnotationMake.IsChecked!.Value) 
                {
                    _mouseHasMoved = true;
                    _cords = e.GetPosition(plotControl);
                    _mouseUpCoordinates = plotControl.Plot.GetCoordinates((float)_cords.X, (float)_cords.Y);
                    _mouseUpCoordinates.Y = (float)plotControl.Plot.Axes.Left.Min;

                    if (_mouseUpCoordinates.X < plotControl.Plot.Axes.Bottom.Min)
                        _mouseUpCoordinates.X = plotControl.Plot.Axes.Bottom.Min;

                    if (_mouseUpCoordinates.X > plotControl.Plot.Axes.Bottom.Max)
                        _mouseUpCoordinates.X = plotControl.Plot.Axes.Bottom.Max;

                    _coordinates![0] = _mouseSelectionRect.BottomLeft;
                    _coordinates![1] = _mouseSelectionRect.TopLeft;
                    _coordinates![2] = _mouseSelectionRect.TopRight;
                    _coordinates![3] = _reversedBottomRight;
                    _rectangle!.UpdateCoordinates(_coordinates);
                    plotControl.Refresh();
                    return;
                }

                if (AnnotationEdit.IsChecked!.Value && _rectangleSelected != null && !_mouseIsDown && _plot == plotControl)
                {
                    _cords = e.GetPosition(plotControl);
                    CoordinateRect rect = plotControl.Plot.GetCoordinateRect((float)_cords.X, (float)_cords.Y, radius: 10);

                    if (rect.ContainsX(_rectangleSelected.Coordinates[0].X))
                    {
                        Cursor = Cursors.SizeWE;
                        _mouseAtRectangleEdge = true;
                        _leftEdge = true;
                    }
                    else if (rect.ContainsX(_rectangleSelected.Coordinates[3].X))
                    {
                        Cursor = Cursors.SizeWE;
                        _mouseAtRectangleEdge = true;
                        _leftEdge = false;
                    }
                    else
                    {
                        Cursor = Cursors.Arrow;
                        _mouseAtRectangleEdge = false;
                    }
                    return;
                }

                if (AnnotationEdit.IsChecked!.Value && _mouseAtRectangleEdge && _mouseIsDown)
                {
                    plotControl.Interaction.Disable();
                    _mouseHasMoved = true;
                    _mouseDownCoordinates = _leftEdge ? _rectangleSelected!.Coordinates[2] : _rectangleSelected!.Coordinates[1];

                    _cords = e.GetPosition(plotControl);
                    _mouseUpCoordinates = plotControl.Plot.GetCoordinates((float)_cords.X, (float)_cords.Y);
                    _mouseUpCoordinates.Y = (float)plotControl.Plot.Axes.Left.Min;

                    if (_mouseUpCoordinates.X < plotControl.Plot.Axes.Bottom.Min)
                        _mouseUpCoordinates.X = plotControl.Plot.Axes.Bottom.Min;

                    if (_mouseUpCoordinates.X > plotControl.Plot.Axes.Bottom.Max)
                        _mouseUpCoordinates.X = plotControl.Plot.Axes.Bottom.Max;

                    _coordinates![0] = _mouseSelectionRect.BottomLeft;
                    _coordinates![1] = _mouseSelectionRect.TopLeft;
                    _coordinates![2] = _mouseSelectionRect.TopRight;
                    _coordinates![3] = _reversedBottomRight;
                    _rectangleSelected!.UpdateCoordinates(_coordinates);
                    plotControl.Refresh();
                    return;
                }

                if (AnnotationEdit.IsChecked!.Value && !_mouseAtRectangleEdge && _mouseIsDown)
                {
                    _mouseHasMoved = true;
                    return;
                }
            }
        }

        private void WpfPlot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (AnnotationMake.IsChecked!.Value)
                return;

            if (_mouseAtRectangleEdge && _mouseIsDown)
                _mouseLeftWhileEditing = true;

            _mouseAtRectangleEdge = false;
            Cursor = Cursors.Arrow;
        }

        private void AnnotationMake_Checked(object sender, RoutedEventArgs e)
        {
            if (_rectangleSelected != null)
            {
                _rectangleSelected.LineStyle.IsVisible = false;
                _rectangleSelected = null;

                _plot!.Refresh();
                _plot = null;
            }
        }

        private void ScrollViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (_rectangleSelected != null && e.Key == Key.Delete)
            {
                var viewModel = DataContext as MainWindowViewModel;
                var id = new Guid(_rectangleSelected.Label);

                if (_mouseAtRectangleEdge)
                {
                    _mouseAtRectangleEdge = false;
                    Cursor = Cursors.Arrow;

                    if (_mouseIsDown)
                    {
                        _mouseIsDown = false;
                        _mouseHasMoved = false;
                        _plot!.Interaction.Enable();
                    }
                }

                viewModel!.RemoveMadeAnnotationCommand.Execute(id);
            }
        }
    }
}
