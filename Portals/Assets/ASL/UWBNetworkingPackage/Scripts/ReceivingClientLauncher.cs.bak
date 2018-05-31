using NetworkingPackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace UWBNetworkingPackage
{
    /// <summary>
    /// ReceivingClientLauncher is an abstract class (extended by all "Client" devices - Vive, Oculus, Kinect) that connects 
    /// to Photon and sets up a TCP connection with the Master Client to recieve Room Meshes when they are sent
    /// </summary>
    public abstract class ReceivingClientLauncher : Launcher
    {
        #region Private Properties
        private DateTime lastRoomUpdate = DateTime.MinValue;
        private DateTime _lastUpdate = DateTime.MinValue;
        List<Thread> threads = new List<Thread>(); //List of thread that manage TCP conncetions
        Stack<TangoDatabase.TangoData> TangoRoomStack = new Stack<TangoDatabase.TangoData>(); //TangoRoomStack for requesting Rooms from Master Client
        private static bool _ThreadFinished = true; //Check to see that a room request has finished
        #endregion

        //// Ensure not HoloLens

        public override void Update()
        {
            //    if (Database.LastUpdate != DateTime.MinValue && DateTime.Compare(_lastUpdate, Database.LastUpdate) < 0)
            //    {
            //        if (Database.GetMeshAsBytes() != null)
            //        {
            //            //Create a material to apply to the mesh
            //            Material meshMaterial = new Material(Shader.Find("Diffuse"));

            //            //grab the meshes in the database
            //            IEnumerable<Mesh> temp = new List<Mesh>(Database.GetMeshAsList());

            //            foreach (var mesh in temp)
            //            {
            //                //for each mesh in the database, create a game object to represent
            //                //and display the mesh in the scene
            //                GameObject obj1 = new GameObject("mesh"); 

            //                //add a mesh filter to the object and assign it the mesh
            //                MeshFilter filter = obj1.AddComponent<MeshFilter>();
            //                filter.mesh = mesh;

            //                //add a mesh rendererer and add a material to it
            //                MeshRenderer rend1 = obj1.AddComponent<MeshRenderer>();
            //                rend1.material = meshMaterial;
            //            }
            //        }
            //        _lastUpdate = Database.LastUpdate;
            //    }

            //    //Loading a mesh from a file for testing purposes.
            //    if (Input.GetKeyDown("l"))
            //    {
            //        //Database.UpdateMesh(MeshSaver.Load("RoomMesh"));
            //        var memoryStream = new MemoryStream(File.ReadAllBytes("RoomMesh"));
            //        this.DeleteLocalRoomModelInfo();
            //        Database.UpdateMesh(memoryStream.ToArray());
            //    }

            //    //Testcalls for the added functionality
            //    if (Input.GetKeyDown("s"))
            //    {
            //        this.SendMesh();
            //    }

            //    if (Input.GetKeyDown("d"))
            //    {
            //        this.DeleteRoomInfo();
            //    }

            //    if (Input.GetKeyDown("a"))
            //    {
            //        this.SendAddMesh();
            //    }

            //Check all current Rooms in the database and render any without a current gameobject and mesh renderer
            if (TangoDatabase.LastUpdate != DateTime.MinValue && DateTime.Compare(_lastUpdate, TangoDatabase.LastUpdate) < 0)
            {
                for (int i = 0; i < TangoDatabase.Rooms.Count; i++)
                {
                    TangoDatabase.TangoRoom T = TangoDatabase.GetRoom(i);

                    if (TangoDatabase.ID < T.ID)
                    {
                        TangoDatabase.ID = T.ID;
                        //    Create a material to apply to the mesh
                        Material meshMaterial = new Material(Shader.Find("Diffuse"));

                        //    grab the meshes in the database
                        IEnumerable<Mesh> temp = new List<Mesh>(TangoDatabase.GetMeshAsList(T));

                        foreach (var mesh in temp)
                        {
                            //        for each mesh in the database, create a game object to represent
                            //        and display the mesh in the scene
                            GameObject obj1 = new GameObject(T.name);

                            //        add a mesh filter to the object and assign it the mesh
                            MeshFilter filter = obj1.AddComponent<MeshFilter>();
                            filter.mesh = mesh;

                            //        add a mesh rendererer and add a material to it
                            MeshRenderer rend1 = obj1.AddComponent<MeshRenderer>();
                            rend1.material = meshMaterial;

                            rend1.material.shader = Shader.Find("Custom/UnlitVertexColor");

                            obj1.tag = "Room";
                            obj1.AddComponent<TangoRoom>();
                        }
                    }
                }
            }

            //Check to see that no rooms are currently being recieved and then check if there is a room to request
            if (_ThreadFinished == true)
            {
                if (TangoRoomStack.Count > 0)
                {
                    ReceiveTangoMeshStack();
                }
            }

            //check to see if a thread has stopped and join it
            Thread[] TL = threads.ToArray();
            foreach (Thread T in TL)
            {
                if (T.ThreadState == ThreadState.Stopped)
                {
                    T.Join();
                    //threads.Remove(T);
                }
            }
        }

        /// <summary>
        /// After connect to master server, join the room that's specify by Laucher.RoomName
        /// </summary>
        public override void OnConnectedToMaster()
        {
            bool roomFound = false;
            //RoomInfo[] roomList = PhotonNetwork.GetRoomList();
            //for (int i = 0; i < roomList.Length; i++)
            //{
            //    if (RoomName.Equals(roomList[i]))
            //    {
            //        PhotonNetwork.JoinRoom(RoomName);
            //        roomFound = true;
            //        break;
            //    }
            //}

            roomFound = PhotonNetwork.JoinRoom(RoomName);

            if (!roomFound)
            {
                Debug.LogError("PUN room not found. Please verify name and determine if master client has initialized room.");
                PhotonNetwork.Disconnect();
            }
        }

        /// <summary>
        /// After join the room, ask master client to sent the mesh to this client
        /// Will also request to send all asset bundles: note, if a new platform needs
        /// To be added the code below will have to be changed to accomidate new sets
        /// of AssetBundles
        /// </summary>
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Debug.Log("Client joined room.");
            


            //Debug.Log("Client joined room.");
            ////photonView.RPC("SendMesh", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
            ////photonView.RPC("SendRoomPrefabBundle", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
            //photonView.RPC("SendAssetBundles", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
            ////#if UNITY_ANDROID
            ////            photonView.RPC("SendAndroidBundles", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
            ////#elif !UNITY_EDITOR && UNITY_WSA_10_0
            ////            photonView.RPC("SendHololensBundles", PhotonTargets.MasterClient. PhotonNetwork.player.ID);
            ////#else
            ////            photonView.RPC("SendPCBundles", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
            ////#endif
            //////photonView.RPC("SendRoomModel", PhotonTargets.MasterClient, PhotonNetwork.player.ID);


            //string bundleName = UWB_Texturing.Config.AssetBundle.RoomPackage.CompileFilename();
            ////string ASLBundlePath = Config.AssetBundle.Current.CompileAbsoluteBundlePath(bundleName);
            ////RoomTextureManager.UpdateRoomBundle();
            //int roomModelPort = Config.Ports.GetPort(Config.Ports.Types.RoomBundle);
            ////SendAssetBundle(id, bundlePath, Config.Ports.RoomBundle);
            ////SendAssetBundle(ASLBundlePath, roomModelPort);
            ////photonView.RPC("ReceiveRoomModel", PhotonPlayer.Find(id), IPManager.CompileNetworkConfigString(roomModelPort), bundleName);
            //photonView.RPC("SendRoomModel", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
            //ReceiveRoomModel(IPManager.CompileNetworkConfigString(Config.Ports.RoomBundle), bundleName);

            ///////////SocketClient.ConnectTest(Config.Ports.Base);

//#if !UNITY_WSA_10_0
//            //string roomBundleDirectory = Config.AssetBundle.Current.CompileAbsoluteBundleDirectory();
//            string roomBundleDirectory = Config.Current.AssetBundle.CompileAbsoluteAssetDirectory();
//            SocketClient_PC.RequestFiles(ServerFinder.serverIP, Config.Ports.RoomBundle, roomBundleDirectory);
//            //string rawRoomBundleDirectory = Config.AssetBundle.Current.CompileAbsoluteBundleDirectory();
//            string rawRoomBundleDirectory = Config.Current.AssetBundle.CompileAbsoluteAssetDirectory();
//            SocketClient_PC.RequestFiles(ServerFinder.serverIP, Config.Ports.RoomResourceBundle, rawRoomBundleDirectory);
//            //string assetBundleDirectory = Config.AssetBundle.Current.CompileAbsoluteBundleDirectory();
//            string assetBundleDirectory = Config.Current.AssetBundle.CompileAbsoluteAssetDirectory();
//            SocketClient_PC.RequestFiles(ServerFinder.serverIP, Config.Ports.Bundle, assetBundleDirectory);
//#endif

        }

        /// <summary>
        /// When cannot join the room (refer to UWBNetworkingPackage documentation for possible reasons of failure), 
        /// disconnect from Photon
        /// </summary>
        /// <param name="codeAndMsg">Information about the failed connection</param>
        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            Debug.Log("A room created by the Master Client could not be found. Disconnecting from PUN");
            PhotonNetwork.Disconnect();
        }

        ///// <summary>
        ///// Sends currently held mesh to the master client
        ///// </summary>
        //public override void SendMesh()
        //{
        //    if (File.Exists(UWB_Texturing.Config.AssetBundle.RoomPackage.CompileAbsoluteAssetPath(UWB_Texturing.Config.AssetBundle.RoomPackage.CompileFilename())))
        //    {
        //        photonView.RPC("ReceiveRoomPrefabBundle", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
        //    }
        //    else if (Database.GetMeshAsBytes() != null)
        //    {
        //        photonView.RPC("ReceiveMesh", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
        //    }
        //}

        ///// <summary>
        ///// Send mesh to add to the mesh held in the database and will then be forwarded to all
        ///// clients
        ///// </summary>
        //public override void SendAddMesh()
        //{
        //    if (Database.GetMeshAsBytes() != null)
        //    {
        //        photonView.RPC("ReceiveAddMesh", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
        //    }
        //}

        /// <summary>
        /// This will send a call to delete all meshes held by the clients
        /// This is a RPC method that will be called by ReceivingClient
        /// </summary>
        public void UpdateRoomInfoImmediately(string roomName)
        {
            //if (File.Exists(UWB_Texturing.Config.AssetBundle.RoomPackage.CompileAbsoluteAssetPath(UWB_Texturing.Config.AssetBundle.RoomPackage.CompileFilename())))
            //{
            //    photonView.RPC("DeleteLocalRoomModelInfo", PhotonTargets.MasterClient);
            //    SendRoomModel(PhotonNetwork.player.ID);
            //}
            ////if (Database.GetMeshAsBytes() != null)
            ////{
            ////    photonView.RPC("DeleteMesh", PhotonTargets.MasterClient);
            ////}

            //string filepath = Config.AssetBundle.Current.CompileAbsoluteAssetPath(UWB_Texturing.Config.AssetBundle.RoomPackage.CompileFilename());
            string filepath = Config.Current.AssetBundle.CompileAbsoluteAssetPath(UWB_Texturing.Config.AssetBundle.RoomPackage.CompileFilename());
            if (!PhotonNetwork.isMasterClient
                && File.Exists(filepath))
            {
                photonView.RPC("DeleteLocalRoomModelInfo", PhotonTargets.MasterClient, roomName);
                int roomBundlePort = Config.Ports.RoomBundle_ClientToServer;
#if !UNITY_WSA_10_0
                SocketClient_PC.SendFile(ServerFinder.serverIP, roomBundlePort, filepath);
#endif
            }
        }


        //#region RPC Method

        ///// <summary>
        ///// This will send a mesh to the master client which will updates
        ///// the meshes across the network
        ///// </summary>
        ///// <param name="networkConfig"></param>
        //[PunRPC]
        //public override void SendMesh(string networkConfig)
        //{
        //    var networkConfigArray = networkConfig.Split(':');

        //    TcpClient client = new TcpClient();
        //    client.Connect(IPAddress.Parse(networkConfigArray[0]), Int32.Parse(networkConfigArray[1]));
        //    new Thread(() =>
        //    {
        //        using (NetworkStream stream = client.GetStream())
        //        {
        //            var data = Database.GetMeshAsBytes();
        //            stream.Write(data, 0, data.Length);
        //            Debug.Log("Mesh sent: mesh size = " + data.Length);
        //        }
        //        client.Close();
        //    }).Start();

        //}

        ///// <summary>
        ///// Receive Bundles from the Master client.  Loads all assets from these bundles.
        ///// </summary>
        ///// <param name="networkConfig"></param>
        //[PunRPC]
        //public void ReceiveBundles(string networkConfig)
        //{
        //    var networkConfigArray = networkConfig.Split(':');
        //    Debug.Log("Start receiving bundle.");
        //    TcpClient client = new TcpClient();
        //    Debug.Log("IP Address = " + IPAddress.Parse(networkConfigArray[0]).ToString());
        //    Debug.Log("networkConfigArray[1] = " + Int32.Parse(networkConfigArray[1]));
        //    client.Connect(IPAddress.Parse(networkConfigArray[0]), Int32.Parse(networkConfigArray[1]));
        //    Debug.Log("Client connected to server!");
        //    using (var stream = client.GetStream())
        //    {
        //        byte[] data = new byte[1024];
        //        Debug.Log("Byte array allocated");

        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            Debug.Log("MemoryStream created");
        //            int numBytesRead;
        //            while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
        //            {
        //                ms.Write(data, 0, numBytesRead);
        //                Debug.Log("Data received! Size = " + numBytesRead);
        //            }
        //            Debug.Log("Finish receiving bundle: size = " + ms.Length);
        //            client.Close();

        //            this.networkAssets = AssetBundle.LoadFromMemory(ms.ToArray());

        //            // Save the asset bundle
        //            File.WriteAllBytes(Path.Combine(Application.dataPath, "ASL/Resources/StreamingAssets/AssetBundlesPC/" + this.networkAssets.name + ".asset"), ms.ToArray());
        //            File.WriteAllBytes(Path.Combine(Application.dataPath, "ASL/Resources/StreamingAssets/AssetBundlesAndroid/" + this.networkAssets.name + ".asset"), ms.ToArray());

        //            //AssetBundle newBundle = AssetBundle.LoadFromMemory(ms.ToArray());
        //            //bundles.Add(bundleName, newBundle);
        //            Debug.Log("You loaded the bundle successfully.");

        //        }
        //    }

        //    client.Close();

        //    // Debug.Log("Bundle's name is " + this.networkAssets.name);
        //}

        //[PunRPC]
        //public void ReceiveRoomBundle(string networkConfig)
        //{

        //}

        ///// <summary>
        ///// Receive Room Mesh from specified network configuration. This is a RPC method that will be called by the Master Client
        ///// </summary>
        ///// <param name="networkConfig">The IP and port number that client can reveice room mesh from. The format is IP:Port</param>
        //[PunRPC]
        //public override void ReceiveMesh(string networkConfig)
        //{
        //    this.DeleteLocalRoomModelInfo();
        //    var networkConfigArray = networkConfig.Split(':');

        //    TcpClient client = new TcpClient();
        //    client.Connect(IPAddress.Parse(networkConfigArray[0]), Int32.Parse(networkConfigArray[1]));

        //    using (var stream = client.GetStream())
        //    {
        //        byte[] data = new byte[1024];

        //        Debug.Log("Start receiving mesh.");
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            int numBytesRead;
        //            while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
        //            {
        //                ms.Write(data, 0, numBytesRead);
        //            }
        //            Debug.Log("Finish receiving mesh: size = " + ms.Length);
        //            client.Close();

        //            //DONE RECIEVING MESH FROM THE MASTER SERVER, NOW UPDATE IT

        //            Database.UpdateMesh(ms.ToArray());
        //            Debug.Log("You updated the meshes in the database");
        //        }
        //    }

        //    client.Close();


        //    //CREATE AND DRAW THEM MESHES------------------------------------------------------
        //    Debug.Log("Checking for them meshes in ze database");

        //    //goes into the if statement if the database is not NULL
        //    if (Database.GetMeshAsList() != null)
        //    {
        //        //Create a material to apply to the mesh
        //        Material meshMaterial = new Material(Shader.Find("Diffuse"));

        //        //grab the meshes in the database
        //        IEnumerable<Mesh> temp = new List<Mesh>(Database.GetMeshAsList());

        //        foreach (var mesh in temp)
        //        {
        //            //for each mesh in the database, create a game object to represent
        //            //and display the mesh in the scene
        //            GameObject obj1 = new GameObject("mesh");

        //            //add a mesh filter to the object and assign it the mesh
        //            MeshFilter filter = obj1.AddComponent<MeshFilter>();
        //            filter.mesh = mesh;

        //            //add a mesh rendererer and add a material to it
        //            MeshRenderer rend1 = obj1.AddComponent<MeshRenderer>();
        //            rend1.material = meshMaterial;
        //        }
        //    }
        //    else
        //    {
        //        UnityEngine.Debug.Log("YO... your mesh is empty...");
        //    }
        //    //END OF CREATING AND DRAWING THE MEESHES------------------------------------------
        //}



        ///// <summary>
        ///// Initiates the sending of a Mesh to add
        ///// </summary>
        //[PunRPC]
        //public override void SendAddMesh(string networkConfig)
        //{
        //    var networkConfigArray = networkConfig.Split(':');

        //    TcpClient client = new TcpClient();
        //    client.Connect(IPAddress.Parse(networkConfigArray[0]), Int32.Parse(networkConfigArray[1]));
        //    new Thread(() =>
        //    {
        //        using (NetworkStream stream = client.GetStream())
        //        {
        //            var data = Database.GetMeshAsBytes();
        //            stream.Write(data, 0, data.Length);
        //            Debug.Log("Mesh sent: mesh size = " + data.Length);
        //        }
        //        client.Close();
        //    }).Start();
        //}

        ///// <summary>
        ///// Receive room mesh to add to total mesh (ReceiveClientLauncher)
        ///// and add it to the total roommesh
        ///// </summary>
        ///// <param name="networkConfig">The player id that will receive mesh</param>
        //[PunRPC]
        //public override void ReceiveAddMesh(string networkConfig)
        //{
        //    var networkConfigArray = networkConfig.Split(':');

        //    TcpClient client = new TcpClient();
        //    client.Connect(IPAddress.Parse(networkConfigArray[0]), Int32.Parse(networkConfigArray[1]));

        //    using (var stream = client.GetStream())
        //    {
        //        byte[] data = new byte[1024];

        //        Debug.Log("Start receiving mesh.");
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            int numBytesRead;
        //            while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
        //            {
        //                ms.Write(data, 0, numBytesRead);
        //            }
        //            Debug.Log("Finish receiving mesh: size = " + ms.Length);
        //            client.Close();

        //            //DONE RECIEVING MESH FROM THE MASTER SERVER, NOW UPDATE IT
        //            Database.AddToMesh(ms.ToArray());
        //            Debug.Log("You updated the meshes in the database");
        //        }
        //    }

        //    client.Close();


        //    //CREATE AND DRAW THEM MESHES------------------------------------------------------
        //    Debug.Log("Checking for them meshes in ze database");

        //    //goes into the if statement if the database is not NULL
        //    if (Database.GetMeshAsBytes() != null)
        //    {
        //        //Create a material to apply to the mesh
        //        Material meshMaterial = new Material(Shader.Find("Diffuse"));

        //        //grab the meshes in the database
        //        IEnumerable<Mesh> temp = new List<Mesh>(Database.GetMeshAsList());

        //        foreach (var mesh in temp)
        //        {
        //            //for each mesh in the database, create a game object to represent
        //            //and display the mesh in the scene
        //            GameObject obj1 = new GameObject("mesh");

        //            //add a mesh filter to the object and assign it the mesh
        //            MeshFilter filter = obj1.AddComponent<MeshFilter>();
        //            filter.mesh = mesh;

        //            //add a mesh rendererer and add a material to it
        //            MeshRenderer rend1 = obj1.AddComponent<MeshRenderer>();
        //            rend1.material = meshMaterial;
        //        }
        //    }
        //    else
        //    {
        //        UnityEngine.Debug.Log("YO... your mesh is empty...");
        //    }
        //    //END OF CREATING AND DRAWING THE MEESHES------------------------------------------
        //}

        //    #endregion

            /// <summary>
            /// PunRPC call with the current MasterClient Room info
            /// Traverses through the list and determines if any rooms have been added or removed
            /// </summary>
            /// <param name="data"></param>
        [PunRPC]
        public override void RecieveTangoRoomInfo(string data)
        {
            var dataArray = data.Split('~'); //parse data
            UnityEngine.Debug.Log("RecievedTangoInfo " + dataArray.Length);
            List<String> RoomNames = new List<string>();

            if (dataArray.Length > 1)
            {
                for (int i = 0; i < dataArray.Length; i += 2)
                {
                    TangoDatabase.TangoData T = new TangoDatabase.TangoData();
                    T.name = dataArray[i];
                    RoomNames.Add(T.name);
                    T.size = System.Int32.Parse(dataArray[i + 1]);

                    UnityEngine.Debug.Log(i + " " + T.name + " " + T.size);
                    if (!TangoDatabase.LookUpName(T.name))
                    {
                        TangoRoomStack.Push(T); //if room does not already exist, add to stack
                    }
                }
            }
            else
            {
                RoomNames.Add(" ");
            }

            TangoDatabase.CompareList(RoomNames); //delete any rooms missing
        }

        /// <summary>
        /// Creates a thread to recieve a TangoMesh based on the mesh currently at the top of the stack
        /// Sends a RPC call to the masterclient with the name of the room
        /// </summary>
        public virtual void ReceiveTangoMeshStack()
        {
            TangoDatabase.TangoData T = TangoRoomStack.Pop();

            if (!TangoDatabase.LookUpName(T.name))
            {
                _ThreadFinished = false;
                Thread Th = new Thread(() => RecieveTangoMeshThread(T));
                Th.IsBackground = true;
                Th.Start();
                threads.Add(Th);


                photonView.RPC("SendTangoMeshByName", PhotonTargets.MasterClient, GetLocalIpAddress() + "~" + (Port + 1) + "~" + T.name);
            }
        }

        /// <summary>
        /// Listens for the TCP connection from the master client and recieves the room mesh
        /// </summary>
        /// <param name="T"></param>
        void RecieveTangoMeshThread(TangoDatabase.TangoData T)
        {
            TcpListener receiveTcpListener = new TcpListener(IPAddress.Any, (Port + 1));
            try
            {
                receiveTcpListener.Start();
            }
            catch (SocketException)
            {
                UnityEngine.Debug.Log("Error");
                _ThreadFinished = true;
                Thread.CurrentThread.Abort();
            }

            var client = receiveTcpListener.AcceptTcpClient();

            using (var stream = client.GetStream())
            {
                byte[] data = new byte[T.size];

                UnityEngine.Debug.Log("Start receiving mesh " + data.Length);
                using (MemoryStream ms = new MemoryStream())
                {
                    int numBytesRead;
                    while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
                    {
                        ms.Write(data, 0, numBytesRead);
                    }
                    UnityEngine.Debug.Log("finish receiving mesh: size = " + ms.Length);
                    TangoDatabase.UpdateMesh(ms.ToArray(), T.name);
                }
            }

            client.Close();

            receiveTcpListener.Stop();

            _ThreadFinished = true;

            UnityEngine.Debug.Log("Join Thread");
            Thread.CurrentThread.Join();
        }

        /// <summary>
        /// Creates a thread to manage the TCP connection of sending a room mesh to master client
        /// </summary>
        /// <param name="networkConfig"></param>
        [PunRPC]
        public override void SendTangoMesh(string networkConfig)
        {
            Thread T = new Thread(() => SendTangoMeshThread(networkConfig));
            T.IsBackground = true;
            T.Start();
            threads.Add(T);
        }

        /// <summary>
        /// thread function to establish TCP connection and send byte array of the Tango Mesh
        /// </summary>
        /// <param name="networkConfig"></param>
        void SendTangoMeshThread(string networkConfig)
        {
            var networkConfigArray = networkConfig.Split(':');


            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse(networkConfigArray[0]), Int32.Parse(networkConfigArray[1]));

            using (NetworkStream stream = client.GetStream())
            {
                var data = TangoDatabase.GetMeshAsBytes();
                stream.Write(data, 0, data.Length);
                Debug.Log("Mesh sent: mesh size = " + data.Length);
            }
            client.Close();


            Thread.CurrentThread.Join();
        }

        private IPAddress GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    return ip;
                }
            }
            return null;
        }

        void OnApplicationQuit()
        {
            foreach (Thread T in threads)
            {
                T.Abort();
                //threads.Remove(T);
            }
        }
    }
}
