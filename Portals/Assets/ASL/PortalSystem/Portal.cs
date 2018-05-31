using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.PortalSystem
{
    /// <summary>
    /// Portal is a class for controlling the behavior and properties of portal's in ASL.
    /// Portals can have different view types and materials depending on their state. 
    /// This class assists the Portal Manager in portal linking and teleporting. 
    /// </summary>
    public class Portal : MonoBehaviour
    {
        /// <summary>
        /// ViewType provides portals with 4 different views: none, virtual, physical, and hybrid.
        /// These views apply to what the user sees for the portal destination. A purely virtual 
        /// view will show what can be seen in the Unity scene while a purely physical view will
        /// use a connected device to show a view of the real world. Hybrid views combine the two
        /// by using the connected device to show the real world, but teleporting the user into a
        /// virtual representation.
        /// </summary>
        public enum ViewType
        {
            NONE,
            VIRTUAL,      //displays virtual destination
            PHYSICAL,     //displays a webcam feed of physical destination
            HYBRID        //displays a webcam feed for virtual destination
        };

        /// <summary>
        /// The view type to be used when this portal is set as a destination.
        /// </summary>
        public ViewType viewType = ViewType.VIRTUAL;

        /// <summary>
        /// This GameObject must be set in the editor to ensure the portal has a quad to render to.
        /// </summary>
        public GameObject renderQuad = null;

        /// <summary>
        /// This prefab must be set in the editor to ensure the portal has a camera with preset settings
        /// to use for rendering. The camera settings can be set based on user needs. 
        /// </summary>
        public GameObject copyCameraPrefab = null;

        /// <summary>
        /// This material should be set in the editor for the portal to use when in an idle state.
        /// </summary>
        public Material idleMat = null;

        /// <summary>
        /// This material should be set in the editor for the portal camera to use as a render
        /// target when in an active linked state.
        /// </summary>
        public Material copyCamMat = null;

        /// <summary>
        /// This material should be set in the editor for the rendering of a connected device
        /// when the portal is linked to a portal with a physical or hybrid view. 
        /// </summary>
        public Material webCamMat = null;

        private Portal destinationPortal = null;    //destination portal ref
        private Camera copyCamera = null;           //portal cam ref
        private Camera userCamera = null;           //user cam ref
        private WebCamTexture webCamTexture = null; //texure for using a webcam
        private string preferredWebCam = "";        //name of device used for hybrid/physical viewing

        #region INITIALIZATION
        // Use this for initialization
        private void Start()
        {
            Debug.Assert(copyCameraPrefab != null);
            Debug.Assert(renderQuad != null);

            Debug.Assert(idleMat != null);
            Debug.Assert(copyCamMat != null);
            Debug.Assert(webCamMat != null);
        }

        /// <summary>
        /// Initialize this portal to the given view type,
        /// and try to get user's camera for portal camera use.
        /// </summary>
        /// <param name="viewType">The type of view to be set.</param>
        /// <param name="user">The GameObject to retreive the user camera from.</param>
        public void Initialize(ViewType viewType, GameObject user)
        {
            // Set view type and initialize accordingly 
            this.viewType = viewType;
            switch (viewType)
            {
                case ViewType.VIRTUAL:
                    break;
                case ViewType.PHYSICAL:
                case ViewType.HYBRID:
                    InitWebCam();       //need to query user for preferred device name
                    break;
                default:
                    Debug.LogError("Error: Cannot Initialize portal. Invalid ViewType for initialization!");
                    return;
            }

            if (user != null)
                userCamera = user.GetComponentInChildren<Camera>();

            //set up the copy camera
            InitCopyCam();
        }

        /// <summary>
        /// Set the view type to be used when this portal is set as a destination.
        /// </summary>
        /// <remarks>
        /// When set as hybrid or physical, the preferred device name is used to connect
        /// a webcam. If it isn't found the first device that is found will be used.
        /// If no devices are found it will default to virtual. 
        /// </remarks>
        /// <param name="viewType">The view type to be set.</param>
        public void SetViewType(ViewType viewType)
        {
            // Set view type and initialize accordingly 
            this.viewType = viewType;
            switch (viewType)
            {
                case ViewType.VIRTUAL:
                    break;
                case ViewType.PHYSICAL:
                case ViewType.HYBRID:
                    InitWebCam();
                    break;
                default:
                    Debug.LogError("Error: Cannot Initialize portal. Invalid ViewType for initialization!");
                    return;
            }
        }

        /// <summary>
        /// Set the name of the device you prefer to use when this portal 
        /// is set to a hybrid or physical view type. 
        /// </summary>
        /// <param name="deviceName">The device name to be used as the preferred device.</param>
        public void SetPreferredDeviceName(string deviceName)
        {
            preferredWebCam = deviceName;
        }

        /// <summary>
        /// Set the GameObject being used for the user/client 
        /// position and orientation. If a camera is found within 
        /// the GameObject or it's children, it will be used for the 
        /// positioning and orientation of the portal camera. 
        /// If no camera is found, the main camera is used.
        /// </summary>
        /// <param name="user">The GameObject to retreive the user camera from.</param>
        public void SetUser(GameObject user)
        {
            if (user != null)
                userCamera = user.GetComponentInChildren<Camera>();

            InitCopyCam();
        }

        /*
         * Initialize webcam to preferred device
         */
        private void InitWebCam()
        {
            // Get all potential webcams, check if any exist
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                // No webcams, set to virtual portal type
                this.viewType = ViewType.VIRTUAL;
                return;
            }

            // Try finding the preferred webcam
            string selectedWebCam = "";
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log(devices[i].name);
                selectedWebCam = devices[i].name;
                if (devices[i].name == preferredWebCam)
                    break;
            }

            // Create webcam texture using preferred device or default
            if (selectedWebCam != "")
                webCamTexture = new WebCamTexture(selectedWebCam);
            else
                webCamTexture = new WebCamTexture();

            // Start webcam 
            webCamTexture.Play();
        }

        /*
         * Initialize portal camera
         */
        private void InitCopyCam()
        {
            // If no user cam, try to use main camera
            if (userCamera == null)
                userCamera = Camera.main;

            // Instantiate portal camera using prefab, and save ref to it
            if (copyCamera == null)
                copyCamera = Instantiate(copyCameraPrefab, transform).GetComponent<Camera>();

            // Set target texture to new render texture at current screen size
            if (copyCamera.targetTexture != null)
                copyCamera.targetTexture.Release();
            copyCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        }
        #endregion

        #region PORTAL_LINKING
        /*
         * Try linking this portal to a destination portal
         * depending on destination portal's type
         */
        internal void LinkDestination(Portal other)
        {
            Debug.Log("Linking to Portal with ViewType: " + other.viewType);

            // Create new material for render quad on successful link
            Material renderMat = null;
            Renderer renderer = renderQuad.GetComponent<Renderer>();

            switch (other.viewType)
            {
                // Virtual link requires portal cam to render to this portal
                case ViewType.VIRTUAL:
                    if (copyCamera == null) InitCopyCam();
                    renderMat = new Material(copyCamMat) { mainTexture = copyCamera.targetTexture };
                    renderer.material = renderMat;
                    break;
                // Physical/Hybrid link requires webcam to render to this portal
                case ViewType.PHYSICAL:
                case ViewType.HYBRID:
                    renderMat = new Material(webCamMat) { mainTexture = other.webCamTexture };
                    renderer.material = renderMat;
                    break;
                default:
                    Debug.LogError("Error: Cannot Link. Other portal not initialized!");
                    return;
            }

            // Set destination portal reference
            destinationPortal = other;
        }

        /*
         * Remove link to destination portal
         */
        internal void Close()
        {
            // Unlink from dest portal
            destinationPortal = null;

            // Release render texture reference
            renderQuad.GetComponent<MeshRenderer>().material = idleMat;

            // Destroy portal cam
            //Destroy(copyCamera);
        }

        /// <summary>
        /// Get the current destination portal.
        /// </summary>
        /// <returns>Portal component on destination portal, or null if no destination is set.</returns>
        public Portal GetDest()
        {
            return destinationPortal;
        }
        #endregion

        #region UPDATE
        /*
         * Update portal cam when linked to destination portal
         * using relative transform of user
         */
        void Update()
        {
            if (destinationPortal != null && copyCamera != null && userCamera != null)
            {
                // Calculate matrix for world to local, reflected across portal
                Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
                Matrix4x4 worldToLocal = destinationFlipRotation * transform.worldToLocalMatrix;

                // Calculate portal cams pos and rot in this portal space
                Vector3 camPosInSourceSpace = worldToLocal.MultiplyPoint(userCamera.transform.position);
                Quaternion camRotInSourceSpace = Quaternion.LookRotation(worldToLocal.GetColumn(2), worldToLocal.GetColumn(1)) * userCamera.transform.rotation;

                // Set portal cam relative to destination portal
                UpdateCopyCamera(camPosInSourceSpace, camRotInSourceSpace);
            }
        }

        /*
         * Update the copy camera to match the given
         * relative position and orientation
         */
        private void UpdateCopyCamera(Vector3 pos, Quaternion rot)
        {
            // Transform position and rotation to this portal's space
            copyCamera.transform.position = destinationPortal.transform.TransformPoint(pos);
            copyCamera.transform.rotation = destinationPortal.transform.rotation * rot;

            // Calculate clip plane for portal (for culling of objects inbetween destination camera and portal)
            Vector4 clipPlaneWorldSpace = new Vector4(destinationPortal.transform.forward.x, destinationPortal.transform.forward.y, destinationPortal.transform.forward.z, -Vector3.Dot(destinationPortal.transform.position, destinationPortal.transform.forward));
            Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(copyCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

            // Update projection based on new clip plane
            copyCamera.projectionMatrix = userCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        #endregion

        #region TELEPORTATION
        /*
         * Teleport gameobject if:
         * 1. a destination portal exists
         * 2. It is not pure physical
         * 3. the object is on the front side of the source portal
         * 4. the object is moving towards the portal
         */
        private void TeleportObject(GameObject go)
        {
            Debug.Log("teleportObject! [" + go.name + "]");
            Debug.Log("Source: " + GetComponent<PhotonView>().viewID.ToString());

            //1. Is there a destination portal?
            if (destinationPortal == null)
            {
                Debug.Log("No destination to teleport to, ignoring");
                return;
            }
            Debug.Log("Destination: " + destinationPortal.GetComponent<PhotonView>().viewID.ToString());

            //2. Is the destination not pure physical?
            if (destinationPortal.viewType == ViewType.PHYSICAL)
            {
                Debug.Log("Destination is physical only, ignoring");
                return;
            }

            //3. Is the object in front of the portal?
            Matrix4x4 m = transform.worldToLocalMatrix;
            Vector3 objectOffset = m.MultiplyPoint(go.transform.position);
            bool playerInFront = objectOffset.z > 0.0f;

            if (!playerInFront)
            {
                Debug.Log("Player not in front of portal, ignoring");
                return;
            }

            //4. Is the object moving towards the portal?
            Vector3 objVelocity = go.GetComponent<Rigidbody>().velocity;
            bool movingTowards = Vector3.Dot(transform.forward, objVelocity) < 0.0f;

            if (!(movingTowards || objVelocity == Vector3.zero))
            {
                Debug.Log("Not moving towards portal, ignoring");
                return;
            }

            //teleport the object
            TeleportEnter(go);
        }

        /*
         * Attempt teleportation on collision with portal
         */
        private void OnTriggerEnter(Collider other)
        {
            TeleportObject(other.gameObject);
        }

        /*
         * Prepare a gameobject for teleportation to the destination portal
         */
        private void TeleportEnter(GameObject go)
        {
            // Create world to local, flipped around portal transformation
            Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
            Matrix4x4 m = destinationFlipRotation * transform.worldToLocalMatrix;

            // Calculate go transform and velocity in source portal space
            Vector3 posInSourceSpace = m.MultiplyPoint(go.transform.position);
            Quaternion rotInSourceSpace = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)) * go.transform.rotation;
            Vector3 velInSourceSpace = m.MultiplyVector(go.GetComponent<Rigidbody>().velocity);

            // Send go and relative transform/velocity to destination portal
            destinationPortal.TeleportExit(go,
                                           posInSourceSpace,
                                           rotInSourceSpace,
                                           velInSourceSpace);
        }

        /*
         * Transform gameobject into destination portal space
         */
        private void TeleportExit(GameObject go,
                                Vector3 relativePosition,
                                Quaternion relativeRotation,
                                Vector3 relativeVelocity)
        {
            Matrix4x4 m = transform.localToWorldMatrix;
            go.transform.position = m.MultiplyPoint(relativePosition);
            go.transform.rotation = transform.rotation * relativeRotation;
            go.GetComponent<Rigidbody>().velocity = m.MultiplyVector(relativeVelocity);
        }
        #endregion
    }
}