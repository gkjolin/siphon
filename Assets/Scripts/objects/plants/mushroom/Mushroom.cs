﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EntityAudio))]

public class Mushroom : Plant {

	public float bounceForce = 20f;
	public float horizontalForce = 0;
	public bool adjustTrigger = true;

	private float bounceTimer = 0;
	private MushroomAnimator animator;

	void Start() {
		animator = GetComponent<MushroomAnimator>();
	}

	public override void grow(RaycastHit2D cast) {
		if(!cast){return;}

		transform.rotation = Quaternion.FromToRotation(Vector3.up, (Vector3)cast.normal);
		transform.parent = cast.transform;
	}

	void Update() {
		if(bounceTimer > 0) {
			bounceTimer -= Mathf.Min(Time.deltaTime, bounceTimer);
		}

		if(adjustTrigger) {
			BoxCollider2D box = GetComponent<BoxCollider2D>();
			GameObject player = GameObject.Find("Player");
			if(false && player.GetComponent<PlayerControl>().isGrounded()) {
				box.size = new Vector2(250, 10);
				box.center = new Vector2(35 * player.rigidbody2D.velocity.x, 10);
			}
			else {
				box.size = new Vector2(400, 20 + (10 * Mathf.Abs(player.rigidbody2D.velocity.y)));
				box.center = new Vector2(0, 10 + (5 * Mathf.Abs(player.rigidbody2D.velocity.y)));
			}
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		Bounceable bounceable = col.gameObject.GetComponent<Bounceable>();

		if(bounceable && col.gameObject.rigidbody2D && bounceTimer == 0) {
			float bf = (bounceable.bounceForce != 0) ? bounceable.bounceForce : bounceForce;
			float component = col.gameObject.rigidbody2D.velocity.x;

			if(Mathf.Abs(col.gameObject.rigidbody2D.velocity.y) < bf) {
				col.gameObject.rigidbody2D.velocity = new Vector2(component, bf);
			}
			else {
				col.gameObject.rigidbody2D.velocity = new Vector2(component, Mathf.Abs(col.gameObject.rigidbody2D.velocity.y));
			}

			if(animator){animator.Set("Bounce");}
			EntityAudio audio = GetComponent<EntityAudio>();
			audio.One("Mushroom_Bounce");

			col.gameObject.SendMessage("Bounced", false);
			bounceTimer = 0.1f;
		}
	}

	public override bool canPlant(RaycastHit2D cast) {
		if(!cast || cast.normal.y < 0){return false;}
		if(
			(cast.transform.parent != null && cast.transform.parent.gameObject.GetComponent<Unplantable>() != null) ||
			(cast.transform.gameObject.GetComponent<Unplantable>() != null)
		) {
			return false;
		}

		Vector3 p1 = cast.point;
		Vector3 p2 = cast.point;

		p1.y -= .01f;
		p2.y -= .01f;

		p1.y += .1f;
		p2.y -= .2f;

		p1.x -= .1f;
		p2.x -= .1f;
		Debug.DrawLine(p1, p2, Color.blue);
		cast = Physics2D.Linecast(p1, p2, 1 << LayerMask.NameToLayer("Ground"));
		if(!cast || cast.fraction == 0) {
			return false;
		}

		p1.x += .1f;
		p2.x += .1f;
		Debug.DrawLine(p1, p2, Color.blue);
		cast = Physics2D.Linecast(p1, p2, 1 << LayerMask.NameToLayer("Ground"));
		if(!cast || cast.fraction == 0) {
			return false;
		}

		p1.x += .1f;
		p2.x += .1f;
		Debug.DrawLine(p1, p2, Color.blue);
		cast = Physics2D.Linecast(p1, p2, 1 << LayerMask.NameToLayer("Ground"));
		if(!cast || cast.fraction == 0) {
			return false;
		}

		return true;
	}
}