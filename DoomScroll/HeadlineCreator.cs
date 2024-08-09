using AmongUs.GameOptions;
using Doom_Scroll.Common;
using Doom_Scroll.Patches;
using Hazel;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll
{
    public static class HeadlineCreator
    {
        private static Dictionary<byte, int> playerIDHasHelpedFixSabotage = new Dictionary<byte, int>();
        private static Dictionary<byte, bool> playerIDHasStartedSabotage = new Dictionary<byte, bool>();
        public static Dictionary<byte, byte> LastMeetingVotes { get; private set; } = new Dictionary<byte, byte>();

        public static void AddToFixSabotage(byte id)
        {
            if (playerIDHasHelpedFixSabotage == null)
            {
                playerIDHasHelpedFixSabotage = new Dictionary<byte, int>();
            }
            if (playerIDHasHelpedFixSabotage.ContainsKey(id))
            {
                playerIDHasHelpedFixSabotage[id] += 1;
            }
            else
            {
                playerIDHasHelpedFixSabotage[id] = 1;
            }
        }
        public static void AddToStartedSabotage(byte id)
        {
            if (playerIDHasStartedSabotage == null)
            {
                playerIDHasStartedSabotage = new Dictionary<byte, bool>();
            }
            playerIDHasStartedSabotage[id] = true;
        }
        public static void RpcSabotageContribution(bool helpedFix)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDSABOTAGECONTRIBUTION, (SendOption)1);
            messageWriter.Write(helpedFix);    // true mean helped fix sabotage, false means caused sabotage
            messageWriter.EndMessage();
        }
        public static void ResetSabotageTrackers()
        {
            playerIDHasHelpedFixSabotage = new Dictionary<byte, int>();
            playerIDHasStartedSabotage = new Dictionary<byte, bool>();
        }

        // Create post by player
        public static Headline CreateRandomNews(bool protect, PlayerControl player)
        {     
            bool isTrue = Random.value > 0.75f;
            if (isTrue)
            {
                return CreateRandomTrueNews(PlayerControl.LocalPlayer.PlayerId, player, protect);
            }
            else
            {
                return PlayerCreateFakeHeadline(protect, player);
            }
        }

        private static Headline PlayerCreateFakeHeadline(bool protect, PlayerControl player)
        {
            string headline;
            string source;
            if (protect)
            {
                int rand1 = Random.Range(0, NewsStrings.unTrustProtect.Length);
                headline = NewsStrings.unTrustProtect[rand1];
            }
            else
            {
                int rand2 = Random.Range(0, NewsStrings.unTrustFrame.Length);
                headline = NewsStrings.unTrustFrame[rand2];
            }
            headline = ReplaceSymbolsInHeadline(headline, player, GetFinishedTaskCount(player.PlayerId));

            source = SelectSource(NewsStrings.unTrustSource);
            NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(player.PlayerId);
            // do they sign with their misspelled names?
            string randomPlayer = GetRandomPlayer().name; // or player.name
            source = ReplaceSymbolsInSource(source, playerInfo.GetPlayerColorString(), randomPlayer);

            int id = PlayerControl.LocalPlayer.PlayerId * 10 + HeadlineManager.Instance.NewsPostedByLocalPLayer;
            return new Headline(id, PlayerControl.LocalPlayer.PlayerId, headline, false, source);
        }

        // Automatic News Creation - Only if player is host!
        public static Headline CreateRandomFakeNews()
        {
            bool protect = Random.value > 0.5f;
            string headline;
            int randSource = Random.Range(0, NewsStrings.autoUnTrustSource.Length);
            string source = NewsStrings.autoUnTrustSource[randSource]; // no string replace
            if (protect)
            {
                int rand = Random.Range(0, NewsStrings.autoUnTrustProtect.Length);
                headline = NewsStrings.autoUnTrustProtect[rand];
            }
            else
            {
                int rand1 = Random.Range(0, NewsStrings.autoUnTrustFrame.Length);
                headline = NewsStrings.autoUnTrustFrame[rand1];
            }
            PlayerControl pl = GetRandomPlayer();
            headline = ReplaceSymbolsInHeadline(headline, pl, GetFinishedTaskCount(pl.PlayerId));
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + HeadlineManager.Instance.NewsPostedByLocalPLayer;
            return new Headline(id, 255, headline, false, source);
        }

        public static Headline CreateRandomTrueNews(byte sender, PlayerControl pl, bool protect)
        {
            string headline = "";
            int randSource = Random.Range(0, NewsStrings.autoTrustSource.Length);
            string source = NewsStrings.autoTrustSource[randSource]; // no string replace
            bool foundNews = false;
            List<string> types = new List<string> { "task", "sabotage", "sign-in", "role" };
            while (!foundNews)
            {
                int rand = Random.Range(0, types.Count);
                string type = types[rand];
                switch (type)
                {
                    case "task":
                        {
                            int completedTasks = GetFinishedTaskCount(pl.PlayerId);
                            if ((completedTasks > 0 && protect) || (completedTasks == 0 && !protect))
                            {
                                headline = protect ? SelectHeadline(NewsStrings.autoTrustProtect[0]) : SelectHeadline(NewsStrings.autoTrustFrame[0]);
                                headline = ReplaceSymbolsInHeadline(headline, pl, completedTasks);
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
                                headline = ReplaceSymbolsInHeadline(headline, pl, signedInTasks);
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
                            NetworkedPlayerInfo inf = GameData.Instance.GetPlayerById(pl.PlayerId);
                            if ((inf.Role.Role != RoleTypes.Impostor && protect) || (inf.Role.Role == RoleTypes.Impostor && !protect))
                            {
                                int news = Random.Range(0, NewsStrings.autoTrustProtect[2].Length);
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
                            bool hasSabotaged = GetHasStartedSabotage(pl.PlayerId);
                            int hasHelpedFix = GetHasHelpedFixSabotage(pl.PlayerId);

                            if ((hasHelpedFix > 0 && protect) || (hasSabotaged && !protect))
                            {
                                int news = Random.Range(0, NewsStrings.autoTrustProtect[3].Length);
                                headline = protect ? SelectHeadline(NewsStrings.autoTrustProtect[3]) : SelectHeadline(NewsStrings.autoTrustFrame[3]);
                                if (headline.Contains("{X}")) headline = headline.Replace("{X}", pl.name);
                                if (headline.Contains("{#S}")) headline = headline.Replace("{#S}", hasHelpedFix.ToString());
                                foundNews = true;
                            }
                            else
                            {
                                // TO DO
                                types.Remove(type);
                                if (types.Count == 0) goto default;
                            }
                            break;
                        }
                    case "default":
                    default: // none of the headlines appeared correct ...
                        {
                            if (LastMeetingVotes.ContainsKey(pl.PlayerId))
                            {
                                string voter = GameData.Instance.GetPlayerById(pl.PlayerId) == null ? "some one" : GameData.Instance.GetPlayerById(pl.PlayerId).PlayerName;
                                string suspect = GameData.Instance.GetPlayerById(LastMeetingVotes[pl.PlayerId]) == null ? "no one" : GameData.Instance.GetPlayerById(LastMeetingVotes[pl.PlayerId]).PlayerName;

                                if (AmongUsClient.Instance.AmHost)
                                {
                                    GameLogger.Write(GameLogger.GetTime() + " - " + voter + " has voted for " + suspect);
                                }
                                headline = voter + " has voted for " + suspect;
                            }
                            else
                            {
                                headline = pl.name + " had no chance to vote, yet.";
                            }
                            int hlId = PlayerControl.LocalPlayer.PlayerId * 10 + HeadlineManager.Instance.NewsPostedByLocalPLayer;
                            return new Headline(hlId, PlayerControl.LocalPlayer.PlayerId, headline, true, source);
                        }
                }
            }
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + HeadlineManager.Instance.NewsPostedByLocalPLayer;
            return new Headline(id, sender, headline, true, source);
        }

        public static void UpdatePlayerVote(byte voterId, byte suspectId)
        {
            LastMeetingVotes[voterId] = suspectId;
        }

        private static string ReplaceSymbolsInHeadline(string raw, PlayerControl pl, int count)
        {
            if (raw.Contains("{X}")) raw = raw.Replace("{X}", pl.name);
            if (raw.Contains("{Y}")) raw = raw.Replace("{Y}", GetRandomPlayer(pl.PlayerId).name);
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
            int news = Random.Range(0, headlines.Length);
            return headlines[news];
        }

        private static string SelectSource(string[] sources)
        {
            int item = Random.Range(0, sources.Length);
            return sources[item];
        }

        private static PlayerControl GetRandomPlayer()
        {
            int rand = Random.Range(0, PlayerControl.AllPlayerControls.Count);
            return PlayerControl.AllPlayerControls[rand];
        }

        private static PlayerControl GetRandomPlayer(byte playerId)
        {
            bool isAnotherPlayer;
            int rand;
            do
            {
                rand = Random.Range(0, PlayerControl.AllPlayerControls.Count);
                isAnotherPlayer = playerId == PlayerControl.AllPlayerControls[rand].PlayerId;
            }while(isAnotherPlayer);
            return PlayerControl.AllPlayerControls[rand];
        }
        private static int GetFinishedTaskCount(byte id)
        {
            int count = 0;
            NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(id);
            if(player.Role.Role == RoleTypes.Impostor) return count;
            foreach (NetworkedPlayerInfo.TaskInfo task in player.Tasks)
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
        private static int GetHasHelpedFixSabotage(byte id)
        {
            if (playerIDHasHelpedFixSabotage.ContainsKey(id))
            {
                return playerIDHasHelpedFixSabotage[id];
            }
            else
            {
                DoomScroll._log.LogInfo("Player ID for HasHelpedFixSabotage not in Dictionary?");
                return 0;
            }
        }

        private static bool GetHasStartedSabotage(byte id)
        {
            if (playerIDHasStartedSabotage.ContainsKey(id))
            {

                return playerIDHasStartedSabotage[id];
            }
            else
            {
                DoomScroll._log.LogInfo("Player ID for HasStartedSabotage not in Dictionary?");
                return false;
            }
        }

        private static string RemoveRandomLetter(string name)
        {
            int rand = Random.Range(0, name.Length);
            name = name.Remove(rand, 1);
            return name;
        }
    }
}
