using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UWBNetworkingPackage
{
    public class OwnableObject : Photon.PunBehaviour
    {
        private int SCENE_VALUE = 0;    // Hidden feature: assigning ownership to '0' makes object a scene object
        private bool isRestricted = false;
        private List<int> restrictedIDs;

        // Add object behavior components
        protected void Awake()
        {
            //gameObject.AddComponent<ASL.UI.Mouse.OwnershipTransfer>();
            restrictedIDs = new List<int>();
            PhotonView pv = gameObject.GetPhotonView();
            pv.ownershipTransfer = OwnershipOption.Takeover;
        }

        // Fire an event when instantiated
        // Automatically transfer ownership of objects to default scene.
        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            base.OnPhotonInstantiate(info);
            //this.gameObject.GetPhotonView().TransferOwnership(SCENE_VALUE);
        }

        [PunRPC]
        public void Grab(PhotonMessageInfo info)
        {
            this.RelinquishOwnership(info.sender.ID);

            //int grabbed = (int)OWNERSHIPSTATE.FAIL;

            //if (this.RequestOwnership(info) == 0)
            //    grabbed = (int)OWNERSHIPSTATE.NOW;

            //return grabbed;
        }

        public bool HasOwnership(PhotonPlayer player)
        {
            if (gameObject.GetComponent<PhotonView>() != null)
            {
                return player.Equals(gameObject.GetComponent<PhotonView>().owner);
            }
            else
            {
                return false;
            }
        }
        
        public void RelinquishOwnership(int newPlayerID)
        {
            // Ignore all items tagged with "room" tag
            if (this.tag.ToUpper().CompareTo("ROOM") != 0)
            {
                if (gameObject.GetPhotonView().owner.Equals(PhotonNetwork.player))
                {
                    photonView.TransferOwnership(newPlayerID);
                }
                else if (gameObject.GetPhotonView().owner.Equals(SCENE_VALUE))
                {
                    photonView.RequestOwnership();
                    photonView.TransferOwnership(newPlayerID);
                }
                gameObject.GetPhotonView().ownerId = newPlayerID;
            }
        }

        public bool CanTake()
        {
            if (StoredInScene || !isRestricted || (isRestricted && restrictedIDs.Contains(PhotonNetwork.player.ID)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Take()
        {
            PhotonView pv = gameObject.GetPhotonView();
            if (gameObject.GetPhotonView() != null)
            {
                bool owned = HasOwnership(PhotonNetwork.player);
                if (StoredInScene)
                {
                    pv.RequestOwnership();
                    pv.ownerId = PhotonNetwork.player.ID;
                }
                else if (CanTake() && !owned)
                {
                    pv.RPC("Grab", PhotonTargets.Others);
                }

                if (HasOwnership(PhotonNetwork.player))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        // restrict and add yourself to the list of ownable IDs
        // returns list of ownable IDs
        public List<int> Restrict()
        {
            if (!isRestricted && Take())
            {
                isRestricted = true;
                if (!restrictedIDs.Contains(PhotonNetwork.player.ID))
                {
                    restrictedIDs.Add(PhotonNetwork.player.ID);
                }

                // propagate restriction event
                ObjectManager objManager = GameObject.FindObjectOfType<ObjectManager>();
                if(objManager != null)
                {
                    objManager.SetObjectRestrictions(gameObject, true, restrictedIDs);
                }

                PhotonView pv = gameObject.GetPhotonView();
                pv.ownershipTransfer = OwnershipOption.Fixed;

                return OwnablePlayerIDs;
            }
            else if (isRestricted)
            {
                return OwnablePlayerIDs;
            }
            else
            {
                Debug.LogWarning("Ownership restriction failed.");
                return null;
            }
        }

        public List<int> RestrictToYourself()
        {
            if (!isRestricted && Take())
            {
                ClearOwnablePlayerIDs();
                return Restrict();
            }
            else if (isRestricted)
            {
                return OwnablePlayerIDs;
            }
            else
            {
                Debug.LogWarning("Ownership restriction failed.");
                return null;
            }
        }

        public bool UnRestrict()
        {
            if(isRestricted && Take())
            {
                isRestricted = false;
                
                // propagate restriction event
                ObjectManager objManager = GameObject.FindObjectOfType<ObjectManager>();
                if (objManager != null)
                {
                    objManager.SetObjectRestrictions(gameObject, false, restrictedIDs);
                }

                PhotonView pv = gameObject.GetPhotonView();
                pv.ownershipTransfer = OwnershipOption.Takeover;

                return true;
            }
            else if (!isRestricted)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetRestrictions(bool restricted, int[] ownableIDs)
        {
            isRestricted = restricted;
            if (isRestricted)
            {
                PhotonView pv = gameObject.GetPhotonView();
                pv.ownershipTransfer = OwnershipOption.Fixed;
            }
            else
            {
                PhotonView pv = gameObject.GetPhotonView();
                pv.ownershipTransfer = OwnershipOption.Takeover;
            }

            ClearOwnablePlayerIDs();
            bool ownable = false;
            if (ownableIDs != null)
            {
                foreach (int id in ownableIDs)
                {
                    restrictedIDs.Add(id);
                    if (id == PhotonNetwork.player.ID)
                    {
                        ownable = true;
                    }
                }
            }

            // Handle edge case where you're ripping ownership away from someone who owns the object
            if(HasOwnership(PhotonNetwork.player) && !ownable)
            {
                PhotonView pv = gameObject.GetPhotonView();
                if(pv != null)
                {
                    pv.TransferOwnership(0);
                }
            }
        }

        public void WhiteListPlayerID(List<int> playerIDs)
        {
            bool changed = false;

            if (playerIDs != null)
            {
                foreach (int id in playerIDs)
                {
                    if (!restrictedIDs.Contains(id))
                    {
                        restrictedIDs.Add(id);
                        changed = true;
                    }
                }
            }

            // propagate restriction event
            if (changed)
            {
                ObjectManager objManager = GameObject.FindObjectOfType<ObjectManager>();
                if (objManager != null)
                {
                    objManager.SetObjectRestrictions(gameObject, isRestricted, restrictedIDs);
                }
            }
        }

        public void BlackListPlayerID(List<int> playerIDs)
        {
            bool changed = false;

            if (playerIDs != null)
            {
                foreach (int id in playerIDs)
                {
                    if (restrictedIDs.Contains(id))
                    {
                        restrictedIDs.Remove(id);
                        changed = true;
                    }
                }
            }

            // propagate restriction event
            if (changed)
            {
                ObjectManager objManager = GameObject.FindObjectOfType<ObjectManager>();
                if (objManager != null)
                {
                    objManager.SetObjectRestrictions(gameObject, isRestricted, restrictedIDs);
                }
            }
        }

        public void ClearOwnablePlayerIDs()
        {
            restrictedIDs.Clear();
        }

        // ERROR TESTING - FIX
        public bool IsOwnershipRestricted
        {
            get
            {
                return isRestricted;
            }
        }

        /// <summary>
        /// Not very fast. Returns a safe copy of the restricted IDs. Use "CanTake" method to determine if available in this list.
        /// </summary>
        public List<int> OwnablePlayerIDs
        {
            get
            {
                List<int> copy = new List<int>();
                foreach(int id in restrictedIDs)
                {
                    copy.Add(id);
                }

                return copy;
            }
        }

        public int OwnerID
        {
            get
            {
                return gameObject.GetPhotonView().ownerId;
            }
        }

        public PhotonPlayer Owner
        {
            get
            {
                if (gameObject.GetComponent<PhotonView>() != null)
                {
                    return gameObject.GetComponent<PhotonView>().owner;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool StoredInScene
        {
            get
            {
                return OwnerID == SCENE_VALUE;
            }
        }
    }
}