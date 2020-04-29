using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarMovement : MonoBehaviour
{
    [Header("Rigged Model")]
    [SerializeField]
    ModelJoint[] modelJoints;

    Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = new Dictionary<nuitrack.JointType, ModelJoint>();
    private static float waitTime = 10.0f;
    private static float timer = 0.0f;
    public Text BoneScalingProgress;

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
       if (NuitrackManager.NumUsers > 0 )
       {

            if (timer <= waitTime)
            {
                timer += Time.deltaTime;
                BoneScalingProgress.text = "Bone Scaling: In Progress...";
            } else if (timer >= waitTime) {
                BoneScalingProgress.text = "Bone Scaling: Completed";
            }

           // get ID of user
           nuitrack.User[] users  = NuitrackManager.Users;
           nuitrack.User CurrentUser = users[0];
           int CurrentUserID = CurrentUser.ID;

            foreach (var riggedJoint in jointsRigged)
            {
                //Get joint from the Nuitrack
                nuitrack.Joint joint = NuitrackManager.SkeletonData.GetSkeletonByID(CurrentUserID).GetJoint(riggedJoint.Key);
            
                if (joint.Confidence > 0.5) //Currently, there are only two values of confidence: 0 (Nuitrack thinks that this isn't a joint) and 0.75 (a joint).
                {
                    ModelJoint modeljoint = riggedJoint.Value; //get modelJoint
                    nuitrack.Joint parentjoint = NuitrackManager.SkeletonData.GetSkeletonByID(CurrentUserID).GetJoint(modeljoint.parentJointType);

                    Vector3 newPos = 0.001f * joint.ToVector3(); //given in mm
                    Vector3 parentPos = 0.001f * parentjoint.ToVector3();

                    // Coinvert nuitrack joint orientation to quaternion
                    Quaternion jointOrient = joint.ToQuaternion();

                    // Update Model joint to tracked orientation
                    
                    modeljoint.bone.rotation = jointOrient * modeljoint.baseRotOffset;
                 
                        
                    // perform bone  scaling for 5 seconds at the start maybe?
                    if (modeljoint.parentBone != null && timer <= waitTime)
                    {
                        Debug.Log("BONE SCALING PERFORMED...........");
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
