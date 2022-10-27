using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doom_Scroll.Common
{
    internal class DoomScrollEvent
    {
        public event Action MyAction;
        public void InvokeAction()
        {
            MyAction?.Invoke();
        }
    }
}
