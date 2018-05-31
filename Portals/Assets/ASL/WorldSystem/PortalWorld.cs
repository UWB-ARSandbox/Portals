using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL.PortalSystem;
using ASL.WorldSystem;

 /// <summary>
 /// The PortalWorld class has a portal that users can travel through
 /// and connect to other worlds
 /// </summary>
public class PortalWorld : World {
    public PortalManager portalManager = null;
    public Transform defaultPortalXform = null;
    public Portal defaultPortal = null;
    public Portal.ViewType defaultPortalViewType = Portal.ViewType.VIRTUAL;
    public PortalSelector selector = null;

    public override void Awake()
    {
        base.Awake();

        portalManager = GameObject.Find("PortalManager").GetComponent<PortalManager>();
        Debug.Assert(defaultPortalXform != null);
        Debug.Assert(portalManager != null);
        Debug.Assert(selector != null);

    }

	// Use this for initialization
    /// <summary>
    /// Initialize the PortalWorld. It creates and registers a new portal with the
    /// portal manager, and adds it as a child in the world manager.
    /// </summary>
	public override void Init () {
        base.Init();

        //instantiate a portal as well if we are the master client
        if(network.MasterClient &&
            PhotonNetwork.inRoom &&
           defaultPortalXform != null)
        {
            defaultPortal = portalManager.MakePortal(defaultPortalXform.position, defaultPortalXform.forward, defaultPortalXform.up, defaultPortalViewType);
            portalManager.RequestRegisterPortal(defaultPortal);
            worldManager.AddToWorld(this, defaultPortal.gameObject);
        }
        else if(PhotonNetwork.inRoom && defaultPortalXform != null)
        {
            Portal portal = GetComponentInChildren<Portal>();
            if (portal != null)
            {
                defaultPortal = portal;
            }
            else
            {
                Debug.Log("No default portal found for World: " + gameObject.name);
            }
        }

        //Initialize the portal selector
        Camera cam = Camera.main;
        Debug.Assert(defaultPortal != null);
        selector.Initialize(cam, defaultPortal);


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
