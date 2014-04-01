﻿using UnityEngine;
using System.Collections;

public class MovingPlatformMove : MonoBehaviour {

	public float targetY;

	void Update() {
		float y = transform.position.y;
		y += Mathf.Min(Mathf.Abs(y - targetY), .1f) * (Time.deltaTime * 50) * Mathf.Sign(targetY - y);
		transform.position = new Vector3(transform.position.x, y, transform.position.z);
	}
}