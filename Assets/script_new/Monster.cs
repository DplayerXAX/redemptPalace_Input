using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Monster : MonoBehaviour
{
    [Header("Monster Settings")]
    public Tilemap tilemap;
    public float bounceDamage = 5;
    private Player player;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float bounceSpeed = 10f;
    //[SerializeField] private float wanderSpeed = 1.5f;
    //[SerializeField] private float detectionRange = 500f;
    [SerializeField] private float moveInterval = 1f;
    private bool isBouncing = false;
    private Animator animator;
    private float MonsterLevel = 1;
    public int monsterValue;
    [SerializeField] private int attack = 5;
    [SerializeField] private float initialFrozenTime = 2f;
    public bool isPaused = false;
    protected float health = 10;
    private Queue<Vector3Int> currentPath = new Queue<Vector3Int>();
    private float pathUpdateInterval = 1.5f;
    private float pathUpdateTimer = 0f;
    [Header("Visual")]
    public Vector3 visualOffset = new Vector3(0, 0.25f, 0);
    public SpriteRenderer spriteRenderer;
    public Sprite spriteUp, spriteDown, spriteLeft, spriteRight;
    public float bounceForce = 5f;
    private Vector3Int currentCellPos;
    private bool isMoving = false;
    private Coroutine moveCoroutine;
    private float moveTimer = 0f;
    private Camera mainCamera;
    private Rigidbody2D rb;

    void Start()
    {
        monsterValue = (int)Random.Range(25 * MonsterLevel, 50 * MonsterLevel + 1);
        animator = GetComponent<Animator>();
        player = GameObject.Find("Player").GetComponent<Player>();
        currentCellPos = tilemap.WorldToCell(transform.position);
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        SnapToCell();
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        BasicCameraFollow cameraFollow = mainCamera.GetComponent<BasicCameraFollow>();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            cameraFollow.SetShakeOffset(new Vector3(offsetX, offsetY, 0));

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraFollow.SetShakeOffset(Vector3.zero);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "player")
        {
            player.Hurt(attack);
        }
    }

    public void Hurt()
    {
        health -= player.bounceAttack;
        if (health < 0)
        {
            player.Earn(monsterValue);
            player.KillAMons(this);
        }

    }

    Queue<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        frontier.Enqueue(start);
        visited.Add(start);

        Vector3Int[] directions = {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
    };
        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (var dir in directions)
            {
                Vector3Int next = current + dir;
                if (!visited.Contains(next) && tilemap.HasTile(next + new Vector3Int(1, 1)))
                {
                    frontier.Enqueue(next);
                    visited.Add(next);
                    cameFrom[next] = current;
                }
            }
        }

        Queue<Vector3Int> path = new Queue<Vector3Int>();
        if (!cameFrom.ContainsKey(goal))
            return path;

        Vector3Int currentStep = goal;
        while (currentStep != start)
        {
            path.Enqueue(currentStep);
            currentStep = cameFrom[currentStep];
        }

        Stack<Vector3Int> reversed = new Stack<Vector3Int>();
        while (path.Count > 0)
            reversed.Push(path.Dequeue());
        return new Queue<Vector3Int>(reversed);
    }


    void Update()
    {
        if (isPaused)
        {
            return;
        }
        BounceDetection();
        if (initialFrozenTime > 0)
        {
            initialFrozenTime -= Time.deltaTime;
            return;
        }
        pathUpdateTimer -= Time.deltaTime;
        moveTimer -= Time.deltaTime;
        animator.SetBool("isWalking", isMoving);

        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = pathUpdateInterval;
            Vector3Int targetCell = tilemap.WorldToCell(player.transform.position) - new Vector3Int(1, 1, 0);
            currentPath = FindPath(currentCellPos, targetCell);
        }
        if (moveTimer <= 0f && !isMoving && !isBouncing && currentPath.Count > 0)
        {
            moveTimer = moveInterval;
            Vector3Int nextStep = currentPath.Dequeue();
            Vector3Int direction = nextStep - currentCellPos;
            spriteRenderer.color = Color.red;
            TryMove(direction, chaseSpeed);
        }
    }

    private void BounceDetection()
    {
        Vector3 position = transform.position;
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z - mainCamera.transform.position.z));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, transform.position.z - mainCamera.transform.position.z));
        Vector3Int bounceDirection = Vector3Int.zero;
        if (position.x < min.x)
            bounceDirection = new Vector3Int(1, -1, 0);
        else if (position.x > max.x)
            bounceDirection = new Vector3Int(-1, 1, 0);
        else if (position.y < min.y)
            bounceDirection = new Vector3Int(1, 1, 0);
        else if (position.y > max.y)
            bounceDirection = new Vector3Int(-1, -1, 0);

        if (bounceDirection != Vector3Int.zero && !isBouncing)
        {
            Hurt();
            isBouncing = true;
            Debug.Log("I am bouncing!");
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            StartCoroutine(CameraShake(0.2f, 1.2f));
            for (int i = 10; i >= 1; i--)
            {
                Vector3Int bounceCell = currentCellPos + bounceDirection * i;
                if (tilemap.HasTile(bounceCell + new Vector3Int(1, 1)))
                {
                    currentCellPos = bounceCell;
                    Vector3 targetPos = tilemap.GetCellCenterWorld(currentCellPos) + visualOffset;

                    if (moveCoroutine != null)
                        StopCoroutine(moveCoroutine);

                    moveCoroutine = StartCoroutine(MoveTo(targetPos, bounceSpeed, true));
                    UpdateSprite(bounceDirection);
                    break;
                }
            }
        }
    }
    void SnapToCell()
    {
        transform.position = tilemap.GetCellCenterWorld(currentCellPos) + visualOffset;
    }

    Vector3Int GetDirectionToward(Vector3Int targetCell)
    {
        Vector3Int delta = targetCell - currentCellPos;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return new Vector3Int(Mathf.Clamp(delta.x, -1, 1), 0, 0);
        else
            return new Vector3Int(0, Mathf.Clamp(delta.y, -1, 1), 0);
    }

    Vector3Int GetRandomDirection()
    {
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };
        return directions[Random.Range(0, directions.Length)];
    }

    void TryMove(Vector3Int direction, float speed, bool alterPath = false)
    {
        Vector3Int newCell = currentCellPos + direction;

        if (tilemap.HasTile(newCell + new Vector3Int(1, 1)))
        {
            currentCellPos = newCell;
            Vector3 targetPos = tilemap.GetCellCenterWorld(currentCellPos) + visualOffset;

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            moveCoroutine = StartCoroutine(MoveTo(targetPos, speed));
            UpdateSprite(direction);
        }
    }

    void UpdateSprite(Vector3Int direction)
    {
        if (direction.x > 0) spriteRenderer.sprite = spriteRight;
        else if (direction.x < 0) spriteRenderer.sprite = spriteLeft;
        else if (direction.y > 0) spriteRenderer.sprite = spriteUp;
        else if (direction.y < 0) spriteRenderer.sprite = spriteDown;
    }

    System.Collections.IEnumerator MoveTo(Vector3 target, float speed, bool bounceMark = false)
    {
        isMoving = true;
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        if (bounceMark) isBouncing = false;
        isMoving = false;
    }
}
