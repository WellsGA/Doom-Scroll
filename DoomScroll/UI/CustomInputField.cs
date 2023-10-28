using Doom_Scroll.Common;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Doom_Scroll.UI
{
    internal class CustomInputField : CustomUI
    {
        // inherits UIGameObject from base 
        public TMP_InputField TextInputField { get; }
        public RectTransform InputRectTransform { get; }
        public GameObject TextArea { get; }
        public Image InputBg { get;}
        public GameObject TextInput { get; }
        public TextMeshPro Text { get; }
        public GameObject PlaceHolder { get; }
        public TextMeshPro PlaceholderText { get; }

        public CustomInputField(GameObject parent, string name, string placeholder, float parentWidth) : base(parent, name)
        {
            InputRectTransform = UIGameObject.AddComponent<RectTransform>();
            InputRectTransform.sizeDelta = new Vector2(parentWidth - 0.2f, 0.3f);
            UIGameObject.transform.localScale = Vector3.one;
            UIGameObject.AddComponent<CanvasRenderer>();
            TextInputField = UIGameObject.AddComponent<TMP_InputField>();
            TextInputField.interactable = true;

            InputBg = UIGameObject.AddComponent<Image>();
            InputBg.sprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.input.png");
            InputBg.raycastTarget = true;

            TextArea = new GameObject("Text Area");
            TextArea.transform.SetParent(UIGameObject.transform);
            RectTransform textAreaRT = TextArea.AddComponent<RectTransform>();
            TextArea.AddComponent<RectMask2D>();
            
            PlaceHolder = new GameObject("Placeholder");
            PlaceHolder.transform.SetParent(TextArea.transform);
            RectTransform placeHolderRT = PlaceHolder.AddComponent<RectTransform>();
            placeHolderRT.sizeDelta = InputRectTransform.sizeDelta;
            PlaceHolder.AddComponent<CanvasRenderer>();

            PlaceholderText = PlaceHolder.AddComponent<TextMeshPro>();
            PlaceholderText.fontSize = 4f;
            PlaceholderText.text = placeholder;
            PlaceholderText.alignment = TextAlignmentOptions.Center;
            PlaceholderText.color = Color.grey;
            PlaceHolder.AddComponent<LayoutElement>();

            TextInput = new GameObject("Text");
            TextInput.transform.SetParent(TextArea.transform);
            RectTransform textInputRT = TextInput.AddComponent<RectTransform>();
            textInputRT.sizeDelta = InputRectTransform.sizeDelta;
            Text = TextInput.AddComponent<TextMeshPro>();
            Text.fontSize = 4f;

            TextInputField.textViewport = textAreaRT;
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

        public void SetFontSize(float size)
        {
            Text.fontSize = size;
        }

        public override void SetSize(float height) // change the height of the input area
        {
            InputRectTransform.sizeDelta = InputRectTransform.sizeDelta + new Vector2(0, height); 
        }
    }
}
