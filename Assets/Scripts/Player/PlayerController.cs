using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform visual;
    [SerializeField]  ControllerAnimatorController animator;
    [SerializeField] private int playerIndex = 0;
    [SerializeField]  Rigidbody body;
    [SerializeField] private List<Vector3> wallDirections = new List<Vector3>();
    
    [Header("Move & Look")]
    [SerializeField]  bool canMove = true; 
    [SerializeField]  float speed = 60f;
    [SerializeField]  Vector3 move = Vector3.zero;
    [SerializeField]  float maxSpeed = 14f;
    Quaternion look;
     float lookSmoothTime = 0.1f;
     float maxLookSpeed = 1000f;
    [SerializeField]  float lookSmoothVelocity;
     float turnSmoothTime = 0.2f;
     float maxTurnSpeed = 600f;
    [SerializeField]  float turnSmoothVelocity;
    [SerializeField]  bool rotateWithMove = true;


    [Header("Jump")]
    [SerializeField] float gravityAdd = -1.5f;
    [SerializeField] private int jumpNumber = 1;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private GameObject onFloorGO;
    [SerializeField] private LayerMask floorLayer;

    private const int MAX_JUMP = 2;
    [SerializeField]  bool onFloor = false;
    [SerializeField] private bool falling = false;

    [SerializeField]  bool jump = false;

    [SerializeField] private float jumpCooldown = 1f;
    [SerializeField] private float jumpTimer = 0f;
    [SerializeField] private float inAirTimer = 0f;
    [Header("Wall Jump")]
    private bool onWall = false;
    [SerializeField] float gravityAddOnWall = -0.5f;
    [SerializeField]  bool wallJumpable = false;
    [SerializeField] private Vector3 wallDirection = Vector3.zero;
    [SerializeField] private float wallJumpCooldown = 0.15f;
    [SerializeField] private float wallJumpTimer = 0f;
    [SerializeField]  float onWallTimer = 0f;
    [SerializeField]  const float MAX_WALL_RUN_TIME = 2f;
    [SerializeField]  bool wallJump = false;
    [SerializeField] private float wallJumpForce = 8f;
    [SerializeField] AnimationCurve walljumpUpwardSpeedCurve;

    [Header("Dash")]
     float dashForce = 15f;
    [SerializeField]  float dashCooldown = 0.7f;
     float inAirDashRatio = 0.6f;
    [SerializeField]  float dashTimer = 0f;
    [SerializeField] Inputs.PlayerInputHandler playerInput;

  void Awake()
    {
        SetUp();
    }
   void SetUp()
    {
        for (float x = -Mathf.PI; x < Mathf.PI; x += Mathf.PI * 0.125f)
        {
            wallDirections.Add(new Vector3(Mathf.Cos(x), 0f, Mathf.Sin(x)) * 0.55f);
        }

        visual = transform.Find("Visual");
        body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        speed = (speed + body.drag * 17f) * 0.01f;
    }
    public int GetPlayerIndex()
    {
        return playerIndex;
    }
    public void SetPlayerIndex(int nb)
    {
        playerIndex = nb;
    }
   
    // Update is called once per frame
  void FixedUpdate()
    {
        SetLook(playerInput.GetLook());
        SetMove(playerInput.GetMove());
        UpdatePlayerPosition();
    }
   void UpdatePlayerPosition()
    {
        CheckOnWall();
        var cols = Physics.OverlapSphere(onFloorGO.transform.position, 0.2f, floorLayer);
        var totalForward = CheckOnFloor(cols);

        if (!canMove) return;

        var moveVectX = 0f;
        var moveVectY = 0f;
        var moveVectZ = 0f;
        var wallDir = Vector3.zero;

        CheckMove(ref moveVectX, ref moveVectZ);
        CheckRotation();

        CheckJump(ref moveVectX, ref moveVectY, ref moveVectZ, ref wallDir);

        UpdateRigidbody(totalForward, moveVectX, moveVectY, moveVectZ, wallDir);
        UpdateAnimator();
    }

    private void CheckOnWall()
    {
        var wallContact = 0;
        foreach (Vector3 dir in wallDirections)
        {
            var wallCol = Physics.OverlapSphere(transform.position + dir - 0f * Vector3.up, 0.05f, floorLayer);
            wallContact += wallCol.Length;
            if (wallCol.Length > 0)
            {
                var currDir = -dir;
                wallDirection += new Vector3(currDir.x, 0f, currDir.z);
            }
        }

        if (wallContact > 0)
        {
            wallDirection = wallDirection.normalized;
            if (Vector3.Dot(wallDirection, transform.right) > 0) animator.SetLeftWall(true);
            else animator.SetLeftWall(false);
            animator.SetOnWall(true);
            onWallTimer += Time.deltaTime;
            wallJumpable = true;
        }
        else
        {
            wallDirection = Vector3.zero;
            animator.SetOnWall(false);
            onWallTimer = 0f;
            wallJumpable = false;
        }
    }

    private void CheckMove(ref float moveVectX, ref float moveVectZ)
    {
        //make him move
        if (move.magnitude >= 0.2f)
        {
            MovePlayer(out moveVectX, out moveVectZ);
            if (onFloor) ForceForwardDirection(true, transform.InverseTransformDirection(body.velocity));
        }
    }

    private void CheckRotation()
    {
        //make him look around
        if (Mathf.Abs(playerInput.GetLook().x) >= 0.15f) RotatePlayer(look, lookSmoothTime);
        else if (Mathf.Abs(playerInput.GetMove().x) >= 0.3f && rotateWithMove) RotatePlayer(Quaternion.Euler(0, Vector3.Dot(transform.right, move) * 50f, 0), turnSmoothTime * (1f / Mathf.Abs(playerInput.GetMove().x)) );
        else if (Mathf.Abs(playerInput.GetMove().x) >= 0.3f && rotateWithMove) RotatePlayer(Quaternion.Euler(0, Vector3.Dot(transform.right, move) * 50f, 0), turnSmoothTime * (1f / Mathf.Abs(playerInput.GetMove().x)) );
        else RotatePlayer(Quaternion.identity, lookSmoothTime);
    }

    private void CheckJump(ref float moveVectX, ref float moveVectY, ref float moveVectZ, ref Vector3 wallDir)
    {
        //make him jump
        if ((jump && Time.time > jumpTimer && jumpNumber > 0))
        {
            Debug.Log("PlayerController, FixedUpdate : Jump");
            jumpNumber--;


            var horizVelocity = body.velocity.normalized.x * Vector3.right + body.velocity.normalized.z * Vector3.forward;
            var vertVelocity = body.velocity.normalized.y * Vector3.up;

            var hspeedFactor = (Vector3.Dot(horizVelocity, transform.forward) * horizVelocity.magnitude) * (2f * 0.3f);
            var horizFactor = Mathf.Clamp(2 + hspeedFactor, 0f, 3f);

            var vspeedFactor = (Vector3.Dot(vertVelocity, transform.forward) * vertVelocity.magnitude) * (jumpForce * 0.3f);
            var vertFactor = Mathf.Clamp(jumpForce - vspeedFactor, jumpForce * 0.25f, jumpForce);

            var upwardModifier = 0f;
            if (body.velocity.y < 0) upwardModifier = -body.velocity.y;

            Debug.Log("PlayerController, Jump : horizVel = " + (body.velocity.normalized.x * Vector3.right + body.velocity.normalized.z * Vector3.forward));
            Debug.Log("PlayerController, Jump : horizForce = " + horizFactor);
            Debug.Log("PlayerController, Jump : vertVel = " + body.velocity.normalized.z);
            Debug.Log("PlayerController, Jump : vertForce = " + vertFactor);

            jump = false;
            moveVectX = horizFactor * moveVectX;
            moveVectZ = horizFactor * moveVectZ;
            moveVectY = jumpForce + upwardModifier;
            jumpTimer = Time.time + jumpCooldown;
        }
        else if (wallJump && wallJumpTimer < Time.time)
        {
            var horizVelocity = body.velocity.x * Vector3.right + body.velocity.z * Vector3.forward;

            var upwardModifier = 0f;
            if (body.velocity.y < 0) upwardModifier = -body.velocity.y;

            var calculatedWallJumpForce = Mathf.Clamp(wallJumpForce * horizVelocity.magnitude * 0.1f, 5f, wallJumpForce * 1.5f);

            Debug.Log("PlayerController, WallJump : horizVelocity = " + horizVelocity.magnitude);
            Debug.Log("PlayerController, WallJump : calculatedWallJumpForce = " + calculatedWallJumpForce);

            wallJump = false;
            wallDir = wallDirection * calculatedWallJumpForce ;
            //moveVectY = wallJumpForce * walljumpUpwardSpeedCurve.Evaluate(body.velocity.y) + upwardModifier;
            moveVectY = wallJumpForce * 1.2f + upwardModifier;
            wallJumpTimer = Time.time + wallJumpCooldown;
        }
        else if (!wallJumpable && falling)
            moveVectY = gravityAdd;
        else if (onWallTimer > MAX_WALL_RUN_TIME && falling)
            moveVectY = gravityAddOnWall;
    }


    private void UpdateRigidbody(Vector3 totalForward, float moveVectX, float moveVectY, float moveVectZ, Vector3 wallDir)
    {
        if (onFloor) MoveBody(25f * Vector3.up * Time.deltaTime);
        if (wallJumpable && (moveVectX * Vector3.right + moveVectZ * Vector3.forward).magnitude / speed < 0.2f)
            MoveBody(-0.05f * body.velocity);

        if (wallJumpable)
        {
            var dot = Vector3.Dot(move, wallDirection);
            Debug.Log("PlayerController, UpdateRigidbody : dot = " + dot);
            Debug.Log("PlayerController, UpdateRigidbody : wallDir = " + wallDirection);
            var addForce = dot < 0.05f && dot > -0.05f;
            if (addForce) MoveBody(-wallDirection);
        }

        MoveBody(moveVectX * Vector3.right + moveVectY * Vector3.up + moveVectZ * Vector3.forward + totalForward.y * Vector3.up + wallDir);


        if ((wallJumpable && onWallTimer <= MAX_WALL_RUN_TIME) && body.velocity.y < 0f)
            MoveBody(-body.velocity.y * Vector3.up);
    }

    private Vector3 CheckOnFloor(Collider[] cols)
    {
        if (cols.Length == 0)
        {
            inAirTimer += Time.fixedDeltaTime;
            if (onFloor)
            {
                animator.SetInAir(true);
                onFloor = false;
            }
            if (inAirTimer > 0.1f)
            {
                falling = true;
            }
        }
        else if (!onFloor)
        {

            onWallTimer = 0f;
            inAirTimer = 0f;
            animator.SetInAir(false);
            jumpNumber = MAX_JUMP;
            onFloor = true;
            falling = false;
        }
        else
        {
            var forwardOffset = 0.6f * transform.forward;
            var upOffset = -0.8f * transform.up;
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position + upOffset, Vector3.down, out hit, Mathf.Infinity, floorLayer))
            {
                var vect = (hit.normal).normalized;
                Debug.DrawRay(transform.position + upOffset, (Vector3.down + forwardOffset) * hit.distance * 2, Color.blue);
                Debug.DrawRay(transform.position + upOffset, vect * hit.distance * 2, Color.blue);
                var vectX = transform.forward;
                var vectY = Mathf.Sqrt((1f - Mathf.Pow(vect.y, 2f))) * Vector3.up;
                vectY = vectY * Vector3.Dot(transform.forward, vect.x * Vector3.right + vect.y * Vector3.forward);
                Debug.DrawRay(transform.position, vectX, Color.magenta);
                Debug.DrawRay(transform.position, vectY, Color.magenta);
                var totalForward = vectY + vectX;
                var transformedForward = Vector3.Cross(totalForward, Vector3.right).magnitude * Mathf.Sign(Vector3.Dot(totalForward, Vector3.forward)) * Vector3.forward;
                var transformedRight = Vector3.Cross(totalForward, Vector3.forward).magnitude * Mathf.Sign(Vector3.Dot(totalForward, Vector3.right)) * Vector3.right;
                var transformedUp = Vector3.up * totalForward.y;
                Debug.DrawRay(transform.position + forwardOffset + upOffset, totalForward, Color.yellow);
                Debug.DrawRay(transform.position + forwardOffset + upOffset, transformedRight, Color.green);
                Debug.DrawRay(transform.position + forwardOffset + upOffset, transformedForward, Color.green);
                Debug.DrawRay(transform.position + forwardOffset + upOffset, transformedUp, Color.cyan);
                return totalForward;
            }
        }

        return Vector3.zero;
    }

    private void UpdateAnimator()
    {
        if (move.magnitude >= 0.05f) animator.WalkForward();
        else animator.ToIdle();

        animator.SetSpeed(body.velocity.magnitude / maxSpeed);
    }




    void MovePlayer(out float moveVectX, out float moveVectZ)
    {
        var speed = this.speed;
        var maxSpeed = this.maxSpeed;
        var lookSmoothTime = this.lookSmoothTime;
        var turnSmoothTime = this.turnSmoothTime;
        if (!onFloor) ApplyInAirModifier(out speed, out maxSpeed, out lookSmoothTime, out turnSmoothTime);
        if (wallJumpable && onWallTimer <= MAX_WALL_RUN_TIME) ApplyOnWallModifier(out speed, out maxSpeed, out lookSmoothTime, out turnSmoothTime);

        Debug.Log("PlayerController, MovePlayer : sign x = " + (Mathf.Sign(move.x) == Mathf.Sign(body.velocity.x)) + ", sign z = " + (Mathf.Sign(move.z) == Mathf.Sign(body.velocity.z)));
        moveVectX = move.x * speed * Mathf.Clamp((maxSpeed - Mathf.Abs(body.velocity.x)) / maxSpeed, 0f, 1f);
        moveVectZ = move.z * speed * Mathf.Clamp((maxSpeed - Mathf.Abs(body.velocity.z)) / maxSpeed, 0f, 1f);
    }

   void ForceForwardDirection(bool fastSpeed, Vector3 localVelocity)
    {
        if (!fastSpeed && onFloor)
        {
            //moslty have forward speed
            var driftSpeedRatio = Mathf.Pow(localVelocity.x, 2f) / Mathf.Pow(Mathf.Abs(localVelocity.x) + Mathf.Abs(localVelocity.z), 2f);
            //var breakRatio = carSettings.stopDriftingCurve.Evaluate(driftSpeedRatio);
            var breakRatio = 0.2f;
            localVelocity.x *= breakRatio;
            //body.velocity = Vector3.Lerp(body.velocity, transform.TransformDirection(localVelocity), carSettings.driftSpeedBoostRatio * Time.deltaTime);
            body.velocity = Vector3.Lerp(body.velocity, transform.TransformDirection(localVelocity), 1.5f * Time.deltaTime);
        }
    }

    void MoveBody(Vector3 speed)
    {
        body.velocity += speed;
    }

     public void RotatePlayer(Quaternion inputLook, float smoothFactor)
    {
        var speed = this.speed;
        var maxSpeed = this.maxSpeed;
        var lookSmoothTime = this.lookSmoothTime;
        var turnSmoothTime = this.turnSmoothTime;
        if (!onFloor) ApplyInAirModifier(out speed, out maxSpeed, out lookSmoothTime, out turnSmoothTime);
        if (wallJumpable && onWallTimer <= MAX_WALL_RUN_TIME) ApplyOnWallModifier(out speed, out maxSpeed, out lookSmoothTime, out turnSmoothTime);

        var look = inputLook * transform.forward;
        float targetLookAngle = Mathf.Atan2(look.x, look.z) * Mathf.Rad2Deg;
        float lookAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetLookAngle, ref lookSmoothVelocity, smoothFactor, maxLookSpeed);
        transform.rotation = Quaternion.Euler(0f, lookAngle, 0f);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
    public bool GetCanMove()
    {
        return canMove;
    }
   void ApplyInAirModifier(out float speed, out float maxSpeed, out float lookSmoothTime, out float turnSmoothTime)
    {
        speed = this.speed / 1.5f;
        maxSpeed = this.maxSpeed / 3f;
        lookSmoothTime = this.lookSmoothTime * 1.5f;
        turnSmoothTime = this.turnSmoothTime * 3f;

    }
   void ApplyOnWallModifier(out float speed, out float maxSpeed, out float lookSmoothTime, out float turnSmoothTime)
    {
        speed = this.speed / 1.2f;
        maxSpeed = this.maxSpeed / 1.4f;
        lookSmoothTime = this.lookSmoothTime * 2f;
        turnSmoothTime = this.turnSmoothTime * 2f;

    }
    public void SetMove(Vector2 value)
    {
        move = transform.forward * value.y + transform.right * value.x;
    }
     public void SetJump()
    {
        Debug.Log("PlayerControler, StartJump : Jump");
        if (onFloor && !falling && !jump)
        {
            animator.PlayJump();
            jump = true;
        }
        else if(!jump && wallJumpable && !wallJump)
        {
            animator.PlayJump();
            wallJump = true;
        }
    }
     public void Dash(bool perf, bool canc)
    {
        Debug.Log("PlayerControler, Dash");
        if (perf && dashTimer < Time.time && !jump && !wallJump)
        {
            animator.PlayDash();
            dashTimer = Time.time + dashCooldown;


            var horizVelocity = body.velocity.normalized.x * Vector3.right + body.velocity.normalized.z * Vector3.forward;
            var speedFactor = (Vector3.Dot(horizVelocity, transform.forward) * horizVelocity.magnitude) * (dashForce * 0.3f);
            var Force = Mathf.Clamp(dashForce - speedFactor, dashForce / 3f, dashForce * 1.66667f);
            var upwardModifier = Vector3.zero;
            if (body.velocity.y < 0) upwardModifier = - body.velocity.y * Vector3.up;
            Debug.Log("PlayerController, Dash ; vel = " + body.velocity.normalized.x * Vector3.right + body.velocity.normalized.z * Vector3.forward);
            Debug.Log("PlayerController, dash ; dashForce = " + Force);
            if (!onFloor) Force = Force * inAirDashRatio;
            MoveBody(transform.forward * Force + upwardModifier);
        }
    }
    public void StartFalling()
    {
        falling = true;
    }
    public bool GetOnFloor()
    {
        return onFloor;
    }
     public void SetLook(Vector2 value)
    {
        look = Quaternion.Euler(0, value.x * 50f, 0);
    }

    public void StopMotion()
    {
        MoveBody(-body.velocity);
    }
    public void SetFreeze(bool value)
    {
        if (value)
        {
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            body.constraints = RigidbodyConstraints.None;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (onFloorGO) Gizmos.DrawWireSphere(onFloorGO.transform.position, 0.01f);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 dir in wallDirections)
        {
            Gizmos.DrawWireSphere(transform.position + dir - 0f * Vector3.up, 0.1f);
        }
        Gizmos.color = Color.blue;
        if (wallDirection != Vector3.zero)
        {
            Gizmos.DrawLine(transform.position, transform.position + wallDirection * 2f);
        }
        
    }

}
