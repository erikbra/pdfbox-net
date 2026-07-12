/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/SecurityHandlerFactory.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Diagnostics.CodeAnalysis;

namespace PdfBox.Net.PDModel.Encryption;

public sealed class SecurityHandlerFactory
{
    public static readonly SecurityHandlerFactory INSTANCE = new();

    private readonly Dictionary<string, HandlerRegistration> _nameToHandler = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, HandlerRegistration> _policyToHandler = [];

    private SecurityHandlerFactory()
    {
        RegisterHandler(StandardSecurityHandler.FILTER, typeof(StandardSecurityHandler), typeof(StandardProtectionPolicy));
        RegisterHandler(PublicKeySecurityHandler.FILTER, typeof(PublicKeySecurityHandler), typeof(PublicKeyProtectionPolicy));
    }

    public void RegisterHandler(
        string name,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type securityHandler,
        Type protectionPolicy)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(securityHandler);
        ArgumentNullException.ThrowIfNull(protectionPolicy);

        if (!typeof(ProtectionPolicy).IsAssignableFrom(protectionPolicy))
        {
            throw new ArgumentException("Protection policy type must derive from ProtectionPolicy.", nameof(protectionPolicy));
        }

        if (_nameToHandler.ContainsKey(name))
        {
            throw new InvalidOperationException("The security handler name is already registered.");
        }

        HandlerRegistration registration = new(securityHandler);
        _nameToHandler[name] = registration;
        _policyToHandler[protectionPolicy] = registration;
    }

    public SecurityHandler<ProtectionPolicy>? NewSecurityHandlerForPolicy(ProtectionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (!_policyToHandler.TryGetValue(policy.GetType(), out HandlerRegistration? registration))
        {
            return null;
        }

        return NewSecurityHandler(registration.HandlerType, [policy.GetType()], [policy]);
    }

    public SecurityHandler<ProtectionPolicy>? NewSecurityHandlerForFilter(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (!_nameToHandler.TryGetValue(name, out HandlerRegistration? registration))
        {
            return null;
        }

        return NewSecurityHandler(registration.HandlerType, Type.EmptyTypes, []);
    }

    private static SecurityHandler<ProtectionPolicy> NewSecurityHandler(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerClass,
        Type[] argsClasses,
        object[] args)
    {
        try
        {
            object? instance = handlerClass.GetConstructor(argsClasses)?.Invoke(args);
            if (instance is SecurityHandler<ProtectionPolicy> handler)
            {
                return handler;
            }

            throw new InvalidOperationException($"Security handler '{handlerClass.FullName}' has incompatible type.");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Failed to create security handler instance.", ex);
        }
    }

    private sealed class HandlerRegistration
    {
        public HandlerRegistration(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
        {
            HandlerType = handlerType;
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        public Type HandlerType { get; }
    }
}
