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

using Inflatable.Interfaces;
using System.Reflection;
using System.Text;

namespace Inflatable.Aspect.Interfaces
{
    /// <summary>
    /// End method helper
    /// </summary>
    public interface IEndMethodHelper
    {
        /// <summary>
        /// Setups the specified return value name.
        /// </summary>
        /// <param name="returnValueName">Name of the return value.</param>
        /// <param name="method">The method.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="builder">The builder.</param>
        void Setup(string returnValueName, MethodInfo method, IMapping mapping, StringBuilder builder);
    }
}