using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerCommon : MonoBehaviour
{
    public float speed;
    public float kickforce;
    public Rigidbody2D rb;
    public Vector2 velocity;
    public Vector2 move;
    public string haxis;
    public string vaxis;
    public UnityEngine.KeyCode kick;
    public string player;
    public Vector3 start_position;

    public void Update()
    {
        // Movement
        move = new Vector2(Input.GetAxis(haxis), Input.GetAxis(vaxis));
        velocity = move * speed;
    }
    public void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.deltaTime);
    }

    public void kick_ball(ref GameObject outline)
    {
        if (Input.GetKeyDown(kick))
        {
            space_hit_white(ref outline);
        }
        if (Input.GetKeyUp(kick))
        {
            space_hit_black(ref outline);
        }
    }

    public void space_hit_white(ref GameObject outline)
    {

        outline.SetActive(false);
    }
    public void space_hit_black(ref GameObject outline)
    {
        outline.SetActive(true);
    }
}
