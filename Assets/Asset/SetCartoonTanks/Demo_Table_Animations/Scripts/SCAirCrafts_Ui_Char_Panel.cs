﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//creates the buttons on panel of animations
public class SCAirCrafts_Ui_Char_Panel : MonoBehaviour {

	public GameObject character;
	public Transform acts_table;
 
    public Button buttonPrefab;

    Button sel_btm;

	SCAirCrafts_Actions actions;


	void Start () {
 
		actions = character.GetComponent<SCAirCrafts_Actions> ();
 


		CreateActionButton("Idle");
		CreateActionButton("TakeOff");
		CreateActionButton("Flight");
		CreateActionButton("Dead1");
		CreateActionButton("Dead2");

 

  
 
    }

 

    void CreateActionButton(string name) {
		CreateActionButton(name, name);
	}

 
	void CreateActionButton(string name, string message) {

		Button button = CreateButton (name, acts_table);

		if (name == "Idle")
		{
			sel_btm = button;
			button.GetComponentInChildren<Image>().color = new Color(.5f, .5f, .5f);
		}
		button.GetComponentInChildren<Text>().fontSize = 12;
		button.onClick.AddListener(()  => actions.SendMessage(message, SendMessageOptions.DontRequireReceiver));
		button.onClick.AddListener(() => select_btm(button)  );


	}
    void select_btm(Button btm)
    {
		sel_btm.GetComponentInChildren<Image>().color = new Color(.345f, .345f, .345f);
		btm.GetComponentInChildren<Image>().color = new Color(.5f, .5f, .5f);
        sel_btm = btm;
    }

 
    Button CreateButton(string name, Transform group) {
		GameObject obj = (GameObject) Instantiate (buttonPrefab.gameObject);
		obj.name = name;
		obj.transform.SetParent(group);
		obj.transform.localScale = Vector3.one;
		Text text = obj.transform.GetChild (0).GetComponent<Text> ();
		text.text = name;
		return obj.GetComponent<Button> ();
	}




	public void OpenPublisherPage() {
		Application.OpenURL ("https://assetstore.unity.com/packages/3d/vehicles/land/set-of-cartoon-tanks-105083");
	}

 
}
