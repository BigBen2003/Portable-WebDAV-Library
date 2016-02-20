﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using DecaTec.WebDav.WebDavArtifacts;
using DecaTec.WebDav.Test.Common;

namespace DecaTec.WebDav.Test.UnitIntegrationTest
{
    /// <summary>
    /// Unit integration test class for WebDavClient.
    /// You'll need a file 'TestConfiguration.txt' in the test's ouuput folder with the following content:
    /// Line 1: The user name to use for WebDAV connections
    /// Line 2: The password to use for WebDAV connections
    /// Line 3: The URL of an already exisiting WebDAV folder in the server used for tests
    ///  
    /// If this file is not present, all test will fail!
    /// </summary>
    [TestClass]
    public class UnitIntegrationTestWebDavClient
    {
        private string userName;
        private string password;
        private string webDavRootFolder;

        private const string ConfigurationFile = @"TestConfiguration.txt";
        private const string TestFile = @"TextFile1.txt";
        private const string TestCollection = "TestCollection";

        [TestInitialize]
        public void ReadTestConfiguration()
        {
            try
            {
                var configuration = File.ReadAllLines(ConfigurationFile);
                this.userName = configuration[0];
                this.password = configuration[1];
                this.webDavRootFolder = configuration[2];
            }
            catch (Exception)
            {
                throw;
            }
        }

        private WebDavClient CreateWebDavClientWithDebugHttpMessageHandler()
        {
            var credentials = new WebDavCredential(this.userName, this.password);
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.Credentials = credentials;
            httpClientHandler.PreAuthenticate = true;
            var debugHttpMessageHandler = new DebugHttpMessageHandler(httpClientHandler);
            var wdc = new WebDavClient(debugHttpMessageHandler);
            return wdc;
        }

        #region PropFind

        [TestMethod]
        public void UnitIntegrationTestWebDavClientPropFindAllProp()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();
            PropFind pf = PropFind.CreatePropFindAllProp();
            var response = client.PropFindAsync(this.webDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;
            var multistatus = WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsNotNull(multistatus);
        }

        [TestMethod]
        public void UnitIntegrationTestWebDavClientPropFindNamedProperties()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();
            PropFind pf = PropFind.CreatePropFindWithEmptyProperties("name");
            var response = client.PropFindAsync(this.webDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;
            var multistatus = WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsNotNull(multistatus);           
        }

        [TestMethod]
        public void UnitIntegrationTestWebDavClientPropFindPropName()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();
            PropFind pf = PropFind.CreatePropFindWithPropName();
            var response = client.PropFindAsync(this.webDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;
            var multistatus = WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsNotNull(multistatus);            
        }

        #endregion PropFind

        #region PropPatch / put / delete file

        [TestMethod]
        public void UnitIntegrationTestWebDavClientPropPatch()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();
            var testFile = this.webDavRootFolder + TestFile;

            // Put file.
            var content = new StreamContent(File.OpenRead(TestFile));
            var response = client.PutAsync(testFile, content).Result;
            var putResponseSuccess = response.IsSuccessStatusCode;            

            // PropPatch (set).
            var propertyUpdate = new PropertyUpdate();
            var set = new Set();
            var prop = new Prop();
            prop.DisplayName = "TestFileDisplayName";
            set.Prop = prop;
            propertyUpdate.Items = new object[] {set};
            response = client.PropPatchAsync(testFile, propertyUpdate).Result;
            var propPatchResponseSuccess = response.IsSuccessStatusCode;            

            // PropFind.
            PropFind pf = PropFind.CreatePropFindWithEmptyProperties("displayname");
            response = client.PropFindAsync(testFile, WebDavDepthHeaderValue.Zero, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;
            var multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;
            var displayName = ((Propstat)multistatus.Response[0].Items[0]).Prop.DisplayName;
            // IIS ignores display name and always puts the file name as display name.
            var displayNameResult = "TestFileDisplayName" == displayName || TestFile == displayName;            

            // PropPatch (remove).
            propertyUpdate = new PropertyUpdate();
            var remove = new Remove();
            prop = Prop.CreatePropWithEmptyProperties("displayname");
            remove.Prop = prop;
            propertyUpdate.Items = new object[] { remove };
            response = client.PropPatchAsync(testFile, propertyUpdate).Result;
            var propPatchRemoveResponseSuccess = response.IsSuccessStatusCode;
            multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;
            var multistatusResult = ((Propstat)multistatus.Response[0].Items[0]).Prop.DisplayName;            

            // Delete file.
            response = client.DeleteAsync(testFile).Result;
            var deleteResponseSuccess = response.IsSuccessStatusCode;

            Assert.IsTrue(putResponseSuccess);
            Assert.IsTrue(propPatchResponseSuccess);
            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsTrue(displayNameResult);
            Assert.IsTrue(propPatchRemoveResponseSuccess);
            Assert.AreEqual(string.Empty, multistatusResult);
            Assert.IsTrue(deleteResponseSuccess);
        }

        #endregion PropPatch / put / delete file

        #region Mkcol / delete collection

