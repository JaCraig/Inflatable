/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Microsoft.Extensions.ObjectPool;
using System;
using System.Text;

namespace Inflatable.Aspect.Interfaces
{
    /// <summary>
    /// Interface implementation helper
    /// </summary>
    public interface IInterfaceImplementationHelper
    {
        /// <summary>
        /// Setups the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="aspect">The aspect.</param>
        /// <param name="objectPool">The object pool.</param>
        /// <returns>The resulting code in string format.</returns>
        string Setup(Type type, ORMAspect aspect, ObjectPool<StringBuilder> objectPool);
    }
}