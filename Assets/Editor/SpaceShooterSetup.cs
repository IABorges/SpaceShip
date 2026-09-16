using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SpaceShooterSetup
{
    const string PrefabFolder = "Assets/Prefabs";
    const string RootName = "SpaceShooter";

    [MenuItem("Tools/Space Shooter/Montar Cena")]
    public static void Setup()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Nenhuma Main Camera encontrada na cena.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);

        var root = new GameObject(RootName);
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * (4f / 3f);

        GameObject bulletPrefab = BuildBulletPrefab();
        GameObject enemyPrefab = BuildEnemyPrefab();

        BuildPlayer(root, halfWidth, bulletPrefab);

        var spawnerGo = new GameObject("EnemySpawner");
        spawnerGo.transform.SetParent(root.transform);
        spawnerGo.AddComponent<EnemySpawner>().enemyPrefab = enemyPrefab;

        BuildHudAndGameManager(root);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Space Shooter montado! WASD/Setas move, Espaco atira. Pressione Play.");
    }

    static void BuildPlayer(GameObject root, float halfWidth, GameObject bulletPrefab)
    {
        Sprite shipSprite = LoadSprite("Assets/Ship01.png");

        var player = new GameObject("Player");
        player.transform.SetParent(root.transform);
        player.transform.position = new Vector3(-halfWidth + 1.5f, 0f, 0f);
        player.transform.localScale = Vector3.one * 2f;

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = shipSprite;
        sr.sortingOrder = 10;

        var rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        if (shipSprite != null) col.size = shipSprite.bounds.size * 0.8f;

        var fp = new GameObject("FirePoint");
        fp.transform.SetParent(player.transform, false);
        float noseX = shipSprite != null ? shipSprite.bounds.extents.x : 0.3f;
        fp.transform.localPosition = new Vector3(noseX, 0f, 0f);

        var ctrl = player.AddComponent<PlayerController>();
        ctrl.bulletPrefab = bulletPrefab;
        ctrl.firePoint = fp.transform;
    }

    static GameObject BuildBulletPrefab()
    {
        Sprite sprite = LoadSprite("Assets/Bullet.png");

        var go = new GameObject("Bullet");
        go.transform.localScale = Vector3.one * 2f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        AddTriggerBody(go, sprite, 1f);
        go.AddComponent<Bullet>();

        return SaveAsPrefab(go, "Bullet");
    }

    static GameObject BuildEnemyPrefab()
    {
        Sprite sprite = LoadSprite("Assets/EnemyAlien.png");

        var go = new GameObject("EnemyAlien");
        go.transform.localScale = Vector3.one * 1.8f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 8;

        AddTriggerBody(go, sprite, 0.8f);
        go.AddComponent<EnemyController>();

        return SaveAsPrefab(go, "EnemyAlien");
    }

    // Trigger 2D entre dois corpos kinematicos nao gera evento; por isso tiros e inimigos
    // sao dinamicos (sem gravidade) e so a nave e kinematica.
    static void AddTriggerBody(GameObject go, Sprite sprite, float colliderScale)
    {
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        if (sprite != null) col.size = sprite.bounds.size * colliderScale;
    }

    static GameObject SaveAsPrefab(GameObject go, string name)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void BuildHudAndGameManager(GameObject root)
    {
        var canvasGo = new GameObject("HUD");
        canvasGo.transform.SetParent(root.transform);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1024, 768);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        var canvasTr = canvasGo.transform;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var scoreText = MakeText(canvasTr, "ScoreText", "Pontos: 0", font, 28, TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -16), new Vector2(360, 40), Color.white);
        var livesText = MakeText(canvasTr, "LivesText", "Vidas: 3", font, 28, TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -16), new Vector2(240, 40), Color.white);
        var slowText = MakeText(canvasTr, "SlowMoText", "Tempo lento [E]: 1", font, 30, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(420, 40), new Color(0.5f, 0.85f, 1f));
        MakeText(canvasTr, "HelpText", "WASD / Setas: mover    Espaco: atirar    E: tempo lento", font, 20, TextAnchor.LowerLeft,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, 14), new Vector2(520, 30), new Color(1, 1, 1, 0.7f));

        var overlay = MakeFullscreenImage(canvasTr, "SlowMoOverlay", new Color(0.3f, 0.6f, 1f, 0.04f));
        overlay.transform.SetAsFirstSibling();

        var endPanel = MakeFullscreenImage(canvasTr, "EndPanel", new Color(0f, 0f, 0f, 0.75f));
        var endTitle = MakeText(endPanel.transform, "EndTitle", "DERROTA", font, 84, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(900, 120), Color.white);
        var endScore = MakeText(endPanel.transform, "EndScore", "", font, 32, TextAnchor.UpperCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(900, 200), Color.white);

        var gmGo = new GameObject("GameManager");
        gmGo.transform.SetParent(root.transform);
        var gm = gmGo.AddComponent<GameManager>();
        gm.scoreText = scoreText;
        gm.livesText = livesText;
        gm.slowMoText = slowText;
        gm.slowMoOverlay = overlay;
        gm.endPanel = endPanel;
        gm.endTitleText = endTitle;
        gm.endScoreText = endScore;

        overlay.SetActive(false);
        endPanel.SetActive(false);
    }

    static Text MakeText(Transform parent, string name, string content, Font font, int fontSize, TextAnchor align,
        Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = align;
        text.color = color;
        text.text = content;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        var rt = text.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return text;
    }

    static GameObject MakeFullscreenImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    static Sprite LoadSprite(string path)
    {
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
            if (a is Sprite s) return s;
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
