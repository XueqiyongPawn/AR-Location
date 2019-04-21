using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLocation : MonoBehaviour
{
    public GameObject gpsText;

    // Update is called once per frame
    private void Update()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            gpsText.GetComponent<Text>().text = string.Format("{0},{1},{2},{3}", LocationObtainment.location.latitude.ToString(), LocationObtainment.location.longitude.ToString(), LocationObtainment.location.altitude.ToString(), LocationObtainment.trueHeading.ToString());
        }
    }
}