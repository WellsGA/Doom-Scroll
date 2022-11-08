using System.Collections.Generic;
using Doom_Scroll.UI;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public class Folder : IDirectory
    {
        public List<IDirectory> Content { get; private set; }
        public Folder ParentFolder { get; }
        
        private string name;
        private string path;
        private CustomButton folderBtn;
        private CustomText label;
        private Vector2 parentSize;
        public Folder(string parentPath, string name, GameObject parent, Sprite folderImg)
        {
            this.name = name;
            path = parentPath + "/" + name;
            parentSize = parent.GetComponent<SpriteRenderer>().size;
            Content = new List<IDirectory>();
            Sprite[] images = { folderImg };
            folderBtn = new CustomButton(parent, images, parent.transform.position, 0.8f, name);
            label = new CustomText(name, folderBtn.ButtonGameObject, name);
            folderBtn.ActivateButton(false);
        }

        public void AddItem(IDirectory item)
        {
            Content.Add(item);
        }

        public void RemoveItem(IDirectory item)
        {
            Content.Remove(item);
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
            return folderBtn;
        }
        public CustomText GetLabel()
        {
            return label;
        }

        public void DisplayContent()
        {
            Vector3 pos = new Vector3(0f, 0f, -20f);
            float width = parentSize.x * 0.9f;
            float height = parentSize.y - 1.5f;

            // display items on a 5x4 grid 
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j + i * 5 < Content.Count)
                    {
                        pos.x = j * width / 5 - width / 2 + 0.7f;
                        pos.y = i * -height / 4 + height / 2 - 0.7f;
                        CustomButton btn = Content[j + i * 5].GetButton();
                        btn.SetLocalPosition(pos);
                        btn.ScaleSize(width / 5 - 0.1f);
                        CustomText txt = Content[j + i * 5].GetLabel();
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
                dir.GetButton().ActivateButton(false);
            }
        }
        public string PrintDirectory()
        {
            string items = "";
            foreach (IDirectory dir in Content)
            {
                items += dir.PrintDirectory();
            }
            return path + "[ " + items + " ]";
        }

    }
}
