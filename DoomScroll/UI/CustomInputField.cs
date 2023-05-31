using Doom_Scroll.Common;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Doom_Scroll.UI
{
    internal class CustomInputField : CustomUI
    {
        // inherits UIGameObject from base
        public TextMeshPro PlaceHolder { get; }
        public TextMeshPro TextInput { get; }
        public TMP_InputField TextInputField { get; } 
        public CustomInputField(GameObject parent, string name, string placeholder) : base(parent, name)
        {
            UIGameObject.AddComponent<MeshRenderer>();
            UIGameObject.transform.localScale = Vector3.one;

            TextInputField = UIGameObject.AddComponent<TMP_InputField>();
            TextInputField.image.sprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");

            TextInputField.textComponent.m_enableWordWrapping = true;
            TextInputField.textComponent.m_overflowMode = TextOverflowModes.ScrollRect;

            TextInputField.placeholder = PlaceHolder;
            PlaceHolder.text = placeholder;
            PlaceHolder.alignment = TextAlignmentOptions.Center;
            PlaceHolder.color = Color.black;
        }

        public void SetColor(Color col)
        {
            TextInput.color = col;
        }

        public void SetText(string text)
        {
            TextInput.text = text;
        }

        public override void SetSize(float size)
        {
            TextInput.fontSize = size;
        }
    }
}
