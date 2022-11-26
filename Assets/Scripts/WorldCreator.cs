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

    [Range(0.01f, 2f)] public float spacing = 0.5f;

    public Material mapMaterial;


    private GameObject _mapPointsContainer;
    private Transform[] _pointsTransforms;
    private GameObject _mapGraphic;
    private MeshRenderer _mapMeshRenderer;


    private Texture2D _lastMapSourceImage;
    private Material _lastMapMaterial;

    private float _lastWidth;
    private float _lastHeight;
    private float _lastPointSize;
    private float _lastSpacing;

    private int _lastRows;
    private int _lastColumns;

    private static GameObject _quadPrimitive;
    private static GameObject _cubePrimitive;


    private bool _initialized = false;


    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    void OnInspectorGUI() {
        GenerateMap();
    }
    void OnAwake() {
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
        Debug.Log("Generating map...");

        if (_updating) { return; }
        _updating = true;

        CheckPrimitives();

        if (mapSourceImage == null)
        {
            // null image, clear map, do nothing 
            ClearMap();
            _updating = false;
            return;
        }

        ResetType resetType = ResetType.None;

        if (!_initialized)
        {
            resetType = ResetType.Full;
            _initialized = true;
        }
        else
        {
            if (_mapPointsContainer == null ||
                transform.childCount != 2 ||
                width != _lastWidth ||
                height != _lastHeight ||
                spacing != _lastSpacing)
            {
                // scale changed
                resetType = ResetType.Full;
            }
            else if (
                pointSize != _lastPointSize ||
                mapSourceImage != _lastMapSourceImage ||
                mapMaterial != _lastMapMaterial)
            {
                // just appearance changed, point size or source image 
                resetType = ResetType.AppearanceOnly;
            }
        }

        switch (resetType)
        {
            case ResetType.Full:

                ClearMap();

                // create container 
                _mapPointsContainer = new GameObject("Map Points Container");
                _mapPointsContainer.transform.SetParent(transform);
                _mapPointsContainer.transform.localPosition = Vector3.zero;
                _mapPointsContainer.transform.localEulerAngles = Vector3.zero;
                _mapPointsContainer.transform.localScale = Vector3.one;

                // determine rows and columns
                int rows = Mathf.FloorToInt(height / spacing);
                int columns = Mathf.FloorToInt(width / spacing);

                _pointsTransforms = new Transform[rows * columns];
                int index = 0;

                Debug.Log("rows/columns " + rows + "/" + columns);



                // create points 
                for (int i = 0; i < rows; i++)
                {
                    GameObject r = new GameObject($"Row {i}");
                    r.transform.SetParent(_mapPointsContainer.transform);
                    r.transform.localPosition = new Vector3(0, spacing * i, 0);
                    r.transform.localEulerAngles = Vector3.zero;
                    r.transform.localScale = Vector3.one;
                    for (int j = 0; j < columns; j++)
                    {
                        GameObject pt = GameObject.Instantiate(_cubePrimitive);
                        pt.SetActive(true);
                        pt.name = $"Point {i}:{j}";
                        pt.transform.SetParent(r.transform);
                        pt.transform.localPosition = new Vector3(j * spacing, 0f, 0f);
                        pt.transform.localEulerAngles = Vector3.zero;
                        _pointsTransforms[index] = pt.transform;
                        index++;
                    }
                }

                // reset values 
                _lastWidth = width;
                _lastHeight = height;
                _lastSpacing = spacing;

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
                _mapGraphic.transform.localScale = new Vector3(width, height, 1f);
                _mapMeshRenderer = _mapGraphic.GetComponent<MeshRenderer>();
                Material mat = new Material(mapMaterial);
                mat.mainTexture = mapSourceImage;
                _mapMeshRenderer.sharedMaterial = mat;

                // get all meshrenderers in map points container 
                foreach (Transform t in _pointsTransforms)
                {
                    t.localScale = Vector3.one * pointSize;
                }

                // reset values 
                _lastMapSourceImage = mapSourceImage;
                _lastPointSize = pointSize;
                _lastMapMaterial = mapMaterial;
                break;

            case ResetType.None:
                // do nothing 
                _updating = false;
                return;

            default:
                // invalid type 
                Debug.LogError($"ERROR: invalid reset type {resetType}, returning");
                _updating = false;
                return;
        }

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
        int cc = transform.childCount;
        for (int i = cc - 1; i >= 0; i--)
        {
            DestroyGameObject(transform.GetChild(i).gameObject);
        }
    }

    void DestroyGameObject(GameObject obj)
    {
        if (Application.isPlaying)
            Destroy(_mapPointsContainer);
        else
        {
#if UNITY_EDITOR
            // bonkers workaround to calling destroy 
            UnityEditor.EditorApplication.delayCall += () =>
            {
                // UnityEditor.Undo.DestroyObjectImmediate(_mapPointsContainer);// with undo 
                DestroyImmediate(_mapPointsContainer);// no undo 
            };
#else
            // theoretically impossible to get here but whatever 
            DestroyImmediate(_mapPointsContainer);// no undo 
#endif
        }
    }

}