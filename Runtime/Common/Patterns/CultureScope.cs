using System;
using System.Threading;
using System.Globalization;

namespace SideXP.Core
{

    /// <summary>
    /// Sets the culture of a thread. This is meant to be used in a using statement.
    /// </summary>
    public class CultureScope : IDisposable
    {

        #region Fields

        /// <inheritdoc cref="CultureScope.CultureScope"/>
        public static CultureScope Invariant => new CultureScope();

        /// <summary>
        /// The current thread's culture before applying this scope.
        /// </summary>
        private CultureInfo _previousCulture = null;

        /// <summary>
        /// The thread on which the culture scope has been applied.
        /// </summary>
        private Thread _thread = null;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Applies <see cref="CultureInfo.InvariantCulture"/> to the current thread.
        /// </summary>
        /// <inheritdoc cref="CultureScope(Thread, CultureInfo)"/>
        public CultureScope()
        {
            Apply(Thread.CurrentThread, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Applies the given culture to the current thread.
        /// </summary>
        /// <inheritdoc cref="Apply(Thread, CultureInfo)"/>
        public CultureScope(CultureInfo culture)
        {
            Apply(Thread.CurrentThread, culture);
        }

        /// <summary>
        /// Applies <see cref="CultureInfo.InvariantCulture"/> to the given thread.
        /// </summary>
        /// <inheritdoc cref="CultureScope(Thread, CultureInfo)"/>
        public CultureScope(Thread thread)
        {
            Apply(thread, CultureInfo.InvariantCulture);
        }

        /// <inheritdoc cref="Apply(Thread, CultureInfo)"/>
        public CultureScope(Thread thread, CultureInfo culture)
        {
            Apply(thread, culture);
        }

        /// <summary>
        /// Called when this object is disposed.
        /// </summary>
        public void Dispose()
        {
            _thread.CurrentCulture = _previousCulture;
        }

        #endregion


        #region Private API

        /// <summary>
        /// Applies the given culture on the given thread.
        /// </summary>
        /// <param name="culture">The culture to apply.</param>
        /// <param name="thread">The thread you want to set the culture.</param>
        private void Apply(Thread thread, CultureInfo culture)
        {
            _thread = thread;
            _previousCulture = thread.CurrentCulture.Clone() as CultureInfo;
            thread.CurrentCulture = culture;
        }

        #endregion

    }

}