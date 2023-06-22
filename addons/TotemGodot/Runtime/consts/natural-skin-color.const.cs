using System;
using System.Collections.Generic;
//using UnityEngine;
using Debug = System.Diagnostics.Debug;
//using Random = UnityEngine.Random;
using Godot;

namespace TotemConsts
{
    public static class NaturalSkinColors
    {
        private static List<string> SkinColors { get; } = new List<string>
        {
            "f9d4ab", "efd2c4", "e2c6c2", "e0d0bb", "ebb77d", "dca788", "cda093", "ccab80", "c58351", 
            "b37652", "81574b", "8a6743", "7a3e10", "5c2a19", "472422", "362714"
        };
        
        public static Color GetRandom()
        {
            Random random = new Random();
            var index = random.Next(0, SkinColors.Count);
            var c = Color.FromString($"#{SkinColors[index]}", new Color(0,0,0,1));
            return c;
        }

        public static Color GetColorByString(string colorHex)
        {
            Debug.Assert(colorHex != null, nameof(colorHex) + " != null");
            var color = SkinColors.Find(c=> c == colorHex.ToLower());
            Debug.Assert(color != null, nameof(colorHex) + " isn't a valid skin color!");
            var outColor = Color.FromString($"#{color}", new Color(0, 0, 0, 1));
            return outColor;
        }
    }
}