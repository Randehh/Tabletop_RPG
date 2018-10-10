using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCube : MonoBehaviourPun {

	void Start () {
		
	}

	void Update () {
        if (!photonView.IsMine) return;

        if (Input.GetKey(KeyCode.A)) transform.Translate(-10 * Time.deltaTime, 0, 0);
        if (Input.GetKey(KeyCode.D)) transform.Translate(10 * Time.deltaTime, 0, 0);
    }
}
