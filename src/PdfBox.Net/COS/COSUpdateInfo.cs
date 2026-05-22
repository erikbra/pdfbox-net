/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSUpdateInfo.java
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

namespace PdfBox.Net.COS;

public interface COSUpdateInfo : COSObjectable
{
    /// <summary>
    /// Get the update state for the COSWriter. This indicates whether an object is to be written
    /// when there is an incremental save.
    /// </summary>
    /// <returns>The update state.</returns>
    bool IsNeedToBeUpdated()
    {
        return GetUpdateState().IsUpdated();
    }

    /// <summary>
    /// Set the update state of the dictionary for the COSWriter. This indicates whether an object is
    /// to be written when there is an incremental save.
    /// </summary>
    /// <param name="flag">The update state.</param>
    void SetNeedToBeUpdated(bool flag)
    {
        GetUpdateState().Update(flag);
    }

    /// <summary>
    /// Uses this <see cref="COSUpdateInfo"/> as the base object of a new <see cref="COSIncrement"/>.
    /// </summary>
    /// <returns>A <see cref="COSIncrement"/> based on this <see cref="COSUpdateInfo"/>.</returns>
    COSIncrement ToIncrement()
    {
        return GetUpdateState().ToIncrement();
    }

    /// <summary>
    /// Returns the current <see cref="COSUpdateState"/> of this <see cref="COSUpdateInfo"/>.
    /// </summary>
    /// <returns>The current <see cref="COSUpdateState"/> of this <see cref="COSUpdateInfo"/>.</returns>
    COSUpdateState GetUpdateState();
}

public static class COSUpdateInfoExtensions
{
    public static bool IsNeedToBeUpdated(this COSUpdateInfo updateInfo)
    {
        return updateInfo.GetUpdateState().IsUpdated();
    }

    public static void SetNeedToBeUpdated(this COSUpdateInfo updateInfo, bool flag)
    {
        updateInfo.GetUpdateState().Update(flag);
    }

    public static COSIncrement ToIncrement(this COSUpdateInfo updateInfo)
    {
        return updateInfo.GetUpdateState().ToIncrement();
    }
}
