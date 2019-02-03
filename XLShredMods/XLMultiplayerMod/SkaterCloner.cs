using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using DebugGUI.Helper;

namespace XLMultiplayerMod {
    public class SkaterCloner: MonoBehaviour {

        public void Update() {
            if (Input.GetKeyDown(KeyCode.C)) {
                CopyPlayer();
            }
        }
        //public bool DoesTransformGetRecorded(Transform t) {
        //    if (t == null) return false;
        //    foreach (var recordedTransform in ReplayManager.Instance.recorder.transformsToBeRecorded) {
        //        if (recordedTransform == null) continue;
        //        if (t.ToString() == recordedTransform.ToString()) return true;
        //    }
        //    return false;
        //}
        //public void DestroyNotRecordedTransforms(Transform t) {
        //    if (!DoesTransformGetRecorded(t)) {
        //        Destroy(t.gameObject);
        //        return;
        //    }
        //    for (int i = 0; i < t.childCount; i++) {
        //        DestroyNotRecordedTransforms(t.GetChild(i));
        //    }
        //}
        public void CopyPlayer() {
            try {
                GameObject skaterRoot = PlayerController.Instance.transform.root.Find("Skater Root").gameObject;// skaterController.skaterTransform.gameObject;
                List<Transform> list = new List<Transform>(PlayerController.Instance.respawn.getSpawn);
                foreach (object obj in Enum.GetValues(typeof(HumanBodyBones))) {
                    HumanBodyBones humanBodyBones = (HumanBodyBones)obj;
                    if (humanBodyBones >= HumanBodyBones.Hips && humanBodyBones < HumanBodyBones.LastBone) {
                        Transform boneTransform = PlayerController.Instance.animationController.skaterAnim.GetBoneTransform(humanBodyBones);
                        if (!(boneTransform == null)) {
                            list.Add(boneTransform);
                        }
                    }
                }
                foreach (Transform t in list) {
                    t.gameObject.name = "IMPORTANT-" + t.gameObject.name;
                }
                DebugHelper.LogObjectHierarchy(skaterRoot);

                return;
                skaterRoot.AddComponent<ImportentTransformReferences>().importantTransforms = list.ToArray();
                DebugHelper.LogObjectHierarchy(skaterRoot);
                skaterRoot.SetActive(false);
                GameObject skaterClone = GameObject.Instantiate(skaterRoot);
                Destroy(skaterRoot.GetComponent<ImportentTransformReferences>());
                Transform[] impotatTransforms = skaterClone.GetComponent<ImportentTransformReferences>().importantTransforms;
                skaterClone.transform.SetParent(null);
                skaterRoot.SetActive(true);

                foreach (Transform t in skaterClone.GetComponentsInChildren<Transform>()) {
                    if (t == skaterClone) continue;
                    if (Array.IndexOf(impotatTransforms, t) == -1) {
                        Destroy(t.gameObject);
                    }
                }
                //Destroy(skaterClone.transform.Find("Camera Rig").gameObject);
                //DestroyNotRecordedTransforms(skaterClone.transform);


                foreach (var c in skaterClone.GetComponentsInChildren<GraphicRaycaster>()) {
                    Debug.Log("Destroyed Component " + c + " from skaterClone");
                    Destroy(c);
                }
                foreach (var c in skaterClone.GetComponentsInChildren<RawImage>()) {
                    Debug.Log("Destroyed Component " + c + " from skaterClone");
                    Destroy(c);
                }
                foreach (var c in skaterClone.GetComponentsInChildren<Image>()) {
                    Debug.Log("Destroyed Component " + c + " from skaterClone");
                    Destroy(c);
                }
                foreach (var c in skaterClone.GetComponentsInChildren<Text>()) {
                    Debug.Log("Destroyed Component " + c + " from skaterClone");
                    Destroy(c);
                }
                foreach (var c in skaterClone.GetComponentsInChildren<CanvasScaler>()) {
                    Debug.Log("Destroyed Component " + c + " from skaterClone");
                    Destroy(c);
                }
                foreach (var c in skaterClone.GetComponentsInChildren<StandaloneInputModule>()) {
                    Debug.Log("Destroyed Component " + c + " from skaterClone");
                    Destroy(c);
                }
                foreach (var c in skaterClone.GetComponentsInChildren<Component>()) {
                    if (c is Renderer) continue;
                    if (c is Transform) continue;
                    if (c is MeshFilter) continue;
                    Debug.Log("Destroyed Component " + c + " from skaterClone");
                    Destroy(c);
                }
                skaterClone.SetActive(true);
                DebugHelper.LogObjectHierarchy(skaterClone);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
    }
}
