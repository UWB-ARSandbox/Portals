using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using GameDevWare.Serialization;
using Tango;
using UnityEngine;

namespace UWBNetworkingPackage
{
    public class ReceivingClientLauncher_Tango : ReceivingClientLauncher_PC
    {
        /// <summary>
        /// Updates the mesh based on the current dynamic mesh and marker location
        /// Sends a RPC call for the MasterClient to receive
        /// </summary>
        [PunRPC]
        public override void SendTangoMesh()
        {
            UpdateMesh();

            photonView.RPC("ReceiveTangoMesh", PhotonTargets.MasterClient, PhotonNetwork.player.ID, TangoDatabase.GetMeshAsBytes().Length);
        }

        /// <summary>
        /// Gets all information from the Tango Dynamic Mesh and modifies it based on marker information.
        /// </summary>
        private void UpdateMesh()
        {
            //create lists and populate them with dynamic mesh info
            var tangoApplication =
                GameObject.Find("Tango Manager")
                    .GetComponent<TangoApplication>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Color32> colors = new List<Color32>();
            List<int> triangles = new List<int>();
            tangoApplication.Tango3DRExtractWholeMesh(vertices, normals, colors,
                triangles);

            //get current marker tranform information and apply it to every vert
            Vector3 V;
            Quaternion Q;
            Transform T = GameObject.Find("Dynamic_GameObjects").transform;
            V = T.transform.position;
            Q = T.transform.rotation;

            float angle;
            Vector3 axis;
            Q.ToAngleAxis(out angle, out axis);
            Q = Quaternion.AngleAxis(-angle, axis);

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] -= V;
                vertices[i] = Q * vertices[i]; //inverse Q
            }

            //write the info to the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.colors32 = colors.ToArray();
            mesh.triangles = triangles.ToArray();
            List<Mesh> meshList = new List<Mesh>();
            meshList.Add(mesh);

            //update mesh with info
            TangoDatabase.UpdateMesh(meshList);
            Debug.Log("Mesh Updated");
        }
    }
}