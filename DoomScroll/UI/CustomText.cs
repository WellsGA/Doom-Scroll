using UnityEngine;
using TMPro;

namespace Doom_Scroll.UI
{
    public class CustomText
    {
        public GameObject TextObject { get; }
        public TextMeshPro TextMP { get; }
        private Vector2 parentSize;

        public CustomText(string name, GameObject parent, string text)
        {
            TextObject = new GameObject();
            TextObject.layer = LayerMask.NameToLayer("UI");
            TextObject.name = name;
            TextObject.transform.SetParent(parent.transform);
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            parentSize = sr ? sr.size / 2 : new Vector2(0.5f, 0.5f);
            TextObject.AddComponent<MeshRenderer>();
            TextObject.transform.localScale = Vector3.one;
            TextMP = TextObject.AddComponent<TextMeshPro>();
            TextMP.text = text;
            TextMP.m_enableWordWrapping = true;
            TextMP.alignment = TextAlignmentOptions.Center;
            TextMP.color = Color.black;
        }

        public void SetlocalPosition(Vector3 pos)
        {
            TextObject.transform.position = TextObject.transform.parent.transform.position;
            TextObject.transform.localPosition = pos;
            DoomScroll._log.LogInfo("LABEL POS: " + TextObject.transform.position.ToString());
            DoomScroll._log.LogInfo("LABEL LOCAL POS: " + TextObject.transform.localPosition.ToString());
        }

        public void SetText(string text)
        {
            TextMP.text = text;
        }

        public void SetSize(float size)
        {
            TextMP.fontSize = size;
        }

        public void DisplayLabel(bool value) 
        {
            TextObject.SetActive(value);
        }
    }
}
