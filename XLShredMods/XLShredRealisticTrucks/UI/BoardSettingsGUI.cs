using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GUILayoutLib;

namespace XLShredRealisticTrucks.UI {
    using Extensions;

    class BoardSettingsGUI : MonoBehaviour {
        GUIValueCache guiCache;
        public void Awake() {
            guiCache = new GUIValueCache();
        }
        public Rect WindowRect {
            get { return new Rect(Main.settings.GUIWindowPosition, Main.settings.GUIWindowSize); }
            set {
                if (value.position == Main.settings.GUIWindowPosition && value.size == Main.settings.GUIWindowSize) return;
                Main.settings.GUIWindowPosition = value.position;
                Main.settings.GUIWindowSize = value.size;
                Main.settings.Save(Main.modEntry);
            }
        }
        public void OnGUI() {
            WindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), WindowRect, drawBoardSetupWindow, "Board Setup");

            //GUILayout.BeginArea(new Rect(10f, 10f, 400f, 800f), "Board Properties");
            //GUI.Box(new Rect(0f, 0f, 400f, 800f), "Board Properties");
            //GUILayout.BeginVertical();
            //drawBoardSetupWindow(0);
            //GUILayout.EndVertical();
            //GUILayout.EndArea();
        }
        private void drawBoardSetupWindow(int id) {
            if (Event.current.type == EventType.Repaint) Main.settings.GUIWindowSize.y = 0;

            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();

            GUILayoutHelper.BeginFieldGroup();

            GUILayoutHelper.FloatField("RollFriction", guiCache, () => Main.settings.RollFriction, delegate (float value) {
                Main.settings.RollFriction = value;
                Main.settings.Save(Main.modEntry);
            }, 0f, 2f);
            GUILayoutHelper.FloatField("RollSideWaysFriction", guiCache, () => Main.settings.RollSideWaysFriction, delegate (float value) {
                Main.settings.RollSideWaysFriction = value;
                Main.settings.Save(Main.modEntry);
            }, 0f, 2f);
            GUILayoutHelper.FloatField("PowerSlideFriction", guiCache, () => Main.settings.PowerSlideFriction, delegate (float value) {
                Main.settings.PowerSlideFriction = value;
                Main.settings.Save(Main.modEntry);
            }, 0f, 2f);

            Main.settings.editBothTrucksTogether = GUILayout.Toggle(Main.settings.editBothTrucksTogether, "Edit both trucks together");

            GUILayout.Label(Main.settings.editBothTrucksTogether ? "---  Truck Setup  ---" : "---  Front Truck Setup  ---");

            GUILayoutHelper.FloatField("FrontTruckDamper", guiCache, () => BoardControllerExtension.FrontTruckDamper, delegate (float value) {
                BoardControllerExtension.FrontTruckDamper = value;
                if (Main.settings.editBothTrucksTogether) {
                    BoardControllerExtension.BackTruckDamper = value;
                }
                Main.settings.Save(Main.modEntry);
            }, 0f, 2f);
            GUILayoutHelper.FloatField("FrontTruckSpring", guiCache, () => BoardControllerExtension.FrontTruckSpring, delegate (float value) {
                BoardControllerExtension.FrontTruckSpring = value;
                if (Main.settings.editBothTrucksTogether) {
                    BoardControllerExtension.BackTruckSpring = value;
                }
                Main.settings.Save(Main.modEntry);
            }, 0f, 50f);
            GUILayoutHelper.Vector3Field("FrontTruckKingPinEuler", guiCache, () => BoardControllerExtension.FrontTruckKingPinEuler, delegate (Vector3 value) {
                BoardControllerExtension.FrontTruckKingPinEuler = value;
                if (Main.settings.editBothTrucksTogether) {
                    BoardControllerExtension.BackTruckKingPinEuler = value;
                }
                Main.settings.Save(Main.modEntry);
            });

            if (!Main.settings.editBothTrucksTogether) {
                GUILayout.Label("---  Back Truck Setup  ---");
                GUILayoutHelper.FloatField("BackTruckDamper", guiCache, () => BoardControllerExtension.BackTruckDamper, delegate (float value) {
                    BoardControllerExtension.BackTruckDamper = value;
                    Main.settings.Save(Main.modEntry);
                }, 0f, 2f);
                GUILayoutHelper.FloatField("BackTruckSpring", guiCache, () => BoardControllerExtension.BackTruckSpring, delegate (float value) {
                    BoardControllerExtension.BackTruckSpring = value;
                    Main.settings.Save(Main.modEntry);
                }, 0f, 50f);
                GUILayoutHelper.Vector3Field("BackTruckKingPinEuler", guiCache, () => BoardControllerExtension.BackTruckKingPinEuler, delegate (Vector3 value) {
                    BoardControllerExtension.BackTruckKingPinEuler = value;
                    Main.settings.Save(Main.modEntry);
                });
            }
            GUILayoutHelper.EndFieldGroup(guiCache);

            GUILayout.EndVertical();
        }

    }
}
