﻿using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'activelock' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [DebuggerStepThrough]
    [XmlType(TypeName = WebDavConstants.ActiveLock, Namespace = WebDavConstants.DAV)]
    [XmlRoot(Namespace = WebDavConstants.DAV, IsNullable = false)]
    public class ActiveLock
    {
        private LockScope lockscopeField;
        private LockType locktypeField;
        private string depthField;
        private OwnerHref ownerField;
        private string timeoutField;
        private WebDavLockToken locktokenField;
        private LockRoot lockRootField;

        /// <summary>
        /// Gets or sets the LockScope.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockScope)]
        public LockScope LockScope
        {
            get
            {
                return this.lockscopeField;
            }
            set
            {
                this.lockscopeField = value;
            }
        }

        /// <summary>
        /// Gets or sets the LockType.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockType)]
        public LockType LockType
        {
            get
            {
                return this.locktypeField;
            }
            set
            {
                this.locktypeField = value;
            }
        }

        /// <summary>
        /// Gets or sets the Depth.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Depth)]
        public string Depth
        {
            get
            {
                return this.depthField;
            }
            set
            {
                this.depthField = value;
            }
        }

        /// <summary>
        /// Gets or sets the Owner.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Owner)]
        public OwnerHref Owner
        {
            get
            {
                return this.ownerField;
            }
            set
            {
                this.ownerField = value;
            }
        }

        /// <summary>
        /// Gets or sets the Timeout.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.Timeout)]
        public string Timeout
        {
            get
            {
                return this.timeoutField;
            }
            set
            {
                this.timeoutField = value;
            }
        }

        /// <summary>
        /// Gets or sets the LockToken.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockToken, IsNullable = false)]
        public WebDavLockToken LockToken
        {
            get
            {
                return this.locktokenField;
            }
            set
            {
                this.locktokenField = value;
            }
        }

        /// <summary>
        /// Gets or sets the LockRoot.
        /// </summary>
        [XmlElement(ElementName = WebDavConstants.LockRoot, IsNullable = false)]
        public LockRoot LockRoot
        {
            get
            {
                return this.lockRootField;
            }
            set
            {
                this.lockRootField = value;
            }
        }
    }
}
