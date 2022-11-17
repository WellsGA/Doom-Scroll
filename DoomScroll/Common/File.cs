using System.Reflection;
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
        public string Name { get; private set; }
        public string Path { get; private set; }
        public CustomButton DirBtn { get; private set; }
        public CustomButton ShareBtn { get; private set; }
        public CustomButton DontShareBtn { get; private set; }
        public CustomText Label { get; private set; }
        private FileType type;
        private byte[] content;
        public bool IsShareOpen { get; private set; }
        public File(string parentPath, GameObject parent, string name, byte[] image, FileType fileType)
        {
            Name = name;
            Path = parentPath + "/" + name;
            type = fileType;
            content = image;
            IsShareOpen = false;

            Sprite[] dirBtnImg = { ImageLoader.ReadImageFromByteArray(content) };
            DirBtn = new CustomButton(parent, dirBtnImg, name);
            Label = new CustomText(name, DirBtn.ButtonGameObject, name);
            DirBtn.ActivateButton(false);
            DirBtn.ButtonEvent.MyAction += DisplayContent;

            CreateShareOverlay();
            
        }
        
        public void DisplayContent()
        {
            // display the content of the file -- TO DO
            switch (type)
            {
                case FileType.IMAGE:
                   // DoomScroll._log.LogInfo("Clicked!");
                    ToggleShareButton();
                    return;
                case FileType.MAPSOURCE:
                    return;
            }
        }

        private void ToggleShareButton() 
        {
            DirBtn.EnableButton(IsShareOpen);
            ShareBtn.ActivateButton(!IsShareOpen);
            DontShareBtn.ActivateButton(!IsShareOpen);
            IsShareOpen = !IsShareOpen;
        }
        private void CreateShareOverlay() 
        {
            // text // to do

            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            // Share button
            Sprite[] shareBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", slices);
            ShareBtn = new CustomButton(DirBtn.ButtonGameObject, shareBtnImg, "Share");
            ShareBtn.ScaleSize(1.5f);
            ShareBtn.SetLocalPosition(new Vector3(0f, 0f, -5));
            
            ShareBtn.ActivateButton(false);
            ShareBtn.ButtonEvent.MyAction += RpcShareImage;

            // close button
            Sprite[] dontShareBtnImg = { ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.closeButton.png") };
            DontShareBtn = new CustomButton(DirBtn.ButtonGameObject, dontShareBtnImg, "Don't Share");
            DontShareBtn.ScaleSize(0.4f);
            DontShareBtn.SetLocalPosition(new Vector3(-1.2f, 0.8f, -5));
           
            DontShareBtn.ActivateButton(false);
            DontShareBtn.ButtonEvent.MyAction += ToggleShareButton;
        }
     
        private void RpcShareImage() 
        {
            DoomScroll._log.LogInfo("Share Button Clicked!");
            RPCManager.RpcSendChatImage(content);
        }
        
        public string PrintDirectory()
        {
            return " " + Path + " [file]";
        }
    }
}
