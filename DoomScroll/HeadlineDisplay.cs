using Doom_Scroll.Common;
using Doom_Scroll.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Doom_Scroll
{
    public class HeadlineDisplay
    {
        private static readonly HeadlineDisplay _instance = new HeadlineDisplay(); // readonly: can be assigned only during static initialization
        public static HeadlineDisplay Instance
        {
            get
            {
                return _instance;
            }
        }
        private readonly int maxNewsItemsPerPage = 7; // THIS VALUE SHOULD NOT BE CHANGED IN CLASS

        public List<Headline> AllNewsList { get; private set; }
        private Pageable newsPageHolder;
        private int numPages = 1;
        public Dictionary<byte, string> PlayerScores;
        private HudManager hudManagerInstance;

        private HeadlineDisplay()
        {
            AllNewsList = new List<Headline>();
            PlayerScores = new Dictionary<byte, string>();
            InitHeadlineDisplay();
        }

        public void InitHeadlineDisplay()
        {
            hudManagerInstance = HudManager.Instance;
            // pagination
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            newsPageHolder = new Pageable(parent, new List<CustomUI>(), maxNewsItemsPerPage); // sets up an empty pageable 
        }

        public void CheckForTrustAndShareClicks()
        {
            if (hudManagerInstance == null) return;
            // If chat and folder overlay are open invoke events on button clicks
            if (hudManagerInstance.Chat.State == ChatControllerState.Open && FolderManager.Instance.IsFolderOpen())
            {
                try
                {
                    foreach (Headline news in AllNewsList)
                    {
                        news.CheckForTrustSelect();
                        news.PostButton.ReplaceImgageOnHover();
                        if (news.PostButton.IsEnabled && news.PostButton.IsActive && news.PostButton.IsHovered() && Input.GetKey(KeyCode.Mouse0))
                        {
                            news.PostButton.ButtonEvent.InvokeAction();
                        }
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking share post button method: " + e);
                }
            }
        }

        public void AddNews(Headline news)
        {
            NotificationManager.ShowNotification("News posted\n " + news.Title + " [" + news.Source + "]");
            if (news.AuthorID != 255)
            {
                news.CreateAuthorIcon();
            }
            AllNewsList.Insert(0, news);

            // game log
            if (AmongUsClient.Instance.AmHost)
            {
                string author = news.AuthorID == 255 ? "System" : news.AuthorName;
                GameLogger.Write(GameLogger.GetTime() + " - " + author + " created a headline: " + news.Title + " [" + news.Source + "]");
            }
        }
        // lists posts during meetings
        public void DisplayNews()
        {
            CustomModal parent = FolderManager.Instance.GetFolderArea();

            numPages = (int)Math.Ceiling((float)(AllNewsList.Count) / FileText.maxNumTextItems);
            DoomScroll._log.LogInfo("Number of pages of news: " + numPages);

            List<CustomUI> newsCards = new List<CustomUI>();
            for (int displayPageNum = 1; displayPageNum <= numPages; displayPageNum++)
            {
                Vector3 pos = new Vector3(0, parent.GetSize().y / 2 - 0.8f, -10);
                for (int currentNewsIndex = (displayPageNum - 1) * maxNewsItemsPerPage; currentNewsIndex < AllNewsList.Count && currentNewsIndex < displayPageNum * maxNewsItemsPerPage; currentNewsIndex++)
                // stops before index out of range and before printing tasks that should be on next page
                {
                    DoomScroll._log.LogInfo($"Current News Index: {currentNewsIndex}, allnewsList Count: {AllNewsList.Count}");
                    Headline news = AllNewsList[currentNewsIndex];
                    pos.y -= news.Card.GetSize().y + 0.05f;
                    news.Card.SetLocalPosition(pos);
                    news.DisplayNewsCard();
                    news.PostButton.ActivateCustomUI(true);

                    newsCards.Add(news.Card);
                    news.Card.ActivateCustomUI(false); // unsure if necessary>
                }
            }
            // always show page 1 first
            if (newsPageHolder == null)
            {
                DoomScroll._log.LogInfo($"Creating new pageable");
                newsPageHolder = new Pageable(parent, newsCards, maxNewsItemsPerPage); // sets up an empty pageable 
            }
            else
            {
                DoomScroll._log.LogInfo($"Updating pageable");
                newsPageHolder.UpdatePages(newsCards);
            }
            newsPageHolder.DisplayPage(1);

        }

        public void HideNews()
        {
            if (newsPageHolder != null)
            {
                newsPageHolder.HidePage();
            }
        }

        public Headline GetNewsByID(int id)
        {
            foreach (Headline news in AllNewsList)
            {
                if (news.HeadlineID == id)
                {
                    return news;
                }
            }
            return null;
        }

        public string CalculateEndorsementScores(byte playerID)
        {
            int numCorrect = 0;
            int numIncorrect = 0;
            foreach (Headline newsPost in AllNewsList)
            {
                if (newsPost.PlayersTrustSelections.ContainsKey(playerID))
                {
                    if (newsPost.PlayersTrustSelections[playerID] == newsPost.IsTrue) numCorrect++;
                    else numIncorrect++;
                }
            }
            string score = "\n\t[" + numCorrect + " correct and " + numIncorrect + " incorrect votes out of" + AllNewsList.Count + "]\n";
            PlayerScores[playerID] = score;
            return score;
        }

        public void CheckForDisplayedNewsPageButtonClicks()
        {
            if (newsPageHolder != null)
            {
                newsPageHolder.CheckForDisplayedPageButtonClicks();
            }
        }
        public override string ToString()
        {
            string allnews = "\nNEWS POSTED\n";
            foreach (Headline news in AllNewsList)
            {
                allnews += news.ToString() + "\n";
            }
            return allnews;
        }

        public void Reset()
        {
            AllNewsList.Clear();
            PlayerScores.Clear();
            InitHeadlineDisplay();
        }

    }
}
