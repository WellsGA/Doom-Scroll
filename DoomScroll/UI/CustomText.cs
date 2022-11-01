using UnityEngine;
using TMPro;

namespace Doom_Scroll.UI
{
    public enum TextPosition
    {
        BELOWPARENT,
        ABOVEPARENT,
        MIDDLE,
        LEFTTOPARENT,
        RIGHTTOPARENT
    }
    public class CustomText
    {
        public GameObject TextObject { get; }
        public TextMeshPro TextMP { get; }
        private MeshRenderer m_meshRenderer;
        private GameObject parent;
        private Vector2 parentSize;



        public CustomText(string name, GameObject parent, string text)
        {
            TextObject = new GameObject();
            TextObject.layer = LayerMask.NameToLayer("UI");
            TextObject.name = name;
            this.parent = parent;
            TextObject.transform.SetParent(this.parent.transform);

            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            parentSize = sr ? sr.size / 2 : new Vector2(0.5f, 0.5f);

            // sets the defaul position under the parent object
            SetPosition(TextPosition.BELOWPARENT);
            m_meshRenderer = TextObject.AddComponent<MeshRenderer>();
            TextMP = TextObject.AddComponent<TextMeshPro>();
            TextMP.text = text;
            TextMP.m_enableWordWrapping = true;
            TextMP.alignment = TextAlignmentOptions.Center;
            TextMP.color = Color.black;
            TextMP.fontSize = parentSize.y * 4f;
        }

        public void SetPosition(TextPosition pos)
        {
            Vector3 position = parent.transform.localPosition;
            switch (pos)
            {
                case TextPosition.BELOWPARENT:
                    TextObject.transform.localPosition = new Vector3(position.x, position.y - parentSize.y + 0.1f, position.z);
                    return;
                case TextPosition.ABOVEPARENT:
                    TextObject.transform.localPosition = new Vector3(position.x, position.y + parentSize.y + 0.1f, position.z);
                    return;
                case TextPosition.LEFTTOPARENT:
                    TextObject.transform.localPosition = new Vector3(position.x - parentSize.y + 0.1f, position.y, position.z);
                    return;
                case TextPosition.RIGHTTOPARENT:
                    TextObject.transform.localPosition = new Vector3(position.x + parentSize.y + 0.1f, position.y, position.z);
                    return;
                case TextPosition.MIDDLE:
                default:
                    TextObject.transform.localPosition = position;
                    return;
            }


        }
    }
}
