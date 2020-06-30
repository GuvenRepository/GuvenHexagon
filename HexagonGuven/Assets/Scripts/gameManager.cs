using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

class Anchor
{
    public Vector3 position;
    public List<Vector2Int> attachedObjectIndexes = new List<Vector2Int>();
}
public class gameManager : MonoBehaviour
{
    #region public parameters
    public int colorVariety = 5;
    public int horizontalCount = 9;
    public int verticalCount = 8;
    public uint bombAfter = 1000;
    #endregion

    #region prefabs
    public GameObject hex;
    public GameObject bomb;
    public GameObject popParticle;
    public GameObject circle;
    public Text scoreText;
    public GameObject gameOverPanel;
    #endregion

    #region lists
    private GameObject[,] hexs;
    private List<bomb> bombs = new List<bomb>();
    private List<Anchor> anchors = new List<Anchor>();
    private Color[] colors = { Color.green, Color.red, Color.white, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.gray};
    #endregion
    
    private Vector2 initialPos;
    private SpriteRenderer hexSR;
    private float spriteWidth;
    private float spriteHeight;
    private bool rotateAnimationRunning = false;
    private int score = 0;
    private uint fallAnimationCounter = 0;
    private Anchor currentAnc = null;

    void Start()
    {
        // Initializations
        hexs = new GameObject[horizontalCount, verticalCount];
        hexSR = hex.GetComponent<SpriteRenderer>();
        createInitialGrid();
        createAnchors();
        //Pop until no trinity left
        StartCoroutine(checkContinuously());
    }   
    
    void Update()
    {
        touchCheck();
    }


    private void touchCheck()
    {
        if(!rotateAnimationRunning && fallAnimationCounter == 0)
        {
            //if touch accurs and no animation running
            //set an anchor
            if (touchScreenControllers.touch)
            {
                Vector3 p = touchScreenControllers.touchPos;
                p.z = 10;
                Vector3 pos = Camera.main.ScreenToWorldPoint(p);
                currentAnc = findNearestAnchor(pos);
                circle.transform.position = currentAnc.position;
            }
            // if anchor set before
            // and swipe occurs
            // rotate the anchor
            if (currentAnc != null)
            {
                if (touchScreenControllers.downSwipe)
                    StartCoroutine(rotateClockwise(currentAnc));
                if (touchScreenControllers.upSwipe)
                    StartCoroutine(rotateCounterClockwise(currentAnc));
            }
        }
        
    }

