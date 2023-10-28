using Doom_Scroll.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public class CustomSelect<T>
    {
        public KeyValuePair<T, CustomButton> Selected { get; private set; }
        public bool HasSelected { get; private set; }
        private Vector3 parentSize;
        private Dictionary<T, CustomButton> buttonList;

        public DoomScrollEvent ButtonEvent = new DoomScrollEvent();


        public CustomSelect(Vector3 size)
        {
            parentSize = size;
            HasSelected = false;
            buttonList = new Dictionary<T, CustomButton>();
            Selected = new KeyValuePair<T, CustomButton>();
        }

        public void AddSelectOption(T value, CustomButton Btn)
        {
            buttonList.Add(value, Btn);
        }

        public void ArrangeButtons(float btnSize, int itemsInOneRow, float xPos, float yPos)
        {
            if(buttonList == null) return;;
            int counter = 0;
            Vector3 nextPos = new Vector3(0, 0, -10);
            foreach (CustomButton btn in buttonList.Values)
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
            HasSelected = false;
            if (Selected.Value != null) Selected.Value.SelectButton(false);
            Selected = new KeyValuePair<T, CustomButton>();
        }
        public void RemoveButtons()
        {
            foreach (KeyValuePair<T, CustomButton> item in buttonList)
            {
                UnityEngine.Object.Destroy(item.Value.UIGameObject);
            }
            buttonList.Clear();
        }

        public void ListenForSelection()
        {
            foreach (KeyValuePair<T, CustomButton> select in buttonList)
            {
                select.Value.ReplaceImgageOnHover();
                if (select.Value.IsEnabled && select.Value.IsActive && select.Value.IsHovered() && Input.GetKey(KeyCode.Mouse0))
                {
                    Select(select);
                    ButtonEvent?.InvokeAction();
                }
            }
        }

        private void Select(KeyValuePair<T, CustomButton> current)
        {
            if (HasSelected) 
            {
                Selected.Value.SelectButton(false);
                if (Selected.Key.Equals(current.Key))
                {
                    HasSelected = false;
                    return;
                }
            }
            Selected = current;
            Selected.Value.SelectButton(true);
            HasSelected = true;
        }

        public int GetNumberOfOptions()
        {
            return buttonList.Count;
        }
    }
}
