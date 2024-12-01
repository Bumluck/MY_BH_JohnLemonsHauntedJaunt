using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float normalSpeed;
    public float sprintSpeed;
    public float sprintDuration;
    public float sprintCooldown;
    private float currentSpeed;
    private float sprintTimer;
    private float cooldownTimer;

    private bool isSprinting = false;
    private bool canSprint = true;

    public Image sprintIcon;

    public float turnSpeed = 20f;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    AudioSource m_AudioSource;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AudioSource = GetComponent<AudioSource>();

        currentSpeed = normalSpeed / 4;
        sprintTimer = sprintDuration;
        cooldownTimer = 0f;
    }

    void Update()
    {
        Sprint();

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
        }
        else
        {
            m_AudioSource.Stop();
        }

        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);
    }

    void FixedUpdate()
    {
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * currentSpeed * Time.fixedDeltaTime);
        m_Rigidbody.MoveRotation(m_Rotation);
    }

    void OnAnimatorMove()
    {
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);
        m_Rigidbody.MoveRotation(m_Rotation);
    }

    void Sprint()
    {
        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime;
            if (sprintTimer <= 0f)
            {
                isSprinting = false;
                currentSpeed = normalSpeed / 4;
                cooldownTimer = sprintCooldown;
                canSprint = false;
            }
        }
        else if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canSprint = true;
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && canSprint)
        {
            if (!isSprinting)
            {
                isSprinting = true;
                sprintTimer = sprintDuration;
                currentSpeed = sprintSpeed;
            }
        }
        else
        {
            if (!isSprinting && !Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = normalSpeed / 4;
            }
        }

        sprintIcon.enabled = canSprint;
    }

    private void OnTriggerEnter(Collider whatDidIHit)
    {
        if (whatDidIHit.tag == "ScoreGoal")
        {
            GameObject.Find("ScoreManager").GetComponent<ScoreManager>().EarnScore(5);
            Destroy(whatDidIHit.gameObject);

        }
    }

}