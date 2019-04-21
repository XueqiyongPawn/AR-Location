using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HttpHelper : MonoBehaviour
{
    public const string URL = "http://47.101.196.204:8080/api/v1/ar/objects";
    private static HttpHelper _instance;
    public static HttpHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject httpObj = new GameObject("HttpHelper");
                 _instance =  httpObj.AddComponent<HttpHelper>();    
            }
            return _instance;
        }
    }

    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="action"></param>
    public void Get(string url, Action<UnityWebRequest> actionResult = null)
    {
        StartCoroutine(_Get(url, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="serverURL">服务器请求目标地址</param>
    /// <param name="data">form表单参数</param>
    /// <param name="data">处理返回结果的委托,处理请求对象</param>
    /// <returns></returns>
    public void Post(string serverURL, string data, Action<UnityWebRequest> actionResult = null)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        StartCoroutine(_Post(serverURL, data, actionResult));
    }

    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url">请求地址,like 'http://www.my-server.com/ '</param>
    /// <param name="action">请求发起后处理回调结果的委托</param>
    /// <returns></returns>
    IEnumerator _Get(string url, Action<UnityWebRequest> actionResult = null
        )
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
            if (actionResult != null)
            {
                actionResult(uwr);
            }
        }
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="serverURL">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="data">form表单参数</param>
    /// <param name="data">处理返回结果的委托</param>
    /// <returns></returns>
    IEnumerator _Post(string serverURL, string data, Action<UnityWebRequest> actionResult = null)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Post(serverURL, data))
        {
            yield return uwr.SendWebRequest();
            if (actionResult != null)
            {
                //if (uwr.isHttpError || uwr.isNetworkError)
                //{
                //    Debug.Log(uwr.error);
                //}
                //else
                {
                    actionResult(uwr);
                }

            }
        }
    }
}
