using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [System.Serializable]
    public class Entity
    {
        public string playerName;
        public Stone[] myStones;
        public bool hasTurn;
        public enum PlayerTypes
        {
            HUMAN,
            CPU,
            NO_PLAYER
        }
        public PlayerTypes playerType;
        public bool hasWon;
    }

    public List<Entity> playerList = new List<Entity>();

    //state  machine
    public enum States
    {
        WAITTING,
        ROLL_DICE,
        SWITCH_PLAYER
    }

    public States state;

    public int activePlayer;
    bool switchingPlayer;
    bool turnPossible = true;

    //Human Inputs
    //GameObject for Button
    public GameObject rollButton;
    [HideInInspector]public int rolledHumanDice;

    public Dice dice;

    void Awake()
    {
        instance = this;

        for(int i = 0; i < playerList.Count; i++)
        {
            if(SaveSettings.players[i] == "HUMAN")
                playerList[i].playerType = Entity.PlayerTypes.HUMAN;
            if (SaveSettings.players[i] == "CPU")
                playerList[i].playerType = Entity.PlayerTypes.CPU;

        }
    }

    void Start()
    {
        ActivateButton(false);

        int randomPlayer = Random.Range(0, playerList.Count);
        activePlayer = randomPlayer;
        Info.instance.ShowMessage(playerList[activePlayer].playerName + " starts first!");

    }

    void Update()
    {
        if(playerList[activePlayer].playerType == Entity.PlayerTypes.CPU)
        {
            switch (state)
            {
                case States.ROLL_DICE:
                    {
                        if (turnPossible)
                        {
                            StartCoroutine(RollDiceDelay());
                            state = States.WAITTING;
                        }

                    }
                    break;
                case States.WAITTING:
                    {
                        //IDLE
                    }
                    break;
                case States.SWITCH_PLAYER:
                    {
                        if (turnPossible)
                        {
                            StartCoroutine(SwitchPlayer());
                            state = States.WAITTING;
                        }

                    }
                    break;
            }
        }

        if (playerList[activePlayer].playerType == Entity.PlayerTypes.HUMAN)
        {
            switch (state)
            {
                case States.ROLL_DICE:
                    {
                        if (turnPossible)
                        {
                            //Deactivate highlights
                            ActivateButton(true);
                            state = States.WAITTING;
                        }

                    }
                    break;
                case States.WAITTING:
                    {
                        //IDLE
                    }
                    break;
                case States.SWITCH_PLAYER:
                    {
                        if (turnPossible)
                        {
                            //Deactivate Button
                            //Deactivate Highlights

                            StartCoroutine(SwitchPlayer());
                            state = States.WAITTING;
                        }

                    }
                    break;
            }
        }
    }

    void CPUDice()
    {
        dice.RollDice();
    }

    public void RollDice(int _diceNumber)//call this from dice
    {
        int diceNumber = _diceNumber;//Random.Range(1, 7);
        //int diceNumber = 6;

        if(playerList[activePlayer].playerType == Entity.PlayerTypes.CPU)
        {
            if (diceNumber == 6)
            {
                //check the start node
                CheckStartNode(diceNumber);
            }

            if (diceNumber < 6)
            {
                //check for kick others
                MoveAStone(diceNumber);
            }
        }

        if (playerList[activePlayer].playerType == Entity.PlayerTypes.HUMAN)
        {
            rolledHumanDice = _diceNumber;
            HumanRollDice();
        }


        Debug.Log("Dice Number=" + diceNumber);
        Info.instance.ShowMessage(playerList[activePlayer].playerName + " has rolled " + _diceNumber);
    }

    IEnumerator RollDiceDelay()
    {
        yield return new WaitForSeconds(2);
        //RollDice();
        CPUDice();
    }

    void CheckStartNode(int diceNumber)
    {
        //Is Anyone On Start Node
        bool startNodeFull = false;
        for(int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            if(playerList[activePlayer].myStones[i].currentNode == playerList[activePlayer].myStones[i].startNode)
            {
                startNodeFull = true;
                break;//Done here We Found Match
            }
        }
        if (startNodeFull)
        {
            // Move a Stone
            MoveAStone(diceNumber);
            Debug.Log("The Start node is Full");
        }
        else //Start Node is Empty
        {
            //If at least One is Inside The Base
            for(int i = 0; i < playerList[activePlayer].myStones.Length; i++)
            {
                if (!playerList[activePlayer].myStones[i].ReturnIsOut())
                {
                    //Leave the Base
                    playerList[activePlayer].myStones[i].LeaveBase();
                    state = States.WAITTING;
                    return;
                }

            }
            //Move a Stone
            MoveAStone(diceNumber);


        }
    }

    void MoveAStone(int diceNumber)
    {
        List<Stone> movableStones = new List<Stone>();
        List<Stone> moveKickStones = new List<Stone>();

        //Fill the lists
        for(int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            if (playerList[activePlayer].myStones[i].ReturnIsOut())
            {
                //Check for Possible Kick
                if(playerList[activePlayer].myStones[i].CheckPossibleKick(playerList[activePlayer].myStones[i].stoneId, diceNumber))
                {
                    moveKickStones.Add(playerList[activePlayer].myStones[i]);
                    continue;
                }

                //Check for Possible Move

                if (playerList[activePlayer].myStones[i].CheckPossibleMove(diceNumber))
                {
                    movableStones.Add(playerList[activePlayer].myStones[i]);
                }
            }
        }

        //perform kick is possible
        if(moveKickStones.Count > 0)
        {
            int num = Random.Range(0, moveKickStones.Count);
            moveKickStones[num].StartTheMove(diceNumber);
            state = States.WAITTING;
            return;
        }

        //perform move is possible
        if (movableStones.Count > 0)
        {
            int num = Random.Range(0, movableStones.Count);
            movableStones[num].StartTheMove(diceNumber);
            state = States.WAITTING;
            return;
        }

        //None is possible

        //Switching the Player
        state = States.SWITCH_PLAYER;
        Debug.Log("Switch the player");


    }

    IEnumerator SwitchPlayer()
    {
        if (switchingPlayer)
        {
            yield break;
        }

        switchingPlayer = true;

        yield return new WaitForSeconds(2);

        //set next player
        SetNextActivePlayer();

        switchingPlayer = false;
    }

    void SetNextActivePlayer()
    {
        activePlayer++;
        activePlayer %= playerList.Count;

        int available = 0;

        for(int i = 0; i < playerList.Count; i++)
        {
            if (!playerList[i].hasWon)
            {
                available++;
            }
        }

        if (playerList[activePlayer].hasWon && available > 1)
        {
            SetNextActivePlayer();
            return;

        }else if(available < 2)
        {
            //Game Over Screen
            SceneManager.LoadScene("GameOver");
            state = States.WAITTING;
            return;
        }
        Info.instance.ShowMessage(playerList[activePlayer].playerName + " 's turn!");
        state = States.ROLL_DICE;
    }

    public void ReportTurnPossible(bool possible)
    {
        turnPossible = possible;
    }

    public void ReportWinning()
    {
        //show some UI
        playerList[activePlayer].hasWon = true;
        //save somewhere
        for(int i = 0; i < SaveSettings.winners.Length; i++)
        {
            if(SaveSettings.winners[i] == "")
            {
                SaveSettings.winners[i] = playerList[activePlayer].playerName;
                break;
            }
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }

    public void BackToMenu(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    //-----------HUMAN INPUT------------

    void ActivateButton(bool on)
    {
        rollButton.SetActive(on);
    }

    public void DeactivateAllSelectors()
    {
        for(int i = 0; i < playerList.Count; i++)
        {
            for(int j = 0; j < playerList[i].myStones.Length; j++)
            {
                playerList[i].myStones[j].SetSelector(false);
            }
        }
    }

    //This is on the ROLL DICE button

    public void HumanRoll()
    {
        dice.RollDice();
        ActivateButton(false);

    }

    public void HumanRollDice()
    {

        //ROLL DICE
        //rolledHumanDice = Random.Range(1, 7);
        //rolledHumanDice = 6;

        //Moveable List
        List<Stone> movableStones = new List<Stone>();

       
        //Start node full check
        //Is Anyone On Start Node
        bool startNodeFull = false;
        for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            if (playerList[activePlayer].myStones[i].currentNode == playerList[activePlayer].myStones[i].startNode)
            {
                startNodeFull = true;
                break;//Done here We Found Match
            }
        }

        //Number < 6
        if(rolledHumanDice < 6)
        {
            //for(int i = 0; i < playerList[activePlayer].myStones.Length; i++)
            //{
            //    //Make sure it is out already
            //    if (playerList[activePlayer].myStones[i].ReturnIsOut())
            //    {
            //        if(playerList[activePlayer].myStones[i].CheckPossibleKick(playerList[activePlayer].myStones[i].stoneId, rolledHumanDice))
            //        {
            //            movableStones.Add(playerList[activePlayer].myStones[i]);
            //            continue;
            //        }

            //        if (playerList[activePlayer].myStones[i].CheckPossibleMove(rolledHumanDice))
            //        {
            //            movableStones.Add(playerList[activePlayer].myStones[i]);
            //        }
            //    }
            //}

            movableStones.AddRange(PossibleStones());
        }

        //Number == 6 && !startNode
        if(rolledHumanDice == 6 && !startNodeFull)
        {
            //inside base check
            for(int i = 0; i < playerList[activePlayer].myStones.Length; i++)
            {
                if (!playerList[activePlayer].myStones[i].ReturnIsOut())
                {
                    movableStones.Add(playerList[activePlayer].myStones[i]);
                }
            }

            //outside check
            movableStones.AddRange(PossibleStones());
        }
        //Number == 6 && startNode
        else if (rolledHumanDice == 6 && startNodeFull)
        {
            movableStones.AddRange(PossibleStones());
        }

        //Activate all possible selectors
        if(movableStones.Count > 0)
        {
            for (int i = 0; i < movableStones.Count; i++)
            {
                movableStones[i].SetSelector(true);
            }
        }
        else
        {
            state = States.SWITCH_PLAYER;
        }

    }

    List<Stone> PossibleStones()
    {
        List<Stone> tempList = new List<Stone>();

        for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            //Make sure it is out already
            if (playerList[activePlayer].myStones[i].ReturnIsOut())
            {
                if (playerList[activePlayer].myStones[i].CheckPossibleKick(playerList[activePlayer].myStones[i].stoneId, rolledHumanDice))
                {
                    tempList.Add(playerList[activePlayer].myStones[i]);
                    continue;
                }

                if (playerList[activePlayer].myStones[i].CheckPossibleMove(rolledHumanDice))
                {
                    tempList.Add(playerList[activePlayer].myStones[i]);
                }
            }
        }

        return tempList;
    }
}
