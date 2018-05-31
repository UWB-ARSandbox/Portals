using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;

/// <summary>
/// Contains all classes that define the behavior involved in instantiating
/// and managing "Worlds" in ASL
/// </summary>
namespace ASL.WorldSystem
{
    /// <summary>
    /// WorldManager is a class that allows us to maintain a set of "worlds"
    /// that allow us to instantiate and manipulate sets of GameObjects in ASL
    /// separately from each other.
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        /// <summary>
        /// A map of World objects, using their viewIDs as keys.
        /// </summary>
        private Dictionary<int, World> worlds;

        /// <summary>
        /// Indicates whether the application is a master client. Only the
        /// master client can be the active creator of worlds. Other clients
        /// follow suit once they receive an ASL message indicating that a
        /// world was made.
        /// </summary>
        public bool masterClient = false;

        /// <summary>
        /// The number of worlds currently loaded
        /// </summary>
        private int numWorlds = 0;

        private UWBNetworkingPackage.NetworkManager networkManager;

        // Use this for initialization
        private void Awake()
        {
            networkManager = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
            Debug.Assert(networkManager != null);

            worlds = new Dictionary<int, World>();
            PhotonNetwork.OnEventCall += OnEvent;
        }

        /// <summary>
        /// Call the init method in all of our instantiated worlds.
        /// </summary>
        public void InitializeAll()
        {
            foreach (KeyValuePair<int, World> pair in worlds)
            {
                pair.Value.Init();
            }
        }

        /// <summary>
        /// Loads any instantiated worlds into the "worlds" Dictionary
        /// </summary>
        public void FindWorlds()
        {
            World[] worldChildren = GetComponentsInChildren<World>();
            Debug.Log("Found " + worldChildren.Length + " worlds");
            foreach (World world in worldChildren)
            {
                int worldId = world.GetComponent<PhotonView>().viewID;
                if (!worlds.ContainsKey(worldId))
                {
                    worlds.Add(worldId, world);
                }
                world.Init();
            }
            numWorlds = worlds.Count;
        }

        /// <summary>
        /// Create a new world by prefab name, and add it to the Dictionary
        /// </summary>
        /// <param name="prefabName">The name of the prefab to add</param>
        /// The prefab name must correspond to a prefab that exists under the
        /// ASL/Resources directory, and had a World Component
        public void CreateWorld(string prefabName)
        {
            Debug.Log("making world: " + prefabName);
            GameObject go = networkManager.InstantiateOwnedObject(prefabName);
            World world = go.GetComponent<World>();

            AddWorld(world);
        }

