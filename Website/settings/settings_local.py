
# Copy this file to settings_local.py and adjust the following 
# settings.

DATABASE_ENGINE = 'mysql'   # 'postgresql', 'mysql', 'sqlite3' or 
                                 # 'ado_mssql'.
DATABASE_NAME = 'forumdb' # Or path to database file if using sqlite3.
DATABASE_USER = 'daemon'         # Not used with sqlite3.
DATABASE_PASSWORD = ''       # Not used with sqlite3.
DATABASE_HOST = ''      # Set to empty string for localhost. Not 
                                 # used with sqlite3.
DATABASE_PORT = ''               # Set to empty string for default. Not 
                                 # used with sqlite3.

# Make this unique, and don't share it with anybody.
SECRET_KEY = 'sd-123mla0sd985432mklasd023mkalsm'

CACHE_BACKEND = 'locmem:///'


SPH_SETTINGS = { }

# The workaround_select_related_bug is currently required in django trunk.
# See http://code.djangoproject.com/ticket/4789 (If this patch was committed,
# or you applied the patch manually you can disable this workaround to improve
# performance)
SPH_SETTINGS['workaround_select_related_bug'] = True


# You can configure this to make every subdomain refer to it's own community 'Group'
SPH_HOST_MIDDLEWARE_URLCONF_MAP = {
    r'^(?P<groupName>\w+).localhost.*$': { 'urlconf': 'urlconfs.community_urls', },
    '.*': { 'urlconf': 'urlconfs.community_urls',
            'params': { 'groupName': 'VTank' } },
}
