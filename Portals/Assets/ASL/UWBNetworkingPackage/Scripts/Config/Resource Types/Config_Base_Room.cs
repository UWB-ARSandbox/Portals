using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UWBNetworkingPackage
{
    public class Config_Base_Room : Config_Base_ResourceType
    {
        public static void Start()
        {
            SetFolders();
        }

        public static void SetFolders()
        {
            Directory.CreateDirectory(CompileAbsoluteAssetDirectory());
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            UWB_Texturing.Config_Base.AbsoluteAssetRootFolder = AbsoluteAssetRootFolder;
            UWB_Texturing.Config_Base.AssetSubFolder = AssetSubFolder;
        }

        private static string assetSubFolder = Config_Base_ResourceType.AssetSubFolder + "/Rooms";
        public static new string AssetSubFolder
        {
            get
            {
                return assetSubFolder;
            }
            set
            {
#if UNITY_WSA_10_0
#else
                assetSubFolder = value;
                UWB_Texturing.Config_Base.AssetSubFolder = assetSubFolder;
#endif
            }
        }

        private static string tagFilename = "ScannerPlatform";
        private static string tagFileExtension = ".txt";
        public static string TagFilename
        {
            get
            {
                return tagFilename + tagFileExtension;
            }
            set
            {
#if UNITY_WSA_10_0
#elif UNITY_ANDROID
#else
                string[] components = value.Split('.');
                tagFilename = components[0];
                if(components.Length > 1)
                {
                    tagFileExtension = components[1];
                }
#endif
            }
        }

        private static string tangoFileExtension = ".tngrm";
        public static string TangoFileExtension
        {
            get
            {
                return tangoFileExtension;
            }
            set
            {
                if (value.Contains("."))
                {
                    tangoFileExtension = value;
                }
                else if (value.Length < 7)
                {
                    tangoFileExtension = "." + value;
                }
                else
                {
                    Debug.LogError("Tango file extension too long. Please shorten to less than 7 characters.");
                }
            }
        }

        public static new string CompileAbsoluteAssetDirectory()
        {
            //string roomName = RoomManager.GetAllRoomNames()[0];
            //Debug.Error("Defaulting to room " + roomName);
            //return CompileAbsoluteAssetDirectory(roomName);
            return Path.Combine(AbsoluteAssetRootFolder, AssetSubFolder);
        }

        public static string CompileAbsoluteAssetDirectory(string roomName)
        {
            return Path.Combine(AbsoluteAssetRootFolder, AssetSubFolder) + '/' + roomName;
        }
        
        public static new string CompileAbsoluteAssetPath(string filename)
        {
            //string roomName = RoomManager.GetAllRoomNames()[0];
            //Debug.Error("Defaulting to room " + roomName);
            //return CompileAbsoluteAssetPath(roomName, filename);
            return Path.Combine(CompileAbsoluteAssetDirectory(), filename);
        }

        public static string CompileAbsoluteAssetPath(string roomName, string filename)
        {
            return Path.Combine(CompileAbsoluteAssetDirectory(roomName), filename);
        }

        public static new string CompileUnityAssetDirectory()
        {
            //string roomName = RoomManager.GetAllRoomNames()[0];
            //Debug.Error("Defaulting to room " + roomName);

            //return CompileUnityAssetDirectory(roomName);

            return "Assets/" + AssetSubFolder;
        }

        public static string CompileUnityAssetDirectory(string roomName)
        {
            return "Assets/" + AssetSubFolder + '/' + roomName;
        }

        public static new string CompileUnityAssetPath(string filename)
        {
            //string roomName = RoomManager.GetAllRoomNames()[0];
            //Debug.Error("Defaulting to room " + roomName);

            //return CompileUnityAssetPath(roomName, filename);

            return CompileUnityAssetDirectory() + '/' + filename;
        }

        public static string CompileUnityAssetPath(string roomName, string filename)
        {
            return CompileUnityAssetDirectory(roomName) + '/' + filename;
        }
        
        public static new string CompileResourcesLoadPath(string assetNameWithoutExtension)
        {
            string roomName = RoomManager.GetAllRoomNames()[0];
            Debug.LogError("Defaulting to room " + roomName);

            return CompileResourcesLoadPath(roomName, assetNameWithoutExtension);
        }

        public static new string CompileResourcesLoadPath(string roomName, string assetNameWithoutExtension)
        {
            string directory = CompileUnityAssetDirectory(roomName);
            
            return directory.Substring(directory.IndexOf("Resources") + "Resources".Length + 1) + '/' + assetNameWithoutExtension;
        }
    }
}