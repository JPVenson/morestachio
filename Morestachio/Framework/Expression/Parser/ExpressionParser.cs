﻿using System.Globalization;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.StringParts;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;
using Morestachio.TemplateContainers;

namespace Morestachio.Framework.Expression.Parser;

/// <summary>
///     This class provides methods for parsing ether a String or an Expression
/// </summary>
public static class ExpressionParser
{
	internal const string ExpressionNodeName = "Expression";
	internal const string ExpressionKindNodeName = "ExpressionKind";

	internal static string StringifyVariableAssignmentType(TokenType type)
	{
		switch (type)
		{
			case TokenType.VariableLet:
				return "#let";
			case TokenType.VariableVar:
				return "#var";
			default:
				return "???";
		}
	}

	internal static TokenPair TokenizeVariableAssignment(
		string tokenValue,
		TokenzierContext context,
		TokenType type,
		IEnumerable<ITokenOption> options,
		TokenMatch tokenMatch
	)
	{
		switch (type)
		{
			case TokenType.VariableLet:
				break;
			case TokenType.VariableVar:
				break;
			default:
				context.Errors.Add(new MorestachioSyntaxError(
					tokenMatch.Range,
					"#var", "", "#var name", "Expected #var or #let"));

				break;
		}

		var strVarType = StringifyVariableAssignmentType(type) + " ";
		var variableNameIndex = tokenValue.IndexOf(strVarType, StringComparison.OrdinalIgnoreCase);

		if (variableNameIndex != 0)
		{
			context.Errors.Add(new MorestachioSyntaxError(
				tokenMatch.Range,
				strVarType, "", strVarType + "name", "Expected " + strVarType));

			return default;
		}

		tokenValue = tokenValue.Substring(strVarType.Length);
		//context.AdvanceLocation(strVarType.Length);
		string variableName = null;

		if (strVarType.Length < 3)
		{
			context.Errors.Add(new MorestachioSyntaxError(
				tokenMatch.Range,
				strVarType, "", strVarType + "name", "Invalid character detected. Expected only spaces or letters."));

			return default;
		}

		var i = 0;
		var lengthToExpression = 0;

		for (; i < tokenValue.Length; i++)
		{
			var c = tokenValue[i];

			if (c == '=')
			{
				variableName = tokenValue.Substring(0, i).Trim(' ');

				break;
			}

			lengthToExpression++;

			if (!char.IsLetter(c) && c != ' ')
			{
				if (i == 0 && c == '$')
				{
					continue;
				}

				context.Errors.Add(new MorestachioSyntaxError(
					tokenMatch.Range,
					strVarType, "", strVarType + "name",
					"Invalid character detected. Expected only spaces or letters."));

				return default;
			}
		}

		//context.AdvanceLocation(lengthToExpression);

		if (variableName == null)
		{
			context.Errors.Add(new MorestachioSyntaxError(
				tokenMatch.Range,
				strVarType, "", strVarType + "name", "expected variable name"));
		}

		var expression = tokenValue.Substring(tokenValue.IndexOf('=')).Trim(' ', '=');

		if (string.IsNullOrEmpty(expression))
		{
			context.Errors.Add(new MorestachioSyntaxError(
				tokenMatch.Range,
				strVarType, "", strVarType + "name = ", "expected ether an path expression or an string value"));

			return default;
		}

		return new TokenPair(type,
			variableName,
			tokenMatch.Range,
			ParseExpression(expression, context, tokenMatch.Range.RangeStart)
				.Expression,
			options);
	}

	/// <summary>
	///     Parses an Expression and then executes it
	/// </summary>
	public static async ObjectPromise EvaluateExpression(
		string expressionText,
		ParserOptions options,
		object context,
		TokenzierContext tokenzierContext
	)
	{
		var expression = ParseExpression(expressionText, tokenzierContext).Expression;

		if (expression == null)
		{
			return null;
		}

		var contextObject = new ContextObject("", null, context);
		var value = await expression.GetValue(contextObject, new ScopeData(options)).ConfigureAwait(false);

		return value.Value;
	}

	/// <summary>
	///     Parses an Expression and then executes it
	/// </summary>
	public static ObjectPromise EvaluateExpression(
		string expressionText,
		ParserOptions options,
		object context
	)
	{
		return EvaluateExpression(expressionText, options, context, TokenzierContext.FromText(expressionText));
	}

