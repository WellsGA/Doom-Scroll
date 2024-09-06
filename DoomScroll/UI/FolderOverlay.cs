using System.Reflection; 
using UnityEngine;
using Doom_Scroll.Common;
// using System.Collections.Generic;

namespace Doom_Scroll.UI
{
    //a static container for a set of methods operating on input parameters without having to get or set any internal instance fields.
    public static class FolderOverlay
    {
        private static Vector2 buttonSize = new Vector2(0.4f, 0.4f);

        public static CustomImage CreateFolderOverlay(GameObject parent)
        {
            GameObject cahtScreen = parent.transform.Find("ChatScreenRoot/ChatScreenContainer").gameObject;
            GameObject bg = cahtScreen.transform.Find("Background").gameObject;
            SpriteRenderer backgroundSR = bg.GetComponent<SpriteRenderer>();
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");

            // create the overlay background
            CustomImage folderOverlay = new CustomImage(cahtScreen, "EvidenceFolder", spr);  
            folderOverlay.SetLocalPosition(new Vector3(0f, 0f, -30f));       
            if (backgroundSR != null)
            {
               folderOverlay.SetSize(new Vector2(backgroundSR.size.x * 0.77f, backgroundSR.size.y/3));
               folderOverlay.SetLocalPosition(new Vector3(-0.25f, backgroundSR.size.y /2 - folderOverlay.GetSize().y /2 - 0.3f, -30));
            }
            else
            {
                folderOverlay.SetScale(parent.transform.localScale * 0.5f);
            }
            return folderOverlay;
        }
 
        public static FileText AddPostsBtn(CustomImage parent)
        {
            FileText posts = new FileText("", "Headlines", parent, FileTextType.NEWS);
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postsTab.png", ImageLoader.slices3);
            posts.Btn.ResetButtonImages(BtnSprites);
            Vector3 pos = parent.UIGameObject.transform.position;
            Vector2 parentSize = parent.GetSize();
            float btnWidth = parentSize.x / 5;
            posts.Btn.SetSize(btnWidth);
            posts.Btn.SetLocalPosition(new Vector3(pos.x - parentSize.x / 2 + btnWidth * 0.6f, pos.y + parentSize.y / 2 - 0.25f, pos.z - 10));
            
            return posts;
        }

        public static FileText AddTasksBtn(CustomImage parent)
        {
            FileText tasks = new FileText("", "Tasks", parent, FileTextType.TASKS);
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tasksTab.png", ImageLoader.slices3);
            tasks.Btn.ResetButtonImages(BtnSprites);
            Vector3 pos = parent.UIGameObject.transform.position;
            Vector2 parentSize = parent.GetSize();
            float btnWidth = parentSize.x / 5;
            tasks.Btn.SetSize(btnWidth);
            tasks.Btn.SetLocalPosition(new Vector3(pos.x - parentSize.x / 2 + btnWidth * 1.6f, pos.y + parentSize.y / 2 - 0.25f, pos.z - 10));
            
            return tasks;   
        }
        public static Folder AddScreenshotsBtn(CustomImage parent)
        {
            Folder screenshots = new Folder("", "Images", parent);
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.imagesTab.png", ImageLoader.slices3);
            screenshots.Btn.ResetButtonImages(BtnSprites);
            Vector3 pos = parent.UIGameObject.transform.position;
            Vector2 parentSize = parent.GetSize();
            float btnWidth = parentSize.x / 5;
            screenshots.Btn.SetSize(btnWidth);
            screenshots.Btn.SetLocalPosition(new Vector3(pos.x - parentSize.x / 2 + btnWidth * 2.6f, pos.y + parentSize.y / 2 - 0.25f, pos.z - 10));
            
            return screenshots;
        }

        public static FileText AddSourcesBtn(CustomImage parent)
        {
            FileText sources = new FileText("", "Sources", parent, FileTextType.SOURCES);
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.sourcesTab.png", ImageLoader.slices3);
            sources.Btn.ResetButtonImages(BtnSprites);
            Vector3 pos = parent.UIGameObject.transform.position;
            Vector2 parentSize = parent.GetSize();
            float btnWidth = parentSize.x / 5;
            sources.Btn.SetSize(btnWidth);
            sources.Btn.SetLocalPosition(new Vector3(pos.x - parentSize.x / 2 + btnWidth * 3.6f, pos.y + parentSize.y / 2 - 0.25f, pos.z - 10));

            return sources;
        }

    }
}
