using System;
using Il2CppSystem.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public static class TaskAssignerPanel
    {
        public static void CreateTaskAssignerPanel(GameObject parentPanel) 
        {
            foreach(GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers) 
            {
                if (!playerInfo.IsDead)
                {
                    PoolablePlayer pp = new PoolablePlayer();
                    pp.SetFlipX(false);
                    pp.transform.localScale = Vector3.one;
                    pp.transform.localPosition = parentPanel.transform.position;
                    pp.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
                }
            }
        }
    }
}
