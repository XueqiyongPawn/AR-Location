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

    private GameObject curGo = null;

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
    }

    private void AddModel()
    {
        if (curGo == null)
        {
            curGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Vector3 camFrowardXZ = mainCamera.transform.forward;
            camFrowardXZ.y = 0;
            curGo.transform.position = mainCamera.transform.position + camFrowardXZ.normalized * 2;
            curGo.transform.localScale = Vector3.one * 0.5f;
            logText.text = "添加模型成功";
        }
        else
        {
            logText.text = "模型已存在，请先删除模型";
        }
    }

    private void DeleteModel()
    {
        if (curGo != null)
        {
            GameObject.DestroyImmediate(curGo);
            curGo = null;
            logText.text = "删除成功";
        }
        else
        {
            logText.text = "场景中无模型，请先添加";
        }
    }

    private void Save()
    {
        if (curGo == null)
        {
            logText.text = "场景中无模型，无法保存，请添加";
        }
        else
        {
            ArObjData arObjData = new ArObjData();
            arObjData.location = CoordinateConvert.ChangeARPos2GPSLocation(LocationObtainment.location, curGo.transform.position, mainCamera.transform);
            arObjData.rotate = curGo.transform.eulerAngles;
            arObjData.scale = curGo.transform.localScale;
            PlayerPrefs.SetString("location", JsonUtility.ToJson(arObjData));
            logText.text = "保存成功：pos " + curGo.transform.position + JsonUtility.ToJson(arObjData);
        }
    }

    private void Read()
    {
        if (curGo == null)
        {
            string json = PlayerPrefs.GetString("location");
            if (string.IsNullOrEmpty(json))
            {
                logText.text = "没有可读取的数据";
            }
            else
            {
                ArObjData data = JsonUtility.FromJson<ArObjData>(json);
                curGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curGo.transform.position = CoordinateConvert.ChangeGPSLocation2ARPos(LocationObtainment.location, data.location, mainCamera.transform);
                curGo.transform.eulerAngles = data.rotate;
                curGo.transform.localScale = data.scale;
                logText.text = "读取并加载成功：" + curGo.transform.position;
            }
        }
        else
        {
            logText.text = "场景中已存在模型，请先删除";
        }
    }
}