    //create the first grid and assignments
    private void createInitialGrid()
    {
        spriteWidth = hexSR.bounds.size.x;
        spriteHeight = hexSR.bounds.size.y;
        //initialPos is the position of first hex to make the grid centered
        initialPos = new Vector3(-1*verticalCount*0.75f*spriteWidth/2 + spriteWidth/2,
                                -1 * horizontalCount * spriteHeight / 2 + spriteHeight / 2);
        //placing tiles
        Vector2 currentPos = initialPos;
        for (int i = 0; i < horizontalCount; i++)
        {
            for (int j = 0; j < verticalCount; j++)
            {
                GameObject newHex = Instantiate(hex, currentPos, Quaternion.identity);
                newHex.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colorVariety)];
                newHex.name = i.ToString() + j.ToString();
                hexs[i, j] = newHex;

                currentPos += new Vector2(3*spriteWidth/4f, 0);
                if (j % 2 == 1)
                    currentPos += new Vector2(0, spriteHeight / 2);
                else
                    currentPos -= new Vector2(0, spriteHeight / 2);
            }
            currentPos.x = initialPos.x;
            currentPos += new Vector2(0, spriteHeight);
        }
    }

    //Anchors are the touch points availible to user
    //each hex have two anchers
    //one is on left tip, other is on right
    private void createAnchors()
    {
        Vector2 currentPos = initialPos;
        for (int i = 0; i < horizontalCount; i++)
        {
            for (int j = 0; j < verticalCount; j++)
            {
                // not all edge colunms and rows have anchors
                // check drawings
                if (!((i == 0 && j % 2 == 1) || (i == horizontalCount - 1 && j % 2 == 0)))
                {
                    if (j != verticalCount - 1)
                    {
                        Anchor newAnchor = new Anchor();
                        newAnchor.position = new Vector3(currentPos.x + spriteWidth / 2, currentPos.y, -1);
                        newAnchor.attachedObjectIndexes.Add(new Vector2Int(i, j));
                        if (j % 2 == 0)
                        {
                            newAnchor.attachedObjectIndexes.Add(new Vector2Int(i, j + 1));
                            newAnchor.attachedObjectIndexes.Add(new Vector2Int(i + 1, j + 1));
                        }
                        else
                        {
                            newAnchor.attachedObjectIndexes.Add(new Vector2Int(i - 1, j + 1));
                            newAnchor.attachedObjectIndexes.Add(new Vector2Int(i, j + 1));
                        }
                        
                        anchors.Add(newAnchor);
                    }
                    if (j != 0)
                    {
                        Anchor newAnchor = new Anchor();
                        newAnchor.position = new Vector3(currentPos.x - spriteWidth / 2, currentPos.y, -1);
                        newAnchor.attachedObjectIndexes.Add(new Vector2Int(i, j));
                        if (j % 2 == 1)
                        {
                            newAnchor.attachedObjectIndexes.Add(new Vector2Int(i, j - 1));
                            newAnchor.attachedObjectIndexes.Add(new Vector2Int(i - 1, j - 1));
                        }
                        else
                        {
                            newAnchor.attachedObjectIndexes.Add(new Vector2Int(i + 1, j - 1));
                            newAnchor.attachedObjectIndexes.Add(new Vector2Int(i, j - 1));
                        }
                        anchors.Add(newAnchor);
                    }
                }
                currentPos += new Vector2(3 * spriteWidth / 4f, 0);
                if (j % 2 == 1)
                    currentPos += new Vector2(0, spriteHeight / 2);
                else
                    currentPos -= new Vector2(0, spriteHeight / 2);
            }
            currentPos.x = initialPos.x;
            currentPos += new Vector2(0, spriteHeight);
        }
    }

    // Check and pop until no trinity left
    private IEnumerator checkContinuously()
    {
        while (true)
        {
            if (fallAnimationCounter == 0)
                if (!checkTrinity())
                    break;
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }

    // If there are 3 same color hex together
    // it is trinity
    private bool checkTrinity()
    {
        bool found = false;
        foreach(Anchor anc in anchors)
        {
            Color firstColor;
            Color secondColor;
            Color thirdColor;
            try
            {
                firstColor = hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y].GetComponent<SpriteRenderer>().color;
                secondColor = hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y].GetComponent<SpriteRenderer>().color;
                thirdColor = hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y].GetComponent<SpriteRenderer>().color;
            }
            catch
            {
                continue;
            }
            

            if(firstColor == secondColor)
            {
                if(secondColor == thirdColor)
                {
                    // if found one pop 3 hex attach to this anchor
                    pop(anc);
                    found = true; // set true, so we will check again after fall
                }
            }
        }
        if(found)
            fall();
        return found;
    }

    //Destroy Object attached to anchor
    //Make the particle effect
    //Increase score
    //Clear circle indicator
    private void pop(Anchor anc)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject obj2destroy = hexs[anc.attachedObjectIndexes[i].x, anc.attachedObjectIndexes[i].y];
            if (obj2destroy.tag == "bomb")
            {
                bombs.Remove(obj2destroy.GetComponent<bomb>());
            }
            Destroy(obj2destroy);
            hexs[anc.attachedObjectIndexes[i].x, anc.attachedObjectIndexes[i].y] = null;
            score += 5;
            scoreText.text = "Score: " + score.ToString();
        }

        Instantiate(popParticle, anc.position, Quaternion.identity);
        clearAnchor();
    }

    //if hex have an empty scace below it
    //start fallling
    private void fall()
    {
        for (int i = 1; i < horizontalCount; i++)
        {
            for (int j = 0; j < verticalCount; j++)
            {
                for (int k = i; k > 0; k--)
                {
                    if (hexs[i, j] == null)
                        continue;
                    if (hexs[i - k, j] == null)
                    {
                        hexs[i - k, j] = hexs[i, j];
                        StartCoroutine(fallAnimation(hexs[i, j], hexs[i, j].transform.position - new Vector3(0, k * spriteHeight, 0)));
                        hexs[i, j] = null;
                    }
                }
            }
        }
        fillSpaces();
    }

    //most basic fall animation
    private IEnumerator fallAnimation(GameObject obj, Vector3 finishPos)
    {
        while(rotateAnimationRunning);

        //there is a falling animation now
        //don't make anything stupid
        fallAnimationCounter++;
        while (obj.transform.position != finishPos)
        {
            obj.transform.Translate(0, -0.1f*spriteHeight, 0,Space.World);
            yield return new WaitForSeconds(0.01f);
        }
        //animation has ended for only for this tile
        fallAnimationCounter--;
        yield return null;
    }

    //if a space is empty after fall
    //regenerate them
    private void fillSpaces()
    {
        Vector2 currentPos = initialPos;
        for (int i = 0; i < horizontalCount; i++)
        {
            for (int j = 0; j < verticalCount; j++)
            {
                if(hexs[i, j] == null)
                {
                    //10% chance to spawn a bomb after a specified score
                    if(score > bombAfter && Random.value < 0.1)
                    {
                        GameObject newBomb = Instantiate(bomb, currentPos, Quaternion.identity);
                        newBomb.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colorVariety)];
                        newBomb.name = i.ToString() + j.ToString();
                        bombs.Add(newBomb.GetComponent<bomb>());
                        hexs[i, j] = newBomb;
                    }
                    //If you're lucky a classic hex for you
                    else
                    {
                        GameObject newHex = Instantiate(hex, currentPos, Quaternion.identity);
                        newHex.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colorVariety)];
                        newHex.name = i.ToString() + j.ToString();
                        hexs[i, j] = newHex;
                    }
                }
                currentPos += new Vector2(3 * spriteWidth / 4f, 0);
                if (j % 2 == 1)
                    currentPos += new Vector2(0, spriteHeight / 2);
                else
                    currentPos -= new Vector2(0, spriteHeight / 2);
            }
            currentPos.x = initialPos.x;
            currentPos += new Vector2(0, spriteHeight);
        }
    }

    //Find the closest anchor wherever you touch
    private Anchor findNearestAnchor(Vector2 rawPos)
    {
        Anchor minAnchor = new Anchor();
        float minDistance = 1000f;
        foreach (Anchor anc in anchors)
        {
            float distance = Vector3.Distance(rawPos, anc.position);
            if (distance < minDistance)
            {
                minAnchor = anc;
                minDistance = distance;
            }
        }
        return minAnchor;
    }

    //Rotate anchor clockwise
    private IEnumerator rotateClockwise(Anchor anc)
    {
        int i;
        //Try 3 120 degree turn
        //if pop happens stop
        for (i = 0; i < 3 && !checkTrinity(); i++){
            //Create empty object
            GameObject rootObject = new GameObject();
            rootObject.transform.position = anc.position;

            //Make it parent of attached hexs
            foreach (Vector2Int index in anc.attachedObjectIndexes)
            {
                hexs[index.x, index.y].transform.SetParent(rootObject.transform);
            }

            //Rotate empty object
            rotateAnimationRunning = true;
            StartCoroutine(rotateAnimation(rootObject.transform, -120));
            yield return new WaitForSeconds(0.75f);

            //Release childs
            foreach (Vector2Int index in anc.attachedObjectIndexes)
            {
                hexs[index.x, index.y].transform.SetParent(null);
            }
            rotateAnimationRunning = false;

            //Rearrange grid indexes
            GameObject temp = hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y];
            hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y] = hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y];
            hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y] = hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y];
            hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y] = temp;

            //No need to empty object anymore
            Destroy(rootObject);

        }
        StartCoroutine(checkContinuously());
        //if pop happened decrease bobms
        if (i!=3)
            decreaseBombCount();
    }

    //Rotate anchor counter-clockwise
    private IEnumerator rotateCounterClockwise(Anchor anc)
    {
        int i;
        //Try 3 120 degree turn
        //if pop happens stop
        for (i = 0; i < 3 && !checkTrinity(); i++)
        {
            //Create empty object
            GameObject rootObject = new GameObject();
            rootObject.transform.position = anc.position;

            //Make it parent of attached hexs
            foreach (Vector2Int index in anc.attachedObjectIndexes)
            {
                hexs[index.x, index.y].transform.SetParent(rootObject.transform);
            }

            //Rotate empty object
            rotateAnimationRunning = true;
            StartCoroutine(rotateAnimation(rootObject.transform, 120));
            yield return new WaitForSeconds(0.75f);
            
            foreach (Vector2Int index in anc.attachedObjectIndexes)
            {
                hexs[index.x, index.y].transform.SetParent(null);
                
            }
            rotateAnimationRunning = false;

            //Rearrange grid indexes
            GameObject temp = hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y];
            hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y] = hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y];
            hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y] = hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y];
            hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y] = temp;

            //No need to empty object anymore
            Destroy(rootObject);
        }
        StartCoroutine(checkContinuously());
        //if pop happened decrease bobms
        if (i != 3)
            decreaseBombCount();
    }

    //basic rotate animation
    private IEnumerator rotateAnimation(Transform trs, float angle)
    {
        int clockwise;
        if (angle < 0)
            clockwise = -1;
        else
            clockwise = 1;
        float angleCounter = 0;
        float resolution = 10f;
        while(angleCounter < clockwise * angle)
        {
            trs.Rotate(0, 0, clockwise * resolution);
            angleCounter += resolution;
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }

    //if pop happens no need to show circle
    //user should select new anchor
    private void clearAnchor()
    {
        currentAnc = null;
        circle.transform.position = new Vector3(-1000, -1000, 0); //make it away
    }

    //desrease all bomb counts in list
    //if count=0, gameover
    private void decreaseBombCount()
    {
        foreach(bomb b in bombs)
        {
            b.decreaseCount();
            if(b.getCount() == 0)
            {
                gameOver();
            }
        }
    }

    //gameover screen
    private void gameOver()
    {
        gameOverPanel.SetActive(true);
        rotateAnimationRunning = true; // oyun bittikten sonra kontrolleri kilitle
    }

    //restart button
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

