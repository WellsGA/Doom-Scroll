using System.Reflection;
using System.Collections.Generic;
using Doom_Scroll.UI;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public class Folder : IDirectory
    {
        public List<IDirectory> Content { get; private set; }
        public string Path { get; private set; }
        public GameObject Dir { get; private set; }
        public CustomButton Btn { get; private set; }
        private Vector2 parentSize;
        public Folder(string parentPath, string name, CustomUI parentPanel)
        {
            Sprite[] folderEmpty = { ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderEmpty.png") };

            Dir = new GameObject(name);
            Dir.layer = LayerMask.NameToLayer("UI");
            Dir.transform.SetParent(parentPanel.UIGameObject.transform);
           
            Path = parentPath + "/" + name;
            parentSize = parentPanel.GetSize();
            Content = new List<IDirectory>();
            
            Btn = new CustomButton(Dir, name, folderEmpty);
            Btn.ActivateCustomUI(true);
            Btn.ButtonEvent.MyAction += DisplayContent; // play sound, etc. could be added too
            Btn.UIGameObject.transform.localScale = Vector3.one;

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
            // display items on a 4x2 grid 
            int maxInRow = 4;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j + i * 5 < Content.Count)
                    {
                        pos.x = j * width / 4 - width / 2 + 0.8f;
                        pos.y = i * -height / 2 + height / 2 - 0.8f;
                        GameObject dir = Content[j + i * maxInRow].Dir;
                        CustomButton btn = Content[j + i * maxInRow].Btn;
                        dir.transform.localPosition = pos;
                        btn.SetSize(width / maxInRow - 0.3f);
                        dir.SetActive(true);
                        btn.ActivateCustomUI(true);
                    }
                }
            }
        }
        public void HideContent()
        {
            foreach (IDirectory dir in Content)
            {
                dir.Dir.SetActive(false);
                dir.Btn.ActivateCustomUI(false);
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
