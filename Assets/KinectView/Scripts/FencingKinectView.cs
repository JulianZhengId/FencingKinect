using UnityEngine;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;
using System;
using System.Collections;

public class FencingKinectView : MonoBehaviour
{
    public GameObject BodySourceManager;
    public Kinect.Body[] initialBodies = new Kinect.Body[2];

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    private bool justCalledPause = false;

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
    private List<GestureData> playerGestureDatas = new List<GestureData>();

    //position checker
    private bool isIndexDetermined = false;
    private bool isOnePlayerJoined = false;
    private float tempX;

    public enum Gesture
    {
        None = 0,
        RaiseRightHand, // move right
        RaiseLeftHand, //move left
        Cross, //parry
        TPose, //pause
        RightHandFootForward //attack
    }

    public class GestureData
    {
        public ulong userId;
        public uint state;
        public float duration;
        public Gesture gesture;
        public float timestamp;
        public Action action;
        public Action onReset;

        public GestureData(ulong id, uint state, float duration, Gesture gesture, Action action)
        {
            this.userId = id;
            this.state = state;
            this.duration = duration;
            this.gesture = gesture;
            this.timestamp = 0f;
            this.action = action;
            this.onReset = null;
        }

        public GestureData(ulong id, uint state, float duration, Gesture gesture, Action action, Action onReset)
        {
            this.userId = id;
            this.state = state;
            this.duration = duration;
            this.gesture = gesture;
            this.timestamp = 0f;
            this.action = action;
            this.onReset = onReset;
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
            if (onReset != null)
            {
                onReset();
            }
        }
    }

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
                RemoveGestureDatas(trackingId);
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
                        Debug.Log("one player");
                    }
                    else if (_Bodies.Count == 2)
                    {
                        Debug.Log("two players");

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
                        AddGestureDatas();
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

    private void AddGestureDatas()
    {
        for (int index = 0; index < 2; index++)
        {
            var id = initialBodies[index].TrackingId;
            if (index == 0)
            {
                var player1Controller = GameManager.instance.GetPlayer1Controller();
                GestureData raiseRightHandGestureData = new GestureData(id, 0, 0.05f, Gesture.RaiseRightHand, () => player1Controller.MoveToRight());
                GestureData raiseLeftHandGestureData = new GestureData(id, 0, 0.05f, Gesture.RaiseLeftHand, () => player1Controller.MoveToLeft());
                GestureData crossHandsGestureData = new GestureData(id, 0, 0.1f, Gesture.Cross, () => player1Controller.Defense(), () => player1Controller.StopDefending());
                GestureData tPoseGestureData = new GestureData(id, 0, 0.1f, Gesture.TPose, () => player1Controller.PauseGame());
                GestureData attackGestureData = new GestureData(id, 0, 0.1f, Gesture.RightHandFootForward, () => player1Controller.Attack());
                playerGestureDatas.Add(raiseRightHandGestureData);
                playerGestureDatas.Add(raiseLeftHandGestureData);
                playerGestureDatas.Add(crossHandsGestureData);
                playerGestureDatas.Add(tPoseGestureData);
                playerGestureDatas.Add(attackGestureData);
            }
            else
            {
                var player2Controller = GameManager.instance.GetPlayer2Controller();
                GestureData raiseRightHandGestureData = new GestureData(id, 0, 0.05f, Gesture.RaiseRightHand, () => player2Controller.MoveToRight());
                GestureData raiseLeftHandGestureData = new GestureData(id, 0, 0.05f, Gesture.RaiseLeftHand, () => player2Controller.MoveToLeft());
                GestureData crossHandsGestureData = new GestureData(id, 0, 0.1f, Gesture.Cross, () => player2Controller.Defense(), () => player2Controller.StopDefending());
                GestureData tPoseGestureData = new GestureData(id, 0, 0.1f, Gesture.TPose, () => player2Controller.PauseGame());
                GestureData attackGestureData = new GestureData(id, 0, 0.1f, Gesture.RightHandFootForward, () => player2Controller.Attack());
                playerGestureDatas.Add(raiseRightHandGestureData);
                playerGestureDatas.Add(raiseLeftHandGestureData);
                playerGestureDatas.Add(crossHandsGestureData);
                playerGestureDatas.Add(tPoseGestureData);
                playerGestureDatas.Add(attackGestureData);
            }
        }
    }

    private void RemoveGestureDatas(ulong id)
    {
        Debug.Log("remove: " + id);
        List<GestureData> copyGestureDatas = new List<GestureData>(playerGestureDatas);
        isIndexDetermined = false;
        foreach (var gestureData in copyGestureDatas)
        {
            if (gestureData.userId == id)
                playerGestureDatas.Remove(gestureData);
        }
    }

    public void ProcessSkeleton(Kinect.Body body)
    {
        // get body id
        ulong id = body.TrackingId;

        //check for each gesture
        foreach (var gestureData in playerGestureDatas)
        {
            if (id != gestureData.userId) continue;

            if (gestureData.gesture == Gesture.RightHandFootForward)
            {
                Kinect.Joint rightHandTip = body.Joints[Kinect.JointType.HandTipRight];
                Kinect.Joint rightFoot = body.Joints[Kinect.JointType.FootRight];
                Kinect.Joint head = body.Joints[Kinect.JointType.Head];

                bool rightHandForward = (rightHandTip.Position.Z - head.Position.Z) < -0.6f;
                bool rightFootForward = (head.Position.Z - rightFoot.Position.Z) > 0.25f;
                bool isNotRisingHand = (rightHandTip.Position.Y - head.Position.Y) < 1f;

                if ((rightHandForward || rightFootForward) && isNotRisingHand)
                {
                    switch (gestureData.state)
                    {
                        case 0:
                            gestureData.SetGestureTracking(Time.time);
                            gestureData.ResetOtherGestures(playerGestureDatas, id);
                            break;
                        case 1:
                            gestureData.CheckGestureComplete(Time.time);
                            break;
                    }
                    return;
                }
                else
                {
                    gestureData.SetGestureCancelled();
                }
            }
            else if (gestureData.gesture == Gesture.RaiseRightHand)
            {
                Kinect.Joint rightHand = body.Joints[Kinect.JointType.HandRight];
                Kinect.Joint rightShoulder = body.Joints[Kinect.JointType.ShoulderRight];
                bool raiseRightHand = (rightHand.Position.Y - rightShoulder.Position.Y) > 0.35f;

                if (raiseRightHand)
                {
                    switch (gestureData.state)
                    {
                        //detection
                        case 0:
                            gestureData.SetGestureTracking(Time.time);
                            gestureData.ResetOtherGestures(playerGestureDatas, id);
                            break;
                        //completion
                        case 1:
                            gestureData.CheckGestureComplete(Time.time);
                            break;
                    }
                    return;
                }
                else
                {
                    gestureData.SetGestureCancelled();
                }
            }
            else if (gestureData.gesture == Gesture.RaiseLeftHand)
            {
                Kinect.Joint leftHand = body.Joints[Kinect.JointType.HandLeft];
                Kinect.Joint leftShoulder = body.Joints[Kinect.JointType.ShoulderLeft];
                bool raiseLeftHand = (leftHand.Position.Y - leftShoulder.Position.Y) > 0.35f;

                if (raiseLeftHand)
                {
                    switch (gestureData.state)
                    {
                        case 0:
                            gestureData.SetGestureTracking(Time.time);
                            gestureData.ResetOtherGestures(playerGestureDatas, id);
                            break;
                        case 1:
                            gestureData.CheckGestureComplete(Time.time);
                            break;
                    }
                    return;
                }
                else
                {
                    gestureData.SetGestureCancelled();
                }
            }
            else if (gestureData.gesture == Gesture.TPose)
            {
                Kinect.Joint rightHand = body.Joints[Kinect.JointType.HandRight];
                Kinect.Joint rightElbow = body.Joints[Kinect.JointType.ElbowRight];
                Kinect.Joint rightShoulder = body.Joints[Kinect.JointType.ShoulderRight];
                bool isSpreadingRightHand = Mathf.Abs(rightElbow.Position.Y - rightShoulder.Position.Y) < 0.1f &&
                           Mathf.Abs(rightHand.Position.Y - rightShoulder.Position.Y) < 0.1f;

                Kinect.Joint leftHand = body.Joints[Kinect.JointType.HandLeft];
                Kinect.Joint leftElbow = body.Joints[Kinect.JointType.ElbowLeft];
                Kinect.Joint leftShoulder = body.Joints[Kinect.JointType.ShoulderLeft];
                bool isSpreadingLeftHand = Mathf.Abs(leftElbow.Position.Y - leftShoulder.Position.Y) < 0.1f &&
                           Mathf.Abs(leftHand.Position.Y - leftShoulder.Position.Y) < 0.1f;

                if (isSpreadingLeftHand && isSpreadingRightHand)
                {
                    if (!justCalledPause)
                    {
                        Debug.Log("tpose");
                        justCalledPause = true;
                        gestureData.action();
                        StartCoroutine(Wait3Seconds());
                        justCalledPause = false;
                    }
                    return;
                }
                else
                {
                    gestureData.SetGestureCancelled();
                }
            }
            else if (gestureData.gesture == Gesture.Cross)
            {
                Kinect.Joint rightHandTip = body.Joints[Kinect.JointType.HandTipRight];
                Kinect.Joint leftShoulder = body.Joints[Kinect.JointType.ShoulderLeft];
                bool isRightHandTipInFront = ((leftShoulder.Position.Z - rightHandTip.Position.Z) < 0.35f)
                    && (Mathf.Abs(leftShoulder.Position.Y - rightHandTip.Position.Y) < 0.1f)
                    && (Mathf.Abs(leftShoulder.Position.X - rightHandTip.Position.X) < 0.1f);

                Kinect.Joint leftHandTip = body.Joints[Kinect.JointType.HandTipLeft];
                Kinect.Joint rightShoulder = body.Joints[Kinect.JointType.ShoulderRight];
                bool isLeftHandTipInFront = ((rightShoulder.Position.Z - leftHandTip.Position.Z) < 0.35f)
                    && (Mathf.Abs(rightShoulder.Position.Y - leftHandTip.Position.Y) < 0.1f)
                    && (Mathf.Abs(rightShoulder.Position.X - leftHandTip.Position.X) < 0.1f);

                if (isRightHandTipInFront || isLeftHandTipInFront)
                {
                    switch (gestureData.state)
                    {
                        case 0:
                            gestureData.SetGestureTracking(Time.time);
                            gestureData.ResetOtherGestures(playerGestureDatas, id);
                            break;
                        case 1:
                            gestureData.CheckGestureComplete(Time.time);
                            break;
                    }
                    return;
                }
                else
                {
                    gestureData.SetGestureCancelled();
                }
            }
        }
    }

    private IEnumerator Wait3Seconds()
    {
        yield return new WaitForSeconds(1.5f);
    }

    private void UpdateHand(Kinect.Body body)
    {
        Kinect.Joint rh = body.Joints[Kinect.JointType.HandRight];
        Vector3 handPos = GetVector3FromJoint(rh) * 10;
        handPos.z = 0;

        GameManager.instance.rightHandObject.SetHandPosition(handPos);
    }
}