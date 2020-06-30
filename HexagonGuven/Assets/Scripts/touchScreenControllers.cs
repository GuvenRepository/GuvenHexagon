using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class touchScreenControllers : MonoBehaviour
{
    Vector2 firstPressPos;
    Vector2 secondPressPos;
    Vector2 currentSwipe;

    public static bool touch = false;
    public static bool upSwipe = false;
    public static bool downSwipe = false;
    public static Vector2 touchPos;


    private void Update()
    {
        touch = false;
        upSwipe = false;
        downSwipe = false;

        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                
                //save began touch 2d point
                firstPressPos = new Vector2(t.position.x, t.position.y);
            }
            if (t.phase == TouchPhase.Ended)
            {
                //save ended touch 2d point
                secondPressPos = new Vector2(t.position.x, t.position.y);

                if (Vector2.Distance(firstPressPos, secondPressPos) < 50)
                {
                    touch = true;
                    touchPos = secondPressPos;
                }
                    
                else
                {
                    //create vector from the two points
                    currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

                    //normalize the 2d vector
                    currentSwipe.Normalize();

                    //swipe upwards
                    if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                    {
                        upSwipe = true;
                    }
                    //swipe down
                    else if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                    {
                        downSwipe = true;
                    }
                    
                }  
            }
        }
    }
}
