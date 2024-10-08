﻿using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Models;
using Moder.Core.Models.Vo;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Controls;

public sealed partial class BaseLeaf : Control
{
	public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
		nameof(Key),
		typeof(string),
		typeof(BaseLeaf),
		new PropertyMetadata(null)
	);

	public static readonly DependencyProperty SlotContentProperty = DependencyProperty.Register(
		nameof(SlotContent),
		typeof(object),
		typeof(BaseLeaf),
		new PropertyMetadata(null)
	);

	public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
		nameof(Type),
		typeof(string),
		typeof(BaseLeaf),
		new PropertyMetadata(null)
	);

	public static readonly DependencyProperty LeafContextProperty = DependencyProperty.Register(
		nameof(ObservableGameValue),
		typeof(ObservableGameValue),
		typeof(BaseLeaf),
		new PropertyMetadata(null)
	);

	public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(
		nameof(AddCommand),
		typeof(ICommand),
		typeof(BaseLeaf),
		new PropertyMetadata(null)
	);

	public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
		nameof(RemoveCommand),
		typeof(ICommand),
		typeof(BaseLeaf),
		new PropertyMetadata(null)
	);

	public static readonly DependencyProperty GameVoTypeProperty = DependencyProperty.Register(
		nameof(GameVoType),
		typeof(IReadOnlyList<GameVoType>),
		typeof(BaseLeaf),
		new PropertyMetadata(null)
	);

	public IReadOnlyList<GameVoType> GameVoType
	{
		get => (IReadOnlyList<GameVoType>)GetValue(GameVoTypeProperty);
		private set => SetValue(GameVoTypeProperty, value);
	}

	public ObservableGameValue? LeafContext
	{
		get => (ObservableGameValue?)GetValue(LeafContextProperty);
		set
		{
			SetValue(LeafContextProperty, value);
			if (value is not null)
			{
				RemoveCommand = value.RemoveSelfInParentCommand;
				AddCommand = value.AddAdjacentValueCommand;
				Type = value.TypeString;
				Key = value.Key;
				GameVoType = value.VoTypes;
			}
		}
	}

	public ICommand? AddCommand
	{
		get => (ICommand?)GetValue(AddCommandProperty);
		set => SetValue(AddCommandProperty, value);
	}

	public ICommand? RemoveCommand
	{
		get => (ICommand?)GetValue(RemoveCommandProperty);
		set => SetValue(RemoveCommandProperty, value);
	}

	public string? Type
	{
		get => (string?)GetValue(TypeProperty);
		set => SetValue(TypeProperty, value);
	}

	public object? SlotContent
	{
		get => GetValue(SlotContentProperty);
		set => SetValue(SlotContentProperty, value);
	}

	public string? Key
	{
		get => (string?)GetValue(KeyProperty);
		set => SetValue(KeyProperty, value);
	}

	public BaseLeaf()
	{
		DefaultStyleKey = typeof(BaseLeaf);
	}
}
