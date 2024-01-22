using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using static Il2CppSystem.Runtime.Remoting.RemotingServices;

namespace Doom_Scroll
{
    public class SourceDisplay
    {
        private List<CustomImage> sourceList;
        private CustomModal parent;
        private readonly int maxSourceItemsPerPage = 1; // THIS VALUE SHOULD NOT BE CHANGED IN CLASS
        private Pageable sourcesPageHolder;
        public bool AreSourcesDisplayed { get; private set; }

        public SourceDisplay(CustomModal parentModal)
        {
            parent = parentModal;
            sourceList = new List<CustomImage>();
            AreSourcesDisplayed = false;
            string[] rawSources = NewsStrings.SourceDescriptions;
            Sprite panelSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.card.png");

            foreach (string source in rawSources)
            {
                CustomImage card = new CustomImage(parent.UIGameObject, "source card", panelSprite);
                card.SetSize(new Vector2(parent.GetSize().x * 0.8f, parent.GetSize().y * 0.65f));
                CustomText cardText = new CustomText(card.UIGameObject, "source text", source);
                cardText.SetSize(0.9f);
                //cardText.SetTextAlignment(TMPro.TextAlignmentOptions.Left);
                sourceList.Add(card);
            }

            sourcesPageHolder = new Pageable(parent.UIGameObject.GetComponent<SpriteRenderer>(), new List<CustomUI>(), maxSourceItemsPerPage); // sets up an empty pageable
        }

        public void DispaySources()
        {
            Vector3 pos = new Vector3(0, -0.08f, -10);
            List<CustomUI> sourceCards = new List<CustomUI>();
            int numOnPage = 0;
            foreach (CustomImage source in sourceList)
            {
                pos.y -= - source.GetSize().y /2 +0.05f;
                numOnPage++;
                if (numOnPage >= maxSourceItemsPerPage)
                {
                    numOnPage = 0;
                    pos = new Vector3(0, -0.08f, -10);
                }

                source.SetLocalPosition(pos);
                sourceCards.Add(source);
                source.ActivateCustomUI(false); // unsure if necessary>
            }
            // always show page 1 first
            if (sourcesPageHolder == null || sourcesPageHolder.ThisParent == null)
            {
                DoomScroll._log.LogInfo($"Creating new pageable");
                sourcesPageHolder = new Pageable(parent.UIGameObject.GetComponent<SpriteRenderer>(), sourceCards, maxSourceItemsPerPage); // sets up an empty pageable 
            }
            else
            {
                DoomScroll._log.LogInfo($"Updating pageable");
                sourcesPageHolder.UpdatePages(sourceCards);
            }
            sourcesPageHolder.DisplayPage(1);
            AreSourcesDisplayed = true;
        }

        public void HideSources()
        {
            if (sourcesPageHolder == null) return;
            sourcesPageHolder.HidePage();
            AreSourcesDisplayed = false;
        }

        public void CheckForDisplayedSourcesPageButtonClicks()
        {
            if (sourcesPageHolder == null) return;
            sourcesPageHolder.CheckForDisplayedPageButtonClicks();
        }
    }
}
