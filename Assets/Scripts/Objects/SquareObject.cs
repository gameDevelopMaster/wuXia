using UnityEngine;
using System.Collections;

public class SquareObject : MonoBehaviour 
{
	public Square squareScript;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void OnPress(bool isPressed)
	{
		//Set selected item to this item script
		if(isPressed && MapEditor.Instance != null)
		{
			MapEditor.Instance.ClickOnSquare(squareScript);
		}
	}
}
