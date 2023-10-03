using Doom_Scroll.Common;
using Doom_Scroll.Patches;
using Hazel;
using System;
using System.Reflection;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public class PostEndorsement
    {
        // endorsement
        public string Id { get; private set; }
        public CustomButton EndorseButton { get; private set; }
        public CustomButton DenounceButton { get; private set; }
        public int TotalEndorsement { get; set; }
        public int TotalDenouncement { get; set; }
        private bool localPlayerEndorsed;
        private bool localPlayerDenounced;

        private GameObject chatBubble;
        private SpriteRenderer chatBubbleSR;

        public PostEndorsement(GameObject parent, string id)
        {
            Id = id;
            chatBubble = parent;
            TotalEndorsement = 0;
            TotalDenouncement = 0;
            localPlayerEndorsed = false;
            localPlayerDenounced = false;
            AddEndorseButtons();
        }

        public void AddEndorseButtons()
        {
            float endorseBtnSize = 0.3f;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] endorseSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.endorse.png", slices);
            Sprite[] unEndorseSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.unEndorse.png", slices);

            EndorseButton = CreateEndorsementButton(endorseSprites, endorseBtnSize, Color.green);
            EndorseButton.ButtonEvent.MyAction += OnClickEndorse;

            DenounceButton = CreateEndorsementButton(unEndorseSprites, endorseBtnSize, Color.red);
            DenounceButton.ButtonEvent.MyAction += OnClickUnEndorse;
        }

        private void OnClickEndorse()
        {
            if (localPlayerDenounced) // if already denounced, it has to change too
            {
                OnClickUnEndorse();
            }
            TotalEndorsement = localPlayerEndorsed ? TotalEndorsement - 1 : TotalEndorsement + 1;
            localPlayerEndorsed = !localPlayerEndorsed;
            EndorseButton.Label.SetText(TotalEndorsement.ToString());
            DoomScroll._log.LogInfo("Endorsed: " + TotalEndorsement);
            RpcShareEndorsement(true, localPlayerEndorsed);
        }

        private void OnClickUnEndorse()
        {
            if (localPlayerEndorsed) // if already endorsed, it has to change too
            {
                OnClickEndorse();
            }
            TotalDenouncement = localPlayerDenounced ? TotalDenouncement - 1 : TotalDenouncement + 1;
            localPlayerDenounced = !localPlayerDenounced;
            DenounceButton.Label.SetText(TotalDenouncement.ToString());
            DoomScroll._log.LogInfo("Denounced: " + TotalDenouncement);
            RpcShareEndorsement(false, localPlayerDenounced);
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

        public void CheckForEndorseClicks()
        {
            // If chat and folder overlay are open invoke events on button clicks
            if (HudManager.Instance.Chat.State == ChatControllerState.Open && !FolderManager.Instance.IsFolderOpen())
            {
                try
                {
                    EndorseButton.ReplaceImgageOnHover();
                    DenounceButton.ReplaceImgageOnHover();
                    if (EndorseButton.IsEnabled && EndorseButton.IsActive && EndorseButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        EndorseButton.ButtonEvent.InvokeAction();
                    }
                    if (DenounceButton.IsEnabled && DenounceButton.IsActive &&  DenounceButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        DenounceButton.ButtonEvent.InvokeAction();
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking overlay button method: " + e);
                }
            }

        }

        private CustomButton CreateEndorsementButton(Sprite[] icons, float size, Color color)
        {
            CustomButton btn = new CustomButton(chatBubble, "Emdorse News", icons);
            btn.SetVisibleInsideMask();
            btn.SetSize(size);
            btn.Label.SetText(TotalEndorsement.ToString());
            btn.Label.SetLocalPosition(new Vector3(0, -size / 2 - 0.05f, 0));
            btn.Label.SetSize(1.2f);
            btn.Label.SetColor(color);

            return btn;
        }
    }
}
