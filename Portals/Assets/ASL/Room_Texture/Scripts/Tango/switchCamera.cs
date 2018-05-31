using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tango;
using UnityEngine.UI;

namespace UWB_Texturing
{
    public class switchCamera : UnityEngine.MonoBehaviour
    {
        public GameObject Static;
        public GameObject DynamicMesh;
        public GameObject TangoManager;
        public GameObject Camera;
        public GameObject Dynamic;
        private bool QR = false;
        private bool CameraToggle = true;
        public Text Te;

        // Update is called once per frame
        void Update()
        {
            //for each object, if object is a room or run time init object, set to marker transform
            foreach (GameObject G in GameObject.FindObjectsOfType<GameObject>())
            {
                if (G != Static && !G.transform.parent && G.tag != "Anchor")
                {
                    //G.transform.position = Dynamic.transform.position;
                    if (G.tag == "Room")
                    {
                        GameObject D = new GameObject();
                        G.transform.SetParent(D.transform);
                        G.transform.position = Dynamic.transform.position;
                        G.transform.rotation = Dynamic.transform.rotation;
                        G.transform.SetParent(Dynamic.transform);
                        Destroy(D);
                    }
                    else
                    {
                        G.transform.SetParent(Dynamic.transform);
                    }
                }
            }
        }

        /// <summary>
        /// Clears the Dynamic Mesh
        /// </summary>
        public void Clear()
        {
            DynamicMesh.GetComponent<TangoDynamicMesh>().Clear();
            TangoManager.GetComponent<TangoApplication>().Tango3DRClear();
        }

        /// <summary>
        /// Sets the current Dynamic Parent Game Object to marker transform
        /// </summary>
        /// <param name="T"></param>
        public void setWorldOffset(Transform T)
        {
            if (QR == false)
            {
                Dynamic.transform.position = T.transform.position;
                Dynamic.transform.rotation = T.transform.rotation;
                QR = true;
            }
        }

        /// <summary>
        /// Toggles on/off the Dynamic Mesh
        /// </summary>
        public void ToggleCamera()
        {
            if (DynamicMesh.GetActive() == true)
            {
                Clear();
                DynamicMesh.SetActive(false);
            }
            else
            {
                DynamicMesh.SetActive(true);
            }
        }

        /// <summary>
        /// Set Tango Debug Text
        /// </summary>
        /// <param name="M"></param>
        public void SetText(string s)
        {
            Te.text = s;
        }
    }
}