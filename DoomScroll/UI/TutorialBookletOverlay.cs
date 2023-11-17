using System.Reflection;
using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll.UI
{
    //a static container for a set of methods operating on input parameters without having to get or set any internal instance fields.
    public static class TutorialBookletOverlay
    {
        public static CustomButton CreateTutorialBookletBtn(GameObject parent)
        {
            Vector3 pos = parent.transform.localPosition;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector2 size = sr ? sr.size : new Vector2(1f, 1f);
            Vector3 position = new(pos.x + size.x * 3.7f, pos.y + size.y * 5f, pos.z - 100f);
            Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tutorialBookletToggle.png", ImageLoader.slices2);
            CustomButton tutorialBookletBtn = new CustomButton(parent, "TutorialBookletToggleButton", btnSprites, position, size.x);
            return tutorialBookletBtn;
        }

        public static CustomModal CreateTutorialBookletOverlay(GameObject parent, CustomButton toggler)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Vector2 size = bounds * 0.95f;
            // create the overlay background
            CustomModal tutorialBookletOverlay = new CustomModal(parent, "TutorialBookletOverlay", spr, toggler, true);
            tutorialBookletOverlay.SetLocalPosition(new Vector3(0f, 1.5f, -50f));
            tutorialBookletOverlay.SetSize(size);
            // deactivate by default
            tutorialBookletOverlay.ActivateCustomUI(false);
            return tutorialBookletOverlay;
        }

    }
}
