using System;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public class CustomSelect
    {
        public Dictionary<byte, CustomButton> ButtonList { get; private set; }
        public byte Selected { get; private set; }
        private Sprite[] buttonIcons;

        public CustomSelect(byte[] values, GameObject parent, string name, Sprite[] images)
        {
            buttonIcons = images;
            foreach(byte value in values)
            {
                CustomButton button = new CustomButton(parent, name, images);
                ButtonList.Add(value, button);
            }
        }
    }
}
