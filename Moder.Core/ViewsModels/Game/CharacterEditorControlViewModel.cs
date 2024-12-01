using System.ComponentModel;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Moder.Core.Extensions;
using Moder.Core.Models.Character;
using Moder.Core.Models.Vo;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources;
using Moder.Core.Services.GameResources.Base;
using Moder.Core.Views.Game;
using Moder.Language.Strings;
using NLog;
using ParadoxPower.CSharpExtensions;
using ParadoxPower.Process;

namespace Moder.Core.ViewsModels.Game;

public sealed partial class CharacterEditorControlViewModel : ObservableObject
{
    public ComboBoxItem[] CharactersType { get; } =
        [
            new() { Content = "将军 (corps_commander)", Tag = "corps_commander" },
            new() { Content = "陆军元帅 (field_marshal)", Tag = "field_marshal" },
            new() { Content = "海军将领 (navy_leader)", Tag = "navy_leader" }
        ];

    public IEnumerable<string> CharacterFiles =>
        Directory
            .GetFiles(CharactersFolder, "*.txt", SearchOption.TopDirectoryOnly)
            .Select(filePath => Path.GetFileName(filePath));

    private string CharactersFolder =>
        Path.Combine(_globalSettingService.ModRootFolderPath, Keywords.Common, "characters");

    [ObservableProperty]
    private string _generatedText = string.Empty;

    [ObservableProperty]
    private string _selectedCharacterFile = string.Empty;

    [ObservableProperty]
    private ushort _levelMaxValue;

    public InlineCollection LevelModifierDescription { get; set; } = null!;

    [ObservableProperty]
    private ushort _level;

    [ObservableProperty]
    private ushort _attackMaxValue;

    public InlineCollection AttackModifierDescription { get; set; } = null!;

    [ObservableProperty]
    private ushort _attack;

    [ObservableProperty]
    private ushort _defenseMaxValue;

    public InlineCollection DefenseModifierDescription { get; set; } = null!;

    [ObservableProperty]
    private ushort _defense;

    [ObservableProperty]
    private ushort _planningMaxValue;

    public InlineCollection PlanningModifierDescription { get; set; } = null!;

    [ObservableProperty]
    private ushort _planning;

    [ObservableProperty]
    private ushort _logisticsMaxValue;

    public InlineCollection LogisticsModifierDescription { get; set; } = null!;

    [ObservableProperty]
    private ushort _logistics;

    [ObservableProperty]
    private ushort _maneuveringMaxValue;

    public InlineCollection ManeuveringModifierDescription { get; set; } = null!;

    [ObservableProperty]
    private ushort _maneuvering;

    [ObservableProperty]
    private ushort _coordinationMaxValue;

    public InlineCollection CoordinationModifierDescription { get; set; } = null!;

    [ObservableProperty]
    private ushort _coordination;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _localizedName = string.Empty;

    [ObservableProperty]
    private int _selectedCharacterTypeIndex;

    [ObservableProperty]
    private string _imageKey = string.Empty;
    private IEnumerable<TraitVo> _selectedTraits = [];

    private bool IsSelectedNavy => SelectedCharacterTypeCode == "navy_leader";
    private string SelectedCharacterTypeCode =>
        CharactersType[SelectedCharacterTypeIndex].Tag.ToString()
        ?? throw new InvalidOperationException("未设置 CharactersType 的 null");
    private CharacterSkillType SelectedCharacterSkillType =>
        CharacterSkillType.FromCharacterType(SelectedCharacterTypeCode);

    private readonly GlobalSettingService _globalSettingService;
    private readonly MessageBoxService _messageBoxService;
    private readonly CharacterSkillService _characterSkillService;
    private readonly GlobalResourceService _globalResourceService;
    /// <summary>
    /// 是否已初始化, 防止在初始化期间多次调用生成人物方法
    /// </summary>
    private bool _isInitialized;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public CharacterEditorControlViewModel(
        GlobalSettingService globalSettingService,
        MessageBoxService messageBoxService,
        CharacterSkillService characterSkillService,
        GlobalResourceService globalResourceService
    )
    {
        _globalSettingService = globalSettingService;
        _messageBoxService = messageBoxService;
        _characterSkillService = characterSkillService;
        _globalResourceService = globalResourceService;

        SetSkillsMaxValue();
        _characterSkillService.OnResourceChanged += OnResourceChanged;
    }

