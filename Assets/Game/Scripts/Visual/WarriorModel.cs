using UnityEngine;

namespace CodeLyokoFanGame
{
    // Low-poly Lyoko avatars built from code (F9.6-F9.9): Odd (cat), Ulrich (samurai), Yumi (geisha) and
    // Aelita (elf) on the common CharacterRig. Flat colours and readable silhouettes first; real models can
    // replace them later. Used by the level generators and at runtime (Ulrich's clones).
    public static class WarriorModel
    {
        private const float Outline = 1.1f;

        private static readonly Quaternion Down = Quaternion.Euler(180f, 0f, 0f);
        private static readonly Color EyeColor = new Color(0.08f, 0.1f, 0.2f);

        private struct Palette
        {
            public Color suit;
            public Color legs;
            public Color forearm;
            public Color hands;
            public Color skin;
            public Color hair;
            public Color accent;
            public float scale;
        }

        public static CharacterRig Build(LyokoCharacter character, Transform parent)
        {
            Palette p = PaletteOf(character);
            GameObject root = new GameObject("Model");
            root.transform.SetParent(parent, false);
            root.transform.localScale = Vector3.one * p.scale;
            CharacterRig rig = root.AddComponent<CharacterRig>();

            Transform hips = Bone("Hips", root.transform, new Vector3(0f, 0.95f, 0f));
            Transform spine = Bone("Spine", hips, new Vector3(0f, 0.08f, 0f));
            Transform chest = Bone("Chest", spine, new Vector3(0f, 0.2f, 0f));
            Transform neck = Bone("Neck", chest, new Vector3(0f, 0.25f, 0f));
            Transform head = Bone("Head", neck, new Vector3(0f, 0.06f, 0f));

            Part(hips, "Pelvis", LowPolyMesh.Box(new Vector3(0.3f, 0.15f, 0.19f), new Vector2(1.05f, 1f)), Vector3.zero, p.legs);
            Part(spine, "Waist", LowPolyMesh.Box(new Vector3(0.26f, 0.2f, 0.17f)), new Vector3(0f, 0.1f, 0f), p.suit);
            Part(chest, "Torso", LowPolyMesh.Box(new Vector3(0.36f, 0.3f, 0.2f), new Vector2(1.12f, 1.05f)), new Vector3(0f, 0.13f, 0f), p.suit);
            Part(neck, "NeckMesh", LowPolyMesh.Prism(6, 0.05f, 0.045f, 0.08f), Vector3.zero, p.skin);
            Part(head, "Face", LowPolyMesh.Gem(0.13f), new Vector3(0f, 0.12f, 0f), Quaternion.identity, new Vector3(1f, 1.12f, 1.02f), p.skin);
            Part(head, "Eye_L", LowPolyMesh.Box(new Vector3(0.035f, 0.03f, 0.02f)), new Vector3(-0.045f, 0.14f, 0.128f), EyeColor);
            Part(head, "Eye_R", LowPolyMesh.Box(new Vector3(0.035f, 0.03f, 0.02f)), new Vector3(0.045f, 0.14f, 0.128f), EyeColor);

            for (int s = 0; s < 2; s++)
            {
                float side = s == 0 ? -1f : 1f;
                string suffix = s == 0 ? "_L" : "_R";
                Transform shoulder = Bone("UpperArm" + suffix, chest, new Vector3(side * 0.21f, 0.17f, 0f));
                Transform elbow = Bone("LowerArm" + suffix, shoulder, new Vector3(0f, -0.28f, 0f));
                Transform hand = Bone("Hand" + suffix, elbow, new Vector3(0f, -0.26f, 0f));
                Part(shoulder, "UpperArmMesh", LowPolyMesh.Prism(6, 0.055f, 0.048f, 0.27f), Vector3.zero, Down, Vector3.one, p.suit);
                Mesh forearm = character == LyokoCharacter.Yumi
                    ? LowPolyMesh.Prism(6, 0.05f, 0.1f, 0.25f)
                    : LowPolyMesh.Prism(6, 0.047f, 0.04f, 0.25f);
                Part(elbow, "LowerArmMesh", forearm, Vector3.zero, Down, Vector3.one, p.forearm);
                Part(hand, "HandMesh", LowPolyMesh.Gem(0.05f, 0), new Vector3(0f, -0.03f, 0f), p.hands);

                Transform hip = Bone("UpperLeg" + suffix, hips, new Vector3(side * 0.1f, -0.04f, 0f));
                Transform knee = Bone("LowerLeg" + suffix, hip, new Vector3(0f, -0.45f, 0f));
                Transform ankle = Bone("Foot" + suffix, knee, new Vector3(0f, -0.44f, 0f));
                Part(hip, "UpperLegMesh", LowPolyMesh.Prism(6, 0.075f, 0.062f, 0.44f), Vector3.zero, Down, Vector3.one, p.legs);
                Part(knee, "LowerLegMesh", LowPolyMesh.Prism(6, 0.06f, 0.05f, 0.42f), Vector3.zero, Down, Vector3.one, p.legs);
                Part(ankle, "FootMesh", LowPolyMesh.Box(new Vector3(0.1f, 0.08f, 0.22f), new Vector2(0.9f, 0.8f)), new Vector3(0f, 0.01f, 0.04f), p.forearm);
            }

            switch (character)
            {
                case LyokoCharacter.Odd:
                    DressOdd(rig, p, hips, chest, head);
                    break;
                case LyokoCharacter.Ulrich:
                    DressUlrich(p, hips, spine, chest, head);
                    break;
                case LyokoCharacter.Yumi:
                    DressYumi(p, hips, spine, head);
                    break;
                case LyokoCharacter.Aelita:
                    DressAelita(p, hips, chest, head);
                    break;
            }

            if (FindChild(root.transform, "WeaponTip") == null)
            {
                Transform handR = FindChild(root.transform, "Hand_R");
                Bone("WeaponTip", handR, new Vector3(0f, -0.08f, 0.05f));
            }

            rig.Collect();
            return rig;
        }