        [TestMethod]
        public void UnitIntegrationTestWebDavClientMkcol()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();
            var testCollection = this.webDavRootFolder + TestCollection;

            // Create collection.
            var response = client.MkcolAsync(testCollection).Result;
            var mkColResponseSuccess = response.IsSuccessStatusCode;
            
            // PropFind.
            PropFind pf = PropFind.CreatePropFindAllProp();
            response = client.PropFindAsync(this.webDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;            

            var multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            bool collectionFound = false;

            foreach (var item in multistatus.Response)
            {
                if (item.Href.EndsWith(TestCollection + "/"))
                {
                    collectionFound = true;
                    break;
                }
            }

            // Delete collection.
            response = client.DeleteAsync(testCollection).Result;
            var deleteResponseSuccess = response.IsSuccessStatusCode;

            Assert.IsTrue(mkColResponseSuccess);
            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsTrue(collectionFound);
            Assert.IsTrue(deleteResponseSuccess);
        }

        #endregion Mkcol / delete collection

        #region Get

        [TestMethod]
        public void UnitIntegrationTestWebDavClientGet()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();
            var testFile = this.webDavRootFolder + TestFile;

            // Put file.
            var content = new StreamContent(File.OpenRead(TestFile));
            var response = client.PutAsync(testFile, content).Result;
            var putResponseSuccess = response.IsSuccessStatusCode;

            // Get file.
            response = client.GetAsync(testFile).Result;
            var getResponseSuccess = response.IsSuccessStatusCode;            

            var responseContent = response.Content.ReadAsStringAsync().Result;
            var readResponseContent = response.Content.ReadAsStringAsync().Result;            

            // Delete file.
            response = client.DeleteAsync(testFile).Result;
            var deleteResponseSuccess = response.IsSuccessStatusCode;

            Assert.IsTrue(putResponseSuccess);
            Assert.IsTrue(getResponseSuccess);
            Assert.AreEqual("This is a test file for WebDAV.", readResponseContent);
            Assert.IsTrue(deleteResponseSuccess);
        }

        #endregion Get

        #region Copy

        [TestMethod]
        public void UnitIntegrationTestWebDavClientCopy()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();
            var testCollectionSource = this.webDavRootFolder + TestCollection;
            var testCollectionDestination = this.webDavRootFolder + TestCollection + "2";
            var testFile = testCollectionSource + "/" + TestFile;

            // Create source collection.
            var response = client.MkcolAsync(testCollectionSource).Result;
            var mkColResponseSuccess = response.IsSuccessStatusCode;
           
            // Put file.
            var content = new StreamContent(File.OpenRead(TestFile));
            response = client.PutAsync(testFile, content).Result;
            var putResponseSuccess = response.IsSuccessStatusCode;
            
            // Copy.
            response = client.CopyAsync(testCollectionSource, testCollectionDestination).Result;
            var copyResponseSuccess = response.IsSuccessStatusCode;            

            // PropFind.
            PropFind pf = PropFind.CreatePropFindAllProp();
            response = client.PropFindAsync(testCollectionDestination, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;            

            var multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            bool collectionfound = false;

            foreach (var item in multistatus.Response)
            {
                if (item.Href.EndsWith(TestFile))
                {
                    collectionfound = true;
                    break;
                }
            }

            // Delete source and destination.
            response = client.DeleteAsync(testCollectionSource).Result;
            var deleteSourceResponseSuccess = response.IsSuccessStatusCode;

            response = client.DeleteAsync(testCollectionDestination).Result;
            var deleteDestinationResponseSuccess = response.IsSuccessStatusCode;

            Assert.IsTrue(mkColResponseSuccess);
            Assert.IsTrue(putResponseSuccess);
            Assert.IsTrue(copyResponseSuccess);
            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsTrue(collectionfound);
            Assert.IsTrue(deleteSourceResponseSuccess);
            Assert.IsTrue(deleteDestinationResponseSuccess);
        }

        #endregion Copy

        #region Move

        [TestMethod]
        public void UnitIntegrationTestWebDavClientMove()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();
            var testCollectionSource = this.webDavRootFolder + TestCollection;
            var testCollectionDestination = this.webDavRootFolder + TestCollection + "2";
            var testFile = testCollectionSource + "/" + TestFile;

            // Create source collection.
            var response = client.MkcolAsync(testCollectionSource).Result;
            var mkColResponseSuccess = response.IsSuccessStatusCode;            

            // Put file.
            var content = new StreamContent(File.OpenRead(TestFile));
            response = client.PutAsync(testFile, content).Result;
            var putResponseSuccess = response.IsSuccessStatusCode;            

            // Move.
            response = client.MoveAsync(testCollectionSource, testCollectionDestination).Result;
            var moveResponseSuccess = response.IsSuccessStatusCode;            

            // PropFind.
            PropFind pf = PropFind.CreatePropFindAllProp();
            response = client.PropFindAsync(this.webDavRootFolder, WebDavDepthHeaderValue.Infinity, pf).Result;
            var propFindResponseSuccess = response.IsSuccessStatusCode;            

