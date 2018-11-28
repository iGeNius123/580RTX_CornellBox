using System.Collections.Generic;
using UnityEngine;

public class PT1Custom : MonoBehaviour
{
    //public
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;
    public Light DirectionalLight;
    public Camera _camera;
    public List<GameObject> sphereObjectList;
    public List<GameObject> metalSphereObjectList;
    public List<GameObject> lightSphereObjectList;
    [Header("Spheres")]


    //private
    private float _lastFieldOfView;
    private RenderTexture _target;
    private Material _addMaterial;
    private uint _currentSample = 0;
    private ComputeBuffer _sphereBuffer;
    private List<Transform> _transformsToWatch = new List<Transform>();

    struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    }

    private void Awake()
    {

        _transformsToWatch.Add(transform);
        _transformsToWatch.Add(DirectionalLight.transform);
    }

    private void OnEnable()
    {
        _currentSample = 0;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
    }

    private void Update()
    {
        if (_camera.fieldOfView != _lastFieldOfView)
        {
            _currentSample = 0;
            _lastFieldOfView = _camera.fieldOfView;
        }

        foreach (Transform t in _transformsToWatch)
        {
            if (t.hasChanged)
            {
                _currentSample = 0;
                t.hasChanged = false;
            }
        }
    }

    private void SetUpScene()
    {
        List<Sphere> spheres = new List<Sphere>();
        GameObject[] spArray = sphereObjectList.ToArray();
        // Add non metal spheres
        for (int i = 0; i < spArray.Length ; i++)
        {
            Sphere sphere = new Sphere();
            // Radius
            sphere.radius = spArray[i].GetComponent<SphereCollider>().radius;
            float r = spArray[i].GetComponent<SphereCollider>().radius;
            sphere.position = spArray[i].transform.position + new Vector3(0, r, 0);

            // Albedo and specular color
            Color color = spArray[i].GetComponent<MeshRenderer>().material.color;
            sphere.albedo = new Vector3(color.r,color.g,color.b);
            sphere.specular = new Vector4(0.05f,0.05f,0.05f);

            // Add the sphere to the list
            spheres.Add(sphere);

        }
        //add metal spheres
        spArray = metalSphereObjectList.ToArray();
        for (int i = 0; i < spArray.Length; i++)
        {
            Sphere sphere = new Sphere();
            // Radius
            sphere.radius = spArray[i].GetComponent<SphereCollider>().radius;
            float r = spArray[i].GetComponent<SphereCollider>().radius;
            sphere.position = spArray[i].transform.position + new Vector3(0, r, 0);

            // Albedo and specular color
            Color color = spArray[i].GetComponent<MeshRenderer>().material.color;
            sphere.albedo = new Vector3(color.r, color.g, color.b);
            sphere.specular = new Vector3(color.r, color.g, color.b);

            // Add the sphere to the list
            spheres.Add(sphere);

        }
        //add light source spheres
        spArray = lightSphereObjectList.ToArray();
        for (int i = 0; i < spArray.Length; i++)
        {
            Sphere sphere = new Sphere();
            // Radius
            sphere.radius = spArray[i].GetComponent<SphereCollider>().radius;
            float r = spArray[i].GetComponent<SphereCollider>().radius;
            sphere.position = spArray[i].transform.position + new Vector3(0,r,0);

            // Albedo and specular color
            Color color = spArray[i].GetComponent<MeshRenderer>().material.color;
            sphere.albedo = new Vector3(color.r, color.g, color.b);
            sphere.specular = new Vector4(0.05f, 0.05f, 0.05f);

            // Add the sphere to the list
            spheres.Add(sphere);

        }
        // Assign to compute buffer
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
        if (spheres.Count > 0)
        {
            _sphereBuffer = new ComputeBuffer(spheres.Count, 40);
            _sphereBuffer.SetData(spheres);
        }
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetVector("_PixelOffset1", new Vector2(Random.value, Random.value));
        RayTracingShader.SetVector("_PixelOffset2", new Vector2(Random.value, Random.value));
        RayTracingShader.SetVector("_PixelOffset3", new Vector2(Random.value, Random.value));
        RayTracingShader.SetVector("_PixelOffset4", new Vector2(Random.value, Random.value));
        RayTracingShader.SetVector("_PixelOffset5", new Vector2(Random.value, Random.value));

        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        if (_sphereBuffer != null)
            RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
            {
                _target.Release();
            }

            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();

            // Reset sampling
            _currentSample = 0;
        }
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_target, destination, _addMaterial);
        _currentSample++;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }
}
