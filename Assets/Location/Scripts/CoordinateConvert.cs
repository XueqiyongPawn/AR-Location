using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CoordinateConvert : MonoBehaviour
{
    public static Vector3 ChangeGPSLocation2ARPos(Location referenceLocation, Location targetLocation, Transform cameraTransform)
    {
        double gpsDis = referenceLocation.Distance(targetLocation);
        double gpsAngle = referenceLocation.Bearing(targetLocation);

        double deviceHeading = Input.compass.trueHeading;

        double angleOffset = gpsAngle - deviceHeading;//方位角按顺时针算,unity角度也按照顺时针
        Vector2 camForwardXZ = new Vector2(cameraTransform.forward.x, cameraTransform.forward.z);
        Vector2 camPosXZ = new Vector2(cameraTransform.position.x, cameraTransform.position.z);
        //先将向量平移到原点
        Vector2 moveVec = camForwardXZ + new Vector2(0 - camPosXZ.x, 0 - camPosXZ.y);
        //绕原点旋转
        double angleOffsetRadian = angleOffset / 360d * 2d * Math.PI;
        angleOffsetRadian = -angleOffsetRadian;
        //deg / 360d * 2d * Math.PI;//[x*cosA-y*sinA  ,x*sinA+y*cosA] 向量（x,y） 绕原点逆时针旋转A
        Vector2 rotateVec = new Vector2((float)(moveVec.x * Math.Cos(angleOffsetRadian) - moveVec.y * Math.Sin(angleOffsetRadian)), (float)(moveVec.x * Math.Sin(angleOffsetRadian) + moveVec.y * Math.Cos(angleOffsetRadian)));
        //移会本来的点
        Vector2 moveBackVec = rotateVec + camPosXZ;

        //相机到物体的点向量为
        Vector2 camToTargetVec = (float)(gpsDis / moveBackVec.magnitude) * moveBackVec;

        Vector2 targetVec = camToTargetVec + camPosXZ;

        return new Vector3(targetVec.x, cameraTransform.position.y + (float)(targetLocation.altitude - referenceLocation.altitude), targetVec.y);
    }

    public static Location ChangeARPos2GPSLocation(Location referenceLocation, Vector3 ArPos, Transform camTransform)
    {
        Vector2 arPosXZ = new Vector2(ArPos.x, ArPos.z);
        Vector2 camPosXZ = new Vector2(camTransform.position.x, camTransform.position.z);

        float dis = Vector2.Distance(arPosXZ, camPosXZ);

        Vector2 camForwardXZ = new Vector2(camTransform.forward.x, camTransform.forward.z);
        Vector2 vecCam2ArPos = arPosXZ - camPosXZ;
        float angle = Vector2.Angle(camForwardXZ, vecCam2ArPos);

        //定义向量vec{a}、vec{b}，当vec{a}Xvec{b}<0时（X就表示叉乘），vec{b}对应的线段在vec{a}的顺时针方向；当vec{a}Xvec{b}=0时，vec{a}、vec{b}共线；当vec{a}Xvec{b}>0时，vec{b}在vec{a}的逆时针方向
        if (Vector3.Cross(new Vector3(camForwardXZ.x, 0, camForwardXZ.y), new Vector3(vecCam2ArPos.x, 0, vecCam2ArPos.y)).y > 0)
        {
            angle = -angle;
        }

        float bearing = Input.compass.trueHeading + angle;

        Location targetLocation = referenceLocation.ComputerByLonLat(bearing, dis);
        targetLocation.altitude = referenceLocation.altitude + (ArPos.y - camTransform.position.y);
        return targetLocation;
    }
}

[Serializable]
public class Location
{
    /// <summary>
    /// 纬度
    /// </summary>
    public double latitude;

    /// <summary>
    /// 经度
    /// </summary>
    public double longitude;

    /// <summary>
    /// 海拔
    /// </summary>
    public double altitude;

    public Location()
    {
    }

    public Location(double latitude, double longitude, double altitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = altitude;
    }

