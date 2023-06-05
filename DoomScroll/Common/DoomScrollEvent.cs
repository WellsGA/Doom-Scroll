using System;

namespace Doom_Scroll.Common
{
    public class DoomScrollEvent
    {
        public event Action MyAction;
        public void InvokeAction()
        {
            MyAction?.Invoke();
        }
    }
}
