//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.IdentityModel.Xml
{

    /// <summary>
    /// Represents a XmlDsig KeyInfo element as per:  https://www.w3.org/TR/2001/PR-xmldsig-core-20010820/#sec-KeyInfo
    /// </summary>
    /// <remarks>Only a single 'X509Certificate' is supported. Multiples that include intermediate and root certs are not supported.</remarks>
    public class KeyInfo
    {
        // TODO - IssuerSerial needs to have a structure as 'IssuerName' and 'SerialNumber'
        /// <summary>
        /// Initializes an instance of <see cref="KeyInfo"/>.
        /// </summary>
        public KeyInfo()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="KeyInfo"/>.
        /// </summary>
        /// <param name="certificate">the <see cref="X509Certificate2"/>to populate the X509Data.</param>
        public KeyInfo(X509Certificate2 certificate)
        {
            var data = new X509Data(certificate);
            X509Data.Add(data);
        }

        /// <summary>
        /// Initializes an instance of <see cref="KeyInfo"/>.
        /// </summary>
        /// <param name="key">the <see cref="SecurityKey"/>to populate the <see cref="KeyInfo"/>.</param>
        public KeyInfo(SecurityKey key)
        {
            if (key is X509SecurityKey x509Key)
            {
                var data = new X509Data();
                data.Certificates.Add(Convert.ToBase64String(x509Key.Certificate.RawData));
                X509Data.Add(data);
            }
            else if (key is RsaSecurityKey rsaKey)
            {
                var rsaParameters = rsaKey.Parameters;

                // Obtain parameters from the RSA if the rsaKey does not contain a valid value for RSAParameters
                if (rsaKey.Parameters.Equals(default(RSAParameters)))
                    rsaParameters = rsaKey.Rsa.ExportParameters(false);
        
                RSAKeyValue = new RSAKeyValue(Base64UrlEncoder.Encode(rsaParameters.Modulus), Base64UrlEncoder.Encode(rsaParameters.Exponent));
            }
        }

        /// <summary>
        /// Gets or sets the 'KeyName' that can be used as a key identifier.
        /// </summary>
        public string KeyName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Uri associated with the RetrievalMethod
        /// </summary>
        public string RetrievalMethodUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the RSAKeyValue.
        /// </summary>
        public RSAKeyValue RSAKeyValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the 'X509Data' value.
        /// </summary>
        public ICollection<X509Data> X509Data { get; } = new Collection<X509Data>();

        /// <summary>
        /// Compares two KeyInfo objects.
        /// </summary>
        public override bool Equals(object obj)
        {   
            KeyInfo other = obj as KeyInfo;
            if (other == null)
                return false;
            else if (string.Compare(KeyName, other.KeyName, StringComparison.OrdinalIgnoreCase) != 0
                ||string.Compare(RetrievalMethodUri, other.RetrievalMethodUri, StringComparison.OrdinalIgnoreCase) != 0
                || (RSAKeyValue != null && !RSAKeyValue.Equals(other.RSAKeyValue)
                || !new HashSet<X509Data>(X509Data).SetEquals(other.X509Data)))
                return false;

            return true;
        }

        /// <summary>
        /// Serves as a hash function for KeyInfo.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
