using Microsoft.Maui.Controls;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using ImageCollageCreator.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace ImageCollageCreator;

public partial class CollagePage : ContentPage
{
    private Collage CurrentCollage;

    public CollagePage()
    {
        InitializeComponent();
        CurrentCollage = new Collage
        {
            Title = "New Collage",
            CreatedDate = DateTime.Now,
            Canvas = new Canvas
            {
                Width = 800,
                Height = 600,
                Background = "White"
            }
        };
    }

    private async void OnAddImageClicked(object sender, EventArgs e)
    {
        var fileResult = await FilePicker.Default.PickAsync();
        if (fileResult != null)
        {
            // Create a new image instance
            var image = new Models.Image(fileResult.FullPath, 300, 300, new Point(0, 0)); // Increased default size
            CurrentCollage.AddImage(image); // Add image to the collage model

            // Create a Grid to wrap the image
            var grid = new Grid
            {
                WidthRequest = image.Width,
                HeightRequest = image.Height,
                BackgroundColor = Colors.Transparent
            };

            // Add the image to the Grid
            var imageView = new Microsoft.Maui.Controls.Image
            {
                Source = ImageSource.FromFile(image.FilePath),
                WidthRequest = image.Width,
                HeightRequest = image.Height
            };
            grid.Children.Add(imageView);

            // Add resize handles
            AddResizeHandles(grid, image);

            // Add gestures for dragging the grid
            AddDragGestureToGrid(grid, image);

            // Add the Grid to the canvas
            AbsoluteLayout.SetLayoutBounds(grid, new Rect(image.Position.X, image.Position.Y, image.Width, image.Height));
            AbsoluteLayout.SetLayoutFlags(grid, AbsoluteLayoutFlags.None);
            CanvasArea.Children.Add(grid); // Add the grid to the UI
        }
    }

    private void AddGesturesToImage(Microsoft.Maui.Controls.Image imageView, Models.Image image)
    {
        // Resizing (Pinch Gesture)
        var pinchGesture = new PinchGestureRecognizer();
        pinchGesture.PinchUpdated += (s, e) =>
        {
            if (e.Status == GestureStatus.Running)
            {
                // Update size
                image.Resize(e.Scale);
                imageView.WidthRequest = image.Width;
                imageView.HeightRequest = image.Height;
            }
        };
        imageView.GestureRecognizers.Add(pinchGesture);

        // Dragging (Pan Gesture)
        var dragGesture = new PanGestureRecognizer();
        Point lastPosition = image.Position; // Keep track of the last position
        dragGesture.PanUpdated += (s, e) =>
        {
            if (e.StatusType == GestureStatus.Running)
            {
                // Calculate new position using deltas from gesture
                var newX = lastPosition.X + e.TotalX;
                var newY = lastPosition.Y + e.TotalY;

                // Update position in the model
                image.SetPosition(newX, newY);

                // Update position in the UI
                AbsoluteLayout.SetLayoutBounds(imageView, new Rect(newX, newY, image.Width, image.Height));
            }
            else if (e.StatusType == GestureStatus.Completed)
            {
                // Save the final position when the gesture ends
                lastPosition = image.Position;
            }
        };
        imageView.GestureRecognizers.Add(dragGesture);
    }

    private void OnSaveCollageClicked(object sender, EventArgs e)
    {
        var bitmap = new SKBitmap((int)CanvasArea.Width, (int)CanvasArea.Height);

        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);

