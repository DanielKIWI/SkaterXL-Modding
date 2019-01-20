using System;
using UnityEngine;
using Harmony12;
using System.Reflection;
using UnityModManagerNet;
using XLShredLib;

namespace XLMultiplayerMod {
    static class Main {
        public static bool enabled;

        static bool Load(UnityModManager.ModEntry modEntry) {
            modEntry.OnToggle = OnToggle;

            new GameObject("PlayerCloner").AddComponent<SkaterCloner>();
            
            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            enabled = value;
            if (enabled) {

            }
            return true;
        }
    }
}
