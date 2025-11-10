using System.Collections.ObjectModel;
using System.Linq;

namespace GameModel;

/// <summary>
/// Represents the configuration of a level, including difficulty and enemies.
/// </summary>
public class LevelConfiguration
{
    private readonly ObservableCollection<EnemySpawn> _enemySpawns = new();
    private readonly ReadOnlyObservableCollection<EnemySpawn> _readOnlyEnemySpawns;
    private DifficultyLevel _difficulty;

    public LevelConfiguration(DifficultyLevel difficulty, bool allowPowerUps)
    {
        _readOnlyEnemySpawns = new ReadOnlyObservableCollection<EnemySpawn>(_enemySpawns);
        Difficulty = difficulty;
        AllowPowerUps = allowPowerUps;
    }

    public DifficultyLevel Difficulty
    {
        get => _difficulty;
        set
        {
            if (!Enum.IsDefined(typeof(DifficultyLevel), value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Invalid difficulty value.");
            }

            _difficulty = value;
        }
    }

    public bool AllowPowerUps { get; set; }

    public ReadOnlyObservableCollection<EnemySpawn> EnemySpawns => _readOnlyEnemySpawns;

    public void AddSpawn(EnemySpawn spawn)
    {
        if (spawn is null)
        {
            throw new ArgumentNullException(nameof(spawn));
        }

        _enemySpawns.Add(spawn);
    }

    public void RemoveSpawnAt(int index)
    {
        if (index < 0 || index >= _enemySpawns.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        _enemySpawns.RemoveAt(index);
    }

    public void ClearSpawns() => _enemySpawns.Clear();

    public int CalculateTotalReward() => _enemySpawns.Sum(spawn => spawn.CalculateTotalReward());

    public int CalculateTotalHealth() => _enemySpawns.Sum(spawn => spawn.CalculateTotalHealth());
}
