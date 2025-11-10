using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GameModel;

namespace GameApp;

public partial class MainForm : Form
{
    private readonly Dictionary<string, EnemyDefinition> _enemyDefinitions;
    private readonly BindingList<EnemySpawnRow> _spawnRows = new();
    private LevelConfiguration _levelConfiguration;

    private TextBox _txtPlayerName = null!;
    private TextBox _txtPlayerHealth = null!;
    private TextBox _txtPlayerSpeed = null!;
    private ComboBox _cmbEnemyType = null!;
    private TextBox _txtEnemyQuantity = null!;
    private Button _btnAddEnemy = null!;
    private Button _btnRemoveEnemy = null!;
    private DataGridView _dgvEnemySpawns = null!;
    private RadioButton _rdoEasy = null!;
    private RadioButton _rdoNormal = null!;
    private RadioButton _rdoHard = null!;
    private CheckBox _chkPowerUps = null!;
    private Button _btnSimulate = null!;
    private ListBox _lstSimulationLog = null!;
    private Label _lblTotalHealth = null!;
    private Label _lblTotalReward = null!;
    private Label _lblExpectedScore = null!;

    public MainForm()
    {
        InitializeComponent();
        _enemyDefinitions = CreateEnemyDefinitions();
        _cmbEnemyType.Items.AddRange(_enemyDefinitions.Keys.ToArray());
        if (_cmbEnemyType.Items.Count > 0)
        {
            _cmbEnemyType.SelectedIndex = 0;
        }

        _levelConfiguration = new LevelConfiguration(DifficultyLevel.Normal, allowPowerUps: false);
        _dgvEnemySpawns.DataSource = _spawnRows;
        _dgvEnemySpawns.AutoGenerateColumns = false;
        ConfigureGridColumns();
        UpdateSummaryLabels();
    }

