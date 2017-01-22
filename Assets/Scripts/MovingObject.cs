using UnityEngine;
using System.Collections;

//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
public abstract class MovingObject : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float speedDecay = 0.00001f;

    [SerializeField]
    private float _timeToRespawn = 3f;

	public float moveTime = 0.1f;			//Time it will take object to move, in seconds.
	public LayerMask blockingLayer;			//Layer on which collision will be checked.

    private Vector2 _currVelocity;
	
	private BoxCollider2D boxCollider; 		//The BoxCollider2D component attached to this object.
	private Rigidbody2D rb2D;				//The Rigidbody2D component attached to this object.

    private bool _isInputVelocityOverrideEnabled;

    private GameObject _background;
    protected GameObject Background
    {
        get
        {
            if(_background == null)
            {
                _background = GameObject.Find("background");
            }

            return _background;
        }
    }

    private BoxCollider2D _respawnSafeZoneCollider;
    public BoxCollider2D RespawnSafeZoneCollider
    {
        get
        {
            if(_respawnSafeZoneCollider == null)
            {
                _respawnSafeZoneCollider = GameObject.Find("RespawnSafeZone").GetComponent<BoxCollider2D>();
            }

            return _respawnSafeZoneCollider;
        }
    }

    private bool _isDying = false;
	
	//Protected, virtual functions can be overridden by inheriting classes.
	protected virtual void Start ()
	{
        EnableInputVelocityOverride(true);

		//Get a component reference to this object's BoxCollider2D
		boxCollider = GetComponent <BoxCollider2D> ();
		
		//Get a component reference to this object's Rigidbody2D
		rb2D = GetComponent <Rigidbody2D> ();
	}

    public void EnableInputVelocityOverride(bool flag)
    {
        _isInputVelocityOverrideEnabled = flag;
    }

    public void ResetMovement()
    {
        _currVelocity = Vector2.zero;
    }
	
	//Move returns true if it is able to move and false if not. 
	//Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
	protected bool Move (Vector2 inDir, out RaycastHit2D hit)
	{
		//Store start position to move from, based on objects current transform position.
		Vector2 start = transform.position;

        // get the modified velocity based on the input direction and the current velocity
    //    Debug.Log(inDir);
        _currVelocity = GetModifiedVelocity(inDir);
		
		// Calculate end position based on the direction parameters passed in when calling Move.
		Vector2 end = start + _currVelocity;
		
		//Disable the boxCollider so that linecast doesn't hit this object's own collider.
		boxCollider.enabled = false;
		
		//Cast a line from start point to end point checking collision on blockingLayer.
		hit = Physics2D.Linecast (start, end, blockingLayer);
		
		//Re-enable boxCollider after linecast
		boxCollider.enabled = true;
		
		//Check if anything was hit
		//if(hit.transform == null)
		//{
			//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
       //     if(rb2D.bodyType == RigidbodyType2D.Kinematic)
			    rb2D.velocity = _currVelocity;
        //Debug.Log(_currVelocity);
			//Return true to say that Move was successful
			return true;
		//}
		
		//If something was hit, return false, Move was unsuccesful.
		return false;
	}
	
    /// <summary>
    /// Smooth movement function that calculates the velocity based on a given input direction.
    /// </summary>
    /// <returns>The movement.</returns>
    /// <param name="inDir">In dir.</param>
	private Vector2 GetModifiedVelocity (Vector2 inDir)
	{      
        float sqrInputMagnitude = inDir.sqrMagnitude;

        // if we are moving at all from the input direction, then set the velocity to the max in the correct direction
        if(_isInputVelocityOverrideEnabled && sqrInputMagnitude > .1)
        {
            return inDir * maxSpeed;
        }
        else
        {
         //   Debug.Log("C:" + _currVelocity);
            float currMagnitude = _currVelocity.magnitude;
            currMagnitude = Mathf.Max(currMagnitude - speedDecay, 0f);
            return _currVelocity.normalized * currMagnitude;
        }
	}


    #region Force

    /// <summary>
    /// When we are pushed, change the type of rigidbody to 
    /// </summary>
    /// <param name="pushVector">Push dir.</param>
    public void Push(Vector2 pushVector)
    {
        EnableInputVelocityOverride(false);
        _currVelocity += pushVector;
    }

    #endregion
	
	//The virtual keyword means AttemptMove can be overridden by inheriting classes using the override keyword.
	//AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact with if blocked (Player for Enemies, Wall for Player).
	protected virtual void AttemptMove <T> (Vector2 inDir)
		where T : Component
	{
		//Hit will store whatever our linecast hits when Move is called.
		RaycastHit2D hit;

		//Set canMove to true if Move was successful, false if failed.
		bool canMove = Move (inDir, out hit);
		
		//Check if nothing was hit by linecast
		if(hit.transform == null)
			//If nothing was hit, return and don't execute further code.
			return;
		
		//Get a component reference to the component of type T attached to the object that was hit
		T hitComponent = hit.transform.GetComponent <T> ();
		
		//If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
		if(!canMove && hitComponent != null)
			
			//Call the OnCantMove function and pass it hitComponent as a parameter.
			OnCantMove (hitComponent);
	}

    public bool DieIfNeeded ()
    {
        if(_isDying)
            return false;

        var backgroundCollider = Background.GetComponent<BoxCollider2D>();
        var checkPosition = new Vector3(transform.position.x, transform.position.y, backgroundCollider.transform.position.z);
        bool shouldDie = !backgroundCollider.bounds.Contains(checkPosition);

        if(shouldDie)
        {
            GameManager.instance.StartCoroutine(DieAndRespawn());

            return true;
        }

        return false;
    }

    private IEnumerator DieAndRespawn()
    {
        _isDying = true;

        /*
            if(moveSound1 != null)
            {
                AudioSource.PlayClipAtPoint(moveSound1,transform.position);
            }*/

        EnableInputVelocityOverride(false);

        Debug.Log("die die die");

        gameObject.SetActive(false);

        yield return new WaitForSeconds(_timeToRespawn);

        Respawn();

        EnableInputVelocityOverride(true);

        _isDying = false;

        Debug.Log("no die");
    }

    public void Respawn()
    {
        ResetMovement();
        gameObject.SetActive(true);

        Vector3 minPoint = RespawnSafeZoneCollider.bounds.min;
        Vector3 maxPoint = RespawnSafeZoneCollider.bounds.max;

        Vector3 spawnPoint = !this.name.Equals("Egg") ? new Vector3(Random.Range(minPoint.x, maxPoint.x), Random.Range(minPoint.y, maxPoint.y)) : new Vector3((minPoint.x + maxPoint.x) / 2, (minPoint.x + maxPoint.x) / 2);

        this.transform.position = spawnPoint;
    }
	
	//The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
	//OnCantMove will be overriden by functions in the inheriting classes.
	protected abstract void OnCantMove <T> (T component)
		where T : Component;
}