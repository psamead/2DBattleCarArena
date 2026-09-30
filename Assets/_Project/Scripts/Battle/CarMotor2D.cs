using UnityEngine;

namespace BattleCarArena.Battle
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CarMotor2D : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField, Min(0.1f)] private float maximumSpeed = 2.75f;
        [SerializeField, Min(0f)] private float forcePerEnginePoint = 0.18f;

        private float driveForce;
        private int direction;
        private bool isDriving;

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            body.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        public void Configure(int enginePower, int horizontalDirection)
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            direction = horizontalDirection < 0 ? -1 : 1;
            driveForce = Mathf.Max(0f, enginePower) * forcePerEnginePoint;
        }

        public void StartDriving()
        {
            isDriving = true;
        }

        public void StopDriving()
        {
            isDriving = false;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            if (!isDriving || body == null)
            {
                return;
            }

            body.AddForce(Vector2.right * (direction * driveForce), ForceMode2D.Force);
            Vector2 velocity = body.linearVelocity;
            velocity.x = Mathf.Clamp(velocity.x, -maximumSpeed, maximumSpeed);
            velocity.y = 0f;
            body.linearVelocity = velocity;
        }
    }
}
