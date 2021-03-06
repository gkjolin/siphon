using UnityEngine;
using System;
using System.Collections;

public class PlayerPhysics : MonoBehaviour {
	[HideInInspector] public bool facingRight = true;

	public float moveForce = 190f;
	public float maxSpeed = 5f;
	public float jumpForce = 13;
	public float dangerousVelocity = 35;
	public float lethalVelocity = 85;
	public float ledgeDuration = .4f;
	public float alignSpeed = 4;
	public PhysicsMaterial2D groundedMaterial;
	public PhysicsMaterial2D airborneMaterial;

	[HideInInspector] public float timeSinceFall = 0;
	[HideInInspector] public bool disableControl = false;

	private PlayerAnimator animator;

	public void Start() {
		animator = transform.Find("Animation").GetComponent<PlayerAnimator>();
	}

	public void Move(float factor = 1.0f) {
		if(disableControl){return;}

		bool grounded = GetComponent<PlayerControl>().isGrounded();
		if(grounded) {
			Vector2 normal = GetComponent<PlayerControl>().normal();
			if(normal != Vector2.zero && normal.y < 1 && ((facingRight && normal.x < 0) || (!facingRight && normal.x > 0))) {
				factor = (3 * Mathf.Cos(normal.y));
			}

			foreach(Collider2D col in GetComponents<Collider2D>()) {
				col.sharedMaterial = groundedMaterial;
				col.enabled = false;
				col.enabled = true;
			}
		}
		else {
			factor *= 0.4f;

			foreach(Collider2D col in GetComponents<Collider2D>()) {
				col.sharedMaterial = airborneMaterial;
				col.enabled = false;
				col.enabled = true;
			}
		}

		float h = Input.GetAxisRaw("Horizontal");

		if(h != 0) {
			rigidbody2D.AddForce(Vector2.right * h * moveForce * factor);
		}

		if(Mathf.Abs(rigidbody2D.velocity.x) > maxSpeed) {
			rigidbody2D.velocity = new Vector2(Mathf.Sign(rigidbody2D.velocity.x) * maxSpeed, rigidbody2D.velocity.y);
		}

		if((h > 0 && !facingRight) || (h < 0 && facingRight)) {
			ChangeDirection();
		}
	}

	public void ChangeDirection() {
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void Climb(GameObject ladder) {
		if(disableControl){return;}

		float v = Input.GetAxis("Vertical");
		float h = Input.GetAxis("Horizontal");
		if(h != 0 && Mathf.Sign(h) != Mathf.Sign(transform.localScale.x) && ladder.GetComponent<Climbable>().direction == 0) {
			ChangeDirection();
			transform.position = new Vector3(ladder.transform.position.x + Mathf.Sign(transform.lossyScale.x) * -.3f, transform.position.y, transform.position.z);
		}

		Vector2 p = (Vector2)transform.position;
		Vector2 scale = (Vector2)transform.lossyScale;
		BoxCollider2D box = GetComponent<BoxCollider2D>();

		p += Vector2.Scale(box.center, scale);

		Climbable climbable = ladder.GetComponent<Climbable>();
		rigidbody2D.velocity = Vector2.zero;
		if(Mathf.Sign(v) > 0) {
			if(ladder.name == "GrappleVine"){p.y += (box.size.y * scale.y) * 1.3f;}
			if(p.y < climbable.endPoint.y) {
				rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, (v * (maxSpeed / 2)));
			}
		}
		else {
			p = (Vector2)transform.position;
			if(p.y > climbable.startPoint.y) {
				rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, (v * (maxSpeed / 2)));
			}
			else {
				PlayerControl playerControl = GetComponent<PlayerControl>();
				playerControl.ChangeState(PlayerState.Idling);
			}
		}

