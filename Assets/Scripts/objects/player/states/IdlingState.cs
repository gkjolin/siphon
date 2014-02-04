using UnityEngine;
using System.Collections;

public class IdlingState : PlayerState {

	public override void HandleInput(PlayerControl player) {
		if(player.isRunning()) {
			player.ChangeState(PlayerState.Running);
		}

		if(!player.isGrounded() || Input.GetButtonDown("Jump")) {
			player.ChangeState(PlayerState.Jumping);
		}
	}
	
	public override void Update(PlayerControl player) {
		Vector2 normal = player.normal();
		if(normal != Vector2.zero) {
			Transform animation = player.transform.Find("Animation");
			if(player.transform.lossyScale.x < 0){normal.x *= -1;}
			animation.rotation = Quaternion.Lerp(animation.rotation, Quaternion.FromToRotation(Vector3.up, (Vector3) normal), 4 * Time.deltaTime);
		}
	}

	public override void Enter(PlayerControl player, PlayerState from) {
		player.animator.Add("Idle", true);
	}
}