using UnityEngine;
using UnityEngine.InputSystem;

public class CONTROL : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;

    [Header("Configuración de Entradas")]
    public InputSystem_Actions actions;
    private Vector2 inputMovimiento;
    private bool estaCorriendo; // Nueva variable para saber si se presiona Shift

    [Header("Configuración de Movimiento")]
    public float walkingSpeed = 3.0f; // Velocidad al caminar
    public float runningSpeed = 7.0f; // Velocidad al correr
    public float jumpforce = 5.0f;

    private float smoothVelocity;
    private bool isGrounded;

    void Awake()
    {
        actions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        actions.Player.Enable();

        actions.Player.Move.performed += Movement;
        actions.Player.Move.canceled += Movement;

        actions.Player.Jump.performed += Jumping;

        // --- NUEVAS LÍNEAS PARA EL SHIFT (SPRINT) ---
        // Cuando se presiona Shift, activamos la carrera
        actions.Player.Sprint.performed += ctx => estaCorriendo = true;
        // Cuando se suelta Shift, volvemos a caminar
        actions.Player.Sprint.canceled += ctx => estaCorriendo = false;
    }

    void OnDisable()
    {
        actions.Player.Disable();
        actions.Player.Move.performed -= Movement;
        actions.Player.Move.canceled -= Movement;
        actions.Player.Jump.performed -= Jumping;

        // Nos desuscribimos del Sprint
        actions.Player.Sprint.performed -= ctx => estaCorriendo = true;
        actions.Player.Sprint.canceled -= ctx => estaCorriendo = false;
    }

    void Movement(InputAction.CallbackContext ctx)
    {
        inputMovimiento = ctx.ReadValue<Vector2>();
    }

    void Jumping(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpforce, rb.linearVelocity.z);
            isGrounded = false;
            animator.SetTrigger("Jump");
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Evita que el mutante se tropiece y se caiga de lado al chocar
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 1. CALCULAR DIRECCIÓN DE TRASLADO 3D
        Vector3 direccionMovimiento = new Vector3(inputMovimiento.x, 0, inputMovimiento.y);

        // EVALUACIÓN DE VELOCIDAD: Si está presionando Shift, usa runningSpeed; si no, walkingSpeed
        float velocidadActual = estaCorriendo ? runningSpeed : walkingSpeed;

        // Multiplicamos la dirección por la velocidad elegida
        Vector3 velocidadFinal = direccionMovimiento * velocidadActual;

        // Aplicamos la velocidad al Rigidbody
        rb.linearVelocity = new Vector3(velocidadFinal.x, rb.linearVelocity.y, velocidadFinal.z);

        // 2. ROTACIÓN EN 3D
        if (direccionMovimiento.magnitude > 0.1f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionMovimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, 15f * Time.deltaTime);
        }

        // 3. LOGICA DE SUAVIZADO PARA EL BLEND TREE
        // Si no nos movemos, el objetivo es 0. 
        // Si nos movemos caminando, el objetivo es la velocidad de caminata (ej. 3).
        // Si corremos, el objetivo es la velocidad de carrera (ej. 7).
        float targetVelocity = velocidadFinal.magnitude;

        // MoveTowards incrementará o decrementará el valor de forma fluida
        smoothVelocity = Mathf.MoveTowards(smoothVelocity, targetVelocity, 30f * Time.deltaTime);

        // Le pasamos el valor al parámetro "Speed" de tu Blend Tree
        animator.SetFloat("Speed", smoothVelocity);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Revisamos si impactamos con algo desde abajo (el suelo)
        foreach (ContactPoint contacto in collision.contacts)
        {
            // Si el vector apunta hacia arriba, es el suelo
            if (contacto.normal.y > 0.6f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    // Opcional: Añadimos esto para cuando el personaje camine por bordes o rampas
    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contacto in collision.contacts)
        {
            if (contacto.normal.y > 0.6f)
            {
                isGrounded = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // En cuanto dejas de tocar el objeto, ya no estás en el suelo
        isGrounded = false;
    }


}