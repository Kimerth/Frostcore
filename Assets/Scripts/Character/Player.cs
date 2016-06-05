using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine.UI;

public class Player : MonoBehaviour 
{
    [System.Serializable]
    public class PlayerStats
    {
        public float maxHP;
        [SerializeField]
        private float _curHP;
        public float curHP
        {
            get { return _curHP; }
            set { _curHP = Mathf.Clamp(value, 0, maxHP); }
        }
        public float percHP
        {
            get { return curHP / maxHP * 100; }
        }
        [SerializeField]
        private float _regenHP = 1f;
        public float regenHP
        {
            get { return _regenHP; }
            set { _regenHP = value; }
        }

        public float initialPlayerTemperature = 37f;
        [SerializeField]
        private float _PlayerTemperature;
        public float PlayerTemperature
        {
            get { return _PlayerTemperature; }
            set { _PlayerTemperature = Mathf.Clamp(value, 20, 38); }
        }

        public float maxStamina;
        [SerializeField]
        private float _curStamina;
        public float curStamina
        {
            get { return _curStamina; }
            set { _curStamina = Mathf.Clamp(value, 0, maxStamina); }
        }
        [SerializeField]
        private float _regenStamina = 10f;
        public float regenStamina
        {
            get { return _regenStamina; }
            set { _regenStamina = value; }
        }
        public float delayRegenStamina;
        public bool delayRegenStaminaActivated;
        public bool stopRegenStamina;

        public float MaxCarryWeight;
        [SerializeField]
        private float _carryWeight;
        public float CarryWeight
        {
            get { return _carryWeight; }
            set { _carryWeight = Mathf.Clamp(value, 0, MaxCarryWeight); }
        }

        public EnergyCells energyCells;

        public void ResetHP()
        {
            curHP = maxHP;
        }

        public void ResetStamina()
        {
            curStamina = maxStamina;
        }

        public void ResetTemperature()
        {
            PlayerTemperature = initialPlayerTemperature;
        }
    }

    [System.Serializable]
    public class EnergyCells
    {
        public float TotalEnergy;
        public float virtualTotalEnergy;

        public float this[int i]
        {
            get { return curEnergyInCells[i]; }
            set 
            {
                TotalEnergy += Mathf.Clamp(value, 0, maxEnergyInCells[i]) - curEnergyInCells[i];
                curEnergyInCells[i] = Mathf.Clamp(value, 0, maxEnergyInCells[i]);
            }
        }

        public float[] curEnergyInCells;
        public float[] maxEnergyInCells;

        public RectTransform[] energyCellsBar;
        public RectTransform totalEnergyText;

        public Transform[] EnergyDrainEffects;

        public void ResetEnergy()
        {
            if (curEnergyInCells.Length != maxEnergyInCells.Length)
                throw new UnassignedReferenceException();

            activeCellindexes = new List<int>();
            virtualTotalEnergy = 0;

            for (int index = 0; index < curEnergyInCells.Length; index++)
            {
                this[index] = maxEnergyInCells[index];
                virtualTotalEnergy += maxEnergyInCells[index];
                activeCellindexes.Add(index);
            }
        }

        public void UpdateUI()
        {
            if (energyCellsBar.Length != curEnergyInCells.Length)
                throw new UnassignedReferenceException();

            for(int index = 0; index < curEnergyInCells.Length; index++)
            {
                if (maxEnergyInCells[index] == 0 && energyCellsBar[index].gameObject.activeSelf)
                    energyCellsBar[index].gameObject.SetActive(false);

                energyCellsBar[index].GetChild(0).localScale = new Vector3(1, this[index] / maxEnergyInCells[index], 1);
            }

            if (virtualTotalEnergy != TotalEnergy)
                virtualTotalEnergy = TotalEnergy;

            totalEnergyText.GetComponent<Text>().text = TotalEnergy.ToString("0.00") + " SEU";

            if(runningInstancesOfDrainEnergy > 0)
                for(int index = 0; index < curEnergyInCells.Length; index++)
                {
                    if (activeCellindexes.Contains(index))
                    {
                        if (!EnergyDrainEffects[index].gameObject.activeInHierarchy)
                            EnergyDrainEffects[index].gameObject.SetActive(true);
                    }
                    else
                    {
                        if (EnergyDrainEffects[index].gameObject.activeInHierarchy)
                            EnergyDrainEffects[index].gameObject.SetActive(false);
                    }
                }
            else
                for (int index = 0; index < curEnergyInCells.Length; index++)
                {
                    if (EnergyDrainEffects[index].gameObject.activeInHierarchy)
                        EnergyDrainEffects[index].gameObject.SetActive(false);
                }
        }

