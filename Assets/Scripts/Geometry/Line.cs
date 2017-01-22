using UnityEngine;
using System.Collections;

/// <summary>
/// Line.cs
///
/// Geometry class to handle drawing lines using scaled Sprites.
/// Adapted from http://gamedevelopment.tutsplus.com/tutorials/how-to-generate-shockingly-good-2d-lightning-effects-in-unity-c--cms-21275
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Line : MonoBehaviour 
{
	#region Geometry Data

	
	// start point
    public Vector2 StartPoint { get; private set; }
     
    // end point
    public Vector2 EndPoint { get; private set; }
     
    // line thickness
    public float Thickness { get; private set; }


    #endregion

    #region Drawing Data


    /// <summary>
    /// The line sprite renderer.
    /// </summary>
    private SpriteRenderer _lineSpriteRenderer;


    #endregion

    #region Init


    /// <summary>
    /// Awake initializes Line components.
    /// </summary>
    private void Awake()
    {
    	_lineSpriteRenderer = GetComponent<SpriteRenderer>();
    }


    #endregion

    #region Drawing


	/// <summary>
    /// Activate the line with specified startPoint, endPoint and thickness.
    /// Must call Draw() afterwards to get the line into the right scaling and position.
    /// </summary>
    /// <param name="startPoint">Start point.</param>
    /// <param name="endPoint">End point.</param>
    /// <param name="thickness">Thickness.</param>
    public void Activate(Vector2 startPoint, Vector2 endPoint, float thickness)
    {
    	gameObject.SetActive(true);

        StartPoint = startPoint;
        EndPoint = endPoint;
        Thickness = thickness;

        Draw();
    }

    /// <summary>
    /// Deactivate the line, hiding it.
    /// </summary>
    public void Deactivate()
    {
		gameObject.SetActive(false);
    }

    /// <summary>
    /// Draw the line, scaling and positioning the line to fit within the given start - end point and thickness.
    /// </summary>
	public void Draw()
	{
	    Vector2 difference = EndPoint - StartPoint;
	    float rotation = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
	     
	    // Set the scale of the line to reflect length and thickness
	    // We scale by 100 because that is the default pixels to units in Unity
	    transform.localScale = new Vector3(100 * (difference.magnitude / _lineSpriteRenderer.sprite.rect.width), 
	    	Thickness, transform.localScale.z);
	     
	    //Rotate the line so that it is facing the right direction
	    transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
	     
	    //Move the line to be centered on the starting point
	    transform.position = new Vector3 (StartPoint.x, StartPoint.y, transform.position.z);
	     
	    //Need to convert rotation to radians at this point for Cos/Sin
	    rotation *= Mathf.Deg2Rad;
	     
	    //Store these so we only have to access once
	    float lineChildWorldAdjust = transform.localScale.x * _lineSpriteRenderer.sprite.rect.width / 2f;
	  	
	    //Adjust the middle segment to the appropriate position
	    transform.position += new Vector3 (0.01f * Mathf.Cos(rotation) * lineChildWorldAdjust, 
	    	0.01f * Mathf.Sin(rotation) * lineChildWorldAdjust, 0);
	}


	#endregion

	#region Manipulation

     
    // Used to set the color of the line
    public void SetColor(Color color)
    {
        _lineSpriteRenderer.color = color;
    }


    #endregion
}