    private void OnResourceChanged(object? sender, ResourceChangedEventArgs e)
    {
        App.Current.DispatcherQueue.TryEnqueue(() =>
        {
            SetSkillsMaxValue();
            ResetSkillsModifierDescription();
        });
    }

    public void InitializeSkillDefaultValue()
    {
        Level = 1;
        Attack = 1;
        Defense = 1;
        Planning = 1;
        Logistics = 1;
        Maneuvering = 1;
        Coordination = 1;

        _isInitialized = true;
    }

    partial void OnLevelChanged(ushort value)
    {
        var description = _characterSkillService.GetSkillModifierDescription(
            SkillType.Level,
            SelectedCharacterSkillType,
            value
        );

        AddModifierDescription(LevelModifierDescription, description);
    }

    partial void OnAttackChanged(ushort value)
    {
        var description = _characterSkillService.GetSkillModifierDescription(
            SkillType.Attack,
            SelectedCharacterSkillType,
            value
        );

        AddModifierDescription(AttackModifierDescription, description);
    }

    partial void OnDefenseChanged(ushort value)
    {
        var description = _characterSkillService.GetSkillModifierDescription(
            SkillType.Defense,
            SelectedCharacterSkillType,
            value
        );

        AddModifierDescription(DefenseModifierDescription, description);
    }

    partial void OnPlanningChanged(ushort value)
    {
        var description = _characterSkillService.GetSkillModifierDescription(
            SkillType.Planning,
            SelectedCharacterSkillType,
            value
        );

        AddModifierDescription(PlanningModifierDescription, description);
    }

    partial void OnLogisticsChanged(ushort value)
    {
        var description = _characterSkillService.GetSkillModifierDescription(
            SkillType.Logistics,
            SelectedCharacterSkillType,
            value
        );

        AddModifierDescription(LogisticsModifierDescription, description);
    }

    partial void OnManeuveringChanged(ushort value)
    {
        if (!IsSelectedNavy)
        {
            return;
        }

        var description = _characterSkillService.GetSkillModifierDescription(
            SkillType.Maneuvering,
            SelectedCharacterSkillType,
            value
        );

        AddModifierDescription(ManeuveringModifierDescription, description);
    }

    partial void OnCoordinationChanged(ushort value)
    {
        if (!IsSelectedNavy)
        {
            return;
        }

        var description = _characterSkillService.GetSkillModifierDescription(
            SkillType.Coordination,
            SelectedCharacterSkillType,
            value
        );

        AddModifierDescription(CoordinationModifierDescription, description);
    }

    private static void AddModifierDescription(InlineCollection collection, IEnumerable<Inline> inlines)
    {
        collection.Clear();
        try
        {
            foreach (var inline in inlines)
            {
                collection.Add(inline);
            }
        }
        catch (COMException e)
        {
            Log.Error(e);
        }
    }

    partial void OnSelectedCharacterTypeIndexChanged(int value)
    {
        SetSkillsMaxValue();
        ResetSkillsModifierDescription();
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

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(SelectedCharacterFile))
        {
            await _messageBoxService.WarnAsync(Resource.CharacterEditor_MissingCharacterFileNameTip);
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            await _messageBoxService.WarnAsync(Resource.CharacterEditor_MissingRequiredInfoTip);
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

        var characterTypeNode = newCharacterNode.AddNodeChild(SelectedCharacterTypeCode);
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

    [RelayCommand]
    private async Task OpenTraitsSelectionWindow()
    {
        _globalResourceService.CurrentSelectSelectSkillType = SelectedCharacterSkillType;

        using var window = new TraitsSelectionWindowView();
        var dialog = new ContentDialog
        {
            XamlRoot = App.Current.XamlRoot,
            Content = window,
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = Resource.Common_Ok,
            CloseButtonText = Resource.Common_Close
        };

        window.ViewModel.SyncSelectedTraits(_selectedTraits);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var selectedTraits = window.ViewModel.Traits.Cast<TraitVo>().Where(trait => trait.IsSelected);
            _selectedTraits = selectedTraits;
            OnPropertyChanged(nameof(_selectedTraits));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(GeneratedText) && _isInitialized)
        {
            GeneratedText = GetGeneratedText();
        }
    }

    private string GetGeneratedText()
    {
        if (string.IsNullOrEmpty(LocalizedName))
        {
            LocalizedName = Name;
        }

        return GetGeneratedCharacterNode().PrintRaw();
    }

    public void Close()
    {
        _characterSkillService.OnResourceChanged -= OnResourceChanged;
    }
}
