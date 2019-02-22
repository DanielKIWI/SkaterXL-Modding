using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DebugGUI {
    public static class Profiler {
        public static void ProfileCode(Action action, string name = "noName") {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            action();
            watch.Stop();
            Debug.Log("elapsed Time for action: " + watch.ElapsedMilliseconds + " ms = " + watch.ElapsedTicks + " ticks");
        }
    }
}