    private void ConfigureGridColumns()
    {
        _dgvEnemySpawns.Columns.Clear();
        _dgvEnemySpawns.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(EnemySpawnRow.EnemyName),
            HeaderText = "Enemy",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _dgvEnemySpawns.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(EnemySpawnRow.Quantity),
            HeaderText = "Quantity",
            Width = 80,
            ReadOnly = true
        });
        _dgvEnemySpawns.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(EnemySpawnRow.TotalHealth),
            HeaderText = "Total Health",
            Width = 120,
            ReadOnly = true
        });
        _dgvEnemySpawns.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(EnemySpawnRow.TotalReward),
            HeaderText = "Total Reward",
            Width = 120,
            ReadOnly = true
        });
    }

    private static Dictionary<string, EnemyDefinition> CreateEnemyDefinitions()
    {
        return new Dictionary<string, EnemyDefinition>
        {
            ["Scout"] = new EnemyDefinition("Scout", baseHealth: 30, speed: 2.0, reward: 15),
            ["Raider"] = new EnemyDefinition("Raider", baseHealth: 50, speed: 1.5, reward: 25),
            ["Bruiser"] = new EnemyDefinition("Bruiser", baseHealth: 90, speed: 1.0, reward: 40),
            ["Boss"] = new EnemyDefinition("Boss", baseHealth: 150, speed: 0.6, reward: 120)
        };
    }

    private void OnAddEnemy(object? sender, EventArgs e)
    {
        if (_cmbEnemyType.SelectedItem is null)
        {
            MessageBox.Show(this, "Please select an enemy type.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(_txtEnemyQuantity.Text, out int quantity) || quantity <= 0)
        {
            MessageBox.Show(this, "Enter a positive number of enemies.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string key = _cmbEnemyType.SelectedItem.ToString()!;
        EnemyDefinition definition = _enemyDefinitions[key];
        var spawn = new EnemySpawn(definition, quantity);
        _levelConfiguration.AddSpawn(spawn);
        _spawnRows.Add(new EnemySpawnRow(spawn));
        UpdateSummaryLabels();
        _txtEnemyQuantity.Clear();
        _txtEnemyQuantity.Focus();
    }

    private void OnRemoveEnemy(object? sender, EventArgs e)
    {
        if (_dgvEnemySpawns.CurrentRow is null)
        {
            MessageBox.Show(this, "Select a spawn to remove.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int index = _dgvEnemySpawns.CurrentRow.Index;
        if (index >= 0 && index < _spawnRows.Count)
        {
            _spawnRows.RemoveAt(index);
            _levelConfiguration.RemoveSpawnAt(index);
            UpdateSummaryLabels();
        }
    }

    private void OnSimulate(object? sender, EventArgs e)
    {
        try
        {
            Player player = BuildPlayerFromInputs();
            ApplyDifficultySettings();

            if (!_levelConfiguration.EnemySpawns.Any())
            {
                MessageBox.Show(this, "Add at least one enemy group before simulating.", "No Enemies", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var session = new GameSession(player, _levelConfiguration);
            _lstSimulationLog.Items.Clear();
            _lstSimulationLog.Items.Add($"Expected score: {session.CalculateExpectedScore()}");

            for (int tick = 1; tick <= 5; tick++)
            {
                bool alive = session.SimulateStep();
                _lstSimulationLog.Items.Add($"Step {tick}: Player health {player.CurrentHealth}, score {player.Score}");
                if (!alive)
                {
                    _lstSimulationLog.Items.Add("Player has been defeated.");
                    break;
                }
            }

            if (player.CurrentHealth > 0)
            {
                _lstSimulationLog.Items.Add("Simulation complete! Player survived the encounter.");
            }

            UpdateSummaryLabels(session);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Simulation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnDifficultyChanged(object? sender, EventArgs e)
    {
        UpdateSummaryLabels();
    }

    private Player BuildPlayerFromInputs()
    {
        string name = _txtPlayerName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Player name is required.");
        }

        if (!int.TryParse(_txtPlayerHealth.Text, out int health) || health <= 0)
        {
            throw new InvalidOperationException("Player health must be a positive integer.");
        }

        if (!double.TryParse(_txtPlayerSpeed.Text, out double speed) || speed <= 0)
        {
            throw new InvalidOperationException("Player speed must be a positive number.");
        }

        return new Player(name, health, speed);
    }

    private void ApplyDifficultySettings()
    {
        if (_rdoEasy.Checked)
        {
            _levelConfiguration.Difficulty = DifficultyLevel.Easy;
        }
        else if (_rdoHard.Checked)
        {
            _levelConfiguration.Difficulty = DifficultyLevel.Hard;
        }
        else
        {
            _levelConfiguration.Difficulty = DifficultyLevel.Normal;
        }

        _levelConfiguration.AllowPowerUps = _chkPowerUps.Checked;
    }

    private void UpdateSummaryLabels(GameSession? session = null)
    {
        ApplyDifficultySettings();

        _lblTotalHealth.Text = $"Total Enemy Health: {_levelConfiguration.CalculateTotalHealth()}";
        _lblTotalReward.Text = $"Total Enemy Reward: {_levelConfiguration.CalculateTotalReward()}";
        int expectedScore = 0;
        if (_levelConfiguration.EnemySpawns.Any())
        {
            GameSession scoreSession = session ?? new GameSession(CreatePreviewPlayer(), _levelConfiguration);
            expectedScore = scoreSession.CalculateExpectedScore();
        }
        _lblExpectedScore.Text = $"Estimated Score: {expectedScore}";
    }

    private Player CreatePreviewPlayer()
    {
        string name = string.IsNullOrWhiteSpace(_txtPlayerName.Text) ? "Preview" : _txtPlayerName.Text.Trim();
        int health = int.TryParse(_txtPlayerHealth.Text, out int parsedHealth) && parsedHealth > 0 ? parsedHealth : 100;
        double speed = double.TryParse(_txtPlayerSpeed.Text, out double parsedSpeed) && parsedSpeed > 0 ? parsedSpeed : 1.0;
        return new Player(name, health, speed);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Arcade Skirmish Configurator";
        ClientSize = new Size(960, 640);
        MinimumSize = new Size(960, 640);

        var playerGroup = new GroupBox
        {
            Text = "Player Settings",
            Location = new Point(12, 12),
            Size = new Size(300, 180)
        };
        Controls.Add(playerGroup);

        var lblPlayerName = new Label
        {
            Text = "Name:",
            Location = new Point(12, 30),
            AutoSize = true
        };
        playerGroup.Controls.Add(lblPlayerName);

        _txtPlayerName = new TextBox
        {
            Location = new Point(100, 26),
            Width = 180
        };
        playerGroup.Controls.Add(_txtPlayerName);

        var lblPlayerHealth = new Label
        {
            Text = "Health:",
            Location = new Point(12, 70),
            AutoSize = true
        };
        playerGroup.Controls.Add(lblPlayerHealth);

        _txtPlayerHealth = new TextBox
        {
            Location = new Point(100, 66),
            Width = 180
        };
        playerGroup.Controls.Add(_txtPlayerHealth);

        var lblPlayerSpeed = new Label
        {
            Text = "Speed:",
            Location = new Point(12, 110),
            AutoSize = true
        };
        playerGroup.Controls.Add(lblPlayerSpeed);

        _txtPlayerSpeed = new TextBox
        {
            Location = new Point(100, 106),
            Width = 180
        };
        playerGroup.Controls.Add(_txtPlayerSpeed);

        _chkPowerUps = new CheckBox
        {
            Text = "Enable power ups",
            Location = new Point(100, 140),
            AutoSize = true
        };
        _chkPowerUps.CheckedChanged += OnDifficultyChanged;
        playerGroup.Controls.Add(_chkPowerUps);

        var difficultyGroup = new GroupBox
        {
            Text = "Difficulty",
            Location = new Point(330, 12),
            Size = new Size(200, 180)
        };
        Controls.Add(difficultyGroup);

        _rdoEasy = new RadioButton
        {
            Text = "Easy",
            Location = new Point(16, 30),
            AutoSize = true
        };
        _rdoEasy.CheckedChanged += OnDifficultyChanged;
        difficultyGroup.Controls.Add(_rdoEasy);

        _rdoNormal = new RadioButton
        {
            Text = "Normal",
            Location = new Point(16, 70),
            AutoSize = true,
            Checked = true
        };
        _rdoNormal.CheckedChanged += OnDifficultyChanged;
        difficultyGroup.Controls.Add(_rdoNormal);

        _rdoHard = new RadioButton
        {
            Text = "Hard",
            Location = new Point(16, 110),
            AutoSize = true
        };
        _rdoHard.CheckedChanged += OnDifficultyChanged;
        difficultyGroup.Controls.Add(_rdoHard);

        var enemyGroup = new GroupBox
        {
            Text = "Enemy Waves",
            Location = new Point(550, 12),
            Size = new Size(390, 180)
        };
        Controls.Add(enemyGroup);

        var lblEnemyType = new Label
        {
            Text = "Enemy type:",
            Location = new Point(16, 30),
            AutoSize = true
        };
        enemyGroup.Controls.Add(lblEnemyType);

        _cmbEnemyType = new ComboBox
        {
            Location = new Point(120, 26),
            Width = 240,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        enemyGroup.Controls.Add(_cmbEnemyType);

        var lblEnemyQuantity = new Label
        {
            Text = "Quantity:",
            Location = new Point(16, 70),
            AutoSize = true
        };
        enemyGroup.Controls.Add(lblEnemyQuantity);

        _txtEnemyQuantity = new TextBox
        {
            Location = new Point(120, 66),
            Width = 80
        };
        enemyGroup.Controls.Add(_txtEnemyQuantity);

        _btnAddEnemy = new Button
        {
            Text = "Add wave",
            Location = new Point(220, 64),
            Size = new Size(140, 28)
        };
        _btnAddEnemy.Click += OnAddEnemy;
        enemyGroup.Controls.Add(_btnAddEnemy);

        _btnRemoveEnemy = new Button
        {
            Text = "Remove selected",
            Location = new Point(120, 110),
            Size = new Size(240, 28)
        };
        _btnRemoveEnemy.Click += OnRemoveEnemy;
        enemyGroup.Controls.Add(_btnRemoveEnemy);

        _dgvEnemySpawns = new DataGridView
        {
            Location = new Point(12, 210),
            Size = new Size(928, 220),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };
        Controls.Add(_dgvEnemySpawns);

        _lblTotalHealth = new Label
        {
            Location = new Point(12, 440),
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        };
        Controls.Add(_lblTotalHealth);

        _lblTotalReward = new Label
        {
            Location = new Point(12, 470),
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        };
        Controls.Add(_lblTotalReward);

        _lblExpectedScore = new Label
        {
            Location = new Point(12, 500),
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        };
        Controls.Add(_lblExpectedScore);

        _btnSimulate = new Button
        {
            Text = "Run simulation",
            Location = new Point(12, 540),
            Size = new Size(200, 36)
        };
        _btnSimulate.Click += OnSimulate;
        Controls.Add(_btnSimulate);

        _lstSimulationLog = new ListBox
        {
            Location = new Point(230, 440),
            Size = new Size(710, 136)
        };
        Controls.Add(_lstSimulationLog);

        ResumeLayout(false);
    }

    private sealed class EnemySpawnRow
    {
        public EnemySpawnRow(EnemySpawn spawn)
        {
            Spawn = spawn;
            EnemyName = spawn.Definition.Name;
            Quantity = spawn.Quantity;
            TotalHealth = spawn.CalculateTotalHealth();
            TotalReward = spawn.CalculateTotalReward();
        }

        public EnemySpawn Spawn { get; }
        public string EnemyName { get; }
        public int Quantity { get; }
        public int TotalHealth { get; }
        public int TotalReward { get; }
    }
}
