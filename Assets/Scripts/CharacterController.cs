using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{

    public KeyCode kick;
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;

    public Vector2 velocity;
    public Vector2 move;
    public float speed;
    public float dx = 0.0f;
    public float dy = 0.0f;
    public float step_size = 0.000001f;
    public float kickforce = 2.0f;

    private Vector2 start_position;
    private Rigidbody2D rb;
    GameObject ball;

    public string haxis, vaxis;


    public GameObject outline;

    public void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        start_position = GetComponent<Rigidbody2D>().position;
        dx = dy = 0.0f;
        ball = GameObject.Find("ball");

    }
    public void Update()
    {
        move = new Vector2(Input.GetAxis(haxis), Input.GetAxis(vaxis));
        velocity = move * speed;
        kick_ball();
    }

    public void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.deltaTime);
    }


    public void kick_ball()
    {

        Rigidbody2D ballrb = ball.GetComponent<Rigidbody2D>();
        //Physics2D ballphy = ball.GetComponent<>();
        if (Input.GetKeyDown(kick))
        {
            space_hit_white(ref outline);
            Vector2 ballvec = ballrb.position - gameObject.GetComponent<Rigidbody2D>().position;
            Vector2 balldir = ballvec.normalized;
            if (ballvec.magnitude < 1.5f)
            {
                ballrb.AddForce(balldir * kickforce, ForceMode2D.Impulse);
            }
        }
        if (Input.GetKeyUp(kick))
        {
            space_hit_black(ref outline);
        }
    }

    public void kick_ball_ai(Vector2 desired_direction, float magnitude)
    {

        Rigidbody2D ballrb = ball.GetComponent<Rigidbody2D>();
        //Physics2D ballphy = ball.GetComponent<>();
            space_hit_white(ref outline);
            Vector2 ballvec = ballrb.position - gameObject.GetComponent<Rigidbody2D>().position;
            if (ballvec.magnitude < 1.5f)
            {
                ballrb.AddForce(desired_direction * magnitude, ForceMode2D.Impulse);
            }
            space_hit_black(ref outline);
    }

    public void space_hit_white(ref GameObject outline)
    {

        outline.SetActive(false);
    }
    public void space_hit_black(ref GameObject outline)
    {
        outline.SetActive(true);
    }
    public void reset_player_position()
    {
        gameObject.GetComponent<Rigidbody2D>().position = start_position;
    }
    public void reset_player_velocity()
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, 0.0f);
    }
}
