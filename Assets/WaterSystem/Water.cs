using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Water : MonoBehaviour
{
    [Range(1, 200)]
    public int width, height;
    [Range(1, 10)]
    public int cellSize = 1;

    public float waterDensity = 1000;

    public Material waterMaterial;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh waterMesh;

    float sizexor;
    float lCellSize;

    void CreateMesh(int xSize, int ySize) {
        Mesh mesh = new Mesh();

        int arraySize = (xSize + 1) * (ySize + 1);
        Vector3[] vertices = new Vector3[arraySize];
        Vector2[] uv = new Vector2[arraySize];

        for (int y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                int i = y * (xSize + 1) + x;
                vertices[i] = new Vector3(x * cellSize, 0, y * cellSize);
                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
            }
        }

        int[] triangles = new int[xSize * ySize * 6];
        int ti = 0;

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                int i = y * (xSize + 1) + x;

                triangles[ti++] = i;
                triangles[ti++] = i + xSize + 1;
                triangles[ti++] = i + 1;

                triangles[ti++] = i + 1;
                triangles[ti++] = i + xSize + 1;
                triangles[ti++] = i + xSize + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(1, 1, 1, 0);
        }
        mesh.colors = colors;

        meshRenderer.material = waterMaterial;

        meshFilter.mesh = mesh;
        waterMesh = mesh;
    }

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
    }

    void Update()
    {
        
        int rwidth = width - width % cellSize;
        int rheight = height - height % cellSize;

        if (sizexor != (rwidth ^ rheight) || cellSize != lCellSize) {
            CreateMesh(rwidth, rheight);
            sizexor = rwidth ^ rheight;
            lCellSize = cellSize;
        }
    }
}