	/// <summary>
	///     Parses an Expression and then executes it
	/// </summary>
	public static ObjectPromise EvaluateExpression(
		string expressionText,
		object context
	)
	{
		return EvaluateExpression(expressionText, ParserOptionsDefaultBuilder.GetDefaults().Build(), context);
	}

	/// <summary>
	///     Parses an Expression and then executes it
	/// </summary>
	public static ObjectPromise EvaluateExpression(
		string expressionText,
		ParserOptions options
	)
	{
		return EvaluateExpression(expressionText, options, new Dictionary<string, object>());
	}

	/// <summary>
	///     Parses an Expression and then executes it
	/// </summary>
	public static ObjectPromise EvaluateExpression(string expressionText)
	{
		return EvaluateExpression(expressionText, ParserOptionsDefaultBuilder.GetDefaults().Build(),
			new Dictionary<string, object>());
	}

	/// <summary>
	///     Parses the given text to ether an expression or an string
	/// </summary>
	/// <param name="expression">the text expression that should be parsed until ether EOEX or ; or #</param>
	/// <param name="context">the context describing the whole document</param>
	/// <returns></returns>
	public static ExpressionParserResult ParseExpression(
		string expression,
		TokenzierContext context
	)
	{
		return ParseExpression(expression, context, default);
	}

	/// <summary>
	///     Parses the given text to ether an expression or an string
	/// </summary>
	/// <param name="expression">the text expression that should be parsed until ether EOEX or ; or #</param>
	/// <param name="context">the context describing the whole document</param>
	/// <param name="index">the index of the global template where this token is located. Can be null if there is no global template.</param>
	/// <returns></returns>
	public static ExpressionParserResult ParseExpression(
		string expression,
		TokenzierContext context,
		TextIndex index
	)
	{
		return ParseExpression(expression, context, TextRange.All(expression), index);
	}

	/// <summary>
	///		Defines the result of an expression parsing operation
	/// </summary>
	public ref struct ExpressionParserResult
	{
		/// <summary>
		///		Creates a new Parsing result
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="sourceBoundary"></param>
		public ExpressionParserResult(IMorestachioExpression expression, TextRange sourceBoundary)
		{
			Expression = expression;
			SourceBoundary = sourceBoundary;
		}

		/// <summary>
		///		The created expression
		/// </summary>
		public IMorestachioExpression Expression { get; }

		/// <summary>
		///		The range of chars within the source template of the text handed to the <see cref="ExpressionParser.ParseExpression(string,Morestachio.Framework.Expression.Framework.TokenzierContext)"/> method.
		/// </summary>
		public TextRange SourceBoundary { get; }
	}

	/// <summary>
	///     Parses the given text to ether an expression or an string
	/// </summary>
	/// <param name="text">the text expression that should be parsed until ether EOEX or ; or #</param>
	/// <param name="context">the context describing the whole document</param>
	/// <param name="textBoundary">defines the area within text that should be processed</param>
	/// <param name="index">the index of the global template where this token is located. Can be null if there is no global template.</param>
	/// <returns></returns>
	public static ExpressionParserResult ParseExpression(
		string text,
		TokenzierContext context,
		TextRange textBoundary,
		TextIndex index = default
	)
	{
		if (text.Length == 0)
		{
			context.Errors.Add(new MorestachioSyntaxError(
				new TextRange(index, textBoundary.RangeEnd.Add(context, index)),
				"", "", "", "expected ether an path expression or an string value"));

			return default;
		}

		var tokenize = ExpressionTokenizer.TokenizeExpression(text, context, textBoundary, index);

		if (context.Errors.Any())
		{
			return default;
		}

		tokenize.Item1.Reset();
		var morestachioExpression = ParseExpressionRoot(tokenize.Item1, context);
		return new ExpressionParserResult(morestachioExpression,
			TextRange.RangeIndex(context, index.Index, tokenize.Item2.Index));
	}

	/// <summary>
	///     Parses the given text to ether an expression or an string
	/// </summary>
	/// <param name="expression"></param>
	/// <param name="context"></param>
	/// <param name="cultureInfo"></param>
	/// <returns></returns>
	public static IMorestachioExpression ParseExpression(
		string expression,
		out TokenzierContext context,
		CultureInfo cultureInfo = null
	)
	{
		context = new TokenzierContext(Tokenizer.FindNewLines(expression), cultureInfo ?? CultureInfo.CurrentCulture);
		return ParseExpression(expression, context, TextRange.All(expression)).Expression;
	}

