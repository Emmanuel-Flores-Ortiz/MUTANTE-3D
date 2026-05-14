using UnityEngine; //Libreria basica
using UnityEngine.InputSystem; //Sirve opara definir y conectar las entradas con el codigo

public class CONTROL : MonoBehaviour
{
    //Varibales de nuestro personaje y al mismo tiempo las variables para nuestros 
    Rigidbody rb;

    public InputSystem_Actions actions;

    public float speed;

    float smoothVelocityX; //Variiable que usaremos paras suavizar el traslado de un numero flotante a otro

    public float jumpforce;

    float move;

    bool isGrounded;

    Animator animator;

    void Awake()
    {
        actions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Move.performed += Movement;
        actions.Player.Jump.performed += Jumping;

        actions.Player.Move.canceled += Movement;
        actions.Player.Jump.canceled += Jumping;
    }

    void OnDisable()
    {
        actions.Player.Disable();
        actions.Player.Move.performed -= Movement;
        actions.Player.Jump.performed -= Jumping;

        actions.Player.Move.canceled -= Movement;
        actions.Player.Jump.canceled -= Jumping;
    }


    void Movement(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>().x;
    }

    void Jumping(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            rb.linearVelocityY = jumpforce;
            isGrounded = false;
            animator.SetBool("isJumping", !isGrounded);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        rb.linearVelocityX = move * speed;

        // --- LOGICA DE SUAVIZADO ---
        // Esta parte nos permite saber de que limite a que limite queremos llegar (6 o -6)
        float targetVelocity = Mathf.Abs(rb.linearVelocityX);

        // MoveTowards(valor_actual, destino, velocidad_de_cambio)
        // Cambia el '10f' por un número más bajo si quieres que tarde MÁS en acelerar
        smoothVelocityX = Mathf.MoveTowards(smoothVelocityX, targetVelocity, 10f * Time.deltaTime);

        // Le pasamos el valor suavizado al Animator
        animator.SetFloat("v_x", smoothVelocityX);

        animator.SetFloat("v_y", rb.linearVelocityY);
        flip();
    }

    void flip()
    {

        if (rb.linearVelocityX > 0.1f)
            GetComponent<SpriteRenderer>().flipX = false;

        if (rb.linearVelocityX < -0.1f)
            GetComponent<SpriteRenderer>().flipX = true;
    }

    private void OnCollisionEnter(Collision collision2D)
    {
        isGrounded = true;
        animator.SetBool("isJumping", !isGrounded);
    }

}