    /// <summary>
    /// 计算两个经纬度坐标之间的距离
    /// </summary>
    /// <param name="other"></param>
    /// <returns>单位：m</returns>
    public double Distance(Location other)
    {
        int R = 6371; // 地球半径KM
        double latDistance = Deg2Rad(other.latitude - latitude);
        double lonDistance = Deg2Rad(other.longitude - longitude);
        double a = Math.Sin(latDistance / 2d) * Math.Sin(latDistance / 2d)
                + Math.Cos(Deg2Rad(latitude)) * Math.Cos(Deg2Rad(other.latitude))
                * Math.Sin(lonDistance / 2d) * Math.Sin(lonDistance / 2d);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = R * c * 1000d; // 单位转换成米
        //distance = Math.Pow(distance, 2);  //TODO:这边不懂为什么先平方再开方
        return  distance/*Math.Sqrt(distance)*/;
    }

    /// <summary>
    /// 计算两经纬度坐标基于水平的角度
    /// </summary>
    /// <param name="other"></param>
    /// <returns>单位：度</returns>
    public double Bearing(Location other)
    {
        double longitude1 = longitude;
        double longitude2 = other.longitude;
        double latitude1 = Deg2Rad(latitude);
        double latitude2 = Deg2Rad(other.latitude);
        double longDiff = Deg2Rad(longitude2 - longitude1);
        double y = Math.Sin(longDiff) * Math.Cos(latitude2);
        double x = Math.Cos(latitude1) * Math.Sin(latitude2) - Math.Sin(latitude1) * Math.Cos(latitude2) * Math.Cos(longDiff);
        return (Rad2Deg(Math.Atan2(y, x)) + 360d) % 360d;
    }

    /** 长半径a=6378137 */
    private const double a = 6378137;
    /** 短半径b=6356752.3142 */
    private const double b = 6356752.3142;
    /** 扁率f=1/298.2572236 */
    private const double f = 1 / 298.2572236;

    /// <summary>
    ///
    /// </summary>
    /// <param name="brng">单位：度</param>
    /// <param name="dist">单位：M</param>
    /// <returns></returns>
    public Location ComputerByLonLat(double brng, double dist)
    {
        double lon = longitude;
        double lat = latitude;
        double alpha1 = Deg2Rad(brng);
        double sinAlpha1 = Math.Sin(alpha1);
        double cosAlpha1 = Math.Cos(alpha1);

        double tanU1 = (1 - f) * Math.Tan(Deg2Rad(lat));
        double cosU1 = 1 / Math.Sqrt((1 + tanU1 * tanU1));
        double sinU1 = tanU1 * cosU1;
        double sigma1 = Math.Atan2(tanU1, cosAlpha1);
        double sinAlpha = cosU1 * sinAlpha1;
        double cosSqAlpha = 1 - sinAlpha * sinAlpha;
        double uSq = cosSqAlpha * (a * a - b * b) / (b * b);
        double A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
        double B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));

        double cos2SigmaM = 0;
        double sinSigma = 0;
        double cosSigma = 0;
        double sigma = dist / (b * A), sigmaP = 2 * Math.PI;
        while (Math.Abs(sigma - sigmaP) > 1e-12)
        {
            cos2SigmaM = Math.Cos(2 * sigma1 + sigma);
            sinSigma = Math.Sin(sigma);
            cosSigma = Math.Cos(sigma);
            double deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)
                    - B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
            sigmaP = sigma;
            sigma = dist / (b * A) + deltaSigma;
        }

        double tmp = sinU1 * sinSigma - cosU1 * cosSigma * cosAlpha1;
        double lat2 = Math.Atan2(sinU1 * cosSigma + cosU1 * sinSigma * cosAlpha1,
                (1 - f) * Math.Sqrt(sinAlpha * sinAlpha + tmp * tmp));
        double lambda = Math.Atan2(sinSigma * sinAlpha1, cosU1 * cosSigma - sinU1 * sinSigma * cosAlpha1);
        double C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));
        double L = lambda - (1 - C) * f * sinAlpha
                * (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));

        double revAz = Math.Atan2(sinAlpha, -tmp); // final bearing

        return new Location(Rad2Deg(lat2), lon + Rad2Deg(L), 0);
        //Debug.Log(revAz);
        //Debug.Log(lon + deg(L) + "," + deg(lat2));
    }

    private double Deg2Rad(double deg)
    {
        return deg / 360d * 2d * Math.PI;
    }

    private double Rad2Deg(double ray)
    {
        return ray * 360d / 2d / Math.PI;
    }
}

[Serializable]
public class ArObjData
{
    public Location location;
    public Vector3 rotate;
    public Vector3 scale;
}