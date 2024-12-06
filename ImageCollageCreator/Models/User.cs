using System;

namespace ImageCollageCreator.Models
{
    public class User
    {
        public string Username { get; set; }
        public List<Collage> Collages { get; set; } = new List<Collage>();

        public void CreateCollage(string title, string backgroundColor, int width, int height)
        {
            var newCanvas = new Canvas
            {
                Width = width,
                Height = height,
                Background = backgroundColor
            };

            var newCollage = new Collage
            {
                Title = title,
                CreatedDate = DateTime.Now,
                Canvas = newCanvas
            };

            Collages.Add(newCollage);
        }

        public void SaveCollage(Collage collage)
        {
            // Logic to save collage (e.g., export to a file)
        }
    }
}

