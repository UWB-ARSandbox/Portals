using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.PortalSystem
{
    /// <summary>
    /// PortalSelector is a class for the user to interface with portal linking in a simple way.
    /// It provides a button that follows the portal wherever it's placed. Clicking the button
    /// will link the portal to the next registered portal, even if that portal is the same as 
    /// the source. 
    /// </summary>
    public class PortalSelector : MonoBehaviour
    {
        /// <summary>
        /// Button on PortalSelector prefab. Make sure this is set in the scene.
        /// </summary>
        public GameObject button = null;    
        
        private PortalManager portalManager = null;     //for linking/unlinking portal
        private Portal sourcePortal = null;             //portal to control
        private Camera playerCam = null;                //for raycasting select
        private int sourcePortalID = -1;
        private int destPortalID = -1;

        // Use for instantiation
        void Awake()
        {
            portalManager = GameObject.Find("PortalManager").GetComponent<PortalManager>();
            Debug.Assert(portalManager != null);
            Debug.Assert(button != null);
        }

        // Update is called once per frame
        void Update()
        {
            //need portal to control and cam for click raycast
            if (sourcePortal != null && playerCam != null)
            {
                //make sure position is on left side of portal, facing same direction
                transform.position = sourcePortal.transform.position + 1.5f * sourcePortal.transform.right;
                transform.forward = sourcePortal.transform.forward;

                //left mouse click
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
                    Physics.Raycast(ray, out hit, 100f);

                    //change destination on button click
                    if (hit.collider != null && hit.collider.gameObject == button)
                    {
                        ChangeDestination();
                    }
                }
            }
        }

        /// <summary>
        /// Initialize the PortalSelector with a camera for raycasting button clicks,
        /// and a source Portal to follow and control.
        /// </summary>
        /// <param name="playerCam">Camera to be used for raycasting when clicking the PortalSelector button.</param>
        /// <param name="sourcePortal">The Portal component of the portal to be controlled.</param>
        public void Initialize(Camera playerCam, Portal sourcePortal)
        {
            this.playerCam = playerCam;
            this.sourcePortal = sourcePortal;
            sourcePortalID = this.sourcePortal.GetComponent<PhotonView>().viewID;
            destPortalID = sourcePortalID;
        }

        /*
         * Link the controlled portal to the next available portal
         */
        private void ChangeDestination()
        {
            destPortalID = portalManager.GetNextPortalId(destPortalID);
            portalManager.RequestLinkPortal(sourcePortalID, destPortalID);
        }
    }
}