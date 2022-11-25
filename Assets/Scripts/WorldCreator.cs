using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldCreator : MonoBehaviour
{

    public enum ResetType {
        None, 
        PointsOnly,
        Full,
    }

    public Texture2D mapSourceImage;
    public float width = 20f;
    public float height = 10f;

    [Range(0.01f,0.5f)] public float pointSize = 0.1f;

    [Range(0.01f,2f)] public float spacing = 0.5f;

    private GameObject _mapPointsContainer;


    private Texture2D _lastMapSourceImage;
    private float _lastWidth;
    private float _lastHeight;

    private bool _initialized = false;


    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        GenerateMap();
    }
#endif


    void GenerateMap()
    {
        ResetType resetType = ResetType.None;

        if (_initialized)
        {
            if (
                mapSourceImage == _lastMapSourceImage &&
                width == _lastWidth &&
                height == _lastHeight)
            {
                // matches, already created 
                return;
            }
        }
        _initialized = true;

        ClearMap();


        // create container 
        _mapPointsContainer = new GameObject("Map Points Container");
        _mapPointsContainer.transform.SetParent(transform);
        _mapPointsContainer.transform.localPosition = Vector3.zero;
        _mapPointsContainer.transform.localEulerAngles = Vector3.zero;
        _mapPointsContainer.transform.localScale = Vector3.one;


        // create cubes 


        // update last state 
        _lastWidth = width;
        _lastHeight = height;
        _lastMapSourceImage = mapSourceImage;
    }

    void ClearMap()
    {
        if (_mapPointsContainer != null) {
            Destroy(_mapPointsContainer);
        }
    }

}