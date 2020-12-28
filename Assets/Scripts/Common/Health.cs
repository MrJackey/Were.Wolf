using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {
	[SerializeField] private float health = 100;
	[SerializeField] private float maxHealth = 100;
	[SerializeField] private bool invincible = false;
	[SerializeField] private bool dead = false;
	[SerializeField] private Transformation transformation;

	[Header("Events")]
	[SerializeField] private UnityEvent<float> onTakeDamageWolf = null;
	[SerializeField] private UnityEvent<float> onTakeDamageHuman = null;
	[SerializeField] private UnityEvent<float> onHeal = null;
	[SerializeField] private UnityEvent onValueChange;
	[SerializeField] private UnityEvent<DamageSource> onDie = null;

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

	public UnityEvent<float> OnTakeDamage => onTakeDamageWolf;
	public UnityEvent<float> OnHeal => onHeal;
	public UnityEvent OnValueChange => onValueChange;
	public UnityEvent<DamageSource> OnDie => onDie;

	public void TakeDamage(float amount) {
		TakeDamage(amount, DamageSource.Generic);
	}

	public void TakeDamage(float amount, DamageSource damageSource) {
		if (invincible) return;

		float delta = SetHealth(health - amount);
		if (delta == 0) return;

		if (transformation.State == TransformationState.Wolf)
			onTakeDamageWolf.Invoke(delta);
		else if (transformation.State == TransformationState.Human)
			onTakeDamageHuman.Invoke(delta);

		if (health == 0)
			Die(damageSource);
	}

	public void Heal(float amount) {
		float delta = SetHealth(health + amount);
		if (delta == 0) return;
		onHeal.Invoke(delta);
	}

	public void Die(DamageSource damageSource = DamageSource.Generic) {
		health = 0;
		dead = true;
		onDie.Invoke(damageSource);
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

public enum DamageSource {
	Generic,
	Spike,
}
