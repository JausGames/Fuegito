using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Linq;

namespace Inputs
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] PlayerInput playerInput;
        [SerializeField] PlayerController motor = null;
        [SerializeField] private Vector2 move;
        [SerializeField] private Vector2 look;
        [SerializeField] private float index;
        [SerializeField] private bool attack = false;
        [SerializeField] private bool test = false;

        private PlayerAction controls;
        private PlayerAction Controls
        {
        get
            {
                if (controls != null) { return controls; }
                return controls = new PlayerAction();
            }
        }
        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        // Update is called once per frame
        private void Start()
        {
            if (test) FindPlayer();

        }
        public void FindPlayer()
        {
            index = playerInput.playerIndex;
            var motors = FindObjectsOfType<PlayerController>();
            motor = motors.FirstOrDefault(m => m.GetPlayerIndex() == index);

        }
        public void OnMove(CallbackContext context)
        {
            if (motor == null) return;
            move = context.ReadValue<Vector2>();
           //motor.SetMove(move);
        }
        public Vector2 GetMove()
        {
            return move;
        }
        public void OnJump(CallbackContext context)
        {
            if (motor == null || !context.performed) return;
            motor.SetJump();
        }
        public void OnLook(CallbackContext context)
        {
            if (motor == null) return;
            look = context.ReadValue<Vector2>();
            //motor.SetLook(look);
        }
        public Vector2 GetLook()
        {
            return look;
        }
        public void OnAttack(CallbackContext context)
        {
            Debug.Log("OnAttack");
            var canc = context.canceled;
            var perf = context.performed;
        }
        public void OnDash(CallbackContext context)
        {
            Debug.Log("OnDash");
            if (motor == null) return;
            var perf = context.performed;
            var canc = context.canceled;
            motor.Dash(perf, canc);
        }
    }
}
