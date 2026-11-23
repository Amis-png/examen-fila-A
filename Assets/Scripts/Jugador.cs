using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Movimiento tipo Mario: caminar y saltar.
// Teclas del Grupo A (pizarra): Flecha ↑ = izquierda, A = derecha, V = saltar.
// Para cambiarlas, edita las tres teclas de abajo.
public class Jugador : MonoBehaviour
{
    public float velocidad = 6f;
    public float fuerzaSalto = 12f;

#if ENABLE_INPUT_SYSTEM
    Key teclaIzquierda = Key.UpArrow;
    Key teclaDerecha = Key.A;
    Key teclaSalto = Key.V;

    bool Pulsada(Key t) { return Keyboard.current != null && Keyboard.current[t].isPressed; }
    bool RecienPulsada(Key t) { return Keyboard.current != null && Keyboard.current[t].wasPressedThisFrame; }
#else
    KeyCode teclaIzquierda = KeyCode.UpArrow;
    KeyCode teclaDerecha = KeyCode.A;
    KeyCode teclaSalto = KeyCode.V;

    bool Pulsada(KeyCode t) { return Input.GetKey(t); }
    bool RecienPulsada(KeyCode t) { return Input.GetKeyDown(t); }
#endif

    Rigidbody2D rb;
    bool enSuelo;
    bool quiereSaltar;
    float direccion;
    Vector3 escala;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        escala = transform.localScale;
    }

    void Update()
    {
        direccion = 0f;
        if (Pulsada(teclaIzquierda)) direccion -= 1f;
        if (Pulsada(teclaDerecha)) direccion += 1f;

        if (RecienPulsada(teclaSalto)) quiereSaltar = true;

        // Mirar hacia donde camina
        if (direccion != 0f)
            transform.localScale = new Vector3(Mathf.Abs(escala.x) * Mathf.Sign(direccion), escala.y, escala.z);
    }

    void FixedUpdate()
    {
        Vector2 v = rb.linearVelocity;
        v.x = direccion * velocidad;

        if (quiereSaltar && enSuelo)
        {
            v.y = fuerzaSalto;
            Juego.instancia.SonidoSalto();
        }
        rb.linearVelocity = v;

        quiereSaltar = false;
        enSuelo = false; // se vuelve a activar si sigue tocando el suelo
    }

    void OnCollisionStay2D(Collision2D c)
    {
        for (int i = 0; i < c.contactCount; i++)
        {
            if (c.GetContact(i).normal.y > 0.5f) enSuelo = true; // algo debajo
        }
    }
}
