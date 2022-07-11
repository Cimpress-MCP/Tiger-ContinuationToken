// <copyright file="ContinuationToken{TData}.cs" company="Cimpress, Inc.">
//   Copyright 2020–2022 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

using Swashbuckle.AspNetCore.Annotations;
using Tiger.Types;

namespace Tiger.ContinuationToken;

/// <summary>Represents an encrypted continuation point of a scan of a dataset.</summary>
/// <typeparam name="TData">The type of the underlying data.</typeparam>
/// <param name="Value">The decoded value.</param>
/// <param name="OpaqueValue">The original, opaque value.</param>
[TypeConverter(typeof(TypeConverter))]
[SwaggerSchemaFilter(typeof(SchemaFilter))]
public readonly record struct ContinuationToken<TData>(Option<TData> Value, string OpaqueValue)
    where TData : notnull
{
    /// <summary>Gets the empty continuation token.</summary>
    public static readonly ContinuationToken<TData> Empty;

    /// <inheritdoc/>
    public override string ToString() => Value.Match(
        none: string.Empty,
        some: v => v.ToString() ?? string.Empty);
}
