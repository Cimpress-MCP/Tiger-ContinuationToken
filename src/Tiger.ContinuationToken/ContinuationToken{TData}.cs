// <copyright file="ContinuationToken{TData}.cs" company="Cimpress, Inc.">
//   Copyright 2018 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Swashbuckle.AspNetCore.Annotations;
using Tiger.Types;
using static System.StringComparison;

namespace Tiger.ContinuationToken
{
    /// <summary>Represents an encrypted continuation point of a scan of a dataset.</summary>
    /// <typeparam name="TData">The type of the underlying data.</typeparam>
    [TypeConverter(typeof(TypeConverter))]
    [SwaggerSchemaFilter(typeof(SchemaFilter))]
    public readonly struct ContinuationToken<TData>
        : IEquatable<ContinuationToken<TData>>
    {
        /// <summary>Gets the empty continuation token.</summary>
        public static readonly ContinuationToken<TData> Empty;

        /// <summary>Initializes a new instance of the <see cref="ContinuationToken{TData}"/> struct.</summary>
        /// <param name="value">The value.</param>
        /// <param name="opaqueValue">The original, opaque value.</param>
        public ContinuationToken(TData value, [NotNull] string opaqueValue)
        {
            Value = value;
            OpaqueValue = opaqueValue ?? throw new ArgumentNullException(nameof(opaqueValue));
        }

        /// <summary>Gets the decoded value.</summary>
        public Option<TData> Value { get; }

        /// <summary>Gets the original, opaque value.</summary>
        [CanBeNull]
        public string OpaqueValue { get; }

        /// <summary>
        /// Compares two instances of the <see cref="ContinuationToken{TData}"/> struct for equality.
        /// </summary>
        /// <param name="left">The left value to compare.</param>
        /// <param name="right">The right value to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the instance are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(ContinuationToken<TData> left, ContinuationToken<TData> right) =>
            left.Equals(right);

        /// <summary>
        /// Compares two instances of the <see cref="ContinuationToken{TData}"/> struct for inequality.
        /// </summary>
        /// <param name="left">The left value to compare.</param>
        /// <param name="right">The right value to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the instance are not equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(ContinuationToken<TData> left, ContinuationToken<TData> right) =>
            !(left == right);

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            obj is ContinuationToken<TData> continuationToken && Equals(continuationToken);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + Value.GetHashCode();
                return (hash * 23) + OpaqueValue?.GetHashCode() ?? 0;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => Value.Match(
            none: string.Empty,
            some: v => v.ToString());

        /// <inheritdoc/>
        public bool Equals(ContinuationToken<TData> other) =>
            Option.Equals(Value, other.Value, EqualityComparer<TData>.Default)
            && string.Equals(OpaqueValue, other.OpaqueValue, Ordinal);
    }
}
