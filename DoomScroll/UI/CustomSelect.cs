﻿using Doom_Scroll.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public class CustomSelect<T>
    {
        public KeyValuePair<T, CustomButton> Selected { get; private set; }
        public bool HasSelected { get; private set; }
        private Vector2 parentSize;
        private Dictionary<T, CustomButton> buttonList;

        public DoomScrollEvent ButtonEvent = new DoomScrollEvent();


        public CustomSelect(Vector2 size)
        {
            parentSize = size;
            HasSelected = false;
            buttonList = new Dictionary<T, CustomButton>();
            Selected = new KeyValuePair<T, CustomButton>();
        }

        public void AddSelectOption(T option, CustomButton Btn)
        {
            buttonList.Add(option, Btn);
        }

        public void ArrangeButtons(float btnSize, int itemsInOneRow, float xPos, float yPos)
        {
            if(buttonList == null) return;;
            int counter = 0;
            Vector3 nextPos = new Vector3(0, 0, -10);
            foreach (CustomButton btn in buttonList.Values)
            {       
                nextPos.x = counter % itemsInOneRow == 0 ? -parentSize.x / 2 + xPos : nextPos.x + btnSize +0.1f;
                counter++;
                nextPos.y = yPos - ((float)Math.Ceiling((decimal)counter / itemsInOneRow) * (btnSize + 0.2f));
                btn.SetSize(btnSize);
                btn.SetLocalPosition(nextPos);
                btn.Label.SetLocalPosition(new Vector3(0, -btn.GetSize().y / 2f-0.015f, -10));
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
                if (select.Value.IsEnabled && select.Value.IsActive && select.Value.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
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

        public void ActivateButtons(bool activate)
        {
            foreach(CustomButton btn in buttonList.Values)
            {
                btn.ActivateCustomUI(activate);
            }
        }

        public void SetParentSize(Vector2 size)
        {
            parentSize = size;
        }
    }
}
