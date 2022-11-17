using System.Collections.Generic;
using Doom_Scroll.UI;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public class Folder : IDirectory
    {
        public List<IDirectory> Content { get; private set; }
        public Folder ParentFolder { get; }
        
        public string Name { get; private set; }
        public string Path { get; private set; }
        public CustomButton DirBtn { get; private set; }
        public CustomText Label { get; private set; }
        private Vector2 parentSize;
        public Folder(string parentPath, string name, GameObject parent, Sprite folderImg)
        {
            Name = name;
            Path = parentPath + "/" + name;
            parentSize = parent.GetComponent<SpriteRenderer>().size;
            Content = new List<IDirectory>();
            Sprite[] images = { folderImg };
            DirBtn = new CustomButton(parent, images, name);
            Label = new CustomText(name, DirBtn.ButtonGameObject, name);
            DirBtn.ActivateButton(false);
            DirBtn.ButtonEvent.MyAction += DisplayContent; // play sound, etc. could be added too
        }

        public void AddItem(IDirectory item)
        {
            Content.Add(item);
        }

        public void RemoveItem(IDirectory item)
        {
            Content.Remove(item);
        }

        public void DisplayContent()
        {
            Vector3 pos = new Vector3(0f, 0f, -20f);
            float width = parentSize.x * 0.9f;
            float height = parentSize.y - 1.5f;

            // display items on a 3x2 grid 
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (j + i * 5 < Content.Count)
                    {
                        pos.x = j * width / 3 - width / 2 + 1.2f;
                        pos.y = i * -height / 2 + height / 2 - 1.2f;
                        CustomButton btn = Content[j + i * 5].DirBtn;
                        btn.SetLocalPosition(pos);
                        btn.ScaleSize(width / 3 - 0.1f);
                        CustomText txt = Content[j + i * 3].Label;
                        txt.SetlocalPosition(new Vector3(0, - btn.GetSize().y / 2 - 0.1f, 0));
                        txt.SetSize(1.5f);
                        btn.ActivateButton(true);
                    }
                }
            }
        }
        public void HideContent()
        {
            foreach (IDirectory dir in Content)
            {
                dir.DirBtn.ActivateButton(false);
            }
        }
        public string PrintDirectory()
        {
            string items = "";
            foreach (IDirectory dir in Content)
            {
                items += dir.PrintDirectory();
            }
            return Path + "[ " + items + " ]";
        }

    }
}
