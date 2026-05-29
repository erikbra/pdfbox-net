/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/ExifSchema.java
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

[StructuredType("http://ns.adobe.com/exif/1.0/", "exif")]
public class ExifSchema : XMPSchema
{
    public const string NamespaceUri = "http://ns.adobe.com/exif/1.0/";
    public const string PreferredPrefix = "exif";

    [PropertyType(XmpTypeName.LangAlt)]
    public static readonly string USER_COMMENT = "UserComment";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string EXIF_VERSION = "ExifVersion";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string FLASH_PIX_VERSION = "FlashpixVersion";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string COLOR_SPACE = "ColorSpace";

    [PropertyType(XmpTypeName.Integer, Cardinality.Seq)]
    public static readonly string COMPONENTS_CONFIGURATION = "ComponentsConfiguration";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string COMPRESSED_BPP = "CompressedBitsPerPixel";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string PIXEL_X_DIMENSION = "PixelXDimension";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string PIXEL_Y_DIMENSION = "PixelYDimension";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string RELATED_SOUND_FILE = "RelatedSoundFile";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string DATE_TIME_ORIGINAL = "DateTimeOriginal";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string DATE_TIME_DIGITIZED = "DateTimeDigitized";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string EXPOSURE_TIME = "ExposureTime";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string F_NUMBER = "FNumber";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string EXPOSURE_PROGRAM = "ExposureProgram";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string SPECTRAL_SENSITIVITY = "SpectralSensitivity";

    [PropertyType(XmpTypeName.Integer, Cardinality.Seq)]
    public static readonly string ISO_SPEED_RATINGS = "ISOSpeedRatings";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string SHUTTER_SPEED_VALUE = "ShutterSpeedValue";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string APERTURE_VALUE = "ApertureValue";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string BRIGHTNESS_VALUE = "BrightnessValue";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string EXPOSURE_BIAS_VALUE = "ExposureBiasValue";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string MAX_APERTURE_VALUE = "MaxApertureValue";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string SUBJECT_DISTANCE = "SubjectDistance";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string METERING_MODE = "MeteringMode";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string LIGHT_SOURCE = "LightSource";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string FLASH_ENERGY = "FlashEnergy";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string FOCAL_LENGTH = "FocalLength";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string FOCAL_PLANE_XRESOLUTION = "FocalPlaneXResolution";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string FOCAL_PLANE_YRESOLUTION = "FocalPlaneYResolution";

    [PropertyType(XmpTypeName.Integer, Cardinality.Seq)]
    public static readonly string SUBJECT_AREA = "SubjectArea";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string FOCAL_PLANE_RESOLUTION_UNIT = "FocalPlaneResolutionUnit";

    [PropertyType(XmpTypeName.Integer, Cardinality.Seq)]
    public static readonly string SUBJECT_LOCATION = "SubjectLocation";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string EXPOSURE_INDEX = "ExposureIndex";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string SENSING_METHOD = "SensingMethod";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string FILE_SOURCE = "FileSource";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string SCENE_TYPE = "SceneType";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string CUSTOM_RENDERED = "CustomRendered";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string WHITE_BALANCE = "WhiteBalance";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string EXPOSURE_MODE = "ExposureMode";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string DIGITAL_ZOOM_RATIO = "DigitalZoomRatio";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string FOCAL_LENGTH_IN_3_5MM_FILM = "FocalLengthIn35mmFilm";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string SCENE_CAPTURE_TYPE = "SceneCaptureType";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string GAIN_CONTROL = "GainControl";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string CONTRAST = "Contrast";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string SATURATION = "Saturation";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string SHARPNESS = "Sharpness";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string SUBJECT_DISTANCE_RANGE = "SubjectDistanceRange";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string IMAGE_UNIQUE_ID = "ImageUniqueID";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPSVERSION_ID = "GPSVersionID";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_SATELLITES = "GPSSatellites";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_STATUS = "GPSStatus";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_MEASURE_MODE = "GPSMeasureMode";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_MAP_DATUM = "GPSMapDatum";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_SPEED_REF = "GPSSpeedRef";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_TRACK_REF = "GPSTrackRef";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_IMG_DIRECTION_REF = "GPSImgDirectionRef";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_DEST_BEARING_REF = "GPSDestBearingRef";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_DEST_DISTANCE_REF = "GPSDestDistanceRef";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_PROCESSING_METHOD = "GPSProcessingMethod";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string GPS_AREA_INFORMATION = "GPSAreaInformation";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string GPS_ALTITUDE = "GPSAltitude";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string GPS_DOP = "GPSDOP";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string GPS_SPEED = "GPSSpeed";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string GPS_TRACK = "GPSTrack";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string GPS_IMG_DIRECTION = "GPSImgDirection";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string GPS_DEST_BEARING = "GPSDestBearing";

    [PropertyType(XmpTypeName.Rational)]
    public static readonly string GPS_DEST_DISTANCE = "GPSDestDistance";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string GPS_ALTITUDE_REF = "GPSAltitudeRef";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string GPS_DIFFERENTIAL = "GPSDifferential";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string GPS_TIME_STAMP = "GPSTimeStamp";

    [PropertyType(XmpTypeName.OECF)]
    public static readonly string OECF = "OECF";

    [PropertyType(XmpTypeName.OECF)]
    public static readonly string SPATIAL_FREQUENCY_RESPONSE = "SpatialFrequencyResponse";

    [PropertyType(XmpTypeName.GPSCoordinate)]
    public static readonly string GPS_LATITUDE = "GPSLatitude";

    [PropertyType(XmpTypeName.GPSCoordinate)]
    public static readonly string GPS_LONGITUDE = "GPSLongitude";

    [PropertyType(XmpTypeName.GPSCoordinate)]
    public static readonly string GPS_DEST_LATITUDE = "GPSDestLatitude";

    [PropertyType(XmpTypeName.GPSCoordinate)]
    public static readonly string GPS_DEST_LONGITUDE = "GPSDestLongitude";

    [PropertyType(XmpTypeName.CFAPattern)]
    public static readonly string CFA_PATTERN = "CFAPattern";

    [PropertyType(XmpTypeName.Flash)]
    public static readonly string FLASH = "Flash";

    [PropertyType(XmpTypeName.CFAPattern)]
    public static readonly string CFA_PATTERN_TYPE = "CFAPatternType";

    [PropertyType(XmpTypeName.DeviceSettings)]
    public static readonly string DEVICE_SETTING_DESCRIPTION = "DeviceSettingDescription";

public ExifSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public ExifSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }
}
