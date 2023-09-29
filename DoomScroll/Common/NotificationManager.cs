using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.UI;

namespace Doom_Scroll.Common
{
    public static class NotificationManager
    {
        // private static List<CustomModal> newsList = new List<CustomModal>(); 
        // Thread.Sleep() // will stop the app from responding?
        public static async void ShowNotification(string notification)
        {
            // code before delay       
            CustomModal infoModal = CreateInfoModal(notification);
            // newsList.Add(infoModal);
            await Task.Delay(3000);
            // code after delay
            Object.Destroy(infoModal.UIGameObject);
        }

        private static CustomModal CreateInfoModal(string notification)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.notificationModal.png");
            CustomModal infoModal = new CustomModal(HudManager.Instance.gameObject, "Notification modal", spr);
            Vector3 pos = HudManager.Instance.SettingsButton.transform.localPosition;
            infoModal.SetSize(5.5f);
            Vector2 size = infoModal.GetSize();
            infoModal.SetLocalPosition(new Vector3(pos.x - size.x / 2 - 0.2f, pos.y, -50));
            CustomText infoText = new CustomText(infoModal.UIGameObject, "notification", notification);
            infoText.SetSize(1.1f);
            infoText.SetLocalPosition(new Vector3(0.2f, 0, -10));
            return infoModal;
        }
    }
}
