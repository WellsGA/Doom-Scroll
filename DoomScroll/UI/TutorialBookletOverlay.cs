using System.Reflection;
using UnityEngine;
using Doom_Scroll.Common;
using Sentry.Internal;

namespace Doom_Scroll.UI
{
    //a static container for a set of methods operating on input parameters without having to get or set any internal instance fields.
    public static class TutorialBookletOverlay
    {
        public static CustomButton CreateTutorialBookletBtn(bool inLobby, GameObject parent)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector2 size = sr ? sr.size : new Vector2(1f, 1f);
            if (inLobby)
            {
                Vector3 pos = parent.transform.localPosition;
                Vector3 position = new(pos.x + size.x * 3.7f, pos.y + size.y * 5f, pos.z - 100f);
                Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tutorialBookletToggle.png", ImageLoader.slices2);
                CustomButton tutorialBookletBtn = new CustomButton(parent, "TutorialBookletToggleButton", btnSprites, position, size.x);
                return tutorialBookletBtn;
            }
            else
            {
                HudManager hud = HudManager.Instance;
                DoomScroll._log.LogInfo("TRYING TO ADD TUTORIAL BOOKLET BUTTON TO HUDMANAGER INSTEAD OF LOBBY.");
                //   NEW CODE TO SET UP BUTTON:
                Vector3 position = new Vector3(-9.7f, -6.6f, 0);
                Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tutorialBookletToggle.png", ImageLoader.slices2);
                CustomButton tutorialBookletBtn = new CustomButton(parent, "TutorialBookletToggleButton", btnSprites, position, size.x);
                return tutorialBookletBtn;
            }
        }

        public static CustomModal CreateTutorialBookletOverlay(bool inLobby, GameObject parent, CustomButton toggler)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Vector2 size = bounds * 1.15f;
            // create the overlay background
            CustomModal tutorialBookletOverlay = new CustomModal(parent, "TutorialBookletOverlay", spr, toggler, true);

            if (inLobby)
            {
                tutorialBookletOverlay.SetLocalPosition(new Vector3(0f, 2.2f, -50f));
                tutorialBookletOverlay.SetSize(size);
            }
            else
            {
                tutorialBookletOverlay.SetLocalPosition(new Vector3(-5.75f, -3.5f, -50f));
                tutorialBookletOverlay.SetSize(size);
            }
            // deactivate by default
            tutorialBookletOverlay.ActivateCustomUI(false);
            return tutorialBookletOverlay;
        }

    }
}
