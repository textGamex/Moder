using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Extensions;
using Moder.Core.Models.Character;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources;
using Moder.Core.Services.GameResources.Base;
using NLog;
using ParadoxPower.CSharp;
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
    private string _selectedCharacterFile = string.Empty;

    [ObservableProperty]
    private ushort _levelMaxValue;

    [ObservableProperty]
    private ushort _level = 1;

    [ObservableProperty]
    private ushort _attackMaxValue;

    [ObservableProperty]
    private ushort _attack = 1;

    [ObservableProperty]
    private ushort _defenseMaxValue;

    [ObservableProperty]
    private ushort _defense = 1;

    [ObservableProperty]
    private ushort _planningMaxValue;

    [ObservableProperty]
    private ushort _planning = 1;

    [ObservableProperty]
    private ushort _logisticsMaxValue;

    [ObservableProperty]
    private ushort _logistics = 1;

    [ObservableProperty]
    private ushort _maneuveringMaxValue;

    [ObservableProperty]
    private ushort _maneuvering = 1;

    [ObservableProperty]
    private ushort _coordinationMaxValue;

    [ObservableProperty]
    private ushort _coordination = 1;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _localizedName = string.Empty;

    [ObservableProperty]
    private int _selectedCharacterTypeIndex;

    private bool IsSelectedNavy => SelectedCharacterTypeCode == "navy_leader";
    private string SelectedCharacterTypeCode =>
        CharactersType[SelectedCharacterTypeIndex].Tag.ToString()
        ?? throw new InvalidOperationException("未设置 CharactersType 的 null");
    private readonly GlobalSettingService _globalSettingService;
    private readonly MessageBoxService _messageBoxService;
    private readonly CharacterSkillService _characterSkillService;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public CharacterEditorControlViewModel(
        GlobalSettingService globalSettingService,
        MessageBoxService messageBoxService,
        CharacterSkillService characterSkillService
    )
    {
        _globalSettingService = globalSettingService;
        _messageBoxService = messageBoxService;
        _characterSkillService = characterSkillService;

        SetSkillsMaxValue();
        _characterSkillService.OnResourceChanged += (_, _) =>
            App.Current.DispatcherQueue.TryEnqueue(SetSkillsMaxValue);
    }

    private void SetSkillsMaxValue()
    {
        var type = CharacterSkillType.FromCharacterType(SelectedCharacterTypeCode);
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

    partial void OnSelectedCharacterTypeIndexChanged(int value)
    {
        SetSkillsMaxValue();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(SelectedCharacterFile))
        {
            await _messageBoxService.WarnAsync("请填写写入文件名");
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            await _messageBoxService.WarnAsync("请填写必须的字段");
            return;
        }

        if (string.IsNullOrEmpty(LocalizedName))
        {
            LocalizedName = Name;
        }

        var filePath = Path.Combine(CharactersFolder, SelectedCharacterFile);
        Node? rootNode = null;
        Node? charactersNode = null;
        if (File.Exists(filePath))
        {
            if (TextParser.TryParse(filePath, out var node, out var error))
            {
                rootNode = node;
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
                    $"解析 '{filePath}' 文件失败, 错误原因: {error.ErrorMessage}, 行数: {error.Line}, 列数: {error.Column}"
                );
                return;
            }
        }

        // 如果文件不存在或者在用户选择的文件下没有找到 characters 节点，则新建节点
        charactersNode ??= new Node("characters");
        rootNode ??= charactersNode;

        var newCharacterNode = charactersNode.AddNodeChild(Name);
        newCharacterNode.AddChild(ChildHelper.LeafString("name", LocalizedName));

        var type = newCharacterNode.AddNodeChild(SelectedCharacterTypeCode);
        type.AllArray = GetCharacterSkills();

        await File.WriteAllTextAsync(filePath, rootNode.PrintChildren());
        Log.Info("保存成功");
    }

    private Child[] GetCharacterSkills()
    {
        var array = new Child[5];
        array[0] = ChildHelper.Leaf("level", Level);
        array[1] = ChildHelper.Leaf("attack_skill", Attack);
        array[2] = ChildHelper.Leaf("defense_skill", Defense);

        if (IsSelectedNavy)
        {
            array[3] = ChildHelper.Leaf("planning_skill", Planning);
            array[4] = ChildHelper.Leaf("logistics_skill", Logistics);
        }
        else
        {
            array[3] = ChildHelper.Leaf("maneuvering_skill", Maneuvering);
            array[4] = ChildHelper.Leaf("coordination_skill", Coordination);
        }

        return array;
    }
}
