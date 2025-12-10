using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace CyberCircuit
{
    public class CircuitBackground : VisualElement
    {
        #pragma warning disable CS0618
        public new class UxmlFactory : UxmlFactory<CircuitBackground> {}
        #pragma warning restore CS0618

        private CircuitState _state;
        private StaticLayer _staticLayer;
        private DynamicLayer _dynamicLayer;
        
        private List<Activation> _activeTraces = new List<Activation>();
        private List<Activation> _activeChips = new List<Activation>();
        private float _lastActivationTime;

        public CircuitBackground()
        {
            this.style.flexGrow = 1;
            this.style.backgroundColor = CircuitConstants.COLOR_BG;
            this.style.overflow = Overflow.Hidden;

            _staticLayer = new StaticLayer(this);
            _staticLayer.style.position = Position.Absolute;
            _staticLayer.style.left = 0; _staticLayer.style.top = 0; 
            _staticLayer.style.right = 0; _staticLayer.style.bottom = 0;
            _staticLayer.pickingMode = PickingMode.Ignore;
            Add(_staticLayer);

            _dynamicLayer = new DynamicLayer(this);
            _dynamicLayer.style.position = Position.Absolute;
            _dynamicLayer.style.left = 0; _dynamicLayer.style.top = 0;
            _dynamicLayer.style.right = 0; _dynamicLayer.style.bottom = 0;
            _dynamicLayer.pickingMode = PickingMode.Ignore;
            Add(_dynamicLayer);

            this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            this.schedule.Execute(OnUpdate).Every(16);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            float w = evt.newRect.width;
            float h = evt.newRect.height;
            if (w > 0 && h > 0 && (_state == null || Mathf.Abs(w - evt.oldRect.width) > 50 || Mathf.Abs(h - evt.oldRect.height) > 50))
            {
                _state = CircuitGenerator.Generate(w, h);
                _activeTraces.Clear();
                _activeChips.Clear();
                _staticLayer.MarkDirtyRepaint();
            }
        }

        private void OnUpdate()
        {
            if (_state == null) return;
            float time = Time.realtimeSinceStartup * 1000f;

            _activeTraces.RemoveAll(a => time - a.startTime > CircuitConstants.ACTIVATION_DURATION_MS);
            _activeChips.RemoveAll(a => time - a.startTime > CircuitConstants.ACTIVATION_DURATION_MS);

            if (time - _lastActivationTime > CircuitConstants.ACTIVATION_INTERVAL_MS)
            {
                _lastActivationTime = time;

                if (_activeTraces.Count < CircuitConstants.MAX_ACTIVE_TRACES && _state.traces.Count > 0)
                {
                    int centerIdx = UnityEngine.Random.Range(0, _state.traces.Count);
                    var centerTrace = _state.traces[centerIdx];
                    if (centerTrace.points.Count > 0)
                    {
                        var p = centerTrace.points[0];
                        float rSq = CircuitConstants.ACTIVATION_RADIUS * CircuitConstants.ACTIVATION_RADIUS;
                        for (int i = 0; i < _state.traces.Count; i++)
                        {
                            if (_activeTraces.Count >= CircuitConstants.MAX_ACTIVE_TRACES) break;
                            var t = _state.traces[i];
                            if (t.points.Count < 1) continue;
                            var tp = t.points[0];
                            if ((p.x - tp.x)*(p.x - tp.x) + (p.y - tp.y)*(p.y - tp.y) < rSq && !_activeTraces.Any(at => at.index == i))
                                _activeTraces.Add(new Activation { index = i, startTime = time });
                        }
                    }
                }

                 if (_activeChips.Count < CircuitConstants.MAX_ACTIVE_CHIPS && _state.chips.Count > 0)
                {
                    int centerIdx = UnityEngine.Random.Range(0, _state.chips.Count);
                    var centerChip = _state.chips[centerIdx];
                    float cx = centerChip.x + centerChip.width/2f;
                    float cy = centerChip.y + centerChip.height/2f;
                    float rSq = CircuitConstants.ACTIVATION_RADIUS * CircuitConstants.ACTIVATION_RADIUS;
                    for (int i = 0; i < _state.chips.Count; i++)
                    {
                        if (_activeChips.Count >= CircuitConstants.MAX_ACTIVE_CHIPS) break;
                        var c = _state.chips[i];
                        float ccx = c.x + c.width/2f;
                        float ccy = c.y + c.height/2f;
                        if ((cx - ccx)*(cx - ccx) + (cy - ccy)*(cy - ccy) < rSq && !_activeChips.Any(ac => ac.index == i))
                            _activeChips.Add(new Activation { index = i, startTime = time });
                    }
                }
            }
            _dynamicLayer.MarkDirtyRepaint();
        }

        public CircuitState State => _state;
        public List<Activation> ActiveTraces => _activeTraces;
        public List<Activation> ActiveChips => _activeChips;

        private static void DrawRect(Painter2D painter, float x, float y, float w, float h, bool fill)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, y));
            painter.LineTo(new Vector2(x + w, y));
            painter.LineTo(new Vector2(x + w, y + h));
            painter.LineTo(new Vector2(x, y + h));
            painter.ClosePath();
            if (fill) painter.Fill(); else painter.Stroke();
        }

        private class StaticLayer : VisualElement
        {
            private CircuitBackground _owner;
            public StaticLayer(CircuitBackground owner) { _owner = owner; generateVisualContent += Generate; }

            void Generate(MeshGenerationContext mgc)
            {
                if (_owner.State == null) return;
                var state = _owner.State;
                var painter = mgc.painter2D;

                painter.strokeColor = CircuitConstants.COLOR_GRID;
                painter.lineWidth = 1;
                painter.BeginPath();
                float w = _owner.layout.width;
                float h = _owner.layout.height;
                for (float x = 0; x <= w; x += CircuitConstants.GRID_SIZE * 4) { painter.MoveTo(new Vector2(x, 0)); painter.LineTo(new Vector2(x, h)); }
                for (float y = 0; y <= h; y += CircuitConstants.GRID_SIZE * 4) { painter.MoveTo(new Vector2(0, y)); painter.LineTo(new Vector2(w, y)); }
                painter.Stroke();

                painter.strokeColor = CircuitConstants.COLOR_TRACE;
                painter.lineWidth = 1.5f;
                painter.lineCap = LineCap.Round;
                painter.lineJoin = LineJoin.Round;
                
                foreach (var trace in state.traces)
                {
                    if (trace.points.Count < 2) continue;
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(trace.points[0].x, trace.points[0].y));
                    for (int i = 1; i < trace.points.Count; i++) painter.LineTo(new Vector2(trace.points[i].x, trace.points[i].y));
                    painter.Stroke();
                    painter.fillColor = CircuitConstants.COLOR_NODE;
                    painter.BeginPath(); painter.Arc(new Vector2(trace.points[0].x, trace.points[0].y), 2, 0, 360); painter.Fill();
                    var last = trace.points[trace.points.Count - 1];
                    painter.BeginPath(); painter.Arc(new Vector2(last.x, last.y), 2, 0, 360); painter.Fill();
                }

                foreach (var chip in state.chips)
                {
                    painter.fillColor = CircuitConstants.COLOR_CHIP_BODY;
                    DrawRect(painter, chip.x, chip.y, chip.width, chip.height, true);
                    painter.strokeColor = CircuitConstants.COLOR_CHIP_BORDER;
                    painter.lineWidth = 1;
                    DrawRect(painter, chip.x, chip.y, chip.width, chip.height, false);
                    painter.fillColor = CircuitConstants.COLOR_TRACE_HIGHLIGHT;
                    if (chip.type == "cpu") { float p = 10; DrawRect(painter, chip.x + p, chip.y + p, chip.width - p*2, chip.height - p*2, true); }
                    else { painter.BeginPath(); for (float i = 10; i < chip.width; i+=10) DrawRect(painter, chip.x + i, chip.y + 5, 2, chip.height - 10, true); }
                }
            }
        }

        private class DynamicLayer : VisualElement
        {
            private CircuitBackground _owner;
            public DynamicLayer(CircuitBackground owner) { _owner = owner; generateVisualContent += Generate; }

            void Generate(MeshGenerationContext mgc)
            {
                if (_owner.State == null) return;
                var state = _owner.State;
                var painter = mgc.painter2D;
                float time = Time.realtimeSinceStartup * 1000f;

                painter.lineWidth = 2f;
                painter.lineCap = LineCap.Round;
                painter.lineJoin = LineJoin.Round;

                foreach (var active in _owner.ActiveTraces)
                {
                    if (active.index >= state.traces.Count) continue;
                    var trace = state.traces[active.index];
                    if (trace.points.Count < 2) continue;
                    float opacity = 1f - (time - active.startTime) / CircuitConstants.ACTIVATION_DURATION_MS;
                    if (opacity <= 0) continue;
                    Color col = CircuitConstants.COLOR_TRACE_HIGHLIGHT; col.a = opacity;
                    painter.strokeColor = col;
                    painter.BeginPath();
                    painter.MoveTo(new Vector2(trace.points[0].x, trace.points[0].y));
                    for (int i = 1; i < trace.points.Count; i++) painter.LineTo(new Vector2(trace.points[i].x, trace.points[i].y));
                    painter.Stroke();
                    Color sigCol = CircuitConstants.COLOR_SIGNAL; sigCol.a = opacity; painter.fillColor = sigCol;
                    painter.BeginPath(); painter.Arc(new Vector2(trace.points[0].x, trace.points[0].y), 3, 0, 360); painter.Fill();
                    var last = trace.points[trace.points.Count - 1];
                    painter.BeginPath(); painter.Arc(new Vector2(last.x, last.y), 3, 0, 360); painter.Fill();
                }

                foreach (var active in _owner.ActiveChips)
                {
                    if (active.index >= state.chips.Count) continue;
                    var chip = state.chips[active.index];
                    float opacity = 1f - (time - active.startTime) / CircuitConstants.ACTIVATION_DURATION_MS;
                    if (opacity <= 0) continue;
                    Color col = CircuitConstants.COLOR_SIGNAL; col.a = opacity * 0.3f; painter.fillColor = col;
                    DrawRect(painter, chip.x - 5, chip.y - 5, chip.width + 10, chip.height + 10, true);
                }

                foreach (var signal in state.signals)
                {
                    if (signal.pathIndex >= state.traces.Count) continue;
                    var trace = state.traces[signal.pathIndex];
                    if (trace.points.Count < 2) continue;
                    signal.progress += signal.speed;
                    if (signal.progress >= 1f) signal.progress = 0f;
                    float totalSegments = trace.points.Count - 1;
                    float segmentProgress = signal.progress * totalSegments;
                    int cur = Mathf.FloorToInt(segmentProgress);
                    float segP = segmentProgress - cur;
                    if (cur >= trace.points.Count - 1) cur = trace.points.Count - 2;
                    var p1 = trace.points[cur]; var p2 = trace.points[cur + 1];
                    float x = p1.x + (p2.x - p1.x) * segP; float y = p1.y + (p2.y - p1.y) * segP;
                    Color glow = signal.color; glow.a = 0.4f; painter.fillColor = glow;
                    painter.BeginPath(); painter.Arc(new Vector2(x, y), 6, 0, 360); painter.Fill();
                    painter.fillColor = Color.white;
                    painter.BeginPath(); painter.Arc(new Vector2(x, y), 2f, 0, 360); painter.Fill();
                }
            }
        }
    }
}