/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSObject.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

using System.Diagnostics;

namespace PdfBox.Net.COS;

/// <summary>
/// This class represents a PDF object.
/// </summary>
/// <remarks>Author: Ben Litchfield</remarks>
public partial class COSObject : COSBase, COSUpdateInfo
{
    private COSBase? _baseObject;
    private ICOSParser? _parser;
    private bool _isDereferenced;
    private readonly COSUpdateState _updateState;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="obj">The object that this encapsulates.</param>
    public COSObject(COSBase? obj)
    {
        _updateState = new(this);
        _baseObject = obj;
        _isDereferenced = true;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="obj">The object that this encapsulates.</param>
    /// <param name="objectKey">The COSObjectKey of the encapsulated object.</param>
    public COSObject(COSBase? obj, COSObjectKey objectKey)
        : this(objectKey, null)
    {
        _baseObject = obj;
        _isDereferenced = true;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="obj">The object that this encapsulates.</param>
    /// <param name="parser">The parser to be used to load the object on demand.</param>
    public COSObject(COSBase? obj, ICOSParser? parser)
    {
        _updateState = new(this);
        _baseObject = obj;
        _isDereferenced = obj is not null;
        _parser = parser;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="key">The object number of the encapsulated object.</param>
    public COSObject(COSObjectKey key)
        : this(key, null)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="key">The object number of the encapsulated object.</param>
    /// <param name="parser">The parser to be used to load the object on demand.</param>
    public COSObject(COSObjectKey key, ICOSParser? parser)
    {
        _updateState = new(this);
        _parser = parser;
        SetKey(key);
    }

    /// <summary>
    /// Indicates if the referenced object is present or not.
    /// </summary>
    /// <returns><c>true</c> if the indirect object is null.</returns>
    public bool IsObjectNull()
    {
        return _baseObject is null;
    }

    /// <summary>
    /// This will get the object that this object encapsulates.
    /// </summary>
    /// <returns>The encapsulated object.</returns>
    public COSBase? GetObject()
    {
        if (!_isDereferenced && _parser is not null)
        {
            try
            {
                // mark as dereferenced to avoid endless recursions
                _isDereferenced = true;
                _baseObject = _parser.DereferenceCOSObject(this);
                _updateState.DereferenceChild(_baseObject);
            }
            catch (IOException e)
            {
                Debug.WriteLine($"[ERROR] Can't dereference {this}: {e.Message}");
            }
            finally
            {
                _parser = null;
            }
        }

        return _baseObject;
    }

    /// <summary>
    /// Sets the base object directly, updating the dereference state.
    /// </summary>
    /// <param name="baseObject">The new base object.</param>
    /// <remarks>This is a .NET adaptation — used by the parser to populate a pre-created shell.</remarks>
    public void SetObject(COSBase? baseObject)
    {
        if (!ReferenceEquals(_baseObject, baseObject))
        {
            _updateState.Update(baseObject);
        }

        _baseObject = baseObject;
        _isDereferenced = baseObject is not null;
        _parser = null;
    }

    /// <summary>
    /// Sets the referenced object to COSNull and removes the initially assigned parser.
    /// </summary>
    public void SetToNull()
    {
        if (_baseObject is not null)
        {
            _updateState.Update();
        }

        _baseObject = COSNull.NULL;
        _parser = null;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"COSObject{{{GetKey()}}}";
    }

    /// <summary>
    /// Visitor pattern double dispatch method.
    /// </summary>
    /// <param name="visitor">The object to notify when visiting this object.</param>
    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromObject(this);
    }

    /// <summary>
    /// Returns <c>true</c> if the hereby referenced <see cref="COSBase"/> has already been parsed and loaded.
    /// </summary>
    /// <returns><c>true</c> if the hereby referenced <see cref="COSBase"/> has already been parsed and loaded.</returns>
    public bool IsDereferenced()
    {
        return _isDereferenced;
    }

    /// <summary>
    /// Returns the current <see cref="COSUpdateState"/> of this <see cref="COSObject"/>.
    /// </summary>
    /// <returns>The current <see cref="COSUpdateState"/> of this <see cref="COSObject"/>.</returns>
    public COSUpdateState GetUpdateState()
    {
        return _updateState;
    }
}