		Debug.DrawLine(p - Vector2.right, p + Vector2.right, Color.green);
	}

	public void Interact(GameObject interactable) {
		if(disableControl){return;}

		var sign = Mathf.Sign(Input.GetAxisRaw("Horizontal"));
		var direction = (facingRight) ? 1 : -1;
		var script = interactable.GetComponent<Interactable>();
		float force = script.force;

		// Check to see if we are allowed to push or pull in that direction.
		if(sign == direction && !script.push || sign == -direction && !script.pull) {
			// If we aren't, make the interactable item immovable.
			interactable.rigidbody2D.mass = script.staticWeight;
			return;
		}

		interactable.rigidbody2D.mass = script.dynamicWeight;

		var velocity = new Vector2(sign * 3, 0);

		if(GetComponent<PlayerControl>().isInteracting() && Input.GetAxisRaw("Horizontal") != 0 && velocity != Vector2.zero) {

			// push
			if(Input.GetAxisRaw("Horizontal") == Mathf.Sign(transform.lossyScale.x)) {
				if(!script.pushing) {
					GetComponent<PlayerControl>().animator.Set("Push", true);
					script.pushing = true;
					script.pulling = false;
				}
				GetComponent<PlayerControl>().animator.TimeScale = Input.GetAxis("Horizontal");

				if(script.movePlayer) {
					rigidbody2D.AddForce(new Vector2(sign * force, 0));
					interactable.rigidbody2D.AddForce(new Vector2(sign * force, 0));
				}
				else {
					interactable.rigidbody2D.AddForceAtPosition(new Vector2(sign * force, 0), interactable.transform.position + script.forceOffsetY * Vector3.up);
				}

				if(Mathf.Abs(rigidbody2D.velocity.x) > Mathf.Abs(velocity.x)){rigidbody2D.velocity = velocity;}
				if(script.movePlayer) {
					if(Mathf.Abs(interactable.rigidbody2D.velocity.x) > Mathf.Abs(velocity.x)){interactable.rigidbody2D.velocity = velocity;}
				}
			}

			// pull
			else if(Input.GetAxisRaw("Horizontal") != Mathf.Sign(transform.lossyScale.x)) {
				if(!script.pulling) {
					GetComponent<PlayerControl>().animator.Set("Pull", true);
					script.pulling = true;
					script.pushing = false;
				}
				GetComponent<PlayerControl>().animator.TimeScale = Input.GetAxis("Horizontal");

				interactable.rigidbody2D.AddForce(new Vector2(sign * force * 4, 0));

				if(Mathf.Abs(rigidbody2D.velocity.x) > Mathf.Abs(velocity.x)){rigidbody2D.velocity = velocity;}
				if(Mathf.Abs(interactable.rigidbody2D.velocity.x) > Mathf.Abs(velocity.x)){interactable.rigidbody2D.velocity = velocity;}
			}
		}
	}

	public void Jump() {
		if(disableControl){return;}
		rigidbody2D.isKinematic = false;
		rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, jumpForce);
	}

	// Black magic that prevents you from sliding off corner of platform.
	public void LedgeCompensate() {
		Vector2 scale = (Vector2) transform.lossyScale;
		CircleCollider2D circle = GetComponent<CircleCollider2D>();
		Vector2 p1 = Vector2.Scale(circle.center, scale) + (Vector2) transform.position;
		Vector2 p2 = p1;

		p2.y -= circle.radius * scale.y + 0.07f;

		p1.x -= circle.radius * Mathf.Abs(scale.x);
		p2.x -= circle.radius * Mathf.Abs(scale.x);
		RaycastHit2D left = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));

		p1.x += circle.radius * Mathf.Abs(scale.x) * 2;
		p2.x += circle.radius * Mathf.Abs(scale.x) * 2;
		RaycastHit2D right = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));

		if(left && !right) {
			rigidbody2D.AddForce(new Vector2(-20 * Mathf.Pow(2, -(timeSinceFall + .6f)), 0));
		}

		if(right && !left) {
			rigidbody2D.AddForce(new Vector2(20 * Mathf.Pow(2, -(timeSinceFall + .6f)), 0));
		}

		timeSinceFall += Time.deltaTime;
	}

	public void AlignUpright() {
		Transform animation = transform.Find("Animation");
		animation.rotation = Quaternion.Lerp(animation.rotation, Quaternion.FromToRotation(Vector3.up, Vector3.up), alignSpeed * Time.deltaTime);
	}

	public void AlignSlope() {
		Vector2 normal = GetComponent<PlayerControl>().normal();
		if(normal != Vector2.zero) {
			Transform animation = transform.Find("Animation");
			if(transform.lossyScale.x < 0){normal.x *= -1;}
			if(Vector2.Angle(Vector2.up, normal) > 45) {
				normal = new Vector2(Mathf.Deg2Rad * Mathf.Cos(45) * Mathf.Sign(normal.x), Mathf.Deg2Rad * Mathf.Sin(45));
			}
			animation.rotation = Quaternion.Lerp(animation.rotation, Quaternion.FromToRotation(Vector3.up, (Vector3) normal), alignSpeed * Time.deltaTime);
		}
	}

	public void Slide() {}
}
