using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

/// <summary>
/// LineGroup.cs
///
/// A LineGroup is a geometry object that can be used for drawing paths and other line creations.
/// A LineGroup is implemented as a collection of Lines from Line.cs
/// We arrange the LineGroup using a collection of Vector2 points.
///
/// Adapted from http://gamedevelopment.tutsplus.com/tutorials/how-to-generate-shockingly-good-2d-lightning-effects-in-unity-c--cms-21275
/// </summary>
class LineGroup : MonoBehaviour
{
	#region Drawing Objects Data


    // List of all of our active/inactive lines
    private List<Line> _activeLines;
    private List<Line> _inactiveLines;
     
    // Prefab for a line
    [SerializeField]
    private Line LinePrefab;


    #endregion

    #region Drawing Properties Data

     
    /// <summary>
    /// The color of the lines in the group.
    /// </summary>
    [SerializeField]
    private Color _color = Color.white;
  	public Color Color
  	{
  		get { return _color; }
  		set { _color = value; }
  	}

    /// <summary>
    /// The thickness of the lines in the group.
    /// </summary>
    [SerializeField]
    private float _thickness = 1f;
    public float Thickness
    {
    	get { return _thickness; }
    	set { _thickness = value; }
    }


    #endregion

    #region Meta Data


    /// <summary>
    /// Whether line group has been properly initialized.
    /// </summary>
    private bool _isInitialized;


    #endregion

    #region Init


    /// <summary>
    /// Initialize the LineGroup with the specified pool size of lines.
    /// </summary>
    /// <param name="maxSegments">Max segments.</param>
    public void Initialize(int maxSegments)
    {
        // Initialize lists for pooling
        _activeLines = new List<Line>();
        _inactiveLines = new List<Line>();
         
        for(int i = 0; i < maxSegments; ++i)
        {
            //instantiate from our Line Prefab
            Line line = (Line) Instantiate(LinePrefab);
             
            // parent it to our bolt object
            line.transform.parent = transform;
             
            // set it inactive
            line.Deactivate();
             
            // add it to our list
            _inactiveLines.Add(line);
        }

        _isInitialized = true;
    }


    #endregion

    #region Manipulation


	/// <summary>
	/// Activates the a group of line segments represented by a list of points.
	/// Must use Draw() after specifying Line Group for lines to be ready to show.
	/// </summary>
	/// <param name="points">Points.</param>
    public void ActivateLineSegments(List<Vector2> points)
    {
    	Assert.IsTrue(_isInitialized, "Line Group must be initialized before use.");

    	if(points.Count == 0)
    		return;
         
        // Start at the source
        Vector2 prevPoint = points[0];
         
        for (int i = 1; i < points.Count; ++i)
        {
            ActivateLine(prevPoint, points[i]);
            prevPoint = points[i];
        }
    }
  	

	/// <summary>
	/// Deactivates the line segments.
	/// </summary>
    public void DeactivateSegments()
    {
        for(int i = _activeLines.Count - 1; i >= 0; i--)
        {
            Line line = _activeLines[i];
            line.Deactivate();
            _activeLines.RemoveAt(i);
            _inactiveLines.Add(line);
        }
    }

	/// <summary>
	/// Draw the line group, drawing each segment.
	/// This function should be called AFTER ActivateLineSegments.
	/// </summary>
    public void Draw()
    {
        foreach (Line line in _activeLines)
        {
            line.SetColor(Color);
            line.Draw();
        }
    }


	#endregion

	#region Private Implementation


	/// <summary>
    /// Activates the line, if there's enough objects in the pool.
    /// </summary>
    /// <param name="startPoint">Start point.</param>
    /// <param name="endPoint">End point.</param>
	private void ActivateLine(Vector2 startPoint, Vector2 endPoint)
    {
        // get the inactive count
        int inactiveCount = _inactiveLines.Count;
         
        // only activate if we can pull from inactive
        if(inactiveCount <= 0) return;
         
        // pull from pool
       	Line line = _inactiveLines[inactiveCount - 1];
         
        // activate the line
        line.SetColor(Color.white);
        line.Activate(startPoint, endPoint, Thickness);
        _inactiveLines.RemoveAt(inactiveCount - 1);
        _activeLines.Add(line);
    }


	#endregion
}