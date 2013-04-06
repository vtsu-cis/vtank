from django.conf.urls.defaults import *
#from wiki.feeds import *
from django.conf import settings

from django.conf.urls.defaults import *
# feeds for wikiPages and wikiNews
"""
feeds = {
    'latestpages': LatestPages,
}

sitemaps = {
    'wiki': Wiki,
}
"""

# newforms admin magic

from django.contrib import admin
admin.autodiscover()


urlpatterns = patterns('',
    # Example:
    # (r'^goimcommunity/', include('goimcommunity.apps.foo.urls.foo')),

    # Uncomment this for admin:
                       (r'^admin/(.*)', admin.site.root),

                       (r'^board/', include('sphene.sphboard.urls')),
                       
                       (r'^(?P<urlPrefix>test/(?P<groupName>\w+))/board/', include('sphene.sphboard.urls')),
                       (r'^(?P<urlPrefix>test/(?P<groupName>\w+))/wiki/',  include('sphene.sphwiki.urls')),

                       (r'^wiki/',  include('sphene.sphwiki.urls'), { 'urlPrefix': 'wiki', 'groupName': 'Sphene' }),


                       (r'^static/sphene/(.*)$', 'django.views.static.serve', {'document_root': settings.ROOT_PATH + '/../../communitytools/static/sphene' }),
                       (r'^static/(.*)$', 'django.views.static.serve', {'document_root': settings.ROOT_PATH + '/../static' }),



                       (r'^site_media/(.*)$', 'django.views.static.serve', {'document_root': '/home/kahless/dev/python/diamanda/media'}), # change it or remove if not on dev server

                       (r'^accounts/login/$', 'django.contrib.auth.views.login'),
                       (r'^accounts/logout/$','django.contrib.auth.views.logout'),
                       (r'^accounts/register/(?P<emailHash>[a-zA-Z/\+0-9=]+)/$', 'sphene.community.views.register_hash'),
                       (r'^downloads/(?P<path>.*)$', 'django.views.static.serve',
        {'document_root': settings.STATIC_DOC_ROOT}),
#                       (r'^/$', 'community.views.index'),  
#                       (r'^index/$', 'community.views.index'),
      #                (r'^index/(?P<path>.*)$', 'django.views.static.serve',
      #  {'document_root': 'index.html'}),

                       

#                       (r'^forum/', include('myghtyboard.URLconf')), # forum
#                       (r'^muh/', 'wiki.views.show_page'), # wiki main page under /
#                       (r'^wiki/', include('wiki.URLconf')), # wiki
#                       (r'^wiki/feeds/(?P<url>.*)/$', 'django.contrib.syndication.views.feed', {'feed_dict': feeds}), # wiki feeds
#                       (r'^wiki/sitemap.xml$', 'django.contrib.sitemaps.views.sitemap', {'sitemaps': sitemaps}), # wikiPages sitemap


                       
)
