# GC2Toolkit
Used to be a GC2iOS Song Unlocker
Since Taito implemented HTTPS in iOS Client, this tool becomes obsolete(but it still works, see below.)

# How to use it (in 2019) ?
You will need to do some MITM stuff(faking certs, setup own server, re-route server address by hosts, etc.)

### With mitmproxy
1.Set up mitmproxy

2.filter all request to gc2018.gczero.com (it's a https address)

3.game will freeze when started(awaiting for reply, and no timeout)
  and you will get a request(in mitmproxy) accessing start.php or sync.php
  
4.dump the request body in mitmproxy

5.Run in cmd or bash

    (Windows)GC2Toolkit.exe manual *path to your request body*
    
    (non-Windows) mono GC2Toolkit.exe manual *path to your request body*

6.send *path to your request body*.output as response back to your game.

7.If your game boots, you are now full-unlocked.


### With Home-made servers
1.Change port to 443(in code) and bind a certificate at that port

  (in windows, and that step can't be done in code so you should do it yourself)
  
2.Route gc2018.gczero.com to your server.

3.your game will request to your server now

  and everythine should be fine(if HTTPListener can handle HTTPS and gc2.gczero.com is still working)
