using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Doom_Scroll.Common
{
    internal class ImageLoader
    {
        public static readonly Vector4[] slices2 = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
        public static readonly Vector4[] slices3 = { new Vector4(0, 0.66f, 1, 1), new Vector4(0, 0.33f, 1, 0.66f), new Vector4(0, 0, 1, 0.33f) };
        public static readonly Vector4[] slices4 = { new Vector4(0, 0.75f, 1, 1), new Vector4(0, 0.5f, 1f, 0.75f), new Vector4(0, 0.25f, 1, 0.5f), new Vector4(0, 0, 1, 0.25f) };

        // makes TMP_Sprite from a byte array
        public static TMP_Sprite ReadTMPSpriteFromByteArray(byte[] imageByte) 
        {
            Sprite sprite = ReadImageFromByteArray(imageByte);
            
            TMP_Sprite tmpSprite = new TMP_Sprite();
            tmpSprite.name = "screenshot";
            tmpSprite.x = sprite.rect.x;
            tmpSprite.y = sprite.rect.y;
            tmpSprite.width = sprite.rect.width;
            tmpSprite.height = sprite.rect.height;
            tmpSprite.xAdvance = sprite.rect.width;
            tmpSprite.xOffset = -2f;
            tmpSprite.yOffset = sprite.rect.height * 0.8f;
            tmpSprite.scale = 1f;
            tmpSprite.id = 1;
            tmpSprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(tmpSprite.name);

            return tmpSprite;
        }


        // makes sprite from a byte array
        public static Sprite ReadImageFromByteArray(byte[] imageByte)
        {
            // uses RGB24 as texture format, will be jpg
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
            ImageConversion.LoadImage(tex, imageByte, false);
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        // the whole image is one sprite
        public static Sprite ReadImageFromAssembly(Assembly assembly, string resource)
        {
            Texture2D tex = ReadTextureFromAssembly(assembly, resource);
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100.0f,
                0,
                SpriteMeshType.FullRect
            );
        }

        // image sliced into separate sprites
        // sprite rect is left to right and bottom to top!!! 
        public static Sprite[] ReadImageSlicesFromAssembly(Assembly assembly, string resource, Vector4[] slices)
        {
            Texture2D tex = ReadTextureFromAssembly(assembly, resource);
            Sprite[] spriteArray = new Sprite[slices.Length];
            for (int i = 0; i < slices.Length; i++)
            {
                spriteArray[i] = Sprite.Create(
                    tex,
                    new Rect(tex.width * slices[i].x, tex.height * slices[i].y, tex.width * slices[i].z - tex.width * slices[i].x, tex.height * slices[i].w - tex.height * slices[i].y),
                    new Vector2(0.5f, 0.5f),
                    100.0f,
                    0,
                    SpriteMeshType.FullRect
                );
            }
            return spriteArray;
        }

        // reads the texture from an file, can be sliced into several sprites
        public static Texture2D ReadTextureFromAssembly(Assembly assembly, string resource)
        {
            // uses ARGB32 as texture format, asset needs to be .png!!
            Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            try
            {
                Stream stream = assembly.GetManifestResourceStream(resource);
                byte[] imageByte = new byte[stream.Length];
                stream.Read(imageByte, 0, (int)stream.Length);
                ImageConversion.LoadImage(tex, imageByte, false);
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Failed to load image from assembly: " + e);
            }

            return tex;
        }
    }
}
