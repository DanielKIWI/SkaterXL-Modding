using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XLShredReplayEditor {


    public class ReplaySaver : MonoBehaviour {

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


        public void StartSaving() {
            this.fileName = "Replay_" + DateTime.Now.ToString("yy-MM-dd_HH:mm:ss.f") + this.movieFileEnding;
            base.enabled = true;
            ReplayManager.Instance.cameraController.enabled = false;
            ReplayManager.Instance.enabled = false;
        }


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


        private void CaptureCameraTexture() {
            this.camera.Render();
        }


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


        public void EndSaving() {
            base.enabled = false;
            ReplayManager.Instance.cameraController.enabled = true;
            ReplayManager.Instance.enabled = true;
        }


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


        private void Save() {
            base.StartCoroutine(this.SaveAsync());
        }


        private void OnPostRender() {
            if (this.saveRendered) {
                Texture2D texture2D = new Texture2D(this.camera.pixelWidth, this.camera.pixelHeight, TextureFormat.RGB24, false);
                texture2D.ReadPixels(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), 0, 0, false);
                texture2D.Apply();
                this.textureFrames.Add(texture2D);
                this.saveRendered = false;
            }
        }


        private IEnumerator CreateReplayTextureList() {
            this.textureFrames = new List<Texture2D>();
            for (float num = ReplayManager.Instance.clipStartTime; num < ReplayManager.Instance.clipEndTime; num += 1f / (float)this.fps) {
                ReplayManager.Instance.SetPlaybackTime(num);
                ReplayManager.Instance.cameraController.EvaluateKeyStones();
                this.CaptureCameraTexture();
                yield return new WaitUntil(() => !this.saveRendered);
            }
            RenderTexture.active = null;
            yield break;
        }


        private IEnumerator SaveAsync() {
            if (!this.fileName.EndsWith(this.movieFileEnding)) {
                this.fileName += this.movieFileEnding;
            }
            yield return this.CreateReplayTextureList();
            this.CreateMovie();
            this.EndSaving();
            yield break;
        }


        private Camera camera;


        private string fileName;


        private Rect saveDialogeBoxRect;


        private Rect saveDialogeTextRect;


        private RenderTexture renderTexture;


        private List<Texture2D> textureFrames;


        private int fps;


        private Rect saveDialogeSaveButtonRect;


        private Rect saveDialogeCancelButtonRect;


        private string videoDir;


        private bool saveRendered;


        private string movieFileEnding;


        private ReplaySaver.SaveMode mode;


        private enum SaveMode {

            NONE,

            JSON,

            VIDEO
        }
    }

}
