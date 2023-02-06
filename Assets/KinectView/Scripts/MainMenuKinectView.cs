using UnityEngine;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;
using System;

public class MainMenuKinectView : MonoBehaviour
{
    public GameObject BodySourceManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    private Kinect.JointType rightHand = Kinect.JointType.HandRight;

    [SerializeField] private SphereController rightHandObject;

    void Update()
    {
        //check all components
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        //add tracker ids
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        //create body object
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }


            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                UpdateHand(body);
            }
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        return body;
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    private void UpdateHand(Kinect.Body body)
    {
        Kinect.Joint rh = body.Joints[rightHand];
        Vector3 handPos = GetVector3FromJoint(rh);
        handPos.z = 0;

        rightHandObject.SetHandPosition(handPos);
    }
}
