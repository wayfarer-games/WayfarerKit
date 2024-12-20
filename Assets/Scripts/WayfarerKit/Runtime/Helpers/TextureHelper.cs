using UnityEngine;

namespace WayfarerKit.Helpers
{
    public static class TextureHelper
    {
        public static Texture2D Duplicate(this Texture2D source)
        {
            var newTexture = new Texture2D(source.width, source.height, source.format, source.mipmapCount > 1)
            {
                filterMode = source.filterMode
            };
            
            newTexture.SetPixels(source.GetPixels());
            newTexture.Apply();

            return newTexture;
        }
    }
}