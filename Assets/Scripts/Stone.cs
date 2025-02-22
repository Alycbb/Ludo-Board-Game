﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public int stoneId;
    [Header("ROUTES")]
    public Route commonRoute; //Outer Route
    public Route finalRoute;

    public List<Node> fullRoute = new List<Node>();
    [Header("NODES")]
    public Node startNode;

    public Node baseNode; //Nodes in Home Base
    public Node currentNode;
    public Node goalNode;

    int routePosition;
    int startNodeIndex;

    int steps; //Rolled Dice Amount
    int doneSteps;


    [Header("BOOLS")]
    public bool isOut;
    bool isMoving;

    bool hasTurn; //For human input

    [Header("SELECTOR")]
    public GameObject selector;

    //ARC Movement
    float amplitude = 0.5f;
    float cTime = 0f;

    void Start()
    {
        startNodeIndex = commonRoute.RequestPosition(startNode.gameObject.transform);
        CreateFullRoute();

        SetSelector(false);
    }

    void CreateFullRoute()
    {
        for(int i = 0; i < commonRoute.childNodeList.Count; i++)
        {
            int tempPos = startNodeIndex + i;
            tempPos %= commonRoute.childNodeList.Count;

            fullRoute.Add(commonRoute.childNodeList[tempPos].GetComponent<Node>());
        }


        for (int i = 0; i < finalRoute.childNodeList.Count; i++)
        {
            fullRoute.Add(finalRoute.childNodeList[i].GetComponent<Node>());
        }
    }

    IEnumerator Move(int diceNumber)
    {
        if (isMoving)
        {
            yield break;
        }

        isMoving = true;

        while (steps > 0)
        {
            routePosition++;

            Vector3 nextPos = fullRoute[routePosition].gameObject.transform.position;
            Vector3 startPos = fullRoute[routePosition - 1].gameObject.transform.position;
            //while (MoveToNextNode(nextPos, 8f))
            //{
            //    yield return null;
            //}

            while (MoveInArcToNextNode(startPos, nextPos, 4f))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            cTime = 0;
            steps--;
            doneSteps++;
        }

        goalNode = fullRoute[routePosition];
        //check Possible Kick

        if (goalNode.isTaken)
        {
            //Kick the other Stone
            goalNode.stone.ReturnToBase();
        }

        currentNode.stone = null;
        currentNode.isTaken = false;

        goalNode.stone = this;
        goalNode.isTaken = true;

        currentNode = goalNode;
        goalNode = null;

        //Report to GameManager
        //Win Condition Check
        if (WinCondition())
        {
            GameManager.instance.ReportWinning();
        }

        //Switch the Player
        if (diceNumber < 6)
        {
            GameManager.instance.state = GameManager.States.SWITCH_PLAYER;

        }
        else
        {
            GameManager.instance.state = GameManager.States.ROLL_DICE;
        }

        isMoving = false;
    }

    bool MoveToNextNode(Vector3 goalPos, float speed)
    {
        return goalPos != (transform.position = Vector3.MoveTowards(transform.position, goalPos, speed * Time.deltaTime));
    }

    bool MoveInArcToNextNode(Vector3 startPos, Vector3 goalPos, float speed)
    {
        cTime += speed * Time.deltaTime;
        Vector3 myPosition = Vector3.Lerp(startPos, goalPos, cTime);

        myPosition.y += amplitude * Mathf.Sin(Mathf.Clamp01(cTime) * Mathf.PI);

        return goalPos != (transform.position = Vector3.Lerp(transform.position, myPosition, cTime));
    }

    public bool ReturnIsOut()
    {
        return isOut;
    }

    public void LeaveBase()
    {
        steps = 1;
        isOut = true;
        routePosition = 0;
        //Start Coroutine
        StartCoroutine(MoveOut());

    }

    IEnumerator MoveOut()
    {
        if (isMoving)
        {
            yield break;
        }

        isMoving = true;

        while (steps > 0)
        {
            //routePosition++;

            Vector3 nextPos = fullRoute[routePosition].gameObject.transform.position;
            //while (MoveToNextNode(nextPos, 8f))
            //{
            //    yield return null;
            //}

            Vector3 startPos = baseNode.gameObject.transform.position;

            while (MoveInArcToNextNode(startPos, nextPos, 4f))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            cTime = 0;
            steps--;
            doneSteps++;
        }

        //Update Node
        goalNode = fullRoute[routePosition];
        //Check For Kicking Other Stone
        if (goalNode.isTaken)
        {
            //Return to Start Base Node
            goalNode.stone.ReturnToBase();
        }

        goalNode.stone = this;
        goalNode.isTaken = true;

        currentNode = goalNode;
        goalNode = null;


        //Report Back To GameManager
        GameManager.instance.state = GameManager.States.ROLL_DICE;
        isMoving = false;
    }

    public bool CheckPossibleMove(int diceNumber)
    {
        int temPos = routePosition + diceNumber;
        if(temPos >= fullRoute.Count)
        {
            return false;
        }

        return !fullRoute[temPos].isTaken;
    }

    public bool CheckPossibleKick(int stoneID, int diceNumber)
    {
        int temPos = routePosition + diceNumber;
        if (temPos >= fullRoute.Count)
        {
            return false;
        }

        if (fullRoute[temPos].isTaken)
        {
            if(stoneID == fullRoute[temPos].stone.stoneId)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public void StartTheMove(int diceNumber)
    {
        steps = diceNumber;
        StartCoroutine(Move(diceNumber));
    }

    public void ReturnToBase()
    {
        StartCoroutine(Return());
    }

    IEnumerator Return()
    {
        GameManager.instance.ReportTurnPossible(false);
        routePosition = 0;
        currentNode = null;
        goalNode = null;
        isOut = false;
        doneSteps = 0;

        Vector3 baseNodePos = baseNode.gameObject.transform.position;
        while(MoveToNextNode(baseNodePos, 100f))
        {
            yield return null;
        }
        GameManager.instance.ReportTurnPossible(true);

    }

    bool WinCondition()
    {
        for(int i = 0; i < finalRoute.childNodeList.Count; i++)
        {
            if (!finalRoute.childNodeList[i].GetComponent<Node>().isTaken)
            {
                return false;
            }
        }
        return true;
    }


    //-------------HUMAN INPUT---------------

    public void SetSelector(bool on)
    {
        selector.SetActive(on);
        // This is for  Having the click ability
        hasTurn = on;
    }

    void OnMouseDown()
    {
        if (hasTurn)
        {
            if (!isOut)
            {
                LeaveBase();
            }
            else
            {
                StartTheMove(GameManager.instance.rolledHumanDice);
            }
            GameManager.instance.DeactivateAllSelectors();
        }

    }
}
