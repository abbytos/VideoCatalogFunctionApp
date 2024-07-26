namespace VideoCatalogFunctionApp.Utilities
{
    /// <summary>
    /// Represents a model for a video file with its name and size.
    /// </summary>
    public class VideoFileModel
    {
        /// <summary>
        /// Gets or sets the name of the video file.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the video file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the size of the video file in bytes.
        /// </summary>
        /// <value>
        /// A <see cref="long"/> representing the size of the video file in bytes.
        /// </value>
        public long FileSize { get; set; }
    }
}
