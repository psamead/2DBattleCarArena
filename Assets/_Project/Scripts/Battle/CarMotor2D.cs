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
        private float contactPushMultiplier = 1f;
        private float reboundRemaining;
        private int reboundDirection;

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
            contactPushMultiplier = 1f;
            reboundRemaining = 0f;
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

            if (reboundRemaining > 0f)
            {
                body.AddForce(Vector2.right * (reboundDirection * driveForce * 1.35f), ForceMode2D.Force);
                reboundRemaining = Mathf.Max(0f, reboundRemaining - Time.fixedDeltaTime);
            }
            else
            {
                body.AddForce(Vector2.right * (direction * driveForce * contactPushMultiplier), ForceMode2D.Force);
            }
            Vector2 velocity = body.linearVelocity;
            velocity.x = Mathf.Clamp(velocity.x, -maximumSpeed, maximumSpeed);
            velocity.y = 0f;
            body.linearVelocity = velocity;
        }

        public void SetContactPushMultiplier(float multiplier)
        {
            contactPushMultiplier = Mathf.Clamp(multiplier, 0.5f, 1.5f);
        }

        public void ReboundFromWall(int awayFromWallDirection)
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (body == null) return;
            reboundDirection = awayFromWallDirection < 0 ? -1 : 1;
            reboundRemaining = 0.85f;
            contactPushMultiplier = 1f;
            Vector2 velocity = body.linearVelocity;
            velocity.x = reboundDirection * Mathf.Max(2.2f, Mathf.Abs(velocity.x));
            body.linearVelocity = velocity;
        }
    }
}
