﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerForPlayback : MonoBehaviour {
    private VideoPlayer _videoPlayer;

    private void Awake() {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start() {
        _videoPlayer.Play();
        _videoPlayer.Pause();
    }

    public void OnPlayPreview(float startFrom) {
        _videoPlayer.playbackSpeed = 1; // 60.0f; /// 72.0f; // motionDataFps;

        _videoPlayer.Play();
    }

    public void OnStartCapture() {
        _videoPlayer.playbackSpeed = 1.0f;
    }

    public void OnStop() {
        _videoPlayer.Pause();
        _videoPlayer.frame = 0;
    }

    public void OnSeek(float startFrom) {
        //_videoPlayer.frame = (long)(secs * _videoPlayer.frameRate);

        _videoPlayer.frame = (int)(_videoPlayer.frameCount * startFrom);
    }
}
