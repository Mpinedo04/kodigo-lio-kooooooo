using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Picks the behaviour of each AbilityDefinition by its id. Passive traits that PlayerController already
    // handles (Odd's claws) and abilities of characters not playable yet return null.
    public static class AbilityFactory
    {
        public static Ability Create(AbilityDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            switch (definition.abilityId)
            {
                case "odd_shield":
                    return new OddShieldAbility(definition);
                case "odd_future_flash":
                    return new FutureVisionAbility(definition);
                case "odd_feline_rain":
                    return new FelineRainAbility(definition);
                case "ulrich_supersprint":
                    return new SupersprintAbility(definition);
                case "ulrich_triplicate":
                    return new TriplicateAbility(definition);
                case "ulrich_impact":
                    return new ImpactAbility(definition);
                case "ulrich_katana_deflect":
                    return new KatanaDeflectAbility(definition);
                case "yumi_tk_grab":
                    return new TelekinesisGrabAbility(definition);
                case "yumi_tk_wall":
                    return new TelekinesisWallAbility(definition);
                case "yumi_fan_dance":
                    return new FanDanceAbility(definition);
                case "aelita_creativity_bridge":
                    return new CreativityAbility(definition, CreativityKind.Bridge);
                case "aelita_creativity_wall":
                    return new CreativityAbility(definition, CreativityKind.Wall);
                case "aelita_wings":
                    return new WingsAbility(definition);
                default:
                    return null;
            }
        }
    }
}