        // Purple cat suit, blond spiky hair with its purple spot, cat ears, gauntlets and a tail.
        private static void DressOdd(CharacterRig rig, Palette p, Transform hips, Transform chest, Transform head)
        {
            Part(head, "HairSpike", LowPolyMesh.Prism(5, 0.1f, 0f, 0.22f), new Vector3(0f, 0.2f, -0.01f), p.hair);
            Part(head, "HairSpot", LowPolyMesh.Gem(0.035f, 0), new Vector3(0f, 0.27f, 0.035f), p.suit);
            Part(head, "HairBack", LowPolyMesh.Gem(0.13f), new Vector3(0f, 0.15f, -0.03f), Quaternion.identity, new Vector3(1.02f, 0.82f, 1f), p.hair);
            Part(head, "Ear_L", LowPolyMesh.Prism(4, 0.05f, 0f, 0.12f), new Vector3(-0.085f, 0.21f, -0.01f), Quaternion.Euler(0f, 0f, 22f), Vector3.one, p.suit);
            Part(head, "Ear_R", LowPolyMesh.Prism(4, 0.05f, 0f, 0.12f), new Vector3(0.085f, 0.21f, -0.01f), Quaternion.Euler(0f, 0f, -22f), Vector3.one, p.suit);
            Part(chest, "ChestStripe", LowPolyMesh.Box(new Vector3(0.12f, 0.24f, 0.02f)), new Vector3(0f, 0.13f, 0.105f), p.accent, 0.4f);
            for (int s = 0; s < 2; s++)
            {
                Transform forearm = FindChild(chest, s == 0 ? "LowerArm_L" : "LowerArm_R");
                Part(forearm, "Gauntlet", LowPolyMesh.Prism(6, 0.062f, 0.062f, 0.12f), new Vector3(0f, -0.08f, 0f), Down, Vector3.one, p.forearm);
            }

            Transform previous = Bone("Tail_0", hips, new Vector3(0f, -0.02f, -0.1f));
            for (int i = 0; i < 4; i++)
            {
                float radius = 0.035f - i * 0.005f;
                Part(previous, "TailMesh", LowPolyMesh.Prism(5, radius, radius * 0.85f, 0.15f), Vector3.zero, Quaternion.Euler(-100f, 0f, 0f), Vector3.one, p.suit);
                if (i < 3)
                {
                    previous = Bone("Tail_" + (i + 1), previous, new Vector3(0f, 0.03f, -0.14f));
                }
            }
        }

