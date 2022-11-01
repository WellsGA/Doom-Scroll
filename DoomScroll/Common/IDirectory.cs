using Doom_Scroll.UI;

namespace Doom_Scroll.Common
{
    public interface IDirectory
    {
        public string GetName();
        public string GetPath();
        public CustomButton GetButton();
        public void DisplayContent();
        public void HideContent();
        public string PrintDirectory();
    }
}
