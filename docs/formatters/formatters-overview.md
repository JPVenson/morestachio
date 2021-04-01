Morestachio can be used with Formatters.

A Formatter is custom .net code that can be invoked from within the Template to format a value for display.
Formatters are managed by the `MorestachioFormatterService`. This class holds and executes all Formatters that are called via the `Path.To.Data.FormatAs()` syntax. The last part of the Path is seen as the name of the formatter that should be invoked, its just like a normal function you call from c# code.

An Formatter call is build like this:
![flowdiagram](https://i.imgur.com/JA2Qq5F.png)
```
'(' (Optional)
   : path   
   | string

string
   : dot (Optional)
   | ')' (Optional)

path
   | dot
   | ')'

dot
   | path
   | formatter_name

'('
   : [argument_name] (Optional)
   : "string"|'string'
   : expression
   : , (Seperator)

')' (Optional)
   : path
```

# Formatter Framework

The Formatter Framework is build for mapping C# functions to be used within Morestachio. You can declare them by annotating an static method or function with one of the 3 `Attribute`s.  
There are 3 different kinds of formatters you can declare:
- Standard Formatter:
  - An formatter with a name that is called directly on an object (SourceObject), that as no, one or many arguments and _can_ return another object. Declared by `[MorestachioFormatter]`
- Global Formatter:
  - An formatter with a name that can be called anywhere and does not check for an specific type, that as no, one or many other arguments and _can_ return another object. Declared by `[MorestachioGlobalFormatter]`
- Operator Formatter:
  - An formatter that does _not_ have an name but declares an `OperatorTypes`, must only have exactly two parameters and must declare the type of operator it is handling. Declared by `[MorestachioOperator]` (See https://github.com/JPVenson/morestachio/wiki/Formatter-Operators)
### Binding static methods

You can add static methods from an external class by calling `ParserOptions.Formatters.AddFromType` on the type. 
The `ParserOptions.Formatters.AddFromType` method relies on the presence of an attribute (see list above)

**Example**    
```csharp
public static class StringFormatter
{
        /// <summary>
	///	Example Formatter: "string".reverse()
	/// </summary>
	/// <returns></returns>
	[MorestachioFormatter("reverse", "XXX")]
	public static string Reverse(string originalObject)
	{
		return originalObject.Reverse().Select(e => e.ToString()).Aggregate((e, f) => e + f);
	}

        /// <summary>
	///	Example Global Formatter: Random()
	/// </summary>
	/// <returns></returns>
	[MorestachioGlobalFormatter("Random", "Gets a non-negative random number")]
	public static int Random()
	{
		return _random.Next();
	}

	/// <summary>
	///	Example Operator: "test" + "test"
	/// </summary>
	/// <param name="source"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	[MorestachioOperator(OperatorTypes.Add, "Concatenates two strings")]
	public static string Append(string source, string target)
	{
		return source + target;
	}
}
```
You can add the whole class's formatters by calling the `AddFromType` Extension method from your ParserOptions like this:
```csharp
ParserOptions.Formatters.AddFromType(typeof(StringFormatter));
```
this function does only support static functions from that type. If you want to add an delegate you can call `ParserOptions.Formatters.AddSingle(Delegate delegate, String name = null)` method or one of its overloads.   

### Binding instance methods

To allow the calling of an instance method you must also annotate the method on your class with an attribute but on instance methods only the `MorestachioFormatterAttribute` is supported. Instance methods cannot be global or operators. Then proceed by invoking `ParserOptions.Formatters.AddFromType` with your instance type.


## Binding
In difference to the single type single formatter usage of Morestachio lib, this syntax also supports Generic types and extended mapping of types to formatters. You must annotate the method with the `MorestachioFormatter` and define a name for it.   

The Formatter Framework understands Generics for any parameter and for all kinds of formatters. Generic constraints are *not* supported nor are they checked.

# Warning: MemoryLeaks ahead
If you are using the `AddSingle(Delegate, String)` method to add an delegate that will access any other objects properties morestachio will hold a reference to that object as long as the parent `MorestachioFormatterService` exists. Be aware that this might causes event leaks (https://stackoverflow.com/questions/4526829/why-and-how-to-avoid-event-handler-memory-leaks)  
 

## Async/Await in formatter

All formatters support `async` & `await`. When using NetCore Version > 2.1 you could also use `ValueTask`

## Formatter service injection

Please see https://github.com/JPVenson/morestachio/wiki/Services

## Formatter arguments

You can ether give an string or an expression as an argument to the Formatter.

### Formatter string
```csharp
{{Data.Path.To.Value.NameOfFormatter("i am a string value")}}
```
Escaping formatter values
In general all formatter arguments are considered strings or references. You must escape every string with ether " or ' if you write something without escaping it, it will be considered an expression to a property. To escape the string keyword write " or '.

### Example of escaping

```csharp
{{this.is.a.valid.formatter("a string, with a comma, and other {[]}{§$%& stuff. also a escaped \" and \\" and so on")}}
```

Hint
> When using a Formatter with an tilde operator (~) for formatting an root object take note that you cannot access the current scope. For example:

```csharp
{{#Data.Data}} <-- Scopes to Data.Data
{{ValueInDataData}} <-- Access values inside Data.Data
{{~Data.Foo(ValueInDataData)}} <--not possible as the whole expression is in the scope of the root object
{{/Data.Data}}
```

> this is by design, as when using the ~ operator you scope the whole expression that follows to the Root object.

### Expressions

To include a other value of the Model in a formatter just use Path in the place of the parameter. Can be combined with `[Name]Path.To.Value`.

You can also call Formatter within the expression and give them individual arguments that can also be ether a string or an expression.

## Value Converter

Morstachio understands the basic .net conversion like converting an string value from the template for an int argument used in the arguments of an formatter. You can extend this list of formatters globally and per function argument.

To extend the global list of formatters just add your implementation of `IFormatterValueConverter` to the used MorestachioFormatterService. If you want to specify a formatter that should be always used for a certain argument, you can annotate the parameter with the `FormatterValueConverterAttribute`

For Example, if you want to convert any object to an string for an certain parameter on a certain formatter, you will have to first, create a `IFormatterValueConverter` to make this conversion like this:   

*Example Encoding Converter*
```csharp
/// <summary>
///		Parses an string to an encoding object
/// </summary>
public class EncodingConverter : IFormatterValueConverter
{
	/// <inheritdoc />
	public bool CanConvert(Type sourceType, Type requestedType)
	{
		return (sourceType == typeof(string) || sourceType == typeof(Encoding))
		       && requestedType == typeof(Encoding);
	}
	
	/// <inheritdoc />
	public object Convert(object value, Type requestedType)
	{
		if (value is Encoding)
		{
			return value;
		}

		return Encoding.GetEncoding(value.ToString());
	}
}
```
This formatter can ether pass an encoding if both `sourceType` (the type of value from the Expression) and `requestedType` (the type of value present as the argument) are `Encoding` or if the `sourceType` is an string.

To use this Converter you can ether add it to the list of global Converters to allow a general conversion of (`Encoding`, `string`) => `Encoding` like this:
```csharp
var parserOptions = new ParserOptions(...);
parserOptions.Formatters.ValueConverter.Add(new EncodingConverter());
```
   
Or you can annotate a single parameter with the `FormatterValueConverterAttribute` to only declare a conversion for this single parameter.
Example:
```csharp
[MorestachioFormatter("FormatterName", "XXX")]
public static int BlaBliBlub([FormatterValueConverter(typeof(EncodingConverter))]Encoding value)
{
	return value.CodePage;
}
```
