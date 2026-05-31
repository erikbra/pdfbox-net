// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestCreateSignature.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: adapted
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

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

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test for signature creation and validation examples.
/// Ported from TestCreateSignature.java — adapted because the full test requires BouncyCastle
/// cryptographic primitives (signing, OCSP, timestamping, CRL, certificate chain), a PKCS#12
/// keystore fixture, NTP synchronisation, and several signing APIs not yet ported to .NET.
/// Individual sub-tests are retained as traceability stubs.
/// </summary>
public class TestCreateSignature
{
    [Fact(Skip = "Adapted — requires BouncyCastle signing APIs and keystore fixture not yet ported")]
    public void TestDetachedSha256()
    {
    }

    [Fact(Skip = "Adapted — requires BouncyCastle signing APIs and keystore fixture not yet ported")]
    public void TestDetachedSha256WithTSA()
    {
    }

    [Fact(Skip = "Adapted — requires BouncyCastle signing APIs and keystore fixture not yet ported")]
    public void TestCreateVisibleSignature()
    {
    }

    [Fact(Skip = "Adapted — requires BouncyCastle signing APIs and keystore fixture not yet ported")]
    public void TestCreateVisibleSignature2()
    {
    }

    [Fact(Skip = "Adapted — requires BouncyCastle signing APIs and keystore fixture not yet ported")]
    public void TestAddValidationInformation()
    {
    }

    [Fact(Skip = "Adapted — requires BouncyCastle signing APIs and keystore fixture not yet ported")]
    public void TestCreateEmbeddedTimeStamp()
    {
    }

    [Fact(Skip = "Adapted — requires BouncyCastle signing APIs and keystore fixture not yet ported")]
    public void TestCreateSignedTimeStamp()
    {
    }

    [Fact(Skip = "Adapted — requires BouncyCastle signing APIs and keystore fixture not yet ported")]
    public void TestEmptySignatureForm()
    {
    }
}
