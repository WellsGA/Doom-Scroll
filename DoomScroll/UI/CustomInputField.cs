using BepInEx.Unity.IL2CPP.UnityEngine;
using Doom_Scroll.Common;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.TextField;

namespace Doom_Scroll.UI
{
    internal class CustomInputField : CustomUI
    {
        // inherits UIGameObject from base
        public GameObject PlaceHolder { get; }
        public GameObject TextInput { get; }
        public TextMeshPro Text { get; }
        public TextMeshPro PlaceholderText { get; }
        public TMP_InputField TextInputField { get; }
        public GameObject TextArea { get; }
        public Image InputBg { get;} 
        public CustomInputField(GameObject parent, string name, string placeholder) : base(parent, name)
        {
            UIGameObject.AddComponent<RectTransform>();
            // UIGameObject.transform.localScale = Vector3.one;
            UIGameObject.AddComponent<CanvasRenderer>();
            TextInputField = UIGameObject.AddComponent<TMP_InputField>();
            TextInputField.interactable = true;

            InputBg = UIGameObject.AddComponent<Image>();
            InputBg.sprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            InputBg.raycastTarget = true;

            TextArea = new GameObject("Text Area");
            TextArea.transform.SetParent(UIGameObject.transform);
            RectTransform rt = TextArea.AddComponent<RectTransform>();
            TextArea.AddComponent<RectMask2D>();
            
            PlaceHolder = new GameObject("Placeholder");
            PlaceHolder.transform.SetParent(TextArea.transform);
            PlaceHolder.AddComponent<RectTransform>();
            PlaceHolder.AddComponent<CanvasRenderer>();
            PlaceHolder.AddComponent<RectTransform>();
            PlaceholderText = PlaceHolder.AddComponent<TextMeshPro>();
            PlaceholderText.text = placeholder;
            PlaceholderText.alignment = TextAlignmentOptions.Center;
            PlaceholderText.color = Color.grey;
            PlaceHolder.AddComponent<LayoutElement>();

            TextInput = new GameObject("Text");
            TextInput.transform.SetParent(TextArea.transform);
            TextInput.AddComponent<RectTransform>();
            Text = TextInput.AddComponent<TextMeshPro>();

            TextInputField.textViewport = rt;
            TextInputField.textComponent = Text;
            TextInputField.textComponent.m_enableWordWrapping = true;
            TextInputField.textComponent.m_overflowMode = TextOverflowModes.ScrollRect;

        }

        public void SetColor(Color col)
        {
            Text.color = col;
        }

        public void SetText(string text)
        {
            Text.text = text;
        }

        public override void SetSize(float size)
        {
            Text.fontSize = size;
        }

    }
}
