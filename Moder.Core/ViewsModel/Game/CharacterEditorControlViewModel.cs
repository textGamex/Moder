using System.Diagnostics;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Moder.Core.Models;
using Moder.Core.Models.Game.Character;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources;
using Moder.Core.Services.GameResources.Base;
using Moder.Core.Services.GameResources.Modifiers;
using NLog;

namespace Moder.Core.ViewsModel.Game;

public sealed partial class CharacterEditorControlViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ushort Level { get; set; }

    [ObservableProperty]
    public partial ushort LevelMaxValue { get; set; } = 10;

    [ObservableProperty]
    public partial ushort AttackMaxValue { get; set; }

    [ObservableProperty]
    public partial ushort Attack { get; set; }

    [ObservableProperty]
    public partial ushort DefenseMaxValue { get; set; }

    [ObservableProperty]
    public partial ushort Defense { get; set; }

    [ObservableProperty]
    public partial ushort PlanningMaxValue { get; set; }

    [ObservableProperty]
    public partial ushort Planning { get; set; }

    [ObservableProperty]
    public partial ushort LogisticsMaxValue { get; set; }

    [ObservableProperty]
    public partial ushort Logistics { get; set; }

    [ObservableProperty]
    public partial ushort ManeuveringMaxValue { get; set; }

    [ObservableProperty]
    public partial ushort Maneuvering { get; set; }

    [ObservableProperty]
    public partial ushort CoordinationMaxValue { get; set; }

    [ObservableProperty]
    public partial ushort Coordination { get; set; }

    [ObservableProperty]
    public partial string GeneratedText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LocalizedName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ImageKey { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedCharacterFile { get; set; } = string.Empty;

    public InlineCollection LevelModifierDescription { get; } = [];

    public InlineCollection AttackModifierDescription { get; } = [];

    public InlineCollection DefenseModifierDescription { get; } = [];

    public InlineCollection PlanningModifierDescription { get; } = [];

    public InlineCollection LogisticsModifierDescription { get; } = [];

    public InlineCollection ManeuveringModifierDescription { get; } = [];

    public InlineCollection CoordinationModifierDescription { get; } = [];

    public IEnumerable<string> CharacterFiles =>
        Directory
            .GetFiles(CharactersFolder, "*.txt", SearchOption.TopDirectoryOnly)
            .Select(filePath => Path.GetFileName(filePath));

    private string CharactersFolder =>
        Path.Combine(_appSettingService.ModRootFolderPath, Keywords.Common, "characters");

    public CharacterTypeInfo[] CharactersType { get; } =
        [
            new("将军 (corps_commander)", "corps_commander"),
            new("陆军元帅 (field_marshal)", "field_marshal"),
            new("海军将领 (navy_leader)", "navy_leader")
        ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedNavy))]
    public partial CharacterTypeInfo SelectedCharacterType { get; set; }

    public bool IsSelectedNavy => SelectedCharacterType.Key == "navy_leader";

    private readonly AppSettingService _appSettingService;
    private readonly CharacterSkillService _characterSkillService;
    private readonly ModifierDisplayService _modifierDisplayService;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 是否已初始化, 防止在初始化期间多次调用生成人物方法
    /// </summary>
    private bool _isInitialized;
    private CharacterSkillType SelectedCharacterSkillType =>
        CharacterSkillType.FromCharacterType(SelectedCharacterType.Key);

    public CharacterEditorControlViewModel(
        AppSettingService appSettingService,
        CharacterSkillService characterSkillService,
        ModifierDisplayService modifierDisplayService
    )
    {
        _appSettingService = appSettingService;
        _characterSkillService = characterSkillService;
        _modifierDisplayService = modifierDisplayService;
        SelectedCharacterType = CharactersType[0];

        // TODO: 释放?
        _characterSkillService.OnResourceChanged += OnResourceChanged;
        SetSkillsMaxValue();
        
        InitializeSkillDefaultValue();
    }

    private void OnResourceChanged(object? sender, ResourceChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SetSkillsMaxValue();
            ResetSkillsModifierDescription();
        });
    }

    private void SetSkillsMaxValue()
    {
        var type = SelectedCharacterSkillType;
        LevelMaxValue = _characterSkillService.GetMaxSkillValue(SkillType.Level, type);
        AttackMaxValue = _characterSkillService.GetMaxSkillValue(SkillType.Attack, type);
        DefenseMaxValue = _characterSkillService.GetMaxSkillValue(SkillType.Defense, type);
        if (IsSelectedNavy)
        {
            ManeuveringMaxValue = _characterSkillService.GetMaxSkillValue(SkillType.Maneuvering, type);
            CoordinationMaxValue = _characterSkillService.GetMaxSkillValue(SkillType.Coordination, type);
            PlanningMaxValue = 1;
            LogisticsMaxValue = 1;
        }
        else
        {
            PlanningMaxValue = _characterSkillService.GetMaxSkillValue(SkillType.Planning, type);
            LogisticsMaxValue = _characterSkillService.GetMaxSkillValue(SkillType.Logistics, type);
            ManeuveringMaxValue = 1;
            CoordinationMaxValue = 1;
        }
    }

    private void ResetSkillsModifierDescription()
    {
        OnLevelChanged(Level);
        OnAttackChanged(Attack);
        OnDefenseChanged(Defense);
        OnPlanningChanged(Planning);
        OnCoordinationChanged(Coordination);
        OnLogisticsChanged(Logistics);
        OnManeuveringChanged(Maneuvering);
    }

    // BUG: 切换时描述会丢失
    partial void OnSelectedCharacterTypeChanged(CharacterTypeInfo value)
    {
        SetSkillsMaxValue();
        ResetSkillsModifierDescription();
    }

    partial void OnLevelChanged(ushort value)
    {
        AddModifierDescription(SkillType.Level, value, LevelModifierDescription);
        OnPropertyChanged(nameof(LevelModifierDescription));
    }

    partial void OnAttackChanged(ushort value)
    {
        AddModifierDescription(SkillType.Attack, value, AttackModifierDescription);
        OnPropertyChanged(nameof(AttackModifierDescription));
        OnPropertyChanged(nameof(AttackModifierDescription));
    }

    partial void OnDefenseChanged(ushort value)
    {
        AddModifierDescription(SkillType.Defense, value, DefenseModifierDescription);
        OnPropertyChanged(nameof(DefenseModifierDescription));
        OnPropertyChanged(nameof(DefenseModifierDescription));
    }

    partial void OnPlanningChanged(ushort value)
    {
        AddModifierDescription(SkillType.Planning, value, PlanningModifierDescription);
        OnPropertyChanged(nameof(PlanningModifierDescription));
    }

    partial void OnLogisticsChanged(ushort value)
    {
        AddModifierDescription(SkillType.Logistics, value, LogisticsModifierDescription);
        OnPropertyChanged(nameof(LogisticsModifierDescription));
    }

    partial void OnManeuveringChanged(ushort value)
    {
        if (!IsSelectedNavy)
        {
            return;
        }

        AddModifierDescription(SkillType.Maneuvering, value, ManeuveringModifierDescription);
        OnPropertyChanged(nameof(ManeuveringModifierDescription));
    }

    partial void OnCoordinationChanged(ushort value)
    {
        if (!IsSelectedNavy)
        {
            return;
        }
        
        AddModifierDescription(SkillType.Coordination, value, CoordinationModifierDescription);
        OnPropertyChanged(nameof(CoordinationModifierDescription));
    }

    private void AddModifierDescription(SkillType skillType, ushort value, InlineCollection? collection)
    {
        if (collection is null)
        {
            Log.Warn("技能: {Type} 修饰符 InlineCollection 未设置", skillType);
            return;
        }

        var descriptions = _modifierDisplayService.GetSkillModifierDescription(
            skillType,
            SelectedCharacterSkillType,
            value
        );

        collection.Clear();
        foreach (var description in descriptions)
        {
            collection.Add(description);
        }
    }

    private void InitializeSkillDefaultValue()
    {
        Level = 1;
        Attack = 1;
        Defense = 1;
        Planning = 1;
        Logistics = 1;
        Maneuvering = 1;
        Coordination = 1;

        _isInitialized = true;

        Debug.Assert(
            LevelModifierDescription is not null
                && AttackModifierDescription is not null
                && CoordinationModifierDescription is not null
        );
    }

    [RelayCommand]
    private void OpenTraitsSelectionWindow() { }

    [RelayCommand]
    private void Save() { }
}
