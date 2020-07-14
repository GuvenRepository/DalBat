using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cardDistrubuter : MonoBehaviour {

    Vector2 myDeckStartingPoint = new Vector2(-2.0f, -4.0f);
    Vector2 myDeckCardGap = Vector2.zero;

    Vector2 secondDeckStartingPoint = new Vector2(3f, -1.5f);
    Vector2 secondDeckCardGap = Vector2.zero;

    Vector2 thirdDeckStartingPoint = new Vector2(2.0f, 4.0f);
    Vector2 thirdDeckCardGap = Vector2.zero;

    Vector2 fourthDeckStartingPoint = new Vector2(-3.0f, 1.5f);
    Vector2 fourthDeckCardGap = Vector2.zero;

    public GameObject card;
    public Transform distuributor;
    Vector2 groundPosition = new Vector2(1f, -2.3f);
    GameObject nextCard;

    short animSample=5;
    short cardCounter=0;
    float currentTime;
    short takeCardIndex = 48;

    List<short> cardIndexList = new List<short>();

    public static bool animating = false;

    bool start = true;
    public static bool cardTaken=false;


    private static Client client;

	// Use this for initialization
	void Start ()
    {

        client = FindObjectOfType<Client>();

        #region arragements
        currentTime = Time.time;
        myDeckCardGap.x = 4.0f / 11;
        secondDeckCardGap.y = 3.0f / 11;
        thirdDeckCardGap.x = -4.0f / 11;
        fourthDeckCardGap.y = -3.0f / 11;

        #endregion

        #region shuffle

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 52; j++)
            {
                cardIndexList.Add((short)(j));
            }
        }

        cardIndexList.Add(53);
        cardIndexList.Add(54);

        //cardIndexList.Shuffle();

        #endregion

    }
	
	// Update is called once per frame
	void Update ()
    {
        #region otherDecksDisturb
        if (start)
        {
            for (int i = 0; i < 12; i++)
            {
                nextCard = Instantiate(card, Vector2.zero, Quaternion.Euler(new Vector3(0f,0f,90f))) as GameObject;
                StartCoroutine(cardDisturbAnim(nextCard, Vector2.zero, secondDeckStartingPoint + secondDeckCardGap * i));
                nextCard.GetComponent<SpriteRenderer>().sortingOrder = i;
                gameManager.otherDecks[(Client.playerPosition) % 4 + 1].Add(nextCard.GetComponent<card>());
            }

            for (int i = 0; i < 12; i++)
            {
                nextCard = Instantiate(card, Vector2.zero, Quaternion.Euler(new Vector3(0f, 0f, 180f))) as GameObject;
                StartCoroutine(cardDisturbAnim(nextCard, Vector2.zero, thirdDeckStartingPoint + thirdDeckCardGap * i));
                nextCard.GetComponent<SpriteRenderer>().sortingOrder = i;
                gameManager.otherDecks[(Client.playerPosition + 1) % 4 + 1].Add(nextCard.GetComponent<card>());
            }

            for (int i = 0; i < 12; i++)
            {
                nextCard = Instantiate(card, Vector2.zero, Quaternion.Euler(new Vector3(0f, 0f, 270f))) as GameObject;
                StartCoroutine(cardDisturbAnim(nextCard, Vector2.zero, fourthDeckStartingPoint + fourthDeckCardGap * i));
                nextCard.GetComponent<SpriteRenderer>().sortingOrder = i;
                gameManager.otherDecks[(Client.playerPosition + 2) % 4 + 1].Add(nextCard.GetComponent<card>());
            }

            start = false;
        }
        #endregion

        #region myDeckDisturb
        if (Time.time - currentTime > 0.1f)
        {
            if (cardCounter < 12)
            {
                nextCard = Instantiate(card, distuributor.position, Quaternion.identity) as GameObject;
                StartCoroutine(cardDisturbAnim(nextCard, distuributor.position, myDeckStartingPoint + myDeckCardGap * cardCounter));
                nextCard.GetComponent<card>().cardIndex = cardIndexList[36+cardCounter];
                nextCard.GetComponent<card>().showFace();
                nextCard.transform.position += new Vector3(0f,0f,-cardCounter / 100.0f);
                gameManager.myDeck.Add(cardIndexList[36 + cardCounter]);
                gameManager.myDeckObjects.Add(nextCard.GetComponent<card>());

                cardCounter++;
                currentTime = Time.time;
            }

        }
        #endregion

    }

    private IEnumerator cardDisturbAnim(GameObject card, Vector2 start, Vector2 dest)
    {
        
        card.transform.position = start;
        card.transform.position += new Vector3(0f, 0f, -gameManager.myDeckObjects.IndexOf(card.GetComponent<card>()) / 100f);
        // distance between the waypoints
        Vector2 distance = dest - start;
        
        for (int i = 0; i < animSample ; i++)
        {
            //animation stuff
            yield return new WaitForSecondsRealtime(0.01f);
            card.transform.Translate(distance / animSample,Space.World); 
        }

        

        
    }

    public void takeCard(){
        if (!cardTaken)
        {
            nextCard = Instantiate(card, distuributor.position, Quaternion.identity) as GameObject;
            nextCard.GetComponent<card>().cardIndex = cardIndexList[takeCardIndex];
            nextCard.GetComponent<card>().showFace();
            gameManager.myDeck.Add(cardIndexList[takeCardIndex]);
            gameManager.myDeckObjects.Add(nextCard.GetComponent<card>());
            gameManager.deckArranger(Client.playerPosition);
            StartCoroutine(cardDisturbAnim(nextCard, distuributor.position, nextCard.transform.position));
            takeCardIndex++;
            cardTaken = true;
            client.Send("CTCRD");
        }
    }
    public void takeCard(int playerIndex)
    {
        nextCard = Instantiate(card, distuributor.position, Quaternion.identity) as GameObject;
        gameManager.otherDecks[playerIndex].Add(nextCard.GetComponent<card>());
        gameManager.deckArranger(playerIndex);
        StartCoroutine(cardDisturbAnim(nextCard, distuributor.position, nextCard.transform.position));
        takeCardIndex++;
    }


    public void takeGround()
    {
        if (!cardTaken)
        {
            foreach (card card in gameManager.groundCards)
            {
                gameManager.myDeck.Add(card.cardIndex);
                gameManager.myDeckObjects.Add(card);
                gameManager.deckArranger(Client.playerPosition);

            }

            foreach (card card in gameManager.groundCards)
            {
                StartCoroutine(cardDisturbAnim(card.gameObject, groundPosition, card.transform.position));
            }

            gameManager.groundCards.Clear();
            cardTaken = true;
            client.Send("CTGND");
        }
        
    }
    public void takeGround(int playerIndex)
    {
        foreach (card card in gameManager.groundCards)
        {
            card.hideFace();
            gameManager.otherDecks[playerIndex].Add(card);
            gameManager.deckArranger(playerIndex);
        }

        foreach (card card in gameManager.groundCards)
        {
            StartCoroutine(cardDisturbAnim(card.gameObject, groundPosition, card.transform.position));
        }

        gameManager.groundCards.Clear(); 
    }

}
static class RandomExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}