using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public Camera mainCamera;

    public Button addModelBtn;
    public Button deleteModelBtn;
    public Button saveBtn;
    public Button readBtn;

    public Text logText;

    private GameObject _curGo = null;

    public bool isLocal = false;

    private string _lastPosId = string.Empty;

    // Start is called before the first frame update
    private void Start()
    {
        //ArObjData data = new ArObjData();
        //data.location = new Location(1, 1, 1);
        //data.rotate = Vector3.one;
        //data.scale = Vector3.one;
        //Debug.Log(JsonUtility.ToJson(data));

        addModelBtn.onClick.AddListener(AddModel);
        deleteModelBtn.onClick.AddListener(DeleteModel);
        saveBtn.onClick.AddListener(Save);
        readBtn.onClick.AddListener(Read);
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ArObjData arObjData = new ArObjData();
            SavePosData(JsonUtility.ToJson(arObjData));
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            GetPosData();
        }
    }

    private void AddModel()
    {
        if (_curGo == null)
        {
            _curGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Vector3 camFrowardXZ = mainCamera.transform.forward;
            camFrowardXZ.y = 0;
            _curGo.transform.position = mainCamera.transform.position + camFrowardXZ.normalized * 2;
            _curGo.transform.localScale = Vector3.one * 0.5f;
            logText.text = "添加模型成功";
        }
        else
        {
            logText.text = "模型已存在，请先删除模型";
        }
    }

    private void DeleteModel()
    {
        if (_curGo != null)
        {
            GameObject.DestroyImmediate(_curGo);
            _curGo = null;
            logText.text = "删除成功";
        }
        else
        {
            logText.text = "场景中无模型，请先添加";
        }
    }

    private void Save()
    {
        if (_curGo == null)
        {
            logText.text = "场景中无模型，无法保存，请添加";
        }
        else
        {
            ArObjData arObjData = new ArObjData();
            arObjData.location = CoordinateConvert.ChangeARPos2GPSLocation(LocationObtainment.location, _curGo.transform.position, mainCamera.transform);
            arObjData.rotate = _curGo.transform.eulerAngles;
            arObjData.scale = _curGo.transform.localScale;
            //PlayerPrefs.SetString("location", JsonUtility.ToJson(arObjData));
            SavePosData(JsonUtility.ToJson(arObjData));
            logText.text = "保存成功：pos " + _curGo.transform.position + JsonUtility.ToJson(arObjData);
        }
    }

    private void Read()
    {
        if (_curGo == null)
        {
            //string json = PlayerPrefs.GetString("location");
            string json = GetPosData();
            if (string.IsNullOrEmpty(json))
            {
                logText.text = "没有可读取的数据";
            }
            else
            {
                ArObjData data = JsonUtility.FromJson<ArObjData>(json);
                _curGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _curGo.transform.position = CoordinateConvert.ChangeGPSLocation2ARPos(LocationObtainment.location, data.location, mainCamera.transform);
                _curGo.transform.eulerAngles = data.rotate;
                _curGo.transform.localScale = data.scale;
                logText.text = "读取并加载成功：" + _curGo.transform.position;
            }
        }
        else
        {
            logText.text = "场景中已存在模型，请先删除";
        }
    }

    private string GetPosData()
    {
        string data = string.Empty;
        if (isLocal)
        {
            data = PlayerPrefs.GetString("location");
        }
        HttpHelper.Instance.Get(HttpHelper.URL + "/" + _lastPosId,(result)=>
        {
            if (result.isHttpError || result.isNetworkError)
            {
                Debug.Log(result.error);
            }
            else
            {
                Debug.Log("Get 数据成功：" + result.downloadHandler.text);
                data = result.downloadHandler.text;
            }
        });
        return data;
    }
    private void SavePosData(string data)
    {
        if (isLocal)
        {
            PlayerPrefs.SetString("location", JsonUtility.ToJson(data));
        }
        else
        { 
            HttpHelper.Instance.Post(HttpHelper.URL,data,(result)=>
            {
                if (result.isHttpError || result.isNetworkError)
                {
                    Debug.Log(result.error);
                }
                else
                {
                    Debug.Log("上传成功：" + result.downloadHandler.text);
                    _lastPosId = result.downloadHandler.text;
                }
                
            });
        }

    }
}