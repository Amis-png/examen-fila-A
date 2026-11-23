using UnityEngine;

// Juego 2D Grupo A.
// El nivel (suelo, plataformas y jugador) se guarda dentro de la escena "Juego",
// asi se ve tambien sin darle Play. Si la escena no tiene nivel, se crea al darle Play.
public class Juego : MonoBehaviour
{
    public static Juego instancia;

    Vector3 inicio;
    Transform jugador;
    Rigidbody2D rbJugador;
    Camera cam;

    AudioSource audioSource;
    AudioClip sonidoSalto;
    AudioClip sonidoCaida;

    string mensaje = "";
    float tiempoMensaje = 0f;

    // Recursos que usa el constructor del nivel
    static Sprite cuadrado;
    static Material materialSprite;
    static PhysicsMaterial2D sinFriccion;

    // Se ejecuta solo al darle Play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Arrancar()
    {
        if (FindAnyObjectByType<Juego>() != null) return;
        new GameObject("Juego").AddComponent<Juego>();
    }

    void Awake()
    {
        instancia = this;

        cam = Camera.main;
        if (cam == null)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
        }
        ConfigurarCamara(cam);
        if (FindAnyObjectByType<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
        audioSource = cam.gameObject.AddComponent<AudioSource>();

        sonidoSalto = CrearTono("salto", 300f, 700f, 0.12f);
        sonidoCaida = CrearTono("caida", 500f, 120f, 0.4f);

        // Si la escena ya tiene el nivel guardado, se usa ese
        GameObject nivel = GameObject.Find("Nivel");
        if (nivel == null)
        {
            CrearRecursosEnMemoria();
            nivel = ConstruirNivel(cuadrado, sinFriccion, materialSprite);
        }

        jugador = nivel.transform.Find("Jugador");
        rbJugador = jugador.GetComponent<Rigidbody2D>();
        inicio = jugador.position;
    }

    // ---------- Construccion del nivel (se usa en Play y en el editor) ----------

    public static void ConfigurarCamara(Camera c)
    {
        c.orthographic = true;
        c.orthographicSize = 5f;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = new Color(0.45f, 0.75f, 1f); // cielo
        c.transform.position = new Vector3(-2f, 0f, -10f);
        c.transform.rotation = Quaternion.identity;
    }

