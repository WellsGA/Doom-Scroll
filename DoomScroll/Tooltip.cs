using Doom_Scroll.Common;
using Doom_Scroll.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;

namespace Doom_Scroll
{
    public class Tooltip
    {
        private int displayTime;

        public static bool TutorialModeOn { get; private set; } = true;
        public static List<Tooltip> currentTooltips;
        public CustomText TextObject { get; private set; }
        public CustomImage ImageObject { get; private set; }
        public Tooltip(GameObject parent, string toolTipKeyword, string toolTipText, float backgroundSize, float backgroundXMultiplier, Vector3 toolTipLocation, float toolTipFontSize, int waitMilliseconds = 0)
        {
            displayTime = waitMilliseconds;
            CreateToolTipUI(parent, toolTipKeyword, toolTipText, backgroundSize, backgroundXMultiplier, toolTipLocation, toolTipFontSize);
            ActivateToolTip(Tooltip.TutorialModeOn);
            addToCurrentTooltips(this);
        }
        private static void addToCurrentTooltips(Tooltip newTooltip)
        {
            if (currentTooltips == null)
            {
                DoomScroll._log.LogInfo("Current tooltip for FIRST ENTRY POINT: " + (string)newTooltip.TextObject.TextMP.text);
                ResetCurrentTooltips();
            }
            currentTooltips.Add(newTooltip);
        }

        private void CreateToolTipUI(GameObject parent, string toolTipKeyword, string toolTipText, float backgroundSize, float backgroundXMultiplier, Vector3 toolTipLocation, float toolTipFontSize)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tooltipBackground.png");
            ImageObject = new CustomImage(parent, $"{toolTipKeyword}ToolTipModal", spr);
            ImageObject.SetSize(backgroundSize);
            ImageObject.SetSize(new Vector3(ImageObject.GetSize().x*backgroundXMultiplier, ImageObject.GetSize().y));
            ImageObject.SetLocalPosition(toolTipLocation);
            ImageObject.UIGameObject.layer = LayerMask.NameToLayer("UI");

            TextObject = new CustomText(ImageObject.UIGameObject, $"{toolTipKeyword}ToolTipText", toolTipText);
            TextObject.SetColor(Color.yellow);
            TextObject.SetSize(toolTipFontSize);
            TextObject.SetLocalPosition(new Vector3(0, 0, -20));

            //ImageObject.UIGameObject.transform.SetParent(parent.transform);
            ImageObject.ActivateCustomUI(false);
        }

        public async void ActivateToolTip(bool on)
        {
            if (TutorialModeOn)
            {
                ImageObject.ActivateCustomUI(on);
                DoomScroll._log.LogInfo("Tooltip should be activated/deactivated!");

                if (on == true && displayTime != 0)
                {
                    await Task.Delay(displayTime);
                    ImageObject.ActivateCustomUI(!on);
                    DoomScroll._log.LogInfo("Tooltip deactivated after set time!");
                }
            }
            else
            {
                ImageObject.ActivateCustomUI(false);
                DoomScroll._log.LogInfo("Tooltip should be deactivated!");
            }
        }
        public static void ToggleTutorialMode()
        {
            TutorialModeOn = !TutorialModeOn;
            updateCurrentTooltipsActivation();
            DoomScroll._log.LogInfo($"TutorialModeOn toggled to {TutorialModeOn}!");
        }

        private static void updateCurrentTooltipsActivation()
        {
            foreach (Tooltip tt in currentTooltips)
            {
                DoomScroll._log.LogInfo("Found a tooltip!");
                if (tt.ImageObject.UIGameObject != null)
                {
                    try
                    {
                        DoomScroll._log.LogInfo("Current tooltip to toggle:" + tt.ToString());
                        tt.ActivateToolTip(TutorialModeOn);
                    }
                    catch (System.Exception e)
                    {
                        DoomScroll._log.LogError("Tooltip couldn't be toggled due to some error: " + e);
                        GameObject.Destroy(tt.ImageObject.UIGameObject);
                        currentTooltips.Remove(tt);
                        DoomScroll._log.LogInfo("Error-causing Tooltip destroyed and removed!");
                    }
                }
                else
                {
                    GameObject.Destroy(tt.ImageObject.UIGameObject);
                    currentTooltips.Remove(tt);
                    DoomScroll._log.LogInfo("Tooltip destroyed and removed!");
                }
            }
        }

        public static void ResetCurrentTooltips()
        {
            if (currentTooltips != null)
            {
                foreach (Tooltip tt in currentTooltips)
                {
                    if (tt != null)
                    {
                        GameObject.Destroy(tt.ImageObject.UIGameObject);
                    }
                }
            }
            currentTooltips = new List<Tooltip>();
            DoomScroll._log.LogInfo("CurrentTooltips reset!");
            DoomScroll._log.LogInfo("CurrentTooltips:" + currentTooltips.ToString());
        }


        // Tutorial mode handling!


        public static CustomButton CreateTutorialModeToggleBtn(GameObject parent, Vector3 position)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector2 size = sr ? sr.size : new Vector2(1f, 1f);
            Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tutorialModeToggle.png", ImageLoader.slices2);
            Sprite[] threeBtnSprites = new Sprite[] { btnSprites[0], btnSprites[1], btnSprites[1] };
            CustomButton tutorialModeBtn = new CustomButton(parent, "TutorialBookletToggleButton", threeBtnSprites, position, size.x);
            tutorialModeBtn.ActivateCustomUI(false);
            tutorialModeBtn.ButtonEvent.MyAction += Tooltip.ToggleTutorialMode;

            // Add tooltip as well
            Tooltip toggleBtnTooltip = new Tooltip(tutorialModeBtn.UIGameObject, "TutorialModeToggleBtn", "Click this button to activate or\ndeactivate Tooltips such as this one", 0.5f, 3.3f, new Vector3(0, 0.5f, 0), 1f);
            toggleBtnTooltip.ActivateToolTip(TutorialModeOn);

            return tutorialModeBtn;
        }

        public override string ToString()
        {
            return (string)TextObject.TextMP.text;
        }


    }
}
