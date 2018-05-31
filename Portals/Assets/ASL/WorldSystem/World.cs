using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.WorldSystem;

/// <summary>
/// The Primary World Class. Worlds with extra initialization functionality, etc.
/// should inherit from this one
/// </summary>
public class World : MonoBehaviour {
    public UWBNetworkingPackage.NetworkManager network = null;
    public WorldManager worldManager = null;

    public virtual void Awake()
    {
        network = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        Debug.Assert(network != null);
        Debug.Assert(worldManager != null);
    }

    /// <summary>
    /// Initializes the world. 
    /// </summary>
    public virtual void Init() {
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
