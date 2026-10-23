namespace CodeLyokoFanGame
{
    public enum LyokoCharacter
    {
        Odd,
        Ulrich,
        Yumi,
        Aelita,
        Jeremie,
        William,
        XanaWilliam
    }

    public enum AttackStyle
    {
        Ranged,
        Melee,
        Telekinetic,
        Support,
        Operator
    }

    public enum VehicleStyle
    {
        None,
        Overboard,
        Overbike,
        Overwing,
        Skidbladnir,
        NavSkid,
        TransportOrb,
        BlackManta
    }

    public enum EnemyKind
    {
        Kankrelat,
        Blok,
        Krab,
        Hornet,
        Megatank,
        Tarantula,
        Creeper,
        Manta,
        BlackManta,
        Kongre,
        Shark,
        Kalamar,
        Guardian,
        Scyphozoa,
        Kolossus,
        Specter,
        PolymorphicClone,
        Zombies,
        XanaWilliam,
        DarkMonster,
        Scorpion,
        ExplodingRoachster,
        Mite,
        Volcanoid,
        Insekt,
        Omegatank,
        Ubertank
    }

    public enum EnemyBehaviorStyle
    {
        SwarmShooter,
        Turret,
        HeavyWalker,
        FlyingShooter,
        ArmoredBurst,
        EliteShooter,
        Ambusher,
        MineLayer,
        DigitalSeaChaser,
        Grappler,
        CaptureField,
        MemoryDrainer,
        RaidBoss,
        RealWorldPossessor,
        Duelist,
        Exploder
    }

    public enum MissionPhase
    {
        Intro,
        ReachBridge,
        ClearCombatPlateau,
        EscortAelita,
        DefeatBoss,
        DeactivateTower,
        Complete,
        Failed
    }

    public enum MissionStepCondition
    {
        Immediate,
        ReachMarker,
        EnemiesAtMost,
        TowerCleared,
        Never
    }

    public enum GameEventId
    {
        PlayerDevirtualized,
        EnemyKilled,
        TowerActivated,
        TowerDeactivated,
        MissionFailed,
        MissionComplete
    }

    public enum AbilitySlot
    {
        Ability1,
        Ability2,
        Ultimate,
        Passive
    }

    public enum AbilityKind
    {
        Dash,
        Shield,
        Clone,
        Telekinesis,
        Creativity,
        Projectile,
        AreaAttack,
        Flight,
        Precognition,
        Movement
    }

    public enum AbilityResource
    {
        None,
        SpecialMeter,
        Concentration,
        FlightEnergy
    }

    public enum CreativityKind
    {
        Bridge,
        Wall
    }

    // Orders the player gives Aelita with Z (F7.2).
    public enum AelitaOrder
    {
        Follow,
        Wait,
        Hide,
        GoToTower
    }

    // Halo colour of a tower (Biblia 7): blue neutral, red X.A.N.A., green Jeremie/Aelita, white Franz Hopper.
    public enum TowerState
    {
        Neutral,
        Xana,
        Jeremie,
        FranzHopper
    }

    public enum Team
    {
        Neutral,
        Player,
        Enemy
    }

    public enum DamageType
    {
        Melee,
        Laser,
        Projectile,
        Ability,
        Fall,
        Environment
    }

    public enum SectorId
    {
        Forest,
        Desert,
        Ice,
        Mountain,
        Sector5,
        DigitalSea,
        Replika,
        Cortex
    }

    // F06: enemy brain states and attack kinds.
    public enum EnemyStateId
    {
        Idle,
        Patrol,
        Alert,
        Chase,
        Attack,
        Stagger,
        Dead
    }

    public enum EnemyAttackType
    {
        Laser,
        Burst,
        Melee,
        IceShot,
        FireRing,
        PrisonRay,
        BeamHorizontal,
        BeamVertical
    }

    public enum EnemyAttackPhase
    {
        None,
        Windup,
        Active,
        Recovery
    }
}
