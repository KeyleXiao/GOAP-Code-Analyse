using UnityEngine;

namespace Player
{
    public class PlayerPhysics
    {
        public bool AddJumpControlForce { get; set; }
        public bool AddJumpDownForce { get; set; }
        public Vector3 MoveDir { get; set; }

        private Rigidbody rb;
        private PlayerAtts player;

        public PlayerPhysics(Rigidbody _rb, PlayerAtts _player)
        {
            AddJumpControlForce = false;
            rb = _rb;
            player = _player;
        }

        public void FixedUpdate()
        {
            if (AddJumpControlForce)
                rb.AddForce(MoveDir * player.jumpProps.airControlForce);
            if (AddJumpDownForce)
                rb.AddForce(Vector3.down * player.jumpProps.airDownForce, ForceMode.Impulse);
        }
    }
}