        List<int> activeCellindexes;

        volatile int runningInstancesOfDrainEnergy = 0;
        public IEnumerator DrainEnergy(float amount, float time = 0)
        {
            runningInstancesOfDrainEnergy++;
            virtualTotalEnergy -= amount;
            float divAmount;

            if (time == 0)
                divAmount = amount / activeCellindexes.Count;
            else
                divAmount = amount / activeCellindexes.Count / time * Time.deltaTime;

            while(amount > 0)
            {
                for (int indexOfindex = 0; indexOfindex < activeCellindexes.Count; indexOfindex++)
                {
                    if (amount - divAmount <= 0)
                    {
                        if(curEnergyInCells[activeCellindexes[indexOfindex]] >= amount)
                        {
                            this[activeCellindexes[indexOfindex]] -= amount;
                            amount = 0;
                            break;
                        }
                        else
                        {
                            amount -= this[activeCellindexes[indexOfindex]];
                            this[activeCellindexes[indexOfindex]] = 0;
                            continue;
                        }
                    }

                    if (curEnergyInCells[activeCellindexes[indexOfindex]] <= divAmount)
                    {
                        amount -= this[activeCellindexes[indexOfindex]];
                        this[activeCellindexes[indexOfindex]] = 0;
                        
                        activeCellindexes.RemoveAt(indexOfindex);

                        if (time == 0)
                            divAmount = amount / activeCellindexes.Count;
                        else
                            divAmount = amount / activeCellindexes.Count / time * Time.deltaTime;
                        
                        continue;
                    }

                    this[activeCellindexes[indexOfindex]] -= divAmount;
                    amount -= divAmount;
                }

                yield return null;
            }

            virtualTotalEnergy += amount;

            runningInstancesOfDrainEnergy--;
        }
    }

    public PlayerStats pStats;

    public static Player Instance;

    private float timeToRegenStamina;

    public bool IsDead = false;

    [Range(0, 1)]
    public float LowTemperatureSpeedModifier;

    public float temperatureLossPerTime = 0.02f;
    public float timeToLoseTemperature = 2f;

    [System.Serializable]
    public class PlayerStatsUI
    {
        public RectTransform HPBar;
        public Text HPPercentage;

        public RectTransform BodyTemperatureBar;
        public Text BodyTemperature;

        public RectTransform StaminaBar;

        public Text EnvironmentTemperature;
    }

    public PlayerStatsUI pStatsUI;

    void Awake()
    {
        if(pStats == null)
            pStats = new PlayerStats();

        Instance = this;
    }

    public void Start() 
    {
        pStats.ResetHP();
        pStats.ResetStamina();
        pStats.ResetTemperature();
        pStats.energyCells.ResetEnergy();
        pStats.CarryWeight = 0;

        timeToRegenStamina = Time.time;

        GameMaster.gm.m_Player = this.transform;
	}

    public void AttachCollisionBasedDamage(CollisionBasedDamage theSource)
    {
        theSource.thisPlayers = this;
    }

    void Update()
    {
        if (pStats.PlayerTemperature == 20)
        {
            object[] damage = new object[2];
            damage[0] = pStats.maxHP / 4 * Time.deltaTime;
            ApplyDamage(damage);

            UpdateUI();
            return;
        }

        if (pStats.curStamina == 0 && !pStats.delayRegenStaminaActivated)
        {
            pStats.delayRegenStaminaActivated = true;
            timeToRegenStamina = Time.time + pStats.delayRegenStamina;
        }

        if (timeToRegenStamina < Time.time && !pStats.stopRegenStamina)
        {
            pStats.delayRegenStaminaActivated = false;
            pStats.curStamina += pStats.regenStamina * Time.deltaTime;
        }

        pStats.curHP += pStats.regenHP * Time.deltaTime;

        pStats.PlayerTemperature -= temperatureLossPerTime * Time.deltaTime;

        pStats.stopRegenStamina = false;

        if(shield != null)
        {
            if(Input.GetKeyDown(KeyCode.H))
            {
                if (shield.gameObject.activeInHierarchy)
                    shield.gameObject.SetActive(false);
                else
                {
                    if (pStats.energyCells.virtualTotalEnergy > shield.TurnOnEnegy)
                    {
                        StartCoroutine(pStats.energyCells.DrainEnergy(shield.TurnOnEnegy, 0.1f));
                        shield.gameObject.SetActive(true);
                    }
                }
            }
        }

        UpdateUI();
    }

