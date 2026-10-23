using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Common skeleton of the four warriors (F9.10): same bone names and proportions, so one procedural animator
    // (and later real animation clips) works for all of them. Built by WarriorModel.
    public sealed class CharacterRig : MonoBehaviour
    {
        [SerializeField] private Transform hips;
        [SerializeField] private Transform spine;
        [SerializeField] private Transform chest;
        [SerializeField] private Transform head;
        [SerializeField] private Transform upperArmL;
        [SerializeField] private Transform upperArmR;
        [SerializeField] private Transform lowerArmL;
        [SerializeField] private Transform lowerArmR;
        [SerializeField] private Transform upperLegL;
        [SerializeField] private Transform upperLegR;
        [SerializeField] private Transform lowerLegL;
        [SerializeField] private Transform lowerLegR;
        [SerializeField] private Transform weaponTip;
        [SerializeField] private List<Transform> tail = new List<Transform>();

        public Transform Hips => hips;
        public Transform Spine => spine;
        public Transform Chest => chest;
        public Transform Head => head;
        public Transform UpperArmL => upperArmL;
        public Transform UpperArmR => upperArmR;
        public Transform LowerArmL => lowerArmL;
        public Transform LowerArmR => lowerArmR;
        public Transform UpperLegL => upperLegL;
        public Transform UpperLegR => upperLegR;
        public Transform LowerLegL => lowerLegL;
        public Transform LowerLegR => lowerLegR;
        // Tip of the katana or the hand that throws (trails start here).
        public Transform WeaponTip => weaponTip;
        public IReadOnlyList<Transform> Tail => tail;

        // Called once by WarriorModel after building the bones.
        public void Collect()
        {
            hips = Find("Hips");
            spine = Find("Spine");
            chest = Find("Chest");
            head = Find("Head");
            upperArmL = Find("UpperArm_L");
            upperArmR = Find("UpperArm_R");
            lowerArmL = Find("LowerArm_L");
            lowerArmR = Find("LowerArm_R");
            upperLegL = Find("UpperLeg_L");
            upperLegR = Find("UpperLeg_R");
            lowerLegL = Find("LowerLeg_L");
            lowerLegR = Find("LowerLeg_R");
            weaponTip = Find("WeaponTip");
            tail.Clear();
            for (int i = 0; ; i++)
            {
                Transform segment = Find("Tail_" + i);
                if (segment == null)
                {
                    break;
                }

                tail.Add(segment);
            }
        }

        private Transform Find(string boneName)
        {
            Transform[] all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == boneName)
                {
                    return all[i];
                }
            }

            return null;
        }
    }
}
