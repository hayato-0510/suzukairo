using System.Collections.Generic;
using UnityEngine;

namespace CyberCircuit
{
    public static class CircuitGenerator
    {
        private static int Snap(float val)
        {
            return (int)(Mathf.Round(val / CircuitConstants.GRID_SIZE) * CircuitConstants.GRID_SIZE);
        }

        public static CircuitState Generate(float width, float height)
        {
            if (width <= 0 || height <= 0) return new CircuitState();

            int cols = Mathf.CeilToInt(width / CircuitConstants.GRID_SIZE);
            int rows = Mathf.CeilToInt(height / CircuitConstants.GRID_SIZE);

            var state = new CircuitState();
            state.gridOccupied = new bool[cols, rows]; 

            // 1. Generate Chips
            float totalPixels = width * height;
            int numChips = Mathf.Max(3, Mathf.FloorToInt(totalPixels * CircuitConstants.CHIP_COUNT_FACTOR));

            for (int i = 0; i < numChips; i++)
            {
                int chipW = UnityEngine.Random.Range(2, 7) * CircuitConstants.GRID_SIZE;
                int chipH = UnityEngine.Random.Range(2, 7) * CircuitConstants.GRID_SIZE;
                
                if (width - chipW - CircuitConstants.GRID_SIZE <= CircuitConstants.GRID_SIZE) continue;
                if (height - chipH - CircuitConstants.GRID_SIZE <= CircuitConstants.GRID_SIZE) continue;

                int chipX = Snap(UnityEngine.Random.Range(CircuitConstants.GRID_SIZE, width - chipW - CircuitConstants.GRID_SIZE));
                int chipY = Snap(UnityEngine.Random.Range(CircuitConstants.GRID_SIZE, height - chipH - CircuitConstants.GRID_SIZE));

                int startCol = chipX / CircuitConstants.GRID_SIZE;
                int startRow = chipY / CircuitConstants.GRID_SIZE;
                int endCol = startCol + chipW / CircuitConstants.GRID_SIZE;
                int endRow = startRow + chipH / CircuitConstants.GRID_SIZE;

                bool overlap = false;
                for (int c = startCol - 1; c <= endCol + 1; c++)
                {
                    for (int r = startRow - 1; r <= endRow + 1; r++)
                    {
                        if (c >= 0 && c < cols && r >= 0 && r < rows && state.gridOccupied[c, r])
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (overlap) break;
                }

                if (!overlap)
                {
                    state.chips.Add(new Chip
                    {
                        x = chipX,
                        y = chipY,
                        width = chipW,
                        height = chipH,
                        type = UnityEngine.Random.value > 0.7f ? "cpu" : "memory"
                    });

                    for (int c = startCol; c < endCol; c++)
                    {
                        for (int r = startRow; r < endRow; r++)
                        {
                            if (c >= 0 && c < cols && r >= 0 && r < rows)
                                state.gridOccupied[c, r] = true;
                        }
                    }
                }
            }

            // 2. Generate Traces
            int numTraces = Mathf.FloorToInt(totalPixels * CircuitConstants.TRACE_COUNT_FACTOR);

            for (int i = 0; i < numTraces; i++)
            {
                int currX = Snap(UnityEngine.Random.Range(CircuitConstants.GRID_SIZE, width - CircuitConstants.GRID_SIZE));
                int currY = Snap(UnityEngine.Random.Range(CircuitConstants.GRID_SIZE, height - CircuitConstants.GRID_SIZE));
                int col = currX / CircuitConstants.GRID_SIZE;
                int row = currY / CircuitConstants.GRID_SIZE;

                if (col >= 0 && col < cols && row >= 0 && row < rows && state.gridOccupied[col, row]) continue;

                var points = new List<Point> { new Point(currX, currY) };
                int pathLength = UnityEngine.Random.Range(5, 21);
                int direction = UnityEngine.Random.Range(0, 4); 

                for (int s = 0; s < pathLength; s++)
                {
                    if (UnityEngine.Random.value < 0.3f)
                        direction = (direction + (UnityEngine.Random.value > 0.5f ? 1 : 3)) % 4;

                    int len = UnityEngine.Random.Range(2, 7) * CircuitConstants.GRID_SIZE;
                    int dx = 0, dy = 0;

                    if (direction == 0) dy = -len;
                    else if (direction == 1) dx = len;
                    else if (direction == 2) dy = len;
                    else if (direction == 3) dx = -len;

                    int nextX = currX + dx;
                    int nextY = currY + dy;

                    if (nextX < CircuitConstants.GRID_SIZE || nextX > width - CircuitConstants.GRID_SIZE ||
                        nextY < CircuitConstants.GRID_SIZE || nextY > height - CircuitConstants.GRID_SIZE)
                        break;

                    int nextCol = Mathf.RoundToInt(nextX / (float)CircuitConstants.GRID_SIZE);
                    int nextRow = Mathf.RoundToInt(nextY / (float)CircuitConstants.GRID_SIZE);

                    if (nextCol >= 0 && nextCol < cols && nextRow >= 0 && nextRow < rows && state.gridOccupied[nextCol, nextRow])
                    {
                        points.Add(new Point(nextX, nextY));
                        break;
                    }

                    currX = nextX;
                    currY = nextY;
                    points.Add(new Point(nextX, nextY));
                }

                if (points.Count > 1)
                {
                    state.traces.Add(new TracePath { points = points, length = points.Count });

                    if (UnityEngine.Random.value < CircuitConstants.SIGNAL_DENSITY)
                    {
                        state.signals.Add(new Signal
                        {
                            pathIndex = state.traces.Count - 1,
                            progress = UnityEngine.Random.value,
                            speed = 0.005f + UnityEngine.Random.value * 0.01f,
                            color = UnityEngine.Random.value > 0.8f ? CircuitConstants.COLOR_SIGNAL_SECONDARY : CircuitConstants.COLOR_SIGNAL
                        });
                    }
                }
            }

            return state;
        }
    }
}