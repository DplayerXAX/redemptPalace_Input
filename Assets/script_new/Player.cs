using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.U2D;
using TMPro;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

//This class represents a player, but lots of code here
//All you need to know is the player can move, dash and damaged by monsters
//It gets buff by items
public class Player : MonoBehaviour
{
    public static Player Instance;

    [Header("Player data")]
    //private bool enterRoom = false;
    private bool canControl = true;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider energyBar;
    // { get; private set; } means you can get the value from other scripts
    // but only this script can change it....
    //Similarly, { private get;  set; }  means other scripts can set it, but cannot read!
    public float energy  = 100;
    public float health  = 100;
    [SerializeField] private float energyRecoverySpeed = 0.2f;
    private float ERMulti = 1f;
    [SerializeField] private float energyLostSpeed = 0.4f;
    [SerializeField] private MapGenerator mg;
    private bool startEscaping = false;
    public int bounceAttack = 5;
    private float bounceMulti = 1f;

    [Header("Player Movement")]
    public Tilemap tilemap;
    public RectInt mySafeHouse;
    public float moveSpeed = 5f;
    public float moveSpeedMultiplier = 1f;
    public int moveMultiplier = 1;
    public Vector3 visualOffset = new Vector3(0, 0.25f, 0);
    private Vector3Int currentCellPos;
    public bool isMoving = false;
    [SerializeField] private float defaultSpeed = 3f;
    [SerializeField] public float rushingSpeed = 6f;
    public float rushMulti = 1f;
    public float spawnInterval = 5f;
    public int spawnRadius = 4;
    public int maxMonstersPerSpawn = 3;
    public GameObject monsterPrefab;
    private Vector3 targetWorldPos;
    private Coroutine moveCoroutine;
    public ItemEffectHandler effectHandler;
    private bool flipControl = false;
    private Vector3Int lastDirection = Vector3Int.zero;
    private PixelPerfectCamera ppc;
    [Header("Player Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;

    [Header("Player in-run")]
    [SerializeField] private bool generateMonster = true;
    private int diamonds_play = 0;
    private int money_play = 999;
    private int bodyPart_play = 0;
    private List<Monster> monsters = new List<Monster>();
    [SerializeField] private TextMeshProUGUI moneyDisplay;
    [SerializeField] private TextMeshProUGUI diamondDisplay;
    [SerializeField] private TextMeshProUGUI bodyPartDisplay;
    [SerializeField] private float runCameraSize = 4.5f;
    //[SerializeField] private float safeCameraSize = 1.5f;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _energyRecoverySpeed = 0.2f;
    [SerializeField] private float invinCoolDown = 0.5f;
    private float cameraMultiplier = 1f;
    private bool invincibility;
    [Header("Player origin attribute")]
    public float _rushSpeed;
    [SerializeField] private float _bounceAttack;

    private List<ItemData> ownedItems = new();

    private int stepTaken = 0;

    private void Awake()
    {
        Instance = this;
        ppc = Camera.main.GetComponent<PixelPerfectCamera>();
    }

    void Start()
    {
        effectHandler = this.gameObject.AddComponent<ItemEffectHandler>();
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        currentCellPos = tilemap.WorldToCell(transform.position);
        SnapToCell();
    }

    void Update()
    {
        //When player leaves the original spot, start escape
        if (!startEscaping && !mySafeHouse.Contains(new Vector2Int(currentCellPos.x + 1, currentCellPos.y + 1)))
        {
            Debug.Log("Player starts escaping!");
            startEscaping = true;
            StartCoroutine(AnimateResolutionChange(runCameraSize, 0.2f));
            if(generateMonster)
                StartCoroutine(SpawnMonstersRoutine());

        }
        energyBar.value = energy;
        healthBar.value = health;
        Vector3Int direction = Vector3Int.zero;
        if (!canControl) return;
        if (invincibility)
        {
            invinCoolDown -= Time.deltaTime;
            if (invinCoolDown < 0) { invincibility = false; invinCoolDown = 0.5f; }
        }

        if (Input.GetKey(KeyCode.Space) && energy > energyLostSpeed)
        {
            moveSpeed = rushingSpeed;
            energy -= energyLostSpeed;
        }
        else
        {
            moveSpeed = defaultSpeed;
            if (energy < 100)
                energy += energyRecoverySpeed;
        }
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            direction = new Vector3Int((int)Mathf.Sign(horizontal) * moveMultiplier, 0, 0);

            if (horizontal > 0)
                spriteRenderer.sprite = spriteRight;
            else
                spriteRenderer.sprite = spriteLeft;
        }
        else if (Mathf.Abs(vertical) > 0.1f)
        {
            direction = new Vector3Int(0, (int)Mathf.Sign(vertical) * moveMultiplier, 0);

            if (vertical > 0)
                spriteRenderer.sprite = spriteUp;
            else
                spriteRenderer.sprite = spriteDown;
        }

        if (direction != Vector3Int.zero)
        {
            if (flipControl)
                direction = -direction;
            if (!isMoving || direction != lastDirection)
            {
                Vector3Int newCellPos = currentCellPos + direction;
                if (tilemap.HasTile(newCellPos + new Vector3Int(1, 1)))
                {
                    currentCellPos = newCellPos;
                    targetWorldPos = tilemap.GetCellCenterWorld(currentCellPos) + visualOffset;
                    stepTaken++;
                    if (isMoving && moveCoroutine != null)
                    {
                        StopCoroutine(moveCoroutine);
                        isMoving = false;
                    }
                    lastDirection = direction;
                    MoveEffect(targetWorldPos);
                    moveCoroutine = StartCoroutine(MoveTo(targetWorldPos));
                }
            }
        }

        checkCurrentPile();
        //spriteRenderer.sortingOrder = -(int)transform.position.y;
    }

    #region item Management
    ItemEffect CreateEffectByName(string effectName)
    {
        switch (effectName)
        {
            case "SpeedUpWhileDashing":
                return new SpeedUpWhileDashing(this);
            case "EnergyOnKill":
                return new EnergyOnKill(this);
            case "NoEnergyUseHPToDash":
                return null;

            default:
                return null;
        }
    }

    public bool TryPurchaseItem(ItemData item, int price)
    {
        if (money_play - price >= 0 && ItemDatabase.Instance.availableItems.Contains(item))
        {
            money_play -= price;
            AddItem(item);
            return true;

        }
        return false;
    }
    public void ApplyItemEffects(List<ItemEffectEntry> effects)
    {
        if (effects == null) return;
        foreach (var pair in effects)
        {
            switch (pair.effect)
            {
                case "moveSpeedPercent":
                    moveSpeedMultiplier += pair.value;
                    moveSpeed = _moveSpeed * moveSpeedMultiplier;
                    break;
                case "cameraViewPercent":
                    cameraMultiplier += pair.value;
                    Debug.Log($"I see{pair.value}");
                    StartCoroutine(AnimateResolutionChange(runCameraSize * cameraMultiplier, 0.5f));
                    break;
                case "offscreenDamageBonus":
                    bounceMulti += pair.value;
                    bounceAttack = (int)(_bounceAttack * bounceMulti);
                    break;
                case "energyRegen":
                    ERMulti += pair.value;
                    energyRecoverySpeed = _energyRecoverySpeed * ERMulti;
                    break;


                    /*EnergyOnKill,
    NoEnergyUseHPToDash,
    DashHeal,
    BurnTrail,
    AutoSpike,
    DashArmorAndDamage,
    DodgeChance,
    FlipControl,
    CoinDropOnDash,
    EnergyRegen,
    RefillEnergy,
    DeathImmunity,
    EnemyKnockback*/
                    //more
            }
        }
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;
        //add item to the playerList
        ownedItems.Add(item);
        //add some basic value
        ApplyItemEffects(item.effects);
        //add special Value
        if (!string.IsNullOrEmpty(item.specialEffect))
        {
            ItemEffect effect = CreateEffectByName(item.specialEffect);
            if (effect != null)
            {
                effectHandler.AddEffect(effect);
            }
        }
    }


    public void AddItem(int itemId)
    {
        var item = ItemDatabase.Instance.GetItemById(itemId);
        if (item == null) return;
        //add item to the playerList
        ownedItems.Add(item);
        //add some basic value
        ApplyItemEffects(item.effects);
        //add special Value
        if (!string.IsNullOrEmpty(item.specialEffect))
        {
            ItemEffect effect = CreateEffectByName(item.specialEffect);
            if (effect != null)
            {
                effectHandler.AddEffect(effect);
            }
        }
    }
    #endregion


    #region player status

    public 

    //This is just zoom out the camera using Coroutine
    IEnumerator AnimateResolutionChange(float size, float duration)
    {
        float startSize = Camera.main.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Camera.main.orthographicSize = Mathf.Lerp(startSize, size, t);
            yield return null;
        }

        Camera.main.orthographicSize = size;
    }
    public void Earn(int money = 0, int diamond = 0, int bodies = 0)
    {
        money_play += money;
        diamonds_play += diamond;
        bodyPart_play += bodies;
        moneyDisplay.text = money_play.ToString();
        diamondDisplay.text = diamonds_play.ToString();
        bodyPartDisplay.text = bodyPart_play.ToString();

    }
    public void PauseControl(bool newControl)
    {
        canControl = newControl;
        foreach (var mon in monsters)
        {
            mon.isPaused = newControl;
        }
    }


