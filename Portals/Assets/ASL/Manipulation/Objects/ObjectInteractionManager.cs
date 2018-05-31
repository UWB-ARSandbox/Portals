using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.Manipulation.Objects
{
    public class ObjectInteractionManager : MonoBehaviour
    {
        private UWBNetworkingPackage.NodeType platform;
        private UWBNetworkingPackage.NetworkManager networkManager;
        public event ObjectSelectedEventHandler FocusObjectChangedEvent;

        public void RequestOwnership(GameObject obj)
        {
            OnObjectSelected(obj);
            networkManager.RequestOwnership(obj);
        }

        public void Focus(GameObject obj)
        {
            OnObjectSelected(obj);
        }

        protected void OnObjectSelected(GameObject obj)
        {
            int focuserID = PhotonNetwork.player.ID;
            //Debug.Log("About to trigger On Object Selected event");
            if (obj != null)
            {
                if (obj.GetPhotonView() != null)
                {
                    FocusObjectChangedEvent(new ObjectSelectedEventArgs(obj, obj.GetPhotonView().ownerId, focuserID));
                }
                else
                {
                    FocusObjectChangedEvent(new ObjectSelectedEventArgs(obj, 0, focuserID));
                }
            }
            else
            {
                FocusObjectChangedEvent(new ObjectSelectedEventArgs(obj, 0, focuserID));
            }
            //Debug.Log("Event triggered");
        }

        public void Awake()
        {
            networkManager = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
#if UNITY_WSA_10_0
#elif UNITY_ANDROID
            gameObject.AddComponent<ASL.Manipulation.Controllers.Android.BehaviorDifferentiator>();
#else
            UWBNetworkingPackage.NodeType platform = UWBNetworkingPackage.Config.NodeType;

            switch (platform)
            {
                case UWBNetworkingPackage.NodeType.PC:
                    gameObject.AddComponent<ASL.Manipulation.Controllers.PC.Mouse>();
                    gameObject.AddComponent<ASL.Manipulation.Controllers.PC.Keyboard>();
                    break;
                case UWBNetworkingPackage.NodeType.Kinect:
                    break;
                case UWBNetworkingPackage.NodeType.Vive:
                    //gameObject.AddComponent<ASL.Manipulation.Controllers.Vive.ControllerUIManager>();
                    break;
                case UWBNetworkingPackage.NodeType.Oculus:
                    break;
                default:
                    Debug.LogWarning("Unsupported platform encountered");
                    break;
            }
#endif
        }

        public void FixedUpdate()
        {
            // reset manager if platform is too quick to update properly at startup
            if (platform != UWBNetworkingPackage.Config.NodeType)
            {
                Resources.Load("Prefabs/ObjectInteractionManager");
                //GameObject.Destroy(gameObject);
            }
        }

        public GameObject Instantiate(GameObject go)
        {
            return networkManager.Instantiate(go);
        }

        public GameObject Instantiate(string prefabName)
        {
            return networkManager.Instantiate(prefabName);
        }

        public GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return networkManager.Instantiate(prefabName, position, rotation, scale);
        }

        public GameObject InstantiateOwnedObject(string prefabName)
        {
            return networkManager.InstantiateOwnedObject(prefabName);
        }

        public bool Destroy(GameObject go)
        {
            return networkManager.Destroy(go);
        }
        
        public T RegisterBehavior<T>()
        {
            if(gameObject.GetComponent<T>() != null)
            {
                Debug.Log("ObjectInteractionManager already has behavior attached. Ignoring request to reattach behavior script to avoid missing proper startup logic calls.");
            }
            else
            {
                gameObject.AddComponent(typeof(T));
            }

            return gameObject.GetComponent<T>();
        }
    }
}