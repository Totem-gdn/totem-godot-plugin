using System;
using System.Collections.Generic;
using System.Diagnostics;
//using UnityEngine;
using Godot;
using GodotUtils;

namespace TotemConsts
{
    public static class NaturalHairColors
    {
        private static List<string> HColors { get; } = new List<string>
        {
            "b1b1b1", "070504", "341c0d", "62422e", "914329", "cd622b", "ad7b41", "e4b877"
        };
        
        public static Color GetRandom()
        {
            Random random = new Random();
            var index = random.Next(0, HColors.Count);
            var c = Color.FromString($"#{HColors[index]}", new Color(0, 0, 0, 1));
            return c;
        }
        
        public static Color GetColorByString(string colorHex)
        {
            Debug.Assert(colorHex != null, nameof(colorHex) + " != null");
            var color = HColors.Find(c=> c == colorHex.ToLower());
            Debug.Assert(color != null, nameof(colorHex) + " isn't a valid hair color!");
            var outColor = Color.FromString($"#{color}", new Color(0, 0, 0, 1));
            return outColor;
        }
    }
}