using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAvatar : MonoBehaviour {
    private bool initialized = false;
    private bool controlled = false;

    public float movementSpeed = 10.0f;
    public float rotateSpeed = 10.0f;
    public float acceleration = 10.0f;

    private Rigidbody rigidBody = null;

    public Camera userCamera = null;
    public MasterController controller = null;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody>();
        Debug.Assert(rigidBody != null);
    }

    public void Initialize(AvatarInfo avatarProperties, Camera mainCamera, MasterController mc)
    {
        SetColor(avatarProperties.color);
        transform.localPosition = avatarProperties.spawnPosition;

        if (avatarProperties.playerID == PhotonNetwork.player.ID) {
            controlled = true;
            controller = mc;
            userCamera = mainCamera;
            userCamera.transform.SetParent(transform);
            userCamera.transform.localPosition = 0.5f * transform.up;
        }

        initialized = true;
    }

    public void SetColor(Color toSet)
    {
        GetComponent<MeshRenderer>().material.color = toSet;
    }

    // Update is called once per frame
    void Update () {
        if (!initialized)
            return;

        if (controlled)
        {
            PlayerMovementControls();
        }
    }

    private void PlayerMovementControls()
    {

        #region WASD
        Vector3 moveVelocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            //transform.localPosition += transform.forward * Time.deltaTime * movementSpeed;
            moveVelocity += transform.forward * acceleration;
            
        }        

        if (Input.GetKey(KeyCode.A))
        {
            //transform.localPosition -= transform.right * Time.deltaTime * movementSpeed;
            moveVelocity -= transform.right * movementSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            //transform.localPosition -= transform.forward * Time.deltaTime * movementSpeed;
            moveVelocity -= transform.forward * movementSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //transform.localPosition += transform.right * Time.deltaTime * movementSpeed;
            moveVelocity += transform.right * movementSpeed;
        }
        rigidBody.velocity = moveVelocity;



        #endregion

        #region CAM_ROTATE
        if (Input.GetMouseButton(1))
        {
            float deltaMouseX = Input.GetAxis("Mouse X");
            float deltaMouseY = Input.GetAxis("Mouse Y");
            
            transform.Rotate(Vector3.up, deltaMouseX * rotateSpeed);
            userCamera.transform.Rotate(Vector3.right, -deltaMouseY * rotateSpeed);
        }
        #endregion
    }
}