            var multistatus = (Multistatus)WebDavResponseContentParser.ParseMultistatusResponseContentAsync(response.Content).Result;

            bool foundCollection1 = false;
            bool foundCollection2 = false;

            foreach (var item in multistatus.Response)
            {
                if (item.Href.EndsWith(TestCollection + "/"))
                    foundCollection1 = true;

                if (item.Href.EndsWith(TestCollection + "2/"))
                    foundCollection2 = true;                       
            }          

            // Delete source and destination.
            // Delete file.
            response = client.DeleteAsync(testCollectionDestination).Result;
            var deleteResponseSuccess = response.IsSuccessStatusCode;

            Assert.IsTrue(mkColResponseSuccess);
            Assert.IsTrue(putResponseSuccess);
            Assert.IsTrue(moveResponseSuccess);
            Assert.IsTrue(propFindResponseSuccess);
            Assert.IsFalse(foundCollection1);
            Assert.IsTrue(foundCollection2);
            Assert.IsTrue(deleteResponseSuccess);
        }

        #endregion Move

        #region Lock / unlock

        [TestMethod]
        public void UnitIntegrationTestWebDavClientLockRefreshLockUnlock()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();

            // Lock.
            var lockInfo = new LockInfo();
            lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
            lockInfo.LockType = LockType.CreateWriteLockType();
            lockInfo.Owner = new OwnerHref("test@test.com");
            var response = client.LockAsync(this.webDavRootFolder, WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(15)), WebDavDepthHeaderValue.Infinity, lockInfo).Result;
            var lockResponseSuccess = response.IsSuccessStatusCode;            
            LockToken lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);            

            // Refresh lock.
            response = client.RefreshLockAsync(this.webDavRootFolder, WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(10)), lockToken).Result;
            var refreshLockResponseSuccess = response.IsSuccessStatusCode;            

            // Unlock.
            response = client.UnlockAsync(this.webDavRootFolder, lockToken).Result;
            var unlockResponseSuccess = response.IsSuccessStatusCode;

            Assert.IsTrue(lockResponseSuccess);
            Assert.IsNotNull(lockToken);
            Assert.IsTrue(refreshLockResponseSuccess);
            Assert.IsTrue(unlockResponseSuccess);
        }

        [TestMethod]
        public void UnitIntegrationTestWebDavClientLockAndPutWithoutToken()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();

            // Lock.
            var lockInfo = new LockInfo();
            lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
            lockInfo.LockType = LockType.CreateWriteLockType();
            lockInfo.Owner = new OwnerHref("test@test.com");
            var response = client.LockAsync(this.webDavRootFolder, WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(15)), WebDavDepthHeaderValue.Infinity, lockInfo).Result;
            var lockResponseSuccess = response.IsSuccessStatusCode;            

            LockToken lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);            

            // Put file (without lock token) -> this should fail.
            var content = new StreamContent(File.OpenRead(TestFile));
            response = client.PutAsync(this.webDavRootFolder + TestFile, content).Result;
            var putResponseSuccess = response.IsSuccessStatusCode;

            // Unlock.
            response = client.UnlockAsync(this.webDavRootFolder, lockToken).Result;
            var unlockResponseSuccess = response.IsSuccessStatusCode;

            Assert.IsTrue(lockResponseSuccess);
            Assert.IsNotNull(lockToken);
            Assert.IsFalse(putResponseSuccess);
            Assert.IsTrue(unlockResponseSuccess);
        }

        [TestMethod]
        public void UnitIntegrationTestWebDavClientLockAndPutWithToken()
        {
            var client = CreateWebDavClientWithDebugHttpMessageHandler();

            // Lock.
            var lockInfo = new LockInfo();
            lockInfo.LockScope = LockScope.CreateExclusiveLockScope();
            lockInfo.LockType = LockType.CreateWriteLockType();
            lockInfo.Owner = new OwnerHref("test@test.com");
            var response = client.LockAsync(this.webDavRootFolder, WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(15)), WebDavDepthHeaderValue.Infinity, lockInfo).Result;
            var lockResponseSuccess = response.IsSuccessStatusCode; 
            LockToken lockToken = WebDavHelper.GetLockTokenFromWebDavResponseMessage(response);            

            // Put file.
            var content = new StreamContent(File.OpenRead(TestFile));
            response = client.PutAsync(this.webDavRootFolder + TestFile, content, lockToken).Result;
            var putResponseSuccess = response.IsSuccessStatusCode;            

            // Delete file.
            response = client.DeleteAsync(this.webDavRootFolder + TestFile, lockToken).Result;
            var deleteResponseSuccess = response.IsSuccessStatusCode;            

            // Unlock.
            response = client.UnlockAsync(this.webDavRootFolder, lockToken).Result;
            var unlockResponseSuccess = response.IsSuccessStatusCode;

            Assert.IsTrue(lockResponseSuccess);
            Assert.IsNotNull(lockToken);
            Assert.IsTrue(putResponseSuccess);
            Assert.IsTrue(deleteResponseSuccess);
            Assert.IsTrue(unlockResponseSuccess);
        }

        #endregion Lock / unlock
    }
}
