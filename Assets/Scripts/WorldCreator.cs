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


    const float MIN_SPACING = 0.5f;
    const float MAX_SPACING = 2f;

    [Header("Generation Properties")]
    [SerializeField] private bool GenerateOnStartAndAwake = true;
    [SerializeField] private bool GenerateOnValidate = true;
    [SerializeField] private bool GenerateOnEnable = true;
    [SerializeField] private bool ClearOnDisable = true;
    [SerializeField, Space(5)] private bool AllowGeneratedContentSave = false;
    [SerializeField, Space(5)] private bool ForceReGenerate = false;


    [Header("Map and Grid Size")]

    [Delayed] public float width = 20f;
    [Delayed] public float height = 10f;

    [Range(MIN_SPACING, MAX_SPACING)] public float horzSpacing = 0.5f;
    [Range(MIN_SPACING, MAX_SPACING)] public float vertSpacing = 0.5f;
    [Range(0.2f, 2f)] public float spacingUniformMultiplier = 1f;



    [Header("Point Display Properties")]
    [Range(0.001f, 2f)] public float pointSizeMultiplier = 0.75f;
    public bool dynamicPointSize = true;
    public MeshType pointMeshType;


    [Header("Surface Display Options")]
    public bool showLand = true;
    public bool showWater = false;
    public bool showBorder = false;
    public bool generateHiddenSurfaces = false;



    [Header("Color Compression")]
    private const int MAX_COLOR_COMPRESSION = 32;
    public bool useColorCompression = false;
    [Range(0, MAX_COLOR_COMPRESSION)] public int colorCompressionLevel = 20;
    [Range(0, 5)] public int colorAverageOffset = 0;



    [Header("Water Color Detection")]
    [Space(5)] public Color[] waterColors;
    private Color[] _compressedWaterColors;
    [Range(0, 1)] public float waterCutoff = 0.3f;



    [Header("Materials References")]
    public Material mapMaterial;
    public Material pointMaterial;


    private GameObject _mapPointsContainer;
    private GameObject _mapGraphic;

    private bool[] _pointIsVisible = new bool[0];
    private Transform[] _pointsTransforms = new Transform[0];
    private MeshFilter[] _pointsMeshFilters = new MeshFilter[0];
    private MeshRenderer[] _pointsMeshRenderers = new MeshRenderer[0];
    private Vector2[] _pointsPositions = new Vector2[0];
    private Color[] _pointsColor = new Color[0];
    private bool[] _pointIsWater = new bool[0];



    private float _lastWidth;
    private float _lastHeight;
    private bool _lastShowBorder;
    private float _lastPointSizeMultiplier;
    private bool _lastDynamicPointSize;
    private bool _lastShowLand;
    private bool _lastShowWater;
    private bool _lastGenerateHiddenSurfaces = false;
    private int _lastColorAverageOffset;
    private Color[] _lastWaterColors;
    private float _lastWaterCutoff;
    private int _lastColorCompressionLevel;
    private bool _lastUseColorCompression;
    private float _lastHorzSpacing;
    private float _lastVertSpacing;
    private float _lastSpacingMultiplier;
    private MeshType _lastPointMeshType;


    private readonly List<Color> _waterColorsList = new();
    private readonly List<Color> _landColorsList = new();

    private Dictionary<Color, Material> _pointMaterialsByColor = new();
    private Dictionary<MeshType, Dictionary<Color, Mesh>> _meshDicts = new Dictionary<MeshType, Dictionary<Color, Mesh>>();


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

    private static bool _updating = false;



    [ExecuteInEditMode]
    void Start()
    {
        if (GenerateOnStartAndAwake) { GenerateMap(); }
    }
    [ExecuteInEditMode]
    void Awake()
    {
        if (GenerateOnStartAndAwake) { GenerateMap(); }
    }

    void OnEnable()
    {
        if (GenerateOnEnable) { GenerateMap(); }
    }
    void OnDisable()
    {
        if (this == null || gameObject == null) { return; }// failsafe in case of deletion out of order
        if (ClearOnDisable) { ClearMap(); }
    }

    [ExecuteInEditMode]
    private void Update()
    {
        if (ForceReGenerate)
        {
            GenerateMap();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (GenerateOnValidate || ForceReGenerate)
        {
            UnityEditor.EditorApplication.delayCall += GenerateMap;
        }
    }
#endif


    void GenerateMap()
    {
        if (!gameObject.activeInHierarchy)
        {
            // will not generate while inactive 
            return;
        }

        if (_updating) { return; }
        _updating = true;

        ResetType resetType = ResetType.None;

        if (!_initialized || ForceReGenerate)
        {
            resetType = ResetType.Full;
            _initialized = true;
            ForceReGenerate = true;
        }
        else
        {
            bool hardRedraw =
                _mapPointsContainer == null ||
                transform.childCount != 2 ||
                showLand != _lastShowLand ||
                showWater != _lastShowWater ||
                showBorder != _lastShowBorder ||
                generateHiddenSurfaces != _lastGenerateHiddenSurfaces ||
                _pointIsVisible == null || _pointIsVisible.Length == 0 ||
                _pointsMeshFilters == null || _pointsMeshFilters.Length == 0 ||
                _pointsMeshRenderers == null || _pointsMeshRenderers.Length == 0 ||
                _pointsTransforms == null || _pointsTransforms.Length == 0 ||
                _pointsPositions == null || _pointsPositions.Length == 0 ||
                _pointIsWater == null || _pointIsWater.Length == 0 ||
                _pointsColor == null || _pointsColor.Length == 0 ||

                _pointIsVisible.Length != _pointsMeshRenderers.Length ||
                _pointsMeshFilters.Length != _pointsMeshRenderers.Length ||
                _pointsPositions.Length != _pointsMeshRenderers.Length ||
                _pointIsWater.Length != _pointsMeshRenderers.Length ||
                _pointsColor.Length != _pointsMeshRenderers.Length ||
                _pointsTransforms.Length != _pointsMeshRenderers.Length;

            bool softRedraw = hardRedraw ||
                width != _lastWidth ||
                height != _lastHeight ||
                horzSpacing != _lastHorzSpacing ||
                vertSpacing != _lastVertSpacing ||
                spacingUniformMultiplier != _lastSpacingMultiplier;


            if (softRedraw || hardRedraw)
            {
                // check for parenting issue 
                if (hardRedraw)
                {
                    ForceReGenerate = true;// gotta recreate the hierarchy, force redraw 
                }
                // scale changed
                resetType = ResetType.Full;
            }
            else if (
                pointSizeMultiplier != _lastPointSizeMultiplier ||
                dynamicPointSize != _lastDynamicPointSize ||
                colorAverageOffset != _lastColorAverageOffset ||
                waterColors != _lastWaterColors ||
                waterCutoff != _lastWaterCutoff ||
                colorCompressionLevel != _lastColorCompressionLevel ||
                useColorCompression != _lastUseColorCompression ||
                pointMeshType != _lastPointMeshType)
            {
                // just appearance changed, point size or source image 
                resetType = ResetType.AppearanceOnly;
            }
        }

        CheckPrimitives(ForceReGenerate);

        switch (resetType)
        {
            case ResetType.Full:

                // determine rows and columns
                int rows = Mathf.RoundToInt(height / (vertSpacing * spacingUniformMultiplier)) + 1;
                int columns = Mathf.RoundToInt(width / (horzSpacing * spacingUniformMultiplier)) + 1;

                if (rows < 2) { rows = 2; }
                if (columns < 2) { columns = 2; }

                // determine if recalculation is ACTUALLY needed 

                if (ForceReGenerate ||
                    width != _lastWidth ||
                    showBorder != _lastShowBorder ||
                    height != _lastHeight ||
                    rows != _lastRows ||
                    columns != _lastColumns)
                {

                    ClearMap();

                    // create container 
                    _mapPointsContainer = new GameObject("Map Points Container");
                    _mapPointsContainer.hideFlags = AllowGeneratedContentSave ?
                        HideFlags.None : HideFlags.DontSave;
                    _mapPointsContainer.transform.SetParent(transform);
                    _mapPointsContainer.transform.localPosition = new Vector3(
                        width * -0.5f, height * -0.5f, 0);
                    _mapPointsContainer.transform.localEulerAngles = Vector3.zero;
                    _mapPointsContainer.transform.localScale = Vector3.one;

                    int totalCount = showBorder ?
                        rows * columns :
                        (rows - 2) * (columns - 2);

                    _pointsTransforms = new Transform[totalCount];
                    _pointsMeshFilters = new MeshFilter[totalCount];
                    _pointsMeshRenderers = new MeshRenderer[totalCount];
                    _pointsPositions = new Vector2[totalCount];
                    _pointIsVisible = new bool[totalCount];
                    _pointIsWater = new bool[totalCount];
                    _pointsColor = new Color[totalCount];

                    int index = -1;// start at -1, because it gets incremented at START of the loop 

                    _columnSpacing = width / (columns - 1);
                    _rowSpacing = height / (rows - 1);

                    int rowStart = showBorder ? 0 : 1;
                    int rowEnd = showBorder ? rows : rows - 1;
                    int columnStart = showBorder ? 0 : 1;
                    int columnEnd = showBorder ? columns : columns - 1;

                    // get reference to texture 
                    Texture2D texture = (Texture2D)mapMaterial.mainTexture;

                    // create points 
                    for (int i = rowStart; i < rowEnd; i++)
                    {

                        // generate the ROW object 
                        GameObject r = new GameObject($"Row {i}");
                        r.hideFlags = AllowGeneratedContentSave ?
                            HideFlags.None : HideFlags.DontSave;
                        r.transform.SetParent(_mapPointsContainer.transform);
                        r.transform.localPosition = new Vector3(0, (_rowSpacing * i), 0);
                        r.transform.localEulerAngles = Vector3.zero;
                        r.transform.localScale = Vector3.one;

                        for (int j = columnStart; j < columnEnd; j++)
                        {
                            index++;
                            
                            // generate the POINT object (in place of the column)

                            float x = (float)j / (columns - 1);
                            float y = (float)i / (rows - 1);
                            _pointsPositions[index] = new Vector2(x, y);
                            Color color = TestPixel(texture, x, y, out bool isWater);
                            bool showSurface = isWater ? showWater : showLand;

                            // properties that are always present 
                            _pointsColor[index] = color;
                            _pointIsWater[index] = isWater;

                            bool ptVis = showSurface || generateHiddenSurfaces;
                            _pointIsVisible[index] = ptVis;

                            if (ptVis)
                            {
                                Debug.Log("ptVIs: " + ptVis + ", isWater: " + isWater);
                                // generate object if necessary 
                                GameObject pt = new GameObject($"Point {i}:{j}");
                                pt.hideFlags = AllowGeneratedContentSave ?
                                    HideFlags.None : HideFlags.DontSave;
                                pt.transform.SetParent(r.transform);
                                pt.transform.localPosition = new Vector3(j * _columnSpacing, 0f, 0f);
                                pt.transform.localEulerAngles = Vector3.zero;
                                MeshFilter mf = pt.AddComponent<MeshFilter>();
                                MeshRenderer mr = pt.AddComponent<MeshRenderer>();
                                _pointsTransforms[index] = pt.transform;
                                _pointsMeshFilters[index] = mf;
                                _pointsMeshRenderers[index] = mr;

                            }
                        }
                    }
                }

                // reset values 
                _lastWidth = width;
                _lastHeight = height;
                _lastShowBorder = showBorder;
                _lastHorzSpacing = horzSpacing;
                _lastVertSpacing = vertSpacing;
                _lastSpacingMultiplier = spacingUniformMultiplier;
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
                    _mapGraphic.hideFlags = AllowGeneratedContentSave ?
                        HideFlags.None : HideFlags.DontSave;
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
                if (ForceReGenerate || resetType == ResetType.Full ||
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
                    for (int i = 0; i < _pointIsVisible.Length; i++)
                    {
                        if (_pointIsVisible[i])
                        {
                            _pointsTransforms[i].localScale = size;
                        }
                    }
                }

                if (ForceReGenerate || newMap || resetType == ResetType.Full ||
                    showLand != _lastShowLand || showWater != _lastShowWater ||
                    generateHiddenSurfaces != _lastGenerateHiddenSurfaces ||
                    colorAverageOffset != _lastColorAverageOffset ||
                    waterColors != _lastWaterColors || waterCutoff != _lastWaterCutoff ||
                    _lastPointMeshType != pointMeshType || useColorCompression != _lastUseColorCompression ||
                    colorCompressionLevel != _lastColorCompressionLevel)
                {
                    bool meshTypeIsVisible = pointMeshType != MeshType.None;
                    _compressedWaterColors = new Color[(waterColors.Length * 2) + 1];
                    for (int i = 0; i < waterColors.Length; i++)
                    {
                        _compressedWaterColors[i] = waterColors[i];
                    }
                    for (int i = waterColors.Length; i < waterColors.Length * 2; i++)
                    {
                        _compressedWaterColors[i] = CompressColor(waterColors[i - waterColors.Length]);
                    }
                    _compressedWaterColors[_compressedWaterColors.Length - 1] = Color.black;// add black to very end of array 
                    for (int i = 0; i < _pointIsVisible.Length; i++)
                    {
                        if (_pointIsVisible[i])
                        {
                            if (meshTypeIsVisible)
                            {
                                // yep, visible quad type, determine if surface type is visible 
                                if (_pointIsWater[i] ? showWater : showLand)
                                {
                                    // yep, visible point 
                                    Color color = _pointsColor[i];
                                    _pointsMeshFilters[i].sharedMesh = GetMeshType(pointMeshType, color);
                                    _pointsMeshRenderers[i].sharedMaterial = GetMaterialByColor(color);
                                    _pointsMeshRenderers[i].enabled = true;
                                    _pointsTransforms[i].gameObject.SetActive(true);
                                    continue;
                                }
                            }
                            // not visible
                            _pointsMeshFilters[i].sharedMesh = null;
                            _pointsMeshRenderers[i].sharedMaterial = null;
                            _pointsMeshRenderers[i].enabled = false;
                            _pointsTransforms[i].gameObject.SetActive(false);
                        }
                    }
                }

                // reset values 
                _lastPointSizeMultiplier = pointSizeMultiplier;
                _lastDynamicPointSize = dynamicPointSize;
                _lastShowLand = showLand;
                _lastGenerateHiddenSurfaces = generateHiddenSurfaces;
                _lastColorAverageOffset = colorAverageOffset;
                _lastShowWater = showWater;
                _lastWaterColors = waterColors;
                _lastWaterCutoff = waterCutoff;
                _lastColorCompressionLevel = colorCompressionLevel;
                _lastUseColorCompression = useColorCompression;
                _lastPointMeshType = pointMeshType;
                break;

            case ResetType.None:
                // do nothing 
                break;

            default:
                // invalid type 
                Debug.LogError($"ERROR: invalid reset type {resetType}, returning");
                break;
        }

        ForceReGenerate = false;
        _updating = false;

    }

    private Material GetMaterialByColor(Color color)
    {
        if (_pointMaterialsByColor == null) { _pointMaterialsByColor = new(); }
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
            _meshDicts.Add(type, new());
            return GetMeshType(type, color);
        }
    }


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
            _quadPrimitive.hideFlags = AllowGeneratedContentSave ?
                HideFlags.HideInHierarchy : HideFlags.HideAndDontSave;
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
            _cubePrimitive.hideFlags = AllowGeneratedContentSave ?
                HideFlags.HideInHierarchy : HideFlags.HideAndDontSave;
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
            _spherePrimitive.hideFlags = AllowGeneratedContentSave ?
                HideFlags.HideInHierarchy : HideFlags.HideAndDontSave;
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
        if (useColorCompression &&
            colorCompressionLevel >= 0 && colorCompressionLevel <= MAX_COLOR_COMPRESSION)
        {
            if (colorCompressionLevel == 0)
            {
                int c = Mathf.RoundToInt((color.r + color.g + color.b) / 3);
                color.r = c;
                color.g = c;
                color.b = c;
            }
            else
            {
                color.r = Mathf.Round(color.r * colorCompressionLevel) / colorCompressionLevel;
                color.g = Mathf.Round(color.g * colorCompressionLevel) / colorCompressionLevel;
                color.b = Mathf.Round(color.b * colorCompressionLevel) / colorCompressionLevel;
            }
        }
        return color;
    }

    private Color TestPixel(Texture2D texture, float widthPercent, float heightPercent, out bool isWater)
    {
        int x = Mathf.FloorToInt(texture.width * Mathf.Clamp01(widthPercent));
        int y = Mathf.FloorToInt(texture.height * Mathf.Clamp01(heightPercent));

        // TODO: this line is EXPENSIVE, find a better way 
        Color color = texture.GetPixel(x, y);

        if (colorAverageOffset > 0)
        {
            List<Color> colors = new()
            {
                color,
                texture.GetPixel(x - colorAverageOffset, y - colorAverageOffset),
                texture.GetPixel(x - colorAverageOffset, y + colorAverageOffset),
                texture.GetPixel(x + colorAverageOffset, y - colorAverageOffset),
                texture.GetPixel(x + colorAverageOffset, y + colorAverageOffset)
            };
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


    void ClearMap()
    {
        // clear lists 
        _waterColorsList.Clear();
        _landColorsList.Clear();
        _pointMaterialsByColor.Clear();
        _meshDicts.Clear();
        // clear arrays 
        _pointIsVisible = new bool[0];
        _pointsTransforms = new Transform[0];
        _pointsMeshFilters = new MeshFilter[0];
        _pointsMeshRenderers = new MeshRenderer[0];
        _pointsPositions = new Vector2[0];
        _pointsColor = new Color[0];
        _pointIsWater = new bool[0];
        // destroy children 
        if (this != null && transform != null)
        {
            for (int i = transform.childCount; i > 0; --i)
            {
                DestroyGivenObject(transform.GetChild(0).gameObject);
            }
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