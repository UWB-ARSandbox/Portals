//-----------------------------------------------------------------------
// The following file is based on Google's
// "MarkerVisualizationObject.cs" listed in the Google Tango SDK Marker
// Detection Example folder. Please reference the file for its copyright 
// information.
//-----------------------------------------------------------------------

using Tango;
using UnityEngine;

namespace UWB_Texturing
{
    /// <summary>
    /// Unity object that represents a marker. 
    /// A marker object renders its bounding box and three axes.
    /// </summary>
    public class MarkerAlignmentObject : MonoBehaviour
    {
        public Canvas canvas;

        /// <summary>
        /// Specify canvas to use for ease of reference/adjustment later.
        /// </summary>
        public void Awake()
        {
            canvas = GameObject.Find("Tango_Scan_Canvas").GetComponent<Canvas>();
            if(canvas == null)
            {
                canvas = GameObject.FindObjectOfType<Canvas>();
                Debug.Log("Specified Tango scan canvas not found. Generic canvas used.");
            }
        }

        /// <summary>
        /// Update the object with a new marker.
        /// </summary>
        /// <param name="marker">
        /// The input marker.
        /// </param>
        public void AlignTo(TangoSupport.Marker marker)
        {
            // Apply the pose of the marker to the object. 
            // This also applies implicitly to the axis object.
            transform.position = marker.m_translation;
            transform.rotation = marker.m_orientation;

            switchCamera switchCam = canvas.GetComponent<switchCamera>();
            if(switchCam != null)
            {
                switchCam.setWorldOffset(transform);
            }
        }
    }
}