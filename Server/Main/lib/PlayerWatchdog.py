import time;
import re;
now = time.time;

debug_mode = False;

def debug_print(message):
    if debug_mode:
        print "[TimedCollection]: %s" % (message);

class TimedObject:
    def __init__(self, obj, lifetime):
        self.obj = obj;
        self.timestamp = now();
        self.lifetime = lifetime;
        self.expire_flag = False;
    
    def __str__(self):
        return str(self.obj);
    
    def expired(self):
        if self.expire_flag:
            return True;
        
        current_time = now();
        if current_time - self.timestamp > self.lifetime:
            self.expire_flag = True;
            return True;
        
        return False;
        
    def get_object(self):
        return self.obj;

class TimedCollection:
    DEFAULT_LIFETIME = 5.0; # seconds
    
    def __init__(self, lifetime=DEFAULT_LIFETIME):
        self.objs = [];
        self.lifetime = lifetime;
    
    def __delitem__(self, item):
        self.remove(item);
        
    def __len__(self):
        return len(self.objs);
    
    def set_lifetime(self, lifetime):
        debug_print("Lifetime=%f" % (lifetime));
        self.lifetime = lifetime;
    
    def add(self, obj, lifetime=None):
        if lifetime == None:
            lifetime = self.lifetime;
        
        debug_print("Added %s (lifetime=%f)" % (str(obj), lifetime));
        self.objs.append(TimedObject(obj, lifetime));
        
    def remove(self, obj):
        debug_print("Removing %s" % (str(obj)));
        index = self.index_of(obj);
        if index < 0:
            return False;
        
        del self.objs[index];
        return True;
        
    def index_of(self, obj):
        for n in xrange(0, len(self.objs)):
            if self.objs[n] == obj:
                return n;
        
        return -1;
    
    def clear(self):
        debug_print("Cleared");
        del self.objs;
        self.objs = [];
        
    def update(self):
        remove_list = [];
        for item in self.objs:
            if item.expired():
                remove_list.append(item);
        
        for item in remove_list:
            assert self.remove(item);

watchdog_player_list = TimedCollection()
watchdog_player_list.set_lifetime(21600)
	
		
def add_player(player_name):
	watchdog_player_list.update()
	exists = False
	
	for obj in watchdog_player_list.objs:
		if str(obj) == player_name:
			exists = True
			break;
	
	if exists == False:	
		watchdog_player_list.add(player_name);
	
def needs_mail(player_name):
	watchdog_player_list.update()
	needs_mail = True
	
	exclusions = ["exolun", 
				  "andysib",
				  "v2chrisb",
				  "williston vtc"]
	
	if re.match("^test{1}[0-9]{1,}$", player_name) != None:			
		needs_mail = False;
		
	if player_name.lower() in exclusions:
		needs_mail = False;
	
	for obj in watchdog_player_list.objs:		
		if str(obj) == player_name:			
			needs_mail = False
			break;
	
	return needs_mail;	

