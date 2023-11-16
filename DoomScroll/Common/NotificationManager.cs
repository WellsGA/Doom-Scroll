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
            CustomImage infoBackground = CreateInfoModal(notification);
            // newsList.Add(notificationBg);
            await Task.Delay(3000);
            // code after delay
            Object.Destroy(infoBackground.UIGameObject);
        }

        private static CustomImage CreateInfoModal(string notification)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.notificationModal.png");
            CustomImage notificationBg = new CustomImage(HudManager.Instance.gameObject, "Notification background", spr);
            Vector3 pos = HudManager.Instance.SettingsButton.transform.localPosition;
            notificationBg.SetSize(5.5f);
            Vector2 size = notificationBg.GetSize();
            notificationBg.SetLocalPosition(new Vector3(pos.x - size.x / 2 - 0.2f, pos.y, -50));
            CustomText infoText = new CustomText(notificationBg.UIGameObject, "notification", notification);
            infoText.SetSize(1.1f);
            infoText.SetLocalPosition(new Vector3(0.2f, 0, -10));
            return notificationBg;
        }
    }
}
