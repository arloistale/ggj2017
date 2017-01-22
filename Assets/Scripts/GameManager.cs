using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

using System.Collections.Generic;		//Allows us to use Lists. 
using UnityEngine.UI;					//Allows us to use UI.

public struct GameScore
{
    int team1;
    int team2;

    public void reset()
    {
        team1 = 0;
        team2 = 0;
    }
    public int getTeam1()
    {
        return team1;
    }
    public int getTeam2()
    {
        return team2;
    }
    public void scoredT1()
    {
        team1++;
    }
    public void scoredT2()
    {
        team2++;
    }
}

public class GameManager : MonoBehaviour
{
    public const int WINNING_SCORE = 3;
    public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
	public float turnDelay = 0.1f;							//Delay between each Player turn.
	public int playerFoodPoints = 100;						//Starting value for Player food points.
	public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
	[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.
	
	
	private Text levelText;									//Text to display current level number.
	private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
	private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
	private int level = 1;									//Current level number, expressed in game as "Day 1".
	private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
	private bool enemiesMoving;								//Boolean to check if enemies are moving.
	private bool doingSetup = true;                         //Boolean to check if we're setting up board, prevent Player from moving during setup.
    public GameScore score;                                 //Scores of both teams
    public List<Player> leftTeam;                           //Holds players of left team
    public List<Player> rightTeam;                          //Holds players of right team


    //Awake is always called before any Start functions
    void Awake()
	{
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);	
		
		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);
		
		//Assign enemies to a new List of Enemy objects.
		enemies = new List<Enemy>();
		
		//Get a component reference to the attached BoardManager script
		boardScript = GetComponent<BoardManager>();

        //Reset GameScore
        score.reset();

