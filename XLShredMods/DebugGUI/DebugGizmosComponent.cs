using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DebugGUI {
    class DebugGizmosComponent : MonoBehaviour {
        LineDrawer lineDrawer;
        LineDrawer lineDrawer2;
        LineDrawer lineDrawer3;

        public void Awake() {
            lineDrawer = new LineDrawer(0.02f);
            lineDrawer2 = new LineDrawer(0.02f);
            lineDrawer3 = new LineDrawer(0.02f);
        }
        public void Update() {
            lineDrawer.DrawLineInGameView(transform.position, transform.right, Color.red);
            lineDrawer.DrawLineInGameView(transform.position, transform.up, Color.green);
            lineDrawer.DrawLineInGameView(transform.position, transform.forward, Color.blue);
        }
    }
}
