using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ASL
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class Startup
    {
        static Startup()
        {
            Debug.Log("ASL Booting...");

            AddTags();
            AddLayers();
        }

        private static void AddTags()
        {
            string[] tags =
            {
                "Room"
            };

            foreach(string tag in tags)
            {
                TagsAndLayers.AddTag(tag);
            }
        }

        private static void AddLayers()
        {
            string[] layers =
            {
                // Add layers as appropriate / needed here
            };

            foreach(string layer in layers)
            {
                TagsAndLayers.AddTag(layer);
            }
        }
    }
#endif
}