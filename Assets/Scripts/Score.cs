using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int max_score;
    private int p1_score;
    private int p2_score;
    public Text p1_score_text;
    public Text p2_score_text;
    // Start is called before the first frame update
    void Start()
    {
        p1_score = 0;
        p2_score = 0;
        max_score = 3;
        p1_score_text.text = p1_score.ToString();
        p2_score_text.text = p2_score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.name)
        {
            case "goal_line_left":
                gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, 0.0f);
                gameObject.GetComponent<Transform>().position = new Vector2(0.0f, 0.0f);
                print("p2 scores");
                increment_score(ref p2_score);
                p2_score_text.text = p2_score.ToString();
                if(p2_score == max_score)
                {
                    print("p2 wins");
                    Application.Quit();
                }
                break;
            case "goal_line_right":
                gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, 0.0f);
                gameObject.GetComponent<Transform>().position = new Vector2(0.0f, 0.0f);
                print("p1 scores");
                increment_score(ref p1_score);
                p1_score_text.text = p1_score.ToString();
                if (p1_score == max_score)
                {
                    print("p1 wins");
                    Application.Quit();
                }
                break;
        }

        GameObject[] p_blue = GameObject.FindGameObjectsWithTag("blue_team");
        GameObject[] p_red = GameObject.FindGameObjectsWithTag("red_team");

        foreach(GameObject p in p_blue)
        {
            p.GetComponent<CharacterController>().reset_player_position();
            p.GetComponent<CharacterController>().reset_player_velocity();
        }
        foreach (GameObject p in p_red)
        {
            p.GetComponent<CharacterController>().reset_player_position();
            p.GetComponent<CharacterController>().reset_player_velocity();
        }
        /*        var p2 = UnityEngine.GameObject.Find("p2");
                p2.GetComponent<CharacterController>().reset_player_position();
                p2.GetComponent<CharacterController>().reset_player_velocity();
                var p1 = UnityEngine.GameObject.Find("p1");
                p1.GetComponent<CharacterController>().reset_player_position();
                p1.GetComponent<CharacterController>().reset_player_velocity();*/
    }

    void increment_score(ref int score)
    {
        ++score;
    }

    void decrement_score(ref int score)
    {
        --score;
    }
}
