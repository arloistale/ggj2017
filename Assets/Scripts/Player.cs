﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
		public int pointsPerFood = 10;				//Number of points to add to player food points when picking up a food object.
		public int pointsPerSoda = 20;				//Number of points to add to player food points when picking up a soda object.
		public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.

        // primary projectile
        public GameObject primaryFireProjectilePrefab;
        public float primaryFireCooldownTime = 2f;
        private float _lastPrimaryFireTime;

        public bool HasBeenHitRecentlyByPrimaryProjectile { get; private set; }

        // aiming
        private Vector2 _aimDirection;
        
        public Line playerAimingLine;
        public float playerAimingLineDistance;
        public float playerAimingLineThickness;

		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
		public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a food object.
		public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a food object.
		public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
		public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
		public AudioClip gameOverSound;				//Audio clip to play when player dies.		

		public Vector2 playerPosition;

        public AudioClip pickUpSound;               //Pick up Egg sound
        public AudioClip dropSound;                 //Egg is dropped sound

        private GameObject _leftBase;
        public GameObject leftBase
        {
            get
            {
                if(_leftBase == null)
                {
                    _leftBase = GameObject.Find("LeftBase");
                }

                return _leftBase;
            }
        }

        private GameObject _rightBase;
        public GameObject rightBase
        {
        get
            {
                if(_rightBase == null)
                {
                    _rightBase = GameObject.Find("RightBase");
                }

                return _rightBase;
            }
        }


        public bool isHoldingEgg = false;
        public bool isLeftTeam;

        public float scalex;
		public float scaley;

        public int playerIndex = 0;

        private Egg _egg;
        public Egg egg
        {
            get
            {
                if(_egg == null)
                {
                    var eggObj = GameObject.Find("Egg");
                    _egg = eggObj.GetComponent<Egg>();
                }

                return _egg;
            }
        }
		
		private Animator animator;					//Used to store a reference to the Player's animator component.
		private int food;                           //Used to store player food points total during level.
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif
		
		
		//Start overrides the Start function of MovingObject
		protected override void Start ()
		{
			//Get a component reference to the Player's animator component
			animator = GetComponent<Animator>();
         
			playerPosition = this.transform.position;
			
			//Get the current food point total stored in GameManager.instance between levels.
			food = GameManager.instance.playerFoodPoints;

            _aimDirection = transform.forward;
			
			//Call the Start function of the MovingObject base class.
			base.Start ();
		}
		
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
			GameManager.instance.playerFoodPoints = food;
		}
		
		
		private void Update ()
		{
			//Check if we are running either in the Unity editor or in a standalone build.

            Vector2 inMoveDir = GetInDirectionFromInputBasedOnPlayerIndex();
            Vector2 inAimDir = GetInAimDirectionFromInputBasedOnPlayerIndex().normalized;

            bool primaryFireDown = GetPrimaryFireDownFromInputBasedOnPlayerIndex();
                     
			// movement and aiming
			AttemptMove<Wall> (inMoveDir);        
            AimTowards(inAimDir);

            // firing
            if(primaryFireDown)
            {
                FirePrimaryInAimDirection();
            }

            // die if needed
            DieIfNeeded();
		}

        private void FirePrimaryInAimDirection()
        {
            if(_aimDirection.sqrMagnitude < float.Epsilon)
                return;

            if(Time.time - _lastPrimaryFireTime < primaryFireCooldownTime)
                return;

            _lastPrimaryFireTime = Time.time;

            var projObj = ( GameObject ) Instantiate(this.primaryFireProjectilePrefab, new Vector3(transform.position.x, transform.position.y, -1), Quaternion.LookRotation(_aimDirection));
            var proj = projObj.GetComponent<PushProjectile>();
            proj.SetOwner(this);
            proj.Launch(_aimDirection);
        }

        private void AimTowards(Vector2 inDir)
        {
            if(inDir.sqrMagnitude > float.Epsilon)
            {
                playerAimingLine.Activate(transform.position, transform.position + (Vector3)(inDir * playerAimingLineDistance), playerAimingLineThickness);
                _aimDirection = inDir;
            }
            else
            {
                playerAimingLine.Deactivate();
            }
        }

        private bool GetPrimaryFireDownFromInputBasedOnPlayerIndex()
        {
            bool isDown = false;

            switch(playerIndex)
            {
                case 0:
                    isDown = Input.GetButtonDown("Fire");
                    break;
                case 1:
                    isDown = Input.GetButtonDown("Fire1");
                    break;
                case 2:
                    isDown = Input.GetButtonDown("Fire2");
                    break;
                case 3:
                    isDown = Input.GetButtonDown("Fire3");
                    break;
            }

        Debug.Log(playerIndex + " : " + isDown);

            return isDown;
        }

        private Vector2 GetInDirectionFromInputBasedOnPlayerIndex()
        {
            float h = 0f, v = 0f;

            switch(playerIndex)
            {
                case 0:
                    h = Input.GetAxisRaw ("Horizontal");
                    v = Input.GetAxisRaw ("Vertical");
                    break;
                case 1:
                    h = Input.GetAxisRaw("Horizontal1");
                    v = Input.GetAxisRaw("Vertical1");
                    break;
                case 2:
                    h = Input.GetAxisRaw("Horizontal2");
                    v = Input.GetAxisRaw("Vertical2");
                    break;
                case 3:
                    h = Input.GetAxisRaw("Horizontal3");
                    v = Input.GetAxisRaw("Vertical3");
                    break;
            }

            return new Vector2(h, v);
        }

        private Vector2 GetInAimDirectionFromInputBasedOnPlayerIndex()
        {
            float hori = 0f, vert = 0f;

            switch(playerIndex)
            {
                case 0:
                    hori = Input.GetAxisRaw ("HorizontalAim");
                    vert = Input.GetAxisRaw ("VerticalAim");
                    break;
                case 1:
                    hori = Input.GetAxisRaw("HorizontalAim1");
                    vert = Input.GetAxisRaw("VerticalAim1");
                    break;
                case 2:
                    hori = Input.GetAxisRaw("HorizontalAim2");
                    vert = Input.GetAxisRaw("VerticalAim2");
                    break;
                case 3:
                    hori = Input.GetAxisRaw("HorizontalAim3");
                    vert = Input.GetAxisRaw("VerticalAim3");
                    break;
            }

            return new Vector2(hori, vert);
        }
		
		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
		protected override void AttemptMove <T> (Vector2 inDir)
		{
        	//Every time player moves, subtract from food points total.
			food--;
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
			base.AttemptMove <T> (inDir);
			
			//Hit allows us to reference the result of the Linecast done in Move.
			RaycastHit2D hit;
			
			//If Move returns true, meaning Player was able to move into an empty space.
			if (Move (inDir, out hit)) 
			{
				//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
				//SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
			}
			
			//Since the player has moved and lost food points, check if the game has ended.
			CheckIfGameOver ();

			//Debug.Log ("not dead");
			DieIfNeeded ();
	
			
			//Set the playersTurn boolean of GameManager to false now that players turn is over.
			GameManager.instance.playersTurn = false;
		}
		
		
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = component as Wall;
			
			//Call the DamageWall function of the Wall we are hitting.
			hitWall.DamageWall (wallDamage);
			
			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			animator.SetTrigger ("playerChop");
		}
		
		
		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D (Collider2D other)
		{
		}
		
		
		//Restart reloads the scene when called.
		private void Restart ()
		{
			//Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}
		
		
		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		public void LoseFood (int loss)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
			animator.SetTrigger ("playerHit");
			
			//Subtract lost food points from the players total.
			food -= loss;
			
			//Check to see if game has ended.
			CheckIfGameOver ();
		}
		
		
		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
		private void CheckIfGameOver ()
		{

		}

    public void playHoldingSound()
    {
        SoundManager.instance.PlaySingle(pickUpSound);
        //SoundManager.instance.musicSource.Stop();
    }
    //	void playerShrink(){
    //		
    //		if (this.transform.localScale.x > 0.1f) {
    //			scalex = this.transform.localScale.x;
    //			scaley = this.transform.localScale.y;
    //			this.transform.localScale = new Vector2 (0.99f * scalex, 0.999f * scaley);
    //			Debug.Log (this.transform.localScale.x);
    //			Debug.Log (scalex);
    //			playerShrink ();
    //		}
    //    }	
}
