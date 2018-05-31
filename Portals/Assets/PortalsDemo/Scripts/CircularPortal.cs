using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL.PortalSystem;

public class CircularPortal : Portal
{
    private const int resolution = 36;
    protected Mesh mMesh;

    void Awake()
    {
        mMesh = GetComponentInChildren<MeshFilter>().mesh;
        mMesh.Clear();

        Vector3[] v = new Vector3[resolution + 1];
        Vector2[] uv = new Vector2[resolution + 1];
        int[] t = new int[3 * resolution];

        // Initialize vertices, uvs, and triangles
        InitVertices(v, uv);
        InitTriangles(t);

        // Set values to mesh for shader to use
        mMesh.vertices = v;
        List<Vector2> uv0 = new List<Vector2>(uv);
        mMesh.SetUVs(0, uv0);
        mMesh.triangles = t;
    }

    // Initialize triangles using vertex index
    // This follows a "Left-To-Right-Counter-Clockwise" pattern
    private void InitTriangles(int[] t)
    {
        int index = 1;
        for (int i = 0; i < t.Length; i += 3, index++)
        {
            t[i] = 0;
            t[i + 1] = index;
            if (index + 1 <= resolution)
                t[i + 2] = index + 1;
            else
                t[i + 2] = 1;
        }
    }

    // Initialize vertices based on resolution
    // Traverse in clockwise order, starting at 0 degrees
    private void InitVertices(Vector3[] v, Vector2[] uv)
    {
        v[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f);
        Vector3 currentVertex = new Vector3(0.5f, 0f, 0f);
        Quaternion increment = Quaternion.AngleAxis(-360 / resolution, Vector3.forward);
        for (int i = 1; i <= resolution; ++i)
        {
            v[i] = new Vector3(currentVertex.x, currentVertex.y, 0f);
            uv[i] = new Vector2(v[i].x + 0.5f, v[i].y - 0.5f);
            currentVertex = increment * currentVertex;
        }
    }
}
