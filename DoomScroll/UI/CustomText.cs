using UnityEngine;
using TMPro;

namespace Doom_Scroll.UI
{
    public class CustomText : CustomUI
    {
        // inherits UIGameObject from base
        public TextMeshPro TextMP { get; }
        public CustomText(GameObject parent, string name, string text) :base(parent, name)
        {
            UIGameObject.AddComponent<MeshRenderer>();
            UIGameObject.transform.localScale = Vector3.one;
            TextMP = UIGameObject.AddComponent<TextMeshPro>();
            TextMP.text = text;
            TextMP.m_enableWordWrapping = true;
            TextMP.alignment = TextAlignmentOptions.Center;
            TextMP.color = Color.black;
        }

        public void SetColor(Color col)
        {
            TextMP.color = col;
        }

        public void SetText(string text)
        {
            TextMP.text = text;
        }

        public override void SetSize(float size)
        {
            TextMP.fontSize = size;
        }

    }
}
