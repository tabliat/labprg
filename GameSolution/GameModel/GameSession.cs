namespace GameModel;

/// <summary>
/// Performs calculations related to a game session using the provided model objects.
/// </summary>
public class GameSession
{
    private readonly Random _random = new();

    public GameSession(Player player, LevelConfiguration levelConfiguration)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        LevelConfiguration = levelConfiguration ?? throw new ArgumentNullException(nameof(levelConfiguration));
    }

    public Player Player { get; }

    public LevelConfiguration LevelConfiguration { get; }

    public int CalculateExpectedScore()
    {
        int totalReward = LevelConfiguration.CalculateTotalReward();
        double difficultyMultiplier = LevelConfiguration.Difficulty switch
        {
            DifficultyLevel.Easy => 0.75,
            DifficultyLevel.Normal => 1.0,
            DifficultyLevel.Hard => 1.5,
            _ => 1.0
        };

        double powerUpBonus = LevelConfiguration.AllowPowerUps ? 1.1 : 1.0;
        return (int)Math.Round(totalReward * difficultyMultiplier * powerUpBonus);
    }

    public bool SimulateStep()
    {
        if (Player.CurrentHealth <= 0)
        {
            return false;
        }

        int enemyPressure = LevelConfiguration.CalculateTotalHealth();
        if (enemyPressure <= 0)
        {
            return true;
        }

        double difficultyFactor = LevelConfiguration.Difficulty switch
        {
            DifficultyLevel.Easy => 0.5,
            DifficultyLevel.Normal => 1.0,
            DifficultyLevel.Hard => 1.5,
            _ => 1.0
        };

        double baseDamage = enemyPressure / 50.0;
        int damage = (int)Math.Round(baseDamage * difficultyFactor);
        damage = Math.Max(0, damage + _random.Next(-2, 3));
        Player.ApplyDamage(damage);

        if (Player.CurrentHealth > 0)
        {
            Player.AddScore(Math.Max(1, CalculateExpectedScore() / 10));
        }

        return Player.CurrentHealth > 0;
    }

    public void Reset()
    {
        Player.Reset();
    }
}
