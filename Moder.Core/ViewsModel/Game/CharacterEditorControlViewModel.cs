using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Moder.Core.Extensions;
using Moder.Core.Infrastructure;
using Moder.Core.Infrastructure.Parser;
using Moder.Core.Models;
using Moder.Core.Models.Game.Character;
using Moder.Core.Models.Vo;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources;
using Moder.Core.Services.GameResources.Base;
using Moder.Core.Services.GameResources.Modifiers;
using Moder.Core.Views.Game;
using Moder.Language.Strings;
using NLog;
using ParadoxPower.CSharpExtensions;
using ParadoxPower.Process;

namespace Moder.Core.ViewsModel.Game;

public sealed partial class CharacterEditorControlViewModel : ObservableValidator, IClosed
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

    [Required(
        ErrorMessageResourceType = typeof(Resource),
        ErrorMessageResourceName = "UIErrorMessage_Required"
    )]
    public string Name
    {
        get;
        set => SetProperty(ref field, value, true);
    } = string.Empty;

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

    public bool IsSelectedNavy => SelectedCharacterType.Keyword == "navy_leader";

    private IEnumerable<TraitVo> _selectedTraits = [];
    private readonly AppSettingService _appSettingService;
    private readonly CharacterSkillService _characterSkillService;
    private readonly ModifierDisplayService _modifierDisplayService;
    private readonly MessageBoxService _messageBoxService;
    private readonly AppResourcesService _appResourcesService;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 是否已初始化, 防止在初始化期间多次调用生成人物方法
    /// </summary>
    private bool _isInitialized;

    private SkillCharacterType SelectedSkillCharacterType =>
        SkillCharacterType.FromCharacterType(SelectedCharacterType.Keyword);

    public CharacterEditorControlViewModel(
        AppSettingService appSettingService,
        CharacterSkillService characterSkillService,
        ModifierDisplayService modifierDisplayService,
        MessageBoxService messageBoxService,
        AppResourcesService appResourcesService
    )
    {
        _appSettingService = appSettingService;
        _characterSkillService = characterSkillService;
        _modifierDisplayService = modifierDisplayService;
        _messageBoxService = messageBoxService;
        _appResourcesService = appResourcesService;
        SelectedCharacterType = CharactersType[0];

        _characterSkillService.OnResourceChanged += OnResourceChanged;
        SetSkillsMaxValue();

        InitializeSkillDefaultValue();
        _isInitialized = true;
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
        var type = SelectedSkillCharacterType;
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

    //BUG: 切换时描述会丢失
    // 等框架修复或者换个解决方案
    partial void OnSelectedCharacterTypeChanged(CharacterTypeInfo value)
    {
        _appResourcesService.CurrentSelectedCharacterType = SelectedSkillCharacterType;
        // 这样描述可以正常显示
        Dispatcher.UIThread.Post(() =>
        {
            SetSkillsMaxValue();
            ResetSkillsModifierDescription();
        });
    }

    partial void OnLevelChanged(ushort value)
    {
        AddModifierDescription(SkillType.Level, value, LevelModifierDescription);
    }

    partial void OnAttackChanged(ushort value)
    {
        AddModifierDescription(SkillType.Attack, value, AttackModifierDescription);
    }

    partial void OnDefenseChanged(ushort value)
    {
        AddModifierDescription(SkillType.Defense, value, DefenseModifierDescription);
    }

    partial void OnPlanningChanged(ushort value)
    {
        AddModifierDescription(SkillType.Planning, value, PlanningModifierDescription);
    }

    partial void OnLogisticsChanged(ushort value)
    {
        AddModifierDescription(SkillType.Logistics, value, LogisticsModifierDescription);
    }

    partial void OnManeuveringChanged(ushort value)
    {
        if (!IsSelectedNavy)
        {
            return;
        }

        AddModifierDescription(SkillType.Maneuvering, value, ManeuveringModifierDescription);
    }

    partial void OnCoordinationChanged(ushort value)
    {
        if (!IsSelectedNavy)
        {
            return;
        }

        AddModifierDescription(SkillType.Coordination, value, CoordinationModifierDescription);
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
            SelectedSkillCharacterType,
            value
        );

        collection.Clear();
        collection.AddRange(descriptions);
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
    private async Task OpenTraitsSelectionWindow()
    {
        var window = new TraitSelectionWindowView();
        window.SyncSelectedTraits(_selectedTraits);
        var lifetime = (IClassicDesktopStyleApplicationLifetime?)App.Current.ApplicationLifetime;
        Debug.Assert(lifetime?.MainWindow is not null);

        await window.ShowDialog(lifetime.MainWindow);
        _selectedTraits = window.SelectedTraits;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (string.IsNullOrEmpty(SelectedCharacterFile))
        {
            await _messageBoxService.WarnAsync(Resource.CharacterEditor_MissingCharacterFileNameTip);
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            await _messageBoxService.WarnAsync(Resource.UIErrorMessage_MissingRequiredInfoTip);
            return;
        }

        if (string.IsNullOrEmpty(LocalizedName))
        {
            LocalizedName = Name;
        }

        var filePath = Path.Combine(CharactersFolder, SelectedCharacterFile);
        Node? charactersNode = null;
        if (File.Exists(filePath))
        {
            if (TextParser.TryParse(filePath, out var node, out var error))
            {
                var child = Array.Find(
                    node.AllArray,
                    child =>
                        child.IsNodeChild
                        && StringComparer.OrdinalIgnoreCase.Equals(child.node.Key, "characters")
                );
                // 如果未找到的话, node 就是 null
                charactersNode = child.node;
            }
            else
            {
                await _messageBoxService.ErrorAsync(
                    string.Format(
                        Resource.CharacterEditor_ParserErrorInfo,
                        filePath,
                        error.ErrorMessage,
                        error.Line,
                        error.Column
                    )
                );
                return;
            }
        }

        // 如果文件不存在或者在用户选择的文件下没有找到 characters 节点，则新建节点
        charactersNode ??= new Node("characters");

        charactersNode.AddChild(GetGeneratedCharacterNode());
        await File.WriteAllTextAsync(filePath, charactersNode.PrintRaw());
        Log.Info("保存成功");
    }

    private Node GetGeneratedCharacterNode()
    {
        var newCharacterNode = Node.Create(Name);
        newCharacterNode.AddChild(ChildHelper.LeafString("name", LocalizedName));

        AddCharacterImage(newCharacterNode);

        var characterTypeNode = newCharacterNode.AddNodeChild(SelectedCharacterType.Keyword);
        characterTypeNode.AllArray = GetCharacterSkills();

        AddTraits(characterTypeNode);

        return newCharacterNode;
    }

    private void AddTraits(Node characterTypeNode)
    {
        if (_selectedTraits.Any())
        {
            var traitsNode = characterTypeNode.AddNodeChild("traits");
            traitsNode.AllArray = _selectedTraits
                .Select(trait => ChildHelper.LeafValue(trait.Name))
                .ToArray();
        }
    }

    private void AddCharacterImage(Node newCharacterNode)
    {
        if (string.IsNullOrEmpty(ImageKey))
        {
            return;
        }

        var portraitsNode = newCharacterNode.AddNodeChild("portraits");
        var node = portraitsNode.AddNodeChild(IsSelectedNavy ? "navy" : "army");
        node.AddLeafString("large", ImageKey);
    }

    private Child[] GetCharacterSkills()
    {
        var array = new Child[5];
        array[0] = ChildHelper.Leaf("level", Level);
        array[1] = ChildHelper.Leaf("attack_skill", Attack);
        array[2] = ChildHelper.Leaf("defense_skill", Defense);

        if (IsSelectedNavy)
        {
            array[3] = ChildHelper.Leaf("maneuvering_skill", Maneuvering);
            array[4] = ChildHelper.Leaf("coordination_skill", Coordination);
        }
        else
        {
            array[3] = ChildHelper.Leaf("planning_skill", Planning);
            array[4] = ChildHelper.Leaf("logistics_skill", Logistics);
        }

        return array;
    }

    public void Close()
    {
        _characterSkillService.OnResourceChanged -= OnResourceChanged;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(GeneratedText) && _isInitialized)
        {
            GeneratedText = GetGeneratedText();
        }

        base.OnPropertyChanged(e);
    }

    private string GetGeneratedText()
    {
        if (string.IsNullOrEmpty(LocalizedName))
        {
            LocalizedName = Name;
        }

        return GetGeneratedCharacterNode().PrintRaw();
    }
}
