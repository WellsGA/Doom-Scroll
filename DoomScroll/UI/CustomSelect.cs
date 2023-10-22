using System;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public class CustomSelect<T>
    {
        public Dictionary<T, CustomButton> ButtonList { get; private set; }
        public KeyValuePair<T, CustomButton> Selected { get; private set; }
        public bool HasSelected { get; private set; }
        private Vector3 parentSize;

        public CustomSelect(Vector3 size)
        {
            parentSize = size;
            HasSelected = false;
            ButtonList = new Dictionary<T, CustomButton>();
            Selected = new KeyValuePair<T, CustomButton>();
        }

        public void AddSelectOption(T value, CustomButton Btn)
        {
            ButtonList.Add(value, Btn);
        }

        public void ArrangeButtons(float btnSize, int itemsInOneRow, float xPos, float yPos)
        {
            if(ButtonList == null) return;;
            int counter = 0;
            Vector3 nextPos = new Vector3(0, 0, -10);
            foreach (CustomButton btn in ButtonList.Values)
            {       
                nextPos.x = counter % itemsInOneRow == 0 ? -parentSize.x / 2 + xPos : nextPos.x + 1.5f * btnSize;
                counter++;
                nextPos.y = yPos - ((float)Math.Ceiling((decimal)counter / itemsInOneRow) * (btnSize + 0.2f));
                DoomScroll._log.LogInfo(", X pos" + nextPos.x + ", Y pos" + nextPos.y);
                btn.SetSize(btnSize);
                btn.SetLocalPosition(nextPos);
                btn.Label.SetLocalPosition(new Vector3(0, -btnSize / 1.8f, -10));
                btn.Label.SetSize(1f);
            }
        }

        public void ClearSelection()
        {
            Selected = new KeyValuePair<T, CustomButton>();
            HasSelected = false;
            
        }
        public void RemoveButtons()
        {
            foreach (KeyValuePair<T, CustomButton> item in ButtonList)
            {
                UnityEngine.Object.Destroy(item.Value.UIGameObject);
            }
            ButtonList.Clear();
        }

        public void ListenForSelection()
        {
            foreach (KeyValuePair<T, CustomButton> item in ButtonList)
            {
                item.Value.ReplaceImgageOnHover();
                if (item.Value.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    Select(item);
                }
            }
        }

        private void Select(KeyValuePair<T, CustomButton> current)
        {
            if (HasSelected) 
            {
                Selected.Value.SetButtonSelect(false);
                if (Selected.Key.Equals(current.Key))
                {
                    HasSelected = false;
                    return;
                }
            }
            Selected = current;
            Selected.Value.SetButtonSelect(true);
            HasSelected = true;
        }

    }
}
