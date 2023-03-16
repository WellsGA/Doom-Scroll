using UnityEngine;

namespace Doom_Scroll.UI
{
    public abstract class CustomUI
    {
        public GameObject UIGameObject { get; private set; }
        public CustomUI(GameObject parent, string name) 
        {
            UIGameObject = new GameObject(name);
            UIGameObject.layer = LayerMask.NameToLayer("UI");
            UIGameObject.transform.SetParent(parent.transform);
        }

        public void SetLocalPosition(Vector3 pos)
        {
            UIGameObject.transform.localPosition = pos;
        }

        public void SetScale(Vector3 scale)
        {
            UIGameObject.transform.localScale = scale;
        }

        public virtual void ActivateCustomUI(bool value)
        {
            UIGameObject.SetActive(value);
        }
        public abstract void SetSize(float size);

    }
}