            foreach (var child in CanvasArea.Children)
            {
                if (child is Microsoft.Maui.Controls.Image imageView)
                {
                    var bounds = AbsoluteLayout.GetLayoutBounds(imageView);
                    using var bitmapImage = SKBitmap.Decode(imageView.Source.ToString());
                    var destRect = new SKRect((float)bounds.X, (float)bounds.Y, (float)(bounds.X + bounds.Width), (float)(bounds.Y + bounds.Height));
                    canvas.DrawBitmap(bitmapImage, destRect);
                }
            }
        }

        // Save the bitmap
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Collage.png");
        using var fileStream = File.OpenWrite(path);
        bitmap.Encode(fileStream, SKEncodedImageFormat.Png, 100);

        DisplayAlert("Saved", $"Collage saved to {path}", "OK");
    }

    private void AddResizeHandles(Grid grid, Models.Image image)
    {
        // Helper method to create a resize handle
        BoxView CreateHandle(LayoutOptions horizontal, LayoutOptions vertical)
        {
            return new BoxView
            {
                WidthRequest = 20,
                HeightRequest = 20,
                BackgroundColor = Colors.Red,
                HorizontalOptions = horizontal,
                VerticalOptions = vertical
            };
        }

        // Create handles for edges
        var topHandle = CreateHandle(LayoutOptions.Fill, LayoutOptions.Start);
        var bottomHandle = CreateHandle(LayoutOptions.Fill, LayoutOptions.End);
        var leftHandle = CreateHandle(LayoutOptions.Start, LayoutOptions.Fill);
        var rightHandle = CreateHandle(LayoutOptions.End, LayoutOptions.Fill);

        // Create handles for corners
        var topLeftHandle = CreateHandle(LayoutOptions.Start, LayoutOptions.Start);
        var topRightHandle = CreateHandle(LayoutOptions.End, LayoutOptions.Start);
        var bottomLeftHandle = CreateHandle(LayoutOptions.Start, LayoutOptions.End);
        var bottomRightHandle = CreateHandle(LayoutOptions.End, LayoutOptions.End);

        // Add gestures for edge resizing
        AddEdgeResizeGesture(topHandle, grid, image, adjustWidth: false, adjustHeight: true, anchorX: 0.5, anchorY: 1);
        AddEdgeResizeGesture(bottomHandle, grid, image, adjustWidth: false, adjustHeight: true, anchorX: 0.5, anchorY: 0);
        AddEdgeResizeGesture(leftHandle, grid, image, adjustWidth: true, adjustHeight: false, anchorX: 1, anchorY: 0.5);
        AddEdgeResizeGesture(rightHandle, grid, image, adjustWidth: true, adjustHeight: false, anchorX: 0, anchorY: 0.5);

        // Add gestures for corner resizing
        AddResizeGesture(bottomRightHandle, grid, image, true, true, 0, 0); // Bottom-right
        AddResizeGesture(topLeftHandle, grid, image, true, true, 1, 1); // Top-left
        AddResizeGesture(topRightHandle, grid, image, true, false, 0, 1); // Top-right
        AddResizeGesture(bottomLeftHandle, grid, image, false, true, 1, 0); // Bottom-left

        // Add handles to the grid
        grid.Children.Add(topHandle);
        grid.Children.Add(bottomHandle);
        grid.Children.Add(leftHandle);
        grid.Children.Add(rightHandle);
        grid.Children.Add(topLeftHandle);
        grid.Children.Add(topRightHandle);
        grid.Children.Add(bottomLeftHandle);
        grid.Children.Add(bottomRightHandle);
    }



    private void AddDragGestureToGrid(Grid grid, Models.Image image)
    {
        var dragGesture = new PanGestureRecognizer();
        Point lastPosition = image.Position; // Keep track of the last position
        dragGesture.PanUpdated += (s, e) =>
        {
            if (e.StatusType == GestureStatus.Running)
            {
                // Calculate new position using deltas from gesture
                var newX = lastPosition.X + e.TotalX;
                var newY = lastPosition.Y + e.TotalY;

                // Update position in the model
                image.SetPosition(newX, newY);

                // Update position in the UI
                AbsoluteLayout.SetLayoutBounds(grid, new Rect(newX, newY, grid.WidthRequest, grid.HeightRequest));
            }
            else if (e.StatusType == GestureStatus.Completed)
            {
                // Save the final position when the gesture ends
                lastPosition = image.Position;
            }
        };
        grid.GestureRecognizers.Add(dragGesture);
    }

    private void AddResizeGesture(BoxView handle, Grid grid, Models.Image image, bool adjustWidth, bool adjustHeight, double anchorX, double anchorY)
    {
        double initialWidth = 0;
        double initialHeight = 0;
        double aspectRatio = 1; // Width / Height
        Point initialPosition = new Point();

        var resizeGesture = new PanGestureRecognizer();
        resizeGesture.PanUpdated += (s, e) =>
        {
            if (e.StatusType == GestureStatus.Started)
            {
                // Capture the initial dimensions, position, and aspect ratio at the start of the gesture
                initialWidth = image.Width;
                initialHeight = image.Height;
                initialPosition = image.Position;
                aspectRatio = initialWidth / initialHeight;
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Calculate the delta for resizing
                    var deltaX = e.TotalX * (anchorX == 0 ? 1 : -1);
                    var deltaY = e.TotalY * (anchorY == 0 ? 1 : -1);

                    // Use the larger delta (horizontal or vertical) to maintain aspect ratio
                    double resizeDelta = Math.Abs(deltaX) > Math.Abs(deltaY) ? deltaX : deltaY;

                    // Calculate new dimensions while maintaining the aspect ratio
                    var newWidth = Math.Max(initialWidth + resizeDelta, 50); // Minimum width
                    var newHeight = Math.Max(newWidth / aspectRatio, 50); // Adjust height based on aspect ratio

                    // Adjust position if resizing from top-left or similar corners
                    var newX = anchorX == 1 ? initialPosition.X + (initialWidth - newWidth) : initialPosition.X;
                    var newY = anchorY == 1 ? initialPosition.Y + (initialHeight - newHeight) : initialPosition.Y;

                    // Update the image model
                    image.SetSize(newWidth, newHeight);
                    image.SetPosition(newX, newY);

                    // Update the grid and image view dimensions
                    grid.WidthRequest = newWidth;
                    grid.HeightRequest = newHeight;

                    var imageView = grid.Children[0] as Microsoft.Maui.Controls.Image;
                    if (imageView != null)
                    {
                        imageView.WidthRequest = newWidth;
                        imageView.HeightRequest = newHeight;
                    }

                    // Update the position of the grid
                    AbsoluteLayout.SetLayoutBounds(grid, new Rect(newX, newY, newWidth, newHeight));
                });
            }
        };
        handle.GestureRecognizers.Add(resizeGesture);
    }

    private void AddEdgeResizeGesture(BoxView handle, Grid grid, Models.Image image, bool adjustWidth, bool adjustHeight, double anchorX, double anchorY)
    {
    double initialWidth = 0;
    double initialHeight = 0;
    Point initialPosition = new Point();

    var resizeGesture = new PanGestureRecognizer();
    resizeGesture.PanUpdated += (s, e) =>
    {
        if (e.StatusType == GestureStatus.Started)
        {
            // Capture the initial dimensions and position
            initialWidth = image.Width;
            initialHeight = image.Height;
            initialPosition = image.Position;
        }
        else if (e.StatusType == GestureStatus.Running)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Calculate delta values
                var deltaX = adjustWidth ? e.TotalX * (anchorX == 0 ? 1 : -1) : 0;
                var deltaY = adjustHeight ? e.TotalY * (anchorY == 0 ? 1 : -1) : 0;

                // Calculate new dimensions
                var newWidth = Math.Max(initialWidth + deltaX, 50); // Minimum width
                var newHeight = Math.Max(initialHeight + deltaY, 50); // Minimum height

                // Adjust position if resizing from top/left edges
                var newX = anchorX == 1 ? initialPosition.X + deltaX : initialPosition.X;
                var newY = anchorY == 1 ? initialPosition.Y + deltaY : initialPosition.Y;

                // Update the image model dimensions
                image.SetSize(newWidth, newHeight);
                image.SetPosition(newX, newY);

                // Update the UI elements
                grid.WidthRequest = newWidth;
                grid.HeightRequest = newHeight;

                var imageView = grid.Children[0] as Microsoft.Maui.Controls.Image;
                if (imageView != null)
                {
                    imageView.WidthRequest = newWidth;
                    imageView.HeightRequest = newHeight;
                }

                // Update the layout bounds of the grid
                AbsoluteLayout.SetLayoutBounds(grid, new Rect(newX, newY, newWidth, newHeight));
            });
        }
    };
    handle.GestureRecognizers.Add(resizeGesture);
    }
    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;

        // Clear the canvas with a background color
        canvas.Clear(SKColors.White);

        // Example: Draw something on the canvas
        using (var paint = new SKPaint
        {
            Color = SKColors.Blue,
            StrokeWidth = 5,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        })
        {
            canvas.DrawRect(new SKRect(50, 50, 200, 200), paint);
        }
    }

    private double SnapToGrid(double value, double gridSize)
    {
        return Math.Round(value / gridSize) * gridSize;
    }
}
