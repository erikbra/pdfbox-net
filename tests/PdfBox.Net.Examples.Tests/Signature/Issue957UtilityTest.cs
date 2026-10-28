/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Native regression tests added with AI assistance for upstream synchronization #957.
 * PORT_MODE: native-adapter
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Adapted regression coverage for Apache PDFBox changes through 5206dee5bfe0f435609f6506e3fccccbc46a95cc.
 * Upstream test mappings and adaptations: reports/issue-957-core-slice.json.
 */
using System.Net;
using PdfBox.Net.Examples.Signature;
using PdfBox.Net.Examples.Signature.Internal;
using PdfBox.Net.Examples.Util;

namespace PdfBox.Net.Examples.Tests.Signature;

public class Issue957UtilityTest
{
    [Theory]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.PermanentRedirect)]
    public void OpenUrlFollowsExactHttpsUpgradeForAdditionalRedirectStatuses(HttpStatusCode status)
    {
        using RedirectHandler handler = new(status);
        using HttpClient client = new(handler);
        using Stream stream = SigUtils.OpenURL("http://www.pki.admin.ch/aia/RegularCA01.crt", client);
        using StreamReader reader = new(stream);
        Assert.Equal("certificate", reader.ReadToEnd());
        Assert.Equal(2, handler.Requests);
    }

    [Fact]
    public void NewRootCrlIsInTheExplicitAllowlist()
    {
        SigUtils.CheckAccess(new Uri("http://www.pki.admin.ch/crl/RootCAIV.crl"));
    }

    [Fact]
    public void ConnectionIsReleasedEvenIfInputCloseFails()
    {
        int disconnected = 0;
        ConnectedInputStream stream = new(new CloseFailureStream(), () => disconnected++);
        Assert.Throws<IOException>(() => stream.Dispose());
        Assert.Equal(1, disconnected);
        stream.Dispose();
        Assert.Equal(1, disconnected);
    }

    [Fact]
    public void SequentialCompatibilityConstructorOwnsAndClosesAllInputStreams()
    {
        MemoryStream first = new([1, 2]);
        MemoryStream second = new([3, 4]);
        using (ConnectedInputStream stream = new([first, second]))
        {
            Assert.Equal(0, stream.Read([], 0, 0));
            using MemoryStream result = new();
            stream.CopyTo(result);
            Assert.Equal([1, 2, 3, 4], result.ToArray());
        }
        Assert.False(first.CanRead);
        Assert.False(second.CanRead);
    }

    [Fact]
    public void CmsEncodingConvertsIndefiniteLengthBerToDer()
    {
        byte[] berSequence = [0x30, 0x80, 0x02, 0x01, 0x01, 0x00, 0x00];
        Assert.Equal([0x30, 0x03, 0x02, 0x01, 0x01], CmsEncoding.EncodeDer(berSequence));
    }

    private sealed class CloseFailureStream : MemoryStream
    {
        protected override void Dispose(bool disposing) => throw new IOException("close failed");
    }

    private sealed class RedirectHandler(HttpStatusCode status) : HttpMessageHandler
    {
        internal int Requests { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            HttpResponseMessage response = new(Requests == 1 ? status : HttpStatusCode.OK)
            {
                Content = new StringContent("certificate")
            };
            if (Requests == 1)
            {
                response.Headers.Location = new Uri("https://www.pki.admin.ch/aia/RegularCA01.crt");
            }
            return Task.FromResult(response);
        }
    }
}
