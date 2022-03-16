using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{


    [Header("Player infos")]
    [SerializeField] private float playerIndex = 0;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private float health = 200f;
    [SerializeField] private Sprite picture;
    [SerializeField] protected bool isAlive = true;
    [SerializeField] private bool isSpawnable = true;

    [Header("Player componenents")]
    [SerializeField] public PlayerController controller = null;
    [SerializeField] public Renderer renderer = null;
    [SerializeField] public Outline outline = null;

    [Header("Animation componenents")]
    [SerializeField] public Animator animator;
    [SerializeField] public Transform visual;


    [Header("Physics componenents")]
    [SerializeField] public Rigidbody body;
    [SerializeField] private CapsuleCollider[] inGameColliders;
    [SerializeField] private Collider[] ragdollColliders;
    [SerializeField] private Rigidbody[] ragdollRigidbodies;
    [SerializeField] private List<Vector3> ragdollPosition= new List<Vector3>();
    [SerializeField] private List<Quaternion> ragdollRotation = new List<Quaternion>();

    public bool GetIsSpawnable()
    {
        return isSpawnable;
    }

    // Start is called before the first frame update
    void Awake()
    {
        visual = transform.Find("Visual");
        controller = GetComponent<PlayerController>();
        body = GetComponent<Rigidbody>();

        inGameColliders = GetComponents<CapsuleCollider>();
        //ragdollColliders = visual.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = visual.GetComponentsInChildren<Rigidbody>();
        if (inGameColliders.Length > 0)
        {
            foreach (Collider col in inGameColliders)
            {
                col.enabled = true;
            }
        }
        if (ragdollColliders.Length > 1)
        {
            foreach (Collider col in ragdollColliders) col.enabled = false;
        }
        if (ragdollRigidbodies.Length > 1)
        {
            foreach (Rigidbody bod in ragdollRigidbodies) 
            { 
                bod.isKinematic = true; 
                ragdollPosition.Add(bod.transform.localPosition); 
                ragdollRotation.Add(bod.transform.localRotation); 
            }
        }
    }
    public int GetIndex()
    {
        return (int) playerIndex;
    }
    public Sprite GetPicture()
    {
        return picture;
    }
    private void Update()
    {
        if (body.velocity.magnitude > 1f) Debug.Log("Player, Update : body.velocity.magnitude = " + body.velocity.magnitude);
        /*if (transform.position.y < -5 && isAlive)
        {
            Die();
        }*/
    }
    public bool GetAlive()
    {
        return isAlive;
    }
    public void SetAlive(bool value)
    {
        isAlive = value;
    }
    public void SetCanMove(bool value)
    {
        controller.SetCanMove(value);
    }

    internal void SetSpawnable(bool v)
    {
        isSpawnable = v;
    }

    public void ResetHealth()
    {
        health = 200f;
    }
    public bool GetHit(float damage, Player opponent)
    {
        if (!isAlive) return true;
        if (health > damage) { health -= damage; return true; }
        Die();
        return false;
    }
    internal void GetReject(Player owner)
    {
        var baseRepulse = 20f;
        if (controller.GetOnFloor()) baseRepulse = 30f;
        var vetDir = (this.transform.position - owner.transform.position).normalized;
        var opponentVect = owner.body.velocity.sqrMagnitude;
        var selfSpeedFactor = - Vector3.Dot(vetDir.normalized, body.velocity.normalized) * body.velocity.sqrMagnitude;
        body.AddForce(vetDir * (opponentVect + baseRepulse + selfSpeedFactor), ForceMode.Impulse);
    }
    private void ActiveRagdoll(bool value)
    {
        if (ragdollColliders.Length > 1)
        {
            foreach (Collider col in ragdollColliders) col.enabled = value;
        }
        if (ragdollRigidbodies.Length > 1)
        {
            foreach (Rigidbody bod in ragdollRigidbodies) bod.isKinematic = !value;
        }
        if (inGameColliders.Length > 0) 
        {

            foreach (Collider col in inGameColliders)
            {
                col.enabled = !value;
            }
            body.isKinematic = value; 
        }

        animator.enabled = !value;
    }
    protected void Die()
    {
        ActiveRagdoll(true);
        health = 0f;
        isAlive = false;
        controller.SetCanMove(false);
        controller.SetFreeze(false);
    }
    public void Revive()
    {
        for (int i = 0; i < ragdollPosition.Count; i++)
        {
            ragdollRigidbodies[i].gameObject.transform.localPosition = ragdollPosition[i];
            ragdollRigidbodies[i].gameObject.transform.localRotation = ragdollRotation[i];
        }
        ActiveRagdoll(false);
    }
    public void PlaySpawnAnim()
    {
        //if (animator != null) animator.Spawn();
    }
    public void SetPlayerIndex(int value)
    {
        playerIndex = value;
        controller.SetPlayerIndex(value);
    }
    public void StopMotion()
    {
        controller.StopMotion();
        controller.SetFreeze(true);
    }
    public void AddHealth(float value)
    {
        health += value;
    }

    public void SetColor(Color color)
    {
        this.color = color;
        var mat = new Material(renderer.sharedMaterial);
        mat.color = color;
        renderer.sharedMaterial = mat;
        outline.OutlineColor = color;
        outline.enabled = true;
    }
}
