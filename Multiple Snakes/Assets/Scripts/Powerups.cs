using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerups : MonoBehaviour
{
    [SerializeField] private PowerupDefinition setNone;
    [SerializeField] private PowerupDefinition setRespawnProtection;
    [SerializeField] private PowerupDefinition setSpeedBoost;
    [SerializeField] private PowerupDefinition setInvincible;
    [SerializeField] private PowerupDefinition setIronSnek;
    [SerializeField] private PowerupDefinition setSlowSnek;
    [SerializeField] private PowerupDefinition setDarts;

    public static PowerupDefinition NONE;
    public static PowerupDefinition RESPAWN_PROTECTION;
    public static PowerupDefinition SPEED_BOOST;
    public static PowerupDefinition INVINCIBLE;
    public static PowerupDefinition IRON_SNEK;
    public static PowerupDefinition SLOW_SNEK;
    public static PowerupDefinition DARTS;

    public static List<PowerupDefinition> POWERUPS = new List<PowerupDefinition>();

    private void Awake()
    {
        NONE = setNone;
        RESPAWN_PROTECTION = setRespawnProtection;
        SPEED_BOOST = setSpeedBoost;
        INVINCIBLE = setInvincible;
        IRON_SNEK = setIronSnek;
        SLOW_SNEK = setSlowSnek;
        DARTS = setDarts;

        POWERUPS.Clear();

        POWERUPS.Add(NONE);
        POWERUPS.Add(RESPAWN_PROTECTION);
        POWERUPS.Add(SPEED_BOOST);
        POWERUPS.Add(INVINCIBLE);
        POWERUPS.Add(IRON_SNEK);
        POWERUPS.Add(SLOW_SNEK);
        POWERUPS.Add(DARTS);
    }

    public static PowerupDefinition GetPowerup(int _id)
    {
        foreach (PowerupDefinition powerup in POWERUPS)
        {
            if (powerup.GetID() == _id)
                return powerup;
        }

        return default;
    }
}
