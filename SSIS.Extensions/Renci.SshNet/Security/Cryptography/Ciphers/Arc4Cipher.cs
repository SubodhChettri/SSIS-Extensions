﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements ARCH4 cipher algorithm
    /// </summary>
    public class Arc4Cipher : StreamCipher
    {
        public override byte MinimumSize
        {
            get { return 0; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arc4Cipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public Arc4Cipher(byte[] key, bool dischargeFirstBytes)
            : base(key)
        {
            workingKey = key;
            SetKey(workingKey);
            //   The first 1536 bytes of keystream
            //   generated by the cipher MUST be discarded, and the first byte of the
            //   first encrypted packet MUST be encrypted using the 1537th byte of
            //   keystream.
            if (dischargeFirstBytes)
                this.Encrypt(new byte[1536]);
        }

        /// <summary>
        /// Encrypts the specified region of the input byte array and copies the encrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to encrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write encrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes encrypted.
        /// </returns>
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return this.ProcessBytes(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Decrypts the specified region of the input byte array and copies the decrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to decrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write decrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes decrypted.
        /// </returns>
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return this.ProcessBytes(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Encrypted data.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override byte[] Encrypt(byte[] input)
        {
            var output = new byte[input.Length];
            this.ProcessBytes(input, 0, input.Length, output, 0);
            return output;
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Decrypted data.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override byte[] Decrypt(byte[] input)
        {
            var output = new byte[input.Length];
            this.ProcessBytes(input, 0, input.Length, output, 0);
            return output;
        }

        private readonly static int STATE_LENGTH = 256;

        /*
        * variables to hold the state of the RC4 engine
        * during encryption and decryption
        */

        private byte[] engineState;
        private int x;
        private int y;
        private byte[] workingKey;

        private int ProcessBytes(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if ((inputOffset + inputCount) > inputBuffer.Length)
            {
                throw new IndexOutOfRangeException("input buffer too short");
            }

            if ((outputOffset + inputCount) > outputBuffer.Length)
            {
                throw new IndexOutOfRangeException("output buffer too short");
            }

            for (int i = 0; i < inputCount; i++)
            {
                x = (x + 1) & 0xff;
                y = (engineState[x] + y) & 0xff;

                // swap
                byte tmp = engineState[x];
                engineState[x] = engineState[y];
                engineState[y] = tmp;

                // xor
                outputBuffer[i + outputOffset] = (byte)(inputBuffer[i + inputOffset] ^ engineState[(engineState[x] + engineState[y]) & 0xff]);
            }
            return inputCount;
        }

        public void Reset()
        {
            SetKey(workingKey);
        }

        // Private implementation

        private void SetKey(byte[] keyBytes)
        {
            workingKey = keyBytes;

            // System.out.println("the key length is ; "+ workingKey.Length);

            x = 0;
            y = 0;

            if (engineState == null)
            {
                engineState = new byte[STATE_LENGTH];
            }

            // reset the state of the engine
            for (int i = 0; i < STATE_LENGTH; i++)
            {
                engineState[i] = (byte)i;
            }

            int i1 = 0;
            int i2 = 0;

            for (int i = 0; i < STATE_LENGTH; i++)
            {
                i2 = ((keyBytes[i1] & 0xff) + engineState[i] + i2) & 0xff;
                // do the byte-swap inline
                byte tmp = engineState[i];
                engineState[i] = engineState[i2];
                engineState[i2] = tmp;
                i1 = (i1 + 1) % keyBytes.Length;
            }
        }

    }
}
