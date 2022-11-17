using Doom_Scroll.UI;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public interface IDirectory
    {
        public CustomButton DirBtn { get; }
        public CustomText Label { get; }
        public void DisplayContent();
        public string PrintDirectory();
    }
}
