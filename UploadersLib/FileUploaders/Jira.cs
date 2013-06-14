﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (C) 2008-2013 ShareX Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

// gpailler

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using UploadersLib.GUI;
using UploadersLib.HelperClasses;

namespace UploadersLib.FileUploaders
{
    public class Jira : FileUploader, IOAuth
    {
        private const string PathRequestToken = "/plugins/servlet/oauth/request-token";
        private const string PathAuthorize = "/plugins/servlet/oauth/authorize";
        private const string PathAccessToken = "/plugins/servlet/oauth/access-token";
        private const string PathApi = "/rest/api/2";
        private const string PathSearch = PathApi + "/search";
        private const string PathBrowseIssue = "/browse/{0}";
        private const string PathIssueAttachments = PathApi + "/issue/{0}/attachments";

        private readonly static X509Certificate2 _jiraCertificate;

        private readonly Uri _jiraHost;
        private readonly string _jiraIssuePrefix;

        private Uri _jiraRequestToken;
        private Uri _jiraAuthorize;
        private Uri _jiraAccessToken;
        private Uri _jiraPathSearch;

        #region Keypair

        static Jira()
        {
            // Certificate generated using commands:
            // makecert -pe -n "CN=ShareX" -a sha1 -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12 -len 1024 -sv jira_sharex.pvk jira_sharex.cer
            // pvk2pfx -pvk jira_sharex.pvk -spc jira_sharex.cer -pfx jira_sharex.pfx
            // (Based on: http://nick-howard.blogspot.fr/2011/05/makecert-x509-certificates-and-rsa.html)
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UploadersLib.ApiKeys.jira_sharex.pfx"))
            {
                byte[] pfx = new byte[stream.Length];
                stream.Read(pfx, 0, pfx.Length);
                _jiraCertificate = new X509Certificate2(pfx, string.Empty, X509KeyStorageFlags.Exportable);
            }
        }

        internal static string PrivateKey
        {
            get { return _jiraCertificate.PrivateKey.ToXmlString(true); }
        }

        internal static string PublicKey
        {
            get
            {
                const int LineBreakIdx = 50;

                string publicKey = Convert.ToBase64String(ExportPublicKey(_jiraCertificate.PublicKey));
                int idx = 0;
                StringBuilder sb = new StringBuilder();
                foreach (char c in publicKey)
                {
                    sb.Append(c);
                    if ((++idx % LineBreakIdx) == 0)
                    {
                        sb.AppendLine();
                    }
                }

                return string.Join(Environment.NewLine, new[]
                    {
                        "-----BEGIN PUBLIC KEY-----",
                        sb.ToString(),
                        "-----END PUBLIC KEY-----"
                    });
            }
        }

        private static byte[] ExportPublicKey(PublicKey key)
        {
            // From: http://pstaev.blogspot.fr/2010/08/convert-rsa-public-key-from-xml-to-pem.html
            List<byte> binaryPublicKey = new List<byte>();

            byte[] oid = { 0x30, 0xD, 0x6, 0x9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0xD, 0x1, 0x1, 0x1, 0x5, 0x0 }; // Object ID for RSA

            //Transform the public key to PEM Base64 Format
            binaryPublicKey = key.EncodedKeyValue.RawData.ToList();
            binaryPublicKey.Insert(0, 0x0); // Add NULL value

            CalculateAndAppendLength(ref binaryPublicKey);

            binaryPublicKey.Insert(0, 0x3);
            binaryPublicKey.InsertRange(0, oid);

            CalculateAndAppendLength(ref binaryPublicKey);

            binaryPublicKey.Insert(0, 0x30);
            return binaryPublicKey.ToArray();
        }

