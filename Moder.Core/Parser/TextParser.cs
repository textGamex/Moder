using System.Text;
using ParadoxPower.CSharp;
using ParadoxPower.Process;

namespace Moder.Core.Parser;

public class TextParser
{
	public string FilePath { get; }

	public bool IsSuccess { get; }

	public bool IsFailure => !IsSuccess;

	private readonly ParserError? _error;

	private readonly Node? _node;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="filePath"></param>
	/// <exception cref="FileNotFoundException">如果文件不存在</exception>
	/// <exception cref="IOException"></exception>
	public TextParser(string filePath)
	{
		FilePath = File.Exists(filePath) ? filePath : throw new FileNotFoundException($"找不到文件: {filePath}", filePath);
		var fileName = Path.GetFileName(filePath);
		var result = Parsers.ParseScriptFile(fileName, File.ReadAllText(filePath));
		IsSuccess = result.IsSuccess;
		if (IsFailure)
		{
			_error = result.GetError();
			return;
		}

		_node = Parsers.ProcessStatements(fileName, filePath, result.GetResult());
	}

	public TextParser(string fileName, string fileContent)
	{
		FilePath = fileName;
		var result = Parsers.ParseScriptFile(fileName, fileContent);
		IsSuccess = result.IsSuccess;
		if (IsFailure)
		{
			_error = result.GetError();
			return;
		}

		_node = Parsers.ProcessStatements(fileName, string.Empty, result.GetResult());
	}

	public static async Task<TextParser> Parser(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"找不到文件: {filePath}", filePath);
		}

		var fileName = Path.GetFileName(filePath);
		var result = Parsers.ParseScriptFile(fileName, await File.ReadAllTextAsync(filePath));
		var isSuccess = result.IsSuccess;
		ParserError? error = null;
		Node? node = null;
		if (!isSuccess)
		{
			error = result.GetError();
		}
		else
		{
			node = Parsers.ProcessStatements(fileName, filePath, result.GetResult());
		}

		return new TextParser(filePath, isSuccess, error, node);
	}

	private TextParser(string filePath, bool isSuccess, ParserError? error, Node? node)
	{
		FilePath = filePath;
		IsSuccess = isSuccess;
		_error = error;
		_node = node;
	}

	static TextParser()
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	public Node GetResult()
	{
		return _node ?? throw new InvalidOperationException($"文件解析失败, 无法返回解析结果, 文件路径: {FilePath}.");
	}

	public ParserError GetError()
	{
		return _error ?? throw new InvalidOperationException();
	}
}