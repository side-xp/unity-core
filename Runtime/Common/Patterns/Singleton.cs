namespace SideXP.Core
{

    /// <summary>
    /// Implements a thread-safe version of the Singleton pattern. The goal is to make an instance of a class unique and accessible from
    /// anywhere.<br/>
    /// Note that Singletons are considered as anti-patterns, and may be used only for prototyping quickly or if it's the only solution in
    /// your software architecture.
    /// 
    /// The implementation is inspired from: https://jlambert.developpez.com/tutoriels/dotnet/implementation-pattern-singleton-csharp
    /// </summary>
    public abstract class Singleton<T>
        where T : Singleton<T>, new()
    {

        #region Fields

        /// <summary>
        /// Contains the unique instance of this class.
        /// </summary>
        private static T s_instance = null;

        /// <summary>
        /// Used to make this Singleton thread-safe.
        /// </summary>
        private static object s_padlock = new object();

        #endregion


        #region Lifecycle

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected Singleton() { }

        /// <summary>
        /// Called when this object is set as the singleton instance.
        /// </summary>
        protected virtual void Init() { }

        #endregion


        #region Public API

        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (s_padlock)
                {
                    if (s_instance == null)
                    {
                        s_instance = new T();
                        s_instance.Init();
                    }
                    return s_instance;
                }
            }
        }

        /// <inheritdoc cref="Instance"/>
        public static T I => Instance;

        #endregion

    }

}