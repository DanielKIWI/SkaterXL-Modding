using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Popcron;

namespace DebugGUI {
    class DebugGizmosComponent: MonoBehaviour {
        public void Update() {
            //Popcron.Gizmos.Line(transform.position, transform.right * transform.lossyScale.x, Color.red);
            //Popcron.Gizmos.Line(transform.position, transform.up * transform.lossyScale.y, Color.green);
            //Popcron.Gizmos.Line(transform.position, transform.forward * transform.lossyScale.z, Color.blue);
        }
    }
}
