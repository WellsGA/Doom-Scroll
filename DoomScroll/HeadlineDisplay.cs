﻿using Doom_Scroll.Common;
using Doom_Scroll.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

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
        private readonly int maxNewsItemsPerPage = 3; // THIS VALUE SHOULD NOT BE CHANGED IN CLASS
        private readonly int maxNewsItemsPerPageVote = 7; // THIS VALUE SHOULD NOT BE CHANGED IN CLASS

        public List<Headline> AllNewsList { get; private set; }
        public Dictionary<byte, Tuple<int, int>> PlayerScores; // player id, and nr of correct and incorrect votes for headlines
        public List<HeadlineEndorsement> endorsementList = new List<HeadlineEndorsement>();

        private HudManager hudManagerInstance;

        private Pageable folderNewsPageHolder;
        private Pageable votingNewsPageHolder;
        private static Tooltip voteForHeadlinesTooltip;
        private int numPages = 1;
        
        // headline voting
        public float discussionStartTimer;
        public bool HasHeadlineVoteEnded { get; private set; }
        public bool HasFinishedSetup { get; private set; }

        private HeadlineDisplay()
        {
            AllNewsList = new List<Headline>();
            PlayerScores = new Dictionary<byte, Tuple<int, int>>();
            InitHeadlineDisplay();
        }

        public void InitHeadlineDisplay()
        {
            hudManagerInstance = HudManager.Instance;   
            ResetHeadlineVotes();
            numPages = (int)Math.Ceiling((float)(AllNewsList.Count) / FileText.maxNumTextItems);
            DoomScroll._log.LogInfo("Number of pages of news: " + numPages);
            // pagination
            CustomImage parent = FolderManager.Instance.GetFolderArea();
            folderNewsPageHolder = new Pageable(parent.UIGameObject.GetComponent<SpriteRenderer>(), new List<CustomUI>(), maxNewsItemsPerPage); // sets up an empty pageable
        }

        public void CheckForShareClicks()
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

        public void CheckForTrustClicks()
        {
            if (hudManagerInstance == null || HasHeadlineVoteEnded || !HasFinishedSetup) return;
            // If chat and folder overlay are open invoke events on button clicks
            if (hudManagerInstance.Chat.State == ChatControllerState.Closed && !FolderManager.Instance.IsFolderOpen())
            {
                try
                {
                    foreach (Headline news in AllNewsList)
                    {
                        news.CheckForTrustSelect();
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking trust click method: " + e);
                }
            }
        }
        public void AddNews(Headline news)
        {
            NotificationManager.QueuNotification("News posted\n " + news.Title + " [" + news.Source + "]");
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
            CustomImage parent = FolderManager.Instance.GetFolderArea();
            List<CustomUI> newsCards = new List<CustomUI>();
            Vector3 pos = new Vector3(-0.25f, parent.GetSize().y / 2 - 0.25f, -10);
            int numOnPage = 0;
            foreach (Headline newsPost in AllNewsList)
            {
                DoomScroll._log.LogInfo($"Current News: {newsPost.ToString()}");
                Headline news = newsPost;
                pos.y -= news.Card.GetSize().y + 0.04f;
                news.Card.SetLocalPosition(pos);
                news.DisplayCardButtons(false);
                newsCards.Add(news.Card);
                news.Card.ActivateCustomUI(false); // unsure if necessary>
                numOnPage++;
                if (numOnPage >= maxNewsItemsPerPage)
                {
                    numOnPage = 0;
                    pos = new Vector3(-0.25f, parent.GetSize().y / 2 - 0.25f, -10);
                }
            }
            // always show page 1 first
            if (folderNewsPageHolder == null || folderNewsPageHolder.ThisParent == null)
            {
                DoomScroll._log.LogInfo($"Creating new pageable");
                folderNewsPageHolder = new Pageable(parent.UIGameObject.GetComponent<SpriteRenderer>(), newsCards, maxNewsItemsPerPage); // sets up an empty pageable 
            }
            else
            {
                DoomScroll._log.LogInfo($"Updating pageable");
                folderNewsPageHolder.UpdatePages(newsCards);
            }
            folderNewsPageHolder.DisplayPage(1);

        }
        public void SetUpVoteForHeadlines(SpriteRenderer glass)
        {
            // stop chat and voting, set screen for headline trust selection
            voteForHeadlinesTooltip = new Tooltip(glass.gameObject, "VoteForHeadlines", "FAKE NEWS:\r\nEmotional/polarizing\r\nHyperbolic\r\nPartisan/biased\r\nMany claims at once\r\nMisleading data\r\nConspiracy theories\r\nTrolling\r\nAttacks opponents\n\nBAD SOURCES:\r\nImpersonators\r\nMisleading domains\r\nUnreliable sponsors\n(blogs, forums)", 2.5f, .6f, new Vector3(-3.3f, 0, 0), 1.4f);
            DisplayHeadlinesForVote(glass);
            HasFinishedSetup = true;
            DoomScroll._log.LogInfo("SETUP OVER");
        }
        public void FinishVoteForHeadlines()
        {
            HideHeadlinesAfterVote();
            HasHeadlineVoteEnded = true;
            DoomScroll._log.LogInfo("VOTING OVER");
        }

        public void ResetHeadlineVotes()
        {
            discussionStartTimer = 0;
            HasHeadlineVoteEnded = false;
            HasFinishedSetup = false;
        }

        public void DisplayHeadlineInFolder()
        {
            DisplayNews();
        }
        public void DisplayHeadlinesForVote(SpriteRenderer parent)
        {
            List<CustomUI> newsCards = new List<CustomUI>();
            int numOnPage = 0;
            Vector3 pos = new Vector3(0, parent.size.y / 2 - 0.8f, -10);
            foreach (Headline news in AllNewsList)
            {
                news.SetParentAndSize(parent.gameObject, parent.size);
                pos.y -= news.Card.GetSize().y + 0.05f;
                news.Card.SetLocalPosition(pos);
                news.DisplayCardButtons(true);
                newsCards.Add(news.Card);
                news.Card.ActivateCustomUI(false); // unsure if necessary>
                numOnPage++;
                if (numOnPage >= maxNewsItemsPerPageVote)
                {
                    numOnPage = 0;
                    pos = new Vector3(0, parent.size.y / 2 - 0.8f, -10);
                }
            }
            // always show page 1 first
            if (votingNewsPageHolder == null || votingNewsPageHolder.ThisParent == null)
            {
                DoomScroll._log.LogInfo($"Creating new pageable");
                votingNewsPageHolder = new Pageable(parent, newsCards, maxNewsItemsPerPageVote, true, true); // sets up an empty pageable 
            }
            else
            {
                DoomScroll._log.LogInfo($"Updating pageable");
                votingNewsPageHolder.UpdatePages(newsCards);
            }
            votingNewsPageHolder.DisplayPage(1);
        }

        public void HideHeadlinesAfterVote()
        {
            voteForHeadlinesTooltip.ActivateToolTip(false);
            CustomImage parent = FolderManager.Instance.GetFolderArea();
            foreach (Headline news in AllNewsList)
            {
                news.SetParentAndSize(parent.UIGameObject, parent.GetSize());
                news.Card.ActivateCustomUI(false);
            }
            votingNewsPageHolder.TogglePageButtons(false);
        }

        public void HideNews()
        {
            if (folderNewsPageHolder != null)
            {
                folderNewsPageHolder.HidePage();
            }

            /*foreach(Headline headline in AllNewsList)
            {
                headline.Card.ActivateCustomUI(false);
            }*/
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

        public void UpdateEndorsementList(string postId, bool isEndorse, bool isAddition)
        {
            foreach (HeadlineEndorsement headline in endorsementList)
            {
                DoomScroll._log.LogInfo("POST ID: " + postId + ", ID OF NEXT POST IN THE LIST: " + headline.Id);
                if (headline.Id == postId)
                {
                    if (isEndorse) // endorse
                    {
                        headline.TotalEndorsement = isAddition ? headline.TotalEndorsement + 1 : headline.TotalEndorsement - 1;
                        headline.LikeLabel.SetText(headline.TotalEndorsement.ToString());
                    }
                    else  // denounce
                    {
                        headline.TotalDenouncement = isAddition ? headline.TotalDenouncement + 1 : headline.TotalDenouncement - 1; ;
                        headline.DislikeLabel.SetText(headline.TotalDenouncement.ToString());
                    }
                    DoomScroll._log.LogInfo("=========== Found news!!!! ==============");
                    return;
                }
            }
            DoomScroll._log.LogInfo("=========== Couldn't find news!!!! ==============");
        }

        public string CalculateScoreStrings(byte playerID)
        {
            Tuple<int, int> score = CalculateScores(playerID);
            string scoreString = "\n\t[" + score.Item1 + " correct and " + score.Item2 + " incorrect votes out of " + AllNewsList.Count + "]\n";
            return scoreString;
        }

        public Tuple<int, int> CalculateScores(byte playerID)
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
            Tuple<int, int> score = new Tuple<int, int>(numCorrect, numIncorrect);
            PlayerScores[playerID] = score; // update scores for playerID
            return score;
        }

        public string GetWrongAnswers(byte playerID)
        {
            string wrongAnswers = "";
            foreach (Headline newsPost in AllNewsList)
            {
                if (newsPost.PlayersTrustSelections.ContainsKey(playerID))
                {
                    if (newsPost.PlayersTrustSelections[playerID] != newsPost.IsTrue)
                    {
                        wrongAnswers += newsPost.ToString() + '\n';
                    }
                }
            }
            return wrongAnswers;
        }


        public void CheckForDisplayedNewsPageButtonClicks()
        {
            if (folderNewsPageHolder != null)
            {
                folderNewsPageHolder.CheckForDisplayedPageButtonClicks();
            }
        }
        public void CheckForDisplayedVotingPageButtonClicks()
        {
            if (folderNewsPageHolder != null)
            {
                votingNewsPageHolder.CheckForDisplayedPageButtonClicks();
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
