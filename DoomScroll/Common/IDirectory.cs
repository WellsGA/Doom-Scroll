using Doom_Scroll.UI;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public interface IDirectory
    {
        public GameObject Dir { get; }
        public CustomButton Btn { get;}
        public CustomText Label { get; }
        public string Path { get; }
        public void DisplayContent();
        public void HideContent();
        public string PrintDirectory();
    }
}
