using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationObtainment : MonoBehaviour
{
    public static Location location;

    public static float trueHeading;

    private void Awake()
    {
        location = new Location();
    }

    // Use this for initialization
    private IEnumerator Start()
    {
        if (!Input.location.isEnabledByUser)
        {
            print("LocationService not available, aborting!");
            yield break;
        }

        // Start service before querying location
        Input.location.Start();
        Input.compass.enabled = true;
        // Wait until service initializes
        int waitTime = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && waitTime > 0)
        {
            yield return new WaitForSeconds(1);
            waitTime--;
        }
        // Service didn't initialize in 20 seconds
        if (waitTime < 1)
        {
            print("LocationService timed out!");
            yield break;
        }
        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            location.Longitude = Input.location.lastData.longitude;
            location.Latitude = Input.location.lastData.latitude;
            location.Altitude = Input.location.lastData.altitude;
            trueHeading = Input.compass.trueHeading;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            location.Longitude = Input.location.lastData.longitude;
            location.Latitude = Input.location.lastData.latitude;
            location.Altitude = Input.location.lastData.altitude;
            trueHeading = Input.compass.trueHeading;
        }
    }

    private IEnumerator OnApplicationPause(bool pauseState)
    {
        if (pauseState)
        {
            Input.location.Stop();
        }
        else
        {
            // Start service before querying location
            Input.location.Start();
            // Wait until service initializes
            int waitTime = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && waitTime > 0)
            {
                yield return new WaitForSeconds(1);
                waitTime--;
            }
            // Service didn't initialize in 20 seconds
            if (waitTime < 1)
            {
                print("LocationService timed out!");
                yield break;
            }
            // Connection has failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                print("Unable to determine device location");
                yield break;
            }
            else
            {
                // Access granted and location value could be retrieved
                location.Longitude = Input.location.lastData.longitude;
                location.Latitude = Input.location.lastData.latitude;
                location.Altitude = Input.location.lastData.altitude;
                trueHeading = Input.compass.trueHeading;
            }
        }
    }
}