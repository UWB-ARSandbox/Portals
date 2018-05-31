using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UWBNetworkingPackage;

namespace ASL.UI.Menus.Scanning
{
#if UNITY_EDITOR
    public class RoomSave : EditorWindow
    {
        #region Fields
        /// <summary>
        /// Text entry box
        /// </summary>
        public string RoomName = "TangoDefault";

        /// <summary>
        /// Default room name used
        /// </summary>
        public const string DefaultRoomName = "TangoDefault";
        
        /// <summary>
        /// Reference to the folder in which the Room objects are kept
        /// </summary>
        public string RoomFolder = "";

        public string TangoTag = "Tango";
        
        /// <summary>
        /// List of all Room objects
        /// </summary>
        private List<GameObject> roomList = new List<GameObject>();

        /// <summary>
        /// Reference to the persistent data path for the application
        /// </summary>
        private DirectoryInfo root;

        /// <summary>
        /// List of directories in the persistent data path
        /// </summary>
        private List<DirectoryInfo> directoryInfoList = new List<DirectoryInfo>();

        /// <summary>
        /// List of string names of the directories in the persistent data path
        /// </summary>
        private List<string> directoryPathList = new List<string>();

        private List<string> roomNameList = new List<string>();

        /// <summary>
        /// Room drop down value (i.e. index of room is selected)
        /// </summary>
        private int selected = 0;

        /// <summary>
        /// Stack to keep track of room loading
        /// </summary>
        private Stack<fileToLoad> FilesToLoad = new Stack<fileToLoad>();
        
        /// <summary>
        /// Struct to keep track of file path and name for loading (i.e. metadata)
        /// </summary>
        private struct fileToLoad
        {
            public string filePath;
            public string name;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates the UI for the Room Manager Window
        /// </summary>
        void OnGUI()
        {
            // Create a label for save section
            GUI.Label(new Rect(10, 5, position.width - 20, 20), "Directory to Save Rooms to: ");
            // Create a text entry for Directory creation/saving
            RoomName = EditorGUI.TextField(new Rect(10, 25, position.width - 20, 20),
                    "Room Name: ",
                    RoomName);
            
            // Create a save Rooms button and link it to the function
            if (GUI.Button(new Rect(10, 50, position.width - 20, 20), "Save Rooms"))
                SaveRooms(roomList, directoryInfoList);

            // If there are directories within the root directory
            if (directoryInfoList.Count > 0)
            {
                // Create the load lable
                // GUI.Label(new Rect(10, 85, position.width - 20, 20), "Directory to Load Rooms From: ");
                GUI.Label(new Rect(10, 85, position.width - 20, 20), "Room: ");
                
                // Get the currently selected item in the drop down
                //selected = EditorGUI.Popup(new Rect(10, 110, position.width - 20, 20), selected, DirNames.ToArray());
                selected = EditorGUI.Popup(new Rect(10, 110, position.width - 20, 20), selected, roomNameList.ToArray());

                // Create load button
                if (GUI.Button(new Rect(10, 135, position.width - 20, 20), "Load Selected Room"))
                    LoadRoom(directoryInfoList[selected]);

                // Create unload button
                if (GUI.Button(new Rect(10, 160, position.width - 20, 20), "Unload Selected Room"))
                    UnloadRoom(directoryInfoList[selected]);
            }

            // Create a delete all button
            if (GUI.Button(new Rect(10, 225, position.width - 20, 20), "Delete All Rooms"))
                deleteAllRooms();

            Repaint();
        }

