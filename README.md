# Morestachio 
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio?ref=badge_shield)

![Icon](https://github.com/JPVenson/morestachio/blob/master/Morestachio/Morestachio%20248x248.png?raw=true)

A Lightweight, powerful, flavorful, templating engine for C# and other .net-based languages. Its a fork of Mustachio.

#### What's this for?

*Morestachio* allows you to create simple text-based templates that are fast and safe to render. It is optimized for WebServers and high on customersation with its Formatter syntax.

#### How to use Morestachio:

```csharp
// Your Template
var sourceTemplate = "Dear {{name}}, this is definitely a personalized note to you. Very truly yours, {{sender}}"

// Parse the Template into the Document Tree. 
var document = Morestachio.Parser.ParseWithOptions(new Morstachio.ParserOptions(sourceTemplate));

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
Stream content = document.Create(model);
content.Stringify(Encoding.Default); // Dear John, this is definitely a personalized note to you. Very truly yours, Sally
```

#### Installing Morestachio:

Morestachio can be installed via [NuGet](https://www.nuget.org/packages/Morestachio/):

```bash
Install-Package Morestachio
```

To get the Extended Formatter Service please install *ether* the 
```bash
Install-Package Morestachio.Formatter.Framework
```
or 

```bash
Install-Package Morestachio.Formatter.Linq
```
I am currently working on the CI process to seperate the builds.

##### Key Features
Morestachio is build upon Mustachio and extends the mustachio syntax in a few ways.

1. each value can be formatted by adding formatter the the morestachio
2. Templates will be parsed as streams and will create a new stream for its output. This is better when creating larger templates and best for web as you can also limit the length of the "to be" created template to a certain size and write the result ether directly to an output stream or the Disc.
3. Morestachio accepts any object as source
4. Cancellation of Template generation is supported
5. Async calls are supported (For Formatters)
6. Minimal Reference count (For .Net only Mirosoft.CSharp.dll & System.dll. Reference to JetBrains.Annotations.dll is optional)
7. NetFramework and NetCore are supported
8. Using of JetBrains Annotations for R# user ( if you are not a R# user just ignore this point )
9. Supports user Encoding of the result template
10. Supports Template Partials `{{#include secondary_template }}`
11. Complex paths are supported `{{ this.is.a.valid.path }}` and `{{ ../this.goes.up.one.level }}` and `{{ ~.this.goes.up.one.level }}`
12. Loops with `#each`
13. Formatters can be declared in C# and be called from the template to provide you with a maximum of freedom
14. The Parser produces a Serilizable Document Tree that can be send to clients to provide a rich user edit experience 
 
**Template partials** ARE a great feature for large scale template development.

You can create a Partial with the `{{#declare NAME}}Partial{{/declare}}` syntax. You can navigate up inside this partials. Partials can also be nested but are currently restricted to a maximum recursion of 255 depth. The programmer has the choice to define a behavior that ether throws an Exception or does nothing and ignores any deeper recusions. 

A Partial must be declared before its usage with `{{#include NAME}}` but you can use a partial to create hirarical templates. 

###### Infos about new features
 
Its possible to use plain C# objects they will be called by reflection. 
Also you can now spezify the excact Size of the template to limit it (this could be come handy if you are in a hostet env) use the `ParserOptions.MaxSize` option to define a max size. It will be enforced on exact that amount of bytes in the stream.

##### Streams
One mayor component is the usage of Streams in morestachio. You can declare a Factory for the streams generated in the `ParserOptions.SourceFactory`. This is very important if you are rendering templates that will be very huge and you want to stream them directly to the harddrive or elsewhere. This has also a very positive effect on the performance as we will not use string concatination for compiling the template. If you do not set the `ParserOptions.SourceFactory` and the `ParserOptions.Encoding`, a memory stream will be created and the `Encoding.Default` will be used.
 
###### Formatter
Use the `ContextObject.DefaultFormatter` collection to create own formatter for all your types or add one to the `ParserOptions.Formatters` object for just one call. To invoke them in your template use the new Function syntax:
```csharp
{{Just.One.Formattable("AnyString").Thing}}
```

The formatter CAN return a new object on wich you can call new Propertys or it can return a string.
There are formatter prepaired for all Primitve types. That means per default you can call on an object hat contains a DateTime:
```csharp
{{MyObject.DateTime("D")}}
```
that will call the `IFormattable` interface on the DateTime. 

**Formatter References** 
Can be used to reference another property/key in the template and then use it in a Formatter. Everything that is not a string (ether prefixed and suffixed with " or ') will be threaded as an expression that also can contain formatter calls
```csharp
{{MyObject.Value(Key)}}
```
This will call a formatter that is resposible for the type that `Value` has and will give it whats in `Key`. Example:
```csharp
//create the template
var template = "{{Value(Key)}}";
//create the model
var model = new Dictionary<string, object>();
model["Value"] = DateTime.Now; 
model["Key"] = "D";
//now add a formatter for our DateTime and add it to the ParserOptions

var parserOptions = new ParserOptions(template);
//                         Value   | Argument| Return
parserOptions.AddFormatter<DateTime, string,   string>((value, argument) => {
  //value will be the DateTime object and argument will be the value from Key
  return value.ToString(argument);
});

Parser.CreateAndStringify(parserOptions); // Friday, September 21, 2018 ish

```
###### Enumerating IDictionarys
Any instance of IDictionary<string,object> is viewed as an object. You cannot enumerate then with #each but you could write a formatter that accepts an Instance of IDictionary and return a List of KeyValuePair and enumerate this new List or enumerate it as every other object with the ? operator like this `{{#each Data.Dictionary.?}} {{/each}}`. 



## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FJPVenson%2Fmorestachio?ref=badge_large)
