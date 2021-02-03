using Panda;
using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BehaviorActions : MonoBehaviour
{
    string team_tag;
    public GameObject[] team_players;
    string opp_tag;
    public GameObject[] opp_players;
    public GameObject ball;
    public GameObject team_goal;
    public GameObject opp_goal;

    public float kv = 1.0f;
    public float kp = 1.0f;

    private PandaBehaviour panda_bt;
    private Rigidbody2D rb;
    private CharacterController ctrl;

    public float controller_move_force = 50.0f;
    public float max_speed = 20.0f;

    public float aggression = 0.5f;

    public float attack_multiplier = 1.0f;
    public float attack_decay = 0.05f;

    public float defend_distance = 10.0f;
    public float defend_multiplier = 0.1f;

    public float dribble_multiplier = 1.0f;

    public float kick_distance = 2.0f;
    public float kick_multiplier = 1.0f;
    public float kick_decay = 0.1f;
    public float kick_force_max = 2.0f;

    public float action_attack = 0.0f;
    public float action_defend = 0.0f;
    public float action_dribble = 0.0f;
    public float action_kick = 0.0f;

    public Slider slider_attack;
    public Slider slider_defend;
    public Slider slider_dribble;
    public Slider slider_kick;

    public int best_taskid = int.MinValue;
    public float best_score = float.MinValue;



    // Start is called before the first frame update
    void Start()
    {
        panda_bt = GetComponent<PandaBehaviour>();
        rb = GetComponent<Rigidbody2D>();
        ctrl = GetComponent<CharacterController>();

        team_tag = gameObject.tag;
        opp_tag = team_tag == "red_team" ? "blue_team" : team_tag == "blue_team" ? "blue_team" : "invalid";

        ball = UnityEngine.GameObject.Find("ball");
        team_players = GameObject.FindGameObjectsWithTag(team_tag);
        opp_players = GameObject.FindGameObjectsWithTag(opp_tag);
    }

    // Update is called once per frame
    void Update()
    {
        panda_bt.Reset();
        panda_bt.Tick();


        slider_attack.value = action_attack;
        slider_defend.value = action_defend;
        slider_dribble.value = action_dribble;
        slider_kick.value = action_kick;
    }

    public float clamp01(float val)
    {
        val = val > 1.0f ? 1.0f : val;
        val = val < 0.0f ? 0.0f : val;
        return val;
    }

    public void max_combine()
    {
        best_score = 0.0f;
        if (best_score < action_attack)
        {
            best_taskid = 1;
            best_score = action_attack;
        }
        if (best_score < action_defend)
        {
            best_taskid = 2;
            best_score = action_defend;
        }
        if (best_score < action_dribble)
        {
            best_taskid = 3;
            best_score = action_dribble;
        }
        if (best_score < action_kick)
        {
            best_taskid = 4;
            best_score = action_kick;
        }
        //print($"{best_taskid},{action_attack},{action_dribble}");
    }

    [Task]
    void Scorer()
    {
        action_attack = clamp01(score_attack());
        action_defend = clamp01(score_defend());
        action_dribble = clamp01(score_dribble());
        action_kick = clamp01(score_kick());
        max_combine();
        Task.current.Succeed();
    }

    public float score_attack()
    {
        Vector2 ball_position = ball.GetComponent<Rigidbody2D>().position;
        Vector2 my_position = gameObject.GetComponent<Rigidbody2D>().position;
        float distance = (ball_position - my_position).magnitude;
        float score = (1 - (1 / (distance*distance)));
        return score*attack_multiplier;
    }
    public float score_defend()
    {
        Vector2 pos_team_goal = team_goal.GetComponent<Rigidbody2D>().position;
        Vector2 pos_ball = ball.GetComponent<Rigidbody2D>().position;
        float score = 0.0f;
        if((pos_ball-pos_team_goal).magnitude < defend_distance)
        {
            score = defend_distance/(pos_ball - pos_team_goal).magnitude;
        }
        return score*defend_multiplier;
    }

    public float score_dribble()
    {
        Vector2 ball_position = ball.GetComponent<Rigidbody2D>().position;
        Vector2 my_position = gameObject.GetComponent<Rigidbody2D>().position;
        Vector2 dir_me_to_ball = (ball_position - my_position).normalized;
        Vector2 dir_ball_to_goal = (opp_goal.GetComponent<Rigidbody2D>().position - ball.GetComponent<Rigidbody2D>().position).normalized;
        float angle_current_desired = Mathf.Acos(Vector2.Dot(dir_me_to_ball, dir_ball_to_goal));
        // normalize between [0,180] degrees OR [0,PI] radians
        float score = aggression*angle_current_desired/Mathf.PI;
        return score*dribble_multiplier;

    }

    public float score_kick()
    {
        float dist_ball_to_team_goal = (ball.GetComponent<Rigidbody2D>().position - team_goal.GetComponent<Rigidbody2D>().position).magnitude;
        float dist_ball_to_opp_goal = (ball.GetComponent<Rigidbody2D>().position - opp_goal.GetComponent<Rigidbody2D>().position).magnitude;
        float dist_me_to_ball = (ball.GetComponent<Rigidbody2D>().position - gameObject.GetComponent<Rigidbody2D>().position).magnitude;

        float ball_progression = dist_ball_to_team_goal / (dist_ball_to_team_goal + dist_ball_to_opp_goal);

        float score = (1/(dist_me_to_ball* dist_me_to_ball)) * (aggression) * (ball_progression);
        return score*kick_multiplier;
    }



    public void decay()
    {
        // constantly penalize the attack action to simulate player losing stamina
        // clamp to zero
        action_attack  = Mathf.Max(0.0f, action_attack - attack_decay);

        // if I just kicked the ball then penalize the kick action
        if(best_taskid == 4)
        {
            action_kick -= kick_decay;
        }
    }

    [Task]
    void Attack()
    {
        int taskid = 1;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1 )
        {
            //print("inside attack");
            Vector2 desired_position = InterceptPosition(ball, gameObject.GetComponent<Rigidbody2D>().position);
            SimpleGotoPosition(desired_position);
        }
    }

    [Task]
    void Defend(float defend_threshold)
    {
        int taskid = 2;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1 || action_defend > defend_threshold)
        {
           // print("inside defend");
            Vector2 goal_pos = team_goal.GetComponent<Rigidbody2D>().position;
            Vector2 ball_relative_direction = (ball.GetComponent<Rigidbody2D>().position - gameObject.GetComponent<Rigidbody2D>().position).normalized;
            Vector2 desired_position = goal_pos + ball_relative_direction*defend_distance*(1-defend_multiplier);
            SimpleGotoPosition(desired_position);
            Debug.DrawLine(transform.position, ball.GetComponent<Rigidbody2D>().position, Color.cyan);
            KickBall(1.0f);
        }
    }

    [Task]
    void Dribble()
    {
        int taskid = 3;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1)
        {
            //print("inside dribble");
            Vector2 ball_velocity = ball.GetComponent<Rigidbody2D>().velocity;
            Vector2 ball_relative_position = ball.GetComponent<Rigidbody2D>().position - gameObject.GetComponent<Rigidbody2D>().position;
            Vector2 desired_direction = (opp_goal.GetComponent<Rigidbody2D>().position - ball.GetComponent<Rigidbody2D>().position).normalized;
            Vector2 desired_position = ball.GetComponent<Rigidbody2D>().position - desired_direction * 5.0f;
            if (ball_relative_position.magnitude > 5.0f)
            {
                GotoState(desired_position, ball_velocity);
            }
            else
            {
                SimpleGotoPosition(desired_position);
            }
        }
    }

    [Task]
    void KickBall(float kick_multiplier)
    {
        int taskid = 4;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1)
        {
           // print("inside kick");
            Vector2 ball_position = ball.GetComponent<Rigidbody2D>().position;
            Vector2 opp_goal_position = opp_goal.GetComponent<Rigidbody2D>().position;
            Vector2 desired_direction = (opp_goal_position - ball_position).normalized;
            gameObject.GetComponent<CharacterController>().kick_ball_ai(desired_direction, kick_force_max*kick_multiplier);
        }
    }

    [Task]
    void ChaseBall()
    {
        int taskid = -1;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1)
        {
            SimpleGotoPosition(ball.GetComponent<Rigidbody2D>().position);
        }
    }

    [Task]
    void GotoGoal()
    {
        int taskid = -1;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1)
        {
            SimpleGotoPosition(team_goal.GetComponent<Rigidbody2D>().position);
        }
    }


    [Task]
    bool IsBallCloserThan(float distance)
    {
        int taskid = -1;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1)
        {
            return (ball.GetComponent<Rigidbody2D>().position - gameObject.GetComponent<Rigidbody2D>().position).sqrMagnitude < distance * distance;
        }
        return false;
    }

    [Task]
    bool IsBallInOwnZone()
    {
        int taskid = -1;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1)
        {
            float dist_team_goal = (team_goal.GetComponent<Rigidbody2D>().position - ball.GetComponent<Rigidbody2D>().position).sqrMagnitude;
            float dist_opp_goal = (opp_goal.GetComponent<Rigidbody2D>().position - ball.GetComponent<Rigidbody2D>().position).sqrMagnitude;
            return dist_team_goal < dist_opp_goal;
        }
        return false;
    }

    [Task]
    bool IsGoalie()
    {
        int taskid = -1;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1)
        {
            float[] goalie_fitness_values;
            goalie_fitness_values = new float[team_players.Length];
            int i = 0;

            foreach (GameObject team_mate in team_players)
            {
                goalie_fitness_values[i++] = GoalieFitness(team_mate);
            }

            return Mathf.Approximately(goalie_fitness_values.Max(), GoalieFitness(gameObject));
        }
        return false;
    }

    [Task]
    bool IsChaser()
    {
        int taskid = -1;
        Task.current.Succeed();
        if (best_taskid == taskid || taskid == -1)
        {
            float[] goalie_fitness_values;
            goalie_fitness_values = new float[team_players.Length];
            float[] chaser_fitness_values;
            chaser_fitness_values = new float[team_players.Length];
            int i = 0;

            foreach (GameObject team_mate in team_players)
            {
                goalie_fitness_values[i] = GoalieFitness(team_mate);
                chaser_fitness_values[i] = ChaserFitness(team_mate);
                ++i;
            }
            float best_goalie_fitness = goalie_fitness_values.Max();
            int best_goalie_index = Array.IndexOf(goalie_fitness_values, best_goalie_fitness);
            chaser_fitness_values[best_goalie_index] = -1000.0f;
            return Mathf.Approximately(chaser_fitness_values.Max(), ChaserFitness(gameObject));
        }
        return false;
    }

    void SimpleGotoPosition(Vector2 desired_position)
    {
        move_vect(desired_position - gameObject.GetComponent<Rigidbody2D>().position);
    }

    void GotoState(Vector2 desired_position, Vector2 desired_velocity)
    {
        Vector2 position_error = desired_position - gameObject.GetComponent<Rigidbody2D>().position;
        Vector2 velocity_error = desired_velocity - gameObject.GetComponent<Rigidbody2D>().position;
        move_vect(position_error * kp + velocity_error * kv);
    }

    Vector3 InterceptPosition(GameObject target, Vector2 pursuer_position)
    {
        Vector2 target_velocity = target.GetComponent<Rigidbody2D>().velocity;
        Vector2 target_relative_position = target.GetComponent<Rigidbody2D>().position - pursuer_position;
        Vector2 target_direction = target_relative_position.normalized;

        float intercept_time = target_relative_position.magnitude / max_speed;
        Vector3 return_value = target.GetComponent<Rigidbody2D>().position + target_velocity * intercept_time;
        return_value.z = intercept_time;

        Debug.DrawLine(target.GetComponent<Rigidbody2D>().position, return_value);
        Debug.DrawLine(pursuer_position, return_value);

        return return_value;
    }

    float GoalieFitness(GameObject candidate)
    {
        Vector2 goal_to_player = candidate.GetComponent<Rigidbody2D>().position - team_goal.GetComponent<Rigidbody2D>().position;
        Vector2 goal_to_ball = ball.GetComponent<Rigidbody2D>().position - team_goal.GetComponent<Rigidbody2D>().position;

        float distance_beyond_ball = Mathf.Clamp(goal_to_player.magnitude - goal_to_ball.magnitude, 0.0f, 100f);
        float distance_of_center = Mathf.Abs(Vector2.Dot(goal_to_player, goal_to_ball.normalized));
        float distance_from_goal = goal_to_player.magnitude;

        return -distance_beyond_ball - distance_of_center - distance_from_goal;
    }

    float ChaserFitness(GameObject candidate)
    {
        Vector2 opp_goal_to_player = candidate.GetComponent<Rigidbody2D>().position - opp_goal.GetComponent<Rigidbody2D>().position;
        Vector2 opp_goal_to_ball = ball.GetComponent<Rigidbody2D>().position - opp_goal.GetComponent<Rigidbody2D>().position;

        float distance_beyond_ball = (opp_goal_to_player - opp_goal_to_ball).magnitude;
        if (distance_beyond_ball < 0.0f)
        {
            distance_beyond_ball = 100.0f;
        }
        float distance_of_center = Mathf.Abs(Vector2.Dot(opp_goal_to_player, opp_goal_to_ball.normalized));
        return -distance_beyond_ball - distance_of_center;
    }

    float BallProgression()
    {
        float dist_ball_team_goal = (ball.GetComponent<Rigidbody2D>().position - team_goal.GetComponent<Rigidbody2D>().position).magnitude;
        float dist_ball_opp_goal = (ball.GetComponent<Rigidbody2D>().position - opp_goal.GetComponent<Rigidbody2D>().position).magnitude;
        return dist_ball_team_goal / (dist_ball_team_goal + dist_ball_opp_goal);

    }
    void move_vect(Vector2 desired_force)
    {
        gameObject.GetComponent<Rigidbody2D>().AddForce(desired_force*controller_move_force);
    }
}
