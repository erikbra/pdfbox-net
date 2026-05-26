/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/filetypedetector/ByteTrie.java
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

namespace PdfBox.Net.Util.FileTypeDetector;

internal class ByteTrie<T>
{
    internal sealed class ByteTrieNode<TNode>
    {
        private readonly Dictionary<byte, ByteTrieNode<TNode>> _children = [];
        private TNode? _value;
        private bool _hasValue;

        public IReadOnlyDictionary<byte, ByteTrieNode<TNode>> Children => _children;

        public TNode? Value => _value;

        public bool HasValue => _hasValue;

        public void SetValue(TNode value)
        {
            if (_hasValue)
            {
                throw new InvalidOperationException("Value already set for this trie node");
            }

            _value = value;
            _hasValue = true;
        }

        public ByteTrieNode<TNode> GetOrAdd(byte key)
        {
            if (!_children.TryGetValue(key, out ByteTrieNode<TNode>? child))
            {
                child = new ByteTrieNode<TNode>();
                _children[key] = child;
            }

            return child;
        }
    }

    private readonly ByteTrieNode<T> _root = new();
    private int _maxDepth;

    public T? Find(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        ByteTrieNode<T> node = _root;
        T? value = node.HasValue ? node.Value : default;
        foreach (byte b in bytes)
        {
            if (!node.Children.TryGetValue(b, out ByteTrieNode<T>? child))
            {
                break;
            }

            node = child;
            if (node.HasValue)
            {
                value = node.Value;
            }
        }

        return value;
    }

    public void AddPath(T value, params byte[][] parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        int depth = 0;
        ByteTrieNode<T> node = _root;
        foreach (byte[] part in parts)
        {
            ArgumentNullException.ThrowIfNull(part);
            foreach (byte b in part)
            {
                node = node.GetOrAdd(b);
                depth++;
            }
        }

        node.SetValue(value);
        _maxDepth = Math.Max(_maxDepth, depth);
    }

    public void SetDefaultValue(T defaultValue)
    {
        _root.SetValue(defaultValue);
    }

    public int GetMaxDepth()
    {
        return _maxDepth;
    }
}
