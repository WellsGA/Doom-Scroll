﻿using System;
using System.Linq;
using System.Reflection;
using Doom_Scroll.UI;
using Hazel;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Doom_Scroll.UI.CustomButton;

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

        public Pageable(GameObject parent, List<CustomUI> items, int itemsPerPage, bool inFolderSystem = true)
        {
            // set up page buttons
            DoomScroll._log.LogInfo("Tryna add buttons");
            DoomScroll._log.LogInfo(parent);
            m_nextBtn = AddRightButton(parent, inFolderSystem);
            DoomScroll._log.LogInfo("Right button added");
            m_backBtn = AddLeftButton(parent, inFolderSystem);
            m_nextBtn.SetScale(new Vector3(-1, 1, 1));
            DoomScroll._log.LogInfo("Left button added");
            m_nextBtn.ButtonEvent.MyAction += OnClickRightButton;
            m_backBtn.ButtonEvent.MyAction += OnClickLeftButton;
            DoomScroll._log.LogInfo("Button events added");
            m_nextBtn.ActivateCustomUI(false);
            m_backBtn.ActivateCustomUI(false);
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

                if (currentPage == 1)
                {
                    m_backBtn.EnableButton(false);
                }
                else
                {
                    m_backBtn.EnableButton(true);
                }
                if (currentPage == numPages)
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
                //hovers
                if (m_nextBtn != null && m_nextBtn.UIGameObject.active)
                {
                    m_nextBtn.ReplaceImgageOnHover();
                }
                if (m_backBtn != null && m_backBtn.UIGameObject.active)
                {
                    m_backBtn.ReplaceImgageOnHover();
                }
                //clicks
                if (m_nextBtn != null && m_nextBtn.UIGameObject.active && m_nextBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_nextBtn.ButtonEvent.InvokeAction();
                }
                if (m_backBtn != null && m_backBtn.UIGameObject.active && m_backBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_backBtn.ButtonEvent.InvokeAction();
                }
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking overlay button method: " + e);
            }
        }

        public static CustomButton AddLeftButton(GameObject parent, bool inFolderSystem)
        {
            float yPos = inFolderSystem ? -0.3f : 4.5f;
            Vector2 customButtonSize = inFolderSystem ? buttonSize * 0.4f : buttonSize;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", slices);
            Vector3 backPosition = new Vector3(-sr.size.x * 0.41f, yPos, -5f);
            return new CustomButton(parent, "Flip to prior page", backBtnImg, backPosition, customButtonSize.x);

        }
        public static CustomButton AddRightButton(GameObject parent, bool inFolderSystem)
        {
            float yPos = inFolderSystem ? -0.3f : 4.5f;
            Vector2 customButtonSize = inFolderSystem ? buttonSize * 0.4f : buttonSize;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", slices);
            Vector3 forwardPosition = new Vector3(sr.size.x * 0.41f, yPos, -5f);
            return new CustomButton(parent, "Flip to next page", backBtnImg, forwardPosition, -customButtonSize.x);

        }
    }
}