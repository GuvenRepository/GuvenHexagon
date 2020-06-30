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
    public GameObject hex;
    private GameObject[,] hexs;
    public GameObject bomb;
    private List<bomb> bombs = new List<bomb>();
    public GameObject popParticle;
    public uint bombAfter = 1000;
    private List<Anchor> anchors = new List<Anchor>();
    public GameObject circle;
    private Color[] colors = { Color.green, Color.red, Color.white, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.gray};
    public int colorVariety = 5;
    private Vector2 initialPos;
    private SpriteRenderer hexSR;
    private float spriteWidth;
    private float spriteHeight;
    public int horizontalCount = 9;
    public int verticalCount = 8;
    private bool rotateAnimationRunning = false;
    private int score = 0;
    public Text scoreText;
    public GameObject gameOverPanel;
    private uint fallAnimationCounter = 0;
    private Anchor currentAnc = null;

    void Start()
    {
        hexs = new GameObject[horizontalCount, verticalCount];
        hexSR = hex.GetComponent<SpriteRenderer>();
        createInitialGrid();
        createAnchors();
        StartCoroutine(checkContinuously());
    }   
    
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.Mouse0) && !rotateAnimationRunning && fallAnimationCounter == 0)
        //{
        //    Vector3 p = Input.mousePosition;
        //    p.z = 10;
        //    Vector3 pos = Camera.main.ScreenToWorldPoint(p);
        //    currentAnc = findNearestAnchor(pos);
        //    circle.transform.position = currentAnc.position;
        //}
        //if (currentAnc != null)
        //{
        //    if (Input.GetKeyDown(KeyCode.DownArrow))
        //        StartCoroutine(rotateClockwise(currentAnc));
        //    if (Input.GetKeyDown(KeyCode.UpArrow))
        //        StartCoroutine(rotateCounterClockwise(currentAnc));
        //}

        touchCheck();
    }


    private void touchCheck()
    {
        if (touchScreenControllers.touch && !rotateAnimationRunning && fallAnimationCounter == 0)
        {
            Vector3 p = touchScreenControllers.touchPos;
            p.z = 10;
            Vector3 pos = Camera.main.ScreenToWorldPoint(p);
            currentAnc = findNearestAnchor(pos);
            circle.transform.position = currentAnc.position;
        }
        if (currentAnc != null && !rotateAnimationRunning)
        {
            if (touchScreenControllers.downSwipe)
                StartCoroutine(rotateClockwise(currentAnc));
            if (touchScreenControllers.upSwipe)
                StartCoroutine(rotateCounterClockwise(currentAnc));
        }
    }
    private void createInitialGrid()
    {
        spriteWidth = hexSR.bounds.size.x;
        spriteHeight = hexSR.bounds.size.y;
        initialPos = new Vector3(-1*verticalCount*0.75f*spriteWidth/2 + spriteWidth/2,
                                -1 * horizontalCount * spriteHeight / 2 + spriteHeight / 2);
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
    private void createAnchors()
    {
        Vector2 currentPos = initialPos;
        for (int i = 0; i < horizontalCount; i++)
        {
            for (int j = 0; j < verticalCount; j++)
            {
                if (!((i == 0 && j % 2 == 1) || (i == horizontalCount - 1 && j % 2 == 0)))
                {
                    if (j != verticalCount - 1)
                    {
                        //Instantiate(circle, new Vector3(currentPos.x + spriteWidth / 2, currentPos.y, -1), Quaternion.identity);
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
                        //Instantiate(circle, new Vector3(currentPos.x - spriteWidth / 2, currentPos.y, -1), Quaternion.identity);
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
                    pop(anc);
                    found = true;
                }
            }
        }
        if(found)
            fall();
        return found;
    }
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
                        //hexs[i, j].transform.position -= new Vector3(0, k * spriteHeight, 0);
                        StartCoroutine(fallAnimation(hexs[i, j], hexs[i, j].transform.position - new Vector3(0, k * spriteHeight, 0)));
                        hexs[i, j] = null;
                    }
                }
            }
        }
        fillSpaces();
    }
    private IEnumerator fallAnimation(GameObject obj, Vector3 finishPos)
    {
        while(rotateAnimationRunning);

        fallAnimationCounter++;
        while (obj.transform.position != finishPos)
        {
            obj.transform.Translate(0, -0.1f*spriteHeight, 0,Space.World);
            yield return new WaitForSeconds(0.01f);
        }
        fallAnimationCounter--;
        yield return null;
    }
    private void fillSpaces()
    {
        Vector2 currentPos = initialPos;
        for (int i = 0; i < horizontalCount; i++)
        {
            for (int j = 0; j < verticalCount; j++)
            {
                if(hexs[i, j] == null)
                {
                    if(score > bombAfter && Random.value < 0.1)
                    {
                        GameObject newBomb = Instantiate(bomb, currentPos, Quaternion.identity);
                        newBomb.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colorVariety)];
                        newBomb.name = i.ToString() + j.ToString();
                        bombs.Add(newBomb.GetComponent<bomb>());
                        hexs[i, j] = newBomb;
                    }
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
    private IEnumerator rotateClockwise(Anchor anc)
    {
        int i;
        for (i = 0; i < 3 && !checkTrinity(); i++){ 
            GameObject rootObject = new GameObject();
            rootObject.transform.position = anc.position;
            foreach(Vector2Int index in anc.attachedObjectIndexes)
            {
                hexs[index.x, index.y].transform.SetParent(rootObject.transform);
            }

            rotateAnimationRunning = true;
            StartCoroutine(rotateAnimation(rootObject.transform, -120));
            yield return new WaitForSeconds(0.75f);

            foreach (Vector2Int index in anc.attachedObjectIndexes)
            {
                hexs[index.x, index.y].transform.SetParent(null);
            }
            rotateAnimationRunning = false;

            GameObject temp = hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y];
            hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y] = hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y];
            hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y] = hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y];
            hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y] = temp;

            Destroy(rootObject);

        }
        StartCoroutine(checkContinuously());
        if (i!=3)
            decreaseBombCount();
    }
    private IEnumerator rotateCounterClockwise(Anchor anc)
    {
        int i;
        for (i = 0; i < 3 && !checkTrinity(); i++)
        {
            GameObject rootObject = new GameObject();
            rootObject.transform.position = anc.position;
            foreach (Vector2Int index in anc.attachedObjectIndexes)
            {
                hexs[index.x, index.y].transform.SetParent(rootObject.transform);
            }

            rotateAnimationRunning = true;
            StartCoroutine(rotateAnimation(rootObject.transform, 120));
            yield return new WaitForSeconds(0.75f);
            
            foreach (Vector2Int index in anc.attachedObjectIndexes)
            {
                //hexs[index.x, index.y].transform.up = Vector3.up;
                hexs[index.x, index.y].transform.SetParent(null);
                
            }
            rotateAnimationRunning = false;

            GameObject temp = hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y];
            hexs[anc.attachedObjectIndexes[0].x, anc.attachedObjectIndexes[0].y] = hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y];
            hexs[anc.attachedObjectIndexes[2].x, anc.attachedObjectIndexes[2].y] = hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y];
            hexs[anc.attachedObjectIndexes[1].x, anc.attachedObjectIndexes[1].y] = temp;

            Destroy(rootObject);
        }
        StartCoroutine(checkContinuously());
        if (i != 3)
            decreaseBombCount();
    }
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
    private void clearAnchor()
    {
        currentAnc = null;
        circle.transform.position = new Vector3(-1000, -1000, 0);
    }
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
    private void gameOver()
    {
        gameOverPanel.SetActive(true);
        rotateAnimationRunning = true; // oyun bittikten sonra kontrolleri kilitle
    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

