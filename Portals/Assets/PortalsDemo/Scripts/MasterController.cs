using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;
using ASL.PortalSystem;
using ASL.WorldSystem;

public class MasterController : MonoBehaviour
{
    //are we the master client?
    public bool masterClient = false;

    //core components
    public UWBNetworkingPackage.NetworkManager networkManager = null;
    public PortalManager portalManager = null;
    public WorldManager worldManager = null;

    //the main camera (for raycasting, and attaching to the player)
    public Camera mainCamera = null;

    //Player
    public GameObject playerAvatar = null;
    public string avatarName = "MasterAvatar";
    public Color avatarColor = Color.white;
    public Vector3 SpawnPosition = new Vector3(0, 1, 0);

    //Worlds
    public List<string> worldPrefabs;

    //Setup State
    private bool setupComplete = false;

    // Use this for initialization
    void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
        PhotonNetwork.OnEventCall += OnEvent;

        Debug.Assert(networkManager != null);
        Debug.Assert(worldManager != null);
        Debug.Assert(portalManager != null);
    }

    // Update is called once per frame
    void Update()
    {
        //Do Setup
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (!setupComplete && PhotonNetwork.inRoom)
            {
                MakeAvatar();

                if (masterClient)
                {
                    CreateDefaultWorlds();
                    Debug.Log("Sending Master load event");
                    RaiseEventOptions options = new RaiseEventOptions();
                    options.Receivers = ReceiverGroup.Others;
                    PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_MASTER_LOAD, 1, true, options);
                }
                else
                {
                    worldManager.FindWorlds();
                }

                World world = worldManager.getWorldByName("PortalWorld");
                if (world != null)
                {
                    worldManager.AddToWorld(world, playerAvatar);
                    playerAvatar.transform.localPosition = SpawnPosition;
                }

                setupComplete = true;
            }
            else if (playerAvatar == null && PhotonNetwork.inRoom)
            {
                MakeAvatar();
                World world = worldManager.getWorldByName("PortalWorld");
                if (world != null)
                {
                    worldManager.AddToWorld(world, playerAvatar);
                    playerAvatar.transform.localPosition = SpawnPosition;
                }
            }
        }
    }

    //Create the Worlds that will exist from the outset
    private void CreateDefaultWorlds()
    {
        foreach (string worldPrefab in worldPrefabs)
        {
            worldManager.CreateWorld(worldPrefab);
        }
        worldManager.InitializeAll();
    }

    //Make Avatar
    private void MakeAvatar()
    {
        //make the user avatar/camera
        playerAvatar = networkManager.InstantiateOwnedObject("UserAvatar") as GameObject;
        PlayerAvatar avatarComponent = playerAvatar.GetComponent<PlayerAvatar>();

        AvatarInfo avatarProperties = new AvatarInfo(PhotonNetwork.player.ID,
                                                    playerAvatar.GetComponent<PhotonView>().viewID,
                                                    SpawnPosition,
                                                    avatarColor);
        avatarComponent.Initialize(avatarProperties, mainCamera, this);
        portalManager.player = playerAvatar;

        //send a message off so it can be properly initialized
        Debug.Log("Sending Color: " + avatarProperties.color);
        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.Others;
        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_AVATAR_MAKE, avatarProperties, true, options);
    
        }


    //PlayerCreatePortal
    //Try to create a portal where the player camera is looking at on the plane
    public void PlayerCreatePortal(Vector3 position, Vector3 forward, Vector3 up, Portal.ViewType vType = Portal.ViewType.VIRTUAL)
    {
        portalManager.MakePortal(position, forward, up, vType);
    }

    //PlayerRegisterPortal
    //Try to register the portal with the PortalManager
    public void PlayerRegisterPortal(GameObject portalGO)
    {
        Portal portal = portalGO.GetComponent<Portal>();
        if (portal != null)
            portalManager.RequestRegisterPortal(portal);
        else
        {
            Debug.LogError("Object [" + portalGO.name + "] is not a portal! cannot register!");
        }
    }

    #region EVENT_PROCESSING
    //handle events specifically related to portal stuff
    private void OnEvent(byte eventCode, object content, int senderID)
    {
        //handle events specifically related to portal stuff
        switch (eventCode)
        {
            case UWBNetworkingPackage.ASLEventCode.EV_MASTER_LOAD:
                Debug.Log("EV_MASTER_LOAD: " + (int)content);
                ProcessMasterLoad();
                break;
            case UWBNetworkingPackage.ASLEventCode.EV_AVATAR_MAKE:
                Debug.Log("E_AVATAR_MAKE: ");
                ProcessAvatarMake((AvatarInfo)content);
                break;
        }
    }

    private void ProcessMasterLoad()
    {
        if (!masterClient)
        {
            worldManager.FindWorlds();

            if (playerAvatar != null)
            {
                World world = worldManager.getWorldByName("PortalWorld");
                if (world != null)
                {
                    worldManager.AddToWorld(world, playerAvatar);
                    playerAvatar.transform.localPosition = SpawnPosition;
                }
            }

            setupComplete = true;
        }
    }

    //Receiving an avatar another user has made
    //Initialize our copy to have the same properties
    private void ProcessAvatarMake(AvatarInfo info)
    {
        Debug.Log("User Made Avatar, initializing");
        Debug.Log("Avatar: " + 
                  "{playerID: " + info.playerID + 
                  ", viewID: " + info.viewID + 
                  ", spawnPosition: " + info.spawnPosition +
                  ", color: " + info.color + "}");
         PhotonView view = PhotonView.Find(info.viewID);
         if (view == null)
         {
             Debug.LogError("Error: No PhotonView Component!");
             return;
         }

         PlayerAvatar newAvatar = view.GetComponent<PlayerAvatar>();
         if (newAvatar == null)
         {
             Debug.LogError("Error: No PlayerAvatar Component!");
             return;
         }

         newAvatar.Initialize(info, null, null);
    }
    #endregion
}