	private static IMorestachioExpression ParseExpressionRoot(
		ExpressionTokens tokens,
		TokenzierContext context
	)
	{
		return ParseAnyExpression(tokens, context, token =>
		{
			switch (token.TokenType)
			{
				//only paths,brackets,numbers,string and operators are expected to lead an expression
				case ExpressionTokenType.Path:
				case ExpressionTokenType.Bracket:
				case ExpressionTokenType.Number:
				case ExpressionTokenType.String:
				case ExpressionTokenType.Operator:
				case ExpressionTokenType.LambdaOperator:
					return true;
				default:
					tokens.SyntaxError(context,
						token.Location,
						"Expected an expression, opening bracket, number, string or operator but got an argument name or argument seperator instead.");

					return false;
			}
		});
	}

	private static IMorestachioExpression ParseAnyExpression(
		ExpressionTokens tokens,
		TokenzierContext context,
		Func<IExpressionToken, bool> condition
	)
	{
		var expressionToken = tokens.TryPeek();
		var topParent = new MorestachioMultiPartExpressionList(expressionToken.Location);
		tokens.PeekLoop(condition, token => { ParseExpression(topParent, tokens, context, condition); });

		//optimize result tree
		if (topParent.Expressions.Count == 1)
		{
			return topParent.Expressions[0];
		}

		return topParent;
	}

	private static void ParseExpression(
		IMorestachioExpression topParent,
		ExpressionTokens tokens,
		TokenzierContext context,
		Func<IExpressionToken, bool> condition
	)
	{
		bool LoopExpressions(IExpressionToken token)
		{
			switch (token.TokenType)
			{
				case ExpressionTokenType.Path:
					ParseAndAddExpression((ExpressionToken)token, tokens, context, topParent);
					break;
				case ExpressionTokenType.Bracket:
					ParseAndAddBracket((ExpressionValueToken)token, tokens, context, topParent);
					break;
				case ExpressionTokenType.Number:
					ParseAndAddNumber((NumberToken)token, topParent);
					break;
				case ExpressionTokenType.String:
					ParseAndAddString((StringToken)token, topParent);
					break;
				case ExpressionTokenType.Operator:
					ParseAndAddOperator((OperatorToken)token, topParent, tokens, context, condition);
					break;
				case ExpressionTokenType.LambdaOperator:
					ParseAndAddLambdaOperator((LambdaExpressionToken)token, tokens, context, topParent, condition);
					break;
				case ExpressionTokenType.ArgumentSeperator:
				default:
					tokens.SyntaxError(context, token.Location, "Unexpected use of an argument seperator");

					return false;
			}

			return true;
		}

		tokens.Loop(condition, LoopExpressions);
	}

	private static void ParseAndAddLambdaOperator(
		LambdaExpressionToken token,
		ExpressionTokens tokens,
		TokenzierContext context,
		IMorestachioExpression topParent,
		Func<IExpressionToken, bool> condition
	)
	{
		MorestachioLambdaExpression lambdaExpression;

		if (topParent is MorestachioMultiPartExpressionList parentBracket)
		{
			if (parentBracket.Expressions.Count == 0)
			{
				tokens.SyntaxError(context,
					token.Location, "Invalid use of lambda operator without an list of parameters to its left");

				return;
			}

			var leftExpression = parentBracket.Expressions.Last();
			ValidateArgumentItemOrList(leftExpression, tokens, context, token);
			parentBracket.Expressions.Remove(leftExpression);
			parentBracket.Expressions.Add(lambdaExpression
				= new MorestachioLambdaExpression(leftExpression, leftExpression.Location));
		}
		else if (topParent is MorestachioExpression exp)
		{
			if (exp.Formats.Count == 0)
			{
				tokens.SyntaxError(context,
					token.Location, "Invalid use of lambda operator without an list of parameters to its left");

				return;
			}

			var argExp = exp.Formats.Last().MorestachioExpression;
			ValidateArgumentItemOrList(argExp, tokens, context, token);
			exp.Formats.Last().MorestachioExpression
				= lambdaExpression = new MorestachioLambdaExpression(argExp, argExp.Location);
		}
		else
		{
			tokens.SyntaxError(context,
				token.Location,
				"Invalid use of a Lambda expression. A Lambda expression can only be used as an argument for an Formatter");

			return;
		}

		if (tokens.Count == 0)
		{
			tokens.SyntaxError(context,
				token.Location, "Expected a 2nd expression for the used binary operator");

			return;
		}

		var operatRightExpression = ParseAnyExpression(tokens, context, subToken => { return condition(subToken); });
		lambdaExpression.Expression = operatRightExpression;
	}