        /// <summary>
        /// Add an already instantiated world to the Dictionary to be managed
        /// </summary>
        /// <param name="world">The world to add to the WorldManager</param>
        private void AddWorld(World world)
        {
            int worldId = world.GetComponent<PhotonView>().viewID;

            world.transform.parent = gameObject.transform;
            world.transform.localPosition = Vector3.up * worlds.Count * 1000;          //improvements necessary (what if we remove a world?)

            worlds.Add(worldId, world);

            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.Others;
            PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_WORLD_ADD, worldId, true, options);
        }

        /// <summary>
        /// Set an object as a child of a world, and raises an event to the other clients
        /// so their WorldManagers update to match
        /// </summary>
        /// <param name="world">The world object to add to</param>
        /// <param name="go">The game object to add</param>
        /// The given world object must have a PhotonView component with a valid viewID
        public void AddToWorld(World world, GameObject go)
        {
            int worldId = world.GetComponent<PhotonView>().viewID;

            PhotonView view = go.GetComponent<PhotonView>();
            if (view == null)
            {
                Debug.LogError("Cannot add to world! Must have photon view!");
                return;
            }

            go.transform.parent = worlds[worldId].transform;

            //send an event off, so other clients can add the same object to their instance
            //of the world
            int[] pair = { worldId, view.viewID };

            RaiseEventOptions options = new RaiseEventOptions();
            options.Receivers = ReceiverGroup.Others;
            PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_WORLD_ADD_TO, pair, true, options);
        }

        /// <summary>
        /// Retrieve a world by its viewID (found in a PhotonView component)
        /// </summary>
        /// <param name="id">the viewID of the world to retrieve</param>
        /// <returns>The retrieved world, or null if not found</returns>
        public World GetWorldById(int id)
        {
            return worlds[id];
        }

        /// <summary>
        /// Retrieve a world from the Dictionary by name
        /// </summary>
        /// <param name="worldName">The name of the world to retrieve</param>
        /// <returns>The retrieved world, or null if not found.</returns>
        public World getWorldByName(string worldName)
        {
            foreach (KeyValuePair<int, World> pair in worlds)
            {
                if (pair.Value.name == worldName)
                    return pair.Value;
            }

            return null;
        }

        //responses to events. In other worlds. i.e. synchronizing with the master client
        #region EVENT_PROCESSING
        /// <summary>
        /// Handle ASL Events pertaining to the WorldManager. Called automatically
        /// as part of PhotonNetwork.OnEventCall
        /// </summary>
        /// <param name="eventCode">The code representing the particular event</param>
        /// <param name="content">Data sent with the event</param>
        /// <param name="senderID">The ID of the sender of the event</param>
        private void OnEvent(byte eventCode, object content, int senderID)
        {
            switch (eventCode)
            {
                case UWBNetworkingPackage.ASLEventCode.EV_WORLD_ADD:    //Add a new world
                    ProcessWorldAdd((int)content);
                    break;
                case UWBNetworkingPackage.ASLEventCode.EV_WORLD_ADD_TO: //Add an object to a world
                    int[] addToWorldPair = (int[])content;
                    ProcessWorldSet(addToWorldPair[0], addToWorldPair[1]);
                    break;
            }
        }


        /// <summary>
        /// Syncronize with the master client when it adds a new world to the Dictionary
        /// </summary>
        /// <param name="worldId">The viewID of the world to add</param>
        /// worldID must correspond to an object with a World Component
        private void ProcessWorldAdd(int worldId)
        {
            World world = PhotonView.Find(worldId).GetComponent<World>();
            world.transform.parent = gameObject.transform;
            worlds.Add(worldId, world);
        }

        //An object's world was set,
        //set it here too
        /// <summary>
        /// Synchronize with the master client when it adds an object to a world
        /// </summary>
        /// <param name="worldId">The viewID of the world to add to</param>
        /// <param name="toSetId">The viewID of the object to add</param>
        private void ProcessWorldSet(int worldId, int toSetId)
        {
            World world = worlds[worldId];
            GameObject toSet = PhotonView.Find(toSetId).gameObject;

            toSet.transform.parent = world.transform;
        }

        #endregion

        //not used for the time being
        #region WORLD_VISIBILITY
        /*
        //set the layer the camera can see
        public bool SetVisibleWorld(int playerID)
        {
            if (!worlds.ContainsKey(playerID))
            {
                Debug.LogError("Could not set world visible -- world does not exist!");
                return false;
            }

            if(visibleWorld != playerID)
            {
                if(visibleWorld != -1)
                    SetGOInvisible(worlds[visibleWorld]);
                SetGOVisible(worlds[playerID]);
                visibleWorld = playerID;
            }

            return true;
        }

        public bool setAppropriateLayer(GameObject go, int playerID)
        {
            if (!worlds.ContainsKey(playerID))
            {
                Debug.LogError("Could not set world visible -- world does not exist!");
                return false;
            }

            if (playerID == visibleWorld)
                SetGOVisible(go);
            else
                SetGOInvisible(go);

            return true;
        }

        private void SetGOInvisible(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("OtherWorld");

            for(int i = 0; i < go.transform.childCount; i++)
            {
                GameObject child = go.transform.GetChild(i).gameObject;
                SetGOInvisible(child);
            }
        }

        private void SetGOVisible(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("ActiveWorld");

            for (int i = 0; i < go.transform.childCount; i++)
            {
                GameObject child = go.transform.GetChild(i).gameObject;
                SetGOVisible(child);
            }
        }
        */
        #endregion
    }
}