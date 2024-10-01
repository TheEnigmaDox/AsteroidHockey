using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
namespace AsteroidHockey
{
    public static class UtilityFunctions
    {
        //Function to handle collision response.
        static public void cresponse(Vector2 position1, Vector2 position2,
            ref Vector2 velocity1, ref Vector2 velocity2,
            float weight1, float weight2)
        {
            //Calculate collision response directions.
            Vector2 x = position1 - position2;
            x.Normalize();
            Vector2 v1x = x * Vector2.Dot(x, velocity1);
            Vector2 v1y = velocity1 - v1x;
            x = -x;
            Vector2 v2x = x * Vector2.Dot(x, velocity2);
            Vector2 v2y = velocity2 - v2x;

            velocity1 = v1x * (weight1 - weight2) / (weight1 / weight2)
                + v2x * (2 * weight2) / (weight1 + weight2) + v1y;
            velocity2 = v1x * (2 * weight1) / (weight1 + weight2)
                + v2x * (weight2 - weight1) / (weight1 + weight2) + v2y;

#if DEBUG
            Trace.WriteLineIf(float.IsNaN(velocity1.X), "Velocity1.X is not a number!");
            Trace.WriteLineIf(float.IsNaN(velocity1.Y), "Velocity1.Y is not a number!");
            Trace.WriteLineIf(float.IsNaN(velocity2.X), "Velocity1.X is not a number!");
            Trace.WriteLineIf(float.IsNaN(velocity2.Y), "Velocity1.Y is not a number!");
#endif
        }
    }
}
