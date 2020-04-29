
using System.Collections.Generic;
using UnityEngine;

public static class NuitrackUtils
{
    #region Transform
    public static Vector3 ToVector3(this nuitrack.Joint joint)
    {
        return new Vector3(joint.Real.X, joint.Real.Y, joint.Real.Z);
    }

    public static Quaternion ToQuaternion(this nuitrack.Joint joint)
    {
        Vector3 jointUp = new Vector3(joint.Orient.Matrix[1], joint.Orient.Matrix[4], joint.Orient.Matrix[7]);   //Y(Up)
        Vector3 jointForward = new Vector3(joint.Orient.Matrix[2], joint.Orient.Matrix[5], joint.Orient.Matrix[8]);   //Z(Forward)

        return Quaternion.LookRotation(jointForward, jointUp);
    }

    public static Quaternion ToQuaternionMirrored(this nuitrack.Joint joint)
    {
        Vector3 jointUp = new Vector3(-joint.Orient.Matrix[1], joint.Orient.Matrix[4], -joint.Orient.Matrix[7]);   //Y(Up)
        Vector3 jointForward = new Vector3(joint.Orient.Matrix[2], -joint.Orient.Matrix[5], joint.Orient.Matrix[8]);   //Z(Forward)

        if (jointForward.magnitude < 0.01f)
            return Quaternion.identity; //should not happen

        return Quaternion.LookRotation(jointForward, jointUp);
    }

    #endregion

    #region ToTexture2d

    public static Texture2D ToTexture2D(this nuitrack.ColorFrame frame)
    {
        byte[] sourceData = frame.Data;

        for (int i = 0; i < sourceData.Length; i += 3)
        {
            byte temp = sourceData[i];
            sourceData[i] = sourceData[i + 2];
            sourceData[i + 2] = temp;
        }

        Texture2D rgbTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
        rgbTexture.LoadRawTextureData(sourceData);
        rgbTexture.Apply();

        Resources.UnloadUnusedAssets();

        return rgbTexture;
    }
    
    #endregion

}
