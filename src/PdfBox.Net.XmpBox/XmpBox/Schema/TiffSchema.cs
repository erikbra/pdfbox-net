/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/TiffSchema.java
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

using PdfBox.Net.XmpBox.Type;

namespace PdfBox.Net.XmpBox.Schema;

[StructuredType("http://ns.adobe.com/tiff/1.0/", "tiff")]
public class TiffSchema : XMPSchema
{
    public const string NamespaceUri = "http://ns.adobe.com/tiff/1.0/";
    public const string PreferredPrefix = "tiff";

    [PropertyType(XmpTypeName.LangAlt)]
    public static readonly string IMAGE_DESCRIPTION = "ImageDescription";

    [PropertyType(XmpTypeName.LangAlt)]
    public static readonly string COPYRIGHT = "Copyright";

    [PropertyType(XmpTypeName.ProperName)]
    public static readonly string ARTIST = "Artist";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string IMAGE_WIDTH = "ImageWidth";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string IMAGE_LENGTH = "ImageLength";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string COMPRESSION = "Compression";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string PHOTOMETRIC_INTERPRETATION = "PhotometricInterpretation";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string ORIENTATION = "Orientation";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string SAMPLES_PER_PIXEL = "SamplesPerPixel";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string PLANAR_CONFIGURATION = "PlanarConfiguration";

    [PropertyType(XmpTypeName.Integer, Cardinality.Seq)]
    public static readonly string YCB_CR_SUB_SAMPLING = "YCbCrSubSampling";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string YCB_CR_POSITIONING = "YCbCrPositioning";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string XRESOLUTION = "XResolution";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string YRESOLUTION = "YResolution";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string RESOLUTION_UNIT = "ResolutionUnit";

    [PropertyType(XmpTypeName.Integer, Cardinality.Seq)]
    public static readonly string TRANSFER_FUNCTION = "TransferFunction";

    [PropertyType(XmpTypeName.Rational, Cardinality.Seq)]
    public static readonly string WHITE_POINT = "WhitePoint";

    [PropertyType(XmpTypeName.Rational, Cardinality.Seq)]
    public static readonly string PRIMARY_CHROMATICITIES = "PrimaryChromaticities";

    [PropertyType(XmpTypeName.Rational, Cardinality.Seq)]
    public static readonly string YCB_CR_COEFFICIENTS = "YCbCrCoefficients";

    [PropertyType(XmpTypeName.Rational, Cardinality.Seq)]
    public static readonly string REFERENCE_BLACK_WHITE = "ReferenceBlackWhite";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string DATE_TIME = "DateTime";

    [PropertyType(XmpTypeName.AgentName)]
    public static readonly string SOFTWARE = "Software";

    [PropertyType(XmpTypeName.ProperName)]
    public static readonly string MAKE = "Make";

    [PropertyType(XmpTypeName.ProperName)]
    public static readonly string MODEL = "Model";

public TiffSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public TiffSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }
}
