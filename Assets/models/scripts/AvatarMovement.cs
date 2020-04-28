using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMovement : MonoBehaviour
{
    [Header("Rigged Model")]
    [SerializeField]
    ModelJoint[] modelJoints;

    Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = new Dictionary<nuitrack.JointType, ModelJoint>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < modelJoints.Length; i++)
        {
            modelJoints[i].baseRotOffset = modelJoints[i].bone.rotation;
            jointsRigged.Add(modelJoints[i].jointType, modelJoints[i]);
            
            // adding base distances between the child and parent bone
            if (modelJoints[i].parentJointType != nuitrack.JointType.None)
            {
                AddBoneScale(modelJoints[i].jointType, modelJoints[i].parentJointType);
            }
        }
    }

    void AddBoneScale(nuitrack.JointType targetJoint, nuitrack.JointType parentJoint)
    {
        // take the position of the model bone
        Vector3 targetBonePos = jointsRigged[targetJoint].bone.position;
        // take the position of the model parent bone
        Vector3 parentBonePos = jointsRigged[parentJoint].bone.position;
        // Find distance between the two points from the tracked data
        jointsRigged[targetJoint].baseDistanceToParent = Vector3.Distance(parentBonePos, targetBonePos);
        // record the transform of the model parent bone 
        jointsRigged[targetJoint].parentBone = jointsRigged[parentJoint].bone;
        // extract the parent bone form the hierarchy to make it independent
        //jointsRigged[targetJoint].parentBone.parent = transform.root;
    }

    // Update is called once per frame
    void Update()
    {
       if (NuitrackManager.numOfSkeletons > 0 )
       {
            foreach (var riggedJoint in jointsRigged)
            {
                //Get joint from the Nuitrack
                nuitrack.Joint joint = NuitrackManager.trackedSkeleton.GetJoint(riggedJoint.Key);
               
                if (joint.Confidence > 0.5) //Currently, there are only two values of confidence: 0 (Nuitrack thinks that this isn't a joint) and 0.75 (a joint).
                {
                    ModelJoint modeljoint = riggedJoint.Value; //get modelJoint
                    nuitrack.Joint parentjoint = NuitrackManager.trackedSkeleton.GetJoint(modeljoint.parentJointType);

                    Vector3 newPos = 0.001f * ToVector3(joint); //given in mm
                    Vector3 parentPos = 0.001f * ToVector3(parentjoint);

                    // Coinvert nuitrack joint orientation to quaternion
                    Quaternion jointOrient = ToQuaternion(joint);

                    // Update Model joint to tracked orientation
                    
                    modeljoint.bone.rotation = jointOrient * modeljoint.baseRotOffset;
                 

                    // perform bone  scaling
                    if (modeljoint.parentBone != null)
                    {
                        // take the transform of the parent bone
                        //Transform parentBone = modeljoint.parentBone;
                        // calculate how many times the distance between the child bone and its parent bone has changed compared to the base distances (which was recorded at the start)
                        float scaleDif = modeljoint.baseDistanceToParent / Vector3.Distance(newPos,parentPos);
                        // change the size of the bone to the resulting value
                        modeljoint.parentBone.localScale = Vector3.one / scaleDif;
                    }
                }

            }   

       }
    }

    public static Vector3 ToVector3(nuitrack.Joint joint)
    {
        return new Vector3(joint.Real.X, joint.Real.Y, joint.Real.Z);
    }

    // Learn how this works
    public static Quaternion ToQuaternion(nuitrack.Joint joint)
    {
        Vector3 jointUp = new Vector3(joint.Orient.Matrix[1], joint.Orient.Matrix[4], joint.Orient.Matrix[7]);   //Y(Up)
        Vector3 jointForward = new Vector3(joint.Orient.Matrix[2], joint.Orient.Matrix[5], joint.Orient.Matrix[8]);   //Z(Forward)

        return Quaternion.LookRotation(jointForward, jointUp);
    }
}

[System.Serializable]
class ModelJoint
{
    public Transform bone;
    public nuitrack.JointType jointType;
    [HideInInspector] public Quaternion baseRotOffset;

    public nuitrack.JointType parentJointType;
    [HideInInspector] public Transform parentBone;
    [HideInInspector] public float baseDistanceToParent;
}
