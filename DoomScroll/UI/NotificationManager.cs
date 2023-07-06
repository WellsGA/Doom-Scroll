using Doom_Scroll.Common;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public static class NotificationManager
    {

        // Thread.Sleep() // will stop the app from responding?
        public static async void ShowNotification(string notification)
        {
            // code before delay       
            CustomModal infoModal = CreateInfoModal(notification);
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