    public void DrainStamina(float amount)
    {
        pStats.stopRegenStamina = true;
        pStats.curStamina -= amount;
    }

    public EnergyShield shield;
    List<CollisionBasedDamage> ignoreSources = new List<CollisionBasedDamage>();

    public void ApplyDamage(object[] tempStorage)
    {
        float damage = (float)tempStorage[0];
        CollisionBasedDamage source = tempStorage[1] as CollisionBasedDamage;

        if (source != null)
            if (ignoreSources.Contains(source))
                return;

        if (source != null)
        {
            ignoreSources.Add(source);
            StartCoroutine(resetIgnoreSource(source));
        }

        if (GetComponent<P2D_Motor>().IsDashing)
            return;
        pStats.curHP -= damage;

        if(pStats.curHP == 0)
        {
            IsDead = true;
            UpdateUI();
            GameMaster.KillPlayer(this);
        }
    }

    IEnumerator resetIgnoreSource(CollisionBasedDamage source)
    {
        yield return new WaitForSeconds(0.5f);

        ignoreSources.Remove(source);
    }

    public void RecieveHeat(float amount)
    {
        pStats.PlayerTemperature += amount;
    }

    void OnCollisionEnter2D(Collision2D theCollision)
    {
        float damage = 0;
        foreach (ContactPoint2D contact in theCollision.contacts)
        {
            float auxdamage = Mathf.Abs(Vector2.Dot(contact.normal, lastFrameVelocity));
            if (auxdamage > damage)
                damage = auxdamage;
        }

        if (damage > 15)
        {
            object[] tempStorage = new object[2];
            tempStorage[0] = 0.2f;
            tempStorage[1] = false;
            P2D_Motor.Instance.Stagger(tempStorage);
        }

        if (damage > 20)
            damage *= pStats.maxHP / 100;
        else 
            damage = 0;

        if (P2D_Motor.Instance.IsDashing)
            damage = 0;

        damage = Mathf.Clamp(damage - 20, 0, pStats.maxHP);

        object[] tempStorage2 = new object[2];
        tempStorage2[0] = damage;
        ApplyDamage(tempStorage2);

        if (damage > 0)
            GameMaster.gm.camShake.Shake(damage / 400, damage / 400);
    }

    Vector2 lastFrameVelocity;

    void OnEnable()
    {
        StartCoroutine(SaveThisFrameVelocity());
    }

    IEnumerator SaveThisFrameVelocity()
    {
        for(;;)
        {
            yield return new WaitForEndOfFrame();

            lastFrameVelocity = GetComponent<Rigidbody2D>().velocity;

            yield return null;
        }
    }

    void UpdateUI()
    {
        pStatsUI.HPBar.GetComponent<Image>().fillAmount = pStats.curHP / pStats.maxHP;
        pStatsUI.BodyTemperatureBar.GetComponent<Image>().fillAmount = (pStats.PlayerTemperature - 20) / 18;

        if (pStats.percHP < 35 && pStats.PlayerTemperature > 20)
        {
            pStatsUI.HPBar.GetComponent<Image>().color = Color.red;
            pStatsUI.HPPercentage.color = Color.red;
        }
        else if (pStats.PlayerTemperature > 20)
        {
            pStatsUI.HPBar.GetComponent<Image>().color = Color.green;
            pStatsUI.HPPercentage.color = Color.green;
        }
        else
        {
            pStatsUI.HPBar.GetComponent<Image>().color = Color.blue;
            pStatsUI.HPPercentage.color = Color.blue;
        }

        if (pStats.PlayerTemperature < 25)
        {
            pStatsUI.BodyTemperatureBar.GetComponent<Image>().color = Color.red;
            pStatsUI.BodyTemperature.color = Color.red;
        }
        else
        {
            pStatsUI.BodyTemperatureBar.GetComponent<Image>().color = Color.blue;
            pStatsUI.BodyTemperature.color = Color.blue;
        }

        pStatsUI.HPPercentage.text = " " + pStats.percHP.ToString("0.00") + "%";
        pStatsUI.BodyTemperature.text = pStats.PlayerTemperature.ToString("0.00") + " °C";

        FrostEffect.Instance.FrostAmount = 0.6f - pStats.PlayerTemperature / 100;

        pStatsUI.EnvironmentTemperature.text = "273K";

        pStatsUI.StaminaBar.localScale = new Vector3(pStats.curStamina / pStats.maxStamina, 1, 1);

        pStats.energyCells.UpdateUI();
    }
}
