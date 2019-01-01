using UnityEngine;
using Harmony12;
using UnityModManagerNet;

namespace XLShredReplayEditor {
    
    static class Main {

        // Send a response to the mod manager about the launch status, success or not.
        static void Load() {
            GameObject rmGO = new GameObject("ReplayEditor");
            ReplayManager rm = rmGO.AddComponent<ReplayManager>();
        }
    }
}
