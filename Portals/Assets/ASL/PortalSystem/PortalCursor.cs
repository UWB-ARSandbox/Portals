using UnityEngine;

namespace ASL.PortalSystem
{
    /// <summary>
    /// PortalCursor is a class that allows a user to create and manage Portals
    /// with a visual cursor interface. The cursor can be toggled to enable or
    /// disable controls.
    /// </summary>
    /// <remarks>
    /// To use the PortalCursor with your own controls/buttons you can extend this class
    /// and override the PlayerPortalControls method. Make sure that the new method 
    /// calls UpdateCursorTransform to update the position and orientation of the cursor.
    /// </remarks>
    public class PortalCursor : MonoBehaviour
    {
        private MeshRenderer[] meshRenderers;           //mesh in cursor for hiding
        private float rotation;                         //for portal orientation
        private bool hiding = true;                     //only active when visible
        private int src = 0, dest = 0;                  //for linking portals
        private PortalManager mPortalManager = null;    //for making, linking portals

        // Use this for initialization
        private void Start()
        {
            // Get Portal Manager ref
            mPortalManager = GetComponentInParent<PortalManager>();
            Debug.Assert(mPortalManager != null);

            // Grab all the mesh renderers in cursor, hide them
            meshRenderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mesh in meshRenderers)
            {
                mesh.enabled = false;
            }

            // Start rotation at 0
            rotation = 0.0f;
        }

        // Update is called once per frame
        private void Update()
        {
            PlayerPortalControls();
        }

        /*
         * Hide/show the cursor, disabling/enabling it
         */
        private void HideCursor(bool hide)
        {
            foreach (MeshRenderer mesh in meshRenderers)
            {
                mesh.enabled = !hide;
            }
            hiding = hide;
        }

        /*
         * Get the portal this cursor touches, if touching portal
         */
        internal GameObject GetPortal()
        {
            if (!hiding)
            {
                // Do a raycast based on head position and orientation.
                var headPosition = Camera.main.transform.position;
                var gazeDirection = Camera.main.transform.forward;
                RaycastHit hitInfo;
                if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
                {
                    // Check for portal on collision
                    if (hitInfo.collider.gameObject != null)
                    {
                        if (hitInfo.collider.gameObject.name.Contains("Portal"))
                            return hitInfo.collider.gameObject;
                        else if (hitInfo.collider.transform.parent.name.Contains("Portal"))
                            return hitInfo.collider.transform.parent.gameObject;
                    }
                }
            }
            return null;
        }

        /*
         * Only called when cursor isn't hiding
         * Update cursor position and orientation so that:
         * cursor lays flush on surface in front of user
         * cursor points in user controlled direction
         */
        private void UpdateCursorTransform()
        {
            // Do a raycast based on head position and orientation.
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;
            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {
                // Render cursor only when raycast hits object
                foreach (MeshRenderer mesh in meshRenderers)
                {
                    mesh.enabled = true;
                }

                // Move the cursor to the point where the raycast hit
                this.transform.position = hitInfo.point;

                // Rotate the cursor to hug the surface of the hologram
                this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
            else
            {
                // Don't render cursor when raycast misses object
                foreach (MeshRenderer mesh in meshRenderers)
                {
                    mesh.enabled = false;
                }
            }

            // Rotate counter-clockwise
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                rotation -= 45.0f;
            }

            // Rotate clockwise
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                rotation += 45.0f;
            }

            // Apply rotation to current cursor direction
            this.transform.rotation *= Quaternion.AngleAxis(rotation, Vector3.up);
        }

        /// <summary>
        /// This method is called from the Update function ensuring that 
        /// the portal cursor updates for each frame. It provides the user
        /// controls for interacting with portals, but these can be overriden 
        /// in a class that extends PortalCursor.
        /// </summary>
        /// <remarks>
        /// Controls for the portal cursor are as follows:
        /// P - enable/disable portal cursor
        /// Space - Create virtual portal
        /// C - Create webcam portal
        /// R - Register portal
        /// T - Set source portal
        /// Y - Set destination portal
        /// U - Link source to destination portal
        /// X - Unlink portal
        /// </remarks>
        public virtual void PlayerPortalControls()
        {
            // Toggle cursor and controls
            if (Input.GetKeyDown(KeyCode.P))
            {
                HideCursor(!hiding);
            }
            //Create/Register/Link Portals when enabled
            if (!hiding)
            {
                //Update position and orientation
                UpdateCursorTransform();

                //Create virtual Portal
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 pos = transform.position;
                    mPortalManager.MakePortal(pos, -transform.forward, transform.up, Portal.ViewType.VIRTUAL, "CircularPortal");
                }

                //Create physical portal
                if (Input.GetKeyDown(KeyCode.C))
                {
                    Vector3 pos = transform.position + 0.01f * transform.up;
                    mPortalManager.MakePortal(pos, -transform.forward, transform.up, Portal.ViewType.PHYSICAL);

                }

                //Register Portal
                if (Input.GetKeyDown(KeyCode.R))
                {
                    GameObject portalObj = GetPortal();
                    if (portalObj != null)
                    {
                        Portal portal = portalObj.GetComponent<Portal>();
                        if (portal != null)
                            mPortalManager.RequestRegisterPortal(portal);
                    }
                }

                //Set Portal Source
                if (Input.GetKeyDown(KeyCode.T))
                {
                    GameObject portalObj = GetPortal();
                    if (portalObj != null)
                    {
                        src = portalObj.GetComponent<PhotonView>().viewID;
                    }
                }

                //Set Portal Destination
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    GameObject portalObj = GetPortal();
                    if (portalObj != null)
                    {
                        dest = portalObj.GetComponent<PhotonView>().viewID;
                    }
                }

                //Link Source to Destination
                if (Input.GetKeyDown(KeyCode.U))
                {
                    if (src != -1 && dest != -1)
                    {
                        mPortalManager.RequestLinkPortal(src, dest);
                        src = -1;
                        dest = -1;
                    }
                }

                //UnLink Portal
                if (Input.GetKeyDown(KeyCode.X))
                {
                    GameObject portalObj = GetPortal();
                    if (portalObj != null)
                    {
                        mPortalManager.RequestUnlinkPortal(portalObj.GetComponent<Portal>());
                    }
                }
            }
        }
    }
}