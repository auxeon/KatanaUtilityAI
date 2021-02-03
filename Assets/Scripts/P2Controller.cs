using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Controller : MonoBehaviour
{
    public ControllerCommon ctrl;
    public Vector3 start_position;
    public GameObject outline;
    
    // Start is called before the first frame update
    void Start()
    {
        ctrl = this.gameObject.AddComponent<ControllerCommon>();
        ctrl.rb = GetComponent<Rigidbody2D>();
        ctrl.haxis = "p2_horizontal";
        ctrl.vaxis = "p2_vertical";
        ctrl.kick = KeyCode.N;
        ctrl.speed = 5.0f;
        ctrl.kickforce = 1.0f;
        ctrl.player = "p2";
        start_position = gameObject.GetComponent<Transform>().position;
        ctrl.start_position = start_position;
    }

    // Update is called once per frame
    public void Update()
    {
        ctrl.Update();
        ctrl.kick_ball(ref outline);
    }
    
    // FixedUpdate is called once every physics frame
    public void FixedUpdate()
    {
        ctrl.FixedUpdate();
    }

    public void reset_player_position()
    {
        gameObject.GetComponent<Rigidbody2D>().position = ctrl.start_position;
    }
    public void reset_player_velocity()
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f, 0.0f);
    }
}
