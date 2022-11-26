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

    private static bool _updating = false;

    public Texture2D mapSourceImage;
    public float width = 20f;
    public float height = 10f;

    [Range(0.01f, 0.5f)] public float pointSize = 0.1f;

    [Range(0.25f, 2f)] public float horzSpacing = 0.5f;
    [Range(0.25f, 2f)] public float vertSpacing = 0.5f;

    public Material mapMaterial;
    public Material pointMaterial;


    private GameObject _mapPointsContainer;
    private Transform[] _pointsTransforms;
    private MeshRenderer[] _pointsMeshRenderer;
    private GameObject _mapGraphic;
    private MeshRenderer _mapMeshRenderer;


    private Texture2D _lastMapSourceImage;
    private Material _lastMapMaterial;
    private Material _lastPointMaterial;

    private float _lastWidth;
    private float _lastHeight;
    private float _lastPointSize;
    private float _lastHorzSpacing;
    private float _lastVertSpacing;

    private int _lastRows;
    private int _lastColumns;

    private static GameObject _quadPrimitive;
    private static GameObject _cubePrimitive;


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

        CheckPrimitives();

        ResetType resetType = ResetType.None;

        if (!_initialized || forceRedraw)
        {
            resetType = ResetType.Full;
            _initialized = true;
            forceRedraw = true;
        }
        else
        {
            if (_mapPointsContainer == null ||
                transform.childCount != 2 ||
                width != _lastWidth ||
                height != _lastHeight ||
                horzSpacing != _lastHorzSpacing ||
                vertSpacing != _lastVertSpacing)
            {
                // scale changed
                resetType = ResetType.Full;
            }
            else if (
                pointSize != _lastPointSize ||
                mapSourceImage != _lastMapSourceImage ||
                mapMaterial != _lastMapMaterial ||
                pointMaterial != _lastPointMaterial)
            {
                // just appearance changed, point size or source image 
                resetType = ResetType.AppearanceOnly;
            }
        }

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
                    _pointsMeshRenderer = new MeshRenderer[rows * columns];
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
                            GameObject pt = GameObject.Instantiate(_cubePrimitive);
                            pt.SetActive(true);
                            pt.name = $"Point {i}:{j}";
                            pt.transform.SetParent(r.transform);
                            pt.transform.localPosition = new Vector3(j * columnSpacing, 0f, 0f);
                            pt.transform.localEulerAngles = Vector3.zero;
                            _pointsTransforms[index] = pt.transform;
                            _pointsMeshRenderer[index] = pt.GetComponent<MeshRenderer>();
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
                }
                _mapGraphic.transform.localScale = new Vector3(width, height, 1);
                _mapMeshRenderer = _mapGraphic.GetComponent<MeshRenderer>();
                Material mat = new Material(mapMaterial);
                mat.mainTexture = mapSourceImage;
                _mapMeshRenderer.sharedMaterial = mat;

                // get all meshrenderers in map points container 
                if (resetType == ResetType.Full || _lastPointSize != pointSize)
                {
                    foreach (Transform t in _pointsTransforms)
                    {
                        t.localScale = new Vector3(pointSize, pointSize, 0.02f);
                    }
                }
                if (resetType == ResetType.Full || _lastPointMaterial != pointMaterial)
                {
                    foreach (MeshRenderer mr in _pointsMeshRenderer)
                    {
                        mr.sharedMaterial = pointMaterial;
                    }
                }

                // reset values 
                _lastMapSourceImage = mapSourceImage;
                _lastPointSize = pointSize;
                _lastMapMaterial = mapMaterial;
                _lastPointMaterial = pointMaterial;
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

    void CheckPrimitives()
    {
        if (_quadPrimitive == null)
        {
            _quadPrimitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _quadPrimitive.name = "Quad Primitive";
            _quadPrimitive.hideFlags = HideFlags.HideInHierarchy;
            _quadPrimitive.transform.position = Vector3.zero;
            _quadPrimitive.transform.eulerAngles = Vector3.zero;
            _quadPrimitive.transform.localScale = Vector3.one;
            _quadPrimitive.SetActive(false);
        }
        if (_cubePrimitive == null)
        {
            _cubePrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _cubePrimitive.name = "Cube Primitive";
            _cubePrimitive.hideFlags = HideFlags.HideInHierarchy;
            _cubePrimitive.transform.position = Vector3.zero;
            _cubePrimitive.transform.eulerAngles = Vector3.zero;
            _cubePrimitive.transform.localScale = Vector3.one;
            _cubePrimitive.SetActive(false);
        }
    }

    void ClearMap()
    {
        for (int i = this.transform.childCount; i > 0; --i)
        {
            if (Application.isPlaying)
            {
                Destroy(this.transform.GetChild(0).gameObject);
            }
            else
            {
                DestroyImmediate(this.transform.GetChild(0).gameObject);
            }
        }
    }

}