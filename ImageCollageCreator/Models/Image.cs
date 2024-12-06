using System;
using Microsoft.Maui.Graphics;

namespace ImageCollageCreator.Models
{
    public class Image
    {
        public string FilePath { get; set; } // Path to the image file
        public double Width { get; set; }   // Width of the image
        public double Height { get; set; }  // Height of the image
        public Point Position { get; set; } // Position of the image on the canvas

        // Constructor with parameters
        public Image(string filePath, double width, double height, Point position)
        {
            FilePath = filePath;
            Width = width;
            Height = height;
            Position = position;
        }

        // Resizes the image dimensions
        public void Resize(double scale)
        {
            Width *= scale;
            Height *= scale;
        }

        // Updates the position of the image
        public void Move(double deltaX, double deltaY)
        {
            Position = new Point(Position.X + deltaX, Position.Y + deltaY);
        }

        // Provides a method to update size explicitly
        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        // Provides a method to set an explicit position
        public void SetPosition(double x, double y)
        {
            Position = new Point(x, y);
        }
    }
}
