using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class _CardGameManager : MonoBehaviour
{
    public static _CardGameManager Instance;

    [SerializeField] private RawImage _img;
    [SerializeField] private float _x, _y;

    [SerializeField] private TextMeshProUGUI gameTimerText;

    float remainingTime;
    int score;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalscoreText;

    public static int gameSize = 2;

    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject cardList;
    [SerializeField] private Sprite cardBack;

    // all possible sprite
    [SerializeField] private Sprite[] sprites;

    // list of card
    private _Card[] cards;

    //we place card on this panel
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] public GameObject losePanel;
    [SerializeField] private TextMeshProUGUI sizeLabel;
    [SerializeField] private Slider sizeSlider;

    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;
    private bool gameStart;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //the purpose is to allow preloading of panel, so that it does not lag when it loads
        gameStart = false;
        panel.SetActive(false);
    }

   

    public void StartCardGame()
    {
        if (gameStart) return;
        scoreText.text = score.ToString();
        SetGameSize();
        gameStart = true;
        panel.SetActive(true);
        winPanel.SetActive(false);
        SetGamePanel();
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        SpriteCardAllocation();
        StartCoroutine(HideFace());
    }

    private void SetGamePanel()
    {

        int isOdd = gameSize % 2 ;

        cards = new _Card[gameSize * gameSize - isOdd];
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        RectTransform panelsize = panel.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f/gameSize;
        float xInc = row_size/gameSize;
        float yInc = col_size/gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if(isOdd == 0)
        {
            curX += xInc / 2;
            curY += yInc / 2;
        }
        float initialX = curX;
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    c = cards[index].gameObject;
                }
                else
                {
                    // create card prefab
                    c = Instantiate(prefab);
                    //assign parent
                    c.transform.parent = cardList.transform;

                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<_Card>();
                    cards[index].ID = index;
                    //modify its size
                    c.transform.localScale = new Vector3(scale, scale);
                }
                //assign location
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;

            }
            curY += yInc;
        }

    }

    void ResetFace()
    {
        for (int i = 0; i < gameSize; i++)
            cards[i].ResetRotation();
    }

    IEnumerator HideFace()
    {
        //display for a short moment before flipping
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }

    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        //sprite selection
        for (i = 0; i < cards.Length/2; i++)
        {
            //get a random sprite
            int value = Random.Range(0, sprites.Length - 1);
            //check previous number has not been selection
            for (j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;
            }
            selectedID[i] = value;
        }

        //card sprite deallocation
        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }
        //card sprite allocation
        for (i = 0; i < cards.Length / 2; i++)
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }

    }
    public void SetGameSize()
    {
        gameSize = (int)sizeSlider.value;
        sizeLabel.text = gameSize + " * " + gameSize;

        Dictionary<int, int> gameTime = new Dictionary<int, int>
        {
        { 2, 10 },
        { 3, 30 },
        { 4, 60 },
        { 5, 90 },
        { 6, 120 },
        { 7, 150 },
        { 8, 180 }
        };

        if (gameTime.ContainsKey(gameSize))
        {
            remainingTime = gameTime[gameSize];
        }

    }
    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }
    public Sprite CardBack()
    {
        return cardBack;
    }
    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }
    public void cardClicked(int spriteId, int cardId)
    {
        //first selection
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        {
            if (spriteSelected == spriteId)
            {
                //correctly matched
                score += 10;
                scoreText.text = score.ToString();
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;
                CheckGameWin();
                AudioPlayer.Instance.PlayAudio(2);

            }
            else
            {
                //incorrectly matched
                cards[cardSelected].Flip();
                cards[cardId].Flip();
            }
            cardSelected = spriteSelected = -1;
        }
    }

    private void CheckGameWin()
    {
        //win game
        if (cardLeft == 0)
        {
            EndGame();
            winPanel.SetActive(true);
            finalscoreText.text = score.ToString();
            AudioPlayer.Instance.PlayAudio(1);
        }
    }

    private void EndGame()
    {
        
        gameStart = false;
        panel.SetActive(false);
    }

    public void GiveUp()
    {
        score = 0;
        EndGame();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }

    private void Update()
    {
        _img.uvRect = new Rect(_img.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _img.uvRect.size);
        if(gameStart)
        {
            if (remainingTime > 0)
            {

                remainingTime -= Time.deltaTime;
            }
            else if (remainingTime < 0)
            {
                remainingTime = 0;

                losePanel.SetActive(true);
            }
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
