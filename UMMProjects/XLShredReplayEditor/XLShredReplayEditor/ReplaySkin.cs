using System;
using UnityEngine;

namespace XLShredReplayEditor {

    // Token: 0x02000227 RID: 551
    public class ReplaySkin {
        // Token: 0x170005A2 RID: 1442
        // (get) Token: 0x060016F9 RID: 5881 RVA: 0x00011933 File Offset: 0x0000FB33
        public static ReplaySkin DefaultSkin {
            get {
                if (ReplaySkin._defaultSkin == null) {
                    ReplaySkin._defaultSkin = new ReplaySkin();
                }
                ReplaySkin._defaultSkin.AdjustToScreen();
                return ReplaySkin._defaultSkin;
            }
        }

        // Token: 0x060016FA RID: 5882 RVA: 0x00072B40 File Offset: 0x00070D40
        public void AdjustToScreen() {
            if (Screen.currentResolution.width != this.oldScreenSize.x || Screen.currentResolution.height != this.oldScreenSize.y) {
                this.oldScreenSize.x = Screen.currentResolution.width;
                this.oldScreenSize.y = Screen.currentResolution.height;
                this.sliderRect = new Rect {
                    xMin = this.margin,
                    xMax = (float)Screen.width - this.margin,
                    y = (float)Screen.height - this.margin - 50f,
                    height = 50f
                };
            }
        }

        // Token: 0x060016FB RID: 5883 RVA: 0x00072C0C File Offset: 0x00070E0C
        public ReplaySkin() {
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.SetPixels(new Color[]
            {
            new Color(0f, 0f, 0f, 0f)
            });
            texture2D.Apply();
            this.clipSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            this.clipSliderStyle.fixedHeight = 25f;
            this.sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
            this.sliderThumbStyle.fixedHeight = 25f;
            this.sliderClipBorderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
            this.sliderClipBorderThumbStyle.fixedHeight = 50f;
            this.sliderClipBorderThumbStyle.normal.background = Texture2D.blackTexture;
            this.clipStartSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            this.clipStartSliderStyle.fixedHeight = 50f;
            this.clipEndSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            this.clipEndSliderStyle.fixedHeight = 50f;
            this.clipEndSliderStyle.normal.background = texture2D;
            this.margin = 20f;
            this.sliderPadding = this.clipSliderStyle.border.horizontal + this.clipSliderStyle.margin.horizontal + this.clipSliderStyle.padding.horizontal;
            this.markerStyle = new GUIStyle(GUI.skin.button);
            this.markerStyle.normal.background = texture2D;
            this.markerStyle.fontSize = 20;
            this.markerStyle.padding = new RectOffset(0, 0, 0, 0);
            this.markerStyle.border = new RectOffset(0, 0, 0, 0);
            this.markerStyle.fontStyle = FontStyle.Bold;
            this.markerContent = new GUIContent("↥");
            this.markerSize = this.markerStyle.CalcSize(this.markerContent);
            this.AdjustToScreen();
        }

        // Token: 0x170005A3 RID: 1443
        // (get) Token: 0x060016FC RID: 5884 RVA: 0x00072E10 File Offset: 0x00071010
        public Rect clipSliderRect {
            get {
                this.AdjustToScreen();
                float num = this.sliderRect.width - (float)this.sliderPadding;
                return new Rect {
                    xMin = this.sliderRect.xMin + (float)this.sliderPadding / 2f + (ReplayManager.Instance.clipStartTime - ReplayManager.Instance.recorder.startTime) / (ReplayManager.Instance.recorder.endTime - ReplayManager.Instance.recorder.startTime) * num,
                    xMax = this.sliderRect.xMax - (float)this.sliderPadding / 2f + (ReplayManager.Instance.clipEndTime - ReplayManager.Instance.recorder.endTime) / (ReplayManager.Instance.recorder.endTime - ReplayManager.Instance.recorder.startTime) * num,
                    y = this.sliderRect.y,
                    height = this.sliderRect.height / 2f
                };
            }
        }

        // Token: 0x060016FD RID: 5885 RVA: 0x00072F24 File Offset: 0x00071124
        public Rect markerRect(float t) {
            return new Rect {
                size = new Vector2 {
                    x = ReplaySkin.DefaultSkin.markerSize.x,
                    y = ReplaySkin.DefaultSkin.markerSize.y
                },
                center = new Vector2 {
                    x = Mathf.Lerp(this.sliderRect.xMin + (float)this.sliderPadding / 2f, this.sliderRect.xMax - (float)this.sliderPadding / 2f, t),
                    y = ReplaySkin.DefaultSkin.sliderRect.yMax - ReplaySkin.DefaultSkin.markerSize.y / 2f
                }
            };
        }

        // Token: 0x04001102 RID: 4354
        public static ReplaySkin _defaultSkin;

        // Token: 0x04001103 RID: 4355
        private Vector2Int oldScreenSize;

        // Token: 0x04001104 RID: 4356
        public Rect sliderRect;

        // Token: 0x04001105 RID: 4357
        public GUIContent markerContent;

        // Token: 0x04001106 RID: 4358
        public Vector2 markerSize;

        // Token: 0x04001107 RID: 4359
        public GUIStyle markerStyle;

        // Token: 0x04001108 RID: 4360
        public float margin;

        // Token: 0x04001109 RID: 4361
        public int sliderPadding;

        // Token: 0x0400110A RID: 4362
        public GUIStyle clipSliderStyle;

        // Token: 0x0400110B RID: 4363
        public GUIStyle sliderThumbStyle;

        // Token: 0x0400110C RID: 4364
        public GUIStyle clipStartSliderStyle;

        // Token: 0x0400110D RID: 4365
        public GUIStyle clipEndSliderStyle;

        // Token: 0x0400110E RID: 4366
        public GUIStyle sliderClipBorderThumbStyle;
    }

}
