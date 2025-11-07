using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JustAwake;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManage : MonoBehaviour
{
    public bool Paused;
    public bool ShowHintWhenAwake;
    public GameObject player;
    public Vector3 startPoint;
    public GameObject[] Islands;
    public GameObject[] IslandSet;

    [SerializeField] GameObject PausePanel;
    [SerializeField] GameObject HintPanel;

    public Animator animator;
    public Animator HintPanelAnim;
    public string HintPanelPopIn;


    bool generating;
    int minHeightYouAchieved;

    //the heitht we generated the islands last time
    int theHeight;


    private InputSetting playerInput;
    private PlayerController playerController;

    void Awake()
    {
        playerInput = player.GetComponent<InputSetting>();
        playerController = player.GetComponent<PlayerController>();

        ShowHintWhenAwake = PlayerPrefs.GetInt("ShowHintWhenAwake", 1) == 1 ? true : false;
        if (ShowHintWhenAwake)
        {
            HintPanel.SetActive(true);
            HintPanelAnim.Play(HintPanelPopIn);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Paused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        animator.Play("BlackBoardFadeOut");        

        GenerateIsland(startPoint);
        for (int i = 1; i <= 4; i++)
        {
            Vector3 RandomXZ = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            RandomXZ.Normalize();
            GenerateIsland(startPoint + new Vector3(RandomXZ.x, -i, RandomXZ.z)*35f);
        }
        player.transform.position = startPoint;
        minHeightYouAchieved = (int)player.transform.position.y;
        theHeight = (int)player.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerController.Dead)
        {
            Paused = false;
            playerInput.cursorLocked = false;
            LockInput();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Paused = Paused ? false : true;
                Cursor.lockState = Paused ? CursorLockMode.None : CursorLockMode.Locked;
                Time.timeScale = Paused ? 0f : 1f;
                PausePanel.SetActive(Paused);
                HintPanelAnim.Play("HintPanelStaticState");
                if (!Paused) HintPanel.SetActive(Paused);
            }
        }

        PauseGame();
        ShowHint();


        if(minHeightYouAchieved > (int)player.transform.position.y)
        {
            minHeightYouAchieved = (int)player.transform.position.y;

            if(Mathf.Abs(minHeightYouAchieved - theHeight)>70)
            {
                generating = true;
            }
            else if(Mathf.Abs(minHeightYouAchieved - theHeight)>14)
            {
                float theProbability = 1f - Mathf.Pow(Mathf.Abs((float)minHeightYouAchieved - (float)theHeight), -1f/35f);
                generating = (Random.Range(0f, 1f) < theProbability) ? true : false;
            }
            else
            {
                generating = false;
            }

            if(generating)
            {
                int islandCount;
                float f = -1f*Mathf.Log(Random.Range(0f, 1f));
                if(f >= 3f)
                {
                    islandCount = 3;
                }
                else if(f >= 1f)
                {
                    islandCount = (int)f;
                }
                else
                {
                    islandCount = 1;
                }

                Vector3 playerPosition = player.transform.position;
                for (int i = 1; i <= islandCount; i++)
                {
                    int islandIndex = Random.Range(0, IslandSet.Length);
                    GameObject island = Instantiate(IslandSet[islandIndex], transform);

                    // Set the position of the island
                    Vector3 RandomXZ = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                    RandomXZ.Normalize();
                    Vector3 islandPositionXZ = Random.Range(0f+(i-1)*35f, 35f+(i-1)*35f)*RandomXZ;
                    float islandPositionX = playerPosition.x + islandPositionXZ.x;
                    float islandPositionY = playerPosition.y - (i-1)*(14f/islandCount) - Random.Range(0f, 14f/islandCount) - 140f;
                    float islandPositionZ = playerPosition.z + islandPositionXZ.z;
                    island.transform.position = new Vector3(islandPositionX, islandPositionY, islandPositionZ);

                    GameObject obj = IslandSet[islandIndex];
                    IslandSet = IslandSet.Where(item => item != obj).ToArray();
                    if(IslandSet.Length == 0)
                    {
                        IslandSet = Islands;
                        IslandSet = IslandSet.Where(item => item != obj).ToArray();
                    }
                    
                    theHeight = (int)islandPositionY + 140;
                }
                generating = false;
            }
        }
    }

    private void GenerateIsland(Vector3 islandPosition)
    {
        int islandIndex = Random.Range(0, IslandSet.Length);
        GameObject island = Instantiate(IslandSet[islandIndex], transform);
        island.transform.position = islandPosition;
        IslandSet = IslandSet.Where(item => item != IslandSet[islandIndex]).ToArray();
    }

    private void PauseGame()
    {
        if (Paused)
        {
            playerInput.cursorLocked = false;
            playerInput.cursorInputForLook = false;
            playerInput.look = Vector2.zero;
            playerInput.jump = false;
            playerInput.deploy = false;
        }
        else
        {
            playerInput.cursorInputForLook = true;
        }
    }

    private void LockInput()
    {
        playerInput.cursorInputForLook = false;
        playerInput.move = Vector2.zero;
        playerInput.look = Vector2.zero;
        playerInput.sprint = false;
        playerInput.jump = false;
        playerInput.peek = false;
        playerInput.deploy = false;
    }

    // Not a good method
    private void ShowHint()
    {
        if(ShowHintWhenAwake)
        {
            if(Input.GetMouseButtonDown(0))
            {
                HintPanel.SetActive(false);
                ShowHintWhenAwake = false;
            }
        }
    }

    public void ResumeButton()
    {
        Paused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        PausePanel.SetActive(Paused);
    }

    public void RestartButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ExitButton()
    {
        Paused = false;
        Time.timeScale = 1f;
        animator.Play("BlackBoardFadeIn");
    }
}