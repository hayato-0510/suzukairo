using UnityEngine;
using Unity.VectorGraphics;
using System.Linq;
using System.Collections.Generic;
using static Unity.VectorGraphics.VectorUtils;
using System.IO;

[RequireComponent(typeof(SpriteRenderer))]
public class SvgMeshGenerator : MonoBehaviour
{
    public TextAsset svgSource;

    void Start()
    {
        if (svgSource == null) return;

        // SVG を読み込み
        var sceneInfo = SVGParser.ImportSVG(new StringReader(svgSource.text));
        var tessOptions = new TessellationOptions()
        {
            StepDistance = 1f,
            MaxCordDeviation = 0.5f,
            MaxTanAngleDeviation = 0.1f,
            SamplingStepSize = 0.01f
        };

        // シーン全体をメッシュ化（パスを三角形化）
        var geoms = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);

        // 全パスを結合するメッシュを作成
        Mesh mesh = new Mesh();
        VectorUtils.FillMesh(mesh, geoms, 1f);

        // Sprite 用の頂点とインデックスに変換
        var vertices2D = mesh.vertices.Select(v => new Vector2(v.x, v.y)).ToArray();
        ushort[] indices = mesh.triangles.Select(i => (ushort)i).ToArray();

        // 基本スプライト作成（透明 1x1）
        var tex = new Texture2D(2, 2);
        tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        // メッシュ反映
        sprite.OverrideGeometry(vertices2D, indices);

        // SpriteRenderer に登録
        var renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        // Collider を設定したければここで作る
        GenerateCollider(vertices2D);
    }

    void GenerateCollider(Vector2[] vertices)
    {
        // 多角形コライダー
        var poly = gameObject.AddComponent<PolygonCollider2D>();

        // SVG は複数パスの可能性があるが単純版として
        poly.pathCount = 1;
        poly.SetPath(0, vertices);
    }
}
