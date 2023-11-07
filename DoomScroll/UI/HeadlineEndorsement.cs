using Doom_Scroll.Common;
using Doom_Scroll.Patches;
using Hazel;
using System;
using System.Reflection;
using UnityEngine;
using static Il2CppSystem.Net.Http.Headers.Parser;

namespace Doom_Scroll.UI
{
    public enum Vote
    {
        LIKE,
        DISLIKE,
        EMPTY
    }
    public class HeadlineEndorsement
    {
        // endorsement
        public string Id { get; private set; }
        public CustomSelect<bool> LikeButtons { get; private set; }
        public int TotalEndorsement { get; set; }
        public int TotalDenouncement { get; set; }
        public CustomText LikeLabel { get; set; }
        public CustomText DislikeLabel { get; set; }
        private Vote vote;
        public HeadlineEndorsement(GameObject chatBubble, Vector2 size, string id)
        {
            Id = id;
            TotalEndorsement = 0;
            TotalDenouncement = 0;
            vote = Vote.EMPTY;

            Sprite[] endorseSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.endorse.png", ImageLoader.slices3);
            Sprite[] unEndorseSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.unEndorse.png", ImageLoader.slices3);

            LikeButtons = new CustomSelect<bool>(size);
            CustomButton likeBtn = CreateEndorsementButton(chatBubble, endorseSprites, 0.3f, Color.black);
            LikeLabel = likeBtn.Label;
            CustomButton dislikeBtn = CreateEndorsementButton(chatBubble, unEndorseSprites, 0.3f, Color.black);
            DislikeLabel = dislikeBtn.Label;
            LikeButtons.AddSelectOption(true, likeBtn);
            LikeButtons.AddSelectOption(false, dislikeBtn);
            LikeButtons.ButtonEvent.MyAction += OnEndorseClick;
        }

        public void CheckForEndorseClicks()
        {
            // If chat and folder overlay are open invoke events on button clicks
            if (HudManager.Instance.Chat.State == ChatControllerState.Open && !FolderManager.Instance.IsFolderOpen())
            {
                try
                {
                    LikeButtons.ListenForSelection();
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking like or dislike button method: " + e);
                }
            }
        }

        public void OnEndorseClick()
        {
            if (LikeButtons.Selected.Value != null)
            {
                SetEndorsementState(LikeButtons.Selected.Key);
                DoomScroll._log.LogInfo("CLICKED: " + LikeButtons.Selected.Key + ", " + LikeButtons.Selected.Value.Label.TextMP.text);
            }
        }

        private void SetEndorsementState(bool like) 
        { 
            switch (vote)
            {
                case Vote.EMPTY:
                    if (like)
                    {
                        TotalEndorsement++;
                        vote = Vote.LIKE;
                        RpcShareEndorsement(true, true);
                    }
                    else
                    {
                        TotalDenouncement++;
                        vote = Vote.DISLIKE;
                        RpcShareEndorsement(false, true);
                    }
                    break;
                case Vote.LIKE:
                    if (like) // deselecting like
                    {
                        TotalEndorsement--;
                        vote = Vote.EMPTY;
                        RpcShareEndorsement(true, false);
                    }
                    else
                    {
                        TotalEndorsement--;
                        TotalDenouncement++;
                        vote = Vote.DISLIKE;
                        RpcShareEndorsement(true, false);
                        RpcShareEndorsement(false, true);
                    }
                    break;
                case Vote.DISLIKE :
                    if (like)
                    {
                        TotalEndorsement++;
                        TotalDenouncement--;
                        vote = Vote.LIKE;
                        RpcShareEndorsement(true, true);
                        RpcShareEndorsement(false, false);
                    }
                    else
                    {
                        TotalDenouncement--;
                        vote = Vote.EMPTY;
                        RpcShareEndorsement(false, false);
                    }
                    break;
            }
            LikeLabel.SetText(TotalEndorsement.ToString());
            DislikeLabel.SetText(TotalDenouncement.ToString());
        }

        private void RpcShareEndorsement(bool up, bool add)
        {
            // share with others
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDENDORSEMENT, (SendOption)1);
            messageWriter.Write(Id);
            messageWriter.Write(up);                // up or down vote
            messageWriter.Write(add);               // add or deduct
            messageWriter.EndMessage();
        }

        private CustomButton CreateEndorsementButton(GameObject chatBubble, Sprite[] icons, float size, Color color)
        {
            CustomButton btn = new CustomButton(chatBubble, "Emdorse News", icons);
            btn.SetVisibleInsideMask();
            btn.SetSize(size);
            // btn.SetLocalPosition(new Vector3(0,0,-10));
            btn.Label.SetText(TotalEndorsement.ToString());
            btn.Label.SetLocalPosition(new Vector3(0, -size / 2 - 0.07f, 0));
            btn.Label.SetSize(1.3f);
            btn.Label.SetColor(color);
            btn.Label.TextMP.ForceMeshUpdate(true, true);
            return btn;
        }
    }
}
