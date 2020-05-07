﻿using System;
using GameNetworking.Messages.Coders;
using GameNetworking.Messages.Models;

namespace GameNetworking.Messages.Streams {
    public class MessageStreamReader : IStreamReader {
        private readonly object lockToken = new object();
        private readonly byte[] currentBuffer = new byte[1024 * 1024]; // 1MB
        private int currentBufferLength;

        public MessageStreamReader() { }

        public void Add(byte[] buffer, int count) {
            lock (this.lockToken) {
                Array.Copy(buffer, 0, this.currentBuffer, this.currentBufferLength, count);
                this.currentBufferLength += count;
            }
        }

        public MessageContainer Decode() {
            lock (this.lockToken) {
                if (this.currentBufferLength == 0) { return null; }

                int delimiterIndex = CoderHelper.CheckForDelimiter(this.currentBuffer, this.currentBufferLength);
                if (delimiterIndex != -1) {
                    var packageBuffer = MessageContainer.GetBuffer();
                    CoderHelper.PackageBytes(delimiterIndex, this.currentBuffer, packageBuffer);
                    var container = new MessageContainer(packageBuffer, delimiterIndex);
                    this.currentBufferLength = CoderHelper.SliceBuffer(delimiterIndex, this.currentBuffer, this.currentBufferLength);
                    return container;
                }
                return null;
            }
        }
    }
}