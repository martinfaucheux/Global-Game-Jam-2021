using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateAtMouseClick : MonoBehaviour
{
    private GameObject block;
    public List<GameObject> gameObjects;
    private List<GameObject> nextItems;
    [SerializeField] private float QueueTime = 10.0f;
    public float SetQueueTime { set => QueueTime = value; }
    public bool is_created;
    public bool canClick = true;

    private static InstantiateAtMouseClick _instance;
    public static InstantiateAtMouseClick instance { get { return _instance; } }


    // Start is called before the first frame update
    void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        GameManager.RegisterInstantiator(this);
        nextItems = new List<GameObject>();

        AddItemToQueue();
        AddItemToQueue();
        AddItemToQueue();
        AddItemToQueue();

        // Wait for X sec and start the game
        StartCoroutine(startQueue());

        GameEvents.instance.onGameOver += delegate { canClick = false; };
    }



    private IEnumerator startQueue()
    {
        yield return new WaitForSeconds(QueueTime);
        AddItemToQueue();
        // Start Game
        StartCoroutine(startQueue());

    }

    // Update is called once per frame
    void Update()
    {

        if (!is_created && nextItems.Count > 0)
        {
            GameObject next = nextItems[0];
            nextItems.RemoveAt(0);
            ItemListUI.instance.showList(nextItems);

            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            block = Instantiate(next, worldPosition, Quaternion.identity);
            block.GetComponent<FollowMouse>().enabled = true;
            is_created = true;
        }
    }

    public void AddItemToQueue()
    {
        nextItems.Add(gameObjects[Random.Range(0, gameObjects.Count)]);
        ItemListUI.instance.showList(nextItems);
    }


}



