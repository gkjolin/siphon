﻿using UnityEngine;
using System.Collections;

public class GrappleVine : MonoBehaviour {

	private Vector3 startPoint;
	private Vector3 endPoint;
	private bool dirty;
	private PlayerMovement climbing = null;

	// Use this for initialization
	void Start () {
		startPoint = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
		Vector3 up = transform.position;
		up.y += 1000;
		RaycastHit2D cast = Physics2D.Linecast(transform.position, up, 1 << LayerMask.NameToLayer("Ground"));
		endPoint = cast.point;
		
		if(!cast || Vector3.Distance(startPoint, endPoint) > 10) {
			DestroyObject(gameObject);
		}
		
		dirty = false;
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine(startPoint, endPoint, Color.red);
		if(climbing == null) {
			RaycastHit2D cast = Physics2D.Linecast(startPoint, endPoint, 1 << LayerMask.NameToLayer("Player"));
			if(cast) {
				PlayerMovement player = cast.rigidbody.gameObject.GetComponent("PlayerMovement") as PlayerMovement;
				if(!dirty) {
					player.state = PlayerMovement.PlayerState.CLIMBING;
					climbing = player;
					dirty = true;
				}
			}
		}
		else {
			RaycastHit2D cast = Physics2D.Linecast(startPoint, endPoint, 1 << LayerMask.NameToLayer("Player"));
			PlayerMovement player = climbing.gameObject.rigidbody2D.GetComponent("PlayerMovement") as PlayerMovement;
			if(!cast) {
				player.state = PlayerMovement.PlayerState.JUMPING;
				climbing = null;
				dirty = false;
			}
			else if(player.gameObject.rigidbody2D.velocity.y < 0) {
				dirty = false;
				climbing = null;
			}
		}
	}
}