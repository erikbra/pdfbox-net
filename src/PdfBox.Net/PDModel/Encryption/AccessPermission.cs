/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/AccessPermission.java
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

namespace PdfBox.Net.PDModel.Encryption;

public class AccessPermission
{
    private const int DefaultPermissions = ~3;
    private const int PrintBit = 3;
    private const int ModificationBit = 4;
    private const int ExtractBit = 5;
    private const int ModifyAnnotationsBit = 6;
    private const int FillInFormBit = 9;
    private const int ExtractForAccessibilityBit = 10;
    private const int AssembleDocumentBit = 11;
    private const int FaithfulPrintBit = 12;

    private int _bytes;
    private bool _readOnly;

    public AccessPermission()
    {
        _bytes = DefaultPermissions;
    }

    public AccessPermission(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length < 4)
        {
            throw new ArgumentException("Permission byte array must contain at least 4 bytes.", nameof(bytes));
        }

        _bytes = 0;
        _bytes |= bytes[0] & 0xFF;
        _bytes <<= 8;
        _bytes |= bytes[1] & 0xFF;
        _bytes <<= 8;
        _bytes |= bytes[2] & 0xFF;
        _bytes <<= 8;
        _bytes |= bytes[3] & 0xFF;
    }

    public AccessPermission(int permissions)
    {
        _bytes = permissions;
    }

    public static AccessPermission GetOwnerAccessPermission()
    {
        AccessPermission permission = new();
        permission.SetCanAssembleDocument(true);
        permission.SetCanExtractContent(true);
        permission.SetCanExtractForAccessibility(true);
        permission.SetCanFillInForm(true);
        permission.SetCanModify(true);
        permission.SetCanModifyAnnotations(true);
        permission.SetCanPrint(true);
        permission.SetCanPrintFaithful(true);
        return permission;
    }

    public bool IsOwnerPermission()
    {
        return CanAssembleDocument() &&
               CanExtractContent() &&
               CanExtractForAccessibility() &&
               CanFillInForm() &&
               CanModify() &&
               CanModifyAnnotations() &&
               CanPrint() &&
               CanPrintFaithful();
    }

    public int GetPermissionBytesForPublicKey()
    {
        SetPermissionBit(1, true);
        SetPermissionBit(7, false);
        SetPermissionBit(8, false);
        for (int i = 13; i <= 32; i++)
        {
            SetPermissionBit(i, false);
        }

        return _bytes;
    }

    public int GetPermissionBytes()
    {
        return _bytes;
    }

    public bool CanPrint() => IsPermissionBitOn(PrintBit);

    public void SetCanPrint(bool allowPrinting)
    {
        if (!_readOnly)
        {
            SetPermissionBit(PrintBit, allowPrinting);
        }
    }

    public bool CanModify() => IsPermissionBitOn(ModificationBit);

    public void SetCanModify(bool allowModifications)
    {
        if (!_readOnly)
        {
            SetPermissionBit(ModificationBit, allowModifications);
        }
    }

    public bool CanExtractContent() => IsPermissionBitOn(ExtractBit);

    public void SetCanExtractContent(bool allowExtraction)
    {
        if (!_readOnly)
        {
            SetPermissionBit(ExtractBit, allowExtraction);
        }
    }

    public bool CanModifyAnnotations() => IsPermissionBitOn(ModifyAnnotationsBit);

    public void SetCanModifyAnnotations(bool allowAnnotationModification)
    {
        if (!_readOnly)
        {
            SetPermissionBit(ModifyAnnotationsBit, allowAnnotationModification);
        }
    }

    public bool CanFillInForm() => IsPermissionBitOn(FillInFormBit);

    public void SetCanFillInForm(bool allowFillingInForm)
    {
        if (!_readOnly)
        {
            SetPermissionBit(FillInFormBit, allowFillingInForm);
        }
    }

    public bool CanExtractForAccessibility() => IsPermissionBitOn(ExtractForAccessibilityBit);

    public void SetCanExtractForAccessibility(bool allowExtraction)
    {
        if (!_readOnly)
        {
            SetPermissionBit(ExtractForAccessibilityBit, allowExtraction);
        }
    }

    public bool CanAssembleDocument() => IsPermissionBitOn(AssembleDocumentBit);

    public void SetCanAssembleDocument(bool allowAssembly)
    {
        if (!_readOnly)
        {
            SetPermissionBit(AssembleDocumentBit, allowAssembly);
        }
    }

    public bool CanPrintFaithful() => IsPermissionBitOn(FaithfulPrintBit);

    public void SetCanPrintFaithful(bool canPrintFaithful)
    {
        if (!_readOnly)
        {
            SetPermissionBit(FaithfulPrintBit, canPrintFaithful);
        }
    }

    public void SetReadOnly()
    {
        _readOnly = true;
    }

    public bool IsReadOnly()
    {
        return _readOnly;
    }

    public bool HasAnyRevision3PermissionSet()
    {
        return CanFillInForm() || CanExtractForAccessibility() || CanAssembleDocument() || CanPrintFaithful();
    }

    private bool IsPermissionBitOn(int bit)
    {
        return (_bytes & (1 << (bit - 1))) != 0;
    }

    private bool SetPermissionBit(int bit, bool value)
    {
        int permissions = _bytes;
        if (value)
        {
            permissions |= 1 << (bit - 1);
        }
        else
        {
            permissions &= ~(1 << (bit - 1));
        }

        _bytes = permissions;
        return (_bytes & (1 << (bit - 1))) != 0;
    }
}
