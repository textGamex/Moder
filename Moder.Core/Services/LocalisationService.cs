using System.Collections.Frozen;
using System.Diagnostics;
using ParadoxPower.CSharp;
using ParadoxPower.Localisation;

namespace Moder.Core.Services;

public sealed class LocalisationService
{
	private readonly FrozenDictionary<string, string> _localisations;

	public LocalisationService(IEnumerable<string> filePaths)
	{
		// 预设容量值来自 1.14.8 版本
		var localisations = new Dictionary<string, string>(88188);
		foreach (var filePath in filePaths)
		{
			var localisation = YAMLLocalisationParser.parseLocFile(filePath);
			if (localisation.IsFailure)
			{
				break;
			}

			var result = localisation.GetResult();

			foreach (var item in result.entries)
			{
				localisations[item.key] = GetCleanDesc(item.desc);
			}
		}

		_localisations = localisations.ToFrozenDictionary();
	}

	// 去除开头和结尾的 "
	private static string GetCleanDesc(string rawDesc)
	{
		return rawDesc.Length switch
		{
			> 2 => rawDesc[1..^2],
			2 => string.Empty,
			_ => rawDesc
		};
	}

	/// <summary>
	/// 如果本地化文本不存在, 则返回<c>key</c>
	/// </summary>
	/// <returns></returns>
	public string GetValue(string key)
	{
		return _localisations.GetValueOrDefault(key, key);
	}
}