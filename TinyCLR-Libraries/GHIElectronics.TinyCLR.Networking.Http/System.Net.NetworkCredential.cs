////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{

    /// <summary>
    /// Class that keeps user name and password.
    /// </summary>
    public class NetworkCredential
    {

        private string m_userName;
        private string m_password;
        private AuthenticationType m_authenticationType;

        /// <summary>
        /// Construct class with empty user name and password
        /// </summary>
        public NetworkCredential()
        {
        }

        /// <summary>
        /// Constructs credientials and initializes them by provided user name and pssword
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public NetworkCredential(string userName, string password)
            : this(userName, password, AuthenticationType.Basic)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkCredential"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="authenticationType">Type of the authentication.</param>
        public NetworkCredential(string userName, string password, AuthenticationType authenticationType)
        {
            this.UserName = userName;
            this.Password = password;
            this.AuthenticationType = authenticationType;
        }

        /// <summary>
        /// Set or get user name.
        /// </summary>
        public string UserName {
            get => this.m_userName;
            set => this.m_userName = value;
        }

        /// <summary>
        /// Set or get password.
        /// </summary>
        public string Password {
            get => this.m_password;
            set => this.m_password = value;
        }

        /// <summary>
        /// Gets or sets the type of the authentication.
        /// </summary>
        /// <value>The type of the authentication.</value>
        public AuthenticationType AuthenticationType {
            get => this.m_authenticationType;
            set => this.m_authenticationType = value;
        }

    } // class NetworkCredential
} // namespace System.Net


