using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class card : MonoBehaviour {
    public short cardIndex=52;
    public Sprite[] faces;
    public Sprite backFace;
    cardDistrubuter CD;
    gameManager GM;

    public bool followPointer = false;

    public static float mouseDownTime = 0f;

    void Start()
    {
        CD = FindObjectOfType<cardDistrubuter>();
        GM = FindObjectOfType<gameManager>();
    }

    void Update()
    {
        if (followPointer)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = Vector2.Lerp(transform.position, mousePos + new Vector2(gameManager.selectedCards.IndexOf(this)*4f/11,0f), 1f / 20);
        }
    }

    public void showFace()
    {
        GetComponent<SpriteRenderer>().sprite = faces[cardIndex];
    }

    public void showFace(short index)
    {
        GetComponent<SpriteRenderer>().sprite = faces[index];
    }

    public void hideFace()
    {
        GetComponent<SpriteRenderer>().sprite = backFace;
    }

    void OnMouseDown()
    {
        if ((Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            mouseDownTime = Time.time;

            if (gameManager.myDeckObjects.Contains(this))
            {
                GM.selectCard(this);
            }
            else if (CD.distuributor == this.transform)
            {
                if (gameManager.yourTurn)
                    CD.takeCard();
            }
            else if (gameManager.groundCards.Contains(this))
            {
                //take ground
                if (gameManager.yourTurn)
                    CD.takeGround();
            }
        }
    }


    void OnMouseUp()
    {
        if ((Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            mouseDownTime = -1f;
            
        }
    }
}