    public void Hurt(int damage)
    {
        if (!invincibility)
        {
            health -= damage;
            invincibility = true;
        }
    }
    #endregion

    #region Movement
    void SnapToCell()
    {
        transform.position = tilemap.GetCellCenterWorld(currentCellPos);
    }

    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        isMoving = false;
    }
    private void checkCurrentPile()
    {
        Vector3Int tilePos = currentCellPos + new Vector3Int(1, 1, 0);
        TileBase currentTile = tilemap.GetTile(tilePos);


    }

    private void MoveEffect(Vector3 targetWorldPos)
    {
        effectHandler.MoveActivate(targetWorldPos);
    }
    #endregion


    #region Monster
    IEnumerator SpawnMonstersRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnMonstersAroundPlayer();
        }
    }

    void SpawnMonstersAroundPlayer()
    {
        Vector3Int playerCell = tilemap.WorldToCell(transform.position);
        List<Vector3Int> validPositions = new List<Vector3Int>();

        for (int x = -spawnRadius; x <= spawnRadius; x++)
        {
            for (int y = -spawnRadius; y <= spawnRadius; y++)
            {
                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int checkCell = playerCell + offset;

                if (tilemap.HasTile(checkCell + new Vector3Int(1, 1)))
                {
                    validPositions.Add(checkCell);
                }
            }
        }
        int spawnCount = Mathf.Min(maxMonstersPerSpawn, validPositions.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3Int spawnCell = validPositions[Random.Range(0, validPositions.Count)];
            validPositions.Remove(spawnCell);

            Vector3 spawnWorldPos = tilemap.GetCellCenterWorld(spawnCell) + new Vector3(0, 0.25f, 0);
            GameObject newMon = Instantiate(monsterPrefab, spawnWorldPos, Quaternion.identity);
            monsters.Add(newMon.GetComponent<Monster>());
            newMon.SetActive(true);
        }
    }



    internal void KillAMons(Monster monster)
    {
        effectHandler.KillActivate();
        monsters.Remove(monster);
        Destroy(monster.gameObject);
    }
    #endregion
   


   
}