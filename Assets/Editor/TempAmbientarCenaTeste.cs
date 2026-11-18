using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TempAmbientarCenaTeste
{
    static Sprite Carregar(string caminho)
    {
        var todos = AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (todos.Length == 0)
        {
            Debug.LogWarning("[Ambientar] sem sprite em: " + caminho);
            return null;
        }
        Debug.Log("[Ambientar] " + caminho + " -> " + todos.Length + " sprite(s); primeiro '" + todos[0].name + "' tamanho " + todos[0].bounds.size);
        return todos[0];
    }

    static void Colocar(Transform pai, string nome, string caminho, Vector2 pos, float alturaAlvo, int ordem, bool flipX)
    {
        Sprite sp = Carregar(caminho);
        if (sp == null) return;

        Transform existente = pai.Find(nome);
        GameObject go;
        if (existente != null)
        {
            go = existente.gameObject;
            Undo.RecordObject(go.transform, "Ambientar");
        }
        else
        {
            go = new GameObject(nome);
            Undo.RegisterCreatedObjectUndo(go, "Ambientar");
            go.transform.SetParent(pai, false);
        }

        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        float h = sp.bounds.size.y;
        float esc = h > 0f ? alturaAlvo / h : 1f;
        go.transform.localScale = new Vector3(esc, esc, 1f);

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = Undo.AddComponent<SpriteRenderer>(go);
        Undo.RecordObject(sr, "Ambientar");
        sr.sprite = sp;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = ordem;
        sr.flipX = flipX;
        EditorSceneManager.MarkSceneDirty(go.scene);
    }

    [MenuItem("Tools/Temp/Ambientar Cena Teste")]
    public static void Executar()
    {
        GameObject raiz = GameObject.Find("Ambientacao");
        if (raiz == null)
        {
            Debug.LogError("[Ambientar] objeto 'Ambientacao' nao encontrado na cena ativa");
            return;
        }
        Transform p = raiz.transform;

        string poste = "Assets/Sprites/cenarios/Elementos/Poste de luz.png";
        Colocar(p, "Poste 1 (inicio)", poste, new Vector2(-9.5f, -32f), 7f, 2, false);
        Colocar(p, "Poste 2 (buraco)", poste, new Vector2(9.5f, -22f), 7f, 2, true);
        Colocar(p, "Poste 3 (falhando)", poste, new Vector2(-9.5f, -8f), 7f, 2, false);
        Colocar(p, "Poste 4 (Tulipa)", poste, new Vector2(9.5f, 12f), 7f, 2, true);

        Colocar(p, "Gato Azul", "Assets/Sprites/Personagens/Gatos/Azul/Idle gatinho azul.png", new Vector2(-8.5f, -20f), 1.1f, 2, false);
        Colocar(p, "Gato Rosa", "Assets/Sprites/Personagens/Gatos/Rosa/Idle gatinho rosa.png", new Vector2(8.6f, -30f), 1.1f, 2, true);
        Colocar(p, "Gato Roxo", "Assets/Sprites/Personagens/Gatos/Roxo/Idle gatinho roxo.png", new Vector2(-8.8f, 4f), 1.1f, 2, false);
        Colocar(p, "Gato Verde", "Assets/Sprites/Personagens/Gatos/Verde/Idle gatinho verde.png", new Vector2(8.8f, 22f), 1.1f, 2, true);

        Colocar(p, "Cachorro Vermelho e Preto", "Assets/Sprites/Personagens/Cachorros/CACHORRO VERMELHO E PRETO.png", new Vector2(8.8f, -8f), 1.6f, 2, true);
        Colocar(p, "Cachorrinho Azul", "Assets/Sprites/Personagens/Cachorros/Cachorrinho azul.png", new Vector2(-8.7f, 14f), 1.4f, 2, false);

        Colocar(p, "Rato Vermelho", "Assets/Sprites/Personagens/Ratos/Rato vermelho.png", new Vector2(8.5f, -17f), 0.6f, 2, true);

        Debug.Log("[Ambientar] concluido");
    }
}
