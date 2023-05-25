using Doom_Scroll.UI;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public interface IDirectory
    {
        public GameObject Dir { get; }
        public CustomButton Btn { get;}
        public CustomText Label { get; }
        public void DisplayContent();
        public string PrintDirectory();
    }
}
