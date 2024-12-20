using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WayfarerKit.Extensions
{
    public static class TextureExtensions
    {
        public static Texture2D CreateTexture2D(this Texture source, float scale)
        {
            var (width, height) = ((int)(scale * source.width), (int)(scale * source.height));
            var newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var renderTex = RenderTexture.GetTemporary(
                width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);

            var previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            newTexture.ReadPixels(new(0, 0, renderTex.width, renderTex.height), 0, 0);
            newTexture.filterMode = FilterMode.Point;
            newTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            return newTexture;
        }
        
        public static Texture2D AddWatermark(this Texture2D source, Texture2D watermark, bool skipFlag, float alphaMultiplier = 1f, int marginPx = 25)
        {
            if (watermark == null || skipFlag)
                return source;
            
            var sourceWidth = source.width;
            var sourceHeight = source.height;
            var watermarkWidth = watermark.width;
            var watermarkHeight = watermark.height;

            marginPx = Mathf.Clamp(marginPx, 0, (int)((sourceWidth - watermarkWidth) * 0.5f));

            var startX = sourceWidth - watermarkWidth - marginPx;
            var startY = marginPx;

            var sourcePixels = source.GetPixels32();
            var watermarkPixels = watermark.GetPixels32();

            var alphaFactor = alphaMultiplier / 255f;

            for (var y = 0; y < watermarkHeight; y++)
            {
                for (var x = 0; x < watermarkWidth; x++)
                {
                    var sourceX = startX + x;
                    var sourceY = startY + y;

                    if (sourceX < 0 || sourceX >= sourceWidth || sourceY < 0 || sourceY >= sourceHeight)
                        continue;

                    var sourceIndex = sourceY * sourceWidth + sourceX;
                    var watermarkIndex = y * watermarkWidth + x;

                    var bgColor = sourcePixels[sourceIndex];
                    var wmColor = watermarkPixels[watermarkIndex];

                    var alpha = wmColor.a * alphaFactor;
                    var invAlpha = 1f - alpha;

                    bgColor.r = (byte)(bgColor.r * invAlpha + wmColor.r * alpha);
                    bgColor.g = (byte)(bgColor.g * invAlpha + wmColor.g * alpha);
                    bgColor.b = (byte)(bgColor.b * invAlpha + wmColor.b * alpha);
                    bgColor.a = (byte)(bgColor.a * invAlpha + wmColor.a * alpha);

                    sourcePixels[sourceIndex] = bgColor;
                }
            }

            source.SetPixels32(sourcePixels);
            source.Apply();

            return source;
        }
    }
}