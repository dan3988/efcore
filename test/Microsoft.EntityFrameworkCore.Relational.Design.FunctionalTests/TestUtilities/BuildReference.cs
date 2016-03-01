// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
#if NETSTANDARDAPP1_5
using Microsoft.Extensions.DependencyModel;
#endif

namespace Microsoft.EntityFrameworkCore.Relational.Design.FunctionalTests.TestUtilities
{
    public class BuildReference
    {
#if NETSTANDARDAPP1_5
        private static readonly DependencyContext DefaultDependencyContext =
            DependencyContext.Load(typeof(BuildReference).GetTypeInfo().Assembly);
#endif

        private BuildReference(IEnumerable<MetadataReference> references, bool copyLocal = false, string path = null)
        {
            References = references;
            CopyLocal = copyLocal;
            Path = path;
        }

        public IEnumerable<MetadataReference> References { get; }

        public bool CopyLocal { get; }
        public string Path { get; }

        public static BuildReference ByName(string name, bool copyLocal = false, Assembly depContextAssembly = null)
        {
#if NETSTANDARDAPP1_5
            var depContext = depContextAssembly == null
                ? DefaultDependencyContext
                : DependencyContext.Load(depContextAssembly);

            if (depContext != null)
            {
                var library = depContext
                    .CompileLibraries
                    .FirstOrDefault(l => l.PackageName.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (library != null)
                {
                    return new BuildReference(
                        library.ResolveReferencePaths().Select(file => MetadataReference.CreateFromFile(file)),
                        copyLocal);
                }
            }
#else
            var assembly = Assembly.Load(name);
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                return new BuildReference(
                    new[] { MetadataReference.CreateFromFile(assembly.Location) },
                    copyLocal,
                    new Uri(assembly.CodeBase).LocalPath);
            }
#endif

            throw new InvalidOperationException(
                $"Assembly '{name}' not found.");
        }

        public static BuildReference ByPath(string path)
            => new BuildReference(new[] { MetadataReference.CreateFromFile(path) }, path: path);
    }
}