	private static void ValidateArgumentItemOrList(
		IMorestachioExpression argument,
		ExpressionTokens tokens,
		TokenzierContext context,
		LambdaExpressionToken token
	)
	{
		switch (argument)
		{
			case MorestachioBracketExpression bracket:
			{
				foreach (var morestachioExpression in bracket.Expressions)
				{
					if (morestachioExpression is MorestachioExpression exp)
					{
						ValidateArgumentItem(exp, tokens, context, token);
					}
					else
					{
						tokens.SyntaxError(context,
							token.Location,
							$"The argument {morestachioExpression.AsStringExpression()} is not in the correct format only single names without special characters and without path are supported");
					}
				}

				break;
			}
			case MorestachioExpression exp:
				ValidateArgumentItem(exp, tokens, context, token);

				break;
			default:
			{
				tokens.SyntaxError(context,
					token.Location,
					$"The argument {argument.AsStringExpression()} is not in the correct format only single names without special characters and without path are supported");

				break;
			}
		}
	}

	private static void ValidateArgumentItem(
		MorestachioExpression argument,
		ExpressionTokens tokens,
		TokenzierContext context,
		LambdaExpressionToken token
	)
	{
		if (argument.FormatterName == null
			&& argument.PathParts.HasValue
			&& argument.PathParts.Count <= 1
			&& argument.PathParts.Current.Value == PathType.DataPath)
		{
			return;
		}

		tokens.SyntaxError(context,
			token.Location,
			$"The argument {argument.AsStringExpression()} is not in the correct format only single names without special characters and without path are supported");
	}

	private static void ParseAndAddOperator(
		in OperatorToken token,
		IMorestachioExpression topParent,
		ExpressionTokens tokens,
		TokenzierContext context,
		Func<IExpressionToken, bool> condition
	)
	{
		var op = MorestachioOperator.Operators[token.Value];

		if (op.Placement == OperatorPlacement.Right)
		{
			MorestachioOperatorExpression operat;

			//the operator is placed to the right hand of the expression.
			//remove the last added expression or expression list and replace it with this operator
			if (topParent is MorestachioMultiPartExpressionList parentBracket)
			{
				if (parentBracket.Expressions.Count == 0)
				{
					tokens.SyntaxError(context,
						token.Location, "Invalid use of right hand operator without an expression to its left");

					return;
				}

				var leftExpression = parentBracket.Expressions.Last();
				parentBracket.Expressions.Remove(leftExpression);
				parentBracket.Expressions.Add(operat
					= new MorestachioOperatorExpression(op, leftExpression, leftExpression.Location));
			}
			else if (topParent is MorestachioExpression exp)
			{
				if (exp.Formats.Count == 0)
				{
					tokens.SyntaxError(context,
						token.Location, "Invalid use of right hand operator without an expression to its left");

					return;
				}

				var argExp = exp.Formats.Last().MorestachioExpression;

				exp.Formats.Last().MorestachioExpression =
					operat = new MorestachioOperatorExpression(op, argExp, argExp.Location);
			}
			else if (topParent is MorestachioOperatorExpression opr)
			{
				opr.RightExpression = operat = new MorestachioOperatorExpression(op,
					opr.RightExpression, opr.Location);
			}
			else
			{
				tokens.SyntaxError(context,
					token.Location, "Invalid use of a Binary operator on an unsupported expression type");

				return;
			}

			if (op.IsBinaryOperator)
			{
				if (tokens.Count == 0)
				{
					tokens.SyntaxError(context,
						token.Location, "Expected a 2nd expression for the used binary operator");

					return;
				}

				var operatRightExpression = ParseAnyExpression(tokens, context,
					subToken => { return condition(subToken) && subToken.TokenType != ExpressionTokenType.Operator; });
				operat.RightExpression = operatRightExpression;
			}
		}
		else
		{
			//the operator is placed on the left hand of the expression so it must be an unary operator
			//it can only accept one argument
			var operatorExp = new MorestachioOperatorExpression(op, null, token.Location);
			AddToParent(operatorExp, topParent);
			operatorExp.LeftExpression = ParseAnyExpression(tokens, context,
				subToken => { return condition(subToken) && subToken.TokenType != ExpressionTokenType.Operator; });
		}
	}

	private static void ParseAndAddNumber(in NumberToken token, IMorestachioExpression topParent)
	{
		var expressionNumber = new MorestachioExpressionNumber(token.Number, token.Location);
		AddToParent(expressionNumber, topParent);
	}

