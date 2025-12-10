using System.Collections.Generic;
using UnityEngine;

namespace CyberCircuit
{
    // --- 定数・設定 (Config) ---
    public static class CircuitConstants
    {
        public const int GRID_SIZE = 10;
        public const float CHIP_COUNT_FACTOR = 0.0001f;
        public const float TRACE_COUNT_FACTOR = 0.001f;
        public const float SIGNAL_DENSITY = 0.03f;
        
        public const float ACTIVATION_INTERVAL_MS = 1000f;
        public const float ACTIVATION_DURATION_MS = 2000f;
        public const int MAX_ACTIVE_TRACES = 100;
        public const int MAX_ACTIVE_CHIPS = 1;
        public const float ACTIVATION_RADIUS = 200f;
        
        public static readonly Color COLOR_BG = new Color32(5, 5, 5, 255);
        public static readonly Color COLOR_GRID = new Color32(10, 10, 10, 255);
        public static readonly Color COLOR_TRACE = new Color32(26, 35, 50, 255);
        public static readonly Color COLOR_TRACE_HIGHLIGHT = new Color32(42, 59, 85, 255);
        public static readonly Color COLOR_CHIP_BODY = new Color32(15, 19, 26, 255);
        public static readonly Color COLOR_CHIP_BORDER = new Color32(35, 52, 70, 255);
        public static readonly Color COLOR_NODE = new Color32(31, 46, 64, 255);
        public static readonly Color COLOR_SIGNAL = new Color32(0, 242, 255, 255);
        public static readonly Color COLOR_SIGNAL_SECONDARY = new Color32(0, 136, 255, 255);
    }

    // --- 型定義 (Types) ---
    public struct Point
    {
        public float x;
        public float y;
        public Point(float x, float y) { this.x = x; this.y = y; }
    }

    public class TracePath
    {
        public List<Point> points = new List<Point>();
        public int length;
    }

    public class Chip
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public string type;
    }

    public class Signal
    {
        public int pathIndex;
        public float progress;
        public float speed;
        public Color color;
    }

    public class Activation
    {
        public int index;
        public float startTime;
    }

    public class CircuitState
    {
        public List<TracePath> traces = new List<TracePath>();
        public List<Chip> chips = new List<Chip>();
        public List<Signal> signals = new List<Signal>();
        public bool[,] gridOccupied;
    }
}