using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class gameManager : MonoBehaviour {

    public static List<short> myDeck = new List<short>();
    public static List<card> myDeckObjects = new List<card>();
    public static List<card> selectedCards = new List<card>();
    public static List<card> groundCards = new List<card>();

    public static Dictionary<int, List<card>> otherDecks = new Dictionary <int, List<card>>();
    public static Dictionary<int, List<miniCard>> openedCards = new Dictionary<int, List<miniCard>>();

    private static Client client;

    short animSample = 5;
    public static bool yourTurn = false;
    bool openedBefore = false;

    public Text[] names;

    public Transform[] cardOpenRectancle = new Transform[8];
    public static bool followingPointer = false;
    public GameObject miniCard;

	void Start () {
        client = FindObjectOfType<Client>();
        otherDecks.Add(1, new List<card>());
        otherDecks.Add(2, new List<card>());
        otherDecks.Add(3, new List<card>());
        otherDecks.Add(4, new List<card>());
        openedCards.Add(1, new List<miniCard>());
        openedCards.Add(2, new List<miniCard>());
        openedCards.Add(3, new List<miniCard>());
        openedCards.Add(4, new List<miniCard>());

        client.GM = this;
        printNames();
	}
	
	void Update () {

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (mousePos.x > cardOpenRectancle[0].position.x &&
                mousePos.x < cardOpenRectancle[7].position.x &&
                mousePos.y > cardOpenRectancle[7].position.y &&
                mousePos.y < cardOpenRectancle[0].position.y
                && canOpen())
            {
                if (followingPointer && yourTurn )
                {
                    //openCards
                    openCards();
                }
            }
            else if (followingPointer)
            {
                deckArranger(Client.playerPosition);
                followingPointer = false;
                foreach (card c in selectedCards)
                {
                    c.followPointer = false;
                }
            }
                
           
        }

	}

    public void sortDeck()
    {
        foreach (card card in selectedCards.ToArray())
        {
            card.transform.Translate(new Vector3(0f, -0.5f, 0f));
        }
        selectedCards.Clear();

        myDeck.Sort();
        myDeck.Reverse();

        for (int i = 0; i < myDeckObjects.Count; i++)
        {
            myDeckObjects[i].cardIndex = myDeck[i];
        }

        for (int i = 0; i < myDeckObjects.Count; i++)
        {
            myDeckObjects[i].GetComponent<card>().showFace(myDeck[i]);
        }
        

    }

    public void selectCard(card card){

        
        if (myDeckObjects.Contains(card))
        {
            if (!selectedCards.Contains(card))
            {
                card.transform.Translate(new Vector3(0f, 0.5f, 0f));
                selectedCards.Add(card);
            }
            else
            {
                StartCoroutine(holdDetector(card));
            }

        } 
        
        
    }

    private bool checkSequences()
    {
        List<short> selectedIndexes = new List<short>();

        List<short> selectedClubs = new List<short>();
        List<short> selectedSpades = new List<short>();
        List<short> selectedDiamonds = new List<short>();
        List<short> selectedHearts = new List<short>();

        int totalPers=0;

        foreach (card card in selectedCards)
        {
            selectedIndexes.Add(card.cardIndex);
        }

        selectedIndexes.Sort();

        #region seperation to shapes
        for (int i = 0; i < selectedIndexes.Count; i++)
        {
            if (selectedIndexes[i] < 13)
                selectedClubs.Add(selectedIndexes[i]);

            else if (selectedIndexes[i] < 26)
                selectedSpades.Add(selectedIndexes[i]);

            else if (selectedIndexes[i] < 39)
                selectedDiamonds.Add(selectedIndexes[i]);

            else if (selectedIndexes[i] < 52)
                selectedHearts.Add(selectedIndexes[i]);
        }
        #endregion

        #region counting pers

        //totalPers = perListforShape(selectedClubs).Count + perListforShape(selectedSpades).Count +
        //        perListforShape(selectedDiamonds) + perCount(selectedHearts);

        #endregion

        if (totalPers > 0)
            return true;
        else
            return false;
    }

    
    public static void deckArranger(int playerIndex)
    {
        if (playerIndex == Client.playerPosition)
        {
            Vector2 deckPosition = new Vector2(0f,-4f);
            Vector2 myDeckCardGap = new Vector2(4.0f/11, 0);
            for (int i=0; i < myDeckObjects.Count; i++)
            {
                Vector2 newPosition = (deckPosition + myDeckCardGap * i) - (myDeckCardGap * (myDeckObjects.Count - 1) / 2);
                myDeckObjects[i].transform.position = new Vector3(newPosition.x, newPosition.y, -i / 100f);
            }
            foreach (card card in selectedCards)
            {
                card.transform.Translate(new Vector3(0f, 0.5f, 0f));
            }
        }
        else if (playerIndex == Client.playerPosition % 4 + 1)
        {
            Vector2 deckPosition = new Vector2(3f, 0f);
            Vector2 DeckCardGap = new Vector2(0, 3.0f / 11);
            for (int i = 0; i < otherDecks[playerIndex].Count; i++)
            {
                Vector2 newPosition = (deckPosition + DeckCardGap * i) - (DeckCardGap * (otherDecks[playerIndex].Count - 1) / 2);
                otherDecks[playerIndex][i].gameObject.GetComponent<SpriteRenderer>().sortingOrder = i;
                otherDecks[playerIndex][i].transform.position = new Vector3(newPosition.x, newPosition.y, 0f);
                otherDecks[playerIndex][i].transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            }
        }
        else if (playerIndex == (Client.playerPosition + 1) % 4 + 1)
        {
            Vector2 deckPosition = new Vector2(0f, 4f);
            Vector2 DeckCardGap = new Vector2(-4.0f / 11, 0f);
            for (int i = 0; i < otherDecks[playerIndex].Count; i++)
            {
                Vector2 newPosition = (deckPosition + DeckCardGap * i) - (DeckCardGap * (otherDecks[playerIndex].Count - 1) / 2);
                otherDecks[playerIndex][i].gameObject.GetComponent<SpriteRenderer>().sortingOrder = i;
                otherDecks[playerIndex][i].transform.position = new Vector3(newPosition.x, newPosition.y, 0f);
                otherDecks[playerIndex][i].transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            }
        }
        else if (playerIndex == (Client.playerPosition + 2) % 4 + 1)
        {
            Vector2 deckPosition = new Vector2(-3f, 0f);
            Vector2 DeckCardGap = new Vector2(0, -3.0f / 11);
            for (int i = 0; i < otherDecks[playerIndex].Count; i++)
            {
                Vector2 newPosition = (deckPosition + DeckCardGap * i) - (DeckCardGap * (otherDecks[playerIndex].Count - 1) / 2);
                otherDecks[playerIndex][i].gameObject.GetComponent<SpriteRenderer>().sortingOrder = i;
                otherDecks[playerIndex][i].transform.position = new Vector3(newPosition.x, newPosition.y, 0f);
                otherDecks[playerIndex][i].transform.localEulerAngles = new Vector3(0f, 0f, 270f);
            }
        }
    }

    public void throwCard(card card)
    {
        card.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        StartCoroutine(throwCard(card.gameObject));

        if (groundCards.Count != 0)
        {
            groundCards[groundCards.Count - 1].transform.position = new Vector3(1f, -2.3f, 1f / groundCards.Count);
        }
        groundCards.Add(card);

        
        selectedCards.Remove(card);
        myDeckObjects.Remove(card);
        myDeck.Remove(card.cardIndex);
        client.Send("CTHRW|" + card.cardIndex.ToString());
        yourTurn = false;
        
        deckArranger(Client.playerPosition);
        if (Client.playerPosition != 4 )
            positionToText(Client.playerPosition + 1).color = Color.green;
        else
            positionToText(1).color = Color.green;


    }
    public void throwCard(int playerPosition, short cardIndex)
    {
        int random =Random.Range(0, otherDecks[playerPosition].Count - 1);
        card card = otherDecks[playerPosition][random];
        card.cardIndex = cardIndex;
        card.transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
        card.showFace();
        card.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        otherDecks[playerPosition].RemoveAt(random);
        StartCoroutine(throwCard(card.gameObject));

        
        if (groundCards.Count != 0)
        {
            groundCards[groundCards.Count - 1].transform.position = new Vector3(1f, -2.3f, 1f / groundCards.Count);
        }
        groundCards.Add(card);

        deckArranger(playerPosition);

        positionToText(playerPosition).color = Color.white;
        if (playerPosition != 4 && playerPosition % 4 + 1 != Client.playerPosition)
            positionToText(playerPosition + 1).color = Color.green;
        else if (playerPosition == 4 && Client.playerPosition != 1)
            positionToText(1).color = Color.green;

        
        
    }

    private IEnumerator throwCard(GameObject card)
    {
        Vector2 start = card.transform.position;
        Vector2 dest = new Vector2(1f, -2.3f);
        // distance between the waypoints
        Vector2 distance = dest - start;

        for (int i = 0; i < animSample; i++)
        {
            //animation stuff
            yield return new WaitForSecondsRealtime(0.01f);
            card.transform.Translate(distance / animSample, Space.World);
        }
    
    }

    public void printNames(){
        foreach (GameClient c in client.otherPlayers)
        {
            positionToText(c.playerPosition).text = c.name;
        }
    }
    private Text positionToText(int position)
    {
        if (Client.playerPosition == position % 4 + 1)
        {
            return names[2];
        }
        else if (Client.playerPosition == (position + 1) % 4 + 1)
        {
            return names[1];
        }
        else if (Client.playerPosition == (position + 2) % 4 + 1)
        {
            return names[0];
        }

        return null;
    }

    List<short> extraSelectedCardIndexes = new List<short>();

    private List<List<short>> getPerListforShape(List<short> shapeList)
    {
        extraSelectedCardIndexes = new List<short>();
        bool gapFound = false;
        List<short> tempList = new List<short>();
        List<List<short>> perList = new List<List<short>>();



        for (int i = 0; i < shapeList.Count; i++)
        {
            tempList.Add(shapeList[i]);

            for (int j = 1; !gapFound && j < 13; j++)
            {
                if (shapeList.Contains((short)(shapeList[i] + j)))
                {
                    tempList.Add((short)(shapeList[i] + j));
                    shapeList.Remove((short)(shapeList[i] + j));
                }
                else
                {
                    gapFound = true;
                }
            }

            if (tempList.Count < 3)
            {
                foreach (short index in tempList)
                {
                    extraSelectedCardIndexes.Add(index);
                }
                tempList.Clear();
            }
            else
            {
                perList.Add(tempList);
            }

            //shapeList.Remove(shapeList[i]);
            tempList.Clear();
            gapFound = false;
        }

        return perList;
    }
    private List<List<short>> getPerList()
    {
        List<List<short>> perList = new List<List<short>>();

        List<short> selectedIndexes = new List<short>();

        List<short> selectedClubs = new List<short>();
        List<short> selectedSpades = new List<short>();
        List<short> selectedDiamonds = new List<short>();
        List<short> selectedHearts = new List<short>();


        foreach (card card in selectedCards)
        {
            selectedIndexes.Add(card.cardIndex);
        }

        selectedIndexes.Sort();

        #region seperation to shapes
        for (int i = 0; i < selectedIndexes.Count; i++)
        {
            if (selectedIndexes[i] < 13)
                selectedClubs.Add(selectedIndexes[i]);

            else if (selectedIndexes[i] < 26)
                selectedSpades.Add(selectedIndexes[i]);

            else if (selectedIndexes[i] < 39)
                selectedDiamonds.Add(selectedIndexes[i]);

            else if (selectedIndexes[i] < 52)
                selectedHearts.Add(selectedIndexes[i]);
        }
        #endregion

        foreach (List<short> list in getPerListforShape(selectedClubs))
        {
            perList.Add(list);
        }

        foreach (List<short> list in getPerListforShape(selectedSpades))
        {
            perList.Add(list);
        }

        foreach (List<short> list in getPerListforShape(selectedDiamonds))
        {
            perList.Add(list);
        }

        foreach (List<short> list in getPerListforShape(selectedHearts))
        {
            perList.Add(list);
        }

        return perList;
    }

    private bool canOpen()
    {
        //Debug.Log(getPerList().Count);
        if (!openedBefore)
        {
            if (getPerList().Count >= 2 && extraSelectedCardIndexes.Count == 0)
                return true;
        }
        else
        {
            if (getPerList().Count > 0)
            {
                if (extraSelectedCardIndexes.Count == 0)
                    return true;

            }
            else
            {
                if (areCardsAddable())
                    return true;
            }

        }


        return false;
    }
    private bool areCardsAddable()
    {
        List<short> tempExtraSelected = new List<short>(extraSelectedCardIndexes);
        List<short> openedIndexes = new List<short>();

        foreach (miniCard card in openedCards[Client.playerPosition])
        {
            openedIndexes.Add(card.cardIndex);
        }

        for (int i = 0; i < 5; i++)
        {
            if (tempExtraSelected.Count == 0)
                return true;

            foreach (short index in tempExtraSelected.ToArray())
            {
                if (openedIndexes.Contains((short)(index + 1)) || openedIndexes.Contains((short)(index - 1)))
                {
                    openedIndexes.Add(index);
                    tempExtraSelected.Remove(index);
                }
            }

        }

        return false;

    }

    private void openCards()
    {

        for (int i=0; i<selectedCards.Count; i++)
        {
            miniCard miniCardInstance = ((GameObject)Instantiate(miniCard, new Vector2(cardOpenRectancle[6].position.x + openedCards[Client.playerPosition].Count * 0.25f, 
                cardOpenRectancle[6].position.y), Quaternion.identity)).GetComponent<miniCard>();
            miniCardInstance.cardIndex = selectedCards[i].cardIndex;
            miniCardInstance.showFace();
            openedCards[Client.playerPosition].Add(miniCardInstance);

        }

        string openedData = "";

        //Clear selected cards
        foreach (card card in selectedCards)
        {
            openedData += card.cardIndex.ToString() + "|";
            myDeck.Remove(card.cardIndex);
            myDeckObjects.Remove(card);
            Destroy(card.gameObject);
        }
        selectedCards.Clear();

        deckArranger(Client.playerPosition);

        openedBefore = true;
        openedArranger(Client.playerPosition);

        client.Send("COPNC|"+ openedData);
    }

    public void openedArranger(int playerPosition)
    {
        sortCardList(openedCards[playerPosition]);

        int counter_x = 0, counter_y = 0;
        for (int i = 0; i < openedCards[playerPosition].Count; i++ )
        {
            miniCard card = openedCards[playerPosition][i];

            int startPoint = 0;

            if (playerPosition == Client.playerPosition)
                startPoint = 6;
            else if (playerPosition == Client.playerPosition % 4 + 1)
                startPoint = 4;
            else if (playerPosition == (Client.playerPosition + 1) % 4 + 1)
                startPoint = 0;
            else
                startPoint = 2;

            card.transform.position = new Vector2(cardOpenRectancle[startPoint].position.x + counter_x * 0.25f,
                cardOpenRectancle[startPoint].position.y - counter_y * 0.5f);
            counter_x++;

            if (i != openedCards[playerPosition].Count - 1 && (
                card.cardIndex != openedCards[playerPosition][i + 1].cardIndex - 1) || (card.cardIndex + 1) % 13 == 0 )
            {
                counter_x++;
            }

            if (counter_x * 0.25f > cardOpenRectancle[startPoint + 1].position.x)
            {
                counter_y++;
                counter_x = 0;
            }
        }
    }

    void sortCardList(List<card> cardList)
    {
        for (int i = 0; i < cardList.Count - 1; i++)
        {
            for (int j = 0; j < cardList.Count - i - 1; j++)
            {
                if (cardList[j + 1].cardIndex < cardList[j].cardIndex)
                {
                    card tempCard = cardList[j];
                    cardList[j] = cardList[j + 1];
                    cardList[j + 1] = tempCard;
                }
            }
        }
    }
    void sortCardList(List<miniCard> cardList)
    {
        for (int i = 0; i < cardList.Count - 1; i++)
        {
            for (int j = 0; j < cardList.Count - i - 1; j++)
            {
                if (cardList[j + 1].cardIndex < cardList[j].cardIndex)
                {
                    miniCard tempCard = cardList[j];
                    cardList[j] = cardList[j + 1];
                    cardList[j + 1] = tempCard;
                }
            }
        }
    }

    private IEnumerator holdDetector(card card)
    {
        while (true)
        {
            if(card.mouseDownTime<0f)
            {
                if (selectedCards.Count == 1 && yourTurn && cardDistrubuter.cardTaken)
                {
                    //throw card
                    throwCard(card);
                }
                else
                {
                    card.transform.Translate(new Vector3(0f, -0.5f, 0f));
                    selectedCards.Remove(card);
                }
                break;
                
            }
            if(Time.time - card.mouseDownTime > 1f)
            {
                sortCardList(selectedCards);
                foreach (card c in selectedCards)
                {
                    c.followPointer = true;
                }
                followingPointer = true;
                break;
            }
            yield return new WaitForSeconds(0f);
        }
    }
}

    