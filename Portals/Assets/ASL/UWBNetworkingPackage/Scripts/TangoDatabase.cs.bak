using System;
using UnityEngine;
using System.Collections.Generic;
//using HoloToolkit.Unity;

namespace UWBNetworkingPackage
{
    /// <summary>
    /// Database is a static class that stores the most recently sent Room Mesh (the Room Mesh is created by a HoloLens),
    /// and allows any classes in the UWBNetworkingPackage to access it
    /// </summary>
    // Note: For future improvment, you should add: a) Parameter check
    public class TangoDatabase
    {
        #region Private Properties

        /// <summary>
        /// Keeps track of each room gameobjects info
        /// </summary>
        public struct TangoRoom
        {
            public byte[] _meshes;
            public bool isDirty;
            public int ID;
            public int PhotonPlayer;
            public string name;
        }

        /// <summary>
        /// keeps track of data that needs to be requested
        /// </summary>
        public struct TangoData
        {
            public string name;
            public int size;
        }

        //values that keep track of the current count of rooms
        public static int count = 0;
        public static int ID = 0;

        //List of TangoRooms to keep track of all rooms
        public static List<TangoRoom> Rooms = new List<TangoRoom>();

        private static byte[] _meshes;  // Stores the current Room Mesh data as a serialized byte array

        #endregion

        #region Public Properties

        public static DateTime LastUpdate = DateTime.MinValue;  // Used for keeping the Room Map up-to-date

        #endregion

        /// <summary>
        /// Retrieves the Room Mesh as a deserialized list
        /// </summary>
        /// <returns>Deserialized Room Mesh</returns>
        public static IEnumerable<Mesh> GetMeshAsList()
        {
            return NetworkingPackage.SimpleMeshSerializerTango.Deserialize(_meshes);
        }
        public static IEnumerable<Mesh> GetMeshAsList(TangoRoom T)
        {
            return NetworkingPackage.SimpleMeshSerializerTango.Deserialize(T._meshes);
        }

        /// <summary>
        /// Check to see if the TangoRoom exists and returns true or false
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool LookUpName(string name)
        {
            lock (Rooms)
            {
                foreach (TangoRoom T in Rooms)
                {
                    if (name == T.name)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Compares a list of of Room Names against the list of current rooms and removes any that have been deleted from the MasterServer
        /// </summary>
        /// <param name="TData"></param>
        public static void CompareList(List<string> TData)
        {
            lock (Rooms)
            {
                foreach (TangoRoom T in Rooms)
                {
                    bool found = false;
                    foreach (string name in TData)
                    {
                        if (T.name == name)
                        {
                            found = true;
                        }
                    }

                    if (found == false)
                    {
                        GameObject.Destroy(GameObject.Find(T.name).gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the Room Mesh as a serialized byte array
        /// </summary>
        /// <returns>Serialized Room Mesh</returns>
        public static byte[] GetMeshAsBytes()
        {
            return _meshes;
        }

        /// <summary>
        /// Retrieves a specific Room Mesh as a serialized byte array
        /// </summary>
        /// <param name="T"></param>
        /// <returns>Serialized Room Mesh</returns>
        public static byte[] GetMeshAsBytes(TangoRoom T)
        {
            return T._meshes;
        }

        /// <summary>
        /// Returns the specific TangoRoom based on ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns>TangoRoom</returns>
        public static TangoRoom GetRoom(int ID)
        {
            lock (Rooms)
            {
                return Rooms[ID];
            }
        }

        /// <summary>
        /// Deletes the TangoRoom from the Room List
        /// </summary>
        /// <param name="T"></param>
        public static void DeleteRoom(TangoRoom T)
        {
            lock (Rooms)
            {
                Rooms.Remove(T);
            }
        }

        /// <summary>
        /// Gets a string of all Rooms currently in the list of Rooms
        /// </summary>
        /// <returns>string of all rooms</returns>
        public static string GetAllRooms()
        {
            string data = Rooms[0].name;
            data += '~';
            data += Rooms[0]._meshes.Length;

            for (int i = 1; i < Rooms.Count; i++)
            {
                data += '~';
                data += Rooms[i].name;
                data += '~';
                data += Rooms[i]._meshes.Length;
            }

            return data;
        }

        /// <summary>
        /// Returns a TangoRoom based on the input name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>TangoRoom</returns>
        public static TangoRoom GetRoomByName(string name)
        {
            lock (Rooms)
            {
                foreach (TangoRoom T in Rooms)
                {
                    if (name == T.name)
                    {
                        return T;
                    }
                }
            }
            return new TangoRoom();
        }

        /// <summary>
        /// Update the currently saved mesh to be the given deserialized Room Mesh
        /// This method will also update the LastUpdate time
        /// </summary>
        /// <param name="newMesh">Deserialized Room Mesh stored in a list</param>
        public static void UpdateMesh(IEnumerable<Mesh> newMesh)
        {
            _meshes = NetworkingPackage.SimpleMeshSerializerTango.Serialize(newMesh);
            
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Update the currently saved mesh to be the given serialized Room Mesh
        /// This method will also update the LastUpdate time
        /// </summary>
        /// <param name="newMesh">Serialized Room Mesh stored in a byte array</param>
        public static void UpdateMesh(byte[] newMesh, int playerID)
        {
            TangoRoom T = new TangoRoom();
            T.isDirty = true; //keeps track that room needs to be rendered
            T._meshes = newMesh; //init the byte array
            count++; //increment total count
            T.ID = count; //ID = count
            T.PhotonPlayer = playerID; //inits the player ID it was recieved from
            string name = (string)(playerID + "_" + DateTime.Now); //creates a unique name based on ID and time
            name = name.Replace('/', '_');
            name = name.Replace('\\', '_');
            name = name.Replace(' ', '_');
            name = name.Replace(':', '_');
            T.name = name;
            lock (Rooms)
            {
                Rooms.Add(T); //adds room the list
            }
            //_meshes = newMesh;
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Updates room list with a new room with an established name
        /// </summary>
        /// <param name="newMesh"></param>
        /// <param name="name"></param>
        public static void UpdateMesh(byte[] newMesh, string name)
        {
            TangoRoom T = new TangoRoom(); //creates a new room
            T.isDirty = true; //marks that it needs to be rendered
            T._meshes = newMesh; //init byte array
            count++; //increment count
            T.ID = count; //set id
            //T.PhotonPlayer = playerID;
            T.name = (string)(name); //creates name based on input name
            lock (Rooms)
            {
                Rooms.Add(T); //adds room to list
            }
            //_meshes = newMesh;
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Delete the currently held Room Mesh
        /// This method will also update the LastUpdate time
        /// </summary>
        public static void DeleteMesh()
        {
            Debug.Log("Deleting Mesh");
            _meshes = null;
            LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Update the currently saved mesh to add the new mesh
        /// This method will also update the LastUpdate time
        /// </summary>
        /// <param name="newMesh">Serialized Room Mesh stored in a byte array</param>
        public static void AddToMesh(byte[] newMesh)
        {
            int length = newMesh.Length + _meshes.Length;
            byte[] totalMesh = new byte[length];
            Buffer.BlockCopy(_meshes, 0, totalMesh, 0, _meshes.Length);
            Buffer.BlockCopy(newMesh, 0, totalMesh, _meshes.Length, newMesh.Length);
            LastUpdate = DateTime.Now;
        }
    }
}