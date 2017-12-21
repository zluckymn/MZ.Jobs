using MongoDB.Bson;

namespace MZ.Jobs.Core.Business.Info
{

    /// <summary>
    /// The url info.
    /// </summary>
    public class UrlExecInfo
    {
        #region Fields

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see>
        ///   <cref>UrlInfo</cref>
        ///     </see>
        ///     class.
        /// </summary>
        /// <param name="urlString">
        /// The url string.
        /// </param>
        public UrlExecInfo(string urlString)
        {
            this.UrlString = urlString;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the depth.
        /// </summary>
        public BsonDocument ParamDoc { get; set; }

        /// <summary>
        /// Gets the url string.
        /// </summary>
        public string UrlString { get; set; }
    }
    #endregion
}
