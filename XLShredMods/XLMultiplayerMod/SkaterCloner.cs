using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace XLMultiplayerMod {
    public class SkaterCloner: MonoBehaviour {

        public void Update() {
            if (Input.GetKeyDown(KeyCode.C)) {
                CopyPlayer();
            }
        }
        public void CopyPlayer() {
            try {
                GameObject skaterRoot = PlayerController.Instance.transform.root.gameObject;// skaterController.skaterTransform.gameObject;
                skaterRoot.SetActive(false);
                GameObject skaterClone = GameObject.Instantiate(skaterRoot);
                skaterClone.transform.SetParent(null);
                skaterRoot.SetActive(true);
                //skaterClone.SetActive(false);
                Debug.Log("skaterClone: " + skaterClone);

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
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
    }
}
