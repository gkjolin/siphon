using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerThrow : MonoBehaviour {

	public GameObject seed;
	[HideInInspector] public Camera mainCamera;
	
	public Plant[] slots;
	private List<Queue> slotQueues;

	public float throwForce = 700f;
	public float topDeadZone = 15f;
	public float bottomDeadZone = 15f;

	public Texture2D[] cursors;

	[HideInInspector] public int activeSlot;
	[HideInInspector] public bool throwable = true;
	
	private RaycastHit2D target;

	void Start() {
		mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		
		slotQueues = new List<Queue>();
		for(int i = 0; i < slots.Length; i++) {
			slotQueues.Add(new Queue());
		}

		StartCoroutine(SetInitialCursor());
	}

	private IEnumerator SetInitialCursor() {
		yield return new WaitForSeconds(.1f);
		if(cursors.Length > 0) {
			Screen.showCursor = true;
			//Cursor.SetCursor(cursors[0], Vector2.zero, CursorMode.Auto);
		}
	}

	public void AddSlotQueue() {
		slotQueues.Add(new Queue());
	}
	
	void Update () {
		if(slots.Length > 0) {
			bool targeting = Input.GetAxis("Seed Horizontal") != 0 || Input.GetAxis("Seed Vertical") != 0 || Input.GetMouseButton(0);
			bool throwing = Input.GetAxis("Seed Throw") != 0 || Input.GetMouseButtonUp(0);
			
			if(targeting && !throwing) {
				Vector3 playerPos = transform.position;
				BoxCollider2D box = GetComponent<BoxCollider2D>();
				playerPos.x += box.center.x * transform.lossyScale.x;
				playerPos.y += box.center.y * transform.lossyScale.y;
				
				Vector3 end;
				if(Input.GetMouseButton(0)) {
					Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
					Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, 0));
					float distance;
					xy.Raycast(ray, out distance);
					end = ray.GetPoint(distance);
				}
				else {
					end = playerPos + (Vector3)new Vector2(Input.GetAxis("Seed Horizontal"), Input.GetAxis("Seed Vertical")) * 10;
				}
				
				target = Physics2D.Linecast(playerPos, end, 1 << LayerMask.NameToLayer("Ground"));
				if(target) {
					end = target.point;
				}
				
				Debug.DrawLine(playerPos, end, slots[activeSlot].canPlant(target) ? Color.green : Color.red);
			}
			
			
			if(throwing && slots[activeSlot].canPlant(target)) {
				while(slotQueues[activeSlot].Count > 0) {
					Plant old = (Plant)slotQueues[activeSlot].Dequeue();
					Destroy(old.gameObject);
				}
				Plant plant = (Plant)Instantiate(slots[activeSlot], target.point, Quaternion.identity);
				slotQueues[activeSlot].Enqueue(plant);
				plant.grow(target);
				
				GetComponent<PlayerControl>().animator.Set("Throw", false, 1);
				
				target = default(RaycastHit2D);
			}
			
			selectSeed();
		}
	}
	
	private void selectSeed() {
		for(int i = 0; i < slots.Length; i++) {
			if(Input.GetKeyDown((i + 1).ToString())) {
				activeSlot = i;
				//Cursor.SetCursor(cursors[i], Vector2.zero, CursorMode.Auto);
				break;
			}
		}

		if(Input.GetAxis("Mouse ScrollWheel") > 0) {
			activeSlot = (activeSlot + 1) % slots.Length;
			//Cursor.SetCursor(cursors[activeSlot], Vector2.zero, CursorMode.Auto);
		}
		else if(Input.GetAxis("Mouse ScrollWheel") < 0) {
			activeSlot--;
			if(activeSlot < 0){activeSlot = slots.Length - 1;}
			//Cursor.SetCursor(cursors[activeSlot], Vector2.zero, CursorMode.Auto);
		}
	}
}
