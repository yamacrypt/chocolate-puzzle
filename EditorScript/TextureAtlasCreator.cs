#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TextureAtlasCreator : MonoBehaviour
{
    public GameObject[] gameObjects; // アトラス化するテクスチャを持つゲームオブジェクトの配列
    public int atlasSize = 1024; // アトラスのサイズ

    public void AtlasToPng(){
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/atlas.asset");
        var bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/atlas.png", bytes);
    }
    public void CreateTextureAtlas()
    {
        Texture2D[] textures = new Texture2D[gameObjects.Length];
        Mesh[] meshs = new Mesh[gameObjects.Length];

        // ゲームオブジェクトからテクスチャを取得
        for (int i = 0; i < gameObjects.Length; i++)
        {
            textures[i] = gameObjects[i].GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
            meshs[i] = gameObjects[i].GetComponent<MeshFilter>().sharedMesh;
        }

        Texture2D atlas = new Texture2D(atlasSize, atlasSize); // アトラスを作成
        Rect[] rects = atlas.PackTextures(textures, 2); // テクスチャをアトラスに結合
        SaveObj(atlas, "atlas");
        for (int i = 0; i < gameObjects.Length; i++)
        {
            // 対応するUV座標をリマップ
            Mesh mesh = Mesh.Instantiate(meshs[i]);//CopyMesh(meshs[i]);// meshs[i];//gameObjects[i].GetComponent<MeshFilter>().mesh;
            Vector2[] originalUVs = meshs[i].uv;
            Vector2[] remappedUVs = new Vector2[originalUVs.Length];

            Rect rect = rects[i]; // 対応するテクスチャの矩形を取得

            for (int j = 0; j < originalUVs.Length; j++)
            {
                remappedUVs[j] = new Vector2(rect.x + originalUVs[j].x * rect.width, rect.y + originalUVs[j].y * rect.height);
            }

            mesh.uv = remappedUVs;
            SaveMesh(mesh, gameObjects[i].name, false); // メッシュを保存
            gameObjects[i].GetComponent<MeshFilter>().sharedMesh=mesh;
            gameObjects[i].GetComponent<Renderer>().sharedMaterial.mainTexture = atlas; // 新しいアトラスを適用
        }
    }
    Mesh CopyMesh(Mesh mesh)
    {
        Mesh newMesh = new Mesh();

        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.uv = mesh.uv;
        newMesh.uv2 = mesh.uv2;
        newMesh.tangents = mesh.tangents;
        newMesh.normals = mesh.normals;
        newMesh.colors = mesh.colors;
        newMesh.bindposes = mesh.bindposes;
        newMesh.boneWeights = mesh.boneWeights;

        newMesh.RecalculateBounds();

        return newMesh;
    }

       private Mesh CreateSimplePlane()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 1)
        };

        int[] triangles = {
            0, 1, 2,
            2, 3, 0
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void SaveMesh(Mesh mesh, string name, bool makeNewInstance)
    {
        if (makeNewInstance)
            mesh = Mesh.Instantiate(mesh) as Mesh; // メッシュの新しいインスタンスを作成する場合

        AssetDatabase.CreateAsset(mesh, "Assets/" + name + ".asset");
        AssetDatabase.SaveAssets();
    }

    private void SaveObj(Object mesh, string name)
    {
        AssetDatabase.CreateAsset(mesh, "Assets/" + name + ".asset");
        AssetDatabase.SaveAssets();
    }
}
#endif