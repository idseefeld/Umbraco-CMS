﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Strings;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using CoreCurrent = Umbraco.Core.DependencyInjection.Current;

namespace Umbraco.Web
{
    // must remain internal - this class is here to support the transition from singletons
    // and resolvers to injection - by providing a static access to singleton services - it
    // is initialized once with a service container, in WebBootManager.
    public static class Current
    {
        private static readonly object Locker = new object();

        private static IUmbracoContextAccessor _umbracoContextAccessor;
        private static IFacadeAccessor _facadeAccessor;

        // in theory with proper injection all accessors should be injected, but during the
        // transitions there are places where we need them and they are not available, so
        // use the following properties:

        public static IUmbracoContextAccessor UmbracoContextAccessor
        {
            get
            {
                if (_umbracoContextAccessor != null) return _umbracoContextAccessor;
                return (_umbracoContextAccessor = CoreCurrent.Container.GetInstance<IUmbracoContextAccessor>());
            }
            set { _umbracoContextAccessor = value; } // for tests
        }

        public static IFacadeAccessor FacadeAccessor
        {
            get
            {
                if (_facadeAccessor != null) return _facadeAccessor;
                return (_facadeAccessor = CoreCurrent.Container.GetInstance<IFacadeAccessor>());
            }
            set { _facadeAccessor = value; } // for tests
        }

        public static UmbracoContext UmbracoContext 
            => UmbracoContextAccessor.UmbracoContext;

        // have to support set for now, because of 'ensure umbraco context' which can create
        // contexts pretty much at any time and in an uncontrolled way - and when we do not have
        // proper access to the accessor.
        public static void SetUmbracoContext(UmbracoContext value, bool canReplace)
        {
            lock (Locker)
            {
                if (UmbracoContextAccessor.UmbracoContext != null && canReplace == false)
                    throw new InvalidOperationException("Current UmbracoContext can be set only once per request.");
                UmbracoContextAccessor.UmbracoContext?.Dispose(); // dispose the one that is being replaced, if any
                UmbracoContextAccessor.UmbracoContext = value;
            }
        }

        // this is because of the weird mixed accessor we're using that can store things in thread-static var
        public static void ClearUmbracoContext()
        {
            lock (Locker)
            {
                UmbracoContextAccessor.UmbracoContext?.Dispose(); // dispose the one that is being cleared, if any
                UmbracoContextAccessor.UmbracoContext = null;
            }
        }

        // cannot set - it's set by whatever creates the facade, which should have the accessor injected
        public static IFacade Facade 
            => FacadeAccessor.Facade;

        // cannot set - this is temp
        public static EventMessages EventMessages 
            => CoreCurrent.Container.GetInstance<IEventMessagesFactory>().GetOrDefault();

        public static IEnumerable<IUrlProvider> UrlProviders
            => CoreCurrent.Container.GetInstance<UrlProviderCollection>();

        #region Core

        // just repeating Core for convenience

        public static IEnumerable<IUrlSegmentProvider> UrlSegmentProviders
            => CoreCurrent.Container.GetInstance<UrlSegmentProviderCollection>();


        #endregion
    }
}