        /// <summary>
        /// Goes through the list of directories and determines if the save directory already exists
        /// If the directory does not exist, it creates it
        /// Goes through each room game object and writes it to the directory
        /// </summary>
        /// <param name="roomList"></param>
        /// <param name="Dir"></param>
        public void SaveRooms(List<GameObject> roomList, List<DirectoryInfo> Dir)
        {
            //bool directoryExists = false;

            //if(RoomName == "")
            //{
            //    RoomName = "Room";
            //}

            //foreach(DirectoryInfo d in Dir)
            //{
            //    if (d.Name == RoomName)
            //    {
            //        directoryExists = true;
            //    }
            //}

            //if(directoryExists == false)
            //{
            //    Directory.CreateDirectory(root.FullName + '\\' + RoomName);
            //}

            foreach (GameObject room in roomList)
            {
                byte[] b = TangoDatabase.GetMeshAsBytes(TangoDatabase.GetRoomByName(room.name));
                string roomFolder = Path.Combine(root.FullName, RoomName);
                SetRoomFolder(roomFolder);

                string filename = room.name + Config.Current.Room.TangoFileExtension;
                string filepath = Path.Combine(RoomFolder, filename);

                //UnityEngine.Debug.Log(root.FullName + '\\' + RoomName + '\\' + room.name);
                //File.WriteAllBytes(root.FullName + '\\' + RoomName + '\\' + room.name, b);
                File.WriteAllBytes(filepath, b);

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
        }

        /// <summary>
        /// Gets each file within a directory and checks to see if the room gameobject already exists.
        /// if it does not exist it pushes the room to a stack to be read
        /// </summary>
        /// <param name="Dir"></param>
        public void LoadRoom(DirectoryInfo Dir)
        {
            foreach (FileInfo f in Dir.GetFiles())
            {
                fileToLoad file = new fileToLoad();
                int numNameComponents = f.Name.Split('.').Length;

                file.filePath = f.FullName;
                file.name = f.Name.Split('.')[0];
                string extension = '.' + f.Name.Split('.')[numNameComponents-1];
                
                if (extension.Equals(Config.Current.Room.TangoFileExtension))
                {
                    bool cached = false;
                    foreach (GameObject g in roomList)
                    {
                        if (g.name == file.name)
                        {
                            cached = true;
                        }
                    }
                    if (cached == false)
                    {
                        FilesToLoad.Push(file);
                    }
                }
            }
        }

        /// <summary>
        /// Goes through each file in a directory and room gameobject and deletes the appropriate rooms
        /// </summary>
        /// <param name="Dir"></param>
        public void UnloadRoom(DirectoryInfo Dir)
        {
            foreach (FileInfo f in Dir.GetFiles())
            {
                foreach (GameObject g in roomList)
                {
                    string fileName = f.Name.Split('.')[0];
                    if (g.name.Equals(fileName))
                    {
                        Destroy(g);
                    }
                }
            }
        }

        private void OnEnable()
        {
            Initialize();
        }

        /// <summary>
        /// Goes through each of the files to be loaded and load one per update
        /// </summary>
        [ExecuteInEditMode]
        void Update()
        {
            if (FilesToLoad.Count > 0)
            {
                fileToLoad f = FilesToLoad.Pop();
                ReadRoom(f.filePath, f.name);
            }
            
            RegisterAllTangoRooms();
            CullLists();
        }
        
        [ExecuteInEditMode]
        void OnDestroy()
        {
            Reset();
        }

        #region Helper Methods

        private void Initialize()
        {
            SetDefaultRoomFolder();
            Reset();

            // Get all directories in the root directory
            SetRoot();
            foreach (DirectoryInfo d in root.GetDirectories())
            {
                if (IsTangoRoomDirectory(d))
                {
                    RegisterDirectory(d);
                }
            }

            // Get all game objects with the "Room" tag
            RegisterAllTangoRooms();
        }

        private void Reset()
        {
            // Clear all lists
            roomList.Clear();
            directoryInfoList.Clear();
            directoryPathList.Clear();
            roomNameList.Clear();
        }

        private DirectoryInfo SetRoot()
        {
#if UNITY_EDITOR
            root = new DirectoryInfo(Config.Current.Room.CompileAbsoluteAssetDirectory());
#else
            root = new DirectoryInfo(Config.Current.Room.CompileUnityAssetDirectory());
#endif
            return root;
        }

        private DirectoryInfo SetDefaultRoomFolder()
        {
#if UNITY_EDITOR
            //DirectoryInfo dirInfo = SetRoomFolder(Config.Current.Room.CompileAbsoluteAssetDirectory(RoomName));
            DirectoryInfo dirInfo = null;
#else
            //DirectoryInfo dirInfo = SetRoomFolder(Config.Current.Room.CompileUnityAssetDirectory());
            DirectoryInfo dirInfo = null;
#endif
            return dirInfo;
        }

        private DirectoryInfo SetRoomFolder(string roomFolder)
        {
            DirectoryInfo dirInfo;
            RoomFolder = roomFolder;
            bool created = CreateRoomFolder(out dirInfo);

            // Handle cases where the directory already exists
            if (!created)
            {
                dirInfo = new DirectoryInfo(RoomFolder);
                if (!DirectoryInfoRegistered(RoomFolder))
                {
                    RegisterDirectory(dirInfo);
                    TagTangoRoomDirectory();
                }
            }
            
            return dirInfo;
        }

        private bool DirectoryInfoRegistered(string roomFolder)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(roomFolder);
            
            bool registeredDirInfo = false;
            if (dirInfo != null)
            {
                foreach (DirectoryInfo info in directoryInfoList)
                {
                    if (info.FullName.Equals(roomFolder))
                    {
                        registeredDirInfo = true;
                        break;
                    }
                }
            }

            return registeredDirInfo;
        }

