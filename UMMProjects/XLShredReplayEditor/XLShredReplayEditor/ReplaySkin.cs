using System;
using UnityEngine;

namespace XLShredReplayEditor {


    public class ReplaySkin {


        public static ReplaySkin DefaultSkin {
            get {
                if (ReplaySkin._defaultSkin == null) {
                    ReplaySkin._defaultSkin = new ReplaySkin();
                }
                ReplaySkin._defaultSkin.AdjustToScreen();
                return ReplaySkin._defaultSkin;
            }
        }


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


        public static ReplaySkin _defaultSkin;


        private Vector2Int oldScreenSize;


        public Rect sliderRect;


        public GUIContent markerContent;


        public Vector2 markerSize;


        public GUIStyle markerStyle;


        public float margin;


        public int sliderPadding;


        public GUIStyle clipSliderStyle;


        public GUIStyle sliderThumbStyle;


        public GUIStyle clipStartSliderStyle;


        public GUIStyle clipEndSliderStyle;


        public GUIStyle sliderClipBorderThumbStyle;
    }

}
