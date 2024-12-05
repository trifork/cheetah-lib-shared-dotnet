/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using OpenSearch.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client.CheetahJsonSerializer
{
    /// <summary>
    /// 
    /// </summary>
    public class CheetahJsonSerializer : ConnectionSettingsAwareSerializerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builtinSerializer"></param>
        /// <param name="connectionSettings"></param>
        /// <param name="jsonSerializerOptionsFactory"></param>
        /// <param name="modifyContractResolver"></param>
        /// <param name="contractJsonConverters"></param>
        public CheetahJsonSerializer(
            IOpenSearchSerializer builtinSerializer,
            IConnectionSettingsValues connectionSettings,
            Func<JsonSerializerOptions>? jsonSerializerOptionsFactory = null,
            Action<ConnectionSettingsAwareContractResolver>? modifyContractResolver = null,
            IEnumerable<JsonConverter>? contractJsonConverters = null
        )
            : base(builtinSerializer, connectionSettings, jsonSerializerOptionsFactory ?? (() => new JsonSerializerOptions()), modifyContractResolver ?? (_ => { }), contractJsonConverters ?? Array.Empty<JsonConverter>()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builtin"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IOpenSearchSerializer Default(IOpenSearchSerializer builtin, IConnectionSettingsValues values)
        {
            return new CheetahJsonSerializer(builtin, values);
        }

    }
}