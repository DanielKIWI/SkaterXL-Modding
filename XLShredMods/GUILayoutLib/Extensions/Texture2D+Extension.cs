using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GUILayoutLib.Extensions {
    public static class Texture2D_Extension {
        public static Texture2D Clone(this Texture2D original) {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                original.width,
                                original.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(original, tmp);
            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;
            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;
            // Create a new readable Texture2D to copy the pixels to it
            Texture2D newTexture = new Texture2D(original.width, original.height);
            // Copy the pixels from the RenderTexture to the new Texture
            newTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            
            // Reset the active RenderTexture
            RenderTexture.active = previous;
            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);
            return newTexture;
        }
    }
}
