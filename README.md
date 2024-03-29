# Tiger.ContinuationToken

## What It Is

Tiger.ContinuationToken is an ASP.NET Core library for producing encoded continuation tokens for pagination in a RESTful web API. A "continuation token" may be referred to in various places around the internet as an "offset", a "pagination token", or a "value we have to provide so that the _@#$^!%_ service doesn't start from the beginning every time we query it."

## Why You Want It

In order for a particular scan of a paginated collection resource in a RESTful web API to pick up where it left off, the client has to give to the server some representation of the last thing it saw. If these representations are guessable (say, an auto-incrementing ID), a client may start trying to synthesize them -- sending queries that start from any location, rather than starting at the start and continuing through each of the pages. Tiger.ContinuationToken creates these representations in such a format that they are opaque to clients, but a server application can decode them to provide the pagination functionality. These can then be used in URLs returned via the `Link` header with a `rel` value of "next", for example.

## How to Use It

Begin by configuring the ASP.NET Core Data Protection API. This is typically done in the `Configure` method of the application's startup class. Many deployment strategies can accept the default, but AWS Lambda Functions (or anything like them) should consider using a store that is outside of any particular compute environment. A database, Redis, SSM Parameter store, all of these are possibilities.

Once the protection has been configured, the tokens' model binder can be made active in the `ConfigureServices` method of an ASP.NET Core application after a call to `AddMvc`.

```csharp
services.AddMvc().AddContinuationTokens();
```

The configuration framework wiil, by default, look for the ARN of a KMS key at the configuration path `ContinuationToken:KmsKeyArn`. An alternative section name (that is, the `ContinuationToken` part of the default value) may be provided as an argument to `AddContinuationTokens`. An implmenting service will require the following permissions on the provided key:

- `kms:Decrypt`
- `kms:Encrypt`
- `kms:GenerateDataKey`

If desired, permissions can be narrowed further by ensuring that the encyption context has the following values via an IAM `Condition`:

- "Environment": `DOTNET_ENVIRONMENT`
- "Purpose": "Tiger.ContinuationToken"

The type may now be used as a parameter to a controller action, as demonstrated here with an action which retrieved a page of Pool entities:

```csharp
public async Task<ActionResult<Dto.PoolPage>> GetPools(
  [FromQuery] ContinuationToken<DateTimeOffset> offset,
  [FromQuery, Range(1, 25)] int? limit,
  CancellationToken cancellationToken = default) { /* ... */ }
```

A limitation of the library is that it can only encode and decode values of types that can be converted to and from strings. It checks for this functionality by ensuring that the type's type converter supports such conversion by calling the method `CanConvertFrom` with an argument of `typeof(string)`. Nearly every C# primitive implements this in a reasonable way, and the library provides support for `DateTimeOffset`, which does not. Custom types can opt into such convertability via the `TypeConverterAttribute`. For example, a cartesian coordinate could convert to the string value "2,3", which would then be encoded. (It's recommended to keep the string representations produced by such type converters terse, as they're not intended to be human-produced or -readable.) That said, this library considers it unlikely that the decoded type of a continuation token will fall outside of this collection of types:

- `DateTimeOffset`
- `int`
- `long`
- `Guid`
- `string`

## Thank You

Seriously, though. Thank you for using this software. The author hopes it performs admirably for you.
