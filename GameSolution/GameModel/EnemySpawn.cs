namespace GameModel;

/// <summary>
/// Describes a group of enemies that appear together in a level.
/// </summary>
public class EnemySpawn
{
    private int _quantity;

    public EnemySpawn(EnemyDefinition definition, int quantity)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Quantity = quantity;
    }

    public EnemyDefinition Definition { get; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be positive.");
            }

            _quantity = value;
        }
    }

    public int CalculateTotalReward()
    {
        checked
        {
            return Quantity * Definition.Reward;
        }
    }

    public int CalculateTotalHealth()
    {
        checked
        {
            return Quantity * Definition.BaseHealth;
        }
    }
}
