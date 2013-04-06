import Ice;
import Glacier2;

import sys, os, thread;
from threading import Lock;
sys.path.append("../Server/Main/lib");

def load_slice_files(icedir=os.path.normpath("../Ice")):
    """
    Load slice files. This can be done dynamically in Python. Note that this requires
    the ICEROOT environment variable to be (correctly) set.
    """
    import Ice;
    
    # The following line throws an exception if ICEROOT is not an environment variable:
    iceroot = os.environ["ICEROOT"];
    if not iceroot:
        print "WARNING: ICEROOT is not defined! It's required to load slice files dynamically!";
    
    def create_load_string(target_file):
        """
        Create a string which loads Slice definition files.
        @param target_file File to load.
        @return Formatted string.
        """
        return "-I\"%s/slice\" -I\"%s\" --all %s/%s" % (
            iceroot, icedir, icedir, target_file);
    
    ice_files = [ file for file in os.listdir(icedir) if file.endswith(".ice") ];
    for file in ice_files:
        Ice.loadSlice(create_load_string(file));

load_slice_files();

from Exceptions import *;
from copy import copy;
import random;
import Main;
import VTankObject;
import time;
import IGame;
import GameSession;
from random import randint;

def create_account(proxy, username, password, email):
    return proxy.CreateAccount(username, password, email);

def create_tank(session, name):
    models = ("Guardian", "Rhino", "Behemoth");
    model = models[randint(0, 2)];
    weaponId = randint(1, 4);
    red = randint(0, 255);
    green = randint(0, 255);
    blue = randint(0, 255);
    tank = VTankObject.TankAttributes(
        name,               # name
        1.0,                # speed
        1.0,                # armor
        0,                  # points (irrelevant)
        100,                # health (irrelevant)
        model, # model
        VTankObject.Weapon(weaponId, 0.0, "", "", "",
        VTankObject.Projectile(1)),
        VTankObject.VTankColor(red, green, blue));
    if not session.CreateTank(tank):
        raise Exception("CreateTank failed.");

def create_game_router(proxy, username):
    gameRouter = Glacier2.RouterPrx.uncheckedCast(
        Ice.RouterPrx.uncheckedCast(proxy));
    if not gameRouter:
        raise Exception, "Game router is null.";
    
    #print "Connected to router, creating router session...";
    session = gameRouter.createSession(username, "");
    
    return session, gameRouter;
        
class GameCallback(GameSession.ClientEventCallback):
    def PlayerJoined(self, tank, current=None):
        #print "Player joined: id=%i, name=%s" % (tank.id, tank.attributes.name);
        pass;
    
    def PlayerLeft(self, id, current=None):
        #print "Player left: id=%i" % (id);
        pass;
    
    def PlayerMove(self, id, point, direction, current=None):
        #print "Movement: id=%i point=(%f, %f) direction=%s" % (
        #    id, point.x, point.y, str(direction));
        pass
    
    def PlayerRotate(self, id, angle, direction, current=None):
        #print "Rotation: id=%i angle=%f direction=%s" % (
        #            id, angle, str(direction));
        pass    
            
    def TurretSpinning(self, id, angle, direction, current=None):
        pass;
        
    def PlayerDamaged(self, id, projectileId, owner, damageTaken, current=None):
        global my_tank;
        #print "Damage: id=%i projectile=%i owner=%i damage=%i" % (
        #    id, projectileId, owner, damageTaken);
        if my_tank.id == id:
            my_tank.attributes.health -= damageTaken;
            if my_tank.attributes.health <= 0:
                # dead blow
                my_tank.alive = False;
        
    def PlayerRespawned(self, id, where, current=None):
        global my_tank;
        #print "Respawn: id=%i point=(%f, %f)" % (
        #    id, where.x, where.y);
        if id == my_tank.id:
            my_tank.alive = True;
            my_tank.attributes.health = 100;
            my_tank.position.x = where.x;
            my_tank.position.y = where.y;
        
    def CreateProjectile(self, ownerId, projectileId, projectileType, target, current=None):
        #print "Projectile: owner=%i projectile_id=%i projectile_type=%i target=(%f, %f)" % (
        #    ownerId, projectileId, projectileType.projectileId, target.x, target.y);
        pass
        
    def DestroyProjectile(self, projectileId, current=None):
        pass;
        
    def ChatMessage(self, message, color, current=None):
        print "[Chat color=%s]" % color, message;
        
    def ResetPosition(self, point, current=None):
        pass;
        
    def ResetAngle(self, angle, current=None):
        pass;
        
    def RotateMap(self, current=None):
        #print "The server is rotating to the next map.";
        pass;

