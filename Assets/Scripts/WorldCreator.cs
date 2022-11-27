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


    [Range(0.001f, 2f)] public float pointSizeMultiplier = 0.75f;
    public bool dynamicPointSize = true;

    private const int MAX_MATERIAL_COMPRESSION = 101;
    [Range(1, MAX_MATERIAL_COMPRESSION)] public int materialCompression = 20;

    const float MIN_SPACING = 0.5f;
    const float MAX_SPACING = 2f;

    [Range(MIN_SPACING, MAX_SPACING)] public float horzSpacing = 0.5f;
    [Range(MIN_SPACING, MAX_SPACING)] public float vertSpacing = 0.5f;
    [Range(0.1f, 2f)] public float spacingMultiplier = 1f;


    public bool showLand = true;
    public bool showWater = false;

    public Color[] waterColors;
    private Color[] _compressedWaterColors;
    [Range(0, 1)] public float waterCutoff = 0.3f;

    [Range(0, 5)] public int colorAverageOffset = 0;



    public MeshType meshType;

    public Material mapMaterial;
    public Material pointMaterial;


    private GameObject _mapPointsContainer;
    private GameObject _mapGraphic;

    private Transform[] _pointsTransforms = new Transform[0];
    private MeshFilter[] _pointsMeshFilters = new MeshFilter[0];
    private MeshRenderer[] _pointsMeshRenderers = new MeshRenderer[0];
    private Vector2[] _pointsPositions = new Vector2[0];



    private float _lastWidth;
    private float _lastHeight;
    private float _lastPointSizeMultiplier;
    private bool _lastDynamicPointSize;
    private bool _lastShowLand;
    private bool _lastShowWater;
    private int _lastColorAverageOffset;
    private Color[] _lastWaterColors;
    private float _lastWaterCutoff;
    private int _lastMaterialCompression;
    private float _lastHorzSpacing;
    private float _lastVertSpacing;
    private float _lastSpacingMultiplier;
    private MeshType _lastMeshType;

    private int _lastRows;
    private int _lastColumns;

    private static GameObject _quadPrimitive;
    private static GameObject _cubePrimitive;
    private static GameObject _spherePrimitive;
    private static Mesh _quadMesh;
    private static Mesh _cubeMesh;
    private static Mesh _sphereMesh;


    private float _rowSpacing = 0f;
    private float _columnSpacing = 0f;


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
                _pointsMeshFilters == null || _pointsMeshFilters.Length == 0 ||
                _pointsMeshRenderers == null || _pointsMeshRenderers.Length == 0 ||
                _pointsTransforms == null || _pointsTransforms.Length == 0 ||
                _pointsPositions == null || _pointsPositions.Length == 0 ||

                _pointsMeshFilters.Length != _pointsMeshRenderers.Length ||
                _pointsPositions.Length != _pointsMeshRenderers.Length ||
                _pointsTransforms.Length != _pointsMeshRenderers.Length;

            bool softRedraw = hardRedraw ||
                width != _lastWidth ||
                height != _lastHeight ||
                horzSpacing != _lastHorzSpacing ||
                vertSpacing != _lastVertSpacing ||
                spacingMultiplier != _lastSpacingMultiplier;


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
                pointSizeMultiplier != _lastPointSizeMultiplier ||
                dynamicPointSize != _lastDynamicPointSize ||
                showLand != _lastShowLand ||
                colorAverageOffset != _lastColorAverageOffset ||
                showWater != _lastShowWater ||
                waterColors != _lastWaterColors ||
                waterCutoff != _lastWaterCutoff ||
                materialCompression != _lastMaterialCompression ||
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
                int rows = Mathf.RoundToInt(height / (vertSpacing * spacingMultiplier)) + 1;
                int columns = Mathf.RoundToInt(width / (horzSpacing * spacingMultiplier)) + 1;

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
                    _pointsPositions = new Vector2[rows * columns];
                    int index = 0;

                    if (rows < 2) { rows = 2; }
                    if (columns < 2) { columns = 2; }

                    _columnSpacing = width / (columns - 1);
                    _rowSpacing = height / (rows - 1);

                    // create points 
                    for (int i = 0; i < rows; i++)
                    {
                        GameObject r = new GameObject($"Row {i}");
                        r.transform.SetParent(_mapPointsContainer.transform);
                        r.transform.localPosition = new Vector3(0, (_rowSpacing * i), 0);
                        r.transform.localEulerAngles = Vector3.zero;
                        r.transform.localScale = Vector3.one;
                        for (int j = 0; j < columns; j++)
                        {
                            GameObject pt = new GameObject($"Point {i}:{j}");
                            pt.transform.SetParent(r.transform);
                            pt.transform.localPosition = new Vector3(j * _columnSpacing, 0f, 0f);
                            pt.transform.localEulerAngles = Vector3.zero;
                            MeshFilter mf = pt.AddComponent<MeshFilter>();
                            // mf.sharedMesh = GetMeshType(meshType);
                            MeshRenderer mr = pt.AddComponent<MeshRenderer>();
                            // mr.sharedMaterial = pointMaterial;
                            _pointsTransforms[index] = pt.transform;
                            _pointsMeshFilters[index] = mf;
                            _pointsMeshRenderers[index] = mr;
                            _pointsPositions[index] = new Vector2((float)j / (columns - 1), (float)i / (rows - 1));
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
                _lastSpacingMultiplier = spacingMultiplier;
                _lastRows = rows;
                _lastColumns = columns;

                // done, update points 
                goto case ResetType.AppearanceOnly;

            case ResetType.AppearanceOnly:

                bool newMap = false;

                // update map 
                if (_mapGraphic == null)
                {
                    newMap = true;
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
                if (forceRedraw || resetType == ResetType.Full ||
                    _lastPointSizeMultiplier != pointSizeMultiplier ||
                    _lastDynamicPointSize != dynamicPointSize)
                {
                    float horz = pointSizeMultiplier;
                    float vert = pointSizeMultiplier;

                    if (dynamicPointSize)
                    {
                        horz *= _columnSpacing;
                        vert *= _rowSpacing;
                    }

                    Vector3 size = new Vector3(horz, vert, Mathf.Min(horz, vert));
                    foreach (Transform t in _pointsTransforms)
                    {
                        t.localScale = size;
                    }
                }

                if (forceRedraw || newMap || resetType == ResetType.Full ||
                    showLand != _lastShowLand || showWater != _lastShowWater ||
                    colorAverageOffset != _lastColorAverageOffset ||
                    waterColors != _lastWaterColors || waterCutoff != _lastWaterCutoff ||
                    _lastMeshType != meshType || materialCompression != _lastMaterialCompression)
                {
                    bool visible = meshType != MeshType.None;
                    Texture2D texture = (Texture2D)mapMaterial.mainTexture;
                    _compressedWaterColors = new Color[waterColors.Length];
                    for (int i = 0; i < _compressedWaterColors.Length; i++)
                    {
                        _compressedWaterColors[i] = CompressColor(waterColors[i]);
                    }
                    for (int i = 0; i < _pointsMeshFilters.Length; i++)
                    {
                        if (visible)
                        {
                            // yep, visible quad type, determine if surface type is visible 
                            Color color = TestPixel(texture, _pointsPositions[i].x, _pointsPositions[i].y, out bool isWater);
                            if (isWater ? showWater : showLand)
                            {
                                // yep, visible point 
                                _pointsMeshFilters[i].sharedMesh = GetMeshType(meshType, color);
                                _pointsMeshRenderers[i].sharedMaterial = GetMaterialByColor(color);
                                _pointsTransforms[i].gameObject.name += isWater ? " W" : " L";
                                _pointsMeshRenderers[i].enabled = true;
                                continue;
                            }
                        }
                        // not visible
                        _pointsMeshFilters[i].sharedMesh = null;
                        _pointsMeshRenderers[i].sharedMaterial = null;
                        _pointsMeshRenderers[i].enabled = false;
                    }
                }

                // reset values 
                _lastPointSizeMultiplier = pointSizeMultiplier;
                _lastDynamicPointSize = dynamicPointSize;
                _lastShowLand = showLand;
                _lastColorAverageOffset = colorAverageOffset;
                _lastShowWater = showWater;
                _lastWaterColors = waterColors;
                _lastWaterCutoff = waterCutoff;
                _lastMaterialCompression = materialCompression;
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

    private Dictionary<Color, Material> _pointMaterialsByColor = new Dictionary<Color, Material>();
    private Material GetMaterialByColor(Color color)
    {
        if (_pointMaterialsByColor == null) { _pointMaterialsByColor = new Dictionary<Color, Material>(); }
        if (_pointMaterialsByColor.ContainsKey(color))
        {
            return _pointMaterialsByColor[color];
        }
        else
        {
            Material mat = Instantiate(pointMaterial);
            mat.color = color;
            _pointMaterialsByColor.Add(color, mat);
            return mat;
        }
    }

    private Mesh GetMeshType(MeshType type, Color color)
    {
        if (type == MeshType.None) { return null; }

        if (_meshDicts.ContainsKey(type))
        {
            if (_meshDicts[type].ContainsKey(color))
            {
                return _meshDicts[type][color];
            }
            else
            {
                Mesh mesh;
                switch (type)
                {
                    case MeshType.Quad:
                        mesh = Instantiate(_quadMesh);
                        break;
                    case MeshType.Cube:
                        mesh = Instantiate(_cubeMesh);
                        break;
                    case MeshType.Sphere:
                        mesh = Instantiate(_sphereMesh);
                        break;
                    default:
                        Debug.LogError($"ERROR: invalid mesh type {type}, cannot instantiate mesh");
                        return null;
                }
                Color[] cs = mesh.colors;
                if (cs.Length == 0 || cs[0] != color || cs[cs.Length - 1] != color)
                {
                    if (cs.Length == 0)
                    {
                        cs = new Color[mesh.vertices.Length];
                    }
                    for (int j = 0; j < cs.Length; j++)
                    {
                        cs[j] = color;
                    }
                }
                mesh.SetColors(cs);
                _meshDicts[type].Add(color, mesh);
                return mesh;
            }
        }
        else
        {
            _meshDicts.Add(type, new Dictionary<Color, Mesh>());
            return GetMeshType(type, color);
        }
    }

    private Dictionary<MeshType, Dictionary<Color, Mesh>> _meshDicts = new Dictionary<MeshType, Dictionary<Color, Mesh>>();

    void CheckPrimitives(bool forceReset = false)
    {
        if (forceReset)
        {
            _meshDicts.Clear();
            if (_quadPrimitive != null) { DestroyGivenObject(_quadPrimitive); }
            if (_cubePrimitive != null) { DestroyGivenObject(_cubePrimitive); }
            if (_spherePrimitive != null) { DestroyGivenObject(_spherePrimitive); }
        }
        Color[] c;
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
            _quadMesh = Instantiate(_quadPrimitive.GetComponent<MeshFilter>().sharedMesh);
            c = new Color[_quadMesh.vertices.Length];
            for (int i = 0; i < c.Length; i++) { c[i] = Color.white; }
            _quadMesh.SetColors(c);
            _quadPrimitive.GetComponent<MeshFilter>().mesh = _quadMesh;
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
            _cubeMesh = Instantiate(_cubePrimitive.GetComponent<MeshFilter>().sharedMesh);
            c = new Color[_cubeMesh.vertices.Length];
            for (int i = 0; i < c.Length; i++) { c[i] = Color.white; }
            _cubeMesh.SetColors(c);
            _cubePrimitive.GetComponent<MeshFilter>().mesh = _cubeMesh;
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
            _sphereMesh = Instantiate(_spherePrimitive.GetComponent<MeshFilter>().sharedMesh);
            c = new Color[_sphereMesh.vertices.Length];
            for (int i = 0; i < c.Length; i++) { c[i] = Color.white; }
            _sphereMesh.SetColors(c);
            _spherePrimitive.GetComponent<MeshFilter>().mesh = _sphereMesh;
        }
    }

    private Color CompressColor(Color color)
    {
        if (materialCompression > 0 && materialCompression < MAX_MATERIAL_COMPRESSION)
        {
            color.r = Mathf.Round(color.r * materialCompression) / materialCompression;
            color.g = Mathf.Round(color.g * materialCompression) / materialCompression;
            color.b = Mathf.Round(color.b * materialCompression) / materialCompression;
        }
        return color;
    }

    private Color TestPixel(Texture2D texture, float widthPercent, float heightPercent, out bool isWater)
    {
        int x = Mathf.FloorToInt(texture.width * Mathf.Clamp01(widthPercent));
        int y = Mathf.FloorToInt(texture.height * Mathf.Clamp01(heightPercent));
        Color color = texture.GetPixel(x, y);
        if (colorAverageOffset > 0)
        {
            List<Color> colors = new List<Color>();
            colors.Add(color);
            colors.Add(texture.GetPixel(x - colorAverageOffset, y - colorAverageOffset));
            colors.Add(texture.GetPixel(x - colorAverageOffset, y + colorAverageOffset));
            colors.Add(texture.GetPixel(x + colorAverageOffset, y - colorAverageOffset));
            colors.Add(texture.GetPixel(x + colorAverageOffset, y + colorAverageOffset));
            color = ColorComparator.AverageColor(colors.ToArray());
        }
        return GetTestedColor(color, out isWater);
    }
    private Color GetTestedColor(Color color, out bool isWater)
    {
        // limit RGB channels 
        CompressColor(color);
        // determine if water color 
        if (_waterColorsList.Contains(color))
        {
            isWater = true;
        }
        else
        {
            isWater = false;
            if (!_landColorsList.Contains(color))
            {
                // new color, determine if water or land 
                for (int i = 0; i < _compressedWaterColors.Length; i++)
                {
                    float colorDiff = ColorComparator.GetColorDifference(color, _compressedWaterColors[i]);

                    if (colorDiff <= waterCutoff)
                    {
                        isWater = true;
                        break;
                    }
                }
                if (isWater)
                {
                    _waterColorsList.Add(color);
                }
                else
                {
                    _landColorsList.Add(color);
                }
            }
        }
        return color;
    }

    private List<Color> _waterColorsList = new List<Color>();
    private List<Color> _landColorsList = new List<Color>();

    void ClearMap()
    {
        _waterColorsList.Clear();
        _landColorsList.Clear();
        _pointMaterialsByColor.Clear();
        _meshDicts.Clear();
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