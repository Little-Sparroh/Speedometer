using System;
using System.Reflection;
using Pigeon.Movement;
using UnityEngine;

public sealed class PlayerSpeedReader
{
    private readonly FieldInfo currentMoveSpeedField;
    private readonly FieldInfo moveVelocityField;
    private readonly FieldInfo rbField;
    private readonly PropertyInfo rbProp;
    private readonly FieldInfo vkField;
    private readonly PropertyInfo vkProp;

    public PlayerSpeedReader()
    {
        try
        {
            currentMoveSpeedField =
                typeof(Player).GetField("currentMoveSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

            vkField = typeof(Player).GetField("velocity", BindingFlags.NonPublic | BindingFlags.Instance) ??
                      typeof(Player).GetField("velocity", BindingFlags.Public | BindingFlags.Instance);
            if (vkField == null)
                vkProp = typeof(Player).GetProperty("velocity", BindingFlags.Public | BindingFlags.Instance);

            if (vkField == null && vkProp == null)
                rbField = typeof(Player).GetField("rb", BindingFlags.NonPublic | BindingFlags.Instance) ??
                          typeof(Player).GetField("rb", BindingFlags.Public | BindingFlags.Instance);
            if (rbField == null && vkField == null && vkProp == null)
                rbProp = typeof(Player).GetProperty("rb", BindingFlags.Public | BindingFlags.Instance);

            if (rbField == null && rbProp == null && vkField == null && vkProp == null)
                moveVelocityField =
                    typeof(Player).GetField("moveVelocity", BindingFlags.NonPublic | BindingFlags.Instance) ??
                    typeof(Player).GetField("moveVelocity", BindingFlags.Public | BindingFlags.Instance);
        }
        catch (Exception ex)
        {
            SpeedometerPlugin.Logger?.LogError($"Failed to initialize PlayerSpeedReader reflection: {ex.Message}");
        }
    }


    public float Read(Player player)
    {
        if (player == null)
            return 0f;

        var speed = 0f;

        if (vkField != null || vkProp != null)
        {
            if (vkField != null)
            {
                var velObj = vkField.GetValue(player);
                if (velObj is Vector3 vel)
                    speed = vel.magnitude;
            }
            else if (vkProp != null)
            {
                var velObj = vkProp.GetValue(player);
                if (velObj is Vector3 vel)
                    speed = vel.magnitude;
            }
        }
        else if (rbField != null || rbProp != null)
        {
            if (rbField != null)
            {
                var rbObj = rbField.GetValue(player);
                if (rbObj is Rigidbody rb)
                    speed = rb.velocity.magnitude;
            }
            else if (rbProp != null)
            {
                var rbObj = rbProp.GetValue(player);
                if (rbObj is Rigidbody rb)
                    speed = rb.velocity.magnitude;
            }
        }

        if (speed == 0f && currentMoveSpeedField != null)
        {
            var cmsObj = currentMoveSpeedField.GetValue(player);
            if (cmsObj is float cms)
                speed = cms;
        }

        if (speed == 0f && moveVelocityField != null)
        {
            var velObj = moveVelocityField.GetValue(player);
            if (velObj is Vector3 mv)
                speed = mv.magnitude;
        }

        return speed;
    }
}