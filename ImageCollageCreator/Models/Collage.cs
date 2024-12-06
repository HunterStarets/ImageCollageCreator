using System;
using System.Collections.Generic;

namespace ImageCollageCreator.Models
{
    public class Collage
    {
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public Canvas Canvas { get; set; }
        public List<Image> Images { get; set; } = new List<Image>();

        public void AddImage(Image image)
        {
            Images.Add(image);
        }

        public void RemoveImage(Image image)
        {
            Images.Remove(image);
        }
    }
}


