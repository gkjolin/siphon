﻿using UnityEngine;
using System.Collections;
using Spine;

public class PlayerControl : MonoBehaviour {

	[HideInInspector] public SkeletonAnimation skeletonAnimation;
	[HideInInspector] public GameObject climbing = null;

	[HideInInspector] public PlayerAnimator animator;
	[HideInInspector] public PlayerAudio mozart;
	[HideInInspector] public PlayerState state;
	[HideInInspector] public PlayerPhysics physics;

	private bool climbFlag = true;

	void Start() {
		physics = GetComponent<PlayerPhysics>();
		animator = transform.Find("Animation").GetComponent<PlayerAnimator>();
		mozart = GetComponent<PlayerAudio>();

		ChangeState(PlayerState.Idling);
	}

	void Update() {
		if(Input.GetKeyDown(KeyCode.R)){Application.LoadLevel(Application.loadedLevel);}
		state.HandleInput(this);
	}

	void FixedUpdate() {
		state.Update(this);
	}

	public void ChangeState(PlayerState next) {
		PlayerState previous = this.state;
		if(previous != null) {
			previous.Exit(this, next);
		}

		this.state = next;
		this.state.Enter(this, previous);
	}
	
	public Vector2 normal() {
		Vector2 scale = (Vector2) transform.lossyScale;
		CircleCollider2D circle = GetComponent<CircleCollider2D>();
		Vector2 p1 = Vector2.Scale(circle.center, scale) + (Vector2) transform.position;
		Vector2 p2 = p1;

		p2.y -= circle.radius * scale.y + 0.07f;
		
		RaycastHit2D center = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));
		if(center){return center.normal;}

		p1.x -= circle.radius * scale.x;
		p2.x -= circle.radius * scale.x;
		RaycastHit2D left = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));
		if(left){return left.normal;}

		p1.x += circle.radius * scale.x * 2;
		p2.x += circle.radius * scale.x * 2;
		RaycastHit2D right = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));
		if(right){return right.normal;}

		return Vector2.zero;
	}
	
	public bool isGrounded() {

		// Can probably cache most of this (sans raycasts)
		Vector2 scale = (Vector2) transform.lossyScale;
		CircleCollider2D circle = GetComponent<CircleCollider2D>();
		Vector2 p1 = Vector2.Scale(circle.center, scale) + (Vector2) transform.position;
		Vector2 p2 = p1;

		p2.y -= circle.radius * scale.y + 0.07f;

		p1.x -= circle.radius * scale.x;
		p2.x -= circle.radius * scale.x;
		//Debug.DrawLine(p1, p2, Color.red);
		RaycastHit2D left = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));
		if(left){return true;}

		p1.x += circle.radius * scale.x;
		p2.x += circle.radius * scale.x;
		//Debug.DrawLine(p1, p2, Color.red);
		RaycastHit2D center = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));
		if(center){return true;}

		p1.x += circle.radius * scale.x;
		p2.x += circle.radius * scale.x;
		//Debug.DrawLine(p1, p2, Color.red);
		RaycastHit2D right = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));
		if(right){return true;}

		return false;
	}
	
	public bool isIdle() {
		return isGrounded() && Input.GetAxisRaw("Horizontal") == 0;
	}

	public bool isRunning() {
		return isGrounded() && Input.GetAxisRaw("Horizontal") != 0;
	}

	public bool canLedgeGrab() {
		Vector2 p1 = (Vector2) transform.position;
		Vector2 p2 = (Vector2) transform.position;
		Vector2 scale = (Vector2) transform.lossyScale;
		BoxCollider2D box = GetComponent<BoxCollider2D>();
		p1 += Vector2.Scale(box.center, scale);
		p2 += Vector2.Scale(box.center, scale);

		p2.y -= box.size.y * scale.y * 1.3f;
		//Debug.DrawLine(p1, p2, Color.green);
		if(Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")))) {
			return false;
		}

		p2.y = p1.y;
		p1.y += box.size.y * scale.y * .3f;
		p2.y += box.size.y * scale.y * .3f;
		p2.x += box.size.x * scale.x * .8f;

		//Debug.DrawLine(p1, p2, Color.blue);
		
		if(Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")))) {
			p1.y += box.size.y * scale.y * .25f;
			p2.y += box.size.y * scale.y * .25f;
			if(!Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")))) {
				return true;
			}
		}
		else {
			p1.y += box.size.y * scale.y * .25f;
			p2.y += box.size.y * scale.y * .25f;
		
			//Debug.DrawLine(p1, p2, Color.blue);
		}

		return false;
	}
	
	public bool canClimb() {
		GameObject[] ladders = GameObject.FindGameObjectsWithTag("Ladder");
		bool ladder = false;
		foreach(GameObject obj in ladders) {
			Climbable climbable = obj.GetComponent<Climbable>();
			RaycastHit2D cast = Physics2D.Linecast(climbable.startPoint, climbable.endPoint, 1 << LayerMask.NameToLayer("Player"));
			if(cast) {
				ladder = true;
				break;
			}
		}
		
		if(!ladder){climbFlag = true; return false;}
		else {
			if(state == PlayerState.Climbing){return true;}
			if(rigidbody2D.velocity.y < 0){climbFlag = true;}
			if(climbFlag){climbFlag = false; return true;}
			return false;
		}
	}
}
