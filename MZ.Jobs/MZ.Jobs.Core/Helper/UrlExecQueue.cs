// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DBChangeQueue.cs" company="pzcast">
//   (C) 2015 pzcast. All rights reserved.
// </copyright>
// <summary>
//   The url queue.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using MZ.Jobs.Core.Business.Info;
 
namespace MZ.Jobs.Core
{
    /// <summary>
    /// The url queue.
    /// </summary>
    public class UrlExecQueue : SecurityQueue<UrlExecInfo>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="DBChangeQueue"/> class from being created.
        /// </summary>
        private UrlExecQueue()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static UrlExecQueue Instance => Nested.Inner;

        #endregion

        /// <summary>
        /// The nested.
        /// </summary>
        private static class Nested
        {
            #region Static Fields

            /// <summary>
            /// The inner.
            /// </summary>
            internal static readonly UrlExecQueue Inner = new UrlExecQueue();

            #endregion
        }
    }
}