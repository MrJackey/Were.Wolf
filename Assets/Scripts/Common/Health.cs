using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {
	[SerializeField] private float health = 100;
	[SerializeField] private float maxHealth = 100;
	[SerializeField] private bool invincible = false;
	[SerializeField] private bool dead = false;

	[Header("Events")]
	[SerializeField] private UnityEvent<float> onTakeDamage = null;
	[SerializeField] private UnityEvent<float> onHeal = null;
	[SerializeField] private UnityEvent onValueChange;
	[SerializeField] private UnityEvent onDie = null;

	public float Value {
		get => health;
		set => SetHealth(value);
	}

	public float MaxValue {
		get => maxHealth;
		set => maxHealth = value;
	}

	public float HealthFraction {
		get => health / maxHealth;
		set => Value = maxHealth * value;
	}

	public int IntValue => Mathf.RoundToInt(health);

	public bool IsInvincible {
		get => invincible;
		set => invincible = value;
	}

	public bool IsDead {
		get => dead;
		set => dead = value;
	}

	public UnityEvent<float> OnTakeDamage => onTakeDamage;
	public UnityEvent<float> OnHeal => onHeal;
	public UnityEvent OnValueChange => onValueChange;
	public UnityEvent OnDie => onDie;


	public void TakeDamage(float amount) {
		if (invincible) return;

		float delta = SetHealth(health - amount);
		if (delta == 0) return;
		onTakeDamage.Invoke(delta);

		if (health == 0)
			Die();
	}

	public void Heal(float amount) {
		float delta = SetHealth(health + amount);
		if (delta == 0) return;
		onHeal.Invoke(delta);
	}

	public void Die() {
		health = 0;
		dead = true;
		onDie.Invoke();
	}

	public void RestoreHealth() {
		health = maxHealth;
		dead = false;
		onValueChange.Invoke();
	}

	private float SetHealth(float value) {
		float newValue = Mathf.Clamp(value, 0, maxHealth);
		if (health == newValue) return 0;

		float oldValue = health;
		health = newValue;
		onValueChange.Invoke();
		return oldValue - newValue;
	}
}