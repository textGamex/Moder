using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Messages;
using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

// 不能使用 Ioc 容器管理, 因为需要使用 IDisposable 接口释放资源
public sealed partial class CharacterEditorControlView : IDisposable
{
    public CharacterEditorControlViewModel ViewModel { get; }

    public CharacterEditorControlView()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetRequiredService<CharacterEditorControlViewModel>();
        ViewModel.LevelModifierDescription = LevelModifierDescriptionTextBlock.Inlines;
        ViewModel.AttackModifierDescription = AttackModifierDescriptionTextBlock.Inlines;
        ViewModel.DefenseModifierDescription = DefenseModifierDescriptionTextBlock.Inlines;
        ViewModel.PlanningModifierDescription = PlanningModifierDescriptionTextBlock.Inlines;
        ViewModel.LogisticsModifierDescription = LogisticsModifierDescriptionTextBlock.Inlines;
        ViewModel.ManeuveringModifierDescription = ManeuveringModifierDescriptionTextBlock.Inlines;
        ViewModel.CoordinationModifierDescription = CoordinationModifierDescriptionTextBlock.Inlines;

        ViewModel.InitializeSkillDefaultValue();

        WeakReferenceMessenger.Default.Register<AppLanguageChangedMessage>(this, OnLanguageChanged);
    }

    private void OnLanguageChanged(object recipient, AppLanguageChangedMessage message)
    {
        Bindings.Update();
    }

    public void Dispose()
    {
        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    ~CharacterEditorControlView()
    {
        ReleaseResources();
    }

    private void ReleaseResources()
    {
        ViewModel.Close();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}