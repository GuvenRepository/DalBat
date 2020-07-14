using UnityEngine;
using System.Collections;

public class miniCard : MonoBehaviour {

    public short cardIndex = 52;
    public Sprite[] faces;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void showFace()
    {
        GetComponent<SpriteRenderer>().sprite = faces[cardIndex];
    }
}
