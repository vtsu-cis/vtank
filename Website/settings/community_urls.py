from django.conf import settings
from django.conf.urls.defaults import *
from sphene.sphwiki.sitemaps import WikiSnipSitemap
from sphene.sphboard.sitemaps import ThreadsSitemap
from sphene.sphwiki.feeds import LatestWikiChanges
from sphene.sphwiki import urls as sphwiki_urls

defaultdict = { 'groupName': None,
                'urlPrefix': '', }

sitemaps = {
    'wiki': WikiSnipSitemap,
    'board': ThreadsSitemap,
    }

feeds = {
    'wiki': LatestWikiChanges,
    }

# newforms admin magic

from django.contrib import admin
admin.autodiscover()

urlpatterns = patterns('',
                       #(r'^$', 'django.views.generic.simple.redirect_to', { 'url': '/wiki/show/Start/' }),
                       (r'^$', 'sphene.community.views.groupaware_redirect_to', { 'url': '/index/', 'groupName': None }),
                       (r'^sitemap.xml$', 'django.contrib.sitemaps.views.sitemap', { 'sitemaps': sitemaps }),
                       (r'^feeds/(?P<url>.*)/$', 'django.contrib.syndication.views.feed', {'feed_dict': feeds}),
                       (r'^community/', include('sphene.community.urls'), defaultdict),
                       (r'^board/', include('sphene.sphboard.urls'), defaultdict),
                       (r'^wiki/',  include('sphene.sphwiki.urls'), defaultdict),
                       (r'^blog/',  include('sphene.sphblog.urls'), defaultdict),
                       #(r'^block/', include('sphene.sphblockframework.urls'), defaultdict),
                       (r'^accounts/login/$', 'django.contrib.auth.views.login'),
                       (r'^accounts/logout/$', 'django.contrib.auth.views.logout' ),
                       (r'^downloads/(?P<path>.*)$', 'django.views.static.serve',
        {'document_root': settings.STATIC_DOC_ROOT}),

#                       (r'^index/$', 'community.views.index'),
#                       Added to serve static home page 6/8/09
#                       (r'^index/(?P<path>.*)$', 'django.views.static.serve',
 #       {'document_root': 'index.html'}),

                       #(r'^accounts/register/$', 'sphene.community.views.register', defaultdict),
                       #(r'^accounts/register/(?P<emailHash>[a-zA-Z/\+0-9=]+)/$', 'sphene.community.views.register_hash', defaultdict),

                       (r'^admin/(.*)', admin.site.root),
                       ## for development only ...
                       (r'^static/sphene/(.*)$', 'django.views.static.serve', {'document_root': settings.ROOT_PATH + '/../../communitytools/static/sphene' }),
                       (r'^static/(.*)$', 'django.views.static.serve', {'document_root': settings.ROOT_PATH + '/../static' }),

                       (r'^i18n/', include('django.conf.urls.i18n')),
                       )