	private static void ParseAndAddString(in StringToken token, IMorestachioExpression topParent)
	{
		var strExpression = new MorestachioExpressionString(token.Location, token.Delimiter);
		strExpression.StringParts.Add(new ExpressionStringConstPart(token.Value, token.Location));
		AddToParent(strExpression, topParent);
	}

	private static void ParseAndAddBracket(
		ExpressionValueToken token,
		ExpressionTokens tokens,
		TokenzierContext context,
		IMorestachioExpression topParent
	)
	{
		var exp = new MorestachioBracketExpression(token.Location);

		bool SubCondition(IExpressionToken subToken)
		{
			return !(subToken.TokenType == ExpressionTokenType.Bracket &&
				((ExpressionValueToken)subToken).Value == ")");
		}

		tokens.PeekLoop(SubCondition, subToken =>
		{
			if (subToken.TokenType == ExpressionTokenType.ArgumentSeperator)
			{
				tokens.Dequeue();

				return;
			}

			exp.Add(ParseAnyExpression(tokens, context,
				expToken => SubCondition(expToken) && expToken.TokenType != ExpressionTokenType.ArgumentSeperator));
		});

		tokens.TryDequeue(() =>
		{
			tokens.SyntaxError(context,
				token.Location, "Expected a )");
		}); //dequeue )
		AddToParent(exp, topParent);
	}

	private static void ParseAndAddExpression(
		in ExpressionToken expressionToken,
		ExpressionTokens tokens,
		TokenzierContext context,
		IMorestachioExpression topParent
	)
	{
		var expression = ParseMorestachioExpression(expressionToken, tokens, context);
		AddToParent(expression, topParent);
	}

	private static void AddToParent(IMorestachioExpression expression, IMorestachioExpression topParent)
	{
		if (topParent is MorestachioMultiPartExpressionList parentBracket)
		{
			parentBracket.Add(expression);
		}
		else if (topParent is MorestachioExpression exp)
		{
			exp.Formats.Add(new ExpressionArgument(exp.Location, expression, null));
		}
		else if (topParent is MorestachioOperatorExpression opr)
		{
			opr.RightExpression = expression;
		}
		else
		{
			throw new MorestachioParserException(
				$"Internal Parser error. Tried to add '{expression.GetType()}' to '{topParent.GetType()}'");
		}
	}

	private static IMorestachioExpression ParseMorestachioExpression(
		ExpressionToken token,
		ExpressionTokens tokens,
		TokenzierContext context
	)
	{
		var expression = new MorestachioExpression(token.Location);
		var next = tokens.TryPeek();

		if (next?.TokenType == ExpressionTokenType.Bracket && ((ExpressionValueToken)next).Value == "(")
		{
			expression.FormatterName = token.Value.GetFormatterName(context, 0);
			expression.PathParts = new Traversable(token.Value.Compile(context, 0));

			tokens.TryDequeue(() =>
			{
				tokens.SyntaxError(context,
					token.Location, "Expected a (");
			});

			bool Condition(IExpressionToken innerToken)
			{
				return !(innerToken.TokenType == ExpressionTokenType.Bracket &&
					((ExpressionValueToken)innerToken).Value == ")");
			}

			tokens.PeekLoop(Condition, subToken =>
			{
				string argumentName = null;

				if (subToken.TokenType == ExpressionTokenType.Argument)
				{
					tokens.Dequeue();
					argumentName = ((ExpressionValueToken)subToken).Value;
				}

				var anyElse = ParseAnyExpression(tokens, context,
					innerToken =>
					{
						return Condition(innerToken) && innerToken.TokenType != ExpressionTokenType.ArgumentSeperator;
					});
				expression.Formats.Add(new ExpressionArgument(anyElse.Location, anyElse, argumentName));
				next = tokens.TryPeek();

				if (next == null)
				{
					tokens.SyntaxError(context,
						token.Location,
						"Unexpected end of expression. Expected ether a argument seperator ',' or a closing bracket ')'");
				}

				if (next?.TokenType != ExpressionTokenType.ArgumentSeperator)
				{
					return false;
				}

				if (next?.TokenType == ExpressionTokenType.ArgumentSeperator)
				{
					tokens.Dequeue(); //dequeue ,
				}

				return true;
			});

			tokens.TryDequeue(() => { tokens.SyntaxError(context, token.Location, "Expected a )"); });
		}
		else
		{
			expression.PathParts = new Traversable(token.Value.CompileListWithCurrent(context, 0));
		}

		return expression;
	}
}