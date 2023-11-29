using System;
using System.Reflection;
using Doom_Scroll.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public class Pageable
    {
        private static Vector2 buttonSize = new Vector2(1.5f, 1.5f);
        //public GameObject Dir { get; private set; }
        //public string Path { get; private set; }
        //public CustomButton Btn { get; set; }
        private List<List<CustomUI>> pageList; // A list of pages. Each page is a list, too.

        // flipping between pages
        private int numPages = 1;
        private int currentPage = 1;
        private int maxItemsInPage = 1;
        private CustomButton m_nextBtn;
        private CustomButton m_backBtn;

        public Pageable(CustomModal parent, List<CustomUI> items, int itemsPerPage, bool inFolderSystem = true)
        {
            // set up page buttons
            DoomScroll._log.LogInfo("Tryna add buttons");
            DoomScroll._log.LogInfo(parent.UIGameObject.name);
            m_nextBtn = AddRightButton(parent, inFolderSystem);
            DoomScroll._log.LogInfo("Right button added");
            m_backBtn = AddLeftButton(parent, inFolderSystem);
            m_nextBtn.SetScale(new Vector3(-1, 1, 1));
            DoomScroll._log.LogInfo("Left button added");
            m_nextBtn.ButtonEvent.MyAction += OnClickRightButton;
            m_backBtn.ButtonEvent.MyAction += OnClickLeftButton;
            DoomScroll._log.LogInfo("Button events added");
            m_nextBtn.ActivateCustomUI(false);
            m_nextBtn.EnableButton(true);
            m_backBtn.ActivateCustomUI(false);
            m_backBtn.EnableButton(true);
            DoomScroll._log.LogInfo("Buttons deactivated");
            
            maxItemsInPage = itemsPerPage;
            UpdatePages(items);
        }
        public void UpdatePages(List<CustomUI> items)
        {
            List<List<CustomUI>> pages = new List<List<CustomUI>>();
            int calculatedNumPages = (int)Math.Ceiling((1.0 * items.Count) / maxItemsInPage);
            DoomScroll._log.LogInfo($"{items.Count} items divided by {maxItemsInPage} items per page = {calculatedNumPages}");
            for (int currPage = 1; currPage <= calculatedNumPages; currPage++)
            {
                List<CustomUI> currentPage = new List<CustomUI>();
                for (int itemsOnPage = 0; itemsOnPage < maxItemsInPage && (maxItemsInPage * currPage - maxItemsInPage + itemsOnPage) < items.Count; itemsOnPage++)
                {
                    DoomScroll._log.LogInfo($"about to use this index: {maxItemsInPage * currPage - maxItemsInPage + itemsOnPage}");
                    DoomScroll._log.LogInfo($"page {currPage}, item: {items[maxItemsInPage * currPage - maxItemsInPage + itemsOnPage]}");
                    currentPage.Add(items[maxItemsInPage * currPage - maxItemsInPage + itemsOnPage]);
                }
                pages.Add(currentPage);
            }
            pageList = pages;
            numPages = calculatedNumPages;
            currentPage = 1;
            TogglePageButtons(true);

            DoomScroll._log.LogInfo($"Entering HidePage from UpdatePages");
            HidePage();
        }

        public void DisplayPage(int displayPageNum)
        {
            if (numPages != 0)
            {
                HidePage();
                DoomScroll._log.LogInfo($"Entering HidePage()!");
                if (displayPageNum < 1 || displayPageNum > numPages)
                {
                    DisplayPage(1); // there will always be at least one page
                }

                currentPage = displayPageNum;

                foreach (CustomUI item in pageList[currentPage - 1])
                {
                    DoomScroll._log.LogInfo($"Showed item {item} on page {currentPage}");
                    item.ActivateCustomUI(true);
                }
                TogglePageButtons(true);
                DoomScroll._log.LogInfo($"Showed buttons");
                DoomScroll._log.LogInfo("Opening page");

                if (currentPage <= 1)
                {
                    m_backBtn.EnableButton(false);
                }
                else
                {
                    m_backBtn.EnableButton(true);
                }
                if (currentPage >= numPages)
                {
                    m_nextBtn.EnableButton(false);
                }
                else
                {
                    m_nextBtn.EnableButton(true);
                }
            }
            else
            {
                TogglePageButtons(true);
                m_backBtn.EnableButton(false);
                m_nextBtn.EnableButton(false);
                DoomScroll._log.LogInfo($"Showed buttons");
                DoomScroll._log.LogInfo("Opening page");
            }
        }
        public void HidePage()
        {
            if (numPages != 0)
            {
                DoomScroll._log.LogInfo($"Entering HidePage()!");
                foreach (CustomUI item in pageList[currentPage - 1])
                {
                    DoomScroll._log.LogInfo($"Hid item {item} on page {currentPage}");
                    item.ActivateCustomUI(false);
                }
            }
            TogglePageButtons(false);
            DoomScroll._log.LogInfo($"Hid buttons");
            DoomScroll._log.LogInfo("Closing page");
        }
        public void TogglePageButtons(bool activate)
        {
            if (m_nextBtn != null && m_backBtn != null)
            {
                m_nextBtn.ActivateCustomUI(activate);
                m_backBtn.ActivateCustomUI(activate);
                DoomScroll._log.LogInfo("Page buttons activated or deactivated");
            }
        }

        public void OnClickRightButton()
        {
            HidePage();
            DisplayPage(currentPage + 1);
        }

        public void OnClickLeftButton()
        {
            HidePage();
            DisplayPage(currentPage - 1);
        }

        public void CheckForDisplayedPageButtonClicks()
        {
            try
            {
                //next
                if (m_nextBtn != null && m_nextBtn.UIGameObject.activeSelf)
                {
                    m_nextBtn.ReplaceImgageOnHover();

                    if(m_nextBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0) && m_nextBtn.IsEnabled)
                    {
                        m_nextBtn.ButtonEvent.InvokeAction();
                    }
                }

                //back
                if (m_backBtn != null && m_backBtn.UIGameObject.activeSelf)
                {
                    m_backBtn.ReplaceImgageOnHover();

                    if(m_backBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0) && m_backBtn.IsEnabled)
                    {
                        m_backBtn.ButtonEvent.InvokeAction();
                    }
                }
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking overlay button method: " + e);
            }
        }

        public static CustomButton AddLeftButton(CustomModal parent, bool inFolderSystem)
        {
            float yPos = inFolderSystem ? -0.3f : 1.2f;
            Vector2 customButtonSize = inFolderSystem ? buttonSize * 0.4f : buttonSize*0.35f;
            Vector2 parentSize = parent.GetSize();
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", ImageLoader.slices2);
            Vector3 backPosition = new Vector3(-parentSize.x * 0.41f, yPos, -5f);
            return new CustomButton(parent.UIGameObject, "Flip to prior page", backBtnImg, backPosition, customButtonSize.x);

        }
        public static CustomButton AddRightButton(CustomModal parent, bool inFolderSystem)
        {
            float yPos = inFolderSystem ? -0.3f : 1.2f;
            Vector2 customButtonSize = inFolderSystem ? buttonSize * 0.4f : buttonSize*0.35f;
            Vector2 parentSize = parent.GetSize();
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", ImageLoader.slices2);
            Vector3 forwardPosition = new Vector3(parentSize.x * 0.41f, yPos, -5f);
            return new CustomButton(parent.UIGameObject, "Flip to next page", backBtnImg, forwardPosition, customButtonSize.x);

        }
    }
}