class ClockSyncI(GameSession.ClockSynchronizer):
    def Request(self, current=None):
        t = long(time.time() * 1000.0);
        #print "Received clock sync request.", t;
        return t;

def pinger(session):
    global running, lock;
    try:
        while True:
            session.KeepAlive();
            time.sleep(10);
            with lock:
                if not running:
                    break;
    except Exception, e:
        print "KeepAlive thread error:", e;
    finally:
        with lock:
            running = False;
        
    print "KeepAlive thread exited.";

def main(host_ip, host_port, username, password):
    global running, lock;
    running = True;
    lock = Lock();
    from hashlib import sha1;

    HOST = ((host_ip, host_port));
    EMAIL = "asibley@summerofsoftware.org";
    try:
        comm = Ice.initialize([
                "--Ice.Trace.Network=0",
                "--Callback.Endpoints="
            ]);
            
        routerPrx = comm.stringToProxy("VTank/router:tcp -h %s -p %i -t 8000" % (HOST[0], HOST[1]));
        router = Glacier2.RouterPrx.checkedCast(routerPrx);
        
        version = VTankObject.Version();
        auth = None;
        try:
            auth = Main.AuthPrx.uncheckedCast(
                comm.stringToProxy("Auth:tcp -h %s -p %i" % (HOST[0], HOST[1])).ice_router(None));
            version = auth.CheckCurrentVersion();
        except Exception, e:
            #print "Didn't grab version from server:", e;
            pass;
        
        temp_session = router.createSession(username, "");
        
        #print "Connected to router. Category:", router.getCategoryForClient();
        main = Main.SessionFactoryPrx.uncheckedCast(temp_session.ice_router(router));
        if not main:
            raise Exception("Couldn't connect: Invalid cast.");
        
        #print "Logging in...";
        try:
            session = Main.MainSessionPrx.uncheckedCast(
                main.VTankLogin(username, password, version).ice_router(router));
        except PermissionDeniedException, ex:
            print ex ;
            #print "Attempting to create new account..";
            if not auth:
                raise Exception("No route to Auth (using Glacier2): Cannot create account.");
            
            if not create_account(auth, username, sha1(password).hexdigest(), EMAIL):
                raise Exception("Couldn't create a new account.");
                
            time.sleep(1);
            
            session = main.VTankLogin(username, sha1(password).hexdigest(), version).ice_router(router);
            
        #print "Login successful.";
        
        list = session.GetTankList();
        
        #print "Own %i tanks." % len(list);
        
        if len(list) == 0:
            print "Creating new tank, '" + username + "tank'...";
            
            create_tank(session, username + "tank");
            list = session.GetTankList();
            
            print "Tank created!";
        
        #print "Selecting tank...";
        if not session.SelectTank(list[0].name):
            raise Exception("SelectTank failed.");
           
        game_servers = session.GetGameServerList();
        
        #print "Retrieved %i game servers." % len(game_servers);
        if not len(game_servers):
            print "No game servers are available. Terminating script.";
            
            return 0;
            
        #print "Joining server %s (%s:%i)." % (game_servers[0].name, game_servers[0].host, game_servers[0].port);
        
        key = session.RequestJoinGameServer(game_servers[0]);
        
        #print "Session key:", key, "-- Using Glacier2:", game_servers[0].usingGlacier2;
        
        if game_servers[0].usingGlacier2:
            proxy = comm.stringToProxy("Theatre/router:tcp -h %s -p %i -t 10000" % (
                game_servers[0].host, game_servers[0].port)).ice_router(None); 
            gameauth, gameRouter = create_game_router(proxy, username);
            comm.setDefaultRouter(gameRouter);
            #print "Connection to game server router successful.";
            
            category = gameRouter.getCategoryForClient();
            
            adapter = comm.createObjectAdapterWithRouter("Callback", gameRouter);
        else:
            gameauth = comm.stringToProxy("GameSessionFactory:tcp -h %s -p %i -t 10000" % (
                game_servers[0].host, game_servers[0].port)).ice_router(None);
            category = "fake";
            
            adapter = comm.createObjectAdapterWithEndpoints("Callback", "tcp -p 31334");
            
        callbackReceiverIdent = Ice.Identity();
        callbackReceiverIdent.name = "ClientEventCallback";
        callbackReceiverIdent.category = category;
        
        clockIdent = Ice.Identity();
        clockIdent.name = "ClockSynchronizer";
        clockIdent.category = category;
        
        callback = GameCallback();
        clock = ClockSyncI();
        adapter.add(callback, callbackReceiverIdent);
        adapter.add(clock, clockIdent);
        
        #print "Attempting login.";
        if game_servers[0].usingGlacier2:
            game = IGame.AuthPrx.uncheckedCast(gameauth).ice_router(gameRouter);
            
            callbackPrx = GameSession.ClientEventCallbackPrx.uncheckedCast(
                adapter.createProxy(callbackReceiverIdent).ice_router(gameRouter));
            clockPrx = GameSession.ClockSynchronizerPrx.uncheckedCast(
                adapter.createProxy(clockIdent).ice_router(gameRouter));
        else:
            game = IGame.AuthPrx.uncheckedCast(gameauth).ice_router(None);
            
            callbackPrx = GameSession.ClientEventCallbackPrx.uncheckedCast(
                adapter.createProxy(callbackReceiverIdent).ice_router(None));
            clockPrx = GameSession.ClockSynchronizerPrx.uncheckedCast(
                adapter.createProxy(clockIdent).ice_router(None));
            
        game_session = game.JoinServer(key, clockPrx, callbackPrx);
        
        map_name = game_session.GetCurrentMapName();
        #print "Joined the game server. Current map:", map_name;
        
        players = game_session.GetPlayerList();
        global my_tank;
        my_tank = None;
        
        #print "Player list:";
        for player in players:
            #print player.attributes.name, "(%i)" % (player.id);
            if player.attributes.name == list[0].name:
                my_tank = player;
                
        if my_tank == None:
            raise Exception("Didn't find my own tank in the player list.");
        
        adapter.activate();
        
        thread.start_new_thread(pinger, (session,));
        
        cooldown = my_tank.attributes.weapon.cooldown;
        game_session = GameSession.GameInfoPrx.uncheckedCast(game_session.ice_oneway());
        while True:
            time.sleep(cooldown);
            
            pos = VTankObject.Point(my_tank.position.x, my_tank.position.y);
            pos.x += random.randint(-500, 500);
            pos.y -= random.randint(-500, 500);
            game_session.Fire(long(time.time() * 1000), pos);
            
            with lock:
                if not running:
                    raise Exception, "Unexpectedly no longer running.";
    except Glacier2.CannotCreateSessionException, e:
        print "Glacier2:", e;
        
        return 1;
    except Ice.Exception, e:
        print "Ice:", e;
        
        return 1;
    except Exception, e:
        print "Exception:", e;
        
        return 1;
    finally:
        with lock:
            running = False;
    
    return 0;

if __name__ == '__main__':
    # Default.
    host_ip = "glacier2a.cis.vtc.edu";
    host_port = 4063;
    username = "test1";
    password = "1";
	
    import sys;
    if len(sys.argv) > 1:
        username = sys.argv[1];
        if username == host_ip:
            print "You no longer need to enter the host IP.";
            print "Usage:", sys.argv[0], "[username] [password]";
            sys.exit(1);
            
        if len(sys.argv) > 2:
            password = sys.argv[2];
    
    sys.exit(main(host_ip, host_port, username, password));
    