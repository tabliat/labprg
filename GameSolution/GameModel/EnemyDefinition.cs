namespace GameModel;

/// <summary>
/// Represents a type of enemy with its basic attributes.
/// </summary>
public class EnemyDefinition
{
    public EnemyDefinition(string name, int baseHealth, double speed, int reward)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Enemy name cannot be empty.", nameof(name));
        }

        if (baseHealth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseHealth), "Base health must be positive.");
        }

        if (speed <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be positive.");
        }

        if (reward < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reward), "Reward cannot be negative.");
        }

        Name = name.Trim();
        BaseHealth = baseHealth;
        Speed = speed;
        Reward = reward;
    }

    public string Name { get; }

    public int BaseHealth { get; }

    public double Speed { get; }

    public int Reward { get; }
}
