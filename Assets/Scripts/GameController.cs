using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class GameController : MonoBehaviour
{
    public GameManager manager;
    public LayerMask cardMask;
    public Transform target;
    public int controlPlayer;

    private Camera mainCam;
    private int order;
    private GameState cacheState = GameState.Dealing;
    private int playerTurn;
    private List<Card> placedCards;
    private Card LastCard => placedCards != null && placedCards.Count > 0 ? placedCards[placedCards.Count - 1] : null;
    private List<Card> CurrentPlayerHand => manager.PlayerCard[playerTurn];

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if(cacheState != manager.GameState) // On Enter new state
        {
            cacheState = manager.GameState;

            if(manager.GameState == GameState.Playing)
            {
                order = manager.Cards.Count;
                placedCards = new List<Card>();
                SetMainPlayer(controlPlayer);
            }

        }
        else // On Stay state
        {
            if(manager.GameState == GameState.Dealing)
            {

            }
            else if(manager.GameState == GameState.Playing) 
            {
                Playing();
            }
        }

    }

    private void Playing()
    {
        if(playerTurn != controlPlayer)
        {
            AI();
            return;
        }

        if(Input.GetMouseButtonUp(0))
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            var collider = Physics2D.OverlapPoint(mousePos, cardMask);
            if(collider)
            {
                var cards = manager.Cards;
                var name = collider.name;
                var card = cards.FirstOrDefault(e => e.card.name.Equals(name));
                if(card != null)
                {
                    bool isPlace = CurrentPlayerHand.Exists(e => e == card);
                    if(isPlace)
                        Place(card);
                    else 
                        Draw(card);
                }
            }
        }
    }

    private void AI()
    {
        var availableCards = CurrentPlayerHand.Where(e => Validate(e)).ToArray();
        if(availableCards.Length == 0)
            Draw(manager.Pile.Last());
        else 
            Place(availableCards[Random.Range(0, availableCards.Length)]);
    }


    private void Draw(Card card)
    {
        card.SetAvailable(playerTurn == controlPlayer);
        StartCoroutine(manager.Draw(playerTurn, card, playerTurn == controlPlayer));
        NextTurn();
    }

    private void Place(Card card)
    {
        // Validate
        if(!Validate(card)) 
            return;

        // Place
        card.SetAvailable(false);
        var c = card.card.transform;
        c.position = new Vector3(c.position.x, c.position.y, order);
        c.localEulerAngles = target.localEulerAngles;
        StartCoroutine(Extensions.MoveTo(c, c.position, new Vector3(target.position.x, target.position.y, order), c.localEulerAngles, target.localEulerAngles, 0.25f));
        order--;
        placedCards.Add(card);
        CurrentPlayerHand.Remove(card);
        if(CurrentPlayerHand.Count == 0)
        {
            manager.EndGame();
            return;
        }

        NextTurn();
    }

    private bool Validate(Card card)
    {
        if(LastCard != null)
        {
            GetSuitAndRank(card, out string suit, out int rank);
            GetSuitAndRank(LastCard, out string lsuit, out int lrank);

            if(suit != lsuit && rank != lrank)
                return false;
        } 

        return true;
    }

    private void GetSuitAndRank(Card card, out string suit, out int rank)
    {
        string cardName = card.card.name;
        suit = cardName.Remove(cardName.Length - 2);
        string rankStr = cardName.Remove(0, cardName.Length - 2);
        if(rankStr[0].Equals('0')) rankStr = rankStr[1].ToString();
        rank = int.Parse(rankStr);
    }

    private void NextTurn()
    {
        playerTurn = playerTurn == manager.players.Length - 1 ? 0 : playerTurn + 1;
    }

    private void SetMainPlayer(int playerIndex)
    {
        foreach(var e in manager.Cards)
        {
            e.SetAvailable(true);
            manager.Flip(e, false);
        }

        foreach(var e in manager.PlayerCard)
        {
            bool p = e.Key == playerIndex;

            foreach(var card in e.Value) 
            {
                card.SetAvailable(p);
                manager.Flip(card, p);
            }
        }
    }
}
