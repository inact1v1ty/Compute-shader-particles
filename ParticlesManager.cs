using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ParticlesManager : MonoBehaviour {
    public struct Particle
    {
        public Vector2 position;
        public Vector2 velocity;
    }

    public ComputeShader computeShader;
    public Material material;

    ComputeBuffer particles;

    const int WARP_SIZE = 1024;

    int size = 1024000;
    int stride;

    int warpCount;

    int kernelIndex;

    Particle[] initBuffer;

    /*private void OnDrawGizmos()
    {
        var p = new Particle[size];
        particles.GetData(p);
        for (int i = 0; i < size; i++)
        {
            Gizmos.DrawSphere(p[i].position, 0.1f);
        }
    }*/

    // Use this for initialization
    void Start () {

        warpCount = Mathf.CeilToInt((float)size / WARP_SIZE);

        stride = Marshal.SizeOf(typeof(Particle));
        particles = new ComputeBuffer(size, stride);

        initBuffer = new Particle[size];

        for (int i = 0; i < size; i++)
        {
            initBuffer[i] = new Particle();
            initBuffer[i].position = Random.insideUnitCircle * 10f;
            initBuffer[i].velocity = Vector2.zero;
        }

        particles.SetData(initBuffer);

        kernelIndex = computeShader.FindKernel("Update");
        
        computeShader.SetBuffer(kernelIndex, "Particles", particles);

        material.SetBuffer("Particles", particles);
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.R))
        {
            particles.SetData(initBuffer);
        }

        computeShader.SetInt("shouldMove", Input.GetMouseButton(0) ? 1 : 0);
        var mousePosition = GetMousePosition();
        computeShader.SetFloats("mousePosition", mousePosition);
        computeShader.SetFloat("dt", Time.deltaTime);
        computeShader.Dispatch(kernelIndex, warpCount, 1, 1);
	}

    float[] GetMousePosition()
    {
        var mp = Input.mousePosition;
        var v = Camera.main.ScreenToWorldPoint(mp);
        return new float[] { v.x, v.y };
    }

    void OnRenderObject()
    {
        material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, 1, size);
    }

    void OnDestroy()
    {
        if (particles != null)
            particles.Release();
    }
}
