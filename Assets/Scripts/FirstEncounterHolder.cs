﻿using UnityEngine;
using System;

public class FirstEncounterHolder : MonoBehaviour {

	public GameObject plant;

	private bool interacted = false;
	private bool destroyed = false;
	
	void Update() {
		if(Input.GetButtonUp("Action") && !interacted) {
			// Check to see if children triggers have been activated -- the player is within range
			foreach(Transform child in transform) {
				// Each trigger contains a script that keeps triggered state
				FirstEncounterTrigger script = child.gameObject.GetComponent<FirstEncounterTrigger>();
				// Check if we have a child with the script and a triggered state
				if(script && script.triggered) {
					// Add plant to Player's inventory
					Debug.Log("Adding Plant");
					// Find the player
					GameObject player = GameObject.Find("Player");
					// Grab the PlayerThrow script
					PlayerThrow playerScript = player.GetComponent<PlayerThrow>();
					// Copy the new GameObject plant into the existing slots
					int slotLength = playerScript.slots.Length;
					GameObject[] newSlots = { plant };
					// A bit of Array magic for resizing and copying
					Array.Resize<GameObject>(ref playerScript.slots, slotLength + newSlots.Length);
					Array.Copy(newSlots, 0, playerScript.slots, slotLength, newSlots.Length);
					// Adjust the slot queues (Not sure what they are, but this helps)
					playerScript.AdjustSlotQueues();
					// Set an interacted state to avoid multiple slot additions
					interacted = true;
				}
			}
		}
		else if(interacted && !destroyed) {
			// Kill the seed to make it look a spiffy
			foreach(Transform child in transform) {
				if(child.gameObject.tag == "Seed") {
					Destroy(child.gameObject);
					destroyed = true;
				}
			}
		}
	}
}