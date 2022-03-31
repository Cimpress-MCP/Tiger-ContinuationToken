// <copyright file="SchemaFilter.cs" company="Cimpress, Inc.">
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

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tiger.ContinuationToken;

/// <summary>
/// Modifies the OpenAPI schema for the <see cref="ContinuationToken{TData}"/> struct.
/// </summary>
sealed class SchemaFilter
    : ISchemaFilter
{
    /// <inheritdoc/>
    void ISchemaFilter.Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        schema.Type = "string";

        /* note(cosborn)
         * I don't think that this is a real format, but it should convince humans
         * that this isn't a thing that they can get data from by parsing or whatnot.
         */
        schema.Format = "opaque";
    }
}
