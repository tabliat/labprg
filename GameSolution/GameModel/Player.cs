namespace GameModel;

/// <summary>
/// Describes the player participating in the game session.
/// </summary>
public class Player
{
    private string _name;
    private int _maxHealth;
    private int _currentHealth;
    private double _speed;

    public Player(string name, int maxHealth, double speed)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Player name cannot be empty.", nameof(name));
        }

        if (maxHealth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHealth), "Health must be greater than zero.");
        }

        if (speed <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be greater than zero.");
        }

        _name = name.Trim();
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        _speed = speed;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Player name cannot be empty.", nameof(value));
            }

            _name = value.Trim();
        }
    }

    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Max health must be positive.");
            }

            _maxHealth = value;
            _currentHealth = Math.Min(_currentHealth, _maxHealth);
        }
    }

    public int CurrentHealth => _currentHealth;

    public double Speed
    {
        get => _speed;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Speed must be positive.");
            }

            _speed = value;
        }
    }

    public int Score { get; private set; }

    public void ApplyDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative.");
        }

        _currentHealth = Math.Max(0, _currentHealth - damage);
    }

    public void Heal(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal amount cannot be negative.");
        }

        _currentHealth = Math.Min(_maxHealth, _currentHealth + amount);
    }

    public void Reset()
    {
        _currentHealth = _maxHealth;
        Score = 0;
    }

    public void AddScore(int points)
    {
        if (points < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(points), "Points cannot be negative.");
        }

        checked
        {
            Score += points;
        }
    }
}
