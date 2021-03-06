using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MusicTrigger : MonoBehaviour {

	public string track;
	public float volume;

	private bool triggered;
	private GameObject player;
	private CameraAudio cameraAudio;

	void Start() {
		player = GameObject.Find("Player");
		cameraAudio = Camera.main.GetComponent<CameraAudio>();
	}

	void OnTriggerEnter2D(Collider2D col) {
		if(col.gameObject.Equals(player) && !triggered) {
			triggered = true;
			cameraAudio.CrossFade(track, 4f, 0, volume);
		}
	}

}

