# Morestachio 
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio?ref=badge_shield)



![Icon](https://github.com/JPVenson/morestachio/blob/master/Morestachio/Morestachio%20248x248.png?raw=true)

A Lightweight, powerful, flavorful, templating engine for C# and other .net-based languages. Its a fork of Mustachio.

#### Installing Morestachio:


|Project|Github|Nuget|NugetCLI|Status|
|---|---|---|---|---|
| Morestachio | [![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/JPVenson/morestachio?include_prereleases)](https://github.com/JPVenson/morestachio/releases) | [![Nuget Morestachio](https://img.shields.io/nuget/v/Morestachio?label=Morestachio)](https://www.nuget.org/packages/Morestachio/) | Install-Package Morestachio | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3)|
| Morestachio.Linq |  | [![Nuget Morestachio Linq](https://img.shields.io/nuget/v/Morestachio.Linq?label=%20Morestachio.Linq)](https://www.nuget.org/packages/Morestachio.Linq/) | Install-Package Morestachio.Linq | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3)|
| Morestachio.Runner |  | [![Nuget Morestachio Runner](https://img.shields.io/nuget/v/Morestachio.Runner?label=%20Morestachio.Runner)](https://www.nuget.org/packages/Morestachio.Runner/) | Install-Package Morestachio.Runner | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3)|
| Morestachio.Newtonsoft.Json|  | [![Nuget Morestachio Json](https://img.shields.io/nuget/v/Morestachio.Newtonsoft.Json?label=Morestachio.Newtonsoft.Json)](https://www.nuget.org/packages/Morestachio.Newtonsoft.Json/) | Install-Package Morestachio.Newtonsoft.Json | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3)|

#### What's this for?

*Morestachio* allows you to create simple text-based templates that are fast and safe to render. It is optimized for WebServers and offers a high degree of customization with its Formatter syntax.

#### Morestachio Playground:
Try it out, without consequenses. The Morestachio Online editor allows you to create templates within your browser:
[Editor](https://morestachio.jean-pierre-bachmann.dev/)

#### How to use Morestachio:

```csharp
// Your Template
var sourceTemplate = "Dear {{name}}, this is definitely a personalized note to you. Very truly yours, {{sender}}"

// Parse the Template into the Document Tree. 
var document = Morestachio.Parser.ParseWithOptions(new Morestachio.ParserOptions(sourceTemplate));

// Create the values for the template model:
dynamic model = new ExpandoObject();
model.name = "John";
model.sender = "Sally";
// or with dictionarys
IDictionary model = new Dictionary<string, object>();
model["name"] = "John";
model["sender"] = "Sally";
//or with any other object
var model = new {name= "John", sender= "Sally"}

// Combine the model with the template to get content:
var content = document.CreateRenderer().RenderAndStringify(model); // Dear John, this is definitely a personalized note to you. Very truly yours, Sally
```


##### Key Features
Morestachio is build upon Mustachio and extends the mustachio syntax in a a lot of points.

1. each object can be formatted by adding formatter the the morestachio
2. Templates will be parsed as streams and will create a new stream for its output. This is better when creating larger templates and best for web as you can also limit the length of the "to be" created template to a certain size and write the result ether directly to an output stream or the Disc.
3. Its Lightning fast. Even unreasonably huge templates that contain >5000 instructions can be executed in around *0.5 secounds*
4. Morestachio accepts any object as source
5. Cancellation of Template generation is supported
6. Async calls are supported (For Formatters)
7. No External Depedencies for Morestachio.dll.
8. Support for several .Net framworks:
   - NetStandard (netstandard2.0; netstandard2.1; see [.NET implementation support](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support)) 
   - Net5.0, Net6.0(comming soon)
9. Build in Localization support and Logging support
10. Supports user Encoding of the result template
11. Supports Template Partials `{{#import 'secondary_template' }}`
12. Complex paths are supported `{{ this.is.a.valid.path }}` and `{{ ../this.goes.up.one.level }}` and `{{ ~.this.goes.up.to.Root }}`
13. Loops with `#each` & `#do` & `#while` & `#repeat`
14. Object Enumeration with `#each data.?`
15. Formatters can be declared in C# and be called from the template to provide you with a maximum of freedom
16. Extensive Build-In list of Formatters for most usecases
17. The Parser produces a Serilizable Document Tree that can be send to clients to provide a rich user edit experience 
 
**Template partials** ARE a great feature for large scale template development.

You can create a Partial with the `{{#declare NAME}}Partial{{/declare}}` syntax. You can navigate up inside this partials. Partials can also be nested but are currently restricted to a maximum recursion of 255 depth. The programmer has the choice to define a behavior that ether throws an Exception or does nothing and ignores any deeper recusions.    

A Partial must be declared before its usage with `{{#import 'NAME'}}` but you can use a partial to create hirarical templates.    

You can even inject your predefined Partials into all of your Templates by utilizing the `ParserOptions.PartialsStore`.

###### Infos about new features
 
Its possible to use plain C# objects they will be called by reflection. 
Also you can now set the excact size of the template to limit it (this could be come handy if you are in a hostet environment) use the `ParserOptions.MaxSize` option to define a max size. It will be enforced on exact that amount of bytes in the stream.

##### Variable Output
One mayor component is the usage of Variable output strategies in morestachio.    
The output handling is done by a `IByteCounterStream` that wraps your specific output. This can ether be a `Stream`, `TextWriter`, `StringBuilder` or anything else. For thoese types Morestachio has pre defined Implementations named `ByteCounterStream`, `ByteCounterTextWriter` and `ByteCounterStringBuilder`. All thoese types are enforcing the `ParserOptions.MaxSize` property if set and will write your template with the set `ParserOptions.Encoding`
 
###### Formatter
Use the `ContextObject.DefaultFormatter` collection to create own formatter for all your types or add one to the `ParserOptions.Formatters` object for just one call. To invoke them in your template use the new Function syntax:
```csharp
{{Just.One.Formattable.FormatterToCall().Thing}}
```
This links a function named "FormatterToCall" that is ether present as a anonymous delegate added via `ParserOptions.Formatters.AddSingle(...)` or a `public static ` method attributed with the `MorestachioFormatterAttribute` added via the `ParserOptions.Formatters.AddFromType` or to an instance method attributed with the `MorestachioFormatterAttribute`

The formatter CAN return a new object on wich you can call new Propertys or it can return a string.
There are formatter prepaired for all Primitve types. That means per default you can call on an object hat contains a DateTime:
```csharp
{{MyObject.DateTime.ToString("D")}} <-- this will resolve a property "MyObject" and then "DateTime" and will call ToString on it with the argument "D"
```
that will call the `IFormattable` interface on the DateTime. 

**Formatter References** 
Can be used to reference another property/key in the template and then use it in a Formatter. Everything that is not a string (ether prefixed and suffixed with " or ') will be threaded as an expression that also can contain formatter calls
```csharp
{{MyObject.Value.ToString(Key)}}
```
This will call a formatter that is resposible for the type that `Value` has and will give it whats in `Key`. Example:
```csharp
//create the template
var template = "{{Value.ToStringX(Key)}}";
//create the model
var model = new Dictionary<string, object>();
model["Value"] = DateTime.Now; 
model["Key"] = "D";
//now add a formatter for our DateTime and add it to the ParserOptions

var parserOptions = new ParserOptions(template);
//                                          Value   | Argument| Return
parserOptions.Formatters.AddSingle(new Func<DateTime, string  , string>((value, argument) => {
  //value will be the DateTime object and argument will be the value from Key
  return value.ToString(argument);
}, "ToStringX"));

Parser.ParseWithOptions(parserOptions).CreateRenderer().RenderAndStringify(model); // Friday, September 21, 2018 ish

```

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio?ref=badge_large)
