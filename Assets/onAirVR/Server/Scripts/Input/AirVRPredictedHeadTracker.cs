﻿/***********************************************************

  Copyright (c) 2017-2018 Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using NetMQ.Sockets;
using System;

public class AirVRPredictedHeadTrackerInputDevice : AirVRInputDevice {
    [Serializable]
    private class Arguments {
        public string PredictedMotionOutputEndpoint;
    }

    private NetMQ.Msg _msgRecv;
    private PullSocket _zmqPredictedMotion;
    private long _lastTimeStamp;
    private Quaternion _lastOrientation = Quaternion.identity;
    private Vector3 _centerEyePosition = new Vector3(0.0f, 0.097f, 0.0805f);

    private float _predictionTime;

    // implements AirVRInputDevice
    protected override string deviceName {
        get {
            return AirVRInputDeviceName.HeadTracker;
        }
    }

    protected override void MakeControlList() {
        AddControlTransform((byte)AirVRHeadTrackerKey.Transform);
    }

    protected override void UpdateExtendedControls() {
        if (isRegistered == false) {
            return;
        }

        Debug.Assert(_zmqPredictedMotion != null);
        while (_zmqPredictedMotion.TryReceive(ref _msgRecv, System.TimeSpan.Zero)) {
            if (_msgRecv.Size <= 0) {
                continue;
            }

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(_msgRecv.Data, 0, 8);
                for (int i = 0; i < 5; i++) {
                    Array.Reverse(_msgRecv.Data, 8 + i * 4, 4);
                }
            }

            _lastTimeStamp = BitConverter.ToInt64(_msgRecv.Data, 0);
            _predictionTime = BitConverter.ToSingle(_msgRecv.Data, 8);

            // convert coordinate from OpenGL to Unity
            _lastOrientation = new Quaternion(-BitConverter.ToSingle(_msgRecv.Data, 8 + 4),
                                              -BitConverter.ToSingle(_msgRecv.Data, 8 + 4 * 2),
                                              BitConverter.ToSingle(_msgRecv.Data, 8 + 4 * 3),
                                              BitConverter.ToSingle(_msgRecv.Data, 8 + 4 * 4));
        }

        OverrideControlTransform((byte)AirVRHeadTrackerKey.Transform, _lastTimeStamp, _lastOrientation * _centerEyePosition, _lastOrientation);
    }

    public override void OnRegistered(byte inDeviceID, string arguments) {
        base.OnRegistered(inDeviceID, arguments);

        Arguments args = JsonUtility.FromJson<Arguments>(arguments);
        Debug.Assert(string.IsNullOrEmpty(args.PredictedMotionOutputEndpoint) == false);
        Debug.Assert(_zmqPredictedMotion == null);

        _zmqPredictedMotion = new PullSocket();
        _zmqPredictedMotion.Connect(args.PredictedMotionOutputEndpoint);

        _msgRecv = new NetMQ.Msg();
        _msgRecv.InitPool(8 + 4 * 4);
    }

    public override void OnUnregistered() {
        base.OnUnregistered();

        Debug.Assert(_zmqPredictedMotion != null);
        _zmqPredictedMotion.Close();
        _msgRecv.Close();

        _zmqPredictedMotion.Dispose();
        _zmqPredictedMotion = null;
    }
}
