using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static string GenerateGuid()
    {
        return System.Guid.NewGuid().ToString();
    }

    public static CardinalDirection GetMouseDirectionFromPlayer(Transform player)
    {
        // mouse pos in screen space
        Vector3 mouseScreen = Input.mousePosition;

        // player pos in screen space
        Vector3 playerScreen = Camera.main.WorldToScreenPoint(player.position);

        Vector2 toMouse = new Vector2(mouseScreen.x - playerScreen.x, mouseScreen.y - playerScreen.y).normalized;

        // player forward & right in screen space
        Vector3 forward = Camera.main.WorldToScreenPoint(player.position + player.forward) - playerScreen;
        Vector3 right = Camera.main.WorldToScreenPoint(player.position + player.right) - playerScreen;

        Vector2 screenForward = new Vector2(forward.x, forward.y).normalized;
        Vector2 screenRight = new Vector2(right.x, right.y).normalized;

        
        float fDot = Vector2.Dot(toMouse, screenForward);
        float rDot = Vector2.Dot(toMouse, screenRight);

        if (Mathf.Abs(fDot) > Mathf.Abs(rDot))
            return fDot >= 0 ? CardinalDirection.Down : CardinalDirection.Up;
        else
            return rDot >= 0 ? CardinalDirection.Right : CardinalDirection.Left;
    }
}
