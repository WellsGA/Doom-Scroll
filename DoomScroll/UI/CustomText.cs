﻿using UnityEngine;
using TMPro;

namespace Doom_Scroll.UI
{
    public class CustomText : CustomUI
    {
        // inherits UIGameObject from base
        public TextMeshPro TextMP { get; private set; }
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

        public void SetTextAlignment(TextAlignmentOptions alignType)
        {
            TextMP.alignment = alignType;
        }

        public void SetOverflowMask()
        {
            TextMP.m_StencilValue = 3;
        }
        public Vector2 GetRenderSize()
        {
           return TextMP.bounds.size;
        }

        public override Vector2 GetSize()
        {
            return TextMP.rectTransform.sizeDelta;
        }
    }
}
