using System;
using System.Collections.Generic;
using System.Diagnostics;
using TotemConsts;
using DefaultNamespace;
//using UnityEngine;
using Godot;
using GodotUtils;

namespace DefaultNamespace
{
    public static class NaturalEyeColors
    {
        private static List<string> EyeColors { get; } = new List<string>
        {
            "b5d6e0", "90b4ca", "a7ad7f", "7c8b4f", "c4a05f", "a97e33", "7a3411", "3d0d04"
        };
        
        public static Color GetRandom()
        {
            Random random = new Random();
            var index = random.Next(0, EyeColors.Count);
            var c = Color.FromString($"#{EyeColors[index]}", new Color(0,0,0,1));
            return c;
        }
        
        public static Color GetColorByString(string colorHex)
        {
            Debug.Assert(colorHex != null, nameof(colorHex) + " != null");
            var color = EyeColors.Find(c=> c == colorHex.ToLower());
            Debug.Assert(color != null, nameof(colorHex) + " isn't a valid eye color!");
            var outColor = Color.FromString($"#{color}", new Color(0, 0, 0, 1));
            return outColor;
        }
    }
}