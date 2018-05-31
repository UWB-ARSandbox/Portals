//-----------------------------------------------------------------------
// The following file is based on Google's
// "MarkerDetectionUIController.cs" listed in the Google Tango SDK Marker
// Detection Example folder. Please reference the file for its copyright 
// information.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

using Tango;

namespace UWB_Texturing
{
    /// <summary>
    /// Detect a single AR Tag marker and place a virtual reference object on the
    /// physical marker position.
    /// </summary>
    public class TangoMarkerDetector : MonoBehaviour, ITangoVideoOverlay
    {
        /// <summary>
        /// The prefabs of marker.
        /// </summary>
        public GameObject m_markerPrefab;

        /// <summary>
        /// Length of side of the physical AR Tag marker in meters.
        /// </summary>
        private const double MARKER_SIZE = 0.1397;

        /// <summary>
        /// The objects of all markers.
        /// </summary>
        private Dictionary<String, GameObject> m_markerObjects;

        /// <summary>
        /// The list of markers detected in each frame.
        /// </summary>
        private List<TangoSupport.Marker> m_markerList;

        /// <summary>
        /// A reference to TangoApplication in current scene.
        /// </summary>
        private TangoApplication m_tangoApplication;

        /// <summary>
        /// Initialize recognized marker list and set Tango application as
        /// active Tango application.
        /// </summary>
        public void Start()
        {
            m_tangoApplication = GameObject.FindObjectOfType<TangoApplication>();
            if (m_tangoApplication != null)
            {
                m_tangoApplication.Register(this);
            }
            else
            {
                UnityEngine.Debug.LogError("No Tango Manager found in scene.");
            }

            m_markerList = new List<TangoSupport.Marker>();
            m_markerObjects = new Dictionary<String, GameObject>();
        }

        /// <summary>
        /// Detect one or more markers in the input image.
        /// </summary>
        /// <param name="cameraId">
        /// Returned camera ID.
        /// </param>
        /// <param name="imageBuffer">
        /// Color camera image buffer.
        /// </param>
        public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId,
            TangoUnityImageData imageBuffer)
        {
            TangoSupport.DetectMarkers(imageBuffer, cameraId,
                TangoSupport.MarkerType.ARTAG, MARKER_SIZE, m_markerList);

            for (int i = 0; i < m_markerList.Count; ++i)
            {
                TangoSupport.Marker marker = m_markerList[i];

                if (m_markerObjects.ContainsKey(marker.m_content))
                {
                    GameObject markerObject = m_markerObjects[marker.m_content];
                    markerObject.GetComponent<MarkerVisualizationObject>().SetMarker(marker); // Tango SDK marker visualization object
                }
                else
                {
                    GameObject markerObject = Instantiate<GameObject>(m_markerPrefab);
                    m_markerObjects.Add(marker.m_content, markerObject);
                    markerObject.GetComponent<MarkerAlignmentObject>().AlignTo(marker); // Custom Tango marker visualization object
                }
            }
        }
    }
}