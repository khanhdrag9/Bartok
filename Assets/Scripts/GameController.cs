using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameManager manager;
    public LayerMask cardMask;
    public Transform target;
    public int controlPlayer;

    private Camera mainCam;
    private int order;
    private GameState cacheState = GameState.Dealing;

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
        if(Input.GetMouseButtonUp(0))
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            var collider = Physics2D.OverlapPoint(mousePos, cardMask);
            if(collider)
            {
                var cards = manager.Cards;
                var name = collider.name;
                Debug.Log("Name: " + name);
                var card = cards.FirstOrDefault(e => e.card.name.Equals(name));
                if(card != null)
                {
                    card.SetAvailable(false);
                    var c = card.card.transform;
                    c.position = new Vector3(c.position.x, c.position.y, order);
                    c.localEulerAngles = target.localEulerAngles;
                    StartCoroutine(Extensions.MoveTo(c, c.position, new Vector3(target.position.x, target.position.y, order), c.localEulerAngles, target.localEulerAngles, 0.25f));
                    order--;
                }
            }
        }
    }

    private void SetMainPlayer(int playerIndex)
    {
        foreach(var e in manager.Cards)
        {
            e.SetAvailable(false);
            manager.Flip(e, false);
        }

        foreach(var e in manager.PlayerCard)
        {
            if(e.Key != playerIndex) continue;

            foreach(var card in e.Value) 
            {
                card.SetAvailable(true);
                manager.Flip(card, true);
            }
            break;
        }
    }
}
