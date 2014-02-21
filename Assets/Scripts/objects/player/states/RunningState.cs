using UnityEngine;
using System.Collections;

public class RunningState : PlayerState {
	public override void HandleInput(PlayerControl player) {
		if(!player.isGrounded() || Input.GetButtonDown("Jump")) {
			player.ChangeState(PlayerState.Jumping);
		}
		else if(player.isIdle()) {
			player.ChangeState(PlayerState.Idling);
		}
		
		GameObject interactable;
		if(interactable = player.isInteracting()) {
			PlayerState.Interacting.interactable = interactable;
			player.ChangeState(PlayerState.Interacting);
		}
	}

	public override void Update(PlayerControl player) {
		player.animator.TimeScale = (Mathf.Abs(player.rigidbody2D.velocity.x) / 8) + .3f;
		player.physics.Move();

		if(Input.GetAxisRaw("Horizontal") == 0) {
			player.rigidbody2D.velocity = new Vector2(0, player.rigidbody2D.velocity.y);
		}
		
		Vector2 normal = player.normal();
		if(normal != Vector2.zero) {
			Transform animation = player.transform.Find("Animation");
			if(player.transform.lossyScale.x < 0){normal.x *= -1;}
			if(Mathf.Abs(normal.x) > .5f){normal = new Vector2(.5f * Mathf.Sign(normal.x), normal.y);}
			animation.rotation = Quaternion.Lerp(animation.rotation, Quaternion.FromToRotation(Vector3.up, (Vector3) normal), 4 * Time.deltaTime);
		}

		player.physics.LedgeCompensate();
	}

	public override void Enter(PlayerControl player, PlayerState from) {
		player.animator.Set("Run", true);
	}
}