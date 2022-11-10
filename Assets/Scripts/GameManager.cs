using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public enum GameState
{
    Dealing, Playing, End
}

public class GameManager : MonoBehaviour
{
    [Header("Play space")]
    public Sprite backSprite;
    public Transform[] players;
    public int numberStartCard = 7;
    public float angleForStartCard = 70f;
    public Transform pile;
    public bool flipAllAfterShuffer;


    public List<Card> Cards {get; private set;}
    public GameState GameState {get; set;}
    public Dictionary<int, List<Card>> PlayerCard {get; private set;} 

    private int countDealing;


    public void Flip(Card card, bool value) => card.card.GetComponent<SpriteRenderer>().sprite = value ? card.sprite : backSprite;


    private void Start()
    {
        LoadAllCard();
        Shuffer();
    }

    private void Shuffer()
    {
        GameState = GameState.Dealing;
        StopAllCoroutines();
        Cards.Shuffle();

        foreach(var e in Cards)
        {
            e.card.transform.position = pile.position;
            e.card.transform.rotation = Quaternion.identity;
            Flip(e, false);
        }

        PlayerCard = new Dictionary<int, List<Card>>();
        countDealing = players.Length;
        for(int i = 0; i < players.Length; i++)
        {
            PlayerCard.Add(i, new List<Card>());
            StartCoroutine(MoveCardToPlayerHand(i, players[i]));
        }
    }

    private IEnumerator MoveCardToPlayerHand(int i, Transform player)
    {
        float moveTime = 0.25f;
        float startX = player.localEulerAngles.z + angleForStartCard / 2f;
        float disX = angleForStartCard / numberStartCard;

        int numberPlayer = players.Length;
        int count = 0;
        int playerIndex = i;
        for(; i < Cards.Count; i+=numberPlayer)
        {
            PlayerCard[playerIndex].Add(Cards[i]);

            var c = Cards[i].card.transform;
            var originPos = c.position;
            var originRot = c.localEulerAngles;
            var nextPos = new Vector3(player.position.x, player.position.y, numberStartCard - count + 1);
            var nextRot = player.localEulerAngles + new Vector3(0, 0, startX - count * disX);

            yield return Extensions.MoveTo(c, originPos, nextPos, originRot, nextRot, moveTime);
            count++;
            Flip(Cards[i], flipAllAfterShuffer);

            if(count >= numberStartCard)
                break;
        }

        --countDealing;
        if(countDealing <= 0)   // Finish dealing
        {
            GameState = GameState.Playing;
        }
    }

    private void LoadAllCard()
    {
        var suits = new List<Suit>
        {
            Suit.Club, Suit.Diamond, Suit.Heart, Suit.Spade
        };

        Cards = new List<Card>();
        for(int suit = 0; suit < suits.Count; suit++)
        {
            for(int rank = 1; rank <= 13; rank++)
            {
                CreateACard(suits[suit], rank, Vector3.zero);
            }
        }
    }

    private GameObject CreateACard(Suit suit, int rank, Vector3 pos)
    {
        string name = GetName(suit, rank);

        var data = File.ReadAllBytes(GetPath(suit, rank));
        var tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0));

        var card = new GameObject(name);
        card.transform.localScale = Vector3.one * 0.15f;
        card.transform.position = pos;

        var renderer = card.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;

        var collider = card.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(0, 10.25f);
        collider.size = new Vector2(12.5f, 19.5f);

        Cards.Add(new Card(){ card = card, sprite = sprite});

        return card;
    }

    private void Flip(Card card)
    {
        var render = card.card.GetComponent<SpriteRenderer>();
        if(render.sprite == backSprite) render.sprite = card.sprite;
        else render.sprite = backSprite;
    }
    
    private string GetName(Suit suit, int rank) => suit.ToString() + (rank < 10 ? "0" + rank.ToString() : rank.ToString());
    private string GetPath(Suit suit, int rank) => Path.Combine(Application.dataPath, "Cards", GetName(suit, rank) + ".png");

    private void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 100, 30), "Flip all"))
        {
            foreach(var e in Cards)
            {
                Flip(e);
            }
        }

        if(GUI.Button(new Rect(0, 40, 100, 30), "Shuffer"))
        {
            Shuffer();
        }
    }

    private enum Suit
    {
        Club, Diamond, Heart, Spade
    }

    public class Card
    {
        public GameObject card;
        public Sprite sprite;

        public void SetAvailable(bool value)
        {
            card.GetComponent<Collider2D>().enabled = value;
        }
    }
}
