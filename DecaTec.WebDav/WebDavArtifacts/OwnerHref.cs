﻿using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DecaTec.WebDav.WebDavArtifacts
{
    /// <summary>
    /// Class representing an 'owner' XML element for WebDAV communication.
    /// </summary>
    [DataContract]
    [XmlType(TypeName = "owner", Namespace = "DAV:")]
    [XmlRoot(Namespace = "DAV:", IsNullable = false)]
    public class OwnerHref
    {
        /// <summary>
        /// Initializes a new instace of OwnerHref.
        /// </summary>
        public OwnerHref()
        {
        }

        /// <summary>
        /// Initializes a new instance of OwnerHref.
        /// </summary>
        /// <param name="ownerString">The owner string.</param>
        public OwnerHref(string ownerString)
        {
            this.Href = new string[] { ownerString }; 
        }

        private string[] hrefField;

        /// <summary>
        /// Gets or sets the Href.
        /// </summary>
        [XmlElement(ElementName = "href")]
        public string[] Href
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }
    }
}
