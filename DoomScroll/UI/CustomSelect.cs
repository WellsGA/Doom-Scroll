using System;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public class CustomSelect
    {
        public Dictionary<byte, CustomButton> ButtonList { get; private set; }
        public byte Selected { get; private set; }
        public bool HasSelecrted { get; private set; }
        private Sprite[] buttonIcons;        

        public CustomSelect(byte[] values, GameObject parent, string name, Sprite[] icons)
        {
            buttonIcons = icons;
            foreach(byte value in values)
            {
                CustomButton button = new CustomButton(parent, name, icons);
                ButtonList.Add(value, button);
            }
        }
    }
}
