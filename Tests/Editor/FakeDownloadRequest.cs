using System;
using System.Collections.Generic;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// A request of which the availability and the handling are set by the test.
    /// </summary>
    internal class FakeDownloadRequest : DownloadRequest
    {

        #region Lifecycle

        /// <summary>
        /// Creates a fake request.
        /// </summary>
        /// <param name="key">The key of the request, also used to make its URL.</param>
        public FakeDownloadRequest(string key)
        {
            Key = key;
            Url = $"https://example.com/{key}";
        }

        #endregion


        #region Public API

        /// <inheritdoc/>
        public override string Key { get; }

        /// <inheritdoc/>
        public override string Url { get; }

        /// <summary>
        /// Whether the resource is available. Set to true once it's handled successfully.
        /// </summary>
        public bool IsResourceAvailable { get; set; } = false;

        /// <summary>
        /// Handles a downloaded file, and returns the outcome. Succeeds by default.
        /// </summary>
        public Func<byte[], EDownloadHandlingResult> OnHandle { get; set; } = _ => EDownloadHandlingResult.Success;

        /// <summary>
        /// The content of the files passed to <see cref="Handle(byte[], out string)"/>, in order.
        /// </summary>
        public List<byte[]> HandledData { get; } = new List<byte[]>();

        /// <summary>
        /// The number of calls to <see cref="IsAvailable"/>.
        /// </summary>
        public int AvailabilityCheckCount { get; private set; } = 0;

        /// <inheritdoc/>
        public override bool IsAvailable()
        {
            AvailabilityCheckCount++;
            return IsResourceAvailable;
        }

        #endregion


        #region Protected API

        /// <inheritdoc/>
        /// <remarks>
        /// Keeps the <c>protected internal</c> modifier, since the tests assembly has access to the internals of the package. Classes
        /// in other assemblies override it as <c>protected</c>.
        /// </remarks>
        protected internal override EDownloadHandlingResult Handle(byte[] data, out string error)
        {
            HandledData.Add(data);
            EDownloadHandlingResult result = OnHandle(data);
            IsResourceAvailable = result == EDownloadHandlingResult.Success;
            error = result != EDownloadHandlingResult.Success ? $"Handling result: {result}" : null;
            return result;
        }

        #endregion

    }

}
