using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/*
class GameManager {
	currentCoinValue
	poolCount
	holdCount
	Dictionary<Timestamp, value>
	startTime
	List<news>
	playerMoney
}
*/
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public float currentCoinValue;
    public int poolCount;
    public int holdCount;
    public DateTime startTime;

    public float dayTime = 200f;
    public float playerMoney = 0;
    public Dictionary<DateTime, float> coinValueHistory = new Dictionary<DateTime, float>();

    [SerializeField] private List<NewsModel> restNews = new List<NewsModel>();
    [SerializeField] private List<NewsModel> news = new List<NewsModel>();

    public List<GameObject> newsFeedBubbles;

    [SerializeField] private List<DecisionModel> decisions = new List<DecisionModel>();

    public List<GameObject> decisionCards;

    void Awake()
    {
        // Check if an instance of GameManager already exists
        if (instance != null && instance != this)
        {
            // Destroy this instance if a GameManager already exists
            Destroy(gameObject);
            return;
        }

        // Set this as the instance and make it persistent across scenes
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadNews();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadNews()
    {
        NewsLoader newsLoader = ScriptableObject.CreateInstance<NewsLoader>();
        NewsModel[] newsModels = newsLoader.LoadNewsModels();
        List<NewsModel> newsModelList = newsModels.ToList();

        news = newsModelList.GetRange(0, 3);
        restNews = newsModelList.GetRange(3, newsModelList.Count - 3);
        
        ShowNews();
        showDecisions();
    }

    void ShowNews()
    {
        for (int i = 0; i < newsFeedBubbles.Count; i++)
        {
            GameObject newsFeedBubble = newsFeedBubbles[i];

            if (news.Count <= i)
            {
                newsFeedBubble.SetActive(false);
                continue;
            }

            newsFeedBubble.SetActive(true);
            NewsFeedBubbleController newsFeedBubbleController = newsFeedBubble.GetComponent<NewsFeedBubbleController>();
            newsFeedBubbleController.SetNews(news[i]);
        }
    }

    void OnChangeCoinValue(DateTime timestamp, float value)
    {
        coinValueHistory.Add(timestamp, value);
        currentCoinValue = value;
    }

    public void SelectNews(string newsFeedId)
    {

        NewsModel newsModel = news.Find(news => news.id == newsFeedId);

        decisions = newsModel.decisions.ToList();

        showDecisions();

    }

    void showDecisions()
    {
        for (int i = 0; i < decisionCards.Count; i++)
        {
            if (decisions.Count <= i)
            {
                decisionCards[i].SetActive(false);
                continue;
            }

            decisionCards[i].SetActive(true);

            DecisionModel decision = decisions[i];
            GameObject decisionCard = decisionCards[i];
            DecisionCardController decisionCardController = decisionCard.GetComponent<DecisionCardController>();
            decisionCardController.SetDecision(decision);

        }
    }

    public void OnDecisionCardClicked(string id)
    {
        DecisionModel decision = decisions.Find(decision => decision.id == id);
        NewsModel newsModel = news.Find(news => news.id == decision.newsID);

        if (decision == null)
        {
            Debug.LogError("Decision not found");
            return;
        }

        if (newsModel == null)
        {
            Debug.LogError("News not found");
            return;
        }

        calculateCurrentCoinValue(decision, newsModel.effectPoints);
        RemoveNews(newsModel);

        ShowNews();
        showDecisions();
    }

    public void RemoveNews(NewsModel news)
    {
        this.news.Remove(news);
        ShowNews();
        decisions = new List<DecisionModel>();
        StartCoroutine(loadOneRestNews());
    }

    private IEnumerator loadOneRestNews() {
        if (restNews.Count == 0)
        {
            yield break;
        }

        yield return new WaitForSeconds(1);

        NewsModel newsModel = restNews[0];
        restNews.RemoveAt(0);
        this.news.Add(newsModel);
        ShowNews();
    }

    void calculateCurrentCoinValue(DecisionModel decision, float effectPoints)
    {
        // TODO: show people reaction
        ReactionModel[] reactions = decision.reactions;
        
        float randomValue = UnityEngine.Random.Range(0f, 1f);

        ReactionValue reactionValue = ReactionValue.noEffect;

        if (randomValue < decision.approvalPercentage / 100)
        {
            reactionValue = ReactionValue.approval;
        }
        else if (randomValue < (decision.approvalPercentage + decision.disapprovalPercentage)/100)
        {
            reactionValue = ReactionValue.disapproval;
        }


        if (reactionValue == ReactionValue.noEffect) {
            // no effect on the coin value
            return;
        } 

        if (reactionValue == ReactionValue.approval)
        {
            currentCoinValue += effectPoints;
        }
        else if (reactionValue == ReactionValue.disapproval)
        {
            currentCoinValue -= effectPoints;
        }

    }

}
