using System.Collections.Generic;
using System.IO;
using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    public static class PrototypeDataAssetBuilder
    {
        private const string DataFolder = "Assets/Game/Data";

        [MenuItem("Tools/Code Lyoko Fan Game/Create Prototype Data Assets")]
        public static void CreatePrototypeDataAssets()
        {
            Directory.CreateDirectory(DataFolder);

            CharacterDefinition odd = Character("Odd", LyokoCharacter.Odd, AttackStyle.Ranged, VehicleStyle.Overboard, 100f, 7.8f, 11f, 18f, 32f, true, "Fast ranged fighter. Uses laser arrows and the Overboard.");
            CharacterDefinition ulrich = Character("Ulrich", LyokoCharacter.Ulrich, AttackStyle.Melee, VehicleStyle.Overbike, 100f, 7.2f, 10.5f, 24f, 42f, true, "Close-range katana fighter. Uses speed, dash and the Overbike.");
            CharacterDefinition yumi = Character("Yumi", LyokoCharacter.Yumi, AttackStyle.Telekinetic, VehicleStyle.Overwing, 100f, 7f, 10f, 20f, 38f, true, "Balanced fighter with tessen fans and light telekinesis. Uses the Overwing.");
            CharacterDefinition aelita = Character("Aelita", LyokoCharacter.Aelita, AttackStyle.Ranged, VehicleStyle.Overwing, 100f, 6.6f, 9.2f, 12f, 24f, false, "Tower key character. Uses creativity, shields and Code: LYOKO interactions.");
            CharacterDefinition jeremie = Character("Jeremie", LyokoCharacter.Jeremie, AttackStyle.Operator, VehicleStyle.None, 100f, 5.8f, 7.2f, 4f, 10f, false, "Operator character. Runs mission support, scanners and tower guidance from the lab.");
            jeremie.operatorOnly = true;
            CharacterDefinition william = Character("William", LyokoCharacter.William, AttackStyle.Melee, VehicleStyle.BlackManta, 100f, 7.1f, 10.2f, 28f, 48f, false, "Heavy sword fighter. Can become playable after story progression.");
            CharacterDefinition xanaWilliam = Character("XanaWilliam", LyokoCharacter.XanaWilliam, AttackStyle.Melee, VehicleStyle.BlackManta, 280f, 7.4f, 10.8f, 34f, 58f, false, "X.A.N.A.-controlled William rival/boss form.");

            EnemyDefinition kankrelat = Enemy("Kankrelat", EnemyKind.Kankrelat, EnemyBehaviorStyle.SwarmShooter, 60f, 22f, 15f, 3.8f, 9f, 0.95f, false, false, true, "Weak swarmer. Small and fast, best used in groups.");
            EnemyDefinition blok = Enemy("Blok", EnemyKind.Blok, EnemyBehaviorStyle.Turret, 85f, 26f, 20f, 1.8f, 14f, 1f, false, false, true, "Cube turret with room for laser, fire ring and freeze attacks.");
            EnemyDefinition krab = Enemy("Krab", EnemyKind.Krab, EnemyBehaviorStyle.HeavyWalker, 120f, 28f, 18f, 3.2f, 18f, 1.15f, false, false, true, "Tall heavy monster with leg attacks and strong eye lasers.");
            EnemyDefinition hornet = Enemy("Hornet", EnemyKind.Hornet, EnemyBehaviorStyle.FlyingShooter, 55f, 30f, 22f, 4.8f, 12f, 0.72f, true, false, true, "Flying rapid shooter. Should strafe around the player.");
            EnemyDefinition megatank = Enemy("Megatank", EnemyKind.Megatank, EnemyBehaviorStyle.ArmoredBurst, 260f, 34f, 24f, 1.4f, 34f, 2.4f, false, true, true, "Armored boss. Exposes weak point during heavy laser wind-up.");
            megatank.weakPointAlwaysExposed = false;
            EnemyDefinition tarantula = Enemy("Tarantula", EnemyKind.Tarantula, EnemyBehaviorStyle.EliteShooter, 150f, 32f, 25f, 2.8f, 22f, 0.8f, false, false, false, "Durable elite shooter with high-damage front-leg lasers.");
            EnemyDefinition creeper = Enemy("Creeper", EnemyKind.Creeper, EnemyBehaviorStyle.Ambusher, 75f, 22f, 12f, 5.2f, 18f, 1f, false, false, false, "Sector 5 snake-like ambusher. Dangerous in close range.");
            creeper.contactDamage = 32f;
            EnemyDefinition manta = Enemy("Manta", EnemyKind.Manta, EnemyBehaviorStyle.MineLayer, 120f, 34f, 25f, 4.2f, 18f, 1.1f, true, false, false, "Heavy flying monster that should eventually lay mines.");
            manta.hoverHeight = 5.2f;
            EnemyDefinition blackManta = Enemy("BlackManta", EnemyKind.BlackManta, EnemyBehaviorStyle.MineLayer, 180f, 36f, 26f, 4.8f, 22f, 0.9f, true, false, false, "William's X.A.N.A. manta vehicle/support monster.");
            blackManta.hoverHeight = 5.4f;
            EnemyDefinition kongre = Enemy("Kongre", EnemyKind.Kongre, EnemyBehaviorStyle.DigitalSeaChaser, 90f, 34f, 18f, 5f, 16f, 1.1f, true, false, false, "Digital Sea eel/piranha chaser.");
            kongre.digitalSeaOnly = true;
            EnemyDefinition shark = Enemy("Shark", EnemyKind.Shark, EnemyBehaviorStyle.DigitalSeaChaser, 120f, 34f, 18f, 5.4f, 24f, 1.3f, true, false, false, "Digital Sea shark enemy with torpedo-style attacks.");
            shark.digitalSeaOnly = true;
            EnemyDefinition kalamar = Enemy("Kalamar", EnemyKind.Kalamar, EnemyBehaviorStyle.Grappler, 190f, 30f, 10f, 3.4f, 26f, 1.8f, true, false, false, "Skidbladnir grappler/driller monster.");
            kalamar.digitalSeaOnly = true;
            kalamar.contactDamage = 42f;
            EnemyDefinition guardian = Enemy("Guardian", EnemyKind.Guardian, EnemyBehaviorStyle.CaptureField, 220f, 20f, 6f, 2.2f, 12f, 2f, false, false, false, "Capture-field monster/phenomenon. Should become puzzle-based.");
            guardian.weakPointAlwaysExposed = false;
            EnemyDefinition scyphozoa = Enemy("Scyphozoa", EnemyKind.Scyphozoa, EnemyBehaviorStyle.MemoryDrainer, 260f, 30f, 5f, 2.6f, 20f, 1f, true, false, false, "Memory-draining jellyfish monster used for possession and source-code theft.");
            scyphozoa.hoverHeight = 4.2f;
            scyphozoa.weakPointAlwaysExposed = false;
            EnemyDefinition kolossus = Enemy("Kolossus", EnemyKind.Kolossus, EnemyBehaviorStyle.RaidBoss, 900f, 42f, 30f, 1.1f, 60f, 3.4f, false, true, false, "Final raid boss. Huge lava body, sword arm, multi-weak-point fight.");
            kolossus.weakPointAlwaysExposed = false;
            EnemyDefinition specter = Enemy("Specter", EnemyKind.Specter, EnemyBehaviorStyle.RealWorldPossessor, 80f, 24f, 8f, 3.6f, 10f, 1.2f, false, false, false, "Real-world X.A.N.A. manifestation.");
            EnemyDefinition polymorphicClone = Enemy("PolymorphicClone", EnemyKind.PolymorphicClone, EnemyBehaviorStyle.RealWorldPossessor, 100f, 26f, 10f, 3.8f, 14f, 1.1f, false, false, false, "Disguise/clone enemy for real-world missions.");
            EnemyDefinition zombies = Enemy("Zombies", EnemyKind.Zombies, EnemyBehaviorStyle.RealWorldPossessor, 70f, 18f, 4f, 2.8f, 8f, 1.5f, false, false, false, "Real-world swarm event enemy.");
            EnemyDefinition xanaWilliamEnemy = Enemy("XanaWilliamEnemy", EnemyKind.XanaWilliam, EnemyBehaviorStyle.Duelist, 280f, 34f, 18f, 4.2f, 28f, 1f, false, true, false, "Rival duelist boss with sword and Black Manta support.");
            xanaWilliamEnemy.contactDamage = 45f;

            VehicleDefinition overboard = Vehicle("Overboard", VehicleStyle.Overboard, LyokoCharacter.Odd, 1.7f, true, false, true, "Odd's purple flying board. Highest agility.");
            VehicleDefinition overbike = Vehicle("Overbike", VehicleStyle.Overbike, LyokoCharacter.Ulrich, 1.85f, false, false, true, "Ulrich's green one-wheel bike. Glued to the ground, the fastest; rams enemies.");
            VehicleDefinition overwing = Vehicle("Overwing", VehicleStyle.Overwing, LyokoCharacter.Yumi, 1.45f, true, true, true, "Yumi's stable hover-scooter. Best passenger vehicle.");
            // F08 handling (speeds, turning, tricks, ram, passenger...) lives in VehicleDataBuilder.
            VehicleDataBuilder.ApplyHandling(overboard);
            VehicleDataBuilder.ApplyHandling(overbike);
            VehicleDataBuilder.ApplyHandling(overwing);
            VehicleDefinition skid = Vehicle("Skidbladnir", VehicleStyle.Skidbladnir, LyokoCharacter.Aelita, 1.25f, true, true, false, "Team submarine for the Digital Sea.");
            skid.digitalSeaVehicle = true;
            VehicleDefinition navSkid = Vehicle("NavSkid", VehicleStyle.NavSkid, LyokoCharacter.Odd, 1.6f, true, false, false, "Individual Digital Sea pod launched from the Skidbladnir.");
            navSkid.digitalSeaVehicle = true;
            VehicleDefinition transportOrb = Vehicle("TransportOrb", VehicleStyle.TransportOrb, LyokoCharacter.Aelita, 1.2f, true, true, false, "Orb used for team/sector transport.");
            VehicleDefinition blackMantaVehicle = Vehicle("BlackMantaVehicle", VehicleStyle.BlackManta, LyokoCharacter.XanaWilliam, 1.75f, true, true, false, "X.A.N.A. William's dark flying mount.");

            // Movement traits (Biblia 5): Odd climbs walls and double-jumps, Yumi long-jumps, Aelita glides later.
            SetTraits(odd, true, true, false, false);
            SetTraits(ulrich, false, false, false, false);
            SetTraits(yumi, false, false, true, false);
            SetTraits(aelita, false, false, false, true);
            SetTraits(william, false, false, false, false);
            EnsureMovementProfile();
            EnsureCombatProfile();

            // Melee chains (F04): Ulrich's katana and William's greatsword.
            ulrich.meleeCombo = EnsureCombo("Combo_UlrichKatana",
                ComboHit("Tajo 1", 1f, 1f, 0f, 0.3f, 0.12f, 0.55f, 1.5f, false),
                ComboHit("Tajo 2", 1.1f, 1f, 0.2f, 0.3f, 0.12f, 0.55f, 2f, false),
                ComboHit("Remate", 1.7f, 1.25f, 0.4f, 0.45f, 0.25f, 0.7f, 7f, true));
            EditorUtility.SetDirty(ulrich);
            william.meleeCombo = EnsureCombo("Combo_WilliamSword",
                ComboHit("Mandoble", 1.3f, 1.2f, 0.2f, 0.5f, 0.25f, 0.8f, 4f, false),
                ComboHit("Golpe de titan", 2f, 1.4f, 0.4f, 0.65f, 0.35f, 0.9f, 9f, true));
            EditorUtility.SetDirty(william);

            // Eye of X.A.N.A. (F04): small monsters die from one eye hit; heavy ones barely move when hit.
            SetWeakPointRules(kankrelat, true, 1f);
            SetWeakPointRules(hornet, true, 1f);
            SetWeakPointRules(blok, false, 0.5f);
            SetWeakPointRules(krab, false, 0.5f);
            SetWeakPointRules(megatank, false, 0f);
            SetWeakPointRules(tarantula, false, 0.5f);
            SetWeakPointRules(guardian, false, 0f);
            SetWeakPointRules(kolossus, false, 0f);

            // F06: Season 1 canon values, attacks and reactions (Biblia 8.1).
            EnemyDataBuilder.ApplySeasonOne(kankrelat, blok, krab, hornet, megatank);

            // Abilities (Biblia del juego, seccion 5). Behaviour arrives in F05; this is the tunable data.
            AbilityDefinition oddShield = Ability("odd_shield", "Escudo", LyokoCharacter.Odd, AbilitySlot.Ability1, AbilityKind.Shield, 8f, AbilityResource.None, 0f, 4f, 2, true, "Escudo de energia violeta en el brazo. Bloquea laseres frontales y se rompe si recibe demasiado.");
            AbilityDefinition oddFutureFlash = Ability("odd_future_flash", "Vision del futuro", LyokoCharacter.Odd, AbilitySlot.Ability2, AbilityKind.Precognition, 20f, AbilityResource.None, 0f, 3f, 1, true, "Camara lenta y trayectorias de los disparos enemigos marcadas. Solo en el Capitulo 1 y en modo libre.");
            AbilityDefinition oddClaws = Ability("odd_cat_claws", "Garras felinas", LyokoCharacter.Odd, AbilitySlot.Passive, AbilityKind.Movement, 0f, AbilityResource.None, 0f, 0f, 0, true, "Trepa paredes y hace doble salto.");
            AbilityDefinition oddFelineRain = Ability("odd_feline_rain", "Lluvia felina", LyokoCharacter.Odd, AbilitySlot.Ultimate, AbilityKind.AreaAttack, 0f, AbilityResource.SpecialMeter, 100f, 2f, 1, false, "Salto alto y lluvia de flechas laser en area.");
            AbilityDefinition ulrichSprint = Ability("ulrich_supersprint", "Supersprint", LyokoCharacter.Ulrich, AbilitySlot.Ability1, AbilityKind.Dash, 4f, AbilityResource.None, 0f, 1.2f, 0, true, "Carrera ultrarrapida que atraviesa disparos y golpea al chocar.");
            AbilityDefinition ulrichTriplicate = Ability("ulrich_triplicate", "Triplicado", LyokoCharacter.Ulrich, AbilitySlot.Ability2, AbilityKind.Clone, 14f, AbilityResource.None, 0f, 8f, 1, true, "Dos clones que atraen a los enemigos y hacen poco dano.");
            AbilityDefinition ulrichTriangulate = Ability("ulrich_triangulate", "Triangular", LyokoCharacter.Ulrich, AbilitySlot.Passive, AbilityKind.Clone, 0f, AbilityResource.None, 0f, 0f, 1, true, "Supersprint con los clones activos: rodean al objetivo y el golpe del Ulrich real es critico.");
            AbilityDefinition ulrichImpact = Ability("ulrich_impact", "Impacto", LyokoCharacter.Ulrich, AbilitySlot.Ultimate, AbilityKind.AreaAttack, 0f, AbilityResource.SpecialMeter, 100f, 0.5f, 4, true, "Golpe en area que derriba a todos. Llega con la segunda katana (Temporada 4).");
            AbilityDefinition yumiGrab = Ability("yumi_tk_grab", "Telequinesis: agarrar", LyokoCharacter.Yumi, AbilitySlot.Ability1, AbilityKind.Telekinesis, 2f, AbilityResource.Concentration, 30f, 3f, 0, true, "Levanta un enemigo pequeno o una roca y la lanza.");
            AbilityDefinition yumiWall = Ability("yumi_tk_wall", "Telequinesis: muro", LyokoCharacter.Yumi, AbilitySlot.Ability2, AbilityKind.Telekinesis, 10f, AbilityResource.Concentration, 45f, 5f, 1, true, "Levanta una barrera de terreno que para los laseres.");
            AbilityDefinition yumiFanDance = Ability("yumi_fan_dance", "Danza de abanicos", LyokoCharacter.Yumi, AbilitySlot.Ultimate, AbilityKind.AreaAttack, 0f, AbilityResource.SpecialMeter, 100f, 2.5f, 1, false, "Torbellino de abanicos que atrae a los enemigos cercanos.");
            AbilityDefinition aelitaBridge = Ability("aelita_creativity_bridge", "Creatividad: puente", LyokoCharacter.Aelita, AbilitySlot.Ability1, AbilityKind.Creativity, 6f, AbilityResource.None, 0f, 10f, 0, true, "Crea una plataforma temporal en las zonas marcadas.");
            AbilityDefinition aelitaWall = Ability("aelita_creativity_wall", "Creatividad: muro", LyokoCharacter.Aelita, AbilitySlot.Ability2, AbilityKind.Creativity, 10f, AbilityResource.None, 0f, 6f, 1, true, "Crea un muro que bloquea el paso de los monstruos.");
            AbilityDefinition aelitaWings = Ability("aelita_wings", "Alas", LyokoCharacter.Aelita, AbilitySlot.Passive, AbilityKind.Flight, 0f, AbilityResource.FlightEnergy, 0f, 0f, 3, true, "Planea y vuela con energia limitada (desde la Temporada 3).");
            AbilityDefinition williamSmoke = Ability("william_super_smoke", "Supernube", LyokoCharacter.William, AbilitySlot.Ability1, AbilityKind.Dash, 6f, AbilityResource.None, 0f, 1.5f, 4, true, "Se convierte en humo negro: desplazamiento rapido e invulnerable.");
            AbilityDefinition williamGuard = Ability("william_heavy_guard", "Bloqueo pesado", LyokoCharacter.William, AbilitySlot.Ability2, AbilityKind.Shield, 6f, AbilityResource.None, 0f, 3f, 4, false, "Aguanta casi cualquier golpe de frente.");
            AbilityDefinition williamTitan = Ability("william_titan_strike", "Golpe del Titan", LyokoCharacter.William, AbilitySlot.Ultimate, AbilityKind.AreaAttack, 0f, AbilityResource.SpecialMeter, 100f, 0.8f, 4, false, "Carga y tajo que parte enemigos con armadura.");
            AbilityDefinition ulrichDeflect = Ability("ulrich_katana_deflect", "Desvio con katana", LyokoCharacter.Ulrich, AbilitySlot.Passive, AbilityKind.Shield, 0f, AbilityResource.None, 0f, 0f, 0, true, "Un laser desviado con la guardia en el momento justo vuelve contra quien lo disparo.");
            AbilityDataBuilder.TuneAll(oddShield, oddFutureFlash, oddFelineRain, ulrichSprint, ulrichTriplicate, ulrichTriangulate, ulrichImpact, ulrichDeflect, yumiGrab, yumiWall, yumiFanDance, aelitaBridge, aelitaWall, aelitaWings);
            AbilityDataBuilder.ApplyAttackDetails(odd, aelita);
            AbilityDataBuilder.EnsureSquadProfile();

            SetAbilities(odd, oddShield, oddFutureFlash, oddClaws, oddFelineRain);
            SetAbilities(ulrich, ulrichSprint, ulrichTriplicate, ulrichTriangulate, ulrichImpact, ulrichDeflect);
            SetAbilities(yumi, yumiGrab, yumiWall, yumiFanDance);
            SetAbilities(aelita, aelitaBridge, aelitaWall, aelitaWings);
            SetAbilities(william, williamSmoke, williamGuard, williamTitan);

            SectorDefinition forestSector = EnsureSector(SectorId.Forest, "Sector Bosque", new Color(0.08f, 0.7f, 0.33f), "ForestSectorVerticalSlice", 1, "Arboles gigantes y caminos estrechos. Sector de la vertical slice.");
            SectorDefinition desertSector = EnsureSector(SectorId.Desert, "Sector Desierto", new Color(0.95f, 0.55f, 0.18f), "", 0, "Mesetas y canones. Espacios abiertos para vehiculos.");
            SectorDefinition iceSector = EnsureSector(SectorId.Ice, "Sector Hielo", new Color(0.7f, 0.9f, 1f), "", 0, "Glaciares, suelo resbaladizo y puentes de hielo.");
            SectorDefinition mountainSector = EnsureSector(SectorId.Mountain, "Sector Montana", new Color(0.55f, 0.4f, 0.55f), "", 0, "Montanas flotantes y caminos al borde del vacio.");
            SectorDefinition sector5 = EnsureSector(SectorId.Sector5, "Sector 5 (Cartago)", new Color(0.3f, 0.55f, 1f), "", 0, "Laberinto que cambia, Cupula Celeste, Nucleo de Lyoko y Arena.");
            SectorDefinition digitalSea = EnsureSector(SectorId.DigitalSea, "Mar Digital", new Color(0.1f, 0.85f, 1f), "", 0, "Oceano de datos. Caer = devirtualizacion; en la T4 se navega con el Skidbladnir.");

            LookBuilder.ApplySectorLooks(forestSector, desertSector, iceSector, mountainSector, sector5, digitalSea);
            MissionDefinition forestMission = EnsureForestMission();

            ChapterDefinition prologue = EnsureChapter(0, "Prologo: El despertar de X.A.N.A.", "Precuela", "Jeremie encuentra la fabrica, enciende el superordenador y descubre Lyoko, a Aelita y a X.A.N.A.");
            prologue.missions = new List<MissionDefinition>();
            prologue.unlockedCharacters = new List<LyokoCharacter> { LyokoCharacter.Odd, LyokoCharacter.Ulrich, LyokoCharacter.Yumi };
            prologue.unlockedSectors = new List<SectorDefinition> { forestSector };
            prologue.unlockedAbilities = new List<AbilityDefinition> { ulrichSprint, ulrichDeflect, yumiGrab, aelitaBridge, oddClaws };
            EditorUtility.SetDirty(prologue);

            ChapterDefinition chapter1 = EnsureChapter(1, "Materializar a Aelita", "Temporada 1", "X.A.N.A. activa torres mientras Jeremie trabaja para traer a Aelita al mundo real; al final descubren el virus que la ata al superordenador.");
            chapter1.missions = new List<MissionDefinition> { forestMission };
            chapter1.unlockedCharacters = new List<LyokoCharacter> { LyokoCharacter.Aelita };
            chapter1.unlockedSectors = new List<SectorDefinition> { desertSector, iceSector, mountainSector };
            chapter1.unlockedAbilities = new List<AbilityDefinition> { oddFutureFlash, ulrichTriplicate, yumiWall, aelitaWall };
            EditorUtility.SetDirty(chapter1);

            RosterDatabase roster = LoadOrCreate<RosterDatabase>("RosterDatabase");
            roster.characters.Clear();
            roster.characters.AddRange(new[] { odd, ulrich, yumi, aelita, jeremie, william, xanaWilliam });
            roster.enemies.Clear();
            roster.enemies.AddRange(new[]
            {
                kankrelat, blok, krab, hornet, megatank, tarantula, creeper, manta, blackManta,
                kongre, shark, kalamar, guardian, scyphozoa, kolossus, specter, polymorphicClone,
                zombies, xanaWilliamEnemy
            });
            roster.vehicles.Clear();
            roster.vehicles.AddRange(new[] { overboard, overbike, overwing, skid, navSkid, transportOrb, blackMantaVehicle });
            roster.abilities.Clear();
            roster.abilities.AddRange(new[]
            {
                oddShield, oddFutureFlash, oddClaws, oddFelineRain, ulrichSprint, ulrichTriplicate, ulrichTriangulate,
                ulrichImpact, yumiGrab, yumiWall, yumiFanDance, aelitaBridge, aelitaWall, aelitaWings,
                williamSmoke, williamGuard, williamTitan, ulrichDeflect
            });
            roster.sectors.Clear();
            roster.sectors.AddRange(new[] { forestSector, desertSector, iceSector, mountainSector, sector5, digitalSea });
            roster.missions.Clear();
            roster.missions.Add(forestMission);
            roster.chapters.Clear();
            roster.chapters.AddRange(new[] { prologue, chapter1 });
            EditorUtility.SetDirty(roster);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Prototype data assets created in " + DataFolder);
        }

        // Created once; later rebuilds keep the values tuned in the Inspector.
        public static MovementProfile EnsureMovementProfile()
        {
            string path = DataFolder + "/MovementProfile.asset";
            MovementProfile profile = AssetDatabase.LoadAssetAtPath<MovementProfile>(path);
            if (profile == null)
            {
                Directory.CreateDirectory(DataFolder);
                profile = ScriptableObject.CreateInstance<MovementProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            return profile;
        }

        // Created once; later rebuilds keep the values tuned in the Inspector.
        public static CombatProfile EnsureCombatProfile()
        {
            string path = DataFolder + "/CombatProfile.asset";
            CombatProfile profile = AssetDatabase.LoadAssetAtPath<CombatProfile>(path);
            if (profile == null)
            {
                Directory.CreateDirectory(DataFolder);
                profile = ScriptableObject.CreateInstance<CombatProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            return profile;
        }

        private static ComboDefinition EnsureCombo(string assetName, params ComboStep[] steps)
        {
            ComboDefinition combo = LoadOrCreate<ComboDefinition>("Combos/" + assetName);
            combo.steps = new List<ComboStep>(steps);
            EditorUtility.SetDirty(combo);
            return combo;
        }

        private static ComboStep ComboHit(string stepName, float damage, float radius, float forward, float duration, float windowStart, float windowEnd, float knockback, bool heavy)
        {
            return new ComboStep
            {
                stepName = stepName,
                damageMultiplier = damage,
                radiusMultiplier = radius,
                forwardOffset = forward,
                duration = duration,
                chainWindowStart = windowStart,
                chainWindowEnd = windowEnd,
                knockback = knockback,
                heavy = heavy
            };
        }

        private static void SetWeakPointRules(EnemyDefinition enemy, bool instantKill, float knockbackScale)
        {
            enemy.weakPointInstantKill = instantKill;
            enemy.knockbackScale = knockbackScale;
            EditorUtility.SetDirty(enemy);
        }

        private static void SetTraits(CharacterDefinition character, bool doubleJump, bool wallClimb, bool longJump, bool glide)
        {
            character.canDoubleJump = doubleJump;
            character.canWallClimb = wallClimb;
            character.canLongJump = longJump;
            character.canGlide = glide;
            EditorUtility.SetDirty(character);
        }

        public static MissionDefinition EnsureForestMission()
        {
            SectorDefinition forest = AssetDatabase.LoadAssetAtPath<SectorDefinition>(DataFolder + "/Sectors/Forest.asset");
            MissionDefinition mission = LoadOrCreate<MissionDefinition>("Missions/M01_ForestTower");
            mission.missionId = "M01_ForestTower";
            mission.displayName = "CODE LYOKO: Sector Bosque";
            mission.sector = forest;
            mission.briefing = "X.A.N.A. ha activado una torre en el Sector Bosque. Escoltad a Aelita hasta ella.";
            mission.failedObjective = "Todos devirtualizados. Pulsa Reintentar (R / Y).";
            mission.steps = new List<MissionStep>
            {
                Step(MissionPhase.Intro, "Jeremie: X.A.N.A. ha activado una torre. Entrad en Lyoko.", MissionStepCondition.Immediate, "", 0, 3f, "start"),
                Step(MissionPhase.ReachBridge, "Llega al puente de datos y prueba los vehiculos.", MissionStepCondition.ReachMarker, "bridge", 0, 0f, "start"),
                Step(MissionPhase.ClearCombatPlateau, "Limpia la plataforma de combate.", MissionStepCondition.EnemiesAtMost, "", 1, 0f, "bridge"),
                Step(MissionPhase.EscortAelita, "Escolta a Aelita hasta la torre activada (Z: ordenes a Aelita).", MissionStepCondition.ReachMarker, "tower", 0, 0f, "plateau"),
                Step(MissionPhase.DefeatBoss, "Derrota al Megatank y protege a Aelita.", MissionStepCondition.EnemiesAtMost, "", 0, 0f, "tower"),
                Step(MissionPhase.DeactivateTower, "Con Aelita junto a la torre, manten Interactuar (E / RT): entra y escribe CODE: LYOKO.", MissionStepCondition.TowerCleared, "", 0, 0f, "tower"),
                Step(MissionPhase.Complete, "Torre desactivada. Retorno al pasado listo.", MissionStepCondition.Never, "", 0, 0f)
            };
            EditorUtility.SetDirty(mission);
            return mission;
        }

        private static MissionStep Step(MissionPhase phase, string objective, MissionStepCondition condition, string markerId, int enemyThreshold, float minSeconds, string checkpointId = "")
        {
            return new MissionStep
            {
                checkpointId = checkpointId,
                phase = phase,
                objective = objective,
                condition = condition,
                markerId = markerId,
                enemyThreshold = enemyThreshold,
                minSeconds = minSeconds
            };
        }

        private static AbilityDefinition Ability(string id, string displayName, LyokoCharacter owner, AbilitySlot slot, AbilityKind kind, float cooldown, AbilityResource resource, float cost, float duration, int unlockChapter, bool canon, string description)
        {
            AbilityDefinition definition = LoadOrCreate<AbilityDefinition>("Abilities/" + id);
            definition.abilityId = id;
            definition.displayName = displayName;
            definition.owner = owner;
            definition.slot = slot;
            definition.kind = kind;
            definition.cooldown = cooldown;
            definition.resource = resource;
            definition.cost = cost;
            definition.duration = duration;
            definition.unlockChapter = unlockChapter;
            definition.canon = canon;
            definition.description = description;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void SetAbilities(CharacterDefinition character, params AbilityDefinition[] abilities)
        {
            character.abilities = new List<AbilityDefinition>(abilities);
            EditorUtility.SetDirty(character);
        }

        private static SectorDefinition EnsureSector(SectorId id, string displayName, Color color, string sceneName, int towerCount, string notes)
        {
            SectorDefinition definition = LoadOrCreate<SectorDefinition>("Sectors/" + id);
            definition.sectorId = id;
            definition.displayName = displayName;
            definition.themeColor = color;
            definition.sceneName = sceneName;
            definition.towerCount = towerCount;
            definition.notes = notes;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static ChapterDefinition EnsureChapter(int number, string title, string season, string synopsis)
        {
            ChapterDefinition definition = LoadOrCreate<ChapterDefinition>("Chapters/Chapter" + number.ToString("00"));
            definition.chapterNumber = number;
            definition.title = title;
            definition.seasonLabel = season;
            definition.synopsis = synopsis;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CharacterDefinition Character(string assetName, LyokoCharacter id, AttackStyle attack, VehicleStyle vehicle, float health, float walk, float sprint, float basic, float special, bool playable, string notes)
        {
            CharacterDefinition definition = LoadOrCreate<CharacterDefinition>("Characters/" + assetName);
            definition.character = id;
            definition.displayName = assetName;
            definition.attackStyle = attack;
            definition.vehicleStyle = vehicle;
            definition.maxHealth = health;
            definition.walkSpeed = walk;
            definition.sprintSpeed = sprint;
            definition.basicDamage = basic;
            definition.specialDamage = special;
            definition.playableInVerticalSlice = playable;
            definition.canonNotes = notes;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static EnemyDefinition Enemy(string assetName, EnemyKind kind, EnemyBehaviorStyle behavior, float health, float detection, float attackRange, float speed, float damage, float cooldown, bool flying, bool boss, bool slice, string notes)
        {
            EnemyDefinition definition = LoadOrCreate<EnemyDefinition>("Enemies/" + assetName);
            definition.kind = kind;
            definition.behaviorStyle = behavior;
            definition.displayName = assetName;
            definition.maxHealth = health;
            definition.detectionRange = detection;
            definition.attackRange = attackRange;
            definition.moveSpeed = speed;
            definition.projectileDamage = damage;
            definition.fireCooldown = cooldown;
            definition.flying = flying;
            definition.boss = boss;
            definition.appearsInVerticalSlice = slice;
            definition.canonNotes = notes;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static VehicleDefinition Vehicle(string assetName, VehicleStyle style, LyokoCharacter rider, float boostMultiplier, bool canFly, bool passenger, bool slice, string notes)
        {
            VehicleDefinition definition = LoadOrCreate<VehicleDefinition>("Vehicles/" + assetName);
            definition.style = style;
            definition.displayName = assetName;
            definition.defaultRider = rider;
            definition.boostMultiplier = boostMultiplier;
            definition.canFly = canFly;
            definition.canCarryPassenger = passenger;
            definition.appearsInVerticalSlice = slice;
            definition.canonNotes = notes;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static T LoadOrCreate<T>(string relativePath) where T : ScriptableObject
        {
            string path = DataFolder + "/" + relativePath + ".asset";
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