        //Set left team and right team
        Player[] players = FindObjectsOfType(typeof(Player)) as Player[];

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].isLeftTeam)
            {
                leftTeam.Add(players[i]);
            }
            else
            {
                rightTeam.Add(players[i]);
            }
            players[i].isHoldingEgg = false;
        }

        //Call the InitGame function to initialize the first level 
        InitGame();
	}

    public void resetEgg()
    {
        Player[] players = FindObjectsOfType(typeof(Player)) as Player[];

        for (int i = 0; i < leftTeam.Count; i++)
        {
            leftTeam[i].isHoldingEgg = false;
            leftTeam[i].egg.gameObject.SetActive(true);
        }
        for (int i = 0; i < rightTeam.Count; i++)
        {
            rightTeam[i].isHoldingEgg = false;
            rightTeam[i].egg.gameObject.SetActive(true);
        }
    }

    //this is called only once, and the paramter tell it to be called only after the scene was loaded
    //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static public void CallbackInitialization()
    {
        //register the callback to be called everytime the scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //This is called each time a scene is loaded.
    static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        instance.level++;
        instance.InitGame();
    }

	
	//Initializes the game for each level.
	void InitGame()
	{
		//While doingSetup is true the player can't move, prevent player from moving while title card is up.
		doingSetup = true;
		
		//Get a reference to our image LevelImage by finding it by name.
		levelImage = GameObject.Find("LevelImage");
		
		//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		
		//Set the text of levelText to the string "Day" and append the current level number.
		levelText.text = "Round " + level;
		
		//Set levelImage to active blocking player's view of the game board during setup.
		levelImage.SetActive(true);
		
		//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
		Invoke("HideLevelImage", levelStartDelay);
		
		//Clear any Enemy objects in our List to prepare for next level.
		enemies.Clear();
		
		//Call the SetupScene function of the BoardManager script, pass it current level number.
		boardScript.SetupScene(level);
		
	}
	
	
	//Hides black image used between levels
	void HideLevelImage()
	{
		//Disable the levelImage gameObject.
		levelImage.SetActive(false);
		
		//Set doingSetup to false allowing player to move again.
		doingSetup = false;
	}
	
	//Update is called every frame.
	void Update()
	{
        for (int i = 0; i < leftTeam.Count; i++)
        {
            //Check collision
            if ((Vector2.Distance(leftTeam[i].transform.position, leftTeam[i].egg.transform.position) < 1) && (leftTeam[i].egg.gameObject.activeSelf))
            {
                //Player gathered egg
                leftTeam[i].egg.gameObject.SetActive(false);
                leftTeam[i].isHoldingEgg = true;
                //    leftTeam[i].playHoldingSound();
            }
            /*
             * if(dead){
             *  isHoldingEgg = false;
             *  egg.gameObject.setActive(true);
             *  leftTeam[i].playDroppingSound();
             * }
             */
            if (leftTeam[i].isHoldingEgg)
            {
                if (Vector2.Distance(leftTeam[i].transform.position, leftTeam[i].leftBase.transform.position) < 1)
                {
                    //Left team scored
                    score.scoredT1();
                    resetEgg();
                }
            }
        }

        for (int i = 0; i < rightTeam.Count; i++)
        {
            //Check collision
            if (Vector2.Distance(rightTeam[i].transform.position, rightTeam[i].egg.transform.position) < 1)
            {
                rightTeam[i].egg.gameObject.SetActive(false);
                rightTeam[i].isHoldingEgg = true;
                //     rightTeam[i].playHoldingSound();
            }
            /*
             * if(dead){
             *  isHoldingEgg = false;
             *  egg.gameObject.setActive(true);
             *  leftTeam[i].playDropSound();
             * }
             */
            if (rightTeam[i].isHoldingEgg)
            {
                if (!rightTeam[i].isLeftTeam)
                {
                    if (Vector2.Distance(rightTeam[i].transform.position, rightTeam[i].rightBase.transform.position) < 1)
                    {
                        //right team scored
                        rightTeam[i].rightBase.gameObject.SetActive(false);
                        score.scoredT1();
                    }
                }
            }
        }
        /*
         * 
        Debug.Log("======");
        Debug.Log(score.getTeam1());
        Debug.Log(score.getTeam2());
        */
        //if captured, check if winning team won. 2 out of 3
        if (score.getTeam1() == WINNING_SCORE)
        {
            Debug.Log("Team1!");
            //Team 1 wins
        }
        else if (score.getTeam2() == WINNING_SCORE)
        {
            Debug.Log("Team2!");
            //Team 2 wins
        }

        //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
        if (playersTurn || enemiesMoving || doingSetup)
			
			//If any of these are true, return and do not start MoveEnemies.
			return;
		
		//Start moving enemies.
		StartCoroutine (MoveEnemies ());
	}
	
	//Call this to add the passed in Enemy to the List of Enemy objects.
	public void AddEnemyToList(Enemy script)
	{
		//Add Enemy to List enemies.
		enemies.Add(script);
	}
	
	
	//GameOver is called when the player reaches 0 food points
	public void GameOver()
	{
		//Set levelText to display number of levels passed and game over message
		levelText.text = "After " + level + " days, you starved.";
		
		//Enable black background image gameObject.
		levelImage.SetActive(true);
		
		//Disable this GameManager.
		enabled = false;
	}
	
	//Coroutine to move enemies in sequence.
	IEnumerator MoveEnemies()
	{
		//While enemiesMoving is true player is unable to move.
		enemiesMoving = true;
		
		//Wait for turnDelay seconds, defaults to .1 (100 ms).
		yield return new WaitForSeconds(turnDelay);
		
		//If there are no enemies spawned (IE in first level):
		if (enemies.Count == 0) 
		{
			//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
			yield return new WaitForSeconds(turnDelay);
		}
		
		//Loop through List of Enemy objects.
		for (int i = 0; i < enemies.Count; i++)
		{
			//Call the MoveEnemy function of Enemy at index i in the enemies List.
			enemies[i].MoveEnemy ();
			
			//Wait for Enemy's moveTime before moving next Enemy, 
			yield return new WaitForSeconds(enemies[i].moveTime);
		}
		//Once Enemies are done moving, set playersTurn to true so player can move.
		playersTurn = true;
		
		//Enemies are done moving, set enemiesMoving to false.
		enemiesMoving = false;
	}
}