using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;
using System.Linq;

namespace ASL.PortalSystem
{
    /// <summary>
    /// PortalManager is a class for handling all portal creation, registration,
    /// and linking. This ensures that portal events are propagated across all
    /// clients upon success. You do this through PortalManager requests, which 
    /// allow validation before propagation. 
    /// </summary>
    public class PortalManager : MonoBehaviour
    {
        /// <summary>
        /// This property tells the PortalManager how to handle events.
        /// The master client is the first to receive events, and then 
        /// propagates them to other clients.
        /// </summary>
        public bool MasterClient = true;

        /// <summary>
        /// This property should be set when the user loads in their avatar.
        /// It ensures that portals are initialized with the client avatar,
        /// but it is not required to create and use portals.
        /// </summary>
        public GameObject player = null;

        /// <summary>
        /// This property allows you to set the PortalCursor prefab within the editor
        /// to enable it's use. Simply leave the property set to null if you do not 
        /// want to use a PortalCursor.
        /// </summary>
        public GameObject portalCursor = null;

        private UWBNetworkingPackage.NetworkManager networkManager;    //should always exist in ASL scene
        private Dictionary<int, Portal> portalSet;      //the set of all available portals

        // Use this for initialization
        private void Awake()
        {
            networkManager = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
            portalSet = new Dictionary<int, Portal>();
            if (portalCursor != null) Instantiate(portalCursor, transform);
            PhotonNetwork.OnEventCall += OnEvent;
        }

        /// <summary>
        /// Instantiate a portal through ASL to ensure it is created for all clients.
        /// </summary>
        /// <param name="position">Portal position in world space.</param>
        /// <param name="forward">Portal front-facing direction in world space.</param>
        /// <param name="up">Portal up direction in world space.</param>
        /// <param name="vType">Portal view type, see Portal.ViewType.</param>
        /// <param name="portalPrefab">Prefab to be used for portal instantiation.
        /// The prefab should include a component for a Portal, or Portal extension, script.</param>
        /// <returns>Instantiated portal.</returns>
        public Portal MakePortal(Vector3 position, Vector3 forward, Vector3 up, Portal.ViewType vType = Portal.ViewType.VIRTUAL, string portalPrefab = "Portal")
        {
            GameObject newPortal = networkManager.InstantiateOwnedObject(portalPrefab) as GameObject;
            newPortal.transform.position = position;
            newPortal.transform.rotation = Quaternion.LookRotation(forward, up);

            Portal p = newPortal.GetComponent<Portal>();
            p.Initialize(vType, player);

            return p;
        }

        /*
         * Portal System Action Requests
         * (Pre-Master Client Verification)
         */
        #region REQUESTS

        /// <summary>
        /// Request that the given portal be registered. A portal must be 
        /// registered before it can be linked. 
        /// </summary>
        /// <param name="portal">Portal to be registered.</param>
        /// <returns>Registration success.</returns>
        public bool RequestRegisterPortal(Portal portal)
        {
            int viewID = portal.GetComponent<PhotonView>().viewID;
            if (IsIDRegistered(viewID))
            {
                //Unable to register portal! Portal already registered
                return false;
            }

            Debug.Log("Requesting Portal Registration");
            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.MasterClient;
            PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_REG, viewID, true, options);
            return true;
        }