        private static void CalculateAndAppendLength(ref List<byte> binaryData)
        {
            int len;
            len = binaryData.Count;
            if (len <= byte.MaxValue)
            {
                binaryData.Insert(0, Convert.ToByte(len));
                binaryData.Insert(0, 0x81); //This byte means that the length fits in one byte
            }
            else
            {
                binaryData.Insert(0, Convert.ToByte(len % (byte.MaxValue + 1)));
                binaryData.Insert(0, Convert.ToByte(len / (byte.MaxValue + 1)));
                binaryData.Insert(0, 0x82); //This byte means that the length fits in two byte
            }
        }

        #endregion Keypair

        public Jira(string jiraHost, OAuthInfo oauth, string jiraIssuePrefix = null)
        {
            this._jiraHost = new Uri(jiraHost);
            this.AuthInfo = oauth;
            this._jiraIssuePrefix = jiraIssuePrefix;

            this.InitUris();
        }

        public OAuthInfo AuthInfo { get; set; }

        public string GetAuthorizationURL()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args[OAuthManager.ParameterCallback] = "oob"; // Request activation code to validate authentication

            string url = OAuthManager.GenerateQuery(this._jiraRequestToken.ToString(), args, HttpMethod.Post, this.AuthInfo);

            string response = SendRequest(HttpMethod.Post, url);

            if (!string.IsNullOrEmpty(response))
            {
                return OAuthManager.GetAuthorizationURL(response, this.AuthInfo, this._jiraAuthorize.ToString());
            }

            return null;
        }

        public bool GetAccessToken(string verificationCode)
        {
            AuthInfo.AuthVerifier = verificationCode;

            NameValueCollection nv = GetAccessTokenEx(this._jiraAccessToken.ToString(), this.AuthInfo, HttpMethod.Post);

            return nv != null;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            JiraUpload up = new JiraUpload(this._jiraIssuePrefix, this.GetSummary);

            DialogResult result = up.ShowDialog();
            if (result == DialogResult.Cancel)
            {
                return new UploadResult
                    {
                        IsSuccess = true,
                        IsURLExpected = false
                    };
            }

            Uri uri = new Uri(this._jiraHost, string.Format(PathIssueAttachments, up.IssueId));
            string query = OAuthManager.GenerateQuery(uri.ToString(), null, HttpMethod.Post, this.AuthInfo);

            NameValueCollection headers = new NameValueCollection();
            headers.Set("X-Atlassian-Token", "nocheck");

            UploadResult res = this.UploadData(stream, query, fileName, "file", null, null, headers);
            if (res.Response.Contains("errorMessages"))
            {
                res.Errors.Add(res.Response);
            }
            else
            {
                res.IsURLExpected = true;
                var anonType = new[] { new { thumbnail = "" } };
                var anonObject = JsonConvert.DeserializeAnonymousType(res.Response, anonType);
                res.ThumbnailURL = anonObject[0].thumbnail;
                res.URL = new Uri(this._jiraHost, string.Format(PathBrowseIssue, up.IssueId)).ToString();
            }

            return res;
        }

        private string GetSummary(string issueId)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args["jql"] = string.Format("issueKey='{0}'", issueId);
            args["maxResults"] = "10";
            args["fields"] = "summary";
            string query = OAuthManager.GenerateQuery(this._jiraPathSearch.ToString(), args, HttpMethod.Get, this.AuthInfo);

            string response = this.SendGetRequest(query);
            if (!string.IsNullOrEmpty(response))
            {
                var anonType = new { issues = new[] { new { key = "", fields = new { summary = "" } } } };
                var res = JsonConvert.DeserializeAnonymousType(response, anonType);
                return res.issues[0].fields.summary;
            }
            else
            {
                // This query can returns error so we have to remove last error from errors list
                this.Errors.RemoveAt(this.Errors.Count - 1);
            }

            return null;
        }

        private void InitUris()
        {
            this._jiraRequestToken = new Uri(this._jiraHost, PathRequestToken);
            this._jiraAuthorize = new Uri(this._jiraHost, PathAuthorize);
            this._jiraAccessToken = new Uri(this._jiraHost, PathAccessToken);
            this._jiraPathSearch = new Uri(this._jiraHost, PathSearch);
        }
    }
}