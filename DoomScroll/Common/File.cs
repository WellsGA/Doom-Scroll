using Doom_Scroll.UI;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public enum FileType
    {
        IMAGE,
        MAPSOURCE
    }
    public class File : IDirectory
    {
        // type?? image and mapsource ...
        private string name;
        private string path;
        private CustomButton fileBtn;
        private CustomText label;
        private FileType type;
        private byte[] content;
        public File(string parentPath, GameObject parent, string name, byte[] image)
        {
            this.name = name;
            path = parentPath + "/" + name;
            content = image;
            Sprite[] images = { ImageLoader.ReadImageFromByteArray(content) };
            fileBtn = new CustomButton(parent, images, name);
            label = new CustomText(name, fileBtn.ButtonGameObject, name);
            fileBtn.ActivateButton(false);
        }
        public string GetName()
        {
            return name;
        }
        public string GetPath()
        {
            return path;
        }
        public CustomButton GetButton()
        {
            return fileBtn;
        }
        public CustomText GetLabel()
        {
            return label;
        }
        public void DisplayContent()
        {
            // display the content of the file -- TO DO
            switch (type)
            {
                case FileType.IMAGE:
                    return;
                case FileType.MAPSOURCE:
                    return;
            }
        }
        public void HideContent()
        {
            // hide the content -- close the overlay
        }
        public string PrintDirectory()
        {
            return " " + path + " [file]";
        }
    }
}
