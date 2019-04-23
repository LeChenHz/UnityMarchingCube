using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class LoadDLL : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    struct CPPMesh
    {
        public uint VerticesCount;
        public IntPtr Vertices;
        public IntPtr Normals;
    };

    public float threshold = 20000.0f;
    public string FileName;

    [DllImport("MarchingCubeCPP")]
    private static extern void InitMarchingCube(IntPtr inData, ushort x, ushort y, ushort z);

    [DllImport("MarchingCubeCPP")]
    private static extern void MarchingCubeIsosurface(ref CPPMesh mesh, float target_value);

    private volatile bool isInit = false;
    private volatile bool isWorking = false;


    public void Double()
    {
        threshold *= 2.0f;
    }

    public void Half()
    {
        threshold *= 0.5f;
    }

    // Start is called before the first frame update
    void Start()
    {
        TextAsset asset = Resources.Load<TextAsset>(FileName);
        if(asset == null)
        {
            Debug.LogError("File Do not Exists");
            return;
        }
        var allBytes = asset.bytes;
        var data = Marshal.UnsafeAddrOfPinnedArrayElement(allBytes, 0);
        InitMarchingCube(data,200,160,160);
        isInit = true;
        OnValidate();
    }

    public void Reflash() => OnValidate();

    void OnValidate()
    {
        if (isWorking || !isInit)
        {
            return;
        }
        float oldT = threshold;
        isWorking = true;
        CPPMesh mesh = new CPPMesh();
        MarchingCubeIsosurface(ref mesh, oldT);

        int len = (int)(mesh.VerticesCount * 3);
        float[] vertices = new float[len];
        float[] normals = new float[len];
        Marshal.Copy(mesh.Vertices, vertices, 0, len);
        Marshal.Copy(mesh.Normals, normals, 0, len);

        Mesh unityMesh = new Mesh();
        Vector3[] meshV = new Vector3[mesh.VerticesCount];
        Vector3[] meshN = new Vector3[mesh.VerticesCount];
        int[] meshT = new int[mesh.VerticesCount];
        for (int i = 0; i < mesh.VerticesCount; i++) meshT[i] = i;
        for (int i = 0; i < mesh.VerticesCount; i++)
        {
            meshV[i] = new Vector3(vertices[3 * i + 0], vertices[3 * i + 1], vertices[3 * i + 2]);
            meshN[i] = new Vector3(normals[3 * i + 0], normals[3 * i + 1], normals[3 * i + 2]);
        }
        unityMesh.vertices = meshV;
        unityMesh.normals = meshN;
        unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        unityMesh.triangles = meshT;
        gameObject.GetComponent<MeshFilter>().mesh = unityMesh;
        isWorking = false;
        if(oldT != threshold)
        {
            OnValidate();
        }
    }
    private void OnApplicationQuit()
    {
        gameObject.GetComponent<MeshFilter>().mesh = null;
        //Destroy(gameObject.GetComponent<MeshFilter>());
    }
}
