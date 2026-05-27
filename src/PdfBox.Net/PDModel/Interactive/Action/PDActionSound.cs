/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionSound.java
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// This represents a sound action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionSound</c>.</remarks>
public class PDActionSound : PDAction
{
    private static readonly COSName SoundName = COSName.GetPDFName("Sound");
    private static readonly COSName VolumeName = COSName.GetPDFName("Volume");
    private static readonly COSName SynchronousName = COSName.GetPDFName("Synchronous");
    private static readonly COSName RepeatName = COSName.GetPDFName("Repeat");
    private static readonly COSName MixName = COSName.GetPDFName("Mix");

    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "Sound";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionSound()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionSound(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// Sets the sound object defining the sound that shall be played.
    /// </summary>
    public void SetSound(COSStream? sound)
    {
        action.SetItem(SoundName, sound);
    }

    /// <summary>
    /// Gets the sound object defining the sound that shall be played.
    /// </summary>
    public COSStream? GetSound()
    {
        return action.GetCOSStream(SoundName);
    }

    /// <summary>
    /// Sets the volume at which to play the sound, in the range -1.0 to 1.0.
    /// </summary>
    /// <exception cref="ArgumentException">If the volume parameter is outside the valid range.</exception>
    public void SetVolume(float volume)
    {
        if (volume < -1 || volume > 1)
        {
            throw new ArgumentException("volume outside of the range -1.0 to 1.0");
        }
        action.SetFloat(VolumeName, volume);
    }

    /// <summary>
    /// Gets the volume at which to play the sound, in the range -1.0 to 1.0.
    /// </summary>
    public float GetVolume()
    {
        float volume = action.GetFloat(VolumeName, 1f);
        return volume < -1 || volume > 1 ? 1 : volume;
    }

    /// <summary>
    /// Sets whether to play the sound synchronously.
    /// </summary>
    public void SetSynchronous(bool synchronous)
    {
        action.SetBoolean(SynchronousName, synchronous);
    }

    /// <summary>
    /// Gets whether to play the sound synchronously.
    /// </summary>
    public bool GetSynchronous()
    {
        return action.GetBoolean(SynchronousName, false);
    }

    /// <summary>
    /// Sets whether to repeat the sound indefinitely.
    /// </summary>
    public void SetRepeat(bool repeat)
    {
        action.SetBoolean(RepeatName, repeat);
    }

    /// <summary>
    /// Gets whether to repeat the sound indefinitely.
    /// </summary>
    public bool GetRepeat()
    {
        return action.GetBoolean(RepeatName, false);
    }

    /// <summary>
    /// Sets whether to mix this sound with any other sound already playing.
    /// </summary>
    public void SetMix(bool mix)
    {
        action.SetBoolean(MixName, mix);
    }

    /// <summary>
    /// Gets whether to mix this sound with any other sound already playing.
    /// </summary>
    public bool GetMix()
    {
        return action.GetBoolean(MixName, false);
    }
}

