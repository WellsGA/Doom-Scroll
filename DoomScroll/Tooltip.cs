using Doom_Scroll.Common;
using Doom_Scroll.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.UI;
using HarmonyLib;

namespace Doom_Scroll
{
    public class Tooltip
    {
        private int displayTime;

        public static bool TutorialModeOn { get; private set; } = true;
        public static List<Tooltip> currentTooltips;
        public CustomText TextObject { get; private set; }
        public CustomImage ImageObject { get; private set; }
        public Tooltip(GameObject parent, string toolTipKeyword, string toolTipText, float backgroundSize, Vector3 toolTipLocation, float toolTipFontSize, int waitMilliseconds = 0)
        {
            displayTime = waitMilliseconds;
            CreateToolTipUI(parent, toolTipKeyword, toolTipText, backgroundSize, toolTipLocation, toolTipFontSize);
            ActivateToolTip(Tooltip.TutorialModeOn);
        }
        public static void addToCurrentTooltips(Tooltip newTooltip)
        {
            if (currentTooltips == null)
            {
                currentTooltips = new List<Tooltip>();
            }
            currentTooltips.Add(newTooltip);
        }

        private void CreateToolTipUI(GameObject parent, string toolTipKeyword, string toolTipText, float backgroundSize, Vector3 toolTipLocation, float toolTipFontSize)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tooltipBackground.png");
            ImageObject = new CustomImage(HudManager.Instance.gameObject, $"{toolTipKeyword}ToolTipModal", spr);
            ImageObject.SetSize(backgroundSize);
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
                if (tt != null)
                {
                    tt.ActivateToolTip(TutorialModeOn);
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
        }



    }
}
