using Microsoft.UI.Xaml.Controls;
using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

public sealed partial class CharacterEditorControlView : UserControl
{
    public CharacterEditorControlViewModel ViewModel { get; }

    public CharacterEditorControlView(CharacterEditorControlViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        viewModel.LevelModifierDescription = LevelModifierDescriptionTextBlock.Inlines;
        viewModel.AttackModifierDescription = AttackModifierDescriptionTextBlock.Inlines;
        viewModel.DefenseModifierDescription = DefenseModifierDescriptionTextBlock.Inlines;
        viewModel.PlanningModifierDescription = PlanningModifierDescriptionTextBlock.Inlines;
        viewModel.LogisticsModifierDescription = LogisticsModifierDescriptionTextBlock.Inlines;
        viewModel.ManeuveringModifierDescription = ManeuveringModifierDescriptionTextBlock.Inlines;
        viewModel.CoordinationModifierDescription = CoordinationModifierDescriptionTextBlock.Inlines;

        ViewModel.SetSkillDefaultValue();
    }
}
