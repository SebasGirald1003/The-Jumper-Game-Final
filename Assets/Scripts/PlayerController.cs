using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerControls controls;
    private Animator animator;
    private CapsuleCollider capsule;
    public GameObject textObject;

    private bool isGrounded = false;
    private bool isJumping = false;
    private bool isSliding = false;
    private bool isDead = false;
    private float holdTime = 0f;
    private float jumpStartY;

    [Header("Salto interpolado")]
    public float minJumpHeight = 12f;
    public float maxJumpHeight = 24f;
    public float maxHoldTime = 0.4f;
    public float jumpSpeed = 8f;

    [Header("Slide")]
    public float slideDuration = 0.7f;
    public float slideHeight = 0.9f;
    public float slideCenterY = 0.45f;

    private float originalCapsuleHeight;
    private Vector3 originalCapsuleCenter;

    [Header("Dissolve Effect")]
    public Material dissolveMaterial;
    public float dissolveSpeed = 1.2f;
    public string dissolveProperty = "_DissolveStrength";

    private Renderer[] renderers;
    private Material runtimeDissolveMat;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        animator = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();

        if (capsule != null)
        {
            originalCapsuleHeight = capsule.height;
            originalCapsuleCenter = capsule.center;
        }

        controls = new PlayerControls();
        textObject.SetActive(false);

        controls.Player.Jump.started += ctx => StartJump();
        controls.Player.Jump.canceled += ctx => EndJump();
        controls.Player.Slide.started += ctx => StartSlide();
        controls.Player.Slide.canceled += ctx => StopSlide();

        renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Start() => GameManager.Instance.StartGame();

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        if (isDead) return;

        animator.SetFloat("Speed", 1f);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsSliding", isSliding);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        animator.SetFloat("Speed", 1f);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsSliding", isSliding);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    // ---- SALTO ----
    private void StartJump()
    {
        if (isDead) return;
        if (isGrounded && !isSliding)
        {
            isGrounded = false;
            isJumping = true;
            holdTime = 0f;
            jumpStartY = transform.position.y;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            animator.SetTrigger("Jump");
            StartCoroutine(HandleJump());
        }
    }

    private IEnumerator HandleJump()
    {
        while (isJumping)
        {
            holdTime += Time.deltaTime;
            float t = Mathf.Clamp01(holdTime / maxHoldTime);
            float targetHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, t);
            float desiredY = jumpStartY + targetHeight;

            // Subir mientras no superemos la altura deseada
            if (transform.position.y < desiredY && holdTime < maxHoldTime)
            {
                float step = jumpSpeed * Time.deltaTime;
                rb.MovePosition(new Vector3(transform.position.x, transform.position.y + step, transform.position.z));
            }
            else
            {
                // En cuanto llegamos al tope, activar la caída
                BeginFall();
            }

            yield return null;
        }
    }

    private void BeginFall()
    {
        if (!isJumping) return;

        isJumping = false;
        rb.useGravity = true;
        rb.linearVelocity = new Vector3(0f, -4f, 0f); // fuerza de caída controlada
    }

    private void EndJump()
    {
        // Si suelta el botón antes de llegar al máximo, empieza a caer
        if (isJumping)
            BeginFall();
    }

    // ---- SLIDE ----
    private void StartSlide()
    {
        if (isDead) return;

        if (isGrounded && !isSliding && !isJumping)
        {
            isSliding = true;
            animator.SetBool("IsSliding", true);

            if (capsule != null)
            {
                capsule.height = slideHeight;
                capsule.center = new Vector3(originalCapsuleCenter.x, slideCenterY, originalCapsuleCenter.z);
            }

            StartCoroutine(EndSlideAfterTime());
        }
    }

    private IEnumerator EndSlideAfterTime()
    {
        yield return new WaitForSeconds(slideDuration);
        EndSlide();
    }

    private void StopSlide() => EndSlide();

    private void EndSlide()
    {
        if (!isSliding) return;

        isSliding = false;
        animator.SetBool("IsSliding", false);

        if (capsule != null)
        {
            capsule.height = originalCapsuleHeight;
            capsule.center = originalCapsuleCenter;
        }
    }

    // ---- COLISIONES ----
    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        CheckGround(collision);

        // 💥 Solo detectar colisión lateral (en X)
        foreach (ContactPoint cp in collision.contacts)
        {
            // Detectar si el impacto fue más lateral que vertical
            if (Mathf.Abs(cp.normal.x) > 0.5f && collision.gameObject.CompareTag("Obstacle"))
            {
                StartDeathSequence();
                return;
            }
        }
    }

    private void OnCollisionStay(Collision collision) => CheckGround(collision);

    private void CheckGround(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground")) return;

        foreach (ContactPoint cp in collision.contacts)
        {
            if (cp.normal.y > 0.5f)
            {
                isGrounded = true;
                isJumping = false;
                rb.useGravity = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    // ---- MUERTE Y EFECTO ----
    private void StartDeathSequence()
    {
        if (isDead) return;

        isDead = true;
        textObject.SetActive(true);
        textObject.GetComponent<TextMeshProUGUI>().text = "GAME OVER!";
        StartCoroutine(DissolveAndRestart());
    }

    private IEnumerator DissolveAndRestart()
    {
        GameManager.Instance.GameOver();

        // 🔥 Detiene todo el tiempo del juego
        Time.timeScale = 0f;

        // Cambiamos temporalmente a tiempo real para que la corrutina funcione
        float dissolve = 0f;
        if (dissolveMaterial != null)
        {
            runtimeDissolveMat = new Material(dissolveMaterial);

            foreach (var r in renderers)
            {
                int count = r.sharedMaterials.Length;
                Material[] newMats = new Material[count];
                for (int i = 0; i < count; i++) newMats[i] = runtimeDissolveMat;
                r.materials = newMats;
            }

            runtimeDissolveMat.SetFloat(dissolveProperty, dissolve);

            // ⚙️ Usamos tiempo no afectado por Time.timeScale
            while (dissolve < 1f)
            {
                dissolve += Time.unscaledDeltaTime * dissolveSpeed;
                runtimeDissolveMat.SetFloat(dissolveProperty, dissolve);
                yield return null;
            }
        }

        // Espera 2 segundos reales antes de reiniciar
        yield return new WaitForSecondsRealtime(2f);

        // 🔁 Reinicia la escena actual
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