    static void CrearRecursosEnMemoria()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] blanco = new Color[16];
        for (int i = 0; i < 16; i++) blanco[i] = Color.white;
        tex.SetPixels(blanco);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        cuadrado = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f); // 1x1 unidad

        sinFriccion = new PhysicsMaterial2D("SinFriccion");
        sinFriccion.friction = 0f;

        // En URP usamos un material sin luces para que se vea con colores planos
        Shader s = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (s != null) materialSprite = new Material(s);
    }

    public static GameObject ConstruirNivel(Sprite sprite, PhysicsMaterial2D materialFisico, Material material)
    {
        cuadrado = sprite;
        sinFriccion = materialFisico;
        materialSprite = material;

        GameObject nivel = new GameObject("Nivel");
        Transform n = nivel.transform;

        Color verde = new Color(0.25f, 0.7f, 0.25f);
        Color tierra = new Color(0.55f, 0.35f, 0.2f);

        // Suelo: solo 2 bloques, con un hueco en medio para poder caerse
        CrearBloque("Suelo 1", new Vector2(-1f, -3.5f), new Vector2(18f, 1f), verde, true, n);
        CrearBloque("Suelo 2", new Vector2(17.5f, -3.5f), new Vector2(13f, 1f), verde, true, n);

        // Tierra debajo del pasto (solo decoracion)
        CrearBloque("Tierra 1", new Vector2(-1f, -4.5f), new Vector2(18f, 1f), tierra, false, n);
        CrearBloque("Tierra 2", new Vector2(17.5f, -4.5f), new Vector2(13f, 1f), tierra, false, n);

        // Pared invisible a la izquierda
        CrearBloque("Pared", new Vector2(-10.5f, 0f), new Vector2(1f, 20f), Color.clear, true, n);

        // Plataformas flotantes
        CrearBloque("Plataforma 1", new Vector2(4f, -1.2f), new Vector2(3f, 0.4f), tierra, true, n);
        CrearBloque("Plataforma 2", new Vector2(20f, -1.2f), new Vector2(3f, 0.4f), tierra, true, n);
        CrearBloque("Plataforma 3", new Vector2(24f, 0.8f), new Vector2(2f, 0.4f), tierra, true, n);

        CrearJugador(new Vector3(-7f, -2f, 0f), n);
        CrearNombre();
        return nivel;
    }

    // Texto "Amis-png" pegado a la camara: se ve arriba al centro, con o sin Play
    static void CrearNombre()
    {
        Camera c = Camera.main;
        if (c == null || c.transform.Find("Nombre") != null) return;

        GameObject go = new GameObject("Nombre");
        go.transform.SetParent(c.transform, false);
        go.transform.localPosition = new Vector3(0f, 4.7f, 10f);

        TextMesh tm = go.AddComponent<TextMesh>();
        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = fuente;
        tm.text = "Amis-png";
        tm.fontSize = 60;
        tm.characterSize = 0.08f;
        tm.anchor = TextAnchor.UpperCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = Color.white;

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = fuente.material;
        mr.sortingOrder = 10;
    }

    public static GameObject CrearBloque(string nombre, Vector2 posicion, Vector2 tamano, Color color, bool conColision, Transform padre)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = posicion;
        go.transform.localScale = new Vector3(tamano.x, tamano.y, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = cuadrado;
        sr.color = color;
        if (materialSprite != null) sr.sharedMaterial = materialSprite;
        if (conColision) go.AddComponent<BoxCollider2D>();
        return go;
    }

    static void CrearJugador(Vector3 posicion, Transform padre)
    {
        GameObject go = new GameObject("Jugador");
        go.transform.SetParent(padre, false);
        go.transform.position = posicion;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

        // Si pones tu propia imagen en Assets/Resources/jugador.png se usa esa
        Sprite propio = Resources.Load<Sprite>("jugador");
        if (propio != null)
        {
            sr.sprite = propio;
            float k = 1f / propio.bounds.size.y; // que mida 1 unidad de alto
            go.transform.localScale = new Vector3(k, k, 1f);
        }
        else
        {
            // Personaje simple: cuadrado rojo con ojo (como en la pizarra)
            sr.sprite = cuadrado;
            sr.color = new Color(0.9f, 0.15f, 0.15f);
            go.transform.localScale = new Vector3(0.8f, 1f, 1f);
            GameObject ojo = CrearBloque("Ojo", Vector2.zero, new Vector2(0.15f, 0.25f), Color.black, false, go.transform);
            ojo.transform.localPosition = new Vector3(0.2f, 0.2f, 0f);
            ojo.transform.localScale = new Vector3(0.18f, 0.25f, 1f);
            ojo.GetComponent<SpriteRenderer>().sortingOrder = 2;
        }
        if (materialSprite != null) sr.sharedMaterial = materialSprite;
        sr.sortingOrder = 1;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.sharedMaterial = sinFriccion;

        go.AddComponent<Jugador>();
    }

    // ---------- Sonido ----------

    // Genera un pitido sencillo sin necesitar archivos de audio
    AudioClip CrearTono(string nombre, float frecInicio, float frecFin, float duracion)
    {
        int frecuenciaMuestreo = 44100;
        int n = (int)(frecuenciaMuestreo * duracion);
        float[] datos = new float[n];
        float fase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float frec = Mathf.Lerp(frecInicio, frecFin, t);
            fase += 2f * Mathf.PI * frec / frecuenciaMuestreo;
            float onda = Mathf.Sin(fase) > 0 ? 1f : -1f; // sonido tipo retro
            datos[i] = onda * 0.25f * (1f - t);
        }
        AudioClip clip = AudioClip.Create(nombre, n, 1, frecuenciaMuestreo, false);
        clip.SetData(datos, 0);
        return clip;
    }

    // ---------- Juego ----------

    void Update()
    {
        if (jugador == null) return;

        // Mensaje de caida
        if (jugador.position.y < -8f)
        {
            audioSource.PlayOneShot(sonidoCaida);
            MostrarMensaje("¡Te caíste! Vuelve a intentarlo", 2f);
            jugador.position = inicio;
            rbJugador.linearVelocity = Vector2.zero;
        }

        if (tiempoMensaje > 0f) tiempoMensaje -= Time.deltaTime;
    }

    void LateUpdate()
    {
        if (jugador == null) return;
        // La camara sigue al jugador de lado
        float x = Mathf.Max(jugador.position.x, -2f);
        Vector3 p = cam.transform.position;
        p.x = Mathf.Lerp(p.x, x, 5f * Time.deltaTime);
        cam.transform.position = p;
    }

    public void SonidoSalto()
    {
        audioSource.PlayOneShot(sonidoSalto, 0.6f);
    }

    void MostrarMensaje(string texto, float segundos)
    {
        mensaje = texto;
        tiempoMensaje = segundos;
    }

    void OnGUI()
    {
        GUIStyle estilo = new GUIStyle(GUI.skin.label);
        estilo.fontSize = 28;
        estilo.fontStyle = FontStyle.Bold;
        estilo.normal.textColor = Color.white;

        if (tiempoMensaje > 0f)
        {
            GUIStyle grande = new GUIStyle(estilo);
            grande.fontSize = 40;
            grande.alignment = TextAnchor.MiddleCenter;
            Rect r = new Rect(0, Screen.height / 2f - 60, Screen.width, 120);
            GUI.Box(r, "");
            GUI.Label(r, mensaje, grande);
        }
    }
}
