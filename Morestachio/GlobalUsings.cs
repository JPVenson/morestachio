global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Linq;
global using System.Runtime.CompilerServices;
global using System.Runtime.Serialization;
global using Morestachio.Helper;

#if ValueTask
global using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
global using Promise = System.Threading.Tasks.ValueTask;
global using BooleanPromise = System.Threading.Tasks.ValueTask<bool>;
global using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
global using StringPromise = System.Threading.Tasks.ValueTask<string>;
global using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;

global using CoreActionPromise = System.Threading.Tasks.ValueTask<System.Tuple<Morestachio.Document.Contracts.IDocumentItem, Morestachio.Framework.Context.ContextObject>>;
global using TokenizerResultPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Tokenizing.TokenizerResult>;
global using MorestachioDocumentResultPromise = System.Threading.Tasks.ValueTask<Morestachio.MorestachioDocumentResult>;
global using MorestachioDocumentInfoPromise = System.Threading.Tasks.ValueTask<Morestachio.MorestachioDocumentInfo>;
#else
global using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
global using Promise = System.Threading.Tasks.Task;
global using BooleanPromise = System.Threading.Tasks.Task<bool>;
global using ObjectPromise = System.Threading.Tasks.Task<object>;
global using StringPromise = System.Threading.Tasks.Task<string>;
global using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;

global using CoreActionPromise = System.Threading.Tasks.Task<System.Tuple<Morestachio.Document.Contracts.IDocumentItem, Morestachio.Framework.Context.ContextObject>>;
global using TokenizerResultPromise = System.Threading.Tasks.Task<Morestachio.Framework.Tokenizing.TokenizerResult>;
global using MorestachioDocumentResultPromise = System.Threading.Tasks.Task<Morestachio.MorestachioDocumentResult>;
global using MorestachioDocumentInfoPromise = System.Threading.Tasks.Task<Morestachio.MorestachioDocumentInfo>;
#endif