using UnityEngine;
using System.Collections;

public class LedgingState : PlayerState {

	public override void HandleInput(PlayerControl player) {
		if(Input.GetButtonDown("Jump") || (Input.GetAxis("Vertical") > 0 && Input.GetAxis("Vertical") < 1)) {
			player.ChangeState(PlayerState.Jumping);

			player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 0.1f);
			player.physics.Jump();
		}
		else if(Input.GetAxis("Vertical") < 0) {
			player.ChangeState(PlayerState.Falling);
		}
	}

	public override void Enter(PlayerControl player, PlayerState from) {
		player.rigidbody2D.isKinematic = true;
		player.animator.Set("Ledge");

		Vector2 p1 = (Vector2) player.transform.position;
		Vector2 p2 = (Vector2) player.transform.position;
		Vector2 scale = (Vector2) player.transform.lossyScale;
		BoxCollider2D box = player.GetComponent<BoxCollider2D>();
		p1 += Vector2.Scale(box.center, scale);
		p2 += Vector2.Scale(box.center, scale);

		p1.y += box.size.y * scale.y * .3f;
		p2.y += box.size.y * scale.y * .3f;
		p2.x += box.size.x * scale.x * 1.5f;

		RaycastHit2D cast = Physics2D.Linecast(p1, p2, (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("One-Way Ground")));
		float diff = cast.point.x - (player.transform.position.x + (box.size.x * scale.x * .5f));
		player.transform.position = new Vector3(player.transform.position.x + diff, player.transform.position.y, player.transform.position.z);
	}

	public override void Exit(PlayerControl player, PlayerState to) {
		player.rigidbody2D.isKinematic = false;
		player.animator.Set("PullUp");
	}
}