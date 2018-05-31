using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UWBNetworkingPackage
{
    public class TangoRoom : MonoBehaviour
    {
        /// <summary>
        /// When a TangoRoom is delete, remove it from the list in TangoDatabase.cs
        /// </summary>
        private void OnDestroy()
        {
            TangoDatabase.TangoRoom T = TangoDatabase.GetRoomByName(this.gameObject.name);

            TangoDatabase.DeleteRoom(T);
        }
    }
}