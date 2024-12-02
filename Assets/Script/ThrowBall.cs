using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ThrowBall : MonoBehaviour
{
    [SerializeField] GameObject ball;
    Rigidbody2D ballRigidbody;
    ContactFilter2D groundFilter;
    PlayerInput playerInput;
    //------
    float clock = 10;
    float once = 3;
    float saveOnce;
    float saveTimer;
    bool forceButton;
    public float force;
    int current;
    float goBack;
    [SerializeField] bool canThrow;
    bool isFirstTime;

    public List<int> scores;

    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TextMeshProUGUI score1;
    [SerializeField] TextMeshProUGUI score2;
    [SerializeField] TextMeshProUGUI score3;
    [SerializeField] TextMeshProUGUI score4;
    [SerializeField] TextMeshProUGUI score5;
    [SerializeField] TextMeshProUGUI currentScore;

    [SerializeField] Button begin;
    [SerializeField] Button restart;
    [SerializeField] GameObject menu;
    [SerializeField] GameObject game;
    [SerializeField] GameObject postGame;
    //----
    [SerializeField] GameObject batter;
    Animator batterAnimator;

    void Start()
    {
        ballRigidbody.gravityScale = 0;
        ReadJson();
    }

    void FixedUpdate()
    {
        IsBallTouchingGround();
        if(canThrow){Throw();}
        text.text = "Distância: " + Mathf.FloorToInt(ballRigidbody.transform.position.x).ToString() + "m";
    }

    void IsBallTouchingGround()
    {
        if (ballRigidbody.IsTouching(groundFilter))
        {
            current = Mathf.FloorToInt(ballRigidbody.transform.position.x);

            ballRigidbody.linearVelocity = Vector2.zero;
            ballRigidbody.gravityScale = 0;
            if(ballRigidbody.transform.position.x != 1){SaveToJson();}            

            game.SetActive(false);
            postGame.SetActive(true);
            canThrow = false;

            score1.text = "1º: " + scores[0].ToString() + "m";
            score2.text = "2º: " + scores[1].ToString() + "m";
            score3.text = "3º: " + scores[2].ToString() + "m";
            score4.text = "4º: " + scores[3].ToString() + "m";
            score5.text = "5º: " + scores[4].ToString() + "m";
            currentScore.text = "Atual: " + current.ToString() + "m";
        }
    }

    void Throw()
    {
        if(clock < 10)
        {
            clock += Time.fixedDeltaTime;

            if(forceButton)
            {
                force += 0.1f;
            }
        }
        else
        {
            if(once < Time.fixedDeltaTime)
            {
                once += Time.fixedDeltaTime;
                ballRigidbody.gravityScale = 1;
                ballRigidbody.AddForce(new Vector2(force,10), ForceMode2D.Impulse);
                batterAnimator.PlayInFixedTime("hit");
            }
        }
    }

    private void Awake() 
    {
        GetComponents();
        GetInputs();
        GroundFilter();
        ButtonListener();
    }

    void GetComponents()
    {
        ballRigidbody = ball.GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        batterAnimator = batter.GetComponent<Animator>();
    }

    void GetInputs()
    {
        playerInput.actions["Force"].started += ForceInputHandler;
        playerInput.actions["Force"].canceled += ForceInputHandler;
    }

    void ForceInputHandler(InputAction.CallbackContext context) => forceButton = context.ReadValueAsButton();

    void GroundFilter()
    {
        groundFilter.useNormalAngle = true;
        groundFilter.minNormalAngle = 45;
        groundFilter.maxNormalAngle = 135;
    }

    void Reset()
    {
        ballRigidbody.gravityScale = 0;
        ballRigidbody.transform.position = new Vector3(1,0,0);
        clock = 0;
        once = 0;
        force = 0;
        saveOnce = 0;
        canThrow = true;

        batterAnimator.PlayInFixedTime("idle");
    }

    void SaveToJson()
    {
        Score score = new Score();

        if(saveOnce < Time.fixedDeltaTime)
        {
            saveOnce += Time.fixedDeltaTime;

            foreach(int s in scores)
            {
                if(ballRigidbody.transform.position.x > s)
                {
                    scores.Add(Mathf.FloorToInt(ballRigidbody.transform.position.x));
                    scores.Sort((a,b) => b.CompareTo(a));
                    scores.RemoveAt(5);
                    break;
                }
            }

            score.top1 = scores[0].ToString();
            score.top2 = scores[1].ToString();
            score.top3 = scores[2].ToString();
            score.top4 = scores[3].ToString();
            score.top5 = scores[4].ToString();
        }

        string json = JsonUtility.ToJson(score, true);

        if(saveTimer < Time.fixedDeltaTime)
        {
            saveTimer += Time.fixedDeltaTime;
            File.WriteAllText(Application.dataPath + "/ScoresData.json", json);
        }

    }

    void ReadJson()
    {
        if(File.Exists(Application.dataPath + "/ScoresData.json"))
        {
            string json = File.ReadAllText(Application.dataPath + "/ScoresData.json");
            Score score = JsonUtility.FromJson<Score>(json);      
            AddToList(score);            
        }
        else
        {
            Score score = new Score();            
            string json = JsonUtility.ToJson(score, true);
            AddToList(score);
            File.WriteAllText(Application.dataPath + "/ScoresData.json", json);
        }

    }

    void AddToList(Score score)
    {
        if(score.top1 != "")
        {
            scores.Add(Convert.ToInt32(score.top1));
            scores.Add(Convert.ToInt32(score.top2));
            scores.Add(Convert.ToInt32(score.top3));
            scores.Add(Convert.ToInt32(score.top4));
            scores.Add(Convert.ToInt32(score.top5));           
        }
        else if(score.top1 == "")
        {
            for (int i = 0; i < 5; i++)
            {
                scores.Add(0);
            }
        }
    }

    void ButtonListener()
    {
        begin.onClick.AddListener(Button);
        restart.onClick.AddListener(Button);
    }

    void Button()
    {
        menu.SetActive(false);
        postGame.SetActive(false);
        game.SetActive(true);
        Reset();
    }
}
