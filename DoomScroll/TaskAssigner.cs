﻿using Hazel;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Doom_Scroll
{
    // Singleton with static initialization: thread safe without explicitly coding for it,
    // relies on the common language runtime to initialize the variable
    public sealed class TaskAssigner
    {
        private static readonly TaskAssigner _instance = new TaskAssigner(); // readonly: can be assigned only during static initialization
        public static TaskAssigner Instance
        {
            get
            {
                return _instance;
            }
        }
        // list for all the assigned task - this will be displayed during meetings
        private static List<(byte playerId, string taskName)> AssignedTasks = new List<(byte, string)>();
        // byte array to hold the assignable task IDs
        public uint[] AssignableTasksIDs { get; private set; }    
        private List<CustomButton> playerButtons;
        private CustomModal playerButtonHolder;
        private Sprite panelSprite;
        private Sprite[] butttonSprite;
        private Sprite playerSprite;

        // private PoolablePlayer playerIcon = null;
        // private constructor: the class cannot be instantiated outside of itself; therefore, this is the only instance that can exist in the system
        private TaskAssigner()
        {
            AssignableTasksIDs = new uint[2];
            playerButtons = new List<CustomButton>();
            panelSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            butttonSprite[0] = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.emptyBtn.png");
            playerSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.playerIcon.png");
            DoomScroll._log.LogInfo("TASK ASSIGNER CONSTRUCTOR");
        }

        public void AddToAssignedTasks(PlayerControl sender, byte playerID, uint taskId)
        {
            string taskName = " o_O ";
            foreach (PlayerTask task in sender.myTasks)
            {
                if(task.Id == taskId)
                {
                    taskName = task.name;
                }
            }
            // add to list  (maybe we need to check if the task was already assigned and change if so??)
            AssignedTasks.Add((playerID, taskName));
            DoomScroll._log.LogInfo("TASK ASSIGNED TO PLAYER: " + playerID + ", TASK ID:" + taskName);
        }

        public void RPCAddToAssignedTasks(byte player, uint task) 
        {
            if (AmongUsClient.Instance.AmClient)
            {
                AddToAssignedTasks(PlayerControl.LocalPlayer, player, task);
            }
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDASSIGNEDTASK, (SendOption)1);
            messageWriter.Write(player);
            messageWriter.Write(task);
            messageWriter.EndMessage();
        }

        public void SelectRandomTasks(Il2CppSystem.Collections.Generic.List<PlayerTask> tasks) 
        {
            AssignedTasks = new List<(byte, string)>();
            if (tasks.Count == 0 || tasks == null) return;
            for (int i = 0; i < AssignableTasksIDs.Length; i++)
            {
                int index = Random.Range(0, tasks.Count - 1);
                AssignableTasksIDs[i] = tasks[index].Id;
                DoomScroll._log.LogInfo("You Can Assign: " + tasks[index].name + "(" + tasks[index].Id.ToString() + ")" );
            }
        }

        public void AssignPlayerToTask(uint taskId, byte playerId)
        {
            RPCAddToAssignedTasks(playerId, taskId);       
        }

        public void DisplayAssignedTasks()
        {
            // to do: list it on a UI modal 
            DoomScroll._log.LogInfo("TASKS ASSIGNED SO FAR:\n " + ToString()); // debug
        }

        public override string ToString()
        {
            string assignedTasks = "<NAME>\t\t<TASK> \n  " +
                                   "=====================================\n";
            foreach (var entry in AssignedTasks)
            {
                GameData.PlayerInfo player = GameData.Instance.GetPlayerById(entry.playerId);
                if (player == null) { continue; } // if player has left, we leave them out
                assignedTasks += player.PlayerName + "\t\t" + entry.taskName + "\n";
            }
            return assignedTasks;
        }

        public void CreateTaskAssignerPanel(GameObject closeBtn)
        {
            GameObject parentPanel = closeBtn.transform.parent.gameObject;
            playerButtonHolder = new CustomModal(parentPanel, "Player buttons", panelSprite);
            Vector2 size = new Vector2(GameData.Instance.AllPlayers.Count, 1f);
            Vector3 pos = new Vector3(closeBtn.transform.localPosition.x + size.x/2, closeBtn.transform.localPosition.y + 0.3f, closeBtn.transform.localPosition.z - 10);
            playerButtonHolder.SetSize(pos);
            playerButtonHolder.SetLocalPosition(pos);
            Vector3 topLeftPos = new Vector3(pos.x + 0.5f, pos.y, pos.z-10);

            // add the players as buttons
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.IsDead)
                {                  
                    CustomButton btn = new CustomButton(parentPanel, playerInfo.PlayerId.ToString(), butttonSprite, topLeftPos, 0.4f);
                    SpriteRenderer sr = btn.AddIconToButton(playerSprite);
                    sr.color = Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId];
                    playerButtons.Add(btn);
                    DoomScroll._log.LogInfo("Playercolor: " + playerInfo.ColorName );
                    topLeftPos.x += 0.5f;
                }
            }
        }

        public void CheckForPlayerButtonClick(uint taskId)
        {
            foreach (CustomButton btn in playerButtons)
            {
                btn.ReplaceImgageOnHover();

                if (btn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    
                }
            }
        }

    }
}
