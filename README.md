# Morestachio 
![](https://img.shields.io/osslifecycle/JPVenson/morestachio)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio?ref=badge_shield)
[![Converage](https://img.shields.io/azure-devops/coverage/jeanpierrebachmann/Morestachio/3?label=Test%20Coverage)](https://jeanpierrebachmann.visualstudio.com/Morestachio/_build?definitionId=3&view=ms.vss-pipelineanalytics-web.new-build-definition-pipeline-analytics-view-cardmetrics)
![tests](https://img.shields.io/azure-devops/tests/jeanpierrebachmann/Morestachio/3)


![Icon](https://github.com/JPVenson/morestachio/blob/master/Morestachio/Morestachio%20248x248.png?raw=true)

A Lightweight, powerful, flavorful, templating engine for C# and other .net-based languages. Its a fork of Mustachio.


## Need help?
Need general help? open a [Discussion](https://github.com/JPVenson/morestachio/discussions/new)   
Found a bug? open a [Bug](https://github.com/JPVenson/morestachio/issues/new?assignees=&labels=&template=bug_report.md)   

#### Installing Morestachio:


|Project Nuget|Github|Status|Description|
|---|---|---|---|
| Morestachio <br /> [![Nuget Morestachio](https://img.shields.io/nuget/v/Morestachio?label=Morestachio)](https://www.nuget.org/packages/Morestachio/) ![](https://img.shields.io/nuget/dt/morestachio)  | [![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/JPVenson/morestachio?include_prereleases)](https://github.com/JPVenson/morestachio/releases) | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3) [![Deployment Status](https://jeanpierrebachmann.vsrm.visualstudio.com/_apis/public/Release/badge/9c3dc1dc-4391-4970-b520-1713860906f7/1/1)](https://jeanpierrebachmann.visualstudio.com/Morestachio/_release?view=all&_a=releases&definitionId=1)| The base Morestachio lib |
| Morestachio.Linq <br /> [![Nuget Morestachio Linq](https://img.shields.io/nuget/v/Morestachio.Linq?label=%20Morestachio.Linq)](https://www.nuget.org/packages/Morestachio.Linq/) |   | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3) [![Deployment Status](https://jeanpierrebachmann.vsrm.visualstudio.com/_apis/public/Release/badge/9c3dc1dc-4391-4970-b520-1713860906f7/1/1)](https://jeanpierrebachmann.visualstudio.com/Morestachio/_release?view=all&_a=releases&definitionId=1) | [Linq formatter](https://github.com/JPVenson/morestachio/wiki/Predefined-Formatter#class-dynamiclinq) |
| Morestachio.Runner <br /> [![Nuget Morestachio Runner](https://img.shields.io/nuget/v/Morestachio.Runner?label=%20Morestachio.Runner)](https://www.nuget.org/packages/Morestachio.Runner/) |   | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3) [![Deployment Status](https://jeanpierrebachmann.vsrm.visualstudio.com/_apis/public/Release/badge/9c3dc1dc-4391-4970-b520-1713860906f7/1/1)](https://jeanpierrebachmann.visualstudio.com/Morestachio/_release?view=all&_a=releases&definitionId=1) | An [executable interface](https://github.com/JPVenson/morestachio/wiki/Morestachio-Runner) for invoking a Morestachio Template |
| Morestachio.Newtonsoft.Json <br /> [![Nuget Morestachio Json](https://img.shields.io/nuget/v/Morestachio.Newtonsoft.Json?label=Morestachio.Newtonsoft.Json)](https://www.nuget.org/packages/Morestachio.Newtonsoft.Json/) |  | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3) [![Deployment Status](https://jeanpierrebachmann.vsrm.visualstudio.com/_apis/public/Release/badge/9c3dc1dc-4391-4970-b520-1713860906f7/1/1)](https://jeanpierrebachmann.visualstudio.com/Morestachio/_release?view=all&_a=releases&definitionId=1) | Newtonsoft Json [types support](https://github.com/JPVenson/morestachio/wiki/Newtonsoft.Json-support) |
| Morestachio.System.Text.Json <br /> [![Nuget Morestachio System.Text.Json](https://img.shields.io/nuget/v/Morestachio.System.Text.Json?label=Morestachio.System.Text.Json)](https://www.nuget.org/packages/Morestachio.System.Text.Json/) |  | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3) [![Deployment Status](https://jeanpierrebachmann.vsrm.visualstudio.com/_apis/public/Release/badge/9c3dc1dc-4391-4970-b520-1713860906f7/1/1)](https://jeanpierrebachmann.visualstudio.com/Morestachio/_release?view=all&_a=releases&definitionId=1) | System.Text Json [types support](https://github.com/JPVenson/morestachio/wiki/System.Text.Json-support) |
| Morestachio.System.Xml.Linq <br /> [![Nuget Morestachio.System.Xml.Linq](https://img.shields.io/nuget/v/Morestachio.System.Xml.Linq?label=Morestachio.System.Xml.Linq)](https://www.nuget.org/packages/Morestachio.System.Xml.Linq/) |  | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3) [![Deployment Status](https://jeanpierrebachmann.vsrm.visualstudio.com/_apis/public/Release/badge/9c3dc1dc-4391-4970-b520-1713860906f7/1/1)](https://jeanpierrebachmann.visualstudio.com/Morestachio/_release?view=all&_a=releases&definitionId=1) | XDocument [types support](https://github.com/JPVenson/morestachio/wiki/System.Xml-support) |
| Morestachio.Extensions.Logging <br /> [![Nuget Morestachio.Extensions.Logging](https://img.shields.io/nuget/v/Morestachio.Extensions.Logging?label=Morestachio.Extensions.Logging)](https://www.nuget.org/packages/Morestachio.Morestachio.Extensions.Logging/) |  | [![Build status](https://dev.azure.com/JeanPierreBachmann/Mustachio/_apis/build/status/Morestachio-CI%20DotNet)](https://dev.azure.com/JeanPierreBachmann/Mustachio/_build/latest?definitionId=3) [![Deployment Status](https://jeanpierrebachmann.vsrm.visualstudio.com/_apis/public/Release/badge/9c3dc1dc-4391-4970-b520-1713860906f7/1/1)](https://jeanpierrebachmann.visualstudio.com/Morestachio/_release?view=all&_a=releases&definitionId=1) | Microsoft.Extensions.Logging.ILogger [support]([https://github.com/JPVenson/morestachio/wiki/System.Xml-support](https://github.com/JPVenson/morestachio/wiki/Microsoft.Extensions.Logging)) |

#### What's this for?

*Morestachio* allows you to create simple text-based templates that are fast and safe to render. It is optimized for WebServers and offers a high degree of customization with its Formatter syntax.

#### Morestachio Playground:
Try it out, without consequenses. The Morestachio Online editor allows you to create templates within your browser:
[Editor](https://morestachio.jean-pierre-bachmann.dev/)

#### How to use Morestachio:

```csharp
// Your Template
var sourceTemplate = "Dear {{name}}, this is definitely a personalized note to you. Very truly yours, {{sender}}";

// Parse the Template into the Document Tree. 
var document = await ParserOptionsBuilder
   .New() //creates a new builder that inherts all default values
   .WithTemplate(sourceTemplate) //sets the template for that builder
   .BuildAndParseAsync(); //Builds the template and calls ParseAsync() on the returned ParserOptions

// Create the values for the template model:
dynamic model = new ExpandoObject();
model.name = "John";
model.sender = "Sally";
// or with dictionarys
IDictionary model = new Dictionary<string, object>();
model["name"] = "John";
model["sender"] = "Sally";
//or with any other object
var model = new {name= "John", sender= "Sally"};

// Combine the model with the template to get content:
var content = document.CreateRenderer().RenderAndStringify(model); // Dear John, this is definitely a personalized note to you. Very truly yours, Sally
```


##### Key Features
Morestachio is build upon Mustachio and extends the mustachio syntax in a a lot of points.

1. each object can be formatted by adding [formatter](https://github.com/JPVenson/morestachio/wiki/Formatter) to morestachio
2. Templates will be parsed as [streams](https://github.com/JPVenson/morestachio/wiki/ParserOptions#template-itemplatecontainer-property) and will create a new stream for its [output](). This is better when creating larger templates and best for web as you can also [limit the length](https://github.com/JPVenson/morestachio/wiki/ParserOptions#maxsize-long-property) of the "to be" created template to a certain size and write the result ether directly to an output stream or the Disc.
3. [Its Lightning fast](https://github.com/JPVenson/morestachio/wiki/Performance). Even unreasonably huge templates that contain >5000 instructions can be executed in around *0.5 secounds*
4. Morestachio accepts any object as source
5. [Cancellation](https://github.com/JPVenson/morestachio/wiki/ParserOptions#timeout-timespan-property) of Template generation is supported
6. Async calls are supported (For Formatters)
7. No External Depedencies for Morestachio.dll.
8. Support for several .Net framworks:
   - NetStandard (netstandard2.0; netstandard2.1; see [.NET implementation support](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support)) 
   - Net5.0, Net6.0
9. Build in [Localization support](https://github.com/JPVenson/morestachio/wiki/Localisation) and [Logging support](https://github.com/JPVenson/morestachio/wiki/ParserOptions#logger-ilogger-property)
10. Supports user [Encoding](https://github.com/JPVenson/morestachio/wiki/ParserOptions#encoding-encoding-property) of the result template
11. Supports [Template Partials](https://github.com/JPVenson/morestachio/wiki/Templates-Partials) `{{#import 'secondary_template' }}`
12. [Complex paths](https://github.com/JPVenson/morestachio/wiki/Keywords#paths) are supported `{{ this.is.a.valid.path }}` and `{{ ../this.goes.up.one.level }}` and `{{ ~.this.goes.up.to.Root }}`
13. [Loops](https://github.com/JPVenson/morestachio/wiki/Keywords#do--while--repeat---blocks) with `#each` & `#do` & `#while` & `#repeat`
14. [Object Enumeration](https://github.com/JPVenson/morestachio/wiki/Keywords#each--every---blocks) with `#each data.?`
15. Formatters can be declared in C# and be called from the template to provide you with a maximum of freedom
16. Extensive (275) [Build-In list of Formatters](https://github.com/JPVenson/morestachio/wiki/Predefined-Formatter) for a broad usecases
17. The Parser produces a Serilizable [Document Tree](https://github.com/JPVenson/morestachio/wiki/Document-Tree) that can be send to clients to provide a rich user edit experience 

Checkout the Github Wiki for an extensive documentation:
https://github.com/JPVenson/morestachio/wiki
 
**Template partials** ARE a great feature for large scale template development.

You can create a Partial with the `{{#declare NAME}}Partial{{/declare}}` syntax. You can navigate up inside this partials. Partials can also be nested but are currently restricted to a maximum recursion of 255 depth. The programmer has the choice to define a behavior that ether throws an Exception or does nothing and ignores any deeper recusions.    

A Partial must be declared before its usage with `{{#import 'NAME'}}` but you can use a partial to create hirarical templates.    

You can even inject your predefined Partials into all of your Templates by utilizing the `PartialsStore`. Use your own `IPartialStore` or a build in one with `ParserOptionsBuilder.WithDefaultPartialStore(store => {...add partials to store...})`.

###### Infos about new features
 
Its possible to use plain C# objects they will be called by reflection. 
Also you can now set the excact size of the template to limit it (this could be come handy if you are in a hostet environment) use the `ParserOptionsBuilder.WithMaxSize()` option to define a max size. It will be enforced on exact that amount of bytes in the stream.

##### Variable Output
One mayor component is the usage of Variable output strategies in morestachio.    
The output handling is done by a `IByteCounterStream` that wraps your specific output. This can ether be a `Stream`, `TextWriter`, `StringBuilder` or anything else. For thoese types Morestachio has pre defined Implementations named `ByteCounterStream`, `ByteCounterTextWriter` and `ByteCounterStringBuilder`. All thoese types are enforcing the `ParserOptionsBuilder.WithMaxSize()` config if set and will write your template with the set `ParserOptionsBuilder.WithEncoding()`
 
###### Formatter
With Morestachio you can invoke C# methods from you template, so called 'Formatters'. There are [Build in formatters](https://github.com/JPVenson/morestachio/wiki/Predefined-Formatter) you can call in any template, registered via the `DefaultFormatterService.Default` class. When you add a formatter in the default service, it will be availible in every template. You can also add formatters per-template via the `ParserOptionsBuilder.WithFormatters` service.

To Invoke a formatter from you template use the Function syntax:
```csharp
{{Just.One.Formattable.FormatterToCall().Thing}}
```
This links a c# function named "FormatterToCall".

You can register delegates by using `ParserOptionsBuilder.WithFormatter(...)` or you can create a `public static class` that has methods attributed with the `MorestachioFormatterAttribute` and add them via the `ParserOptionsBuilder.WithFormatters<TType>` or you can use an instance method attributed with the `MorestachioFormatterAttribute`.

The formatter CAN return a new object on wich you can call new Propertys or it can return a string.
There are formatter prepaired for types implementing the `IFormattable` interface. This includes all Primitve types. That means for example that you can call the `ToString` formatter on any `DateTime`:
```csharp
{{MyObject.DateTime.ToString("D")}} <-- this will resolve a property "MyObject" and then "DateTime" and will call ToString on it with the argument "D"
```

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

var document = await ParserOptionsBuilder
   .New()
   .WithTemplate(template)
//                         Value   | Argument | Return
   .WithFormatter(new Func<DateTime, string   , string>((value, argument) => {
     //value will be the DateTime object and argument will be the value from Key
     return value.ToString(argument);
   }, "ToStringX")
   .BuildAndParseAsync();

document.CreateRenderer().RenderAndStringify(model); // Friday, September 21, 2018 ish

```

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio?ref=badge_large)