        private bool CreateRoomFolder(out DirectoryInfo dirInfo)
        {
            if (!Directory.Exists(RoomFolder))
            {
                dirInfo = Directory.CreateDirectory(RoomFolder);
                TagTangoRoomDirectory();
#if UNITY_EDITOR
                // Refresh the asset database to show the new folder
                UnityEditor.AssetDatabase.Refresh();
#endif
                RegisterDirectory(dirInfo);

                return true;
            }
            else
            {
                dirInfo = null;
                return false;
            }
        }

        private void RegisterDirectory(DirectoryInfo dirInfo)
        {
            if(dirInfo != null)
            {
                if (!directoryInfoList.Contains(dirInfo))
                {
                    directoryInfoList.Add(dirInfo);
                    directoryPathList.Add(dirInfo.FullName);
                    roomNameList.Add(dirInfo.Name);
                }
            }
        }

        private bool UnregisterRoomFolder(string roomFolder)
        {
            bool unregistered = false;

            DirectoryInfo dirInfo = null;
            foreach(DirectoryInfo info in directoryInfoList)
            {
                if (info.FullName.Equals(roomFolder))
                {
                    dirInfo = info;
                    break;
                }
            }

            if(dirInfo != null)
            {
                string dirName = dirInfo.FullName;
                if (directoryPathList.Contains(dirName))
                {
                    unregistered = directoryPathList.Remove(dirName);
                }
            }

            return unregistered;
        }

        private void TagTangoRoomDirectory()
        {
            string tagPath = Path.Combine(RoomFolder, Config.Current.Room.TagFilename);
            File.WriteAllText(tagPath, TangoTag);
        }

        private bool IsTangoRoom(GameObject go)
        {
            if(go.tag == "Room")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool RegisterAsTangoRoom(GameObject g)
        {
            if (IsTangoRoom(g)
                && !roomList.Contains(g))
            {
                roomList.Add(g);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool RegisterAllTangoRooms()
        {
            bool newRoomsRegistered = false;

            foreach(GameObject go in GameObject.FindObjectsOfType<GameObject>())
            {
                newRoomsRegistered = newRoomsRegistered || RegisterAsTangoRoom(go);
            }

            return newRoomsRegistered;
        }

        private bool CullLists()
        {
            List<GameObject> currentAvailableRoomsList = new List<GameObject>();

            foreach(GameObject go in GameObject.FindObjectsOfType<GameObject>())
            {
                if (IsTangoRoom(go))
                {
                    currentAvailableRoomsList.Add(go);
                }
            }

            // roomList
            // directoryInfoList
            // directoryPathList
            // RoomNameList
            // FilesToLoad

            List<GameObject> roomsToRemove = new List<GameObject>();
            foreach(GameObject room in roomList)
            {
                bool roomFound = false;
                foreach(GameObject currentRoom in currentAvailableRoomsList)
                {
                    if (currentRoom.Equals(room))
                    {
                        roomFound = true;
                        break;
                    }
                }

                if (!roomFound)
                {
                    roomsToRemove.Add(room);
                }
            }

            foreach(GameObject roomToRemove in roomsToRemove)
            {
                roomList.Remove(roomToRemove);
                if(roomToRemove != null)
                {
                    roomNameList.Remove(roomToRemove.name);
                }
            }

            return roomsToRemove.Count > 0;
        }
        
        private bool IsTangoRoomDirectory(DirectoryInfo dirInfo)
        {
            string tagPath = Path.Combine(dirInfo.FullName, Config.Current.Room.TagFilename);
            if (File.Exists(tagPath))
            {
                string tag = File.ReadAllText(tagPath);

                if (tag.Equals(TangoTag))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Toolbar Declaration and window creation
        /// </summary>
        [MenuItem("ASL/Room Scanning/Room Manager Window")]
        private static void showEditor()
        {
            GetWindow<RoomSave>(false, "Room Manager");
        }

        /// <summary>
        /// Reads the room game object from the file path and sends it to the TangoDatabase with it's name
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="name"></param>
        private void ReadRoom(string filePath, string name)
        {
            byte[] b = File.ReadAllBytes(filePath);
            TangoDatabase.UpdateMesh(b, name);
        }

        /// <summary>
        /// Deletes all Room gameobjects
        /// </summary>
        private void deleteAllRooms()
        {
            foreach(GameObject g in roomList)
            {
                Destroy(g);
            }
        }
#endregion
#endregion
        }
#endif
}
