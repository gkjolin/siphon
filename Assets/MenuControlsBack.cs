﻿using UnityEngine;
using System.Collections;

public class MenuBack : MonoBehaviour {

	void MenuMouseClick() {
		SendMessageUpwards("MenuChangePage", "Previous");
	}
}
