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
        public CustomText EndorseLable { get; private set; }
        public CustomButton DenounceButton { get; private set; }
        public CustomText DenounceLable { get; private set; }
        public int TotalEndorsement { get; set; }
        public int TotalDenouncement { get; set; }
        private bool localPlayerEndorsed;
        private bool localPlayerDenounced;

        private GameObject chatBubble;
        private SpriteRenderer chatBubbleSR;

        public PostEndorsement(GameObject parent, SpriteRenderer parentRenderer, byte playerId)
        {
            Id = playerId.ToString() + Time.deltaTime;
            chatBubble = parent;
            chatBubbleSR = parentRenderer;
            TotalEndorsement = 0;
            TotalDenouncement = 0;
            localPlayerEndorsed = false;
            localPlayerDenounced = false;
            AddEndorseButtons();
        }

        public void AddEndorseButtons()
        {
            float endorseBtnSize = 0.15f; //???
            Vector3 pos = new Vector3(chatBubbleSR.size.x / 2 - endorseBtnSize * 2 - 0.2f, +0.05f, -20);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };

            Sprite[] endorseSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.endorse.png", slices);
            EndorseButton = new CustomButton(chatBubble, "Emdorse News", endorseSprites, pos, endorseBtnSize);
            EndorseButton.ButtonEvent.MyAction += OnClickEndorse;
            EndorseLable = new CustomText(EndorseButton.UIGameObject, "Endorse Label", TotalEndorsement.ToString());
            EndorseLable.SetLocalPosition(new Vector3(0, -EndorseButton.GetSize().x / 2 - 0.05f, -10));
            EndorseLable.SetSize(1.2f);
            EndorseLable.SetColor(Color.green);

            Vector3 pos2 = new Vector3(chatBubbleSR.size.x / 2 - endorseBtnSize - 0.1f, +0.05f, -20);
            Sprite[] unEndorseSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.unEndorse.png", slices);
            DenounceButton = new CustomButton(chatBubble, "UnEndorse News", unEndorseSprites, pos2, endorseBtnSize);
            DenounceLable = new CustomText(DenounceButton.UIGameObject, "Un-Endorse Label", TotalEndorsement.ToString());
            DenounceLable.SetLocalPosition(new Vector3(0, -DenounceButton.GetSize().x / 2 - 0.05f, -10));
            DenounceLable.SetSize(1.2f);
            DenounceLable.SetColor(Color.red);
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
            EndorseLable.SetText(TotalEndorsement.ToString());
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
            DenounceLable.SetText(TotalDenouncement.ToString());
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
            if (HudManager.Instance.Chat.State == ChatControllerState.Open && FolderManager.Instance.IsFolderOpen())
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
    }
}