        /// <summary>
        /// Request that the given portal be unregistered. This would prevent a portal
        /// from being linked, and leave it in an idle state.
        /// </summary>
        /// <param name="portal">Portal to be registered.</param>
        /// <returns>Unregistration success.</returns>
        public bool RequestUnregisterPortal(Portal portal)
        {
            int viewID = portal.GetComponent<PhotonView>().viewID;
            if (!IsIDRegistered(viewID))
            {
                //Unable to unregister portal! Portal not registered
                return false;
            }

            Debug.Log("Requesting Portal Unregistration");
            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.MasterClient;

            PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNREG, viewID, true, options);
            return true;
        }

        /// <summary>
        /// Request that the given source portal be linked to the given destination portal.
        /// Both portals must be registered for successful linking. 
        /// </summary>
        /// <param name="source">Portal linking source.</param>
        /// <param name="destination">Portal linking destination.</param>
        /// <returns>Linking success.</returns>
        public bool RequestLinkPortal(Portal source, Portal destination)
        {
            int sourceID = source.GetComponent<PhotonView>().viewID;
            int destID = destination.GetComponent<PhotonView>().viewID;
            if (!IsIDRegistered(sourceID) || !IsIDRegistered(destID))
            {
                //Can't link unregistered portals
                return false;
            }

            Debug.Log("Requesting Portal Registration");
            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.MasterClient;

            int[] linkIDPair = new int[2];
            linkIDPair[0] = sourceID;
            linkIDPair[1] = destID;

            PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, linkIDPair, true, options);
            return true;
        }

        /// <summary>
        /// Request that the given source portal be linked to the given destination portal.
        /// Both portals must be registered for successful linking. 
        /// </summary>
        /// <param name="source">Portal linking source ID.</param>
        /// <param name="destination">Portal linking destination ID.</param>
        /// <returns>Linking success.</returns>
        public bool RequestLinkPortal(int source, int destination)
        {
            if (!IsIDRegistered(source) || !IsIDRegistered(destination))
            {
                Debug.Log("Bad call to ReqLinkPortal, nonregistered portals");
                return false;
            }

            Debug.Log("Requesting Portal Link");
            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.MasterClient;

            int[] linkIDPair = new int[2];
            linkIDPair[0] = source;
            linkIDPair[1] = destination;

            PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, linkIDPair, true, options);
            return true;
        }

        /// <summary>
        /// Request that the given portal be unlinked from its destination.
        /// It must be registered to be linked or unlinked.
        /// </summary>
        /// <param name="source">Portal to be unlinked.</param>
        /// <returns>Unlinking success.</returns>
        public bool RequestUnlinkPortal(Portal source)
        {
            int viewID = source.GetComponent<PhotonView>().viewID;
            if (!IsIDRegistered(viewID))
            {
                //Portal not registered
                return false;
            }

            Debug.Log("Requesting Portal Unlink");
            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.MasterClient;

            PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNLINK, viewID, true, options);
            return true;
        }
        #endregion


        /*
         * Portal System Actions
         * (Post-Master Client Verification)
         * --Only Called from OnEvent
         */

        //Register a portal by adding a Portal to the Dictionary
        private bool RegisterPortal(int portalID)
        {
            if (IsIDRegistered(portalID))
            {
                //Unable to register portal! Portal already registered
                return false;
            }

            PhotonView[] views = GameObject.FindObjectsOfType<PhotonView>();
            foreach (PhotonView view in views)
            {
                if (view.viewID == portalID)
                {
                    GameObject g = view.gameObject;
                    Portal p = g.GetComponent<Portal>();
                    if (p != null)
                    {
                        portalSet.Add(view.viewID, p);
                        return true;
                    }
                    else
                    {
                        Debug.LogError("Unable to register portal! Object associated with photonID [" + portalID + "] is not a portal!");
                        return false;
                    }
                }
            }

            Debug.LogError("Unable to register portal! Could not find portal in PhotonView List!");
            return false;
        }

        //Unregister a portal by removing the Portal from the Dictionary
        private bool UnregisterPortal(int portalID)
        {
            if (!IsIDRegistered(portalID))
            {
                //Unable to unregister portal! Portal not registered
                return false;
            }

            PhotonView[] views = GameObject.FindObjectsOfType<PhotonView>();
            foreach (PhotonView view in views)
            {
                if (view.viewID == portalID)
                {
                    GameObject g = view.gameObject;
                    Portal p = g.GetComponent<Portal>();
                    if (p != null)
                    {
                        portalSet.Remove(view.viewID);
                        p.Close();
                        return true;
                    }
                    else
                    {
                        Debug.LogError("Unable to unregister portal! Object associated with photonID [" + portalID + "] is not a portal!");
                        return false;
                    }
                }
            }

            Debug.LogError("Unable to unregister portal! Could not find portal in PhotonView List!");
            return false;
        }

        //Link source portal to destination portal
        private bool LinkPortal(int sourceID, int destinationID)
        {
            if (IsIDRegistered(sourceID) && IsIDRegistered(destinationID))
            {
                if (portalSet[sourceID].GetDest() != null)
                    UnlinkPortal(sourceID);

                //portalSet[sourceID].Initialize(portalSet[destinationID], player);
                portalSet[sourceID].LinkDestination(portalSet[destinationID]);
                return true;
            }
            else
            {
                Debug.Log("Cannot Link Portal! One or more portals is not registered");
                return false;
            }
        }

        private bool UnlinkPortal(int sourceID)
        {
            if (IsIDRegistered(sourceID))
            {
                if (portalSet[sourceID].GetDest() != null)
                {
                    portalSet[sourceID].Close();
                }
                return true;
            }
            else
            {
                Debug.Log("Cannot Unlink Portal! Source not registered");
                return false;
            }
        }

        #region EVENT_PROCESSING
        //handle events specifically related to portal stuff
        private void OnEvent(byte eventCode, object content, int senderID)
        {
            //handle events specifically related to portal stuff
            switch (eventCode)
            {
                case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_REG:
                    Debug.Log("EV_REG: " + (int)content);
                    ProcessRegisterPortalEvent((int)content);
                    break;
                case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNREG:
                    Debug.Log("EV_UNREG: " + (int)content);
                    ProcessUnregisterPortalEvent((int)content);
                    break;
                case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK:
                    int[] idPair = (int[])content;
                    Debug.Log("EV_LINK: " + idPair);
                    ProcessLinkPortalEvent(idPair);
                    break;
                case UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNLINK:
                    ProcessUnlinkPortalEvent((int)content);
                    Debug.Log("EV_UNLINK: " + (int)content);
                    break;
            }
        }

        private void ProcessRegisterPortalEvent(int portalID)
        {
            if (MasterClient) //if we are master client, verify first
            {
                if (RegisterPortal(portalID))
                {
                    Debug.Log("Portal Registration Request Approved");
                    RaiseEventOptions options = new RaiseEventOptions();
                    options.Receivers = ReceiverGroup.Others;
                    PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_REG, portalID, true, options);
                }
                else
                {
                    Debug.Log("Portal Registration Request Failed");
                }
            }
            else
            {
                RegisterPortal(portalID);
            }
        }

        private void ProcessUnregisterPortalEvent(int portalID)
        {
            if (MasterClient)
            {
                if (UnregisterPortal(portalID))
                {
                    Debug.Log("Portal Unregistration Request Approved");
                    RaiseEventOptions options = new RaiseEventOptions();
                    options.Receivers = ReceiverGroup.Others;
                    PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_UNREG, portalID, true, options);
                }
                else
                {
                    Debug.Log("Portal Unregistration Request Failed");
                }
            }
            else
            {
                UnregisterPortal(portalID);
            }
        }

        private void ProcessLinkPortalEvent(int[] linkIDPair)
        {
            if (MasterClient)
            {
                if (LinkPortal(linkIDPair[0], linkIDPair[1]))
                {
                    Debug.Log("Portal Link Request Approved");
                    RaiseEventOptions options = new RaiseEventOptions();
                    options.Receivers = ReceiverGroup.Others;
                    PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, linkIDPair, true, options);
                }
                else
                {
                    Debug.Log("Portal Link Request Failed");
                }
            }
            else
            {
                LinkPortal(linkIDPair[0], linkIDPair[1]);
            }
        }

        private void ProcessUnlinkPortalEvent(int sourceID)
        {
            if (MasterClient)
            {
                if (UnlinkPortal(sourceID))
                {
                    Debug.Log("Portal Unlink Request Approved");
                    RaiseEventOptions options = new RaiseEventOptions();
                    options.Receivers = ReceiverGroup.Others;
                    PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_PORTAL_LINK, sourceID, true, options);
                }
                else
                {
                    Debug.Log("Portal Unlink Request Failed");
                }
            }
        }
        #endregion

        //test if the id corresponds with a portal
        private bool VerifyViewIDPortal(int candID)
        {
            Debug.Log("Verifying View ID [" + candID + "]");
            PhotonView[] views = GameObject.FindObjectsOfType<PhotonView>();
            foreach (PhotonView view in views)
            {
                if (view.viewID == candID)
                {
                    GameObject g = view.gameObject;
                    Portal p = g.GetComponent<Portal>();
                    if (p != null)
                    {
                        Debug.Log("ID [" + candID + "] is valid");
                        return true;
                    }
                    else
                    {
                        Debug.LogError("Unable to verify portal! Object associated with photonID [" + candID + "] is not a portal!");
                        return false;
                    }
                }
            }

            Debug.LogError("Unable to verify portal! Could not find ID [" + candID + "] in PhotonView List!");
            return false;
        }

        //Test if the id is registered as a portal
        private bool IsIDRegistered(int id)
        {
            return portalSet.ContainsKey(id);
        }

        /// <summary>
        /// Get the Portal component from a registered portal.
        /// </summary>
        /// <param name="portalID">Registered portal ID.</param>
        /// <returns>Portal component or null if it isn't registered.</returns>
        public Portal GetPortal(int portalID)
        {
            return portalSet[portalID];
        }

        /// <summary>
        /// Get the Portal component from the portal that the Portal Cursor is touching.
        /// </summary>
        /// <returns>Portal component or null if the cursor isn't touching a portal.</returns>
        public Portal GetPortal()
        {
            if (portalCursor != null)
            {
                GameObject portal = portalCursor.GetComponent<PortalCursor>().GetPortal();
                return portal.GetComponent<Portal>();
            }
            return null;
        }

        /// <summary>
        /// Get the collection of registered portal IDs.
        /// </summary>
        /// <returns>Collection of registered portal IDs.</returns>
        public Dictionary<int, Portal>.KeyCollection GetPortalIDs()
        {
            return portalSet.Keys;
        }

        /*
         * Use the given portal ID to get the next registered ID.
         * Mostly used for PortalSelector iteration.
         */
        internal int GetNextPortalId(int portalID)
        {
            IEnumerable<int> keys = portalSet.Keys;

            if (!portalSet.ContainsKey(portalID) || keys.Last() == portalID)
                return keys.First();

            IEnumerator<int> keyEnumerator = keys.GetEnumerator();
            while (keyEnumerator.Current != portalID)
            {
                keyEnumerator.MoveNext();
            }

            keyEnumerator.MoveNext();
            return keyEnumerator.Current;
        }
    }
}
