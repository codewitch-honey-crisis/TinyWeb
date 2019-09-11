# TinyWeb
A small home webserver in C#

This is an asynchronous HTTP server written in C#.

HTTP is ugly, but it works. Most of what this project does is hack its way through headers and mangle responses to do chunking. The rest is just serving async, which is pretty easy in .NET, unless you muck it up like I did last time.

What we do is create a listening port and then accept on a pooled thread, where we handle the request. Right now, it's about half asynchronous on the I/O end and the threading takes care of any blocking that still happens, although there's a lot more async support in SocketUtility than is being used right now. I didn't want to complicate the source more than it is.

The code does minimal validation, and I didn't spend a lot of time making it robust. It's more of an example than anything.
