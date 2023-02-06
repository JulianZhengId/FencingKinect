using UnityEngine;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;
using System;

public class ShootingKinectView : MonoBehaviour
{
    public GameObject BodySourceManager;
    public Kinect.Body[] initialBodies = new Kinect.Body[2];

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    [SerializeField] private PlayerController player1Controller;
    [SerializeField] private PlayerController player2Controller;

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

    //P1
    //private List<GestureData> playerGestureDatas = new List<GestureData>();

    //position checker
    private bool isIndexDetermined = false;
    private bool isOnePlayerJoined = false;
    private float tempX;

    /*    public enum GestureShoot
        {
            None = 0,
            RaiseRightHand, // move right
            RaiseLeftHand, //move left
            Cross, //parry
            TPose, //pause
            RightHandFootForward //attack
        }*/

    /*public class GestureData
    {
        public ulong userId;
        public uint state;
        public float duration;
        public Gesture gesture;
        public float timestamp;

        public Action action;

        public GestureData(ulong id, uint state, float duration, Gesture gesture, Action action)
        {
            this.userId = id;
            this.state = state;
            this.duration = duration;
            this.gesture = gesture;
            this.timestamp = 0f;
            this.action = action;
        }

        public void SetGestureTracking(float timestamp)
        {
            this.state = 1;
            this.timestamp = timestamp;
        }

        public void ResetOtherGestures(List<GestureData> gestureDatas, ulong id)
        {
            foreach (GestureData gestureData in gestureDatas)
            {
                if (gestureData.gesture == this.gesture || gestureData.userId != id) continue;
                gestureData.SetGestureCancelled();
            }
        }

        public void CheckGestureComplete(float timestamp)
        {
            float timeLeft = timestamp - this.timestamp;

            if (timeLeft >= this.duration)
            {
                Debug.Log("id: " + this.userId + "\nGesture: " + this.gesture);
                this.timestamp = timestamp;
                this.state = 1;
                this.action();
            }

        }

        public void SetGestureCancelled()
        {
            this.state = 0;
        }
    }*/

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
                //RemoveGestureDatas(trackingId);
                RemovePlayer(trackingId);
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

                if (!isIndexDetermined)
                {
                    if (_Bodies.Count == 1 && !isOnePlayerJoined)
                    {
                        tempX = GetVector3FromJoint(body.Joints[Kinect.JointType.Head]).x;
                        initialBodies[0] = body;
                        isOnePlayerJoined = true;
                    }
                    else if (_Bodies.Count == 2)
                    {
                        isIndexDetermined = true;
                        float otherX = GetVector3FromJoint(body.Joints[Kinect.JointType.Head]).x;
                        if (otherX > tempX)
                        {
                            initialBodies[1] = body;
                        }
                        else
                        {
                            var tempBody = initialBodies[0];
                            initialBodies[0] = body;
                            initialBodies[1] = tempBody;
                        }
                        
                    }
                }
                if (GameManager.instance.GetIsPaused())
                {
                    UpdateHand(body);
                }
                else
                {
                    ProcessSkeleton(body);
                }
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
        return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
    }

    private void RemovePlayer(ulong id)
    {
        isIndexDetermined = false;
    }

    public void ProcessSkeleton(Kinect.Body body)
    {
        // get body id
        ulong id = body.TrackingId;

        int index = 0;

        if (initialBodies[index].TrackingId != id)
        {
            index = 1;
        }

        if (index == 0)
        {
            Vector3 rightHandTip = GetVector3FromJoint(body.Joints[Kinect.JointType.HandTipRight]);
            Vector3 torso = GetVector3FromJoint(body.Joints[Kinect.JointType.SpineMid]);

            Vector3 armDirection = (rightHandTip - torso).normalized;
            float angle = Mathf.Atan2(armDirection.y, armDirection.x) * Mathf.Rad2Deg;
            player1Controller.RotateArm(angle + 10f);

            Kinect.Joint hip = body.Joints[Kinect.JointType.HipLeft];
            Kinect.Joint knee = body.Joints[Kinect.JointType.KneeLeft];
            bool duck = (hip.Position.Y - knee.Position.Y) < 0.2f;
            if (duck) player1Controller.Duck();
            else player1Controller.Unduck();


            //shoot
            if (body.HandRightState == Kinect.HandState.Open)
            {
                StartCoroutine(player1Controller.Shoot(angle));
            }
        }

        //player 2
        if (index == 1)
        {
            Vector3 leftHandTip = GetVector3FromJoint(body.Joints[Kinect.JointType.HandTipLeft]);
            Vector3 torso = GetVector3FromJoint(body.Joints[Kinect.JointType.SpineMid]);

            Vector3 armDirection = (leftHandTip - torso).normalized;
            float angle = Mathf.Atan2(armDirection.y, armDirection.x) * Mathf.Rad2Deg;
            player2Controller.RotateArm(angle + 10f);

            Kinect.Joint hip = body.Joints[Kinect.JointType.HipLeft];
            Kinect.Joint knee = body.Joints[Kinect.JointType.KneeLeft];
            bool duck = (hip.Position.Y - knee.Position.Y) < 0.2f;
            if (duck) player2Controller.Duck();
            else player2Controller.Unduck();

            //shoot
            if (body.HandLeftState == Kinect.HandState.Open)
            {
                //shoot
                StartCoroutine(player2Controller.Shoot(angle));
            }
        }
    }

    private void UpdateHand(Kinect.Body body)
    {
        Kinect.Joint rh = body.Joints[Kinect.JointType.HandRight];
        Vector3 handPos = GetVector3FromJoint(rh) * 10;
        handPos.z = 0;

        GameManager.instance.rightHandObject.SetHandPosition(handPos);
    }
}
