﻿using UnityEngine;
using System.Collections;

public class RootbridgeGrowth : MonoBehaviour {

	private GameObject player;
	private Vector3 l1;
	private Vector3 l2;

	void Start() {
		player = GameObject.Find("Player");
		if(!safe()) {
			DestroyObject(gameObject);
		}
	}

	void Update() {
		Debug.DrawLine(l1, l2, Color.red);
	}

	bool safe() {
		GameObject seed = GameObject.Find("Seed");
		float radius = seed.GetComponent<CircleCollider2D>().radius * seed.transform.lossyScale.x;
		Transform body = transform.Find("body");
		Vector3 extents = body.gameObject.GetComponent<BoxCollider2D>().size * .5f;

		Vector3 p1 = transform.position;
		Vector3 p2 = transform.position;
		extents = Vector2.Scale(extents, transform.lossyScale);

		p1.y -= extents.y + 0.01f;
		p2.y = p1.y - 0.6f - radius;

		if(Physics2D.Linecast(p1, p2, 1 << LayerMask.NameToLayer("Ground"))) {
			return false;
		}

		p1.x += 0.1f;
		p1.y = transform.position.y + extents.y + 0.01f;

		p2.x += radius + 0.2f;
		p2.y = transform.position.y + extents.y + 0.01f;

		l1 = p1;
		l2 = p2;

		// Right
		if(Physics2D.Linecast(p1, p2, 1 << LayerMask.NameToLayer("Ground"))) {
			transform.position = new Vector3(transform.position.x - extents.x + radius, transform.position.y, transform.position.z);
			return true;
		}

		p1.x -= 0.3f + radius;
		p2.x -= 0.2f + radius;

		// Left
		if(Physics2D.Linecast(p1, p2, 1 << LayerMask.NameToLayer("Ground"))) {
			transform.position = new Vector3(transform.position.x + extents.x - radius, transform.position.y, transform.position.z);
			return true;
		}

		return true;
	}
}
