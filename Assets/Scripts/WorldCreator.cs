using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldCreator : MonoBehaviour
{

    public enum ResetType
    {
        None,
        AppearanceOnly,
        Full,
    }

    public enum MeshType
    {
        Cube,
        Sphere,
        Quad,
        None,
    }

    private static bool _updating = false;

    [Delayed] public float width = 20f;
    [Delayed] public float height = 10f;

    [Range(0.01f, 0.5f)] public float pointSize = 0.1f;

    [Range(0.25f, 2f)] public float horzSpacing = 0.5f;
    [Range(0.25f, 2f)] public float vertSpacing = 0.5f;

    public MeshType meshType;

    public Material mapMaterial;
    public Material pointMaterial;


    private GameObject _mapPointsContainer;
    private Transform[] _pointsTransforms;
    private MeshFilter[] _pointsMeshFilters;
    private MeshRenderer[] _pointsMeshRenderers;
    private GameObject _mapGraphic;



    private float _lastWidth;
    private float _lastHeight;
    private float _lastPointSize;
    private float _lastHorzSpacing;
    private float _lastVertSpacing;
    private MeshType _lastMeshType;

    private int _lastRows;
    private int _lastColumns;

    private static GameObject _quadPrimitive;
    private static GameObject _cubePrimitive;
    private static GameObject _spherePrimitive;
    private Mesh _quadMesh;
    private Mesh _cubeMesh;
    private Mesh _sphereMesh;


    private bool _initialized = false;


    [SerializeField] private bool forceRedraw = false;


    [ExecuteInEditMode]
    void Start()
    {
        GenerateMap();
    }

    void OnInspectorGUI()
    {
        GenerateMap();
    }
    void OnEnable()
    {
        GenerateMap();
    }

    [ExecuteInEditMode]
    void Update()
    {
        if (forceRedraw)
        {
            GenerateMap();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += GenerateMap;
    }
#endif


    void GenerateMap()
    {
        // Debug.Log("Generating map...");

        if (_updating) { return; }
        _updating = true;

        ResetType resetType = ResetType.None;

        if (!_initialized || forceRedraw)
        {
            resetType = ResetType.Full;
            _initialized = true;
            forceRedraw = true;
        }
        else
        {
            bool hardRedraw =
                _mapPointsContainer == null ||
                transform.childCount != 2 ||
                _pointsMeshFilters.Length == 0 ||
                _pointsMeshRenderers.Length == 0 ||
                _pointsTransforms.Length == 0 ||
                _pointsMeshFilters.Length != _pointsMeshRenderers.Length ||
                _pointsTransforms.Length != _pointsMeshRenderers.Length;

            bool softRedraw = hardRedraw ||
                width != _lastWidth ||
                height != _lastHeight ||
                horzSpacing != _lastHorzSpacing ||
                vertSpacing != _lastVertSpacing;


            if (softRedraw || hardRedraw)
            {
                // check for parenting issue 
                if (hardRedraw)
                {
                    forceRedraw = true;// gotta recreate the hierarchy, force redraw 
                }
                // scale changed
                resetType = ResetType.Full;
            }
            else if (
                pointSize != _lastPointSize ||
                meshType != _lastMeshType)
            {
                // just appearance changed, point size or source image 
                resetType = ResetType.AppearanceOnly;
            }
        }
        
        CheckPrimitives(forceRedraw);

        switch (resetType)
        {
            case ResetType.Full:

                // determine rows and columns
                int rows = Mathf.RoundToInt(height / vertSpacing) + 1;
                int columns = Mathf.RoundToInt(width / horzSpacing) + 1;

                // determine if recalculation is ACTUALLY needed 

                if (forceRedraw ||
                    width != _lastWidth ||
                    height != _lastHeight ||
                    rows != _lastRows ||
                    columns != _lastColumns)
                {

                    ClearMap();

                    // create container 
                    _mapPointsContainer = new GameObject("Map Points Container");
                    _mapPointsContainer.transform.SetParent(transform);
                    _mapPointsContainer.transform.localPosition = new Vector3(
                        width * -0.5f, height * -0.5f, 0);
                    _mapPointsContainer.transform.localEulerAngles = Vector3.zero;
                    _mapPointsContainer.transform.localScale = Vector3.one;

                    _pointsTransforms = new Transform[rows * columns];
                    _pointsMeshFilters = new MeshFilter[rows * columns];
                    _pointsMeshRenderers = new MeshRenderer[rows * columns];
                    int index = 0;

                    if (rows < 2) { rows = 2; }
                    if (columns < 2) { columns = 2; }

                    float columnSpacing = width / (columns - 1);
                    float rowSpacing = height / (rows - 1);

                    // create points 
                    for (int i = 0; i < rows; i++)
                    {
                        GameObject r = new GameObject($"Row {i}");
                        r.transform.SetParent(_mapPointsContainer.transform);
                        r.transform.localPosition = new Vector3(0, (rowSpacing * i), 0);
                        r.transform.localEulerAngles = Vector3.zero;
                        r.transform.localScale = Vector3.one;
                        for (int j = 0; j < columns; j++)
                        {
                            GameObject pt = new GameObject($"Point {i}:{j}");
                            pt.transform.SetParent(r.transform);
                            pt.transform.localPosition = new Vector3(j * columnSpacing, 0f, 0f);
                            pt.transform.localEulerAngles = Vector3.zero;
                            MeshFilter mf = pt.AddComponent<MeshFilter>();
                            mf.sharedMesh = GetMeshType(meshType);
                            MeshRenderer mr = pt.AddComponent<MeshRenderer>();
                            mr.sharedMaterial = pointMaterial;
                            _pointsTransforms[index] = pt.transform;
                            _pointsMeshFilters[index] = mf;
                            _pointsMeshRenderers[index] = mr;
                            mr.enabled = meshType != MeshType.None;
                            index++;
                        }
                    }
                }

                // reset values 
                _lastWidth = width;
                _lastHeight = height;
                _lastHorzSpacing = horzSpacing;
                _lastVertSpacing = vertSpacing;
                _lastRows = rows;
                _lastColumns = columns;

                // done, update points 
                goto case ResetType.AppearanceOnly;

            case ResetType.AppearanceOnly:

                // update map 
                if (_mapGraphic == null)
                {
                    _mapGraphic = GameObject.Instantiate(_quadPrimitive);
                    _mapGraphic.SetActive(true);
                    _mapGraphic.name = "Map Graphic";
                    _mapGraphic.transform.SetParent(transform);
                    _mapGraphic.transform.localPosition = Vector3.zero;
                    _mapGraphic.transform.localEulerAngles = Vector3.zero;
                    MeshRenderer _mapMeshRenderer = _mapGraphic.GetComponent<MeshRenderer>();
                    _mapMeshRenderer.sharedMaterial = mapMaterial;
                }
                _mapGraphic.transform.localScale = new Vector3(width, height, 1);

                // get all meshrenderers in map points container 
                if (resetType == ResetType.Full || _lastPointSize != pointSize)
                {
                    Vector3 size = Vector3.one * pointSize;
                    foreach (Transform t in _pointsTransforms)
                    {
                        t.localScale = size;
                    }
                }

                if (resetType == ResetType.Full || _lastMeshType != meshType)
                {
                    Mesh mesh = GetMeshType(meshType);
                    bool visible = meshType != MeshType.None;
                    for (int i = 0; i < _pointsMeshFilters.Length; i++)
                    {
                        _pointsMeshFilters[i].sharedMesh = mesh;
                        _pointsMeshRenderers[i].enabled = visible;
                    }
                }

                // reset values 
                _lastPointSize = pointSize;
                _lastMeshType = meshType;
                break;

            case ResetType.None:
                // do nothing 
                break;

            default:
                // invalid type 
                Debug.LogError($"ERROR: invalid reset type {resetType}, returning");
                break;
        }

        forceRedraw = false;
        _updating = false;

    }

    private Mesh GetMeshType(MeshType type)
    {
        switch (type)
        {
            case MeshType.Quad:
                return _quadMesh;
            case MeshType.Cube:
                return _cubeMesh;
            case MeshType.Sphere:
                return _sphereMesh;
            case MeshType.None:
                return null;
            default:
                return null;
        }
    }

    void CheckPrimitives(bool forceReset = false)
    {
        if (forceReset)
        {
            if (_quadPrimitive != null) { DestroyGivenObject(_quadPrimitive); }
            if (_cubePrimitive != null) { DestroyGivenObject(_cubePrimitive); }
            if (_spherePrimitive != null) { DestroyGivenObject(_spherePrimitive); }
        }
        if (_quadPrimitive == null)
        {
            _quadPrimitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
            if (_quadPrimitive.TryGetComponent(out Collider collider)) { DestroyGivenObject(collider); }
            _quadPrimitive.name = "Quad Primitive";
            _quadPrimitive.hideFlags = HideFlags.HideInHierarchy;
            _quadPrimitive.transform.position = Vector3.zero;
            _quadPrimitive.transform.eulerAngles = Vector3.zero;
            _quadPrimitive.transform.localScale = Vector3.one;
            _quadPrimitive.SetActive(false);
            _quadMesh = _quadPrimitive.GetComponent<MeshFilter>().sharedMesh;
        }
        if (_cubePrimitive == null)
        {
            _cubePrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (_cubePrimitive.TryGetComponent(out Collider collider)) { DestroyGivenObject(collider); }
            _cubePrimitive.name = "Cube Primitive";
            _cubePrimitive.hideFlags = HideFlags.HideInHierarchy;
            _cubePrimitive.transform.position = Vector3.zero;
            _cubePrimitive.transform.eulerAngles = Vector3.zero;
            _cubePrimitive.transform.localScale = Vector3.one;
            _cubePrimitive.SetActive(false);
            _cubeMesh = _cubePrimitive.GetComponent<MeshFilter>().sharedMesh;
        }
        if (_spherePrimitive == null)
        {
            _spherePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (_spherePrimitive.TryGetComponent(out Collider collider)) { DestroyGivenObject(collider); }
            _spherePrimitive.name = "Sphere Primitive";
            _spherePrimitive.hideFlags = HideFlags.HideInHierarchy;
            _spherePrimitive.transform.position = Vector3.zero;
            _spherePrimitive.transform.eulerAngles = Vector3.zero;
            _spherePrimitive.transform.localScale = Vector3.one;
            _spherePrimitive.SetActive(false);
            _sphereMesh = _spherePrimitive.GetComponent<MeshFilter>().sharedMesh;
        }
    }

    void ClearMap()
    {
        for (int i = this.transform.childCount; i > 0; --i)
        {
            DestroyGivenObject(this.transform.GetChild(0).gameObject);
        }
    }
    void DestroyGivenObject(Object obj)
    {
        if (Application.isPlaying)
        {
            Destroy(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }
    }

}