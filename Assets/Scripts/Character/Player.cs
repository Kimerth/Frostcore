using UnityEngine;
using System.Collections;
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
    public PlayerStats pStats;

    public static Player Instance;

    private float timeToRegenHP;
    private float timeToRegenStamina;

    public bool IsDead = false;

    public float temperatureLossPerTime = 0.02f;
    public float timeToLoseTemperature = 2f;
    private float timeTemperature;


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
        pStats.CarryWeight = 0;

        timeToRegenHP = Time.time;
        timeToRegenStamina = Time.time;
        timeTemperature = Time.time;

        GameMaster.gm.m_Player = this.transform;
	}

    void Update()
    {
        if (pStats.curStamina == 0 && !pStats.delayRegenStaminaActivated)
        {
            pStats.delayRegenStaminaActivated = true;
            timeToRegenStamina = Time.time + pStats.delayRegenStamina;
        }

        if (timeToRegenStamina < Time.time && !pStats.stopRegenStamina)
        {
            pStats.delayRegenStaminaActivated = false;
            pStats.curStamina += pStats.regenStamina / 10;
            timeToRegenStamina = Time.time + 0.1f;
        }

        if (timeToRegenHP < Time.time)
        {
            pStats.curHP += pStats.regenHP / 10;
            timeToRegenHP = Time.time + 0.1f;
        }

        if (timeTemperature < Time.time)
        {
            timeTemperature = Time.time + timeToLoseTemperature;
            pStats.PlayerTemperature -= temperatureLossPerTime;
        }

        pStats.stopRegenStamina = false;

        UpdateUI();
    }

    public void DrainStamina(float amount)
    {
        pStats.stopRegenStamina = true;
        pStats.curStamina -= amount;
    }

    void ApplyDamage(float damage)
    {
        if (GetComponent<P2D_Motor>().IsDashing)
            return;
        pStats.curHP -= damage;

        if(pStats.curHP == 0)
        {
            IsDead = true;
            GameMaster.KillPlayer(this);
        }
    }

    public void RecieveHeat(float amount)
    {
        pStats.PlayerTemperature += amount;
    }

    void UpdateUI()
    {
        pStatsUI.HPBar.GetComponent<Image>().fillAmount = pStats.curHP / pStats.maxHP;
        pStatsUI.BodyTemperatureBar.GetComponent<Image>().fillAmount = (pStats.PlayerTemperature - 20) / 18;

        if (pStats.percHP < 35)
        {
            pStatsUI.HPBar.GetComponent<Image>().color = Color.red;
            pStatsUI.HPPercentage.color = Color.red;
        }
        else
        {
            pStatsUI.HPBar.GetComponent<Image>().color = Color.green;
            pStatsUI.HPPercentage.color = Color.green;
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

        pStatsUI.HPPercentage.text = " " + pStats.percHP + "%";
        pStatsUI.BodyTemperature.text = pStats.PlayerTemperature + " °C";
        pStatsUI.EnvironmentTemperature.text = "273K";

        pStatsUI.StaminaBar.localScale = new Vector3(pStats.curStamina / pStats.maxStamina, 1, 1);
    }
}
