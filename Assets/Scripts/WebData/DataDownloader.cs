using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

public class DataDownloader : MonoBehaviour
{
    private const string TEST_DOWNLOAD_URL =
        "https://gist.githubusercontent.com/nickyonge/e3864f0dd12eefc90c0e36e8dbd58796/raw/3b344e03590c322deaba91b45386051f09ed706c/helloworld.txt";
    public delegate void DLSuccess(object downloadedData);
    public delegate void DLFailure(object message);

    public enum DownloadDataType
    {
        Text = 0,
        Bytes = 1,
        NativeData = 2,
        FileSize = 3,
    }

    public void DownloadData(string uri,
        DLSuccess successCallback,
        DownloadDataType dataType = DownloadDataType.Text)
    {
        DownloadData(uri, successCallback, null, dataType);
    }
    public void DownloadData(string uri,
        DLSuccess successCallback, DLFailure failureCallback,
        DownloadDataType dataType = DownloadDataType.Text)
    {
        if (Application.isPlaying)
        {
            StartCoroutine(RequestData(uri, successCallback, failureCallback, dataType));
#if UNITY_EDITOR
        }
        else
        {
            EditorCoroutineUtility.StartCoroutine(
                RequestData(uri, successCallback, failureCallback, dataType), this);
#endif
        }

    }

    private IEnumerator RequestData(string uri,
        DLSuccess successCallback, DLFailure failureCallback, 
        DownloadDataType dataType)
    {

        using UnityWebRequest request = UnityWebRequest.Get(uri);

        yield return request.SendWebRequest();

        switch (request.result)
        {
            case UnityWebRequest.Result.Success:
                if (successCallback == null)
                {
                    Debug.LogWarning("WARNING: Successfully downloaded data "
                    + $"from URI {uri}, but no callback was provided.", gameObject);
                }
                else
                {
                    successCallback.Invoke(GetData(request, dataType));
                }
                break;
            default:
                Debug.LogError("Download Error: " + request.error);
                failureCallback?.Invoke(request.error);
                break;
        }

    }

    private object GetData(UnityWebRequest request, DownloadDataType dataType)
    {
        switch (dataType)
        {
            case DownloadDataType.Text:
                return request.downloadHandler.text;
            case DownloadDataType.Bytes:
                return request.downloadHandler.data;
            case DownloadDataType.NativeData:
                return request.downloadHandler.nativeData;
            case DownloadDataType.FileSize:
                return request.downloadedBytes;
            default:
                Debug.LogError($"ERROR: Invalid DownloadDataType {dataType}, " +
                    "cannot get data, returning null", gameObject);
                return null;
        }
    }

    public void TestDownload()
    {
        DownloadData(TEST_DOWNLOAD_URL, TestDownloadSuccess);
    }
    private void TestDownloadSuccess(object result)
    {
        Debug.Log($"Test download success! {(string)result}");
    }

}
