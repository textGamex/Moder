// using System.ComponentModel;
// using System.Diagnostics;
// using CommunityToolkit.Mvvm.ComponentModel;
// using CommunityToolkit.Mvvm.Input;
// using CommunityToolkit.WinUI;
// using EnumsNET;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.UI.Xaml.Controls;
// using Moder.Core.Models.Vo;
// using Moder.Core.Services;
// using NLog;
// using ParadoxPower.Process;
//
// namespace Moder.Core.Models;
//
// public abstract partial class ObservableGameValue(string key, NodeVo? parent) : ObservableObject
// {
//     public string Key { get; } = key;
//     public bool IsChanged { get; private set; }
//     public NodeVo? Parent { get; } = parent;
//     public string TypeString => Type.ToString();
//     public IReadOnlyList<GameVoType> VoTypes => Enums.GetValues<GameVoType>();
//     public GameValueType Type { get; init; }
//
//     public IRelayCommand RemoveSelfInParentCommand =>
//         _removeSelfInParentCommand ??= new RelayCommand(RemoveSelfInParent);
//     private RelayCommand? _removeSelfInParentCommand;
//
//     public IRelayCommand<StackPanel> AddAdjacentValueCommand =>
//         _addAdjacentValueCommand ??= new RelayCommand<StackPanel>(AddAdjacentValue);
//     private RelayCommand<StackPanel>? _addAdjacentValueCommand;
//
//     private static readonly Logger Log = LogManager.GetCurrentClassLogger();
//     protected static readonly LeafConverterService ConverterService =
//         App.Current.Services.GetRequiredService<LeafConverterService>();
//
//     public abstract Child[] ToRawChildren();
//
//     protected override void OnPropertyChanged(PropertyChangedEventArgs e)
//     {
//         IsChanged = true;
//         base.OnPropertyChanged(e);
//     }
//
//     private void RemoveSelfInParent()
//     {
//         Debug.Assert(Parent != null, "Parent cannot be null");
//         if (Parent is null)
//         {
//             Log.Warn("删除节点失败, 父节点为空");
//             return;
//         }
//
//         Parent.Remove(this);
//         // 如果父节点下没有其他子节点，则删除父节点
//         // if (Parent.Children.Count == 0 && Parent.Parent is not null)
//         // {
//         //     Parent.Parent.Remove(Parent);
//         // }
//     }
//
//     private void AddAdjacentValue(StackPanel? value)
//     {
//         if (value is null)
//         {
//             return;
//         }
//
//         if (Parent is null)
//         {
//             Log.Warn("添加相邻节点失败, 父节点为空");
//             return;
//         }
//
//         var addedKeywordTextBox = value.FindChild<TextBox>(box => box.Name == "NewKeywordTextBox");
//         var addedValueTextBox = value.FindChild<TextBox>(box => box.Name == "NewValueTextBox");
//         var typeComboBox = value.FindChild<ComboBox>(box => box.Name == "TypeComboBox");
//         Debug.Assert(
//             addedKeywordTextBox is not null && addedValueTextBox is not null,
//             "添加相邻节点失败, 未找到TextBox"
//         );
//         Debug.Assert(typeComboBox is not null, "添加相邻节点失败, 未找到ComboBox");
//
//         if (typeComboBox.SelectedItem is null)
//         {
//             Log.Warn("添加相邻节点失败, 未选择类型");
//             return;
//         }
//
//         var newKeyword = addedKeywordTextBox.Text;
//         var newValue = addedValueTextBox.Text;
//         var voType = (GameVoType)typeComboBox.SelectedItem;
//
//         if (
//             string.IsNullOrWhiteSpace(newKeyword)
//             || (voType == GameVoType.Leaf && string.IsNullOrWhiteSpace(newValue))
//         )
//         {
//             Log.Warn("添加相邻节点失败, 输入值为空");
//             return;
//         }
//
//         var index = Parent.Children.IndexOf(this) + 1;
//         ObservableGameValue newObservableGameValue = voType switch
//         {
//             GameVoType.Node => new NodeVo(newKeyword, Parent),
//             GameVoType.Leaf => ConverterService.GetSpecificLeafVo(newKeyword, newValue, Parent),
//             GameVoType.LeafValues => new LeafValuesVo(newKeyword, [newValue], Parent),
//             GameVoType.Comment => new CommentVo(newKeyword, Parent),
//             _ => throw new ArgumentOutOfRangeException()
//         };
//         Parent.Children.Insert(index, newObservableGameValue);
//
//         addedKeywordTextBox.Text = string.Empty;
//         addedValueTextBox.Text = string.Empty;
//
//         Log.Info("添加相邻节点成功, 关键字: {Keyword}, 值: {Value}, 父节点: {Parent}", newKeyword, newValue, Parent.Key);
//     }
// }
