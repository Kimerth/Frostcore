using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour 
{
    [System.Serializable]
    public class PlayerStats
    {
        public float maxHP;
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
        private float _regenHP = 1f;
        public float regenHP
        {
            get { return _regenHP; }
            set { _regenHP = value; }
        }

        public float initialPlayerTemperature = 37f;
        private float _PlayerTemperature;
        public float PlayerTemperature
        {
            get { return _PlayerTemperature; }
            set { _PlayerTemperature = Mathf.Clamp(value, 20, 38); }
        }

        public float MaxCarryWeight;
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

        public void ResetTemperature()
        {
            PlayerTemperature = initialPlayerTemperature;
        }
    }
    public PlayerStats pStats;

    public static Player Instance;

    private float curTime;

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

        public Text EnvironmentTemperature;
    }

    public PlayerStatsUI pStatsUI;

    void Awake()
    {
        if(pStats == null)
            pStats = new PlayerStats();

        Instance = this;
    }

    void Start() 
    {
        pStats.ResetHP();
        pStats.ResetTemperature();
        pStats.CarryWeight = 0;

        curTime = Time.time;
        timeTemperature = Time.time;

        GameMaster.gm.m_Player = this.transform;
	}
	
	void Update() 
    {
        if (curTime < Time.time)
        {
            pStats.curHP += pStats.regenHP;
            curTime = Time.time + 1f;
        }

        if (Time.time > timeTemperature)
        {
            timeTemperature = Time.time + timeToLoseTemperature;
            pStats.PlayerTemperature -= temperatureLossPerTime;
        }

        UpdateUI();
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
    }
}
