using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XLShredReplayEditor.Extensions {
    public static class DeckSoundsExtension {
        public static AudioSource[] getAudioSources(this DeckSounds ds) {
            List<AudioSource> sources = new List<AudioSource>(new AudioSource[9] {
                ds.bearingSource,
                ds.deckSource,
                ds.grindHitSource,
                ds.grindLoopSource,
                ds.shoesBoardHitSource,
                ds.shoesScrapeSource,
                ds.wheelHitSource,
                ds.wheelRollingLoopHighSource,
                ds.wheelRollingLoopLowSource,
            });
            for (int i = 0; i< sources.Count; i++) {
                if (sources[i] == null) {
                    sources.RemoveAt(i);
                    DebugGUI.LogWarning("AudioSource at index " + i + " is null");
                }
            }
            return sources.ToArray();
            //ds.GetComponentsInChildren<AudioSource>()
        }
    }
}
