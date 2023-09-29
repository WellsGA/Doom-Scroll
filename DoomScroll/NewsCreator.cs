﻿using AmongUs.GameOptions;
using Doom_Scroll.Common;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public static class NewsCreator
    {
        // Create post by player
        public static NewsItem CreateRandomNews(bool protect, PlayerControl player)
        {
            string headline;
            string source;
            // bool isTrustworthy = UnityEngine.Random.value > 0.5f;
            if (protect)
            {
                int rand1 = UnityEngine.Random.Range(0, NewsStrings.unTrustProtect.Length);
                headline = NewsStrings.unTrustProtect[rand1];
            }
            else
            {
                int rand2 = UnityEngine.Random.Range(0, NewsStrings.unTrustFrame.Length);
                headline = NewsStrings.unTrustFrame[rand2];
            }
            headline = ReplaceSymbolsInHeadline(headline, player.name, GetFinishedTaskCount(player.PlayerId));

            source = SelectSource(NewsStrings.unTrustSource);
            GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(player.PlayerId);
            source = ReplaceSymbolsInSource(source, playerInfo.GetPlayerColorString(), player.name);

            int id = PlayerControl.LocalPlayer.PlayerId * 10 + NewsFeedManager.Instance.NewsPostedByLocalPLayer;
            return new NewsItem(id, PlayerControl.LocalPlayer.PlayerId, headline, false, source);
        }

        // Automatic News Creation - Only if player is host!
        public static NewsItem CreateRandomFakeNews()
        {
            bool protect = UnityEngine.Random.value > 0.5f;
            string headline;
            int randSource = UnityEngine.Random.Range(0, NewsStrings.autoUnTrustSource.Length);
            string source = NewsStrings.autoUnTrustSource[randSource]; // no string replace
            if (protect)
            {
                int rand = UnityEngine.Random.Range(0, NewsStrings.autoUnTrustProtect.Length);
                headline = NewsStrings.autoUnTrustProtect[rand];
            }
            else
            {
                int rand1 = UnityEngine.Random.Range(0, NewsStrings.autoUnTrustFrame.Length);
                headline = NewsStrings.autoUnTrustFrame[rand1];
            }
            PlayerControl pl = GetRandomPlayer();
            headline = ReplaceSymbolsInHeadline(headline, pl.name, GetFinishedTaskCount(pl.PlayerId));
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + NewsFeedManager.Instance.NewsPostedByLocalPLayer;
            return new NewsItem(id, 255, headline, false, source);
        }

        public static NewsItem CreateRandomTrueNews()
        {
            bool protect = UnityEngine.Random.value > 0.5f;
            PlayerControl pl = GetRandomPlayer();
            string headline = "";
            int randSource = UnityEngine.Random.Range(0, NewsStrings.autoTrustSource.Length);
            string source = NewsStrings.autoTrustSource[randSource]; // no string replace
            bool foundNews = false;
            List<string> types = new List<string> { "task", "sabotage", "sign-in", "role" };
            while (!foundNews)
            {
                int rand = UnityEngine.Random.Range(0, types.Count);
                string type = types[rand];
                switch (type)
                {
                    case "task":
                        {
                            int completedTasks = GetFinishedTaskCount(pl.PlayerId);
                            if ((completedTasks > 0 && protect) || (completedTasks == 0 && !protect))
                            {
                                headline = protect ? SelectHeadline(NewsStrings.autoTrustProtect[0]) : SelectHeadline(NewsStrings.autoTrustFrame[0]);
                                headline = ReplaceSymbolsInHeadline(headline, pl.name, completedTasks);
                                foundNews = true;
                            }
                            else
                            {
                                types.Remove(type);
                                if (types.Count == 0) goto default;
                            }
                            break;
                        }
                    case "sign-in":
                        {
                            int signedInTasks = GetAssignedTasks(pl.PlayerId);
                            if ((signedInTasks > 0 && protect) || (signedInTasks == 0 && !protect))
                            {
                                headline = protect ? SelectHeadline(NewsStrings.autoTrustProtect[1]) : SelectHeadline(NewsStrings.autoTrustFrame[1]);
                                headline = ReplaceSymbolsInHeadline(headline, pl.name, signedInTasks);
                                if (headline.Contains("{TN}")) headline = headline.Replace("{TN}", GetSignedInTaskName(pl.PlayerId).ToString());
                                foundNews = true;
                            }
                            else
                            {
                                types.Remove(type);
                                if (types.Count == 0) goto default;
                            }
                            break;
                        }
                    case "role":
                        {
                            GameData.PlayerInfo inf = GameData.Instance.GetPlayerById(pl.PlayerId);
                            if ((inf.Role.Role != RoleTypes.Impostor && protect) || (inf.Role.Role == RoleTypes.Impostor && !protect))
                            {
                                int news = UnityEngine.Random.Range(0, NewsStrings.autoTrustProtect[2].Length);
                                headline = protect ? SelectHeadline(NewsStrings.autoTrustProtect[2]) : SelectHeadline(NewsStrings.autoTrustFrame[2]);
                                if (headline.Contains("{X}")) headline = headline.Replace("{X}", pl.name);
                                foundNews = true;
                            }
                            else
                            {
                                types.Remove(type);
                                if (types.Count == 0) goto default;
                            }
                            break;
                        }
                    case "sabotage":
                        {
                            // TO DO
                            types.Remove(type);
                            if (types.Count == 0) goto default;
                            break;
                        }
                    default: // none of the headlines appeared correct ...
                        {
                            headline = protect ? 
                                "My cat Sir Wigglesworth loves " + pl.name + ", and Big Wigs hates everyone" :
                                "My cat Santiago hates "  + pl.name + " and baby Teegs can see your soul.";
                            foundNews = true;
                            break;
                        }
                }
            }
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + NewsFeedManager.Instance.NewsPostedByLocalPLayer;
            return new NewsItem(id, 255, headline, true, source);
        }

        private static string ReplaceSymbolsInHeadline(string raw, string name, int count)
        {
            if (raw.Contains("{X}")) raw = raw.Replace("{X}", name);
            if (raw.Contains("{Y}")) raw = raw.Replace("{Y}", GetRandomPlayerName());
            if (raw.Contains("{#}")) raw = raw.Replace("{#}", count.ToString());
            return raw;
        }

        private static string ReplaceSymbolsInSource(string raw, string color, string name)
        {
            if (raw.Contains("{C}")) raw = raw.Replace("{C}", color);
            if (raw.Contains("{N}")) raw = raw.Replace("{N}", name);
            if (raw.Contains("{NR}")) raw = raw.Replace("{NR}", RemoveRandomLetter(name));
            return raw;
        }

        private static string SelectHeadline(string[] headlines)
        {
            int news = UnityEngine.Random.Range(0, headlines.Length);
            return headlines[news];
        }

        private static string SelectSource(string[] sources)
        {
            int item = UnityEngine.Random.Range(0, sources.Length);
            return sources[item];
        }

        private static string GetRandomPlayerName()
        {
            int rand = UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count);
            return PlayerControl.AllPlayerControls[rand].name;
        }

        private static PlayerControl GetRandomPlayer()
        {
            int rand = UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count);
            return PlayerControl.AllPlayerControls[rand];
        }
        private static int GetFinishedTaskCount(byte id)
        {
            int count = 0;
            GameData.PlayerInfo player = GameData.Instance.GetPlayerById(id);
            foreach (GameData.TaskInfo task in player.Tasks)
            {
                if (task.Complete) count++;
            }
            return count;
        }

        private static int GetAssignedTasks(byte id)
        {
            int count = 0;
            foreach (AssignedTask task in TaskAssigner.Instance.AssignedTasks)
            {
                if (id == task.AssigneeId) count++;
            }
            return count;
        }

        private static TaskTypes GetSignedInTaskName(byte id)
        {
            TaskTypes type = TaskTypes.None;
            foreach (AssignedTask task in TaskAssigner.Instance.AssignedTasks)
            {
                if (id == task.AssigneeId) type = task.Type;
            }
            return type;
        }

        private static string RemoveRandomLetter(string name)
        {
            int rand = UnityEngine.Random.Range(0, name.Length);
            name = name.Remove(rand, 1);
            return name;
        }
    }
}
