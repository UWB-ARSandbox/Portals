using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * RoomWorld - A World that contains functionality for loading rooms
 *      and streaming them to other clients. The particular room to be
 *      loaded is determined by the string "roomName", and is loaded in
 *      according to the transform determined by "roomOrigin"
 */ 
 /// <summary>
 /// The RoomWorld class contains functionality for loading rooms and streaming
 /// them to other clients.
 /// </summary>
public class RoomWorld : PortalWorld {

    /// <summary>
    /// A reference to the a roomLoader helper class for loading and streaming
    /// room data
    /// </summary>
    public RoomLoader roomLoader = null;

    /// <summary>
    /// The name of the room to be loaded
    /// </summary>
    public string roomName = "UW1_260";

    /// <summary>
    /// Transform that determines the location, orientation, and scale of the room
    /// when it gets loaded in.
    /// </summary>
    public Transform roomOrigin = null;

    public override void Awake()
    {
        base.Awake();
        roomLoader = GetComponent<RoomLoader>();
        Debug.Assert(roomOrigin != null);
    }

    /// <summary>
    /// Initializes The RoomWorld. Begins by calling the base (PortalWorld)
    /// Init method, then continues by telling the RoomLoader to load
    /// the room determined by "roomName" in.
    /// </summary>
    public override void Init()
    {
        base.Init();
        Debug.Log("Room World " + name);

        //load in the room geometry
        GetComponent<RoomLoader>().LoadRoom(roomName, roomOrigin);
    }
}
