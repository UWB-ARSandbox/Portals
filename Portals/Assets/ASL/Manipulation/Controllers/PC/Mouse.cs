using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL.Manipulation.Objects;

namespace ASL.Manipulation.Controllers.PC
{
    public class Mouse : MonoBehaviour
    {
        private ObjectInteractionManager objManager;

        public void Awake()
        {
            objManager = GameObject.Find("ObjectInteractionManager").GetComponent<ObjectInteractionManager>();
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameObject selectedObject = Select();
                objManager.RequestOwnership(selectedObject);
            }
            if (Input.GetMouseButtonDown(1))
            {
                string prefabName = "Sphere";
                Vector3 position = new Vector3(0, 0, 2);
                Quaternion rotation = Quaternion.identity;
                Vector3 scale = Vector3.one;
                //objManager.Instantiate(prefabName, position, rotation, scale);
                //GameObject go = objManager.Instantiate(prefabName);
                GameObject go = objManager.InstantiateOwnedObject(prefabName);
                go.transform.Translate(position);

                UWBNetworkingPackage.NetworkManager nm = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
                List<int> IDsToAdd = new List<int>();
                IDsToAdd.Add(2);
                nm.WhiteListOwnership(go, IDsToAdd);
                //nm.RestrictOwnership(go, IDsToAdd);
            }
        }

        public GameObject Select()
        {
            Camera cam = GameObject.FindObjectOfType<Camera>();
            Vector3 mousePos = Input.mousePosition;
            Vector3 mouseRay = cam.ScreenToWorldPoint(mousePos);
            RaycastHit hit;
            Physics.Raycast(cam.ScreenPointToRay(mousePos), out hit);

            if (hit.collider != null)
            {
                return hit.collider.gameObject;
            }
            else
            {
                GameObject camera = GameObject.Find("Main Camera");
                if(camera != null)
                {
                    return camera;
                }
                else
                {
                    Debug.LogError("Cannot find camera object. Selecting null object.");
                    return null;
                }
            }
        }
    }
}