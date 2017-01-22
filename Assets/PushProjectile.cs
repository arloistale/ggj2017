using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PushProjectile : MonoBehaviour 
{
	AudioSource audioSource;

	private Rigidbody2D _rigidBody;

    [SerializeField]
    private float _launchSpeed;

    private Player _owner;

	[Header("Pull Wavegun")] //I'm just using this to put comments in the inspector
	public AudioClip pullSound;
	[Header("Push Wavegun")]
	public AudioClip pushSound;


    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

        StartCoroutine(KillAfterDelay());

		audioSource = GetComponent<AudioSource> ();
    }

    public void SetOwner(Player owner)
    {
        _owner = owner;
    }

    public void Launch(Vector2 launchDirection)
    {
        float angle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        _rigidBody.velocity = launchDirection * _launchSpeed;

		audioSource.PlayOneShot (pushSound);
    }

    private IEnumerator KillAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool didHit = false;

        if(other.tag.Equals("Player"))
        {
            Player otherPlayer = other.gameObject.GetComponent<Player>();
            if(otherPlayer != null && otherPlayer != _owner)
            {
                Vector2 diff = (other.transform.position - transform.position);
                otherPlayer.Push(diff * 13f);
                didHit = true;

            }
        }
        else
        {
            // otherwise we can just push the object if it's a moving object
            MovingObject otherMovingObj = other.gameObject.GetComponent<MovingObject>();

            if(otherMovingObj != null)
            {
                Vector2 diff = (other.transform.position - transform.position);
                otherMovingObj.Push(diff * 13f);
                didHit = true;
            }
        }

        if(didHit)
            Destroy(this.gameObject);
    }
}