        // Yellow samurai with brown bracers, headband, shoulder guards, sheath and his katana in hand.
        private static void DressUlrich(Palette p, Transform hips, Transform spine, Transform chest, Transform head)
        {
            Color brown = new Color(0.32f, 0.2f, 0.1f);
            Part(head, "Hair", LowPolyMesh.Gem(0.14f), new Vector3(0f, 0.16f, -0.02f), Quaternion.identity, new Vector3(1.02f, 0.85f, 1.05f), p.hair);
            Part(head, "Headband", LowPolyMesh.Ring(0.135f, 0.018f, 12, 4), new Vector3(0f, 0.17f, 0f), Quaternion.Euler(-8f, 0f, 0f), Vector3.one, brown);
            Part(spine, "Belt", LowPolyMesh.Box(new Vector3(0.28f, 0.05f, 0.19f)), new Vector3(0f, 0.02f, 0f), brown);
            Part(chest, "Shoulder_L", LowPolyMesh.Box(new Vector3(0.13f, 0.05f, 0.17f), new Vector2(0.7f, 0.8f)), new Vector3(-0.24f, 0.27f, 0f), Quaternion.Euler(0f, 0f, 18f), Vector3.one, p.forearm);
            Part(chest, "Shoulder_R", LowPolyMesh.Box(new Vector3(0.13f, 0.05f, 0.17f), new Vector2(0.7f, 0.8f)), new Vector3(0.24f, 0.27f, 0f), Quaternion.Euler(0f, 0f, -18f), Vector3.one, p.forearm);
            Part(hips, "Sheath", LowPolyMesh.Box(new Vector3(0.04f, 0.05f, 0.75f)), new Vector3(-0.19f, 0f, -0.08f), Quaternion.Euler(-25f, 0f, 0f), Vector3.one, brown);

            Transform hand = FindChild(chest, "Hand_R");
            Color steel = new Color(0.86f, 0.9f, 0.95f);
            Part(hand, "KatanaGrip", LowPolyMesh.Box(new Vector3(0.03f, 0.03f, 0.2f)), new Vector3(0f, -0.05f, 0.04f), brown);
            Part(hand, "KatanaGuard", LowPolyMesh.Prism(8, 0.05f, 0.05f, 0.015f), new Vector3(0f, -0.05f, 0.14f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, new Color(0.2f, 0.18f, 0.15f));
            Part(hand, "KatanaBlade", LowPolyMesh.Box(new Vector3(0.018f, 0.05f, 0.85f), new Vector2(0.6f, 1f)), new Vector3(0f, -0.05f, 0.58f), steel, 0.25f);
            Bone("WeaponTip", hand, new Vector3(0f, -0.05f, 1f));
        }

        // Black kimono with a red obi and bow, wide sleeves, hair bun with a pin and her fans at the waist.
        private static void DressYumi(Palette p, Transform hips, Transform spine, Transform head)
        {
            Part(head, "Hair", LowPolyMesh.Gem(0.14f), new Vector3(0f, 0.16f, -0.02f), Quaternion.identity, new Vector3(1.03f, 0.9f, 1.06f), p.hair);
            Part(head, "Bun", LowPolyMesh.Gem(0.075f), new Vector3(0f, 0.25f, -0.1f), p.hair);
            Part(head, "HairPin", LowPolyMesh.Box(new Vector3(0.015f, 0.015f, 0.24f)), new Vector3(0f, 0.27f, -0.1f), Quaternion.Euler(0f, 35f, 0f), Vector3.one, p.accent);
            Part(spine, "Obi", LowPolyMesh.Box(new Vector3(0.29f, 0.1f, 0.2f)), new Vector3(0f, 0.04f, 0f), p.accent);
            Part(spine, "ObiBow", LowPolyMesh.Box(new Vector3(0.2f, 0.11f, 0.06f), new Vector2(1.3f, 1f)), new Vector3(0f, 0.05f, -0.12f), p.accent);
            Part(hips, "Fan_L", LowPolyMesh.Sector(0.17f, 40f, 0.02f, 3), new Vector3(-0.17f, 0.05f, 0.02f), Quaternion.Euler(0f, 0f, 90f) * Quaternion.Euler(0f, 90f, 0f), Vector3.one, p.accent);
            Part(hips, "Fan_R", LowPolyMesh.Sector(0.17f, 40f, 0.02f, 3), new Vector3(0.17f, 0.05f, 0.02f), Quaternion.Euler(0f, 0f, -90f) * Quaternion.Euler(0f, -90f, 0f), Vector3.one, p.accent);
        }

        // Pink elf: pointed ears, pink hair, skirt and a light chest piece.
        private static void DressAelita(Palette p, Transform hips, Transform chest, Transform head)
        {
            Part(head, "Hair", LowPolyMesh.Gem(0.145f), new Vector3(0f, 0.15f, -0.015f), Quaternion.identity, new Vector3(1.05f, 0.92f, 1.05f), p.hair);
            Part(head, "Bangs", LowPolyMesh.Box(new Vector3(0.22f, 0.06f, 0.05f), new Vector2(1f, 0.6f)), new Vector3(0f, 0.21f, 0.105f), p.hair);
            Part(head, "Ear_L", LowPolyMesh.Prism(4, 0.035f, 0f, 0.13f), new Vector3(-0.12f, 0.13f, -0.01f), Quaternion.Euler(0f, 0f, 70f), Vector3.one, p.skin);
            Part(head, "Ear_R", LowPolyMesh.Prism(4, 0.035f, 0f, 0.13f), new Vector3(0.12f, 0.13f, -0.01f), Quaternion.Euler(0f, 0f, -70f), Vector3.one, p.skin);
            Part(hips, "Skirt", LowPolyMesh.Prism(8, 0.17f, 0.27f, 0.32f), new Vector3(0f, 0.08f, 0f), Down, Vector3.one, p.suit);
            Part(chest, "ChestPiece", LowPolyMesh.Box(new Vector3(0.22f, 0.1f, 0.03f), new Vector2(0.7f, 1f)), new Vector3(0f, 0.2f, 0.105f), p.accent, 0.3f);
        }

        private static Palette PaletteOf(LyokoCharacter character)
        {
            switch (character)
            {
                case LyokoCharacter.Odd:
                    return new Palette
                    {
                        suit = new Color(0.55f, 0.28f, 0.88f), legs = new Color(0.5f, 0.25f, 0.82f), forearm = new Color(0.3f, 0.13f, 0.52f),
                        hands = new Color(0.3f, 0.13f, 0.52f), skin = new Color(1f, 0.85f, 0.74f), hair = new Color(0.98f, 0.86f, 0.42f),
                        accent = new Color(0.85f, 0.65f, 1f), scale = 1f
                    };
                case LyokoCharacter.Ulrich:
                    return new Palette
                    {
                        suit = new Color(0.92f, 0.78f, 0.32f), legs = new Color(0.84f, 0.7f, 0.3f), forearm = new Color(0.45f, 0.29f, 0.14f),
                        hands = new Color(0.45f, 0.29f, 0.14f), skin = new Color(0.98f, 0.83f, 0.7f), hair = new Color(0.32f, 0.2f, 0.12f),
                        accent = new Color(1f, 0.9f, 0.5f), scale = 1f
                    };
                case LyokoCharacter.Yumi:
                    return new Palette
                    {
                        suit = new Color(0.1f, 0.1f, 0.13f), legs = new Color(0.13f, 0.12f, 0.16f), forearm = new Color(0.1f, 0.1f, 0.13f),
                        hands = new Color(1f, 0.9f, 0.85f), skin = new Color(1f, 0.9f, 0.85f), hair = new Color(0.06f, 0.05f, 0.08f),
                        accent = new Color(0.86f, 0.1f, 0.2f), scale = 1.03f
                    };
                case LyokoCharacter.Aelita:
                    return new Palette
                    {
                        suit = new Color(1f, 0.52f, 0.78f), legs = new Color(1f, 0.8f, 0.9f), forearm = new Color(1f, 0.88f, 0.95f),
                        hands = new Color(1f, 0.87f, 0.8f), skin = new Color(1f, 0.87f, 0.8f), hair = new Color(0.98f, 0.38f, 0.62f),
                        accent = new Color(1f, 0.92f, 0.98f), scale = 0.95f
                    };
                default:
                    return new Palette
                    {
                        suit = new Color(0.4f, 0.4f, 0.45f), legs = new Color(0.3f, 0.3f, 0.35f), forearm = new Color(0.25f, 0.25f, 0.3f),
                        hands = new Color(0.98f, 0.83f, 0.7f), skin = new Color(0.98f, 0.83f, 0.7f), hair = new Color(0.15f, 0.12f, 0.1f),
                        accent = new Color(0.8f, 0.8f, 0.85f), scale = 1f
                    };
            }
        }

        private static Transform Bone(string name, Transform parent, Vector3 localPosition)
        {
            Transform bone = new GameObject(name).transform;
            bone.SetParent(parent, false);
            bone.localPosition = localPosition;
            return bone;
        }

        private static void Part(Transform bone, string name, Mesh mesh, Vector3 position, Color color, float emission = 0f)
        {
            Part(bone, name, mesh, position, Quaternion.identity, Vector3.one, color, emission);
        }

        private static void Part(Transform bone, string name, Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, Color color, float emission = 0f)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(bone, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterial = LyokoMaterials.Toon(color, emission, Outline);
        }

        // Searches the whole model the given transform belongs to.
        private static Transform FindChild(Transform inside, string name)
        {
            CharacterRig model = inside.GetComponentInParent<CharacterRig>();
            Transform[] all = (model != null ? model.transform : inside).GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == name)
                {
                    return all[i];
                }
            }

            return null;
        }
    }
}
