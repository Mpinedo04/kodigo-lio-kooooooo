using NUnit.Framework;
using UnityEngine;

namespace CodeLyokoFanGame.Tests
{
    public sealed class MovementTests
    {
        [Test]
        public void Jump_WorksWithinCoyoteTimeAfterLeavingGround()
        {
            JumpAssist assist = new JumpAssist(0.1f, 0.12f);
            assist.MarkGrounded(1f);
            assist.MarkJumpPressed(1.08f);

            Assert.IsTrue(assist.TryConsumeJump(1.08f));
        }

        [Test]
        public void Jump_FailsAfterCoyoteTimeExpires()
        {
            JumpAssist assist = new JumpAssist(0.1f, 0.12f);
            assist.MarkGrounded(1f);
            assist.MarkJumpPressed(1.2f);

            Assert.IsFalse(assist.TryConsumeJump(1.2f));
        }

        [Test]
        public void Jump_PressedJustBeforeLanding_IsBuffered()
        {
            JumpAssist assist = new JumpAssist(0.1f, 0.12f);
            assist.MarkJumpPressed(2f);
            assist.MarkGrounded(2.1f);

            Assert.IsTrue(assist.TryConsumeJump(2.1f));
        }

        [Test]
        public void Jump_IsConsumedOnlyOnce()
        {
            JumpAssist assist = new JumpAssist(0.1f, 0.12f);
            assist.MarkGrounded(1f);
            assist.MarkJumpPressed(1f);

            Assert.IsTrue(assist.TryConsumeJump(1f));
            Assert.IsFalse(assist.TryConsumeJump(1.01f));
        }

        [Test]
        public void ClearBuffer_PreventsAJumpOnLanding()
        {
            JumpAssist assist = new JumpAssist(0.1f, 0.12f);
            assist.MarkJumpPressed(3f);
            assist.ClearBuffer();
            assist.MarkGrounded(3.05f);

            Assert.IsFalse(assist.TryConsumeJump(3.05f));
        }

        [Test]
        public void Accelerate_ApproachesTargetWithoutOvershooting()
        {
            Vector3 step = MovementMath.Accelerate(Vector3.zero, new Vector3(10f, 0f, 0f), 50f, 0.1f);
            Vector3 full = MovementMath.Accelerate(step, new Vector3(10f, 0f, 0f), 50f, 1f);

            Assert.AreEqual(5f, step.x, 0.0001f);
            Assert.AreEqual(10f, full.x, 0.0001f);
        }

        [Test]
        public void Gravity_IsStrongerWhileFalling_AndClamped()
        {
            float rising = MovementMath.ApplyGravity(5f, -20f, 2f, 40f, 0.1f);
            float falling = MovementMath.ApplyGravity(-5f, -20f, 2f, 40f, 0.1f);
            float terminal = MovementMath.ApplyGravity(-39f, -20f, 2f, 40f, 1f);

            Assert.AreEqual(3f, rising, 0.0001f);
            Assert.AreEqual(-9f, falling, 0.0001f);
            Assert.AreEqual(-40f, terminal, 0.0001f);
        }
    }
}
