using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XLShredReplayEditor {

    // Token: 0x02000229 RID: 553
    public class ReplaySaver : MonoBehaviour {
        // Token: 0x06001702 RID: 5890 RVA: 0x00073180 File Offset: 0x00071380
        public void Awake() {
            this.camera = Camera.main;
            this.saveDialogeBoxRect = new Rect {
                center = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f),
                width = 330f,
                height = 200f
            };
            this.saveDialogeTextRect = new Rect {
                x = this.saveDialogeBoxRect.x + 10f,
                yMin = this.saveDialogeBoxRect.yMin + 100f,
                width = 310f,
                height = 50f
            };
            this.saveDialogeSaveButtonRect = new Rect {
                x = this.saveDialogeBoxRect.x + 10f,
                yMin = this.saveDialogeBoxRect.yMin + 50f,
                width = 150f,
                height = 50f
            };
            this.saveDialogeCancelButtonRect = new Rect {
                x = this.saveDialogeBoxRect.x + this.saveDialogeBoxRect.width - 160f,
                yMin = this.saveDialogeBoxRect.yMin + 50f,
                width = 150f,
                height = 50f
            };
            this.renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            this.fps = 30;
            this.videoDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            this.movieFileEnding = ".avi";
        }

        // Token: 0x06001703 RID: 5891 RVA: 0x0007332C File Offset: 0x0007152C
        public void StartSaving() {
            this.fileName = "Replay_" + DateTime.Now.ToString("yy-MM-dd_HH:mm:ss.f") + this.movieFileEnding;
            base.enabled = true;
            ReplayManager.Instance.cameraController.enabled = false;
            ReplayManager.Instance.enabled = false;
        }

        // Token: 0x06001704 RID: 5892 RVA: 0x00073384 File Offset: 0x00071584
        public void OnGUI() {
            GUI.Box(this.saveDialogeBoxRect, "Save Dialoge");
            switch (this.mode) {
                case ReplaySaver.SaveMode.NONE:
                    if (GUI.Button(this.saveDialogeSaveButtonRect, "JSON")) {
                        this.mode = ReplaySaver.SaveMode.JSON;
                    }
                    if (GUI.Button(this.saveDialogeCancelButtonRect, "VIDEO(Images)")) {
                        this.mode = ReplaySaver.SaveMode.VIDEO;
                        return;
                    }
                    break;
                case ReplaySaver.SaveMode.JSON:
                    this.fileName = GUI.TextField(this.saveDialogeTextRect, this.fileName);
                    if (GUI.Button(this.saveDialogeSaveButtonRect, "Save")) {
                        ReplayData.SaveCurrentToFile(Application.dataPath + "\\Replays\\" + this.fileName);
                        this.EndSaving();
                    }
                    if (GUI.Button(this.saveDialogeCancelButtonRect, "Cancel")) {
                        this.EndSaving();
                        return;
                    }
                    break;
                case ReplaySaver.SaveMode.VIDEO:
                    this.fileName = GUI.TextField(this.saveDialogeTextRect, this.fileName);
                    if (GUI.Button(this.saveDialogeSaveButtonRect, "Save")) {
                        this.Save();
                    }
                    if (GUI.Button(this.saveDialogeCancelButtonRect, "Cancel")) {
                        this.EndSaving();
                        return;
                    }
                    break;
                default:
                    return;
            }
        }

        // Token: 0x06001706 RID: 5894 RVA: 0x00011955 File Offset: 0x0000FB55
        private void CaptureCameraTexture() {
            this.camera.Render();
        }

        // Token: 0x06001707 RID: 5895 RVA: 0x0007349C File Offset: 0x0007169C
        private void CreateMovie() {
            string text = this.videoDir + "\\" + this.fileName;
            for (int i = 0; i < this.textureFrames.Count; i++) {
                File.WriteAllBytes(string.Concat(new object[]
                {
                text,
                "\\frame_",
                i,
                ".jpg"
                }), this.textureFrames[i].EncodeToJPG());
            }
        }

        // Token: 0x06001708 RID: 5896 RVA: 0x00011962 File Offset: 0x0000FB62
        public void EndSaving() {
            base.enabled = false;
            ReplayManager.Instance.cameraController.enabled = true;
            ReplayManager.Instance.enabled = true;
        }

        // Token: 0x06001709 RID: 5897 RVA: 0x00073514 File Offset: 0x00071714
        public void Update() {
            if (Input.GetKeyDown(KeyCode.Return) || PlayerController.Instance.inputController.player.GetButtonDown("A")) {
                this.Save();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) || PlayerController.Instance.inputController.player.GetButtonDown("B")) {
                this.EndSaving();
                return;
            }
        }

        // Token: 0x0600170A RID: 5898 RVA: 0x00011986 File Offset: 0x0000FB86
        private void Save() {
            base.StartCoroutine(this.SaveAsync());
        }

        // Token: 0x0600170B RID: 5899 RVA: 0x00073578 File Offset: 0x00071778
        private void OnPostRender() {
            if (this.saveRendered) {
                Texture2D texture2D = new Texture2D(this.camera.pixelWidth, this.camera.pixelHeight, TextureFormat.RGB24, false);
                texture2D.ReadPixels(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), 0, 0, false);
                texture2D.Apply();
                this.textureFrames.Add(texture2D);
                this.saveRendered = false;
            }
        }

        // Token: 0x0600170C RID: 5900 RVA: 0x00011995 File Offset: 0x0000FB95
        private IEnumerator CreateReplayTextureList() {
            this.textureFrames = new List<Texture2D>();
            for (float num = ReplayManager.Instance.clipStartTime; num < ReplayManager.Instance.clipEndTime; num += 1f / (float)this.fps) {
                ReplayManager.Instance.SetPlaybackTime(num);
                ReplayManager.Instance.cameraController.LerpKeyStones();
                this.CaptureCameraTexture();
                yield return new WaitUntil(() => !this.saveRendered);
            }
            RenderTexture.active = null;
            yield break;
        }

        // Token: 0x0600170D RID: 5901 RVA: 0x000119A4 File Offset: 0x0000FBA4
        private IEnumerator SaveAsync() {
            if (!this.fileName.EndsWith(this.movieFileEnding)) {
                this.fileName += this.movieFileEnding;
            }
            yield return this.CreateReplayTextureList();
            this.CreateMovie();
            this.EndSaving();
            yield break;
        }

        // Token: 0x0400110F RID: 4367
        private Camera camera;

        // Token: 0x04001110 RID: 4368
        private string fileName;

        // Token: 0x04001111 RID: 4369
        private Rect saveDialogeBoxRect;

        // Token: 0x04001112 RID: 4370
        private Rect saveDialogeTextRect;

        // Token: 0x04001113 RID: 4371
        private RenderTexture renderTexture;

        // Token: 0x04001114 RID: 4372
        private List<Texture2D> textureFrames;

        // Token: 0x04001115 RID: 4373
        private int fps;

        // Token: 0x04001116 RID: 4374
        private Rect saveDialogeSaveButtonRect;

        // Token: 0x04001117 RID: 4375
        private Rect saveDialogeCancelButtonRect;

        // Token: 0x04001118 RID: 4376
        private string videoDir;

        // Token: 0x04001119 RID: 4377
        private bool saveRendered;

        // Token: 0x0400111A RID: 4378
        private string movieFileEnding;

        // Token: 0x0400111B RID: 4379
        private ReplaySaver.SaveMode mode;

        // Token: 0x0200022A RID: 554
        private enum SaveMode {
            // Token: 0x0400111D RID: 4381
            NONE,
            // Token: 0x0400111E RID: 4382
            JSON,
            // Token: 0x0400111F RID: 4383
            VIDEO
        }
    }

}
